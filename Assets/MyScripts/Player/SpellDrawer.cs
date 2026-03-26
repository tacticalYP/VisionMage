// using UnityEngine;
// using System.Collections;

// public class SpellDrawer : MonoBehaviour
// {
//     [Header("Line Settings")]
//     public LineRenderer lineRenderer;
//     public Transform drawOrigin;

//     public IEnumerator DrawSpellShape(Spell spell)
//     {
//         if (spell.shapePoints == null || spell.shapePoints.Length == 0)
//             yield break;

//         lineRenderer.positionCount = 0;
//         lineRenderer.enabled = true;

//         float duration = spell.drawDuration;
//         float timer = 0f;

//         int totalPoints = spell.shapePoints.Length;

//         UnityEngine.Debug.Log("1");

//         while (timer < duration)
//         {
//             timer += Time.deltaTime;

//             float progress = timer / duration;

//             if (spell.drawCurve != null)
//                 UnityEngine.Debug.Log("21");
//                 progress = spell.drawCurve.Evaluate(progress);

//             UnityEngine.Debug.Log("22");

//             int pointsToDraw = Mathf.Clamp(
//                 Mathf.FloorToInt(progress * totalPoints),
//                 0,
//                 totalPoints
//             );

//             lineRenderer.positionCount = pointsToDraw;

//             for (int i = 0; i < pointsToDraw; i++)
//             {   
//                 UnityEngine.Debug.Log($"24{i}");
//                 Vector3 worldPos =
//                     drawOrigin.position +
//                     drawOrigin.TransformDirection(spell.shapePoints[i]);

//                 lineRenderer.SetPosition(i, worldPos);
//             }

//             yield return null;
//         }

//         UnityEngine.Debug.Log("3");
//         // Ensure full shape is drawn
//         lineRenderer.positionCount = totalPoints;

//         for (int i = 0; i < totalPoints; i++)
//         {   
//             UnityEngine.Debug.Log($"4{i}");
//             Vector3 worldPos =
//                 drawOrigin.position +
//                 drawOrigin.TransformDirection(spell.shapePoints[i]);

//             lineRenderer.SetPosition(i, worldPos);
//         }

//         yield return new WaitForSeconds(0.1f);

//         UnityEngine.Debug.Log("5");

//         lineRenderer.enabled = false;
//     }
// }

// using UnityEngine;

// public class SpellDrawer : MonoBehaviour
// {
//     public UDPInputReceiver udp;

//     public Transform targetCursor;
//     public LineRenderer lineRenderer;

//     [Header("Drawing Area Limits")]
//     public float minX = -10.5f;
//     public float maxX = 10.5f;

//     public float minY = 1.0f;
//     public float maxY = 30.0f;

//     public float drawZ = 3.0f;

//     int lineIndex = 0;

//     Vector3 lastPosition;

//     void Update()
//     {
//         float normX = udp.CursorX;
//         float normY = udp.CursorY;

//         Vector3 worldPos = MapNormalizedToWorld(normX, normY);
//         worldPos.x+=732; worldPos.y+=12; worldPos.z+=300;
//         targetCursor.position = worldPos;

//         if (udp.CurrentState == 1)
//         {
//             AddPoint(worldPos);
//         }
//         else if(lineIndex!=0)
//         {
//             lineRenderer.positionCount = 0;
//             lineIndex = 0;
//         }
//         Debug.Log($"{udp.CursorX}, {udp.CursorY}, {normX}, {normY}, {worldPos.x}, {worldPos.y}, {worldPos.z}");
//     }

//     Vector3 MapNormalizedToWorld(float x, float y)
//     {
//         return new Vector3(-x, -y, drawZ);
//     }

//     void AddPoint(Vector3 position)
//     {
//         if (lineIndex == 0)
//         {
//             lineRenderer.positionCount = 1;
//             lineRenderer.SetPosition(0, position);
//             lastPosition = position;
//             lineIndex = 1;
//             return;
//         }

//         if (Vector3.Distance(lastPosition, position) > 0.01f)
//         {
//             lineRenderer.positionCount++;
//             lineRenderer.SetPosition(lineIndex, position);

//             lastPosition = position;
//             lineIndex++;
//         }
//     }
// }

using UnityEngine;
using System.Collections.Generic;

public class SpellDrawer : MonoBehaviour
{
    public UDPInputReceiver udp;
    public Transform targetCursor;
    public LineRenderer lineRenderer;

    [Header("Drawing Settings")]
    public float drawDistanceInFront = 2.0f; // Distance from character
    public float inputScale = 5.0f;          // How large the drawing is
    public float minPointDistance = 0.01f;
    private int lineIndex = 0;
    private Vector3 lastLocalPosition;
    List<Vector2> shapePoints = new List<Vector2>();
    // PDollarRecognizer recognizer = new PDollarRecognizer();
    // int lastState = 0;


    List<PDollarRecognizer.Gesture> trainingSet = new List<PDollarRecognizer.Gesture>();

    /// temporarily using these shapes
    string[] spells = { "Triangle", "Square", "Circle", "Zigzag" };

    public GameObject[] spellPrefabs;

    SpellCaster spellCaster;

