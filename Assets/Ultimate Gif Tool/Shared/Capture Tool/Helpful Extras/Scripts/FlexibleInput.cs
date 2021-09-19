//#define TRS_FORCE_LEGACY_INPUT
// Uncomment the above line to force use of legacy input.
// Note: You may want to define this in your own script as this file will be
// automatically overwritten in the next update.

using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
using UnityEngine.InputSystem;
#endif

public class FlexibleInput : MonoBehaviour
{
    public static bool AnyKeyDown()
    {
#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
    return Keyboard.current.anyKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.anyKeyDown;
#else
    return false;
#endif
    }

    public static bool LeftMouseButton()
    {
#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
        return Mouse.current.leftButton.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButton(0);
#else
    return false;
#endif
    }

    public static Vector3 MousePosition()
    {
#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
        return Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
    return Vector3.zero;
#endif
    }
}
