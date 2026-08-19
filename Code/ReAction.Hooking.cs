using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		const string k_ReActionHookName = "ReActionOnButtonHook";

		delegate string GetKeyDisplayNameDelegate(ButtonCode buttonCode);
		delegate string CodeToStringDelegate(ButtonCode buttonCode);
		delegate ButtonCode StringToCodeDelegate(string pString);

		delegate void StartTrappingDelegate(Action<string[]> onTrappedKeysCallback);

		delegate void OnKeyDelegate(ButtonCode scanButtonCode, ButtonCode keyButtonCode, bool down, bool repeat, int ikeymods);

		delegate void DoubleTrouble(OnKeyDelegate originalMethod, ButtonCode scanButtonCode, ButtonCode keyButtonCode, bool down, bool repeat, int ikeymods);

		delegate void OnMouseDelegate(ButtonCode button, bool down, int ikeymods);

		delegate void MoubleBrouble(OnMouseDelegate originalMethod, ButtonCode button, bool down, int ikeymods);

		delegate void OnControllerAxisDelegate(int deviceId, ControllerAxis axis, int value);

		delegate void CoubleArouble(OnControllerAxisDelegate originalMethod, int deviceId, ControllerAxis axis, int value);

		delegate void OnControllerButtonDelegate(int deviceId, ControllerButton button, bool down);

		delegate void CoubleBrouble(OnControllerButtonDelegate originalMethod, int deviceId, ControllerButton button, bool down);

		delegate void OnControllerConnectedDelegate(int joystickId, int deviceId);

		delegate void ICanCallThisAnythingAndItWontMatterLolExclamationPointSmile(OnControllerConnectedDelegate originalMethod, int joystickId, int deviceId);

		delegate void OnControllerDisconnectedDelegate(int joystickId);

		delegate void SuperAwesomeReallyDescriptDelegateName(OnControllerDisconnectedDelegate originalMethod, int joystickId);

		[SkipHotload] static object onKey_Hook;

		[SkipHotload] static object onMouse_Hook;

		[SkipHotload] static object onControllerAxis_Hook;

		[SkipHotload] static object onControllerButton_Hook;

		[SkipHotload] static object onControllerConnected_Hook;

		[SkipHotload] static object onControllerDisconnected_Hook;

		[SkipHotload] static StartTrappingDelegate StartTrappingKeys;

		static bool m_Initialised = false;

