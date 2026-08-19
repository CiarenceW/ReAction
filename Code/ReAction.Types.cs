using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;

namespace ReActionPlugin
{
	public class ButtonAction
	{
		/// <param name="name">The name of the action</param>
		/// <param name="primary">The primary bind</param>
		/// <param name="secondary">The secondary bind</param>
		/// <param name="set">The set of the action, by default <c>General</c></param>
		/// <param name="enabled">Whether or not the action is enabled, meaning <see cref="ButtonAction.Active"/> will always return false</param>
		/// <param name="allowedConditionals">Which <see cref="Conditional"/>s are allowed to be set, this doesn't do anything on its own, use it to limit which conditionals the user can set</param>
		[JsonConstructor]
		internal ButtonAction(string name, Bind primary, Bind secondary, string set = "General", bool enabled = true, Conditional allowedConditionals = Conditional.All)
		{
			this.Name = name;
			this.m_Primary = primary;
			this.m_Secondary = secondary;
			this.Set = set;
			this.Enabled = enabled;
			this.AllowedConditionals = allowedConditionals;
		}

		public string Name { get; init; }

		[JsonIgnore, Hide]
		internal Conditional ConditionalsState { get; set; } = Conditional.None;

		public string Set { get; init; } = "general";

		public bool Enabled 
		{ 
			get; 

			set
			{
				field = value;
				ReAction.UpdateActionEnabled(this);
			} 
		}

		[InlineEditor, Title("Primary Bind")]
		public Bind Primary
		{
			get
			{
				return m_Primary;
			}

			set
			{
				m_Primary = value;
			}
		}

		[Hide] internal Bind m_Primary;

		[InlineEditor, Title("Secondary Bind")]
		public Bind Secondary
		{
			get
			{
				return m_Secondary;
			}

			set
			{
				m_Secondary = value;
			}
		}

		[Hide] internal Bind m_Secondary;

		public ControllerBind Gamepad
		{
			get
			{
				return m_ControllerBind;
			}

			set
			{
				m_ControllerBind = value;
			}
		}

		[Hide] internal ControllerBind m_ControllerBind;

		public Conditional AllowedConditionals { get; set; } = Conditional.All;

		[JsonIgnore, Hide]
		public bool Active 
		{
			//still get Enabled, because it might be disabled in between frames
			get 
			{ 
				return Enabled && field; 
			} 

			internal set; 
		}

		public bool GetConditionState(Conditional conditional)
		{
			return (ConditionalsState & conditional) != Conditional.None;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_Primary, m_Secondary, Set, AllowedConditionals);
		}

		public override string ToString()
		{
			return this.Name;
		}

		public static implicit operator bool(ButtonAction action)
		{
			return action.Active;
		}

		public struct ControllerBind
		{
			public ControllerBind(ControllerButton button, Conditional conditional, Modifiers modifiers = Modifiers.None, float timeout = 0.5f)
			{
				Button = button;
				Conditional = conditional;
				Modifiers = modifiers;
				TimeOut = (Half)timeout;
			}

			//need 6bits for this
			public ControllerButton Button
			{
				readonly get
				{
					return (ControllerButton)(m_InternalBitmask & k_ControllerButtonMask);
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_ControllerButtonMask) | (uint)value;
				}
			}

