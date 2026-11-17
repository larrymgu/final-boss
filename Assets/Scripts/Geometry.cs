using UnityEngine;

namespace FinalBoss
{
    public static class Geometry
    {
        public static bool CirclesOverlap(Vector2 a, float ra, Vector2 b, float rb)
        {
            float r = ra + rb;
            return (a - b).sqrMagnitude <= r * r;
        }

        public static bool CircleTriangle(Vector2 c, float r, Vector2 t0, Vector2 t1, Vector2 t2)
        {
            if (PointInTriangle(c, t0, t1, t2)) return true;
            float r2 = r * r;
            return DistSqToSegment(c, t0, t1) <= r2 ||
                   DistSqToSegment(c, t1, t2) <= r2 ||
                   DistSqToSegment(c, t2, t0) <= r2;
        }

        public static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(b - a, p - a);
            float d2 = Cross(c - b, p - b);
            float d3 = Cross(a - c, p - c);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }

        public static float DistSqToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-8f);
            t = Mathf.Clamp01(t);
            return (a + ab * t - p).sqrMagnitude;
        }

        static float Cross(Vector2 u, Vector2 v)
        {
            return u.x * v.y - u.y * v.x;
        }
    }
}
