using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

#if SANDBOX
using System.Text.Json.Serialization;
#endif

#if UNITY_EDITOR
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
#endif

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		public const string fileName = "actions.json";

		public const string filePath = "ReAction/" + fileName;

		public const string defaultFileName = "defaultActions.json";

		public const string defaultFilePath = "ReAction/" + defaultFileName;

		/// <summary>
		/// Formats the modifiers' strings, like the following: <code>LShift + LMeta + </code>
		/// </summary>
		/// <param name="modifiers"></param>
		/// <returns></returns>
		public static string FormatModifiersString(ReAction.Modifiers modifiers)
		{
			string str = "";

			for (byte b = 1; b > 0; b <<= 1)
			{
				if (((byte)modifiers & b) != 0)
				{
					str += (ReAction.Modifiers)b + " + ";
				}
			}

			return str;
		}

		/// <summary>
		/// Like <see cref="Input.AnalogLook"/> but scaled properly
		/// </summary>
		public static Angles Look => m_AnalogLook;

		static Angles m_AnalogLook;

#if !CUSTOM_SAVE_SYSTEM
		internal static void GetSavedActionData()
		{
			#if SANDBOX
			FileSystem.Data.CreateDirectory("ReAction");
			if (FileSystem.Data.FileExists(FileSystem.NormalizeFilename(filePath)))
			{
				ReActionLogger.Info("Found saved local actions, applying");

				var settings = new ReActionSettings();
				settings.Deserialize(FileSystem.Data.ReadAllText(filePath));

				Actions = settings.Actions;
			}
			else if (ProjectSettings.Get<ReActionSettings>("ReAction/defaultActions.json") != null)
			{
				ReActionLogger.Info("No saved actions, using default actions");
				Actions = new(ProjectSettings.Get<ReActionSettings>("ReAction/defaultActions.json").Actions);
			}
			else
			{
				ReActionLogger.Warning("Could not find a default actions file");
			}
			#endif
		}
#endif

		public static HashSet<ReAction.Action> GetDefaultActions()
		{
			HashSet<ReAction.Action> actions = new HashSet<ReAction.Action>();

			foreach (var action in Input.GetActions())
			{
				actions.Add(new Action(action));
			}

			return actions;
		}

		/*{
			new ReAction.Action(
	"Forward",
	0,
#if SANDBOX
	KeyCode.KEY_W
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.W
#endif
				,
	GamepadInput.LeftStickY,
	true,
	Conditional.Continuous,
	"Movement"
  ),
  new ReAction.Action(
	"Backward",
	1,
#if SANDBOX
	KeyCode.KEY_S
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.S
				#endif
	  ,
	GamepadInput.LeftStickY,
	false,
	Conditional.Continuous,
	"Movement"
  ),
  new ReAction.Action(
	"Left",
	2,
#if SANDBOX
	KeyCode.KEY_A
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.A
				#endif
	,
	GamepadInput.LeftStickX,
	true,
	Conditional.Continuous,
	"Movement"
  ),
  new ReAction.Action(
	"Right",
	3,
#if SANDBOX
	KeyCode.KEY_D
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.D
				#endif
	,
	GamepadInput.LeftStickX,
	false,
	Conditional.Continuous,
	"Movement"
  ),
  new ReAction.Action(
	"Jump",
	4,
#if SANDBOX
	KeyCode.KEY_SPACE
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Space
				#endif
	  ,
	GamepadInput.A,
	Conditional.Press,
	"Movement"
  ),
  new ReAction.Action(
	"Run",
	5,
#if SANDBOX
	KeyCode.KEY_LSHIFT
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.LeftShift
				#endif
	  ,
	GamepadInput.LeftJoystickButton,
	Conditional.Press,
	"Movement"
  ),
  new ReAction.Action(
	"Walk",
	6,
#if SANDBOX
	KeyCode.KEY_LALT
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.LeftAlt
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Movement"
  ),
  new ReAction.Action(
	"Duck",
	7,
#if SANDBOX
	KeyCode.KEY_LCONTROL
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.LeftControl
				#endif
	  ,
	GamepadInput.B,
	Conditional.Press,
	"Movement"
  ),
  new ReAction.Action(
	"Attack1",
	8,
#if SANDBOX
	KeyCode.MouseLeft
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Mouse0
				#endif
	  ,
	GamepadInput.RightTrigger,
	Conditional.Press,
	"Actions"
  ),
  new ReAction.Action(
	"Attack2",
	9,
#if SANDBOX
	KeyCode.MouseRight
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Mouse1
				#endif
	  ,
	GamepadInput.LeftTrigger,
	Conditional.Press,
	"Actions"
  ),
  new ReAction.Action(
	"Reload",
	10,
#if SANDBOX
	KeyCode.KEY_R
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.R
				#endif
	  ,
	GamepadInput.X,
	Conditional.Press,
	"Actions"
  ),
  new ReAction.Action(
	"Use",
	11,
#if SANDBOX
	KeyCode.KEY_E
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.E
				#endif
	,
	GamepadInput.Y,
	Conditional.Press,
	"Actions"
  ),
  new ReAction.Action(
	"Slot1",
	12,
#if SANDBOX
	KeyCode.KEY_1
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha1
				#endif
	  ,
	GamepadInput.DpadWest,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot2",
	13,
#if SANDBOX
	KeyCode.KEY_2
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha2
				#endif
	  ,
	GamepadInput.DpadEast,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot3",
	14,
#if SANDBOX
	KeyCode.KEY_3
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha3
				#endif
	,
	GamepadInput.DpadSouth,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot4",
	15,
#if SANDBOX
	KeyCode.KEY_4
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha4
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot5",
	16,
#if SANDBOX
	KeyCode.KEY_5
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha5
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot6",
	17,
#if SANDBOX
	KeyCode.KEY_6
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha6
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot7",
	18,
#if SANDBOX
	KeyCode.KEY_7
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha7
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot8",
	19,
#if SANDBOX
	KeyCode.KEY_8
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha8
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot9",
	20,
#if SANDBOX
	KeyCode.KEY_9
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha9
				#endif
	,
	GamepadInput.None,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"Slot0",
	21,
#if SANDBOX
	KeyCode.KEY_0
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Alpha9
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"SlotPrev",
	22,
#if SANDBOX
	KeyCode.MouseBack
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Mouse3
				#endif
	  ,
	GamepadInput.SwitchLeftBumper,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"SlotNext",
	23,
#if SANDBOX
	KeyCode.MouseForward
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Mouse4
				#endif
	  ,
	GamepadInput.SwitchRightBumper,
	Conditional.Press,
	"Inventory"
  ),
  new ReAction.Action(
	"View",
	24,
#if SANDBOX
	KeyCode.KEY_C
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.C
				#endif
	  ,
	GamepadInput.RightJoystickButton,
	Conditional.Press,
	"Other"
  ),
  new ReAction.Action(
	"Voice",
	25,
#if SANDBOX
	KeyCode.KEY_V
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.V
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Other"
  ),
  new ReAction.Action(
	"Drop",
	26,
#if SANDBOX
	KeyCode.KEY_G
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.G
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Other"
  ),
  new ReAction.Action(
	"Flashlight",
	27,
#if SANDBOX
	KeyCode.KEY_F
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.F
				#endif
	  ,
	GamepadInput.DpadNorth,
	Conditional.Press,
	"Other"
  ),
  new ReAction.Action(
	"Score",
	28,
#if SANDBOX
	KeyCode.KEY_TAB
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Tab
				#endif
	  ,
	GamepadInput.SwitchLeftMenu,
	Conditional.Press,
	"Other"
  ),
  new ReAction.Action(
	"Menu",
	29,
#if SANDBOX
	KeyCode.KEY_Q
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Q
				#endif
	  ,
	GamepadInput.SwitchRightMenu,
	Conditional.Press,
	"Other"
  ),
  new ReAction.Action(
	"Chat",
	30,
#if SANDBOX
	KeyCode.KEY_ENTER
#elif UNITY_STANDALONE || UNITYEDITOR
	KeyCode.Return
				#endif
	  ,
	GamepadInput.None,
	Conditional.Press,
	"Other"
  )
		};*/

		static double lastTime;

		static Modifiers currentlyActivatedModifiers;

		public static bool IsLShiftDown => (currentlyActivatedModifiers & Modifiers.LShift) != 0;
		public static bool IsLCtrlDown => (currentlyActivatedModifiers & Modifiers.LCtrl) != 0;
		public static bool IsLMetaDown => (currentlyActivatedModifiers & Modifiers.LMeta) != 0;
		public static bool IsLAltDown => (currentlyActivatedModifiers & Modifiers.LAlt) != 0;
		public static bool IsRAltDown => (currentlyActivatedModifiers & Modifiers.RAlt) != 0;
		public static bool IsRMetaDown => (currentlyActivatedModifiers & Modifiers.RMeta) != 0;
		public static bool IsRShiftDown => (currentlyActivatedModifiers & Modifiers.RShift) != 0;
		public static bool IsRCtrlDown => (currentlyActivatedModifiers & Modifiers.RCtrl) != 0;


#if SANDBOX
		[ConVar(Name = "ReAction_debug", Flags = ConVarFlags.Saved, Saved = true, Max = 1, Min = 0, Help = "Enable ReAction debug overlay")]
#endif
		public static bool DebugInput { get; internal set; } = false;

		[ConCmd(Name = "print_reaction_actions", Help = "Prints all ReAction actions to the console for debugging")]
		static void PrintActionsToConsole()
		{
			foreach (var action in Actions)
			{
				ReActionLogger.QuickInfo(nameof(action.InputAction.Name), action.InputAction.Name);
				ReActionLogger.QuickInfo(nameof(action.InputAction.KeyboardCode), (action.InputAction.KeyboardCode.Equals(default) ? "Unbound" : $"{FormatModifiersString(action.Modifiers)}{action.InputAction.KeyboardCode}"));
				ReActionLogger.QuickInfo(nameof(action.Conditional), action.Conditional);
				ReActionLogger.QuickInfo(nameof(action.InputAction.GroupName), action.InputAction.GroupName);
				ReActionLogger.QuickInfo(nameof(action.InputAction.Title), action.InputAction.Title);
			}
		}

		[System.Flags]
		public enum Modifiers : byte
		{
			None = 0,
			LShift = 1 << (8 - 1),
			LCtrl = 1 << (8 - 2),
			LMeta = 1 << (8 - 3),
			LAlt = 1 << (8 - 4),
			RAlt = 1 << (8 - 5),
			RMeta = 1 << (8 - 6),
			RShift = 1 << (8 - 7),
			RCtrl = 1 << (8 - 8),
		}

		static void QueryModifiersIfNeeded()
		{
			if (
#if SANDBOX
				Time.Now
#elif UNITY_STANDALONE || UNITYEDITOR
				Time.time
#endif
				== lastTime)
				return;

			currentlyActivatedModifiers =
				(Modifiers)
				(((byte)
#if SANDBOX
				KeyDown(KeyCode.KEY_LSHIFT)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.LeftShift)
#endif
				.GetHashCode() << 7)  | //this is the same as Convert.ToByte() btw, I refuse to use it, especially when a bool is literally just a byte!!!! let me fucking use it!!!!!!!!!!!!!!!! fuck you
				((byte)

#if SANDBOX
				KeyDown(KeyCode.KEY_LCONTROL)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.LeftControl)
# endif
				.GetHashCode() << 6) |
				((byte)
#if SANDBOX
				KeyDown(KeyCode.KEY_LWIN)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.LeftMeta)
