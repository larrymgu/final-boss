using UnityEngine;
using UnityEngine.SceneManagement;

namespace FinalBoss
{
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public const float MaxStep = 0.05f;

        public static GameManager Instance { get; private set; }

        enum RoundState { Playing, Won, Lost }

        [Header("Arena")]
        public float cameraSize = 5f;
        public Color backgroundColor = new Color(0.72f, 0.74f, 0.78f);

        public bool IsPlaying => roundState == RoundState.Playing;

        RoundState roundState = RoundState.Playing;

        GUIStyle hpStyle;
        GUIStyle endStyle;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Time.timeScale = 1f;
            SetUpCamera();
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (GameInput.RestartPressed)
            {
                Restart();
                return;
            }
            if (roundState != RoundState.Playing) return;

            Player player = Player.Current;
            if (player != null && !player.IsAlive) EndRound(RoundState.Lost);
            else if (AllHeroesDown()) EndRound(RoundState.Won);
        }

        void EndRound(RoundState result)
        {
            roundState = result;
            Time.timeScale = 0f;
        }

        static bool AllHeroesDown()
        {
            var heroes = Enemy.All;
            if (heroes.Count == 0) return false;
            for (int i = 0; i < heroes.Count; i++)
                if (heroes[i] != null && heroes[i].IsAlive) return false;
            return true;
        }

        void Restart()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0)
            {
                SceneManager.LoadScene(scene.buildIndex);
                return;
            }
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Debug.LogWarning("Add this scene to the build's scene list so R can restart it.");
#endif
        }

        void SetUpCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = cameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
        }

        void OnGUI()
        {
            var cam = Camera.main;
            Player player = Player.Current;
            if (cam == null || player == null) return;
            EnsureStyles();

            float pixelsPerUnit = Screen.height / (2f * cam.orthographicSize);
            hpStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(pixelsPerUnit * 0.34f));
            DrawHp(cam, player);
            var heroes = Enemy.All;
            for (int i = 0; i < heroes.Count; i++)
            {
                if (heroes[i] != null && heroes[i].IsPresent) DrawHp(cam, heroes[i]);
            }

            if (roundState == RoundState.Playing) return;
            Color old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;

            endStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.1f);
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height),
                roundState == RoundState.Won ? "YOU WIN" : "HEROES WIN", endStyle);
        }

        void DrawHp(Camera cam, Unit unit)
        {
            Vector3 sp = cam.WorldToScreenPoint(unit.transform.position);
            float h = hpStyle.fontSize * 1.6f;
            float w = hpStyle.fontSize * 3f;
            var rect = new Rect(sp.x - w * 0.5f, Screen.height - sp.y - h * 0.5f, w, h);
            hpStyle.normal.textColor = TextColorFor(unit);
            GUI.Label(rect, unit.Hp.ToString(), hpStyle);
        }

        static Color TextColorFor(Unit unit)
        {
            Color c = unit.body != null ? unit.body.color : Color.black;
            float brightness = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            return brightness > 0.6f ? Color.black : Color.white;
        }

        void EnsureStyles()
        {
            if (hpStyle != null) return;

            hpStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                clipping = TextClipping.Overflow
            };
            endStyle = new GUIStyle(hpStyle);
            endStyle.normal.textColor = Color.white;
        }
    }
}
