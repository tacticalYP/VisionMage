import math
import time
import matplotlib.pyplot as plt

# ---------------------------
# Point class
# ---------------------------
class Point:
    def __init__(self, x, y, id, angle=0.0):
        self.X = x
        self.Y = y
        self.ID = id
        self.Angle = angle  # normalized turning angle ($P+)

# ---------------------------
# PointCloud class
# ---------------------------
class PointCloud:
    def __init__(self, name, points):
        self.Name = name
        self.tp = points
        self.Points = resample(points, NumPoints)
        self.Points = scale_and_translate(self.Points)
        self.Points = compute_normalized_turning_angles(self.Points)  # $P+

# ---------------------------
# Result class
# ---------------------------
class Result:
    def __init__(self, name, score, ms):
        self.Name = name
        self.Score = score
        self.Time = ms

# ---------------------------
# PDollarPlusRecognizer constants
# ---------------------------
NumPointClouds = 4
NumPoints = 32
Origin = Point(0, 0, 0)

# ---------------------------
# PDollarPlusRecognizer class
# ---------------------------
class PDollarPlusRecognizer:
    def __init__(self):
        self.PointClouds = [None] * NumPointClouds

    def recognize(self, points):
        t0 = time.time() * 1000
        candidate = PointCloud("", points)

        u = -1
        b = float('inf')
        for i, pc in enumerate(self.PointClouds):
            d = min(
                cloud_distance(candidate.Points, pc.Points),
                cloud_distance(pc.Points, candidate.Points)
            )
            if d < b:
                b = d
                u = i

        t1 = time.time() * 1000
        if u == -1:
            return Result("No match.", 0.0, t1 - t0)
        else:
            score = 1.0 / b if b > 1.0 else 1.0
            return Result(self.PointClouds[u].Name, score, t1 - t0)

    def add_gesture(self, name, points):
        self.PointClouds.append(PointCloud(name, points))
        return sum(1 for pc in self.PointClouds if pc.Name == name)

    def delete_user_gestures(self):
        self.PointClouds = self.PointClouds[:NumPointClouds]
        return NumPointClouds

# ---------------------------
# Helper functions
# ---------------------------
def cloud_distance(pts1, pts2):
    matched = [False] * len(pts1)
    sum_dist = 0.0

    for i in range(len(pts1)):
        index = -1
        min_d = float('inf')
        for j in range(len(pts1)):
            d = distance_with_angle(pts1[i], pts2[j])
            if d < min_d:
                min_d = d
                index = j
        matched[index] = True
        sum_dist += min_d

    for j, m in enumerate(matched):
        if not m:
            min_d = float('inf')
            for i in range(len(pts1)):
                d = distance_with_angle(pts1[i], pts2[j])
                if d < min_d:
                    min_d = d
            sum_dist += min_d

    return sum_dist

def resample(points, n):
    I = path_length(points) / (n - 1)
    D = 0.0
    newpoints = [points[0]]

    i = 1
    while i < len(points):
        if points[i].ID == points[i-1].ID:
            d = distance(points[i-1], points[i])
            if (D + d) >= I:
                qx = points[i-1].X + ((I - D) / d) * (points[i].X - points[i-1].X)
                qy = points[i-1].Y + ((I - D) / d) * (points[i].Y - points[i-1].Y)
                q = Point(qx, qy, points[i].ID)
                newpoints.append(q)
                points.insert(i, q)
                D = 0.0
            else:
                D += d
        i += 1

    if len(newpoints) == n - 1:
        last = points[-1]
        newpoints.append(Point(last.X, last.Y, last.ID))

    return newpoints

def scale_and_translate(points):
    minX = min(p.X for p in points)
    minY = min(p.Y for p in points)
    maxX = max(p.X for p in points)
    maxY = max(p.Y for p in points)

    sizex = maxX - minX
    sizey = maxY - minY
    return [Point((p.X - minX)/sizex - 0.5, (p.Y - minY)/sizey - 0.5, p.ID) for p in points]


def compute_normalized_turning_angles(points):
    newpoints = [Point(points[0].X, points[0].Y, points[0].ID, 0.0)]

    for i in range(1, len(points) - 1):
        v1x = points[i].X - points[i-1].X
        v1y = points[i].Y - points[i-1].Y

        v2x = points[i+1].X - points[i].X
        v2y = points[i+1].Y - points[i].Y

        dot = v1x * v2x + v1y * v2y
        mag1 = math.sqrt(v1x*v1x + v1y*v1y)
        mag2 = math.sqrt(v2x*v2x + v2y*v2y)

        if mag1 * mag2 == 0:
            angle = 0.0
        else:
            cos_angle = max(-1.0, min(1.0, dot / (mag1 * mag2)))
            angle = math.acos(cos_angle) / math.pi

        newpoints.append(Point(points[i].X, points[i].Y, points[i].ID, angle))

    last = points[-1]
    newpoints.append(Point(last.X, last.Y, last.ID, 0.0))

    return newpoints

