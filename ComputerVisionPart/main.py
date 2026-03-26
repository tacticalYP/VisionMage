# left hand calcx, calcy, head yaw, pitch, right hand drawing + recognition, UDP signal

import cv2
import mediapipe as mp
import time
from collections import deque
import numpy as np
import socket
from shape_recognition import shape_predict

# MediaPipe 0.10.32 Tasks API Setup
BaseOptions = mp.tasks.BaseOptions
HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions
FaceLandmarker = mp.tasks.vision.FaceLandmarker
FaceLandmarkerOptions = mp.tasks.vision.FaceLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

UDP_IP = "127.0.0.1"  # Localhost
UDP_PORT = 5005       # Match this in Unity
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Filter Configuration
MEDIAN_WINDOW = 7  # Increased for better spike rejection
DEADZONE_THRESHOLD = 0.002  # Ignore movements smaller than this to stop oscillations

### For right hand
STATE_DRAW_IDLE = "FinishedAndIdle"
STATE_DRAWING = "Drawing"
drawing_mode = STATE_DRAW_IDLE
current_stroke = [] 
last_draw_point = None
canvas = None
predicted_shape = None

def is_finger_up(landmarks, tip_id, pip_id):
    tip_y = landmarks[tip_id].y
    pip_y = landmarks[pip_id].y
    return tip_y < pip_y
MAX_DRAW_HISTORY = 5

draw_x_history = deque(maxlen=MAX_DRAW_HISTORY)
draw_y_history = deque(maxlen=MAX_DRAW_HISTORY)

def get_smoothed_drawing_coords(x, y):
    draw_x_history.append(x)
    draw_y_history.append(y)
    return np.mean(draw_x_history), np.mean(draw_y_history)

# Global storage for previous values to handle the Deadzone
last_stable_vals = {k: 0.5 for k in [
'wrist_x','wrist_y',
'index_x','index_y',
'pinky_x','pinky_y',
'yaw','pitch'
]}
raw_buffers = {k: deque(maxlen=MEDIAN_WINDOW) for k in last_stable_vals.keys()}

def get_filtered_val(key, new_val):
    # 1. Add to raw buffer for median filtering (removes spikes)
    raw_buffers[key].append(new_val)
    if len(raw_buffers[key]) < MEDIAN_WINDOW:
        return new_val
    
    current_median = np.median(list(raw_buffers[key]))
    
    # 2. Deadzone/Hysteresis Logic (removes micro-oscillations)
    # Only update if the change is significant
    if abs(current_median - last_stable_vals[key]) > DEADZONE_THRESHOLD:
        last_stable_vals[key] = current_median
        
    return last_stable_vals[key]

def get_direction_string(val_x, val_y):
    # Apply the 0.5 deadzone
    x = 0 if -0.5 < val_x < 0.5 else val_x
    y = 0 if -0.5 < val_y < 0.5 else val_y
    
    if x == 0 and y == 0: return "Neutral",0,0
    
    dir_str = ""
    # Y Logic: Negative = Up, Positive = Down
    if y < 0: dir_str += "Up"
    elif y > 0: dir_str += "Down"
    
    # X Logic: Negative = Right, Positive = Left
    if x > 0: dir_str += ("-" if dir_str else "") + "Left"
    elif x < 0: dir_str += ("-" if dir_str else "") + "Right"
    
    return dir_str, x, y

last_hand_dir = ""
last_face_dir = ""

# Data storage
data = {k: np.array([]) for k in ['time', 'wrist_x', 'wrist_y', 'index_x', 'index_y', 'pinky_x', 'pinky_y', 'yaw','pitch']}
calibration_temp = {k: [] for k in raw_buffers.keys()}
offsets = {k: 0.0 for k in raw_buffers.keys()}

# Logic States
STATE_IDLE, STATE_CALIBRATING, STATE_READY, STATE_RECORDING = 0, 1, 2, 3
current_state = STATE_IDLE

CALIBRATION_DURATION = 5.0

