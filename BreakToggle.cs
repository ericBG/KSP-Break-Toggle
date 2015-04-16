using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KSPBreakToggle
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class BreakToggle : MonoBehaviour
	{
		private static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
		private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);
		private static readonly string ConfigPath = Path.Combine(AssemblyDirectory, "key.cfg");
		private Rect _mainWindowPos;
		private Rect _modalWindowPos;
		private KeyBinding _binding;
		private bool _showWindow;
		private Action<KeyCode> _currentSetAction;
		private const int MainWindowID = int.MaxValue;
		private const int ModalWindowID = int.MaxValue - 1;
		private bool _drawingModalWindow;
		public void Start()
		{
			try
			{
			Logger.DebugLog("Start()", "BreakToggle");
			FlightUIController.fetch.brakes.OnPress += OnPress;
			_mainWindowPos = new Rect(Screen.width*.66f, 0f, 0f, 0f);
			_modalWindowPos = new Rect(Screen.width*.25f, Screen.height*.46f, Screen.width*.5f, Screen.height*.08f);
			}
			catch (Exception e)
			{
				Logger.DebugLogError(e, "BreakToggle");
				throw;
			}
			if (_binding == null) _binding = new KeyBinding();
		}
		public void Awake()
		{
			Logger.DebugLog("Awake()", "BreakToggle");
			if (!File.Exists(ConfigPath))
			{
				File.Create(ConfigPath);
				Logger.DebugLogWarning("Config file not found. File created.", "BreakToggle");
				return;
			}
			string[] lines = File.ReadAllLines(ConfigPath);
			if (!lines.Any())
			{
				Logger.DebugLogWarning("Config file found, but empty. Skipping file.", "BreakToggle");
				return;
			}
			string[] codes = lines[0].Split(',');
			if (codes.Length != 2)
			{
				Logger.DebugLogWarning("File found, with contents. Contents not the right length. Skipping file.", "BreakToggle");
				return;
			}
			int primary;
			if (int.TryParse(codes[0], out primary) && Enum.IsDefined(typeof (KeyCode), primary))
			{
				int secondary;
				if (!(int.TryParse(codes[1], out secondary) || Enum.IsDefined(typeof (KeyCode), secondary)))
					Logger.DebugLogWarning("Second value found, but not an integer/not part of the KeyCode enum. Skipping file",
						"BreakToggle");
				else
				{
					_binding = new KeyBinding((KeyCode) primary, (KeyCode) secondary);
					Logger.DebugLogWarning("KeyBinding succesfully parsed from config file.", "BreakToggle");
				}
			}
			else
				Logger.DebugLogWarning("First value found, but not an integer/not part of the KeyCode enum. Skipping file", "BreakToggle");
		}
		private void OnPress()
		{
			Logger.DebugLog("OnPress()", "BreakToggle");
			if (!(Input.GetMouseButton(1) || Input.GetMouseButtonUp(1))) return;
			Logger.DebugLog("Right Click Detected!", "BreakToggle");
			_showWindow = !_showWindow;
			if (_showWindow)
				RenderingManager.AddToPostDrawQueue(0, OnMainWindow);
			else
				RenderingManager.RemoveFromPostDrawQueue(0, OnMainWindow);
		}
		private void OnMainWindow()
		{
			GUI.skin = HighLogic.Skin;
			_mainWindowPos = GUILayout.Window(MainWindowID, _mainWindowPos, OnMainWindowDraw, "Break Toggle Options");
			GUI.skin = null;
		}
		public void Update()
		{
			if (_drawingModalWindow)
			{
				KeyCode key = KeyInput.input.FirstPressedKey();
				if (key != KeyCode.None && !KeyInput.IsMouse(key)) ActionAndRemove(() => _currentSetAction(key));
			}
			else
			{
				if (_binding == null) return;
				if (_binding.GetKeyDown()) FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
			}
		}
		private void Save()
		{
			File.Delete(ConfigPath);
			File.WriteAllLines(ConfigPath, new[] {string.Format("{0},{1}", (int)_binding.primary, (int)_binding.secondary)});
			Logger.DebugLog(string.Format("Should've saved file with \"{0},{1}\"", (int)_binding.primary, (int)_binding.secondary), "BreakToggle");
		}
		private void OnMainWindowDraw(int id)
		{
			GUILayout.BeginHorizontal(GUILayout.Width(250f));
			GUILayout.Label("Break (Toggle): ");
			if (GUILayout.Button(_binding.primary == KeyCode.None ? "" : _binding.primary.ToString()))
				ModalWindow(k => _binding.primary = k);
			if (GUILayout.Button(_binding.secondary == KeyCode.None ? "" : _binding.secondary.ToString()))
				ModalWindow(k => _binding.secondary = k);
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}
		private void ModalWindow(Action<KeyCode> keyCodeSwitcher)
		{
			_drawingModalWindow = true;
			_currentSetAction = keyCodeSwitcher;
			RenderingManager.AddToPostDrawQueue(1, OnModalWindow);
		}
		private void OnModalWindow()
		{
			GUI.skin = HighLogic.Skin;
			_modalWindowPos = GUI.ModalWindow(ModalWindowID, _modalWindowPos, OnModalWindowDraw, "Press the key or joystick button to use for this action!");
			GUI.skin = null;
		}
		private void OnModalWindowDraw(int id)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Clear Binding")) ActionAndRemove(() => _currentSetAction(KeyCode.None));
			if (GUILayout.Button("Cancel Binding")) ActionAndRemove(() => { });
			GUILayout.EndHorizontal();
		}
		private void ActionAndRemove(Action toDo)
		{
			_drawingModalWindow = false;
			toDo();
			Save();
			RenderingManager.RemoveFromPostDrawQueue(1, OnModalWindow);
		}
	}
}