#endif
				.GetHashCode() << 5)     |
				((byte)
#if SANDBOX
				KeyDown(KeyCode.KEY_LALT)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.LeftAlt)
#endif
				.GetHashCode() << 4)     |
				((byte)
#if SANDBOX
				KeyDown(KeyCode.KEY_RALT)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.RightAlt)
#endif
				.GetHashCode() << 3)     |
				((byte)

#if SANDBOX
				KeyDown(KeyCode.KEY_RWIN)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.RightMeta)
# endif
				.GetHashCode() << 2)     |
				((byte)
#if SANDBOX
				KeyDown(KeyCode.KEY_RSHIFT)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.RightShift)
#endif
				.GetHashCode() << 1)   |
				((byte)
#if SANDBOX
				KeyDown(KeyCode.KEY_RCONTROL)
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey(KeyCode.RightControl)
# endif
				.GetHashCode()));
		}

		public static HashSet<Action> Actions
		{
			get;
			set;
		} = new();

		/// <summary>
		/// How long a <see cref="Conditional.LongPress"/> key needs to be held for before activation
		/// </summary>
		public static float LongPressTimeOut
		{
			get;
			set;
		} = .5f;

		/// <summary>
		/// How quickly a <see cref="Conditional.DoubleTap"/> key can be up for, before not being valid for activation
		/// </summary>
		public static float DoubleTapTimeOut
		{
			get;
			set;
		} = .4f;

		public static float MashTimeOut
		{
			get;
			set;
		} = .3f;

		static Dictionary<string, double> keysPressedSince = new();

		static Dictionary<string, double> keysHeldSince = new();

		static Dictionary<string, double> keysUpSince = new();

		static HashSet<string> validDoubleTapKeyCodes = new();

		static HashSet<string> toggledKeyCodes = new();

		static HashSet<string> mashedKeyCodes = new();

		static HashSet<string> validLongPressKeyCodes = new();
		static HashSet<string> oldLongPressKeyCodes = new();

		/*/// <summary>
		/// Like <see cref="Input.AnalogMove"/> but kinda better, and also with ClampLength(1f) applied to it
		/// </summary>
		public static Vector3 AnalogMove
		{
			get
			{
				Vector3 analogMove =
#if SANDBOX
					Vector3.Zero
#elif UNITY_STANDALONE || UNITYEDITOR
					Vector3.zero
				#endif
					;

				if (ReAction.ActionTriggered("Forward", false))
				{
					analogMove +=
#if SANDBOX
						Vector3.Forward
#elif UNITY_STANDALONE || UNITYEDITOR
						Vector3.forward
# endif
						;
				}
				if (ReAction.ActionTriggered("Backward", false))
				{
					analogMove +=
#if SANDBOX
						Vector3.Backward
#elif UNITY_STANDALONE || UNITYEDITOR
						Vector3.back
# endif
						;
				}
				if (ReAction.ActionTriggered("Left", false))
				{
					analogMove += 
#if SANDBOX
					Vector3.Left
#elif UNITY_STANDALONE || UNITYEDITOR
						Vector3.left
#endif
						;
				}
				if (ReAction.ActionTriggered("Right", false))
				{
					analogMove += 
#if SANDBOX
						Vector3.Right
#elif UNITY_STANDALONE || UNITYEDITOR
						Vector3.right
#endif
						;
				}

				float moveX = 0f, moveY = 0f;

				if (ReAction.TryGetAction("Forward", out var forward) && forward.GamepadInput != GamepadInput.None)
				{
					moveX = forward.Analog;
				}
				if (ReAction.TryGetAction("Left", out var left) && left.GamepadInput != GamepadInput.None)
				{
					moveY = left.Analog;
				}

				analogMove += new Vector3(moveY, moveX, 0f);

#if SANDBOX
				return analogMove.ClampLength(1f);
#elif UNITY_STANDALONE || UNITYEDITOR
				return Vector3.ClampMagnitude(analogMove, 1f);
				#endif

			}
		}*/

		/// <summary>
		/// This is kind of the Awake() method for this, probably don't need to call it, the <see cref="ReActionSystem"/> should take care of it
		/// </summary>
		public static void Init()
		{
			ReActionLogger.Info("Initialising ReAction");

			GetSavedActionData();

			AssignInputActions();
		}

		static void AssignInputActions()
		{
			foreach (var action in Actions)
			{
				var matchingAction = ProjectSettings.Input.Actions.Where(e => e.Name == action.InputAction.Name).FirstOrDefault();

				if (matchingAction != null)
				{
					action.InputAction = matchingAction;
				}
				else
				{
					ReActionLogger.Warning($"Found missing action for {action.InputAction.Name}");
				}
			}
		}

		/// <summary>
		/// This is where the Input gets queried, view this as the Update() method for ReAction, you probably don't need to call it, the <see cref="ReActionSystem"/> should take care of it
		/// </summary>
		public static void QueryInput()
		{
			ProcessAnalogLook();

			QueryModifiersIfNeeded();

			PopulateActionsLists();

			RemoveTimedOutKeysFromLists();

			lastTime =
#if SANDBOX
					RealTime.GlobalNow 
#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
#endif
					;

			DrawDebugOverlay();
		}

		static void ProcessAnalogLook()
		{
			m_AnalogLook = default;

			if (!Input.MouseCursorVisible)
			{
				//Check if player is using controller to look
				if (Input.GetAnalog(InputAnalog.RightStickX) == 0 && Input.GetAnalog(InputAnalog.RightStickY) == 0)
				{
					m_AnalogLook = new Angles(Input.MouseDelta.y * Preferences.Sensitivity, -Input.MouseDelta.x * Preferences.Sensitivity, 0f);

					if (Preferences.InvertMousePitch)
					{
						m_AnalogLook.pitch = -m_AnalogLook.pitch;
					}

					if (Preferences.InvertMouseYaw)
					{
						m_AnalogLook.yaw = -m_AnalogLook.yaw;
					}
				}
				else
				{
					m_AnalogLook = new Angles(Input.GetAnalog(InputAnalog.RightStickY) * Time.Delta * Preferences.ControllerLookPitchSpeed, -(Input.GetAnalog(InputAnalog.RightStickX) * Time.Delta * Preferences.ControllerLookYawSpeed), 0f);
				}
			}

		}

		static void PopulateActionsLists()
		{
			validDoubleTapKeyCodes.Clear();
			mashedKeyCodes.Clear();
			validLongPressKeyCodes.Clear();

			foreach (var key in keysHeldSince)
			{
				if (!oldLongPressKeyCodes.Contains(key.Key) && RealTime.GlobalNow - key.Value > LongPressTimeOut)
				{
					validLongPressKeyCodes.Add(key.Key);
					oldLongPressKeyCodes.Add(key.Key);
				}
			}

			//could I replace these with async calls? would it matter?
			foreach (var action in Actions)
			{
				/*
				var key = action.Bind.key;
				var secondaryKey = action.SecondaryBind.key;

				if (
#if SANDBOX
					ReAction.KeyPressed
#elif UNITY_STANDALONE || UNITYEDITOR
					Input.GetKeyDown
#endif
					(key))
				{
					OnKeyPressed(key);
				}

				if (
#if SANDBOX
					ReAction.KeyPressed
#elif UNITY_STANDALONE || UNITYEDITOR
					Input.GetKeyDown
# endif
					(secondaryKey))
				{
					OnKeyPressed(secondaryKey);
				}*/

				var key = action.InputAction.Name;

				if (Input.Pressed(key))
				{
					OnKeyPressed(key);
				}

				if (Input.Released(key))
				{
					if (keysUpSince.TryGetValue(key, out var time))
					{
						keysUpSince.Remove(key);

						if (
#if SANDBOX
						RealTime.GlobalNow
#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
#endif
						- time <= DoubleTapTimeOut && time !=
#if SANDBOX
						RealTime.GlobalNow
#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
#endif
						)
						{
							validDoubleTapKeyCodes.Add(key);
						}
					}
					else
					{
						keysUpSince.Add(key,
#if SANDBOX
						RealTime.GlobalNow
#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
#endif
						);
					}

					oldLongPressKeyCodes.Remove(key);
				}
			}

			static void OnKeyPressed(string key)
			{
				if (!toggledKeyCodes.Add(key))
				{
					toggledKeyCodes.Remove(key);
				}

				if (!keysHeldSince.ContainsKey(key))
				{
					keysHeldSince.Add(key,
#if SANDBOX
					RealTime.GlobalNow
#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
#endif
					);
				}

				if (!keysPressedSince.ContainsKey(key))
				{
					keysPressedSince.Add(key,
#if SANDBOX
					RealTime.GlobalNow
#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
#endif
					);
				}
				else
				{
					keysPressedSince[key] =
#if SANDBOX
					RealTime.GlobalNow
#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
#endif
					;
					mashedKeyCodes.Add(key);
				}
			}
		}

		static void RemoveTimedOutKeysFromLists()
		{
			//prevent collection size from getting modified while enumerating
			var keysHeldSinceLocal = new Dictionary<string, double>(keysHeldSince);

			foreach (var key in keysHeldSince)
			{
				if (!
#if SANDBOX
					Input.Down
#elif UNITY_STANDALONE || UNITYEDITOR
					Input.GetKey
#endif
					(key.Key))
				{
					keysHeldSinceLocal.Remove(key.Key);
				}
			}

			keysHeldSince = keysHeldSinceLocal;

			//ditto
			var keysUpSinceLocal = new Dictionary<string, double>(keysUpSince);

			foreach (var key in keysUpSince)
			{
				if (
				#if SANDBOX
					RealTime.GlobalNow 
				#elif UNITY_STANDALONE || UNITYEDITOR
					Time.time
				#endif
					- key.Value > DoubleTapTimeOut) //wow!!!! key is above the max timeout threshold, and the key isn't being held, yeet that shit
				{
					keysUpSinceLocal.Remove(key.Key);
				}
			}

			keysUpSince = keysUpSinceLocal;

			//ditto ditto
			var keysPressedSinceLocal = new Dictionary<string, double>(keysPressedSince);

			foreach (var key in keysPressedSince)
			{
				if (
					#if SANDBOX 
					RealTime.GlobalNow 
					#elif UNITY_STANDALONE || UNITYEDITOR 
					Time.time
					#endif
					- key.Value > MashTimeOut)
				{
					keysPressedSinceLocal.Remove(key.Key);
				}
			}

			keysPressedSince = keysPressedSinceLocal;
		}

		static void DrawDebugOverlay()
		{
			if (DebugInput && Game.InGame && !Game.IsPaused)
			{
#if SANDBOX
				if (Game.ActiveScene != null && Game.ActiveScene.Camera != null)
				{
					var pixels = new Vector2(Screen.Width, Screen.Height);

					var hud = Game.ActiveScene.Camera.Hud;
					hud.DrawText(new TextRendering.Scope("ReAction state:", Color.Yellow, 32), new Rect(0, 0, pixels.x, pixels.y), TextFlag.LeftTop);
					hud.DrawText(new TextRendering.Scope($"Modifiers: {ReAction.currentlyActivatedModifiers.ToString()}", Color.Yellow, 32), new Rect(0, pixels.y * 0.05f, pixels.x, pixels.y), TextFlag.LeftTop);

					if (validDoubleTapKeyCodes.Count > 0)
					{
						hud.DrawText(new TextRendering.Scope($"Valid double taps: {string.Join(", ", validDoubleTapKeyCodes)}", Color.Yellow, 32), new Rect(0, pixels.y * 0.1f, pixels.x, pixels.y), TextFlag.LeftTop);
					}

					if (mashedKeyCodes.Count > 0)
					{
						hud.DrawText(new TextRendering.Scope($"Valid mashes: {string.Join(", ", mashedKeyCodes)}", Color.Yellow, 32), new Rect(0, pixels.y * 0.15f, pixels.x, pixels.y), TextFlag.LeftTop);
					}

					if (validLongPressKeyCodes.Count > 0)
					{
						hud.DrawText(new TextRendering.Scope($"Valid long presses: {string.Join(", ", validLongPressKeyCodes)}", Color.Yellow, 32), new Rect(0, pixels.y * 0.2f, pixels.x, pixels.y), TextFlag.LeftTop);
					}

					if (oldLongPressKeyCodes.Count > 0)
					{
						hud.DrawText(new TextRendering.Scope($"Old long presses: {string.Join(", ", oldLongPressKeyCodes)}", Color.Red, 32), new Rect(0, pixels.y * 0.25f, pixels.x, pixels.y), TextFlag.Top);
					}

					hud.DrawText(new TextRendering.Scope("Current up keys:", Color.Red, 16), new Rect(pixels.x * .6f, 0, pixels.x, pixels.y), TextFlag.LeftTop);
					int upKeycount = 1;
					foreach (var key in keysUpSince)
					{
						hud.DrawText(new TextRendering.Scope($"{key.Key}: {Time.Now - key.Value}", Color.Red, 16), new Rect(pixels.x * .6f, (pixels.y * 0.025f) * upKeycount, pixels.x, pixels.y), TextFlag.LeftTop);
						upKeycount++;
					}

					hud.DrawText(new TextRendering.Scope("Current held keys:", Color.Green, 16), new Rect(pixels.x * .74f, 0, pixels.x, pixels.y), TextFlag.LeftTop);
					upKeycount = 1;
					foreach (var key in keysHeldSince)
					{
						hud.DrawText(new TextRendering.Scope($"{key.Key}: {Time.Now - key.Value}", Color.Green, 16), new Rect(pixels.x * .74f, (pixels.y * 0.025f) * upKeycount, pixels.x, pixels.y), TextFlag.LeftTop);
						upKeycount++;
					}

					hud.DrawText(new TextRendering.Scope("Current pressed keys:", Color.Blue, 16), new Rect(pixels.x * .88f, 0, pixels.x, pixels.y), TextFlag.LeftTop);
					upKeycount = 1;
					foreach (var key in keysPressedSince)
					{
						hud.DrawText(new TextRendering.Scope($"{key.Key}: {Time.Now - key.Value}", Color.Blue, 16), new Rect(pixels.x * .88f, (pixels.y * 0.025f) * upKeycount, pixels.x, pixels.y), TextFlag.LeftTop);
						upKeycount++;
					}
				}
#endif
			}
		}

		/*public static bool TryGetAction(int index, out Action action)
		{
			return (action = GetAction(index)) != null;
		}

		public static Action GetAction(int index)
		{
			foreach (var action in Actions)
			{
				if (action.Index == index)
				{
					return action;
				}
			}

			return null;
		}*/

		public static bool TryGetAction(string name, out Action action)
		{
			return (action = GetAction(name)) != null;
		}

		public static Action GetAction(string name)
		{
			foreach (var action in Actions)
			{
				if (action.InputAction.Name == name)
				{
					return action;
				}
			}

			return null;
		}

		/*/// <summary>
		/// Is the action currently triggered?
		/// </summary>
		/// <param name="index">
		/// The index of the action, pro tip, this is what exporting the consts are for
		/// </param>
		/// <param name="complainOnMissing">
		/// Will log a warning in the console if the action is missing
		/// </param>
		/// <returns>
		/// If the action is currently triggered
		/// </returns>
		public static bool ActionTriggered(int index, bool complainOnMissing = true)
		{
			if (TryGetAction(index, out var action))
			{
				return ActionTriggered(action);
			}

			if (complainOnMissing)
				ReActionLogger.Warning($"Could not find Action with index {index}");

			return false;
		}*/

		/// <summary>
		/// Is the action triggered?
		/// </summary>
		/// <param name="name">
		/// The name of the action
		/// </param>
		/// <param name="complainOnMissing">
		/// Will log a warning in the console if the action is missing
		/// </param>
		/// <returns>
		/// If the action is currently triggered
		/// </returns>
		public static bool ActionTriggered(string name, bool complainOnMissing = true)
		{
			if (TryGetAction(name, out var action))
			{
				return ActionTriggered(action);
			}

			if (complainOnMissing)
				ReActionLogger.Warning($"Could not find Action with name {name}");

			return false;
		}

		/// <summary>
		/// Is the action triggered?
		/// </summary>
		/// <param name="action">
		/// If you want you can try caching actions and use this
		/// </param>
		/// <returns>
		/// If the action is currently triggered
		/// </returns>
		public static bool ActionTriggered(Action action)
		{
			return ActionTriggeredForCondition(action);
		}

		static bool ActionTriggeredForCondition(Action action)
		{
			return ActionTriggeredForCondition(action, action.Conditional);
		}

		/// <summary>
		/// Allows you to check if an action is valid with the specified conditional
		/// </summary>
		/// <param name="actionName">
		/// The name of the action whose key you want to check (tip: export as const)
		/// </param>
		/// <param name="conditional">
		/// The condition you want to check
		/// </param>
		/// <returns>
		/// If the conditions for this action are valid
		/// </returns>
		public static bool ActionTriggeredForCondition(string actionName, Conditional conditional)
		{
			return TryGetAction(actionName, out var action) && ActionTriggeredForCondition(action, conditional);
		}

		/*/// <summary>
		/// Allows you to check if an action is valid with the specified conditional
		/// </summary>
		/// <param name="index">
		/// The index of the action whose key you want to check (tip: export as const) 
		/// </param>
		/// <param name="conditional">
		/// The condition you want to check
		/// </param>
		/// <returns>
		/// If the conditions for this action are valid
		/// </returns>
		public static bool ActionTriggeredForCondition(int index, Conditional conditional)
		{
			return TryGetAction(index, out var action) && ActionTriggeredForCondition(action, conditional);
		}*/

		/// <summary>
		/// Allows you to check if an action is valid with the specified conditional
		/// </summary>
		/// <param name="action">
		/// The actions whose key you want to check
		/// </param>
		/// <param name="conditional">
		/// The condition you want to check
		/// </param>
		/// <returns>
		/// If the conditions for this action are valid
		/// </returns>
		public static bool ActionTriggeredForCondition(Action action, Conditional conditional)
		{
			return conditional switch
			{
				Conditional.Press => (
#if SANDBOX
				Input.Pressed
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKeyDown
#endif
				(action.InputAction.Name) && ModifiersActive(action.Modifiers)),
				Conditional.LongPress => validLongPressKeyCodes.Contains(action.InputAction.Name) && ModifiersActive(action.Modifiers),//I guess I could have a nullref somehow if there's a race condition with the component's updates?
				Conditional.Continuous => (
#if SANDBOX
				Input.Down
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKey
#endif
				(action.InputAction.Name) && ModifiersActive(action.Modifiers)),
				Conditional.Release => (
#if SANDBOX
				Input.Released
#elif UNITY_STANDALONE || UNITYEDITOR
				Input.GetKeyUp
#endif
				(action.InputAction.Name) && ModifiersActive(action.Modifiers)),
				Conditional.DoubleTap => (validDoubleTapKeyCodes.Contains(action.InputAction.Name) && ModifiersActive(action.Modifiers)),
				Conditional.Toggle => (toggledKeyCodes.Contains(action.InputAction.Name) && ModifiersActive(action.Modifiers)),
				Conditional.Mash => (mashedKeyCodes.Contains(action.InputAction.Name) && ModifiersActive(action.Modifiers)),
				_ => false,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ModifiersActive(Modifiers modifiers)
		{
			return (modifiers == 0) || ((modifiers & currentlyActivatedModifiers) != 0);
		}

#if SANDBOX
		/// <summary>
		/// Like <see cref="Input.Keyboard.Down(string)"/> but you don't have to guess the key name
		/// </summary>
		/// <returns>
		/// If the key is being held
		/// </returns>
		public static bool KeyDown(KeyCode key)
		{
			return Input.Keyboard.Down(keyCodeToString[key]);
		}

		/// <summary>
		/// Like <see cref="Input.Keyboard.Pressed(string)"/> but you don't have to guess the key name
		/// </summary>
		/// <param name="key"></param>
		/// <returns>
		/// If the key was just pressed during this frame
		/// </returns>
		public static bool KeyPressed(KeyCode key)
		{
			return Input.Keyboard.Pressed(keyCodeToString[key]);
		}

		/// <summary>
		/// Like <see cref="Input.Keyboard.Released(string)"/> but you don't have to guess the key name
		/// </summary>
		/// <param name="key"></param>
		/// <returns>
		/// If the key was just released during this frame
		/// </returns>
		public static bool KeyReleased(KeyCode key)
		{
			return Input.Keyboard.Released(keyCodeToString[key]);
		}

		public static Dictionary<KeyCode, string> keyCodeToString = new Dictionary<KeyCode, string>()
		{
			{ KeyCode.KEY_NONE, "" },
			{ KeyCode.KEY_0, "0" },
			{ KeyCode.KEY_1, "1" },
			{ KeyCode.KEY_2, "2" },
			{ KeyCode.KEY_3, "3" },
			{ KeyCode.KEY_4, "4" },
			{ KeyCode.KEY_5, "5" },
			{ KeyCode.KEY_6, "6" },
			{ KeyCode.KEY_7, "7" },
			{ KeyCode.KEY_8, "8" },
			{ KeyCode.KEY_9, "9" },
			{ KeyCode.KEY_A, "a" },
			{ KeyCode.KEY_B, "b" },
			{ KeyCode.KEY_C, "c" },
			{ KeyCode.KEY_D, "d" },
			{ KeyCode.KEY_E, "e" },
			{ KeyCode.KEY_F, "f" },
			{ KeyCode.KEY_G, "g" },
			{ KeyCode.KEY_H, "h" },
			{ KeyCode.KEY_I, "i" },
			{ KeyCode.KEY_J, "j" },
			{ KeyCode.KEY_K, "k" },
			{ KeyCode.KEY_L, "l" },
			{ KeyCode.KEY_M, "m" },
			{ KeyCode.KEY_N, "n" },
			{ KeyCode.KEY_O, "o" },
			{ KeyCode.KEY_P, "p" },
			{ KeyCode.KEY_Q, "q" },
			{ KeyCode.KEY_R, "r" },
			{ KeyCode.KEY_S, "s" },
			{ KeyCode.KEY_T, "t" },
			{ KeyCode.KEY_U, "u" },
			{ KeyCode.KEY_V, "v" },
			{ KeyCode.KEY_W, "w" },
			{ KeyCode.KEY_X, "x" },
			{ KeyCode.KEY_Y, "y" },
			{ KeyCode.KEY_Z, "z" },
			{ KeyCode.KEY_PAD_0, "KP_0" },
			{ KeyCode.KEY_PAD_1, "KP_1" },
			{ KeyCode.KEY_PAD_2, "KP_2" },
			{ KeyCode.KEY_PAD_3, "KP_3" },
			{ KeyCode.KEY_PAD_4, "KP_4" },
			{ KeyCode.KEY_PAD_5, "KP_5" },
			{ KeyCode.KEY_PAD_6, "KP_6" },
			{ KeyCode.KEY_PAD_7, "KP_7" },
			{ KeyCode.KEY_PAD_8, "KP_8" },
			{ KeyCode.KEY_PAD_9, "KP_9" },
			{ KeyCode.KEY_PAD_DIVIDE, "KP_DIVIDE" },
			{ KeyCode.KEY_PAD_MULTIPLY, "KP_MULTIPLY" },
			{ KeyCode.KEY_PAD_MINUS, "KP_MINUS" },
			{ KeyCode.KEY_PAD_PLUS, "KP_PLUS" },
			{ KeyCode.KEY_PAD_ENTER, "KP_ENTER" },
			{ KeyCode.KEY_PAD_DECIMAL, "KP_DEL" },
			{ KeyCode.KEY_LESS, "<" },
			{ KeyCode.KEY_LBRACKET, "[" },
			{ KeyCode.KEY_RBRACKET, "]" },
			{ KeyCode.KEY_SEMICOLON, "SEMICOLON" },
			{ KeyCode.KEY_APOSTROPHE, "'" },
			{ KeyCode.KEY_BACKQUOTE, "`" },
			{ KeyCode.KEY_COMMA, "," },
			{ KeyCode.KEY_PERIOD, "." },
			{ KeyCode.KEY_SLASH, "/" },
			{ KeyCode.KEY_BACKSLASH, "\\" },
			{ KeyCode.KEY_MINUS, "-" },
			{ KeyCode.KEY_EQUAL, "=" },
			{ KeyCode.KEY_ENTER, "ENTER" },
			{ KeyCode.KEY_SPACE, "SPACE" },
			{ KeyCode.KEY_BACKSPACE, "BACKSPACE" },
			{ KeyCode.KEY_TAB, "TAB" },
			{ KeyCode.KEY_CAPSLOCK, "CAPSLOCK" },
			{ KeyCode.KEY_NUMLOCK, "NUMLOCK" },
			{ KeyCode.KEY_ESCAPE, "ESCAPE" },
			{ KeyCode.KEY_SCROLLLOCK, "SCROLLLOCK" },
			{ KeyCode.KEY_INSERT, "INS" },
			{ KeyCode.KEY_DELETE, "DEL" },
			{ KeyCode.KEY_HOME, "HOME" },
			{ KeyCode.KEY_END, "END" },
			{ KeyCode.KEY_PAGEUP, "PGUP" },
			{ KeyCode.KEY_PAGEDOWN, "PGDN" },
			{ KeyCode.KEY_BREAK, "PAUSE" },
			{ KeyCode.KEY_LSHIFT, "SHIFT" },
			{ KeyCode.KEY_RSHIFT, "RSHIFT" },
			{ KeyCode.KEY_LALT, "ALT" },
			{ KeyCode.KEY_RALT, "RALT" },
			{ KeyCode.KEY_LCONTROL, "CTRL" },
			{ KeyCode.KEY_RCONTROL, "RCTRL" },
			{ KeyCode.KEY_LWIN, "LWIN" },
			{ KeyCode.KEY_RWIN, "RWIN" },
			{ KeyCode.KEY_APP, "APP" },
			{ KeyCode.KEY_UP, "UPARROW" },
			{ KeyCode.KEY_LEFT, "LEFTARROW" },
			{ KeyCode.KEY_DOWN, "DOWNARROW" },
			{ KeyCode.KEY_RIGHT, "RIGHTARROW" },
			{ KeyCode.KEY_F1, "F1" },
			{ KeyCode.KEY_F2, "F2" },
			{ KeyCode.KEY_F3, "F3" },
			{ KeyCode.KEY_F4, "F4" },
			{ KeyCode.KEY_F5, "F5" },
			{ KeyCode.KEY_F6, "F6" },
			{ KeyCode.KEY_F7, "F7" },
			{ KeyCode.KEY_F8, "F8" },
			{ KeyCode.KEY_F9, "F9" },
			{ KeyCode.KEY_F10, "F10" },
			{ KeyCode.KEY_F11, "F11" },
			{ KeyCode.KEY_F12, "F12" },
			{ KeyCode.KEY_CAPSLOCKTOGGLE, "CAPSLOCKTOGGLE" },
			{ KeyCode.KEY_NUMLOCKTOGGLE, "NUMLOCKTOGGLE" },
			{ KeyCode.KEY_SCROLLLOCKTOGGLE, "SCROLLLOCKTOGGLE" },
			{ KeyCode.KEY_AC_BACK, "AC_BACK" },
			{ KeyCode.KEY_AC_BOOKMARKS, "AC_BOOKMARKS" },
			{ KeyCode.KEY_AC_FORWARD, "AC_FORWARD" },
			{ KeyCode.KEY_AC_HOME, "AC_HOME" },
			{ KeyCode.KEY_AC_REFRESH, "AC_REFRESH" },
			{ KeyCode.KEY_AC_SEARCH, "AC_SEARCH" },
			{ KeyCode.KEY_AC_STOP, "AC_STOP" },
			{ KeyCode.KEY_AGAIN, "AGAIN" },
			{ KeyCode.KEY_ALTERASE, "ALTERASE" },
			{ KeyCode.KEY_AMPERSAND, "AMPERSAND" },
			{ KeyCode.KEY_ASTERISK, "ASTERISK" },
			{ KeyCode.KEY_AT, "AT" },
			{ KeyCode.KEY_AUDIOMUTE, "AUDIOMUTE" },
			{ KeyCode.KEY_AUDIONEXT, "AUDIONEXT" },
			{ KeyCode.KEY_AUDIOPLAY, "AUDIOPLAY" },
			{ KeyCode.KEY_AUDIOPREV, "AUDIOPREV" },
			{ KeyCode.KEY_AUDIOSTOP, "AUDIOSTOP" },
			{ KeyCode.KEY_BRIGHTNESSDOWN, "BRIGHTNESSDOWN" },
			{ KeyCode.KEY_BRIGHTNESSUP, "BRIGHTNESSUP" },
			{ KeyCode.KEY_CALCULATOR, "CALCULATOR" },
			{ KeyCode.KEY_CANCEL, "CANCEL" },
			{ KeyCode.KEY_CARET, "CARET" },
			{ KeyCode.KEY_CLEAR, "CLEAR" },
			{ KeyCode.KEY_CLEARAGAIN, "CLEARAGAIN" },
			{ KeyCode.KEY_COLON, "COLON" },
			{ KeyCode.KEY_COMPUTER, "COMPUTER" },
			{ KeyCode.KEY_COPY, "COPY" },
			{ KeyCode.KEY_CRSEL, "CRSEL" },
			{ KeyCode.KEY_CURRENCYSUBUNIT, "CURRENCYSUBUNIT" },
			{ KeyCode.KEY_CURRENCYUNIT, "CURRENCYUNIT" },
			{ KeyCode.KEY_CUT, "CUT" },
			{ KeyCode.KEY_DECIMALSEPARATOR, "DECIMALSEPARATOR" },
			{ KeyCode.KEY_DISPLAYSWITCH, "DISPLAYSWITCH" },
			{ KeyCode.KEY_DOLLAR, "DOLLAR" },
			{ KeyCode.KEY_EJECT, "EJECT" },
			{ KeyCode.KEY_EXCLAIM, "EXCLAIM" },
			{ KeyCode.KEY_BTN_EXECUTE, "EXECUTE" },
			{ KeyCode.KEY_EXSEL, "EXSEL" },
			{ KeyCode.KEY_F13, "F13" },
			{ KeyCode.KEY_F14, "F14" },
			{ KeyCode.KEY_F15, "F15" },
			{ KeyCode.KEY_F16, "F16" },
			{ KeyCode.KEY_F17, "F17" },
			{ KeyCode.KEY_F18, "F18" },
			{ KeyCode.KEY_F19, "F19" },
			{ KeyCode.KEY_F20, "F20" },
			{ KeyCode.KEY_F21, "F21" },
			{ KeyCode.KEY_F22, "F22" },
			{ KeyCode.KEY_F23, "F23" },
			{ KeyCode.KEY_F24, "F24" },
			{ KeyCode.KEY_FIND, "FIND" },
			{ KeyCode.KEY_GREATER, "GREATER" },
			{ KeyCode.KEY_HASH, "HASH" },
			{ KeyCode.KEY_HELP, "HELP" },
			{ KeyCode.KEY_KBDILLUMDOWN, "KBDILLUMDOWN" },
			{ KeyCode.KEY_KBDILLUMTOGGLE, "KBDILLUMTOGGLE" },
			{ KeyCode.KEY_KBDILLUMUP, "KBDILLUMUP" },
			{ KeyCode.KEY_KP_00, "KP_00" },
			{ KeyCode.KEY_KP_000, "KP_000" },
			{ KeyCode.KEY_KP_A, "KP_A" },
			{ KeyCode.KEY_KP_AMPERSAND, "KP_AMPERSAND" },
			{ KeyCode.KEY_KP_AT, "KP_AT" },
			{ KeyCode.KEY_KP_B, "KP_B" },
			{ KeyCode.KEY_KP_BACKSPACE, "KP_BACKSPACE" },
			{ KeyCode.KEY_KP_BINARY, "KP_BINARY" },
			{ KeyCode.KEY_KP_C, "KP_C" },
			{ KeyCode.KEY_KP_CLEAR, "KP_CLEAR" },
			{ KeyCode.KEY_KP_CLEARENTRY, "KP_CLEARENTRY" },
			{ KeyCode.KEY_KP_COLON, "KP_COLON" },
			{ KeyCode.KEY_KP_COMMA, "KP_COMMA" },
			{ KeyCode.KEY_KP_D, "KP_D" },
			{ KeyCode.KEY_KP_DBLAMPERSAND, "KP_DBLAMPERSAND" },
			{ KeyCode.KEY_KP_DBLVERTICALBAR, "KP_DBLVERTICALBAR" },
			{ KeyCode.KEY_KP_DECIMAL, "KP_DECIMAL" },
			{ KeyCode.KEY_KP_E, "KP_E" },
			{ KeyCode.KEY_KP_EQUALS, "KP_EQUALS" },
			{ KeyCode.KEY_KP_EQUALSAS400, "KP_EQUALSAS400" },
			{ KeyCode.KEY_KP_EXCLAM, "KP_EXCLAM" },
			{ KeyCode.KEY_KP_F, "KP_F" },
			{ KeyCode.KEY_KP_GREATER, "KP_GREATER" },
			{ KeyCode.KEY_KP_HASH, "KP_HASH" },
			{ KeyCode.KEY_KP_HEXADECIMAL, "KP_HEXADECIMAL" },
			{ KeyCode.KEY_KP_LEFTBRACE, "KP_LEFTBRACE" },
			{ KeyCode.KEY_KP_LEFTPAREN, "KP_LEFTPAREN" },
			{ KeyCode.KEY_KP_LESS, "KP_LESS" },
			{ KeyCode.KEY_KP_MEMADD, "KP_MEMADD" },
			{ KeyCode.KEY_KP_MEMCLEAR, "KP_MEMCLEAR" },
			{ KeyCode.KEY_KP_MEMDIVIDE, "KP_MEMDIVIDE" },
			{ KeyCode.KEY_KP_MEMMULTIPLY, "KP_MEMMULTIPLY" },
			{ KeyCode.KEY_KP_MEMRECALL, "KP_MEMRECALL" },
			{ KeyCode.KEY_KP_MEMSTORE, "KP_MEMSTORE" },
			{ KeyCode.KEY_KP_MEMSUBTRACT, "KP_MEMSUBTRACT" },
			{ KeyCode.KEY_KP_OCTAL, "KP_OCTAL" },
			{ KeyCode.KEY_KP_PERCENT, "KP_PERCENT" },
			{ KeyCode.KEY_KP_PLUSMINUS, "KP_PLUSMINUS" },
			{ KeyCode.KEY_KP_POWER, "KP_POWER" },
			{ KeyCode.KEY_KP_RIGHTBRACE, "KP_RIGHTBRACE" },
			{ KeyCode.KEY_KP_RIGHTPAREN, "KP_RIGHTPAREN" },
			{ KeyCode.KEY_KP_SPACE, "KP_SPACE" },
			{ KeyCode.KEY_KP_TAB, "KP_TAB" },
			{ KeyCode.KEY_KP_VERTICALBAR, "KP_VERTICALBAR" },
			{ KeyCode.KEY_KP_XOR, "KP_XOR" },
			{ KeyCode.KEY_LEFTPAREN, "LEFTPAREN" },
			{ KeyCode.KEY_MAIL, "MAIL" },
			{ KeyCode.KEY_MEDIASELECT, "MEDIASELECT" },
			{ KeyCode.KEY_MODE, "MODE" },
			{ KeyCode.KEY_MUTE, "MUTE" },
			{ KeyCode.KEY_OPER, "OPER" },
			{ KeyCode.KEY_OUT, "OUT" },
			{ KeyCode.KEY_PASTE, "PASTE" },
			{ KeyCode.KEY_PERCENT, "PERCENT" },
			{ KeyCode.KEY_PLUS, "PLUS" },
			{ KeyCode.KEY_POWER, "POWER" },
			{ KeyCode.KEY_PRINTSCREEN, "PRINTSCREEN" },
			{ KeyCode.KEY_PRIOR, "PRIOR" },
			{ KeyCode.KEY_QUESTION, "QUESTION" },
			{ KeyCode.KEY_QUOTEDBL, "QUOTEDBL" },
			{ KeyCode.KEY_RETURN2, "RETURN2" },
			{ KeyCode.KEY_RIGHTPAREN, "RIGHTPAREN" },
			{ KeyCode.KEY_SELECT, "SELECT" },
			{ KeyCode.KEY_SEPARATOR, "SEPARATOR" },
			{ KeyCode.KEY_SLEEP, "SLEEP" },
			{ KeyCode.KEY_STOP, "STOP" },
			{ KeyCode.KEY_SYSREQ, "SYSREQ" },
			{ KeyCode.KEY_THOUSANDSSEPARATOR, "THOUSANDSSEPARATOR" },
			{ KeyCode.KEY_UNDERSCORE, "UNDERSCORE" },
			{ KeyCode.KEY_UNDO, "UNDO" },
			{ KeyCode.KEY_VOLUMEDOWN, "VOLUMEDOWN" },
			{ KeyCode.KEY_VOLUMEUP, "VOLUMEUP" },
			{ KeyCode.KEY_WWW, "WWW" },
			{ KeyCode.KEY_INVERTED_EXCLAMATION_MARK, "INVERTED_EXCLAMATION_MARK" },
			{ KeyCode.KEY_CENT_SIGN, "CENT_SIGN" },
			{ KeyCode.KEY_POUND_SIGN, "POUND_SIGN" },
			{ KeyCode.KEY_CURRENCY_SIGN, "CURRENCY_SIGN" },
			{ KeyCode.KEY_YEN_SIGN, "YEN_SIGN" },
			{ KeyCode.KEY_BROKEN_BAR, "BROKEN_BAR" },
			{ KeyCode.KEY_SECTION_SIGN, "SECTION_SIGN" },
			{ KeyCode.KEY_DIAERESIS, "DIAERESIS" },
			{ KeyCode.KEY_COPYRIGHT_SIGN, "COPYRIGHT_SIGN" },
			{ KeyCode.KEY_FEMININE_ORDINAL_INDICATOR, "FEMININE_ORDINAL_INDICATOR" },
			{ KeyCode.KEY_LEFT_POINTING_DOUBLE_ANGLE_QUOTATION_MARK, "LEFT_POINTING_DOUBLE_ANGLE_QUOTATION_MARK" },
			{ KeyCode.KEY_NOT_SIGN, "NOT_SIGN" },
			{ KeyCode.KEY_REGISTERED_SIGN, "REGISTERED_SIGN" },
			{ KeyCode.KEY_MACRON, "MACRON" },
			{ KeyCode.KEY_DEGREE_SYMBOL, "DEGREE_SYMBOL" },
			{ KeyCode.KEY_PLUS_MINUS_SIGN, "PLUS_MINUS_SIGN" },
			{ KeyCode.KEY_SUPERSCRIPT_TWO, "SUPERSCRIPT_TWO" },
			{ KeyCode.KEY_SUPERSCRIPT_THREE, "SUPERSCRIPT_THREE" },
			{ KeyCode.KEY_ACUTE_ACCENT, "ACUTE_ACCENT" },
			{ KeyCode.KEY_MICRO_SIGN, "MICRO_SIGN" },
			{ KeyCode.KEY_PILCROW_SIGN, "PILCROW_SIGN" },
			{ KeyCode.KEY_MIDDLE_DOT, "MIDDLE_DOT" },
			{ KeyCode.KEY_CEDILLA, "CEDILLA" },
			{ KeyCode.KEY_SUPERSCRIPT_ONE, "SUPERSCRIPT_ONE" },
			{ KeyCode.KEY_MASCULINE_ORDINAL_INDICATOR, "MASCULINE_ORDINAL_INDICATOR" },
			{ KeyCode.KEY_RIGHT_POINTING_DOUBLE_ANGLE_QUOTATION_MARK, "RIGHT_POINTING_DOUBLE_ANGLE_QUOTATION_MARK" },
			{ KeyCode.KEY_VULGAR_FRACTION_ONE_QUARTER, "VULGAR_FRACTION_ONE_QUARTER" },
			{ KeyCode.KEY_VULGAR_FRACTION_ONE_HALF, "VULGAR_FRACTION_ONE_HALF" },
			{ KeyCode.KEY_VULGAR_FRACTION_THREE_QUARTERS, "VULGAR_FRACTION_THREE_QUARTERS" },
			{ KeyCode.KEY_INVERTED_QUESTION_MARK, "INVERTED_QUESTION_MARK" },
			{ KeyCode.KEY_MULTIPLICATION_SIGN, "MULTIPLICATION_SIGN" },
			{ KeyCode.KEY_SHARP_S, "SHARP_S" },
			{ KeyCode.KEY_A_WITH_GRAVE, "A_WITH_GRAVE" },
			{ KeyCode.KEY_A_WITH_ACUTE, "A_WITH_ACUTE" },
			{ KeyCode.KEY_A_WITH_CIRCUMFLEX, "A_WITH_CIRCUMFLEX" },
			{ KeyCode.KEY_A_WITH_TILDE, "A_WITH_TILDE" },
			{ KeyCode.KEY_A_WITH_DIAERESIS, "A_WITH_DIAERESIS" },
			{ KeyCode.KEY_A_WITH_RING_ABOVE, "A_WITH_RING_ABOVE" },
			{ KeyCode.KEY_AE, "AE" },
			{ KeyCode.KEY_C_WITH_CEDILLA, "C_WITH_CEDILLA" },
			{ KeyCode.KEY_E_WITH_GRAVE, "E_WITH_GRAVE" },
			{ KeyCode.KEY_E_WITH_ACUTE, "E_WITH_ACUTE" },
			{ KeyCode.KEY_E_WITH_CIRCUMFLEX, "E_WITH_CIRCUMFLEX" },
			{ KeyCode.KEY_E_WITH_DIAERESIS, "E_WITH_DIAERESIS" },
			{ KeyCode.KEY_I_WITH_GRAVE, "I_WITH_GRAVE" },
			{ KeyCode.KEY_I_WITH_ACUTE, "I_WITH_ACUTE" },
			{ KeyCode.KEY_I_WITH_CIRCUMFLEX, "I_WITH_CIRCUMFLEX" },
			{ KeyCode.KEY_I_WITH_DIAERESIS, "I_WITH_DIAERESIS" },
			{ KeyCode.KEY_ETH, "ETH" },
			{ KeyCode.KEY_N_WITH_TILDE, "N_WITH_TILDE" },
			{ KeyCode.KEY_O_WITH_GRAVE, "O_WITH_GRAVE" },
			{ KeyCode.KEY_O_WITH_ACUTE, "O_WITH_ACUTE" },
			{ KeyCode.KEY_O_WITH_CIRCUMFLEX, "O_WITH_CIRCUMFLEX" },
			{ KeyCode.KEY_O_WITH_TILDE, "O_WITH_TILDE" },
			{ KeyCode.KEY_O_WITH_DIAERESIS, "O_WITH_DIAERESIS" },
			{ KeyCode.KEY_DIVISION_SIGN, "DIVISION_SIGN" },
			{ KeyCode.KEY_O_WITH_STROKE, "O_WITH_STROKE" },
			{ KeyCode.KEY_U_WITH_GRAVE, "U_WITH_GRAVE" },
			{ KeyCode.KEY_U_WITH_ACUTE, "U_WITH_ACUTE" },
			{ KeyCode.KEY_U_WITH_CIRCUMFLEX, "U_WITH_CIRCUMFLEX" },
			{ KeyCode.KEY_U_WITH_DIAERESIS, "U_WITH_DIAERESIS" },
			{ KeyCode.KEY_Y_WITH_ACUTE, "Y_WITH_ACUTE" },
			{ KeyCode.KEY_THORN, "THORN" },
			{ KeyCode.KEY_Y_WITH_DIAERESIS, "Y_WITH_DIAERESIS" },
			{ KeyCode.KEY_EURO_SIGN, "EURO_SIGN" },
			{ KeyCode.KEY_TILDE, "TILDE" },
			{ KeyCode.KEY_LEFT_CURLY_BRACKET, "LEFT_CURLY_BRACKET" },
			{ KeyCode.KEY_RIGHT_CURLY_BRACKET, "RIGHT_CURLY_BRACKET" },
			{ KeyCode.KEY_VERTICAL_BAR, "VERTICAL_BAR" },
			{ KeyCode.KEY_CYRILLIC_YU, "KEY_CYRILLIC_YU" },
			{ KeyCode.KEY_CYRILLIC_E, "KEY_CYRILLIC_E" },
			{ KeyCode.KEY_CYRILLIC_HARD_SIGN, "KEY_CYRILLIC_HARD_SIGN" },
			{ KeyCode.KEY_CYRILLIC_HA, "KEY_CYRILLIC_HA" },
			{ KeyCode.KEY_CYRILLIC_IO, "KEY_CYRILLIC_IO" },
			{ KeyCode.KEY_CYRILLIC_ZHE, "KEY_CYRILLIC_ZHE" },
			{ KeyCode.MouseLeft, "mouse1" },
			{ KeyCode.MouseRight, "mouse2" },
			{ KeyCode.MouseMiddle, "mouse3" },
			{ KeyCode.MouseBack, "mouse4" },
			{ KeyCode.MouseForward, "mouse5" }
		};

#endif

		public static void SaveToFile(
#if SANDBOX
			ReActionSettings meta
#endif
			)
		{
#if SANDBOX
			Sandbox.FileSystem.Data.WriteJson(filePath, meta.Serialize());
#elif UNITY_EDITOR || UNITY_STANDALONE
			var path = Path.Combine(Application.persistentDataPath, filePath);

			Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "ReAction"));
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.Write(SaveToJSONSkullEmojiFuckYouUnity());
				}
			}

			Debug.Log(path);
