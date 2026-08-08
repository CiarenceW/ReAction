using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		unsafe static delegate* unmanaged<nint, int> SDL_GetNumGamepadTouchpadsDelegate = (delegate* unmanaged<nint, int>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetNumGamepadTouchpads");

		unsafe static delegate* unmanaged<nint, int, int> SDL_GetNumGamepadTouchpadFingerDelegate = (delegate* unmanaged<nint, int, int>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetNumGamepadTouchpadFingers");

		unsafe static delegate* unmanaged<nint, int, int, bool*, float*, float*, float*, bool> SDL_GetGamepadTouchpadFingerDelegate = (delegate* unmanaged<nint, int, int, bool*, float*, float*, float*, bool>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetGamepadTouchpadFinger");

		unsafe static delegate* unmanaged<int, nint> SDL_GetGamepadFromIDDelegate = (delegate* unmanaged<int, nint>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetGamepadFromID");

		[SkipHotload] readonly static Dictionary<Controller, Controller.TouchpadData[][]> m_ControllerTouchpadDataDictionary = new();

		static nint GetSDLLibraryHandle()
		{
			var currentProcModules = Process.GetCurrentProcess().Modules;

			foreach (ProcessModule module in currentProcModules)
			{
				if (module.ModuleName.StartsWith("SDL"))
				{
					return module.BaseAddress;
				}
			}

			return nint.Zero;
		}

#pragma warning disable IDE1006 // Naming Styles
		[ConCmd]
		static void debug_sdl_touchpad_bindings()
		{
			ReActionLogger.Info($"SDL handle: {GetSDLLibraryHandle()}");
		}

		[ConCmd]
		static void debug_log_touchpads()
		{
			foreach (var controller in Controller.All)
			{
				ReActionLogger.Info($"{controller.Name} - id: {SDL_GetGamepadFromID(controller.SDLHandle)}");
				ReActionLogger.Info($"touchpads: {controller.GetTouchpadCount()}");

				for (int touchpadIndex = 0; touchpadIndex < controller.GetTouchpadCount(); touchpadIndex++)
				{
					ReActionLogger.Info($"touchpad {touchpadIndex} max fingers: {controller.GetMaxTouchpadFingers(touchpadIndex)}");

					for (int fingerIndex = 0; fingerIndex < controller.GetMaxTouchpadFingers(touchpadIndex); fingerIndex++)
					{
						var data = controller.GetTouchpadData(touchpadIndex, fingerIndex);

						ReActionLogger.Info($"finger {fingerIndex} - down?: {data.down}, x: {data.x}, x delta: {data.deltaX}, y: {data.y}, y delta: {data.deltaY}, pressure: {data.pressure}");
					}
				}
			}
		}
#pragma warning restore IDE1006 // Naming Styles

		static int SDL_GetNumGamepadTouchpads(nint gamepad)
		{
			unsafe
			{
				return SDL_GetNumGamepadTouchpadsDelegate(gamepad);
			}
		}

		static int SDL_GetNumGamepadTouchpadFingers(nint gamepad, int touchpad)
		{
			unsafe
			{
				return SDL_GetNumGamepadTouchpadFingerDelegate(gamepad, touchpad);
			}
		}

		static unsafe bool SDL_GetGamepadTouchpadFinger(nint gamepad, int touchpad, int finger, bool* down, float* x, float* y, float* pressure)
		{
			return SDL_GetGamepadTouchpadFingerDelegate(gamepad, touchpad, finger, down, x, y, pressure);
		}

		static nint SDL_GetGamepadFromID(int instance_id)
		{
			unsafe
			{
				return SDL_GetGamepadFromIDDelegate(instance_id);
			}
		}

		static void ProcessControllersTouchpads()
		{
			foreach (var controller in Controller.All)
			{
				var touchpadCount = controller.GetTouchpadCount();

				//initialise arrays
				if (!m_ControllerTouchpadDataDictionary.ContainsKey(controller))
				{
					var touchpadData_Array = new Controller.TouchpadData[touchpadCount][];

					for (int touchpadIndex = 0; touchpadIndex < touchpadCount; touchpadIndex++)
					{
						touchpadData_Array[touchpadIndex] = new Controller.TouchpadData[controller.GetMaxTouchpadFingers(touchpadIndex)];
					}

					m_ControllerTouchpadDataDictionary[controller] = touchpadData_Array;
				}

				if (touchpadCount > 0)
				{
					for (int touchpadIndex = 0; touchpadIndex < touchpadCount; touchpadIndex++)
					{
						var fingerCount = controller.GetMaxTouchpadFingers(touchpadIndex);

						for (int fingerIndex = 0; fingerIndex < fingerCount; fingerIndex++)
						{
							unsafe
							{
								bool down = false;
								float x = 0, y = 0, pressure = 0;

								if (SDL_GetGamepadTouchpadFinger(SDL_GetGamepadFromID(controller.DeviceId), touchpadIndex, fingerIndex, &down, &x, &y, &pressure))
								{
									//lmao
									ref var touchpadData = ref m_ControllerTouchpadDataDictionary[controller][touchpadIndex][fingerIndex];

									touchpadData = new Controller.TouchpadData(down, x, x - touchpadData.x, y, y - touchpadData.y, pressure);
								}
							}
						}
					}
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

			public int GetTouchpadCount() => SDL_GetNumGamepadTouchpads(SDL_GetGamepadFromID(SDLHandle));

			public int GetMaxTouchpadFingers(int touchpad) => SDL_GetNumGamepadTouchpadFingers(SDL_GetGamepadFromID(SDLHandle), touchpad);

			public TouchpadData GetTouchpadData(int touchpad, int finger) => m_ControllerTouchpadDataDictionary[this][touchpad][finger];

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

			public enum ControllerButton
			{
				None = -1,
				A,
				B,
				X,
				Y,
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
				Paddle1,
				Paddle2,
				Paddle3,
				Paddle4,
				Touchpad,
				[Hide]
				MAX
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

			public readonly struct TouchpadData(bool down, float x, float deltaX, float y, float deltaY, float pressure)
			{
				public readonly bool down = down;
				public readonly float x = x;
				public readonly float deltaX = deltaX;
				public readonly float y = y;
				public readonly float deltaY = deltaY;
				public readonly float pressure = pressure;
			}
		}
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
	}
}
