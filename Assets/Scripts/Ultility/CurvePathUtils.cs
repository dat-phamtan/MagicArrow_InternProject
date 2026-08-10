using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Assets.Scripts.Ultility
{
    public static class CurvePathUtils
    {
        public static void BuildCurved(Vector3[] gridPath, float cornerRadius, int segments, out Vector3[] curvedPath, out float[] cumulativeLength)
        {
            var points = new List<Vector3>
            {
                gridPath[0]
            };
            for (int i = 1; i < gridPath.Length - 1; i++)
            {
                Vector3 dirPrev = (gridPath[i] - gridPath[i - 1]).normalized;
                Vector3 dirNext = (gridPath[i + 1] - gridPath[i]).normalized;

                //straight
                if (dirPrev == dirNext)
                {
                    points.Add(gridPath[i]);
                    continue;
                }

                float maxRadius = Mathf.Min(Vector3.Distance(gridPath[i], gridPath[i - 1]), Vector3.Distance(gridPath[i], gridPath[i + 1]) / 2);
                float radius = Mathf.Clamp(cornerRadius, 0.01f, maxRadius);

                Vector3 startVerticle = gridPath[i] - dirPrev * radius;
                Vector3 endVerticle = gridPath[i] + dirNext * radius;

                points.Add(startVerticle);
                for (int j = 1; j < segments; j++)
                {
                    float t = (float)j / segments;
                    var preLerp = Vector3.Lerp(startVerticle, gridPath[i], t);
                    var nextLerp = Vector3.Lerp(gridPath[i], endVerticle, t);
                    points.Add(Vector3.Lerp(preLerp, nextLerp, t));
                }
                points.Add(endVerticle);
            }
            points.Add(gridPath[gridPath.Length - 1]);

            curvedPath = points.ToArray();
            cumulativeLength = new float[curvedPath.Length];
            cumulativeLength[0] = 0;
            for (int i = 1; i < curvedPath.Length; i++)
            {
                cumulativeLength[i] = cumulativeLength[i - 1] + Vector3.Distance(curvedPath[i - 1], curvedPath[i]);
            }
        }
    }
}