#endif
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		//ONLY WAY TO SERIALISE IS BY USING JSONUTILITY.TOJSON BUT IT'S ONLY FOR FUCKING UNITY OBJECTS!!!!!!!!!!!!! WHAT DO YOU MEAN YOU CAN'T SERIALISE A HASHSET YOU MORON
		public static string SaveToJSONSkullEmojiFuckYouUnity()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("[");
			foreach (var action in Actions)
			{
				sb.AppendLine("\t{");
				foreach (var property in action.GetType().GetProperties((BindingFlags)int.MaxValue))
				{
					if (property.SetMethod != null)
					{
						sb.AppendLine($"\t\t\"{property.Name}\": \"{property.GetValue(action).ToString()}\",");
					}
				}
				sb.AppendLine("\t},");
			}
			sb.AppendLine("]");

			return sb.ToString();
		}
#endif

		public class Action
		{
			[JsonConstructor]
			public Action(InputAction inputAction, ReAction.Conditional conditional = ReAction.Conditional.Press, ReAction.Modifiers modifiers = ReAction.Modifiers.None)
			{
				this.InputAction = inputAction;
				this.Conditional = conditional;
				this.Modifiers = modifiers;
			}

			public Action(Action action)
			{
				this.InputAction = action.InputAction;
				this.Conditional = action.Conditional;
				this.Modifiers = action.Modifiers;
			}

			/*public Action(string name, int index, KeyBind bind, GamepadInput gamepadInput, Conditional conditional, string category = "Default")
			{
				Name = name;
				Index = index;
				Bind = bind;
				GamepadInput = gamepadInput;
				Conditional = conditional;
				Category = category;
			}

			public Action(string name, int index, KeyBind bind, GamepadInput gamepadInput, bool positiveAxis, Conditional conditional, string category = "Default")
			{
				Name = name;
				Index = index;
				Bind = bind;
				GamepadInput = gamepadInput;
				PositiveAxis = positiveAxis;
				Conditional = conditional;
				Category = category;
			}

			public Action(string name, int index, KeyBind bind, Conditional conditional, string category = "Default")
			{
				Name = name;
				Index = index;
				Bind = bind;
				Conditional = conditional;
				Category = category;
				GamepadInput = GamepadInput.None;
			}

			public Action() 
			{
				Index = ReAction.Actions.Count;
				Name = $"New action {Index}";
			}

			public Action(Action other)
			{
				this.Name = other.Name;
				this.Index = ReAction.Actions.Count;
				this.Bind = other.Bind;
				this.SecondaryBind = other.SecondaryBind;
				this.GamepadInput = other.GamepadInput;
				this.PositiveAxis = other.PositiveAxis;
				this.Conditional = other.Conditional;
				this.Category = other.Category;
			}

			public Action(string name, int index, KeyBind bind, KeyBind secondaryBind, Conditional conditional, string category = "Default")
			{
				this.Name = name;
				this.Index = index;
				this.Bind = bind;
				this.SecondaryBind = secondaryBind;
				this.Conditional = conditional;
				this.Category = category;
			}

			public Action(string name, int index, KeyBind bind, KeyBind secondaryBind, GamepadInput gamepadInput, bool positiveAxis, Conditional conditional, string category = "Default")
			{
				Name = name;
				Index = index;
				Bind = bind;
				SecondaryBind = secondaryBind;
				GamepadInput = gamepadInput;
				PositiveAxis = positiveAxis;
				Conditional = conditional;
				Category = category;
			}*/

			/*			/// <summary>
						/// The name of the action
						/// </summary>
						public string Name
						{
							get;
							set;
						}*/

			/*/// <summary>
			/// The index of the action, you should use consts if you want to use this
			/// </summary>
			public int Index
			{
				get;
				set;
			}

			/// <summary>
			/// The current key of the action, you shouldn't use this to check the status of an action
			/// </summary>
			[InlineEditor]
			public KeyBind Bind
			{
				get;
				set;
			}

			/// <summary>
			/// The secondary key of the action
			/// </summary>
			[InlineEditor]
			public KeyBind SecondaryBind
			{
				get;
				set;
			}

			/// <summary>
			/// The GamepadInput for this, uh, yeah idk if it's useful :(
			/// </summary>
			public GamepadInput GamepadInput
			{
				get;
				set;
			}

			/// <summary>
			/// For joysticks, determines on which axis the action will be triggered
			/// </summary>
			public bool PositiveAxis
			{
				get;
				set;
			}*/

			/// <summary>
			/// The <see cref="Sandbox.InputAction"/> this ReAction is bound to.
			/// </summary>
			[InlineEditor]
			public InputAction InputAction
			{
				get;
				set;
			}

			/// <summary>
			/// What's the activation condition?
			/// </summary>
			public Conditional Conditional
			{
				get;
				set;
			}

			/// <summary>
			/// Set to true if you don't want conditionals to be modifiable, useful if you're only going to check if an action is valid by calling <see cref="ActionTriggeredForCondition(Action, Conditional)"/>, use it for custom binding menus... etc..... it doesn't really do anything on its own :)
			/// </summary>
			public bool AllowSetConditional
			{
				get;
				set;
			}
			public Modifiers Modifiers
			{
				get;
				set;
			}

			/*/// <summary>
			/// Category, use this for a keybind menu of some sorts
			/// </summary>
			public string Category
			{
				get;
				set;
			}*/

			/*/// <summary>
			/// You can get this action's analog value with this, if the <see cref="GamepadInput"/>'s assigned button isn't analog, it'll be 1f if the action is currently triggered, and 0f if it's not
			/// </summary>
#if SANDBOX
			[JsonIgnore]
#endif
			public float Analog
			{
				get
				{
					if (GamepadInput < GamepadInput.LeftTrigger)
					{
						return ReAction.ActionTriggered(this) ? 1f : 0f;
					}
					else
					{
						return GetGamepadCodeAnalog(GamepadInput);
					}
				}
			}*/
		}

