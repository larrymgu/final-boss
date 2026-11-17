using UnityEngine;

namespace FinalBoss
{
    public class Mage : Enemy
    {
        [Header("Shooting")]
        public int damage = 1;
        public float shotInterval = 2f;
        public float aimTime = 0.25f;
        public float recoverTime = 0.15f;
        public float shotSpeed = 5f;
        public Projectile ballPrefab;
        public AudioClip shotSound;

        [Header("Spacing")]
        public float minRange = 3.5f;
        public float maxRange = 5.5f;

        float shotTimer;
        bool casting;
        bool firedThisCast;
        float castTime;

        void Reset()
        {
            maxHp = 5;
        }

        void Start()
        {
            shotTimer = shotInterval;
        }

        void Update()
        {
            if (!CanAct) return;
            float dt = Time.deltaTime;

            shotTimer -= dt;

            if (casting)
            {
                castTime += dt;
                if (!firedThisCast && castTime >= aimTime)
                {
                    firedThisCast = true;
                    Fire();
                }
                if (castTime >= aimTime + recoverTime) casting = false;
                return;
            }

            if (shotTimer <= 0f)
            {
                shotTimer += shotInterval;
                casting = true;
                firedThisCast = false;
                castTime = 0f;
                return;
            }

            KeepDistance(minRange, maxRange, dt);
        }

        void Fire()
        {
            if (ballPrefab == null)
            {
                Debug.LogWarning("Mage has no Ball Prefab assigned, so it can't shoot.", this);
                return;
            }
            Vector2 dir = DirectionToPlayer();
            Vector2 from = Position + dir * (radius + ballPrefab.diameter * 0.5f);
            Projectile ball = Instantiate(ballPrefab, new Vector3(from.x, from.y, 0f), Quaternion.identity);
            ball.Launch(Team.Enemy, dir, shotSpeed, damage);
            Sfx.Play(shotSound);
        }
    }
}
