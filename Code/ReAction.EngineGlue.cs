using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		[SkipHotload] static readonly CodeToStringDelegate CodeToString = typeof(Input).Assembly.GetType("NativeEngine.InputSystem").GetMethod("CodeToString", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate<CodeToStringDelegate>();
		[SkipHotload] static readonly GetKeyDisplayNameDelegate GetKeyDisplayName = typeof(Input).Assembly.GetType("NativeEngine.InputSystem").GetMethod("GetKeyDisplayName", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate<GetKeyDisplayNameDelegate>();
		[SkipHotload] static readonly StringToCodeDelegate StringToCode = typeof(Input).Assembly.GetType("NativeEngine.InputSystem").GetMethod("StringToButtonCode", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate<StringToCodeDelegate>();

		/// <summary>
		/// Gets the internal engine name for the key
		/// </summary>
		public static string GetEngineString(this ButtonCode buttonCode) => CodeToString(buttonCode);

		/// <summary>
		/// Gets the locale key name for your keyboard, for example, on QWERTY "W" will return "W", but on AZERTY "W" will return "Z"
		/// </summary>
		/// <param name="buttonCode"></param>
		/// <returns>The locale key name for your keyboard, or null, depending on the key</returns>
		public static string GetKeyDisplay(this ButtonCode buttonCode) => GetKeyDisplayName(buttonCode);

		/// <summary>
		/// Returns <see cref="GetKeyDisplay(ButtonCode)"/> or <see cref="GetEngineString(ButtonCode)"/> if the former is null.
		/// </summary>
		/// <param name="buttonCode"></param>
		/// <returns></returns>
		public static string GetString(this ButtonCode buttonCode) => string.IsNullOrWhiteSpace(GetKeyDisplayName(buttonCode)) ? CodeToString(buttonCode) : GetKeyDisplayName(buttonCode);

		public static ButtonCode GetCodeForString(string keyName) => StringToCode(keyName);

		const short k_JoystickAxisMin = short.MinValue;
		const short k_JoystickAxisMax = short.MaxValue;

		unsafe static delegate* unmanaged<nint, int> SDL_GetNumGamepadTouchpadsDelegate = (delegate* unmanaged<nint, int>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetNumGamepadTouchpads");

		unsafe static delegate* unmanaged<nint, int, int> SDL_GetNumGamepadTouchpadFingerDelegate = (delegate* unmanaged<nint, int, int>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetNumGamepadTouchpadFingers");

		unsafe static delegate* unmanaged<nint, int, int, bool*, float*, float*, float*, bool> SDL_GetGamepadTouchpadFingerDelegate = (delegate* unmanaged<nint, int, int, bool*, float*, float*, float*, bool>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetGamepadTouchpadFinger");

		unsafe static delegate* unmanaged<int, nint> SDL_GetGamepadFromIDDelegate = (delegate* unmanaged<int, nint>)NativeLibrary.GetExport(GetSDLLibraryHandle(), "SDL_GetGamepadFromID");

		static nint GetSDLLibraryHandle()
		{
			var currentProcModules = Process.GetCurrentProcess().Modules;

			foreach (ProcessModule module in currentProcModules)
			{
				if (module.ModuleName.Contains("SDL"))
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

		internal static int SDL_GetNumGamepadTouchpads(nint gamepad)
		{
			unsafe
			{
				return SDL_GetNumGamepadTouchpadsDelegate(gamepad);
			}
		}

		internal static int SDL_GetNumGamepadTouchpadFingers(nint gamepad, int touchpad)
		{
			unsafe
			{
				return SDL_GetNumGamepadTouchpadFingerDelegate(gamepad, touchpad);
			}
		}

		internal static unsafe bool SDL_GetGamepadTouchpadFinger(nint gamepad, int touchpad, int finger, bool* down, float* x, float* y, float* pressure)
		{
			return SDL_GetGamepadTouchpadFingerDelegate(gamepad, touchpad, finger, down, x, y, pressure);
		}

		internal static nint SDL_GetGamepadFromID(int instance_id)
		{
			unsafe
			{
				return SDL_GetGamepadFromIDDelegate(instance_id);
			}
		}

		static int controllerCount = 0;

		//SDL controllers all have device ids that start above 0 (with 0 being an invalid device)
		//on top of that, all device ids are unique per connection, if you plug in a controller, then unplug it, then replug it again, it'll have a different id, thus, we need some bullshit array to make it kind of nicer to work with smile
		internal static int GetControllexIndexForDeviceId(int deviceId)
		{
			for (int i = 0; i < extraPerControllerData.Length; i++)
			{
				if (extraPerControllerData[i].deviceId == deviceId)
				{
					return extraPerControllerData[i].deviceId;
				}
			}

			//0 means a handle is invalid
#if DEBUG
			ReActionLogger.Error($"GetControllerIndexForDeviceId: DeviceID {deviceId} was invalid");
#endif

			return 0;
		}
	}
}
