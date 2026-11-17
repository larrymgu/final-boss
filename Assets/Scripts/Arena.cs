using UnityEngine;

namespace FinalBoss
{
    public static class Arena
    {
        const float WallTolerance = 0.0005f;

        public static Rect Bounds
        {
            get
            {
                var cam = Camera.main;
                if (cam == null) return Rect.MinMaxRect(-8.89f, -5f, 8.89f, 5f);
                float halfH = cam.orthographicSize;
                float halfW = halfH * cam.aspect;
                Vector2 c = cam.transform.position;
                return Rect.MinMaxRect(c.x - halfW, c.y - halfH, c.x + halfW, c.y + halfH);
            }
        }

        public static Vector2 Slide(Vector2 pos, Vector2 lo, Vector2 hi, Vector2 dir)
        {
            if (dir.sqrMagnitude < 1e-10f) return Vector2.zero;
            dir.Normalize();
            Rect b = Bounds;
            if (dir.x < 0f && pos.x + lo.x <= b.xMin + WallTolerance) dir.x = 0f;
            if (dir.x > 0f && pos.x + hi.x >= b.xMax - WallTolerance) dir.x = 0f;
            if (dir.y < 0f && pos.y + lo.y <= b.yMin + WallTolerance) dir.y = 0f;
            if (dir.y > 0f && pos.y + hi.y >= b.yMax - WallTolerance) dir.y = 0f;
            float m = dir.sqrMagnitude;
            return m < 1e-10f ? Vector2.zero : dir / Mathf.Sqrt(m);
        }

        public static Vector2 Slide(Vector2 pos, float radius, Vector2 dir)
        {
            return Slide(pos, new Vector2(-radius, -radius), new Vector2(radius, radius), dir);
        }

        public static Vector2 SlideAway(Vector2 pos, float radius, Vector2 dir, Vector2 awayFrom)
        {
            Vector2 d = Slide(pos, radius, dir);
            if (d != Vector2.zero || dir.sqrMagnitude < 1e-10f) return d;

            Vector2 perp = new Vector2(-dir.y, dir.x).normalized;
            Vector2 a = Slide(pos, radius, perp);
            Vector2 c = Slide(pos, radius, -perp);
            if (a == Vector2.zero) return c;
            if (c == Vector2.zero) return a;
            return ((pos + a) - awayFrom).sqrMagnitude >= ((pos + c) - awayFrom).sqrMagnitude ? a : c;
        }

        public static Vector2 Clamp(Vector2 pos, Vector2 lo, Vector2 hi)
        {
            Rect b = Bounds;
            pos.x = ClampAxis(pos.x, b.xMin - lo.x, b.xMax - hi.x);
            pos.y = ClampAxis(pos.y, b.yMin - lo.y, b.yMax - hi.y);
            return pos;
        }

        public static Vector2 Clamp(Vector2 pos, float radius)
        {
            return Clamp(pos, new Vector2(-radius, -radius), new Vector2(radius, radius));
        }

        public static bool Contains(Vector2 pos, float radius)
        {
            Rect b = Bounds;
            return pos.x - radius >= b.xMin && pos.x + radius <= b.xMax &&
                   pos.y - radius >= b.yMin && pos.y + radius <= b.yMax;
        }

        static float ClampAxis(float v, float min, float max)
        {
            return min > max ? (min + max) * 0.5f : Mathf.Clamp(v, min, max);
        }
    }
}