/*#if SANDBOX_STANDALONE || UNITY_EDITOR || UNITY_STANDALONE
#endif

		public static float GetGamepadCodeAnalog(GamepadInput gamepadInput)
		{
#if SANDBOX
			switch (gamepadInput)
			{
				case GamepadInput.LeftStickY:
					return Input.GetAnalog(InputAnalog.LeftStickY);
				case GamepadInput.LeftStickX:
					return Input.GetAnalog(InputAnalog.LeftStickX);
				case GamepadInput.RightStickY:
					return Input.GetAnalog(InputAnalog.RightStickY);
				case GamepadInput.RightStickX:
					return Input.GetAnalog(InputAnalog.RightStickX);
				case GamepadInput.LeftTrigger:
					return Input.GetAnalog(InputAnalog.LeftTrigger);
				case GamepadInput.RightTrigger:
					return Input.GetAnalog(InputAnalog.RightTrigger);
				default:
					return 0f;
			}
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
#warning TODO: make this interact with SDL
			return 1f;
#endif
		}*/

#if UNITY_EDITOR || UNITY_STANDALONE
		public static void ReadFromJSON(string path)
		{
			HashSet<Action> actions = new();

			Dictionary<string, PropertyInfo> nameForPropertyDic = new();

			foreach (var property in typeof(Action).GetProperties((BindingFlags)int.MaxValue))
			{
				nameForPropertyDic.Add(property.Name, property);
			}

			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					string line;

					Action action = new Action();

					while ((line = sr.ReadLine()) != null)
					{
						if (line.Contains("{") || line.Contains("[") || line.Contains("]")) //this makes this the worst json reader even created, I'm really proud of that fact
							continue;

						if (line.Contains("},"))
						{
							actions.Add(action);
							action = new Action();
							continue;
						}

						string[] splitLine = line.Split(':', StringSplitOptions.RemoveEmptyEntries);

						string propertyName = splitLine[0].Trim().Split('"', StringSplitOptions.RemoveEmptyEntries)[0];
						string propertyValue = splitLine[1].Trim().Split('"', StringSplitOptions.RemoveEmptyEntries)[0];

						if (nameForPropertyDic.TryGetValue(propertyName, out var property))
						{
							Type propertyType = property.PropertyType;

							object value = default;

							if (propertyType == typeof(int))
							{
								value = int.Parse(propertyValue);
							}
							else if (propertyType == typeof(string))
							{
								value = propertyValue;
							}
							else if (propertyType == typeof(bool))
							{
								value = bool.Parse(propertyValue);
							}
							else if (propertyType.IsEnum)
							{
								value = Enum.Parse(propertyType, propertyValue);
							}
							else if (propertyType == typeof(float))
							{
								value = float.Parse(propertyValue);
							}

							property.SetValue(action, value);
						}
					}
				}
			}

			Actions = actions;
		}