#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
		[ModuleInitializer]
		//Create a hook with MonoMod.RuntimeDetour from Sandbox.Engine.InputRouter.OnKey, so, we don't have to bother with the input processing the game does, real raw input! yay!
		//also allows us to get the scan code, and the "real" key code, nice for input within UI and shit :)
		internal static void Main()
		{
			//this runs everytime ReAction is hotloaded, but hotloading our hook objects will cause s&box to crash (sorry!), the hooks do persist, so we can just keep track of whether or not we've already initialised with a bool
			if (!m_Initialised)
			{
				var runtimeDetourAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault((asm) => asm.FullName.Contains("MonoMod.RuntimeDetour"));

				var hookType = runtimeDetourAssembly.GetType("MonoMod.RuntimeDetour.Hook");

				var inputRouterType = Assembly.GetAssembly(typeof(Input)).GetType("Sandbox.Engine.InputRouter");

				var inputRouter_OnKey_MethodBase = inputRouterType.GetMethod("OnKey", BindingFlags.NonPublic | BindingFlags.Static);

				//new Hook(MethodBase from, MethodInfo to);

				onKey_Hook = Activator.CreateInstance(hookType, [inputRouter_OnKey_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnButtonHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<DoubleTrouble>()]);

				var inputRouter_OnMouseButton_MethodBase = inputRouterType.GetMethod("OnMouseButton", BindingFlags.NonPublic | BindingFlags.Static);

				onMouse_Hook = Activator.CreateInstance(hookType, [inputRouter_OnMouseButton_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnMouseButtonHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<MoubleBrouble>()]);

				var inputRouter_OnGameControllerAxis_MethodBase = inputRouterType.GetMethod("OnGameControllerAxis", BindingFlags.NonPublic | BindingFlags.Static);

				onControllerAxis_Hook = Activator.CreateInstance(hookType, [inputRouter_OnGameControllerAxis_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnControllerAxisHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<CoubleArouble>()]);

				var inputRouter_OnGameControllerButton_MethodBase = inputRouterType.GetMethod("OnGameControllerButton", BindingFlags.NonPublic | BindingFlags.Static);

				onControllerButton_Hook = Activator.CreateInstance(hookType, [inputRouter_OnGameControllerButton_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnControllerButtonHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<CoubleBrouble>()]);

				var inputRouter_OnGameControllerConnected_MethodBase = inputRouterType.GetMethod("OnGameControllerConnected", BindingFlags.NonPublic | BindingFlags.Static);

				onControllerConnected_Hook = Activator.CreateInstance(hookType, [inputRouter_OnGameControllerConnected_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnControllerConnected), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<ICanCallThisAnythingAndItWontMatterLolExclamationPointSmile>()]);

				var inputRouter_OnGameControllerDisconnected_MethodBase = inputRouterType.GetMethod("OnGameControllerDisconnected", BindingFlags.NonPublic | BindingFlags.Static);

				onControllerDisconnected_Hook = Activator.CreateInstance(hookType, [inputRouter_OnGameControllerDisconnected_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnControllerDisconnected), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<SuperAwesomeReallyDescriptDelegateName>()]);
			}

			m_Initialised = true;

			/*var globalContextType = typeof(WorldInput).Assembly.GetType("Sandbox.Engine.GlobalContext");

			var currentContext = globalContextType.GetProperty("Current", BindingFlags.Static | BindingFlags.Public).GetValue(null);

			var inputContext = globalContextType.GetProperty("InputContext", BindingFlags.Instance | BindingFlags.Public).GetValue(currentContext);

			StartTrappingKeys = (StartTrappingDelegate)Delegate.CreateDelegate(typeof(StartTrappingDelegate), inputContext, inputContext.GetType().GetMethod("StartTrapping"), true);*/
		}
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries

		static void ReActionOnButtonHook(OnKeyDelegate originalMethod, ButtonCode scanButtonCode, ButtonCode keyButtonCode, bool down, bool repeat, int ikeymods)
		{
			if (!repeat)
			{
				OnGameButton(scanButtonCode, down);
			}

			originalMethod(scanButtonCode, keyButtonCode, down, repeat, ikeymods);
		}

		static void ReActionOnMouseButtonHook(OnMouseDelegate originalMethod, ButtonCode button, bool down, int ikeymods)
		{
			OnGameButton(button, down);

			originalMethod(button, down, ikeymods);
		}

		static void ReActionOnControllerAxisHook(OnControllerAxisDelegate originalMethod, int deviceId, ControllerAxis axis, int value)
		{
			//thumbstick axis values range from -32768 to 32767, while trigger axis values range from 0 to 32767, cheeky branchless epicsauce awesomeness!!!!!!!!! I almost feel like a real programmer.......
			float remappedAxisValue = MathX.LerpInverse(value, k_JoystickAxisMin * Unsafe.BitCast<bool, byte>(axis < ControllerAxis.TriggerLeft), k_JoystickAxisMax);

			//i packed the ControllerAxis stuff into the ControllerButton enum, so that you can select in menus and stuff
			OnControllerAxis(deviceId, axis, remappedAxisValue);

			originalMethod(deviceId, axis, value);
		}

		static void ReActionOnControllerButtonHook(OnControllerButtonDelegate originalMethod, int deviceId, ControllerButton button, bool down)
		{
			OnControllerButton(deviceId, button, down);

			originalMethod(deviceId, button, down);
		}

		static void ReActionOnControllerConnected(OnControllerConnectedDelegate originalMethod, int joystickId, int deviceId)
		{
			OnControllerConnected(joystickId, deviceId);

			originalMethod(joystickId, deviceId);
		}

		static void ReActionOnControllerDisconnected(OnControllerDisconnectedDelegate originalMethod, int joystickId)
		{
			OnControllerDisconnected(joystickId);

			originalMethod(joystickId);
		}
	}
}