def centroid(points):
    x = sum(p.X for p in points) / len(points)
    y = sum(p.Y for p in points) / len(points)
    return Point(x, y, 0)

def path_length(points):
    d = 0.0
    for i in range(1, len(points)):
        if points[i].ID == points[i-1].ID:
            d += distance(points[i-1], points[i])
    return d

def distance_with_angle(p1, p2):
    dx = p2.X - p1.X
    dy = p2.Y - p1.Y
    da = p2.Angle - p1.Angle
    return math.sqrt(dx*dx + dy*dy + da*da*4)

def distance(p1, p2):
    dx = p2.X - p1.X
    dy = p2.Y - p1.Y
    return math.sqrt(dx*dx + dy*dy)

def convert_to_pointcloud(a,shapename):
    newlist = []
    for i,j in a:
        newlist.append(Point(i,j,1))
    newlist = PointCloud(shapename,newlist)

    return newlist

pp = PDollarPlusRecognizer()

triangle_points = [(0.0, 1.0), (0.09045084971874737, 0.8190983005625052), (0.18090169943749473, 0.6381966011250105), (0.27135254915624213, 0.45729490168751574), (0.36180339887498947, 0.27639320225002106), (0.45225424859373686, 0.09549150281252627), (0.5427050983124843, -0.08541019662496852), (0.6331559480312315, -0.2663118960624631), (0.7236067977499789, -0.44721359549995787), (0.8140576474687263, -0.6281152949374527), (0.9045084971874737, -0.8090169943749475), (1.0, -1.0), (0.8090169943749475, -1.0), (0.6067627457812108, -1.0), (0.40450849718747417, -1.0), (0.20225424859373753, -1.0), (8.881784197001252e-16, -1.0), (-0.20225424859373575, -1.0), (-0.4045084971874724, -1.0), (-0.606762745781209, -1.0), (-0.8090169943749457, -1.0), (-1.0, -1.0), (-0.9045084971874747, -0.8090169943749494), (-0.8140576474687274, -0.6281152949374549), (-0.7236067977499802, -0.4472135954999603), (-0.6331559480312329, -0.26631189606246575), (-0.5427050983124856, -0.08541019662497118), (-0.4522542485937383, 0.09549150281252339), (-0.361803398874991, 0.27639320225001796), (-0.27135254915624385, 0.4572949016875123), (-0.18090169943749657, 0.6381966011250069), (-0.09045084971874928, 0.8190983005625014)]

square_points = [
(0.0, 1.0),

(0.25, 1.0), (0.5, 1.0), (0.75, 1.0), (1.0, 1.0),
(1.0, 0.75), (1.0, 0.5), (1.0, 0.25), (1.0, 0.0),
(1.0, -0.25), (1.0, -0.5), (1.0, -0.75), (1.0, -1.0),

(0.75, -1.0), (0.5, -1.0), (0.25, -1.0), (0.0, -1.0),
(-0.25, -1.0), (-0.5, -1.0), (-0.75, -1.0), (-1.0, -1.0),

(-1.0, -0.75), (-1.0, -0.5), (-1.0, -0.25), (-1.0, 0.0),
(-1.0, 0.25), (-1.0, 0.5), (-1.0, 0.75), (-1.0, 1.0),

(-0.75, 1.0), (-0.5, 1.0), (-0.25, 1.0)
]

circle_points = [
(math.sin(2*math.pi*i/32), math.cos(2*math.pi*i/32))
for i in range(32)
]

zigzag_points = [
    (1.0,1.0), (-1.0, 0.0), (1.0,0.0), (-1.0,-1.0)
]

pp.PointClouds[0] = convert_to_pointcloud(triangle_points,"Triangle")
pp.PointClouds[1] = convert_to_pointcloud(square_points,"Square")
pp.PointClouds[2] = convert_to_pointcloud(circle_points,"Circle")
pp.PointClouds[3] = convert_to_pointcloud(zigzag_points,"Zigzag")

def shape_predict(data):
    data = [(-a,-b) for a,b in data]
    data = convert_to_pointcloud(data,"shape")
    data = data.Points

    final = []
    for i in data:
        # print(f"{i.X,i.Y}", end=", ")
        final.append((i.X, i.Y))

    ## Just for plotting the points to see the shape

    # def plot_shape(points, title):

    #     x = [p[0] for p in points]
    #     y = [p[1] for p in points]

    #     plt.figure()

    #     # Draw path in sequence
    #     plt.plot(x, y, marker='o')

    #     # Label each point with its index
    #     for i, (px, py) in enumerate(points):
    #         plt.text(px, py, str(i), fontsize=12)

    #     plt.title(title)
    #     plt.axhline(0)
    #     plt.axvline(0)
    #     plt.gca().set_aspect('equal', adjustable='box')
    #     plt.grid(True)

    #     plt.show()

    # plot_shape(final,"shape")

    result = pp.recognize(data)
    return result.Name