			//These two are 8 bits each, combined, that's 16, but these are only used for digital buttons, so we could use the bits for the deadzone
			[HideIf(nameof(Button), ControllerButton.None)]
			public Modifiers Modifiers
			{
				readonly get
				{
					return (Modifiers)((m_InternalBitmask & k_ModifiersMask) >> k_ModifiersBitOffset);
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_ModifiersMask) | ((uint)value << k_ModifiersBitOffset);
				}
			}

			[HideIf(nameof(Button), ControllerButton.None)]
			public Conditional Conditional
			{
				readonly get
				{
					return (Conditional)((m_InternalBitmask & k_ConditionalMask) >> k_ConditionalBitOffset);
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_ConditionalMask) | ((uint)value << k_ConditionalBitOffset);
				}
			}

			//need the whole 16 bits for this
			[ShowIf(nameof(Button), ControllerButton.None)]
			public Half Deadzone
			{
				readonly get
				{
					return Unsafe.BitCast<ushort, Half>((ushort)((m_InternalBitmask & k_DeadzoneMask) >> k_DeadzoneBitOffset));
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_DeadzoneMask) | ((uint)Unsafe.BitCast<Half, ushort>(value) << k_DeadzoneBitOffset);
				}
			}

			[JsonIgnore, Hide]
			internal bool DoubleTapped
			{
				readonly get
				{
					return (m_InternalBitmask & k_DoubleTappedMask) != 0;
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_DoubleTappedMask) | ((uint)Unsafe.BitCast<bool, byte>(value) << k_DoubleTappedBitOffset);
				}
			}

			[JsonIgnore, Hide]
			internal bool CountTappedTime
			{
				readonly get
				{
					return (m_InternalBitmask & k_TappedMask) != 0;
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_TappedMask) | ((uint)Unsafe.BitCast<bool, byte>(value) << k_TappedBitOffset);
				}
			}

			[JsonIgnore, Hide]
			internal bool LongPressed
			{
				readonly get
				{
					return (m_InternalBitmask & k_LongPressedMask) != 0;
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_LongPressedMask) | ((uint)Unsafe.BitCast<bool, byte>(value) << k_LongPressedBitOffset);
				}
			}

#if DEBUG
			[JsonIgnore, Title("Time Out")]
			//The inspector can't show Half values, lol, do this for simplicity
			float TimeOutF
			{
				readonly get => (float)TimeOut;
				set => TimeOut = (Half)value;
			}
#endif

			Half TimeOut
			{
				get; set;
			}

			Half TappedFor
			{
				get; set;
			}

			//b: controller button bits
			//m: modifier bits
			//c: conditional bits
			//t: tapped bits
			//l: long pressed bits
			//d: double tapped bits
			//x: unused bits
			//z: deadzone bits (replaces modifier and conditional bits if the bind's button is analog)
			//          ZZZZZZ_ZZZZZZZZ_ZZ
			//DLTXXXXX_XXCCCCCC_CCMMMMMM_MMBBBBBB
			uint m_InternalBitmask;

			const uint k_ControllerButtonMask = 0b00000000_00000000_00000000_00111111u;

			const uint k_ModifiersMask = 0b00000000_00000000_00111111_110000000u;
			const int k_ModifiersBitOffset = 6;

			const uint k_ConditionalMask = 0b00000000_00111111_11000000_00000000u;
			const int k_ConditionalBitOffset = 14;

			const uint k_DeadzoneMask = k_ConditionalMask | k_ModifiersMask;
			const int k_DeadzoneBitOffset = k_ModifiersBitOffset;

			const uint k_TappedMask = 0b00100000_00000000_00000000_00000000u;
			const int k_TappedBitOffset = 29;

			const uint k_LongPressedMask = 0b01000000_00000000_00000000_00000000u;
			const int k_LongPressedBitOffset = 30;

			const uint k_DoubleTappedMask = 0b10000000_00000000_00000000_00000000u;
			const int k_DoubleTappedBitOffset = 31;

			public readonly float Analog
			{
				get
				{
					return (Button is > ControllerButton.MAX and not ControllerButton.None) ? ReAction.
				}
			}
		}

		public struct Bind
		{
			public Bind(ButtonCode key, Conditional conditional, Modifiers modifiers = Modifiers.None, float timeOut = .5f)
			{
				this.Key = key;
				this.Modifiers = modifiers;
				this.Conditional = conditional;
#if USE_32BIT_FLOATS_FOR_TIME
				this.TimeOut = timeOut;
#else
				this.TimeOut = (Half)timeOut;
#endif
			}

			public ButtonCode Key
			{
				readonly get
				{
					return (ButtonCode)(m_InternalBitmask & k_KeyMask);
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_KeyMask) | (uint)value;
				}
			}

			[HideIf(nameof(Key), ButtonCode.BUTTON_CODE_NONE)]
			public Modifiers Modifiers
			{
				readonly get
				{
					return (Modifiers)((m_InternalBitmask & k_ModifiersMask) >> k_ModifiersBitOffset);
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_ModifiersMask) | ((uint)value << k_ModifiersBitOffset);
				}
			}

			[HideIf(nameof(Key), ButtonCode.BUTTON_CODE_NONE)]
			public Conditional Conditional
			{
				readonly get
				{
					return (Conditional)((m_InternalBitmask & k_ConditionalMask) >> k_ConditionalBitOffset);
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_ConditionalMask) | ((uint)value << k_ConditionalBitOffset);
				}
			}

			[JsonIgnore, Hide]
			internal bool DoubleTapped
			{
				readonly get
				{
					return (m_InternalBitmask & k_DoubleTappedMask) != 0;
				}

				set
				{
					//crazy performance improvement, saves 2 (two(!)) instructions
					m_InternalBitmask = (m_InternalBitmask & ~k_DoubleTappedMask) | ((uint)Unsafe.BitCast<bool, byte>(value) << k_DoubleTappedBitOffset);
				}
			}

			[JsonIgnore, Hide]
			internal bool CountTappedTime
			{
				readonly get
				{
					return (m_InternalBitmask & k_TappedMask) != 0;
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_TappedMask) | ((uint)Unsafe.BitCast<bool, byte>(value) << k_TappedBitOffset);
				}
			}

			[JsonIgnore, Hide]
			internal bool LongPressed
			{
				readonly get
				{
					return (m_InternalBitmask & k_LongPressedMask) != 0;
				}

				set
				{
					m_InternalBitmask = (m_InternalBitmask & ~k_LongPressedMask) | ((uint)Unsafe.BitCast<bool, byte>(value) << k_LongPressedBitOffset);
				}
			}

