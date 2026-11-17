using UnityEngine;

namespace FinalBoss
{
    public enum Team { Player, Enemy }

    public class Projectile : MonoBehaviour
    {
        public float diameter = 0.16f;

        Team team;
        Vector2 direction = Vector2.up;
        float speed;
        int damage;
        bool launched;

        float Radius => diameter * 0.5f;

        void Awake()
        {
            ApplySize();
        }

        void OnValidate()
        {
            ApplySize();
        }

        void ApplySize()
        {
            var scale = new Vector3(diameter, diameter, 1f);
            if (transform.localScale != scale) transform.localScale = scale;
        }

        public void Launch(Team owner, Vector2 dir, float moveSpeed, int hitDamage)
        {
            team = owner;
            direction = dir.sqrMagnitude > 1e-8f ? dir.normalized : Vector2.up;
            speed = moveSpeed;
            damage = hitDamage;
            launched = true;
        }

        void Update()
        {
            var game = GameManager.Instance;
            if (!launched || game == null || !game.IsPlaying) return;

            float remaining = speed * Time.deltaTime;
            while (remaining > 1e-5f)
            {
                float step = Mathf.Min(remaining, GameManager.MaxStep);
                transform.position += new Vector3(direction.x * step, direction.y * step, 0f);
                remaining -= step;
                if (TryHit())
                {
                    Destroy(gameObject);
                    return;
                }
            }

            Rect b = Arena.Bounds;
            Vector2 p = transform.position;
            float r = Radius;
            if (p.x < b.xMin - r || p.x > b.xMax + r || p.y < b.yMin - r || p.y > b.yMax + r)
                Destroy(gameObject);
        }

        bool TryHit()
        {
            Vector2 p = transform.position;
            float r = Radius;
            if (team == Team.Enemy)
            {
                Player player = Player.Current;
                if (player != null && player.CanBeHurt && player.Overlaps(p, r))
                {
                    player.TakeDamage(damage);
                    return true;
                }
                return false;
            }

            var heroes = Enemy.All;
            for (int i = 0; i < heroes.Count; i++)
            {
                Enemy e = heroes[i];
                if (e != null && e.CanBeHurt && e.Overlaps(p, r))
                {
                    e.TakeDamage(damage);
                    return true;
                }
            }
            return false;
        }
    }
}