#endif

		public static void PopulateActions(bool toDefault = false)
		{
			if (ReAction.Actions.Count > 0 && !toDefault)
				return;

#if SANDBOX
			if (!toDefault)
			{
				if (Sandbox.FileSystem.Data.FileExists(filePath))
				{
					ReActionLogger.Info("Found locally saved set, applying to the thing, yeah");

					var settings = new ReActionSettings();
					settings.Deserialize(Sandbox.FileSystem.Data.ReadAllText(filePath));

					ReAction.Actions = settings.Actions;

					return;
				}
				else
				{
					ReActionLogger.Info("Couldn't find locally saved set");
				}
			}

				if (ProjectSettings.Get<ReActionSettings>(defaultFilePath) != null)
				{
					ReActionLogger.Info("Found default set, applying");

					ReAction.Actions = new(ProjectSettings.Get<ReActionSettings>(defaultFilePath).Actions);

					return;
				}
#elif UNITY_EDITOR || UNITY_STANDALONE
			if (!toDefault)
			{
				var path = Path.Combine(Application.persistentDataPath, filePath);

				if (File.Exists(path))
				{
					using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						using (StreamReader sr = new StreamReader(fs))
						{
							ReAction.ReadFromJSON(path);
						}
					}

					return;
				}
			}

			var defaultPath = Path.Combine(Application.dataPath, defaultFilePath);
			Debug.Log(defaultPath);
			if (File.Exists(defaultPath))
			{
				using (FileStream fs = new FileStream(defaultPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (StreamReader sr = new StreamReader(fs))
					{
						ReAction.ReadFromJSON(defaultPath);
					}
				}

				return;
			}
#endif

			ReActionLogger.Info("Action list is null, populating with defaults");

			ReAction.Actions = GetDefaultActions();
		}

