using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MathHelper
{

    public class Plain
    {

        Vector3 A;
        Vector3 AB;
        Vector3 AC;

        public Plain(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            AB = b - a;
            AC = c - a;
        }

        // x = A + (t * AB) + (s * AC)
        /*
         * 
         * A.x + AB.x * t + AC.x * s = x.x
         * A.y + AB.y * t + AC.y * s = x.y
         * A.z + AB.z * t + AC.z * s = x.z
         * 
         * t = det (A, point, AC) / det (A, AB, AC)
         * 
         * s = det (A, AB, point) / det (A, AB, AC)
         *  
         */

        public bool IsPointOnPlane(Vector3 P)
        {
            float t; float s; float detA; float detA2; float detA3;

            detA = (A.x * AB.y * AC.z) + (AB.x * AC.y * A.z) + (AC.x * A.y * AB.z) -
                   (A.z * AB.y * AC.x) - (AB.z * AC.y * A.x) - (AC.z * A.y * AB.x);

            detA2 = (A.x * P.y * AC.z) + (P.x * AC.y * A.z) + (AC.x * A.y * P.z) -
                    (A.z * P.y * AC.x) - (P.z * AC.y * A.x) - (AC.z * A.y * P.x);

            detA3 = (A.x * AB.y * P.z) + (AB.x * P.y * A.z) + (P.x * A.y * AB.z) -
                    (A.z * AB.y * P.x) - (AB.z * P.y * A.x) - (P.z * A.y * AB.x);

            t = detA2 / detA;
            s = detA3 / detA;

            return P == A + (t * AB) + (s * AC);
        }

        private static float r = 1000;
        public static bool IsPointOnPlane(Vector3 surfacePoint, Vector3 normal, Vector3 point)
        {
            Vector3 roundedSP = new Vector3(((int)(surfacePoint.x * r)) / r, ((int)(surfacePoint.y * r)) / r, ((int)(surfacePoint.z * r)) / r);
            Vector3 roundedN = new Vector3(((int)(normal.x * r)) / r, ((int)(normal.y * r)) / r, ((int)(normal.z * r)) / r);
            Vector3 roundedP = new Vector3(((int)(point.x * r)) / r, ((int)(point.y * r)) / r, ((int)(point.z * r)) / r);
            return Vector3.Dot(roundedP - roundedSP, roundedN) == 0;
        }

        public static Vector3 GetOrthogonalProjectionOfVector(Vector3 x, Vector3 u, Vector3 v)
        {
            //return Vector3.Dot(x, u) / Vector3.Dot(u, u) * u + Vector3.Dot(x, v) / Vector3.Dot(v, v) * v;
            return ((x.x * u.x + x.y * u.y + x.z * u.z) / (u.x * u.x + u.y * u.y + u.z * u.z) * u) +
                   ((x.x * v.x + x.y * v.y + x.z * v.z) / (v.x * v.x + v.y * v.y + v.z * v.z) * v);
        }
    }

    public static class LineCalculations
    {

        private static float CrossProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - b.x * a.y;
        }

        private static float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.y - a.y) * (b.x - a.x) - (c.x - a.x) * (b.y - a.y);
        }

        private static float DotProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.x - a.x) * (b.x - a.x) + (c.y - a.y) * (b.y - a.y);
        }

        // 0 - point is not on line
        // 1 - point is equals to the start- or endpoint of the line
        // 2 - point is between the start and endpoint of the line

        public static int IsPointOnLineSegment(Vector2 start, Vector2 end, Vector2 point)
        {
            if (point == start || point == end) return 1;

            float cross = CrossProduct(start, end, point);
            if (Mathf.Abs(cross) > Mathf.Epsilon) return 0;

            float dot = DotProduct(start, end, point);
            if (dot < 0) return 0;

            if (dot > Mathf.Pow((end - start).magnitude, 2)) return 0;

            return 2;
        }
                
        public static bool LineLineIntersection(Vector2 l1A, Vector2 l1B, Vector2 l2A, Vector2 l2B, out Vector2 intersection, out bool overlapping)
        {
            intersection = Vector2.zero;
            overlapping = false;
            float x1 = l1A.x, x2 = l1B.x, x3 = l2A.x, x4 = l2B.x;
            float y1 = l1A.y, y2 = l1B.y, y3 = l2A.y, y4 = l2B.y;

            float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);            

            if (den == 0)
            {                
                overlapping = IsPointOnLineSegment(l1A, l1B, l2A) == 2 ||
                              IsPointOnLineSegment(l1A, l1B, l2B) == 2 ||
                              IsPointOnLineSegment(l2A, l2B, l1A) == 2 ||
                              IsPointOnLineSegment(l2A, l2B, l1B) == 2;
                return overlapping;
            }

            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            float u = - ((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;

            if (t >= 0 && t <= 1 &&  u >= 0 && u <= 1)
            {
                intersection = l1A + (l1B - l1A) * (int)(t * 100) / 100f;
                return true;
            }
            return false;
        }
    }

    public class BezierCurve
    {

        private static float t;

        public static Vector3[] CalculateCurvePoints(Vector3 start, Vector3 middle, Vector3 end, Vector3[] result)
        {  
            for (int i=0; i<result.Length; i++)
            {
                t = (float)i / (result.Length - 1);
                result[i] = Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * middle + Mathf.Pow(t, 2) * end;
            }

            return result;
        }
    }
}

