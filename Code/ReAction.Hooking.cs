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

		static object onKey_Hook;

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
	}
}
