using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		const string k_ReActionHookName = "ReActionOnButtonHook";

#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
		[ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
		internal static void Main()
		{
			var globalContextType = typeof(WorldInput).Assembly.GetType("Sandbox.Engine.GlobalContext");

			var currentContext = globalContextType.GetProperty("Current", BindingFlags.Static | BindingFlags.Public).GetValue(null);

			var inputContext = globalContextType.GetProperty("InputContext", BindingFlags.Instance | BindingFlags.Public).GetValue(currentContext);

			var inputContextOnGameButtonProp = inputContext.GetType().GetProperty("OnGameButton", BindingFlags.Instance | BindingFlags.Public);

			var actionsArgsTypes = inputContextOnGameButtonProp.PropertyType.GenericTypeArguments;

			var onInputDMDMethod = new DynamicMethod(k_ReActionHookName, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void), actionsArgsTypes, typeof(ReAction).Module, true);

			var ilGen = onInputDMDMethod.GetILGenerator();

			ilGen.Emit(OpCodes.Ldarg_0);
			ilGen.Emit(OpCodes.Ldarg_1);

			//we don't care about the keyboard modifiers, we'll handle this shit ourselves
			//ilGen.Emit(OpCodes.Ldarg_2);

			ilGen.EmitCall(OpCodes.Call, typeof(ReAction).GetMethod(nameof(OnGameButton), BindingFlags.Static | BindingFlags.NonPublic), null);

			onInputDMDMethod.DefineParameter(1, ParameterAttributes.None, "scanCode");
			onInputDMDMethod.DefineParameter(2, ParameterAttributes.None, "pressed");
			onInputDMDMethod.DefineParameter(3, ParameterAttributes.None, "keyboardModifiers");

			var onInputDMD = onInputDMDMethod.CreateDelegate(typeof(Action<,,>).MakeGenericType(actionsArgsTypes));

			var onGameButton = inputContextOnGameButtonProp.GetValue(inputContext) as Delegate;

			inputContextOnGameButtonProp.SetValue(inputContext, Delegate.Combine(onGameButton, onInputDMD));
		}
	}
}
