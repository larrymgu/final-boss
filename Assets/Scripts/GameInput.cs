using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FinalBoss
{
    public static class GameInput
    {
        public static Vector2 Move
        {
            get
            {
                float x = 0f, y = 0f;
#if ENABLE_INPUT_SYSTEM
                var kb = Keyboard.current;
                if (kb == null) return Vector2.zero;
                if (kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.rightArrowKey.isPressed) x += 1f;
                if (kb.downArrowKey.isPressed) y -= 1f;
                if (kb.upArrowKey.isPressed) y += 1f;
#else
                if (Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
                if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
                if (Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                if (Input.GetKey(KeyCode.UpArrow)) y += 1f;
#endif
                return new Vector2(x, y);
            }
        }

        public static bool DashPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var kb = Keyboard.current;
                return kb != null && kb.zKey.wasPressedThisFrame;
#else
                return Input.GetKeyDown(KeyCode.Z);
#endif
            }
        }

        public static bool ShootHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var kb = Keyboard.current;
                return kb != null && kb.xKey.isPressed;
#else
                return Input.GetKey(KeyCode.X);
#endif
            }
        }

        public static bool RestartPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var kb = Keyboard.current;
                return kb != null && kb.rKey.wasPressedThisFrame;
#else
                return Input.GetKeyDown(KeyCode.R);
#endif
            }
        }
    }
}