    void Start()
    {
        lineRenderer.positionCount = 0;
        spellCaster = GetComponent<SpellCaster>();

        // recognizer.AddTemplate("Line", new List<Vector2>{
        //     new Vector2(0,0),
        //     new Vector2(1,0)
        // });

        // recognizer.AddTemplate("V", new List<Vector2>{
        //     new Vector2(0,1),
        //     new Vector2(0.5f,0),
        //     new Vector2(1,1)
        // });

        // recognizer.AddTemplate("Triangle", new List<Vector2>{
        //     new Vector2(0,0),
        //     new Vector2(0.5f,1),
        //     new Vector2(1,0),
        //     new Vector2(0,0)
        // });


    //////////////////////////////////
    
        // Triangle: 3 sharp corners, returning to start
    // List<Vector2> triangle = new List<Vector2> {
    //     new Vector2(0f, 1f),    // Top
    //     new Vector2(0.86f, -0.5f), // Bottom Right
    //     new Vector2(-0.86f, -0.5f), // Bottom Left
    //     new Vector2(0f, 1f)     // Back to Top
    // };
    // trainingSet.Add(new PDollarRecognizer.Gesture("Triangle", triangle));

    // // Square: 4 points, returning to start
    // List<Vector2> square = new List<Vector2> {
    //     new Vector2(-1f, 1f),  // Top Left
    //     new Vector2(1f, 1f),   // Top Right
    //     new Vector2(1f, -1f),  // Bottom Right
    //     new Vector2(-1f, -1f), // Bottom Left
    //     new Vector2(-1f, 1f)   // Back to Top Left
    // };
    // trainingSet.Add(new PDollarRecognizer.Gesture("Square", square));

    // // Circle: 16 points to create a smooth curvature
    // List<Vector2> circle = new List<Vector2>();
    // for (int i = 0; i <= 16; i++) {
    //     float angle = i * Mathf.PI * 2 / 16;
    //     circle.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
    // }
    // trainingSet.Add(new PDollarRecognizer.Gesture("Circle", circle));

    // // Star: 5-point star (outer and inner points)
    // List<Vector2> star = new List<Vector2>();
    // float outerRad = 1.0f;
    // float innerRad = 0.4f;
    // for (int i = 0; i < 11; i++) {
    //     float angle = (i * 36 - 90) * Mathf.Deg2Rad;
    //     float r = (i % 2 == 0) ? outerRad : innerRad;
    //     star.Add(new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r));
    // }
    // trainingSet.Add(new PDollarRecognizer.Gesture("Star", star));
    }

    void Update()
    {
        // 1. Get Normalized Input (-1 to 1 range assumed from UDP)
        float normX = udp.CursorX;
        float normY = udp.CursorY;

        // Create a Local Position
        Vector3 localPos = new Vector3((-normX * inputScale)+2, (-normY * inputScale)+2, drawDistanceInFront);
        
        // Debug.Log($"{udp.CursorX}, {udp.CursorY}, {normX}, {normY}, {localPos.x}, {localPos.y}, {localPos.z}, {targetCursor.position.x}, {targetCursor.position.y}, {targetCursor.position.z}");
        if (udp.CurrentState == 1)
        {   
            Vector3 locaPos = new Vector3((-normX * inputScale)+4, (-normY * inputScale)+4, 0);
            // Move the cursor visual (TransformPoint converts local to world for the visual)
            targetCursor.position = transform.TransformPoint(locaPos);
            AddPoint(localPos);
        }
        else if (lineIndex != 0)
        {   
            RecognizeShape();
            lineRenderer.positionCount = 0;
            lineIndex = 0;
        }
    }

    void AddPoint(Vector3 localPos)
    {
        // Check distance in local space to avoid redundant points
        if (lineIndex > 0 && Vector3.Distance(lastLocalPosition, localPos) < minPointDistance) 
            return;

        if (lineIndex == 0)
        {
            lineRenderer.positionCount = lineIndex + 1;
            shapePoints.Clear();
            shapePoints.Add(new Vector2(localPos.x, localPos.y));
        }
        else
        {
            lineRenderer.positionCount++;
            shapePoints.Add(new Vector2(localPos.x, localPos.y));
        }

        lineRenderer.SetPosition(lineIndex, localPos);

        lastLocalPosition = localPos;
        lineIndex++;
    }

    void RecognizeShape()
    {   
        //////////////////////
        
        // if (shapePoints.Count < 10)
        //     return;

        // var result = recognizer.Recognize(shapePoints);

        // Debug.Log("Recognized: " + result.GestureClass + " Score: " + result.Score);
        // foreach (Vector2 v in shapePoints)
        // {
        //     Debug.Log(v); 
        // }
        

        ////////////////////////////////////
        
        // float score;
        // string result = PDollarRecognizer.Classify(shapePoints, trainingSet, out score);
        
        // if (score > 0.7f) { // 70% confidence threshold
        //     Debug.Log($"Detected: {result} with score {score}");
        // }

        /////////////////////////

        // string result = udp.Shape;
        
        int spellId = UnityEngine.Random.Range(0, spells.Length);
        string result = spells[spellId];

        Debug.Log("Detected: " + result);

        spellCaster.OnSpell2(spellPrefabs[spellId]);
    }
}