#if USE_32BIT_FLOATS_FOR_TIME
				public float TimeOut { readonly get; set; }

				internal float TappedFor { readonly get; set; }
#else
			[Hide]
			public Half TimeOut { readonly get; set; }

#if DEBUG
			[JsonIgnore, Title("Time Out")]
			//The inspector can't show Half values, lol, do this for simplicity
			float TimeOutF
			{
				readonly get => (float)TimeOut;
				set => TimeOut = (Half)value;
			}
#endif

			[JsonIgnore, Hide]
			internal Half TappedFor
			{
				readonly get; set;
			}
#endif

			//this + timeout means this whole struct only takes 64 bits, wow, that's one single register!!
			// k: key code bits
			// m: modifier bits
			// c: conditional bits
			// t: tapped bits
			// l: long pressed bits
			// d: double tapped bits
			// x: unused bits
			// DLTXXXXC_CCCCCCCM_MMMMMMMK_KKKKKKKKK
			[JsonIgnore, Hide] uint m_InternalBitmask;

			const uint k_KeyMask = 0b00000000_00000000_00000001_11111111u;

			const int k_ModifiersBitOffset = 9;
			const uint k_ModifiersMask = 0b00000000_00000001_11111110_00000000u;

			const int k_ConditionalBitOffset = 17;
			const uint k_ConditionalMask = 0b00000001_11111110_00000000_00000000u;

			const int k_TappedBitOffset = 29;
			const uint k_TappedMask = 0b00100000_00000000_00000000_00000000u;

			const int k_LongPressedBitOffset = 30;
			const uint k_LongPressedMask = 0b01000000_00000000_00000000_00000000u;

			const int k_DoubleTappedBitOffset = 31;
			const uint k_DoubleTappedMask = 0b10000000_00000000_00000000_00000000u;

			public readonly override int GetHashCode()
			{
				return HashCode.Combine(TappedFor, TimeOut, m_InternalBitmask);
			}
		}
	}

	//Stub of Controller, this type has the same fields and offsets as the internal type, which lets use Unsafe.As<Controller> without issues
	public class Controller
	{
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1822 // Mark members as static
		internal Controller()
		{
		}

		static readonly MethodInfo m_CurrentController = typeof(Input).GetProperty("CurrentController", (BindingFlags)int.MaxValue).GetMethod;

		public static Controller GetCurrentController()
		{
			//epic!!!
			return Unsafe.As<Controller>(m_CurrentController.Invoke(null, null));
		}

		static readonly MethodInfo m_AllControllers = m_CurrentController.ReturnType.GetProperty("All", (BindingFlags)int.MaxValue).GetMethod;

		public static HashSet<Controller> All => Unsafe.As<HashSet<Controller>>(m_AllControllers.Invoke(null, null));

		public static Controller GetController(int index)
		{
			return All.ElementAt(index);
		}

		/// <summary>
		/// Get an axis
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public float GetAxis(ControllerAxis axis, float defaultValue = 0f) => 0f /*Stub*/;

		/// <summary>
		/// Rumbles the controller.
		/// </summary>
		/// <param name="leftMotor">The speed of the left motor, between 0 and 0xFFFF</param>
		/// <param name="rightMotor">The speed of the right motor, between 0 and 0xFFFF</param>
		/// <param name="duration">The duration of the vibration in ms</param>
		public void Rumble(int leftMotor, int rightMotor, int duration) { /*Stub*/ }

		/// <summary>
		/// Rumbles the controller's triggers (if supported)
		/// </summary>
		/// <param name="leftTrigger">The speed of the left trigger motor, between 0 and 0xFFFF</param>
		/// <param name="rightTrigger">The speed of the right trigger motor, between 0 and 0xFFFF</param>
		/// <param name="duration">The duration of the vibration in ms</param>
		public void RumbleTriggers(int leftTrigger, int rightTrigger, int duration) { /*Stub*/ }

		/// <summary>
		/// Stops all rumble and haptic events on this controller.
		/// </summary>
		public void StopAllHaptics() { /*Stub*/ }

		/// <summary>
		/// Stop all vibration events on this controller.
		/// </summary>
		public void StopAllVibrations() { /*Stub*/ }

		/// <summary>
		/// Trigger a vibration based on a predefined <see cref="T:Sandbox.HapticPattern" />.
		/// All <see cref="T:Sandbox.HapticPattern" />s are normalized (start at 0, peak at 1).
		/// </summary>
		/// <param name="effect">The pattern to use</param>
		/// <param name="lengthScale">The amount to scale the pattern's length by.</param>
		/// <param name="frequencyScale">The amount to scale the pattern's frequency by.</param>
		/// <param name="amplitudeScale">The amount to scale the pattern's amplitude by.</param>
		public void TriggerHapticEffect(HapticEffect effect, float lengthScale = 1, float frequencyScale = 1f, float amplitudeScale = 1f) { /*Stub*/ }

		public int GetTouchpadCount() => ReAction.SDL_GetNumGamepadTouchpads(ReAction.SDL_GetGamepadFromID(SDLHandle));

		public int GetMaxTouchpadFingers(int touchpad) => ReAction.SDL_GetNumGamepadTouchpadFingers(ReAction.SDL_GetGamepadFromID(SDLHandle), touchpad);

		public TouchpadData GetTouchpadData(int touchpad, int finger) => ReAction.extraPerControllerData[ReAction.GetControllexIndexForDeviceId(DeviceId)].touchpadData[touchpad][finger];

		/// <summary>
		/// Gets a sensor reading from the device's accelerometer (if it has one)
		/// </summary>
		public Vector3 Accelerometer => Vector3.Zero;

		/// <summary>
		/// Gets a sensor reading from the device's gyroscope (if it has one)
		/// </summary>
		public Angles Gyroscope => Angles.Zero;

		/// <summary>
		/// Which glyph folder to use for this controller.
		/// Derived from the controller's glyph set.
		/// </summary>
		public string GlyphVendor => string.Empty;

		readonly object ControllerColors; //: Color[]

		public int SDLHandle { get; set; }

		public int DeviceId { get; set; }

		/// <summary>
		/// The glyph set for this controller, used for icon/prompt selection.
		/// </summary>
		public ControllerGlyphSet GlyphSet { get; set; }

		/// <summary>
		/// Sets the color of the gamepad if supported
		/// </summary>
		public Color32 LEDColor { get; set; }

		/// <summary>
		/// The name of this controller (e.g. "Xbox Wireless Controller", "Steam Controller")
		/// </summary>
		public string Name { get; set; }

		readonly object ActiveHapticEffect; //: Color[]

		readonly object InputContext; //: Input.Context

		readonly object ControllerAxes; //: List<Controller.InputAxis>
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
	}

	//slightly modified version of GamepadCode with SDL3 SDL_GamepadButton values, and LeftTrigger + RightTrigger added
	public enum ControllerButton
	{
		//this is 63 because 63 is six 1s, this is like -1 but i don't have to bother with all the bull shit
		None = 63,
		A = 0,
		Cross = A,
		B,
		Circle = B,
		X,
		Square = X,
		Y,
		Triangle = Y,
		Back,
		Guide,
		Start,
		LeftAnalogStick,
		RightAnalogStick,
		LeftShoulder,
		RightShoulder,
		DPadUp,
		DPadDown,
		DPadLeft,
		DPadRight,
		Misc1,
		RightPaddle1,
		LeftPaddle1,
		RightPaddle2,
		LeftPaddle2,
		Touchpad,
		Misc2,
		Misc3,
		Misc4,
		Misc5,
		Misc6,
		[Hide] MAX = Misc6,
		LeftStickX,
		LeftStickY,
		RightStickX,
		RightStickY,
		LeftTrigger,
		RightTrigger,
		[Hide] AnalogStart = LeftStickX,
		[Hide] AnalogEnd = RightTrigger
	}

	public enum ControllerAxis
	{
		Invalid = -1,
		LeftX,
		LeftY,
		RightX,
		RightY,
		TriggerLeft,
		TriggerRight,
		MAX
	}

	public enum ControllerGlyphSet
	{
		Unknown,
		Xbox,
		PlayStation,
		Switch,
		Steam
	}

	//extra per controller bullshit, wow!
	internal struct ExtraControllerData
	{
		public int deviceId;
		public TouchpadData[][] touchpadData;
		public ControllerButtonState[] buttonState;
		public ControllerAnalogState[] analogState;
	}

	public readonly struct TouchpadData(bool down, float x, float deltaX, float y, float deltaY, float pressure)
	{
		public readonly bool down = down;
		public readonly float x = x;
		public readonly float deltaX = deltaX;
		public readonly float y = y;
		public readonly float deltaY = deltaY;
		public readonly float pressure = pressure;
	}

	/// <summary>
	/// The conditions for the button to be active, this is only marked as a bitflag for convenience, probably don't try to use it as one, it wouldn't really work
	/// </summary>
	[Flags]
	public enum Conditional : byte
	{
		/// <summary>
		/// Was the action pressed during this frame?
		/// </summary>
		Press = 1 << 0,

		/// <summary>
		/// Was the action pressed and held?
		/// </summary>
		LongPress = 1 << 1,

		/// <summary>
		/// Was the action released during this frame?
		/// </summary>
		Release = 1 << 2,

		/// <summary>
		/// Is the action being held?
		/// </summary>
		Continuous = 1 << 3,

		/// <summary>
		/// Was the action pressed and quickly released?
		/// </summary>
		Tap = 1 << 4,

		/// <summary>
		/// Was the action tapped twice in quick succession?
		/// </summary>
		DoubleTap = 1 << 5,

		/// <summary>
		/// Is the action being Continuously tapped?
		/// </summary>
		Mash = 1 << 6,

		/// <summary>
		/// Is the action tapped?
		/// </summary>
		Toggle = 1 << 7,

		DoubleTapToggle = DoubleTap | Toggle,

		ReleaseToggle = Release | Toggle,

		LongPressToggle = LongPress | Toggle,

		All = Press | LongPress | Release | Continuous | Tap | DoubleTap | Mash | Toggle,

		[Hide]
		None = 0,
	}

	[Flags]
	public enum Modifiers : byte
	{
		LShift = 1 << 0,
		LCtrl = 1 << 1,
		LMeta = 1 << 2,
		//why not lol
		LWin = LMeta,
		LAlt = 1 << 3,
		RAlt = 1 << 4,
		RMeta = 1 << 5,
		RWin = RMeta,
		RCtrl = 1 << 6,
		RShift = 1 << 7,
		None = 0,
	}

	public enum ButtonCode
	{
		[Hide]
		BUTTON_CODE_INVALID = -1,
		[Hide]
		BUTTON_CODE_NONE,
		[Hide]
		BUTTON_CODE_FIRST = 0,
		[Hide]
		KEY_FIRST = 0,
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
		[Hide]
		KEY_LAST = 313,
		[Hide]
		MOUSE_FIRST,
		MouseLeft = 314,
		MouseRight,
		MouseMiddle,
		MouseBack,
		MouseForward,
		MouseWheelUp,
		MouseWheelDown,
		[Hide]
		MOUSE_LAST = MouseWheelDown,
		[Hide]
		MOUSE_COUNT = 7,
		[Hide]
		JOYSTICK_FIRST = 321,
		[Hide]
		JOYSTICK_FIRST_BUTTON = 321,
		[Hide]
		JOYSTICK_LAST_BUTTON = 448,
		[Hide]
		JOYSTICK_FIRST_POV_BUTTON,
		[Hide]
		JOYSTICK_LAST_POV_BUTTON = 464,
		[Hide]
		JOYSTICK_FIRST_AXIS_BUTTON,
		[Hide]
		JOYSTICK_LAST_AXIS_BUTTON = 512,
		[Hide]
		JOYSTICK_LAST = 512,
		[Hide]
		BUTTON_CODE_COUNT,
		[Hide]
		BUTTON_CODE_LAST = 512,
		[Hide]
		KEY_XBUTTON_UP = 449,
		[Hide]
		KEY_XBUTTON_RIGHT,
		[Hide]
		KEY_XBUTTON_DOWN,
		[Hide]
		KEY_XBUTTON_LEFT,
		[Hide]
		KEY_XBUTTON_A = 321,
		[Hide]
		KEY_XBUTTON_B,
		[Hide]
		KEY_XBUTTON_X,
		[Hide]
		KEY_XBUTTON_Y,
		[Hide]
		KEY_XBUTTON_LEFT_SHOULDER,
		[Hide]
		KEY_XBUTTON_RIGHT_SHOULDER,
		[Hide]
		KEY_XBUTTON_BACK,
		[Hide]
		KEY_XBUTTON_START,
		[Hide]
		KEY_XBUTTON_STICK1,
		[Hide]
		KEY_XBUTTON_STICK2,
		[Hide]
		KEY_XBUTTON_INACTIVE_START,
		[Hide]
		KEY_XSTICK1_RIGHT = 465,
		[Hide]
		KEY_XSTICK1_LEFT,
		[Hide]
		KEY_XSTICK1_DOWN,
		[Hide]
		KEY_XSTICK1_UP,
		[Hide]
		KEY_XBUTTON_LTRIGGER,
		[Hide]
		KEY_XBUTTON_RTRIGGER,
		[Hide]
		KEY_XSTICK2_RIGHT,
		[Hide]
		KEY_XSTICK2_LEFT,
		[Hide]
		KEY_XSTICK2_DOWN,
		[Hide]
		KEY_XSTICK2_UP
	}

	internal struct KeyState()
	{
#if USE_32BIT_FLOATS_FOR_TIME
			//64 bitssssss yeahhhhhhhhhhhhhhhhh babyyyyyyy
			ulong m_Shitmask;
			
			/// <summary>
			/// Gets the time since the key's state changed
			/// </summary>
			public readonly float TimeSinceStateChange
			{
				get => (Down) ? TimeSinceReleased : TimeSincePressed;
			}

			/// <summary>
			/// Gets the time since the key's been pressed
			/// </summary>
			public float TimeSincePressed
			{
				readonly get
				{
					return Time.Now - Unsafe.BitCast<uint, float>(((uint)(m_Shitmask >> k_PressedTimeBitOffset)) & k_AwesomeFloat);
				}

				internal set
				{
					m_Shitmask = (m_Shitmask & ~k_PressedTimeMask) | ((ulong)(Unsafe.BitCast<float, uint>(value) & k_AwesomeFloat) << k_PressedTimeBitOffset);
				}
			}

			/// <summary>
			/// Gets the time since the key's been released
			/// </summary>
			public float TimeSinceReleased
			{
				readonly get
				{
					return Time.Now - Unsafe.BitCast<uint, float>(((uint)(m_Shitmask >> k_ReleasedTimeBitOffset)) & k_AwesomeFloat);
				}

				internal set
				{
					m_Shitmask = (m_Shitmask & ~k_ReleasedTimeMask) | ((ulong)(Unsafe.BitCast<float, uint>(value) & k_AwesomeFloat) << k_ReleasedTimeBitOffset);
				}
			}

			/// <summary>
			/// Is key currently being held?
			/// </summary>
			public bool Down
			{
				readonly get
				{
					return (m_Shitmask & k_DownFlagMask) != 0;
				}

				internal set
				{
					var last = Down;

					m_Shitmask = ((m_Shitmask & ~k_DownFlagMask) | Convert.ToUInt64(value));

					StateChanged |= last ^ Down;

					if (Down)
					{
						TimeSincePressed = Time.Now;
					}
					else
					{
						TimeSinceReleased = Time.Now;
					}
				}
			}

			/// <summary>
			/// Has key's state recently changed?
			/// </summary>
			public bool StateChanged
			{
				readonly get
				{
					return (m_Shitmask & k_JustChangedFlagMask) != 0;
				}

				internal set
				{
					m_Shitmask = ((m_Shitmask & ~k_JustChangedFlagMask) | (Convert.ToUInt64(value) << k_JustChangedMaskBitOffset));
				}
			}

			/// <summary>
			/// Has key been released this frame?
			/// </summary>
			public readonly bool Released => (StateChanged && !Down);

			/// <summary>
			/// Has key been pressed this frame?
			/// </summary>
			public readonly bool Pressed => (StateChanged && Down);

			const ulong k_DownFlagMask           = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000001u;

			const ulong k_JustChangedFlagMask  = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000010u;
			const int k_JustChangedMaskBitOffset = 1;

			//we only need 2 store 31 bits, we can discard the sign bit, because we know that it'll be 0 (positive)
			const ulong k_ReleasedTimeMask = 0b00000000_00000000_00000000_00000001_11111111_11111111_11111111_11111100u;
			const int k_ReleasedTimeBitOffset = 2;

			const ulong k_PressedTimeMask  = 0b11111111_11111111_11111111_11111110_00000000_00000000_00000000_00000000u;
			const int k_PressedTimeBitOffset = 33;

			const uint k_AwesomeFloat = 0b01111111_11111111_11111111_11111111u;
#else
		public readonly Half ChangedStateFor => Down ? PressedFor : ReleasedFor;

		public Half ReleasedFor
		{
			readonly get
			{
				return Unsafe.BitCast<ushort, Half>((ushort)((m_Shitmask >> k_ReleasedTimeBitOffset) & k_HalfClearMask));
			}

			set
			{
				m_Shitmask = (m_Shitmask & ~k_ReleasedTimeMask) | ((uint)Unsafe.BitCast<Half, ushort>(value) & k_HalfClearMask) << k_ReleasedTimeBitOffset;
			}
		}

		public Half PressedFor
		{
			readonly get
			{
				return Unsafe.BitCast<ushort, Half>((ushort)((m_Shitmask >> k_PressedTimeBitOffset) & k_HalfClearMask));
			}

			set
			{
				m_Shitmask = (m_Shitmask & ~k_PressedTimeMask) | (((uint)Unsafe.BitCast<Half, ushort>(value) & k_HalfClearMask) << k_PressedTimeBitOffset);
			}
		}

		public bool Down
		{
			readonly get
			{
				return (m_Shitmask & k_DownFlagMask) != 0;
			}

			internal set
			{
				var last = Down;

				m_Shitmask = ((m_Shitmask & ~k_DownFlagMask) | (Convert.ToUInt32(value)));

				StateChanged |= (last ^ Down);
			}
		}

		internal bool StateChanged
		{
			readonly get
			{
				return (m_Shitmask & k_JustChangedFlagMask) != 0;
			}

			set
			{
				m_Shitmask = ((m_Shitmask & ~k_JustChangedFlagMask) | (Convert.ToUInt32(value) << k_JustChangedFlagBitOffset));
			}
		}

		public readonly bool Released => !Down && StateChanged;

		public readonly bool Pressed => Down && StateChanged;

		// R: released time bits
		// P: pressed time bits
		// D: down bits
		// S: state changed bits
		// PPPPPPPP_PPPPPPPR_RRRRRRRR_RRRRRRSD
		uint m_Shitmask;

		const uint k_DownFlagMask = 0b00000000_00000000_00000000_000000001u;

		const uint k_JustChangedFlagMask = 0b00000000_00000000_00000000_00000010u;
		const int k_JustChangedFlagBitOffset = 1;

		//we only need 2 store 15 bits, we can discard the sign bit, because we know it'll be positive (0)
		const uint k_ReleasedTimeMask = 0b00000000_00000001_11111111_11111100u;
		const int k_ReleasedTimeBitOffset = 2;

		const uint k_PressedTimeMask = 0b11111111_11111110_00000000_00000000u;
		const int k_PressedTimeBitOffset = 9;

		const uint k_HalfClearMask = 0b00000000_00000000_01111111_11111111u;
#endif
	}

	internal struct ControllerButtonState
	{
		public Half ReleasedFor
		{
			readonly get
			{
				return Unsafe.BitCast<ushort, Half>((ushort)((m_InternalBitmask >> k_ReleasedTimeBitOffset) & k_HalfClearMask));
			}

			set
			{
				m_InternalBitmask = (m_InternalBitmask & ~k_ReleasedTimeMask) | (((uint)Unsafe.BitCast<Half, ushort>(value) & k_HalfClearMask) << k_ReleasedTimeBitOffset);
			}
		}

		public Half PressedFor
		{
			readonly get
			{
				return Unsafe.BitCast<ushort, Half>((ushort)((m_InternalBitmask >> k_PressedTimeBitOffset) & k_HalfClearMask));
			}

			set
			{
				m_InternalBitmask = (m_InternalBitmask & ~k_PressedTimeMask) | (((uint)Unsafe.BitCast<Half, ushort>(value) & k_HalfClearMask) << k_PressedTimeBitOffset);
			}
		}

		public bool StateChanged
		{
			readonly get
			{
				return (m_InternalBitmask & k_JustChangedFlagMask) != 0;
			}

			set
			{
				m_InternalBitmask = (m_InternalBitmask & ~k_JustChangedFlagMask) | ((uint)Unsafe.BitCast<bool, byte>(value) << k_JustChangedFlagBitOffset);
			}
		}

		public bool Down
		{
			readonly get
			{
				return (m_InternalBitmask & k_DownFlagMask) != 0;
			}

			set
			{
				m_InternalBitmask = (m_InternalBitmask & ~k_DownFlagMask) | ((uint)Unsafe.BitCast<bool, byte>(value));
			}
		}

		public readonly bool Pressed => Down && StateChanged;

		public readonly bool Released => !Down && StateChanged;

		//super cool bitmask, how original
		//R: released time bits
		//P: pressed time bits
		//D: down bits
		//S: state changed bits
		//PPPPPPPP_PPPPPPPR_RRRRRRRR_RRRRRRSD
		uint m_InternalBitmask;

		const uint k_DownFlagMask = 0b00000000_00000000_00000000_00000001u;

		const uint k_JustChangedFlagMask = 0b00000000_00000000_00000000_00000010u;
		const int k_JustChangedFlagBitOffset = 1;

		//we only need 2 store 15 bits, we can discard the sign bit, because we know it'll be positive (0)
		const uint k_ReleasedTimeMask = 0b00000000_00000001_11111111_11111100u;
		const int k_ReleasedTimeBitOffset = 2;

		const uint k_PressedTimeMask = 0b11111111_11111110_00000000_00000000u;
		const int k_PressedTimeBitOffset = 9;

		const uint k_HalfClearMask = 0b00000000_00000000_01111111_11111111u;
	}

	internal struct ControllerAnalogState
	{
		//is it worth storing as a float? idk :)
		public float value;
		public float delta;
	}
}
