using System.Globalization;
using UnityEngine;

namespace KSPBreakToggle
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class Logger : MonoBehaviour
	{
		public void Awake()
		{
			DontDestroyOnLoad(this);
			DebugLog("Awake()", "Logger");
		}
		public static void DebugLog(object message, string partOfCode)
		{
			Debug.Log(string.Format("[KSP Break Toggle ({0})] {1}", partOfCode, message));
		}
		public static void DebugLogWarning(object message, string partOfCode)
		{
			Debug.LogWarning(string.Format("[KSP Break Toggle ({0})] {1}", partOfCode, message));
		}
		public static void DebugLogError(object message, string partOfCode)
		{
			Debug.LogError(string.Format("[KSP Break Toggle ({0})] {1}", partOfCode, message));
		}
	}
}