using UnityEngine;

namespace FinalBoss
{
    public class Fighter : Enemy
    {
        [Header("Dash attack")]
        public int damage = 1;
        public float engageGap = 0.9f;
        public float dashDistance = 1.4f;
        public float flashInterval = 0.25f;
        public int flashCount = 2;
        public float dashCooldown = 3f;
        public float dashSpeed = 8f;
        public float recoverTime = 0.15f;
        public AudioClip attackSound;

        [Header("Touch")]
        public float touchRepeatTime = 1f;

        enum State { Chase, Windup, Dash, Recover }

        State state = State.Chase;
        float stateTime;
        float dashCooldownLeft;
        Vector2 dashDir;
        float dashTravelled;
        bool dashHitPlayer;

        float DashDistance => dashDistance;

        protected override float TouchRepeatTime => touchRepeatTime;

        void Reset()
        {
            maxHp = 10;
        }

        void Update()
        {
            flashWhite = false;
            if (!CanAct) return;

            float dt = Time.deltaTime;
            stateTime += dt;
            dashCooldownLeft -= dt;

            switch (state)
            {
                case State.Chase:
                {
                    float gap = DistanceToPlayer - radius - Player.ApproxRadius;
                    if (gap > engageGap) WalkTowardPlayer(dt);
                    else if (dashCooldownLeft <= 0f) SetState(State.Windup);
                    break;
                }

                case State.Windup:
                    flashWhite = IsFlashing(stateTime, flashInterval, flashCount);
                    if (stateTime >= flashInterval * flashCount)
                    {
                        dashDir = DirectionToPlayer();
                        dashTravelled = 0f;
                        dashHitPlayer = false;
                        dashCooldownLeft = dashCooldown;
                        Sfx.Play(attackSound);
                        SetState(State.Dash);
                    }
                    break;

                case State.Dash:
                    if (AdvanceDash(dashDir, dashSpeed, DashDistance, ref dashTravelled, ref dashHitPlayer, damage, dt))
                        SetState(State.Recover);
                    break;

                case State.Recover:
                    if (stateTime >= recoverTime) SetState(State.Chase);
                    break;
            }

            ApplyTouchDamage(damage, dt);
        }

        void SetState(State next)
        {
            state = next;
            stateTime = 0f;
        }
    }
}