#if SANDBOX
		/// <summary>
		/// Allows to bind individual axes to actions, this is just <see cref="GamepadCode"/> combined with <see cref="InputAnalog"/>
		/// </summary>
		public enum GamepadInput
		{
			None = -1,
			A = 0,
			B = 1,
			X = 2,
			Y = 3,
			SwitchLeftMenu = 4,
			Guide = 5,
			SwitchRightMenu = 6,
			LeftJoystickButton = 7,
			RightJoystickButton = 8,
			SwitchLeftBumper = 9,
			SwitchRightBumper = 10,
			DpadNorth = 11,
			DpadSouth = 12,
			DpadWest = 13,
			DpadEast = 14,
			Misc1 = 15,
			Paddle1 = 16,
			Paddle2 = 17,
			Paddle3 = 18,
			Paddle4 = 19,
			Touchpad = 20,
			BUTTONS_MAX = 21,
			LeftTrigger = 100,
			RightTrigger = 101,
			RightStickX,
			RightStickY,
			LeftStickX,
			LeftStickY
		}
#elif UNITY_EDITOR || UNITY_STANDALONE
		//Holy fucking shit Unity has fucking awful Gamepad support, what the fuck
		//Let's just use SDL, fuck it

		/// <summary>
		/// These are just the Joystick1Buttons from <see cref="KeyCode"/> but in more human readable form, supposedly they're correct for XB1 controllers on Windows 10
		/// </summary>
		public enum GamepadInput
		{
			None = -1,
			A = 0,
			B = 1,
			X = 2,
			Y = 3,
			SwitchLeftMenu = 4,
			Guide = 5,
			SwitchRightMenu = 6,
			LeftJoystickButton = 7,
			RightJoystickButton = 8,
			SwitchLeftBumper = 9,
			SwitchRightBumper = 10,
			DpadNorth = 11,
			DpadSouth = 12,
			DpadWest = 13,
			DpadEast = 14,
			Misc1 = 15,
			Paddle1 = 16,
			Paddle2 = 17,
			Paddle3 = 18,
			Paddle4 = 19,
			Touchpad = 20,
			BUTTONS_MAX = 21,
			LeftTrigger = 100,
			RightTrigger = 101,
			RightStickX,
			RightStickY,
			LeftStickX,
			LeftStickY
		}
