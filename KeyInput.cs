using System;
using System.Linq;
using UnityEngine;

namespace KSPBreakToggle
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class KeyInput : MonoBehaviour
	{
		private KeyCode[] _keyCodes;
		public static KeyInput input;
		public void Awake()
		{
			input = this;
			_keyCodes = Enum.GetValues(typeof (KeyCode)).Cast<KeyCode>().Where(c => c != KeyCode.None).ToArray();
		}
		public void Start()
		{
			DontDestroyOnLoad(this);
		}
		public KeyCode FirstPressedKey()
		{
			return _keyCodes.FirstOrDefault(Input.GetKeyDown);
		}
		public static bool IsMouse(KeyCode key)
		{
			return key.ToString().StartsWith("Mouse");
		}
	}
}