using UnityEngine;

namespace FinalBoss
{
    public class Cleric : Enemy
    {
        [Header("Healing")]
        public Unit[] healTargets = new Unit[0];
        public int healAmount = 1;
        public float healInterval = 5f;
        public float healDuration = 0.75f;
        public Color glowColor = new Color(1f, 0.94f, 0.3f);
        public float beamWidth = 0.07f;
        public Sprite beamSprite;
        public AudioClip healSound;

        [Header("Spacing")]
        public float minRange = 4.5f;
        public float maxRange = 6.5f;

        SpriteRenderer[] beams = new SpriteRenderer[0];
        float healTimer;
        bool channeling;
        float channelTime;

        void Reset()
        {
            maxHp = 5;
        }

        void Start()
        {
            healTimer = healInterval;

            beams = new SpriteRenderer[healTargets.Length];
            for (int i = 0; i < beams.Length; i++)
            {
                var go = new GameObject("Heal Beam");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = beamSprite;
                sr.color = glowColor;
                sr.sortingOrder = 5;
                sr.enabled = false;
                beams[i] = sr;
            }
        }

        void Update()
        {
            if (!CanAct)
            {
                SetBeamsVisible(false);
                return;
            }
            float dt = Time.deltaTime;
            healTimer -= dt;

            if (channeling)
            {
                channelTime += dt;
                UpdateBeams();
                if (channelTime >= healDuration)
                {
                    channeling = false;
                    SetBeamsVisible(false);
                }
                return;
            }

            if (healTimer <= 0f)
            {
                healTimer += healInterval;
                StartHeal();
                return;
            }

            KeepDistance(minRange, maxRange, dt);
        }

        void StartHeal()
        {
            channeling = true;
            channelTime = 0f;
            Sfx.Play(healSound);

            foreach (var ally in healTargets)
            {
                if (ally == null || ally == this || !ally.IsPresent) continue;
                ally.Heal(healAmount);
                ally.ShowHealGlow(glowColor, healDuration);
            }
            ShowHealGlow(glowColor, healDuration);

            UpdateBeams();
        }

        void UpdateBeams()
        {
            for (int i = 0; i < beams.Length; i++)
            {
                Unit ally = healTargets[i];
                bool show = ally != null && ally != this && ally.IsPresent && beamSprite != null;
                beams[i].enabled = show;
                if (!show) continue;

                Vector2 from = Position;
                Vector2 to = ally.Position;
                Vector2 delta = to - from;
                Transform t = beams[i].transform;
                t.position = new Vector3((from.x + to.x) * 0.5f, (from.y + to.y) * 0.5f, 0f);
                t.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                t.localScale = new Vector3(delta.magnitude, beamWidth, 1f);
            }
        }

        void SetBeamsVisible(bool visible)
        {
            foreach (var b in beams) b.enabled = visible;
        }
    }
}
