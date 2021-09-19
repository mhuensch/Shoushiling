﻿using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
using UnityEngine.InputSystem;
#endif

namespace TRS.CaptureTool.Extras
{
    [System.Serializable]
    public struct HotKeySet
    {
        public bool shift;
        [UnityEngine.Serialization.FormerlySerializedAs("cntrl")]
        public bool ctrl;
        public bool alt;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        public bool cmd;
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		public bool win;
#endif


#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
        public Key key;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode keyCode;
#endif

        public bool MatchesInput()
        {
#if ENABLE_INPUT_SYSTEM && !TRS_FORCE_LEGACY_INPUT
            return Keyboard.current[key].wasPressedThisFrame &&
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                        (!cmd || Keyboard.current.leftCommandKey.isPressed || Keyboard.current.rightCommandKey.isPressed) &&
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				        (!win || Keyboard.current.leftWindowsKey.isPressed || Keyboard.current.rightWindowsKey.isPressed) &&
#endif
                        (!shift || Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed) &&
                        (!ctrl || Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed) &&
                        (!alt || Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode) &&
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                        (!cmd || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) &&
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				        (!win || Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows)) &&
#endif
                        (!shift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                        (!ctrl || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                        (!alt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
#else
            return false;
#endif
        }

        public bool MatchesEvent()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Event.current.keyCode == keyCode &&
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                        (!cmd || Event.current.command) &&
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				        (!win || Event.current.command) &&
#endif
                        (!shift || Event.current.shift) &&
                        (!ctrl || Event.current.control) &&
                        (!alt || Event.current.alt);
#else
            return false;
#endif
        }
    }
}