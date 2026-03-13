// using System.Collections.Generic;
// using UnityEngine;

// public class PDollarRecognizer
// {
//     public class Result
//     {
//         public string GestureClass;
//         public float Score;

//         public Result(string name, float score)
//         {
//             GestureClass = name;
//             Score = score;
//         }
//     }

//     public class Gesture
//     {
//         public string Name;
//         public List<Vector2> Points;

//         public Gesture(string name, List<Vector2> points)
//         {
//             Name = name;
//             Points = points;
//         }
//     }

//     List<Gesture> templates = new List<Gesture>();

//     public void AddTemplate(string name, List<Vector2> points)
//     {
//         templates.Add(new Gesture(name, points));
//     }

//     public Result Recognize(List<Vector2> points)
//     {
//         float bestDistance = float.MaxValue;
//         string bestGesture = "Unknown";

//         foreach (var template in templates)
//         {
//             float d = GreedyCloudMatch(points, template.Points);

//             if (d < bestDistance)
//             {
//                 bestDistance = d;
//                 bestGesture = template.Name;
//             }
//         }

//         float score = Mathf.Max((bestDistance - 2.0f) / -2.0f, 0.0f);

//         return new Result(bestGesture, score);
//     }

//     float GreedyCloudMatch(List<Vector2> points, List<Vector2> template)
//     {
//         float sum = 0f;

//         for (int i = 0; i < points.Count && i < template.Count; i++)
//         {
//             sum += Vector2.Distance(points[i], template[i]);
//         }

//         return sum / points.Count;
//     }
// }

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class PDollarRecognizer
{
    private const int NumPoints = 32; // Standard for $P

    public struct Point {
        public float X, Y;
        public int ID; // Stroke ID
        public Point(float x, float y, int id) { X = x; Y = y; ID = id; }
    }

    public class Gesture {
        public string Name;
        public Point[] Points;

        public Gesture(string name, List<Vector2> points) {
            this.Name = name;
            this.Points = Normalize(points.ToArray());
        }
    }

    // --- Core Recognition Method ---
    public static string Classify(List<Vector2> inputPoints, List<Gesture> templates, out float score) {
        Point[] points = Normalize(inputPoints.ToArray());
        float b = float.PositiveInfinity;
        int u = -1;

        for (int i = 0; i < templates.Count; i++) {
            float d = GreedyCloudMatch(points, templates[i].Points);
            if (d < b) {
                b = d;
                u = i;
            }
        }

        score = Math.Max((b - 2.0f) / -2.0f, 0); // Normalized score 0 to 1
        return (u == -1) ? "Unknown" : templates[u].Name;
    }

    // --- Normalization Pipeline ---
    private static Point[] Normalize(Vector2[] rawPoints) {
        Point[] points = new Point[rawPoints.Length];
        for(int i=0; i<rawPoints.Length; i++) points[i] = new Point(rawPoints[i].x, rawPoints[i].y, 0);

        points = Resample(points, NumPoints);
        points = Scale(points);
        points = TranslateTo(points, Vector2.zero);
        return points;
    }

    private static float GreedyCloudMatch(Point[] points1, Point[] points2) {
        float e = 0.50f;
        int step = (int)Math.Floor(Math.Pow(points1.Length, 1.0 - e));
        float min = float.PositiveInfinity;

        for (int i = 0; i < points1.Length; i += step) {
            float d1 = CloudDistance(points1, points2, i);
            float d2 = CloudDistance(points2, points1, i);
            min = Math.Min(min, Math.Min(d1, d2));
        }
        return min;
    }

    private static float CloudDistance(Point[] pts1, Point[] pts2, int start) {
    int n = pts1.Length;
    // Safety: ensure both arrays are the same size
    int m = Math.Min(pts1.Length, pts2.Length);
    
    bool[] matched = new bool[m];
    float sum = 0;
    int i = start;
    
    do {
        int index = -1;
        float min = float.PositiveInfinity;
        for (int j = 0; j < m; j++) {
            if (!matched[j]) {
                float d = SqrDist(pts1[i], pts2[j]);
                if (d < min) {
                    min = d;
                    index = j;
                }
            }
        }
        
        if (index != -1) { // Safety check
            matched[index] = true;
            sum += (float)Math.Sqrt(min);
        }
        
        i = (i + 1) % m;
    } while (i != start);
    
    return sum;
}

    // Helper Math
    private static float SqrDist(Point p1, Point p2) => (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);

   private static Point[] Resample(Point[] points, int n) {
    float I = PathLength(points) / (n - 1);
    float D = 0;
    List<Point> newPoints = new List<Point> { points[0] };

    for (int i = 1; i < points.Length; i++) {
        float d = (float)Math.Sqrt(SqrDist(points[i - 1], points[i]));
        if (D + d >= I) {
            float qx = points[i - 1].X + ((I - D) / d) * (points[i].X - points[i - 1].X);
            float qy = points[i - 1].Y + ((I - D) / d) * (points[i].Y - points[i - 1].Y);
            Point q = new Point(qx, qy, points[i].ID);
            newPoints.Add(q);
            // Insert q into the original list to continue resampling from this new point
            points[i - 1] = q;
            D = 0;
        }
        else D += d;
    }

    // Rounding error safety: if we are missing the last point, add the end of the original path
    if (newPoints.Count == n - 1) {
        newPoints.Add(points[points.Length - 1]);
    }

    // Force return exactly N points
    // return newPoints.Take(n).ToArray();
    Point[] finalPoints = new Point[n];
for (int i = 0; i < n; i++)
{
    // If newPoints is somehow shorter than n, repeat the last point
    finalPoints[i] = (i < newPoints.Count) ? newPoints[i] : newPoints[newPoints.Count - 1];
}
return finalPoints;
}

    private static Point[] Scale(Point[] points) {
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        foreach (var p in points) {
            minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
            minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
        }
        float size = Math.Max(maxX - minX, maxY - minY);
        Point[] newPoints = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
            newPoints[i] = new Point((points[i].X - minX) / size, (points[i].Y - minY) / size, points[i].ID);
        return newPoints;
    }

    private static Point[] TranslateTo(Point[] points, Vector2 target) {
        Vector2 centroid = Vector2.zero;
        foreach (var p in points) { centroid.x += p.X; centroid.y += p.Y; }
        centroid /= points.Length;
        Point[] newPoints = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
            newPoints[i] = new Point(points[i].X + target.x - centroid.x, points[i].Y + target.y - centroid.y, points[i].ID);
        return newPoints;
    }

    private static float PathLength(Point[] points) {
        float d = 0;
        for (int i = 1; i < points.Length; i++) d += (float)Math.Sqrt(SqrDist(points[i - 1], points[i]));
        return d;
    }
}