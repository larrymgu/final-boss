using UnityEngine;

namespace FinalBoss
{
    public class Rogue : Enemy
    {
        [Header("Ambush")]
        public int damage = 3;
        public float appearInterval = 5f;
        public float appearDistance = 2f;
        public float flashInterval = 0.25f;
        public int flashCount = 2;
        public float dashSpeed = 16f;
        public float lingerTime = 0.5f;
        public AudioClip attackSound;

        [Header("Touch")]
        public float touchRepeatTime = 1f;

        enum State { InRow, Hidden, Windup, Dash, Linger }

        State state = State.InRow;
        float stateTime;
        float appearTimer;
        Vector2 dashDir;
        float dashTravelled;
        bool dashHitPlayer;

        float DashDistance => diameter * 2f;

        public override bool IsPresent => base.IsPresent && state != State.Hidden;

        protected override float TouchRepeatTime => touchRepeatTime;

        void Reset()
        {
            maxHp = 2;
        }

        void Start()
        {
            appearTimer = appearInterval;
        }

        void Update()
        {
            flashWhite = false;
            if (!CanAct) return;

            float dt = Time.deltaTime;
            stateTime += dt;
            appearTimer -= dt;

            switch (state)
            {
                case State.InRow:
                case State.Hidden:
                    if (appearTimer <= 0f)
                    {
                        appearTimer += appearInterval;
                        Appear();
                    }
                    break;

                case State.Windup:
                    flashWhite = IsFlashing(stateTime, flashInterval, flashCount);
                    if (stateTime >= flashInterval * flashCount)
                    {
                        dashDir = DirectionToPlayer();
                        dashTravelled = 0f;
                        dashHitPlayer = false;
                        Sfx.Play(attackSound);
                        SetState(State.Dash);
                    }
                    break;

                case State.Dash:
                    if (AdvanceDash(dashDir, dashSpeed, DashDistance, ref dashTravelled, ref dashHitPlayer, damage, dt))
                        SetState(State.Linger);
                    break;

                case State.Linger:
                    if (stateTime >= lingerTime) Vanish();
                    break;
            }

            ApplyTouchDamage(damage, dt);
        }

        void Appear()
        {
            Position = PickSpotNearPlayer();
            if (body != null) body.enabled = true;
            SetState(State.Windup);
        }

        void Vanish()
        {
            if (body != null) body.enabled = false;
            SetState(State.Hidden);
        }

        Vector2 PickSpotNearPlayer()
        {
            Vector2 centre = Player.Position;
            Vector2 fallback = centre;
            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector2 spot = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * appearDistance;
                if (Arena.Contains(spot, radius)) return spot;
                if (i == 0) fallback = spot;
            }
            return Arena.Clamp(fallback, radius);
        }

        void SetState(State next)
        {
            state = next;
            stateTime = 0f;
        }
    }
}