#endif

		public enum Conditional : byte //why did I make it a byte, idk
		{
			/// <summary>
			/// Active when key is pressed, when it wasn't before
			/// </summary>
			Press,
			/// <summary>
			/// Active when is key is held down for the duration of <see cref="LongPressTimeOut"/>
			/// </summary>
			LongPress,
			/// <summary>
			/// Active while key is down
			/// </summary>
			Continuous,
			/// <summary>
			/// Active when key is released
			/// </summary>
			Release,
			/// <summary>
			/// Active when key is released twice during the <see cref="DoubleTapTimeOut"/> interval
			/// </summary>
			DoubleTap,
			/// <summary>
			/// Active when key is pressed, and inactive after it's pressed again
			/// </summary>
			Toggle,
			/// <summary>
			/// Active when key is mashed quickly
			/// </summary>
			Mash
		}

/*#if UNITY_EDITOR
		[Serialize]
#endif
		public struct KeyBind(ReAction.KeyCode key, ReAction.Modifiers modifiers)
		{
#if SANDBOX
			[JsonInclude]
#endif
			public KeyCode key = key;

			//bitmask, each bit corresponds to the following modifier:
			// 1st bit: lshift
			// 2nd bit: lctrl
			// 3rd bit: lwin/lmeta/lwhatever
			// 4th bit: lalt
			// 5th bit: ralt
			// 6th bit: rwin/rmeta/rwhatever
			// 7th bit: rshift
			// 8th bit: rctrl
#if SANDBOX
			[JsonInclude]
#endif
			public Modifiers modifiers = modifiers;

			public static implicit operator KeyBind(KeyCode key)
			{
				return new KeyBind(key, Modifiers.None);
			}
		}*/

