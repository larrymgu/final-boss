using UnityEngine;

namespace FinalBoss
{
    public class Player : Unit
    {
        [Header("Movement")]
        public float moveSpeed = 3f;

        [Header("Shape")]
        public float length = 1f;
        public float width = 0.8f;

        [Header("Dash (Z)")]
        public float dashDistance = 2f;
        public float dashCooldown = 1.5f;
        public float dashSpeed = 8f;
        public AudioClip dashSound;

        [Header("Shot (X)")]
        public float shotCooldown = 1f;
        public float shotSpeed = 9f;
        public int shotDamage = 1;
        public Projectile shotPrefab;
        public AudioClip shotSound;

        public static Player Current { get; private set; }

        Vector2 facing = Vector2.up;
        bool dashing;
        Vector2 dashDir;
        float dashTravelled;

        public float DashCooldownLeft { get; private set; }
        public float ShotCooldownLeft { get; private set; }
        public bool IsDashing => dashing;
        public Vector2 Facing => facing;

        public float ApproxRadius => length * 0.4f;

        public override bool IsPresent => gameObject.activeInHierarchy;

        public override bool CanBeHurt => IsAlive && !dashing;

        float DashDistance => dashDistance;

        void Reset()
        {
            maxHp = 3;
        }

        protected override void Awake()
        {
            base.Awake();
            Current = this;
            ApplyFacing();
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public override void ApplyBodySize()
        {
            SetBodyScale(new Vector3(width, length, 1f));
        }

        void Update()
        {
            if (!IsAlive || Game == null || !Game.IsPlaying) return;
            float dt = Time.deltaTime;
            DashCooldownLeft = Mathf.Max(0f, DashCooldownLeft - dt);
            ShotCooldownLeft = Mathf.Max(0f, ShotCooldownLeft - dt);

            if (dashing)
            {
                ContinueDash(dt);
                return;
            }

            Vector2 input = GameInput.Move;
            if (input != Vector2.zero)
            {
                facing = input.normalized;
                ApplyFacing();
            }

            if (GameInput.DashPressed && DashCooldownLeft <= 0f)
            {
                StartDash();
                ContinueDash(dt);
                return;
            }

            if (GameInput.ShootHeld && ShotCooldownLeft <= 0f) Shoot();

            Vector2 lo, hi;
            GetExtents(out lo, out hi);
            Vector2 dir = Arena.Slide(Position, lo, hi, input);
            Position = Arena.Clamp(Position + dir * (moveSpeed * dt), lo, hi);
        }

        void StartDash()
        {
            dashing = true;
            dashDir = facing;
            dashTravelled = 0f;
            DashCooldownLeft = dashCooldown;
            Sfx.Play(dashSound);
        }

        void ContinueDash(float dt)
        {
            Vector2 lo, hi;
            GetExtents(out lo, out hi);
            float remaining = Mathf.Min(dashSpeed * dt, DashDistance - dashTravelled);

            while (remaining > 1e-5f)
            {
                Vector2 dir = Arena.Slide(Position, lo, hi, dashDir);
                if (dir == Vector2.zero)
                {
                    dashTravelled = DashDistance;
                    break;
                }
                float step = Mathf.Min(remaining, GameManager.MaxStep);
                Position = Arena.Clamp(Position + dir * step, lo, hi);
                dashTravelled += step;
                remaining -= step;
            }

            if (dashTravelled >= DashDistance - 1e-4f) dashing = false;
        }

        void Shoot()
        {
            ShotCooldownLeft = shotCooldown;
            if (shotPrefab == null)
            {
                Debug.LogWarning("Player has no Shot Prefab assigned, so X does nothing.", this);
                return;
            }
            Vector2 tip = Position + facing * (length * 2f / 3f);
            Projectile shot = Instantiate(shotPrefab, new Vector3(tip.x, tip.y, 0f), Quaternion.identity);
            shot.Launch(Team.Player, facing, shotSpeed, shotDamage);
            Sfx.Play(shotSound);
        }

        public override bool Overlaps(Vector2 centre, float radius)
        {
            Vector2 a, b, c;
            GetTriangle(out a, out b, out c);
            return Geometry.CircleTriangle(centre, radius, a, b, c);
        }

        public void GetTriangle(out Vector2 tip, out Vector2 left, out Vector2 right)
        {
            Offsets(out tip, out left, out right);
            Vector2 p = Position;
            tip += p;
            left += p;
            right += p;
        }

        void Offsets(out Vector2 tip, out Vector2 left, out Vector2 right)
        {
            Vector2 f = facing;
            Vector2 side = new Vector2(-f.y, f.x);
            Vector2 back = -f * (length / 3f);
            tip = f * (length * 2f / 3f);
            left = back + side * (width * 0.5f);
            right = back - side * (width * 0.5f);
        }

        void GetExtents(out Vector2 lo, out Vector2 hi)
        {
            Vector2 a, b, c;
            Offsets(out a, out b, out c);
            lo = Vector2.Min(a, Vector2.Min(b, c));
            hi = Vector2.Max(a, Vector2.Max(b, c));
        }

        void ApplyFacing()
        {
            if (body == null) return;
            float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
            body.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        protected override void Die()
        {
            dashing = false;
        }
    }
}
