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

		delegate void OnControllerAxisDelegate(int deviceId, Controller.ControllerAxis axis, int value);

		delegate void CoubleArouble(OnControllerAxisDelegate originalMethod, int deviceId, Controller.ControllerAxis axis, int value);

		delegate void OnControllerButtonDelegate(int deviceId, Controller.ControllerButton button, bool down);

		delegate void CoubleBrouble(OnControllerButtonDelegate originalMethod, int deviceId, Controller.ControllerButton button, bool down);

		[SkipHotload]
		static object onKey_Hook;

		[SkipHotload]
		static object onMouse_Hook;

		[SkipHotload]
		static object onControllerAxis_Hook;

		[SkipHotload]
		static object onControllerButton_Hook;

		[SkipHotload] static StartTrappingDelegate StartTrappingKeys;

#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
		[ModuleInitializer]
		//Create a hook with MonoMod.RuntimeDetour from Sandbox.Engine.InputRouter.OnKey, so, we don't have to bother with the input processing the game does, real raw input! yay!
		//also allows us to get the scan code, and the "real" key code, nice for input within UI and shit :)
		internal static void Main()
		{
			var runtimeDetourAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault((asm) => asm.FullName.Contains("MonoMod.RuntimeDetour"));

			var hookType = runtimeDetourAssembly.GetType("MonoMod.RuntimeDetour.Hook");

			var inputRouterType = Assembly.GetAssembly(typeof(Input)).GetType("Sandbox.Engine.InputRouter");

			var inputRouter_OnKey_MethodBase = inputRouterType.GetMethod("OnKey", BindingFlags.NonPublic | BindingFlags.Static);

			onKey_Hook = Activator.CreateInstance(hookType, [inputRouter_OnKey_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnButtonHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<DoubleTrouble>()]);

			var inputRouter_OnMouseButton_MethodBase = inputRouterType.GetMethod("OnMouseButton", BindingFlags.NonPublic | BindingFlags.Static);

			onMouse_Hook = Activator.CreateInstance(hookType, [inputRouter_OnMouseButton_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnMouseButtonHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<MoubleBrouble>()]);

			var inputRouter_OnGameControllerAxis_MethodBase = inputRouterType.GetMethod("OnGameControllerAxis", BindingFlags.NonPublic | BindingFlags.Static);

			onControllerAxis_Hook = Activator.CreateInstance(hookType, [inputRouter_OnGameControllerAxis_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnControllerAxisHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<CoubleArouble>()]);

			var inputRouter_OnGameControllerButton_MethodBase = inputRouterType.GetMethod("OnGameControllerButton", BindingFlags.NonPublic | BindingFlags.Static);

			onControllerButton_Hook = Activator.CreateInstance(hookType, [inputRouter_OnGameControllerButton_MethodBase, typeof(ReAction).GetMethod(nameof(ReActionOnControllerButtonHook), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<CoubleBrouble>()]);

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

		static void ReActionOnControllerAxisHook(OnControllerAxisDelegate originalMethod, int deviceId, Controller.ControllerAxis axis, int value)
		{
			originalMethod(deviceId, axis, value);
		}

		static void ReActionOnControllerButtonHook(OnControllerButtonDelegate originalMethod, int deviceId, Controller.ControllerButton button, bool down)
		{
			originalMethod(deviceId, button, down);
		}
	}
}
