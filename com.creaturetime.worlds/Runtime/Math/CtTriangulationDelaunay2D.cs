
using System;
using UnityEngine;

namespace CreatureTime
{
    public static class CtTriangulationDelaunay2D
    {
        private static bool TrianglePolySubFunc_InCircle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            if (Mathf.Abs(p1.y - p2.y) < float.Epsilon && Mathf.Abs(p2.y - p3.y) < float.Epsilon)
            {
                return false;
            }

            float m1, m2, mx1, mx2, my1, my2, xc, yc;
            if (Mathf.Abs(p2.y - p1.y) < float.Epsilon)
            {
                m2 = -(p3.x - p2.x) / (p3.y - p2.y);
                mx2 = (p2.x + p3.x) * 0.5f;
                my2 = (p2.y + p3.y) * 0.5f;
                xc = (p2.x + p1.x) * 0.5f;
                yc = m2 * (xc - mx2) + my2;
            }
            else if (Mathf.Abs(p3.y - p2.y) < float.Epsilon)
            {
                m1 = -(p2.x - p1.x) / (p2.y - p1.y);
                mx1 = (p1.x + p2.x) * 0.5f;
                my1 = (p1.y + p2.y) * 0.5f;
                xc = (p3.x + p2.x) * 0.5f;
                yc = m1 * (xc - mx1) + my1;
            }
            else
            {
                m1 = -(p2.x - p1.x) / (p2.y - p1.y);
                m2 = -(p3.x - p2.x) / (p3.y - p2.y);
                mx1 = (p1.x + p2.x) * 0.5f;
                mx2 = (p2.x + p3.x) * 0.5f;
                my1 = (p1.y + p2.y) * 0.5f;
                my2 = (p2.y + p3.y) * 0.5f;
                xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
                yc = m1 * (xc - mx1) + my1;
            }
        
            float dx = p2.x - xc;
            float dy = p2.y - yc;
            float rsqr = dx * dx + dy * dy;
            dx = p.x - xc;
            dy = p.y - yc;
            double drsqr = dx * dx + dy * dy;
            return (drsqr <= rsqr);
        }

        private static void _CreateSuperTriangle(Vector2[] points, out Vector2 p1, out Vector2 p2, out Vector2 p3)
        {
            var margin = 50;

            var xMin = points[0].x;
            var yMin = points[0].y;
            var xMax = xMin;
            var yMax = yMin;

            for (var i = 1; i < points.Length; i++)
            {
                if (points[i].x < xMin) xMin = points[i].x;
                else if (points[i].x > xMax) xMax = points[i].x;
                if (points[i].y < yMin) yMin = points[i].y;
                else if (points[i].y > yMax) yMax = points[i].y;
            }

            xMin -= margin;
            xMax += margin;
            yMin -= margin;
            yMax += margin;

            var xMid = (xMin + xMax) / 2;   
            var boundarySize = new Vector2(xMax - xMin, yMax - yMin);

            p1 = new Vector2(xMid, boundarySize.y * 2);
            p2 = new Vector2(xMid - boundarySize.x, yMin);
            p3 = new Vector2(xMid + boundarySize.x, yMin);
        }

        public static Vector3Int[] Calculate(Vector2[] points)
        {
            var triangulation = new Vector3Int[] {};

            if (points.Length == 0) return triangulation;

            _CreateSuperTriangle(points, out var p1, out var p2, out var p3);

            var vertexCount = points.Length;

            // var a = new Vector3(p1.x, 10, p1.y);
            // var b = new Vector3(p2.x, 10, p2.y);
            // var c = new Vector3(p3.x, 10, p3.y);
            //
            // Debug.DrawLine(a, b, Color.blue, 5f);
            // Debug.DrawLine(b, c, Color.blue, 5f);
            // Debug.DrawLine(c, a, Color.blue, 5f);

            var expanded = new Vector2[3 + vertexCount];
            for (var i = 0; i < vertexCount; i++)
            {
                expanded[i] = points[i];
            }

            expanded[vertexCount] = p1;
            expanded[vertexCount + 1] = p2;
            expanded[vertexCount + 2] = p3;

            CtArrayUtils.Insert(ref triangulation, new Vector3Int(vertexCount, vertexCount + 1, vertexCount + 2), -1);

            for (var i = 0; i < vertexCount; i++)
            {
                var badTriangles = new Vector3Int[] { };
                var updatedTriangulation = new Vector3Int[] { };

                var point = expanded[i];
                for (var j = 0; j < triangulation.Length; j++)
                {
                    var triangle = triangulation[j];
                    var isDelaunayTriangle = !TrianglePolySubFunc_InCircle(point,
                        expanded[triangle.x], expanded[triangle.y], expanded[triangle.z]);

                    // Debug.Log($"triangle checking against {i} {j} {isDelaunayTriangle} {point} {triangle}");
                    if (isDelaunayTriangle)
                    {
                        CtArrayUtils.Insert(ref updatedTriangulation, triangle, -1);
                    }
                    else
                    {
                        CtArrayUtils.Insert(ref badTriangles, triangle, -1);
                    }
                }

                var edges = new Vector2Int[] { };
                for (var j = 0; j < badTriangles.Length; j++)
                {
                    var badTriangle = badTriangles[j];

                    var edgeA = new Vector2Int(badTriangle.x, badTriangle.y);
                    var edgeB = new Vector2Int(badTriangle.y, badTriangle.z);
                    var edgeC = new Vector2Int(badTriangle.x, badTriangle.z);

                    var index = Array.IndexOf(edges, edgeA);
                    // Debug.Log($"triangle edges index {edgeA} {index}");
                    if (index != -1)
                        CtArrayUtils.Pop(ref edges, index);
                    else
                        CtArrayUtils.Insert(ref edges, edgeA, -1);

                    index = Array.IndexOf(edges, edgeB);
                    // Debug.Log($"triangle edges index {edgeB} {index}");
                    if (index != -1)
                        CtArrayUtils.Pop(ref edges, index);
                    else
                        CtArrayUtils.Insert(ref edges, edgeB, -1);

                    index = Array.IndexOf(edges, edgeC);
                    // Debug.Log($"triangle edges index {edgeC} {index}");
                    if (index != -1)
                        CtArrayUtils.Pop(ref edges, index);
                    else
                        CtArrayUtils.Insert(ref edges, edgeC, -1);

                    // Debug.Log($"triangle badTris {edges}");
                }

                var text = string.Empty;
                if (edges.Length > 0)
                {
                    text = edges[0].ToString();
                    for (var v = 1; v < edges.Length; v++)
                    {
                        text += ", ";
                        text += edges[v].ToString();
                    }
                }

                // Debug.Log($"triangle edges {text}");
                for (var j = 0; j < edges.Length; j++)
                {
                    var edge = edges[j];
                    CtArrayUtils.Insert(ref updatedTriangulation, new Vector3Int(edge.x, edge.y, i), -1);
                }

                text = string.Empty;
                if (updatedTriangulation.Length > 0)
                {
                    text = updatedTriangulation[0].ToString();
                    for (var v = 1; v < updatedTriangulation.Length; v++)
                    {
                        text += ", ";
                        text += updatedTriangulation[v].ToString();
                    }
                }
                Debug.Log($"triangle updatedTriangulation {text}");

                triangulation = updatedTriangulation;
            }

            for (var i = triangulation.Length - 1; i >= 0; i--)
            {
                var t = triangulation[i];
                if (t.x >= vertexCount || t.y >= vertexCount || t.z >= vertexCount)
                    CtArrayUtils.Pop(ref triangulation, i);
            }

            return triangulation;
        }
    }
}