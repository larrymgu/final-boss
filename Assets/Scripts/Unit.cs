using UnityEngine;

namespace FinalBoss
{
    public abstract class Unit : MonoBehaviour
    {
        public SpriteRenderer body;

        [Header("Health")]
        public int maxHp = 5;

        public int Hp { get; private set; }
        public bool IsAlive => Hp > 0;

        public virtual bool IsPresent => IsAlive && gameObject.activeInHierarchy;

        public virtual bool CanBeHurt => IsPresent;

        public Vector2 Position
        {
            get { return transform.position; }
            protected set { transform.position = new Vector3(value.x, value.y, 0f); }
        }

        protected GameManager Game => GameManager.Instance;
        protected Color baseColor = Color.white;

        protected bool flashWhite;

        float healGlowLeft;
        Color healGlowColor;

        protected virtual void Awake()
        {
            if (body == null) body = GetComponentInChildren<SpriteRenderer>();
            if (body != null) baseColor = body.color;
            Hp = Mathf.Max(1, maxHp);
            ApplyBodySize();
        }

        protected virtual void OnValidate()
        {
            ApplyBodySize();
        }

        public abstract void ApplyBodySize();

        protected void SetBodyScale(Vector3 scale)
        {
            if (body == null) return;
            Transform t = body.transform;
            if (t.localScale != scale) t.localScale = scale;
        }

        public abstract bool Overlaps(Vector2 centre, float radius);

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !CanBeHurt) return;
            Hp = Mathf.Max(0, Hp - amount);
            if (Hp == 0) Die();
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            Hp = Mathf.Min(maxHp, Hp + amount);
        }

        public void ShowHealGlow(Color color, float duration)
        {
            healGlowColor = color;
            healGlowLeft = Mathf.Max(healGlowLeft, duration);
        }

        protected virtual void Die()
        {
            gameObject.SetActive(false);
        }

        protected virtual void LateUpdate()
        {
            if (healGlowLeft > 0f) healGlowLeft -= Time.deltaTime;
            if (body == null) return;

            Color c = baseColor;
            if (healGlowLeft > 0f) c = healGlowColor;
            if (flashWhite) c = Color.white;
            body.color = c;
        }

        protected static bool IsFlashing(float t, float interval, int count)
        {
            if (t < 0f || t >= interval * count) return false;
            return Mathf.Repeat(t, interval) < interval * 0.5f;
        }
    }
}