options = HandLandmarkerOptions(
    base_options=BaseOptions(model_asset_path='hand_landmarker.task'),
    running_mode=VisionRunningMode.VIDEO,
    num_hands=2,
    min_hand_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

face_options = FaceLandmarkerOptions(
    base_options=BaseOptions(model_asset_path='face_landmarker.task'),
    running_mode=VisionRunningMode.VIDEO,
    num_faces=1
)

with HandLandmarker.create_from_options(options) as landmarker, \
     FaceLandmarker.create_from_options(face_options) as face_landmarker:
    cap = cv2.VideoCapture(0)
    # Set high resolution
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1920)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1080)
    
    start_program_time = time.time()

    while cap.isOpened():
        success, frame = cap.read()
        if not success: break

        current_hand_dir = "None"
        current_face_dir = "None"
        
        h, w, _ = frame.shape
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
        curr_time = time.time() - start_program_time
        timestamp_ms = int(curr_time * 1000)
        
        result = landmarker.detect_for_video(mp_image, timestamp_ms)
        face_result = face_landmarker.detect_for_video(mp_image, timestamp_ms)
        
        # --- UI Overlay Logic ---
        msg, color = "", (0, 255, 255)

        frame_yaw, frame_pitch, calc_x, calc_y,draw_px,draw_py = 0.0, 0.0, 0.0, 0.0,0.0,0.0
        
        if current_state == STATE_IDLE:
            msg = "Press 'C' to start 5s Calibration"
        elif current_state == STATE_CALIBRATING:
            elapsed = time.time() - calibration_start_time
            remaining = max(0, CALIBRATION_DURATION - elapsed)
            msg = f"CALIBRATING... HOLD STILL ({remaining:.1f}s)"
            if remaining <= 0:
                for k in offsets.keys():
                    if calibration_temp[k]: offsets[k] = np.mean(calibration_temp[k])
                current_state = STATE_READY
        elif current_state == STATE_READY:
            msg = "Calibration Complete! Press 'C' to Start Recording"
            color = (0, 255, 0)
        elif current_state == STATE_RECORDING:
            msg = "RECORDING DATA... Press 'Q' to Finish"
            color = (0, 0, 255)
        
        # Track previous mode to detect the transition
        previous_mode = drawing_mode
        # --- Landmark Processing ---
        if result.hand_landmarks:
            for i, handedness in enumerate(result.handedness):

                lm = result.hand_landmarks[i]

                if handedness[0].category_name == "Left":
                    
                    pts = {'wrist': (lm[0].x, lm[0].y), 'index': (lm[8].x, lm[8].y), 'pinky': (lm[20].x, lm[20].y)}

                    current_f = {}
                    for name, (raw_x, raw_y) in pts.items():
                        current_f[f'{name}_x'] = get_filtered_val(f'{name}_x', raw_x)
                        current_f[f'{name}_y'] = get_filtered_val(f'{name}_y', raw_y)

                    if current_state == STATE_CALIBRATING:
                        for k, v in current_f.items(): calibration_temp[k].append(v)
                    
                    elif current_state == STATE_RECORDING:
                        norm_vals = {}
                        for k in ['wrist_x', 'wrist_y', 'index_x', 'index_y', 'pinky_x', 'pinky_y']:
                            norm_vals[k] = (current_f[k] - offsets[k]) * 5.0

                        calc_x = norm_vals['index_x'] - norm_vals['wrist_x']
                        if calc_x < 0: calc_x = (norm_vals['pinky_x'] - norm_vals['wrist_x'])
                        
                        calc_y = norm_vals['index_y'] - norm_vals['wrist_y']

                        current_hand_dir,calc_x,calc_y = get_direction_string(calc_x, calc_y)

                        # print(f"({calc_x:.4f}, {calc_y:.4f})")

                    # Draw points
                    for name in ['wrist', 'index', 'pinky']:
                        cv2.circle(frame, (int(current_f[f'{name}_x']*w), int(current_f[f'{name}_y']*h)), 8, (0, 255, 0), -1)
                
                elif handedness[0].category_name == "Right":
                    index_up = is_finger_up(lm, 8, 6)
                    middle_up = is_finger_up(lm, 12, 10)

                    if index_up and not middle_up:
                        drawing_mode = STATE_DRAWING
                    elif index_up and middle_up:
                        drawing_mode = STATE_DRAW_IDLE
                    
                    # Optional: get index tip coordinates for drawing
                    draw_px = int(lm[8].x * w)
                    draw_py = int(lm[8].y * h)

                    curr_draw_point = (draw_px, draw_py)

                    if drawing_mode == STATE_DRAWING:
                        if canvas is None:
                            canvas = np.zeros_like(frame)

                        current_stroke.append(curr_draw_point)

                        if last_draw_point is not None:
                            # Draw a line on the canvas from the last position to the current
                            cv2.line(canvas, last_draw_point, curr_draw_point, (0, 255, 0), 5)
                        
                        last_draw_point = curr_draw_point
                    else:
                        canvas = None
                        last_draw_point = None

                    pointer_color = (0, 255, 0) if drawing_mode == STATE_DRAWING else (0, 0, 255)
                    cv2.circle(frame, (draw_px, draw_py), 10, pointer_color, -1)
        
        # Predict shape
        if previous_mode == STATE_DRAWING and drawing_mode == STATE_DRAW_IDLE:
            if len(current_stroke)>5:
                predicted_shape = shape_predict(current_stroke)
                print(f"A {predicted_shape}")
                current_stroke = [] # Reset for next shape

        # --- Face Landmark Processing ---
        if face_result.face_landmarks:

            lm = face_result.face_landmarks[0]
            nose = lm[1]
            left_eye = lm[33]
            right_eye = lm[263]
            chin = lm[152]
            forehead = lm[10]
            # Simple Yaw/Pitch calc
            yaw_raw = nose.x - ((left_eye.x + right_eye.x) / 2)
            pitch_raw = nose.y - ((chin.y + forehead.y) / 2)
            
            yaw = get_filtered_val('yaw', yaw_raw*10)*5.0
            pitch = get_filtered_val('pitch', pitch_raw*10)*5.0

            if current_state == STATE_CALIBRATING:
                calibration_temp['yaw'].append(yaw)
                calibration_temp['pitch'].append(pitch)

            elif current_state == STATE_RECORDING:

                frame_yaw = yaw - offsets['yaw']
                frame_pitch = pitch - offsets['pitch']
                current_face_dir,frame_yaw,frame_pitch = get_direction_string(frame_yaw, frame_pitch)

            # Draw keypoints
            for pt in [nose, left_eye, right_eye, chin, forehead]:
                cv2.circle(frame,(int(pt.x*w),int(pt.y*h)),6,(255,0,0),-1)
        
        # print(f"{calc_x:.4f}, {calc_y:.4f}, {frame_yaw:.4f}, {frame_pitch:.4f}")

        if current_state == STATE_RECORDING:
            # Send UDP string to unity
            draw_flag = 1 if drawing_mode == STATE_DRAWING else 0
            data_string = f"{-calc_x:.4f},{-calc_y:.4f},{-frame_yaw:.4f},{frame_pitch:.4f},{draw_flag},{draw_px:.4f},{draw_py:.4f},{predicted_shape}"
            sock.sendto(data_string.encode(), (UDP_IP, UDP_PORT))
            # print(data_string)

            # Print direction of hand and face
            if current_hand_dir != last_hand_dir or current_face_dir != last_face_dir:
                # print(f"Hand: {current_hand_dir:10} | Face: {current_face_dir:10}")
                last_hand_dir, last_face_dir = current_hand_dir, current_face_dir
        
        if canvas is not None:
            # Flip canvas logic: since we flip the frame later, 
            # we should handle drawing/mirroring carefully.
            # Convert canvas to grayscale to create a mask
            img_gray = cv2.cvtColor(canvas, cv2.COLOR_BGR2GRAY)
            _, img_inv = cv2.threshold(img_gray, 50, 255, cv2.THRESH_BINARY_INV)
            img_inv = cv2.cvtColor(img_inv, cv2.COLOR_GRAY2BGR)
            
            # Black out the area of the drawing on the frame, then add the canvas
            frame = cv2.bitwise_and(frame, img_inv)
            frame = cv2.bitwise_or(frame, canvas)
        frame = cv2.flip(frame, 1)
        cv2.putText(frame, msg, (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, color, 2)
        cv2.imshow('Hand Tracking System', frame)
        
        key = cv2.waitKey(1) & 0xFF
        if key == ord('c'):
            if current_state == STATE_IDLE:
                calibration_start_time = time.time()
                current_state = STATE_CALIBRATING
            elif current_state == STATE_READY:
                current_state = STATE_RECORDING
        elif key == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()
    sock.close()