#if SANDBOX
		//taken from NativeEngine.ButtonCode, as it's an internal enum :(
		public enum KeyCode
		{
			BUTTON_CODE_INVALID = -1,
			KEY_NONE = 0,
			KEY_0,
			KEY_1,
			KEY_2,
			KEY_3,
			KEY_4,
			KEY_5,
			KEY_6,
			KEY_7,
			KEY_8,
			KEY_9,
			KEY_A,
			KEY_B,
			KEY_C,
			KEY_D,
			KEY_E,
			KEY_F,
			KEY_G,
			KEY_H,
			KEY_I,
			KEY_J,
			KEY_K,
			KEY_L,
			KEY_M,
			KEY_N,
			KEY_O,
			KEY_P,
			KEY_Q,
			KEY_R,
			KEY_S,
			KEY_T,
			KEY_U,
			KEY_V,
			KEY_W,
			KEY_X,
			KEY_Y,
			KEY_Z,
			KEY_PAD_0,
			KEY_PAD_1,
			KEY_PAD_2,
			KEY_PAD_3,
			KEY_PAD_4,
			KEY_PAD_5,
			KEY_PAD_6,
			KEY_PAD_7,
			KEY_PAD_8,
			KEY_PAD_9,
			KEY_PAD_DIVIDE,
			KEY_PAD_MULTIPLY,
			KEY_PAD_MINUS,
			KEY_PAD_PLUS,
			KEY_PAD_ENTER,
			KEY_PAD_DECIMAL,
			KEY_LESS,
			KEY_LBRACKET,
			KEY_RBRACKET,
			KEY_SEMICOLON,
			KEY_APOSTROPHE,
			KEY_BACKQUOTE,
			KEY_COMMA,
			KEY_PERIOD,
			KEY_SLASH,
			KEY_BACKSLASH,
			KEY_MINUS,
			KEY_EQUAL,
			KEY_ENTER,
			KEY_SPACE,
			KEY_BACKSPACE,
			KEY_TAB,
			KEY_CAPSLOCK,
			KEY_NUMLOCK,
			KEY_ESCAPE,
			KEY_SCROLLLOCK,
			KEY_INSERT,
			KEY_DELETE,
			KEY_HOME,
			KEY_END,
			KEY_PAGEUP,
			KEY_PAGEDOWN,
			KEY_BREAK,
			KEY_LSHIFT,
			KEY_RSHIFT,
			KEY_LALT,
			KEY_RALT,
			KEY_LCONTROL,
			KEY_RCONTROL,
			KEY_LWIN,
			KEY_RWIN,
			KEY_APP,
			KEY_UP,
			KEY_LEFT,
			KEY_DOWN,
			KEY_RIGHT,
			KEY_F1,
			KEY_F2,
			KEY_F3,
			KEY_F4,
			KEY_F5,
			KEY_F6,
			KEY_F7,
			KEY_F8,
			KEY_F9,
			KEY_F10,
			KEY_F11,
			KEY_F12,
			KEY_CAPSLOCKTOGGLE,
			KEY_NUMLOCKTOGGLE,
			KEY_SCROLLLOCKTOGGLE,
			KEY_AC_BACK,
			KEY_AC_BOOKMARKS,
			KEY_AC_FORWARD,
			KEY_AC_HOME,
			KEY_AC_REFRESH,
			KEY_AC_SEARCH,
			KEY_AC_STOP,
			KEY_AGAIN,
			KEY_ALTERASE,
			KEY_AMPERSAND,
			KEY_ASTERISK,
			KEY_AT,
			KEY_AUDIOMUTE,
			KEY_AUDIONEXT,
			KEY_AUDIOPLAY,
			KEY_AUDIOPREV,
			KEY_AUDIOSTOP,
			KEY_BRIGHTNESSDOWN,
			KEY_BRIGHTNESSUP,
			KEY_CALCULATOR,
			KEY_CANCEL,
			KEY_CARET,
			KEY_CLEAR,
			KEY_CLEARAGAIN,
			KEY_COLON,
			KEY_COMPUTER,
			KEY_COPY,
			KEY_CRSEL,
			KEY_CURRENCYSUBUNIT,
			KEY_CURRENCYUNIT,
			KEY_CUT,
			KEY_DECIMALSEPARATOR,
			KEY_DISPLAYSWITCH,
			KEY_DOLLAR,
			KEY_EJECT,
			KEY_EXCLAIM,
			KEY_BTN_EXECUTE,
			KEY_EXSEL,
			KEY_F13,
			KEY_F14,
			KEY_F15,
			KEY_F16,
			KEY_F17,
			KEY_F18,
			KEY_F19,
			KEY_F20,
			KEY_F21,
			KEY_F22,
			KEY_F23,
			KEY_F24,
			KEY_FIND,
			KEY_GREATER,
			KEY_HASH,
			KEY_HELP,
			KEY_KBDILLUMDOWN,
			KEY_KBDILLUMTOGGLE,
			KEY_KBDILLUMUP,
			KEY_KP_00,
			KEY_KP_000,
			KEY_KP_A,
			KEY_KP_AMPERSAND,
			KEY_KP_AT,
			KEY_KP_B,
			KEY_KP_BACKSPACE,
			KEY_KP_BINARY,
			KEY_KP_C,
			KEY_KP_CLEAR,
			KEY_KP_CLEARENTRY,
			KEY_KP_COLON,
			KEY_KP_COMMA,
			KEY_KP_D,
			KEY_KP_DBLAMPERSAND,
			KEY_KP_DBLVERTICALBAR,
			KEY_KP_DECIMAL,
			KEY_KP_E,
			KEY_KP_EQUALS,
			KEY_KP_EQUALSAS400,
			KEY_KP_EXCLAM,
			KEY_KP_F,
			KEY_KP_GREATER,
			KEY_KP_HASH,
			KEY_KP_HEXADECIMAL,
			KEY_KP_LEFTBRACE,
			KEY_KP_LEFTPAREN,
			KEY_KP_LESS,
			KEY_KP_MEMADD,
			KEY_KP_MEMCLEAR,
			KEY_KP_MEMDIVIDE,
			KEY_KP_MEMMULTIPLY,
			KEY_KP_MEMRECALL,
			KEY_KP_MEMSTORE,
			KEY_KP_MEMSUBTRACT,
			KEY_KP_OCTAL,
			KEY_KP_PERCENT,
			KEY_KP_PLUSMINUS,
			KEY_KP_POWER,
			KEY_KP_RIGHTBRACE,
			KEY_KP_RIGHTPAREN,
			KEY_KP_SPACE,
			KEY_KP_TAB,
			KEY_KP_VERTICALBAR,
			KEY_KP_XOR,
			KEY_LEFTPAREN,
			KEY_MAIL,
			KEY_MEDIASELECT,
			KEY_MODE,
			KEY_MUTE,
			KEY_OPER,
			KEY_OUT,
			KEY_PASTE,
			KEY_PERCENT,
			KEY_PLUS,
			KEY_POWER,
			KEY_PRINTSCREEN,
			KEY_PRIOR,
			KEY_QUESTION,
			KEY_QUOTEDBL,
			KEY_RETURN2,
			KEY_RIGHTPAREN,
			KEY_SELECT,
			KEY_SEPARATOR,
			KEY_SLEEP,
			KEY_STOP,
			KEY_SYSREQ,
			KEY_THOUSANDSSEPARATOR,
			KEY_UNDERSCORE,
			KEY_UNDO,
			KEY_VOLUMEDOWN,
			KEY_VOLUMEUP,
			KEY_WWW,
			KEY_INVERTED_EXCLAMATION_MARK,
			KEY_CENT_SIGN,
			KEY_POUND_SIGN,
			KEY_CURRENCY_SIGN,
			KEY_YEN_SIGN,
			KEY_BROKEN_BAR,
			KEY_SECTION_SIGN,
			KEY_DIAERESIS,
			KEY_COPYRIGHT_SIGN,
			KEY_FEMININE_ORDINAL_INDICATOR,
			KEY_LEFT_POINTING_DOUBLE_ANGLE_QUOTATION_MARK,
			KEY_NOT_SIGN,
			KEY_REGISTERED_SIGN,
			KEY_MACRON,
			KEY_DEGREE_SYMBOL,
			KEY_PLUS_MINUS_SIGN,
			KEY_SUPERSCRIPT_TWO,
			KEY_SUPERSCRIPT_THREE,
			KEY_ACUTE_ACCENT,
			KEY_MICRO_SIGN,
			KEY_PILCROW_SIGN,
			KEY_MIDDLE_DOT,
			KEY_CEDILLA,
			KEY_SUPERSCRIPT_ONE,
			KEY_MASCULINE_ORDINAL_INDICATOR,
			KEY_RIGHT_POINTING_DOUBLE_ANGLE_QUOTATION_MARK,
			KEY_VULGAR_FRACTION_ONE_QUARTER,
			KEY_VULGAR_FRACTION_ONE_HALF,
			KEY_VULGAR_FRACTION_THREE_QUARTERS,
			KEY_INVERTED_QUESTION_MARK,
			KEY_MULTIPLICATION_SIGN,
			KEY_SHARP_S,
			KEY_A_WITH_GRAVE,
			KEY_A_WITH_ACUTE,
			KEY_A_WITH_CIRCUMFLEX,
			KEY_A_WITH_TILDE,
			KEY_A_WITH_DIAERESIS,
			KEY_A_WITH_RING_ABOVE,
			KEY_AE,
			KEY_C_WITH_CEDILLA,
			KEY_E_WITH_GRAVE,
			KEY_E_WITH_ACUTE,
			KEY_E_WITH_CIRCUMFLEX,
			KEY_E_WITH_DIAERESIS,
			KEY_I_WITH_GRAVE,
			KEY_I_WITH_ACUTE,
			KEY_I_WITH_CIRCUMFLEX,
			KEY_I_WITH_DIAERESIS,
			KEY_ETH,
			KEY_N_WITH_TILDE,
			KEY_O_WITH_GRAVE,
			KEY_O_WITH_ACUTE,
			KEY_O_WITH_CIRCUMFLEX,
			KEY_O_WITH_TILDE,
			KEY_O_WITH_DIAERESIS,
			KEY_DIVISION_SIGN,
			KEY_O_WITH_STROKE,
			KEY_U_WITH_GRAVE,
			KEY_U_WITH_ACUTE,
			KEY_U_WITH_CIRCUMFLEX,
			KEY_U_WITH_DIAERESIS,
			KEY_Y_WITH_ACUTE,
			KEY_THORN,
			KEY_Y_WITH_DIAERESIS,
			KEY_EURO_SIGN,
			KEY_TILDE,
			KEY_LEFT_CURLY_BRACKET,
			KEY_RIGHT_CURLY_BRACKET,
			KEY_VERTICAL_BAR,
			KEY_CYRILLIC_YU,
			KEY_CYRILLIC_E,
			KEY_CYRILLIC_HARD_SIGN,
			KEY_CYRILLIC_HA,
			KEY_CYRILLIC_IO,
			KEY_CYRILLIC_ZHE,
			KEY_CYRILLIC_BE,
			KEY_LAST = 313,
			MouseLeft = 314,
			MouseRight,
			MouseMiddle,
			MouseBack,
			MouseForward,
			MouseWheelUp,
			MouseWheelDown,
			JOYSTICK_FIRST = 321,
			JOYSTICK_FIRST_BUTTON = 321,
			JOYSTICK_LAST_BUTTON = 448,
			JOYSTICK_FIRST_POV_BUTTON,
			JOYSTICK_LAST_POV_BUTTON = 464,
			JOYSTICK_FIRST_AXIS_BUTTON,
			JOYSTICK_LAST_AXIS_BUTTON = 512,
			JOYSTICK_LAST = 512,
			BUTTON_CODE_COUNT,
			BUTTON_CODE_LAST = 512,
			KEY_XBUTTON_UP = 449,
			KEY_XBUTTON_RIGHT,
			KEY_XBUTTON_DOWN,
			KEY_XBUTTON_LEFT,
			KEY_XBUTTON_A = 321,
			KEY_XBUTTON_B,
			KEY_XBUTTON_X,
			KEY_XBUTTON_Y,
			KEY_XBUTTON_LEFT_SHOULDER,
			KEY_XBUTTON_RIGHT_SHOULDER,
			KEY_XBUTTON_BACK,
			KEY_XBUTTON_START,
			KEY_XBUTTON_STICK1,
			KEY_XBUTTON_STICK2,
			KEY_XBUTTON_INACTIVE_START,
			KEY_XSTICK1_RIGHT = 465,
			KEY_XSTICK1_LEFT,
			KEY_XSTICK1_DOWN,
			KEY_XSTICK1_UP,
			KEY_XBUTTON_LTRIGGER,
			KEY_XBUTTON_RTRIGGER,
			KEY_XSTICK2_RIGHT,
			KEY_XSTICK2_LEFT,
			KEY_XSTICK2_DOWN,
			KEY_XSTICK2_UP
		}
#endif
	}
}