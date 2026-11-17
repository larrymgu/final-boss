using System.Collections.Generic;
using UnityEngine;

namespace FinalBoss
{
    public abstract class Enemy : Unit
    {
        [Header("Body")]
        public float diameter = 0.7f;
        public float moveSpeed = 2.5f;

        [Header("Death")]
        public AudioClip deathSound;

        static readonly List<Enemy> all = new List<Enemy>();

        float touchCooldownLeft;

        protected virtual float TouchRepeatTime => 1f;

        public static IReadOnlyList<Enemy> All => all;

        public float Radius => diameter * 0.5f;
        protected float radius => diameter * 0.5f;
        protected Player Player => FinalBoss.Player.Current;

        protected bool CanAct => IsAlive && Game != null && Game.IsPlaying && Player != null && Player.IsAlive;

        protected float DistanceToPlayer => Vector2.Distance(Position, Player.Position);

        protected override void Awake()
        {
            base.Awake();
            if (!all.Contains(this)) all.Add(this);
        }

        protected virtual void OnDestroy()
        {
            all.Remove(this);
        }

        protected override void Die()
        {
            Sfx.Play(deathSound);
            base.Die();
        }

        public override void ApplyBodySize()
        {
            SetBodyScale(new Vector3(diameter, diameter, 1f));
        }

        public override bool Overlaps(Vector2 centre, float r)
        {
            return Geometry.CirclesOverlap(Position, radius, centre, r);
        }

        protected void Walk(Vector2 direction, float dt)
        {
            Vector2 dir = Arena.Slide(Position, radius, direction);
            Position = Arena.Clamp(Position + dir * (moveSpeed * dt), radius);
        }

        protected void WalkTowardPlayer(float dt)
        {
            Walk(Player.Position - Position, dt);
        }

        protected void WalkAwayFromPlayer(float dt)
        {
            Vector2 away = Position - Player.Position;
            if (away.sqrMagnitude < 1e-6f) away = Vector2.up;
            Vector2 dir = Arena.SlideAway(Position, radius, away, Player.Position);
            Position = Arena.Clamp(Position + dir * (moveSpeed * dt), radius);
        }

        protected void KeepDistance(float minRange, float maxRange, float dt)
        {
            float d = DistanceToPlayer;
            if (d > maxRange) WalkTowardPlayer(dt);
            else if (d < minRange) WalkAwayFromPlayer(dt);
        }

        protected Vector2 DirectionToPlayer()
        {
            Vector2 d = Player.Position - Position;
            return d.sqrMagnitude < 1e-8f ? Vector2.down : d.normalized;
        }

        protected bool AdvanceDash(Vector2 dashDir, float speed, float distance, ref float travelled,
                                   ref bool hitPlayer, int damage, float dt)
        {
            float remaining = Mathf.Min(speed * dt, distance - travelled);
            while (remaining > 1e-5f)
            {
                Vector2 dir = Arena.Slide(Position, radius, dashDir);
                if (dir == Vector2.zero) return true;
                float step = Mathf.Min(remaining, GameManager.MaxStep);
                Position = Arena.Clamp(Position + dir * step, radius);
                travelled += step;
                remaining -= step;

                if (!hitPlayer && Player.CanBeHurt && Player.Overlaps(Position, radius))
                {
                    hitPlayer = true;
                    Player.TakeDamage(damage);
                    touchCooldownLeft = TouchRepeatTime;
                }
            }
            return travelled >= distance - 1e-4f;
        }

        protected void ApplyTouchDamage(int damage, float dt)
        {
            if (touchCooldownLeft > 0f) touchCooldownLeft -= dt;
            if (touchCooldownLeft > 0f || !IsPresent) return;
            if (Player.CanBeHurt && Player.Overlaps(Position, radius))
            {
                Player.TakeDamage(damage);
                touchCooldownLeft = TouchRepeatTime;
            }
        }
    }
}
