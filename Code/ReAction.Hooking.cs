using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		const string k_ReActionHookName = "ReActionOnButtonHook";

#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
		[ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
		//we have to create a whole new method because NativeEngine.ButtonCode is internal, so we can't just make a compatible method
		internal static void Main()
		{
			var globalContextType = typeof(WorldInput).Assembly.GetType("Sandbox.Engine.GlobalContext");

			var currentContext = globalContextType.GetProperty("Current", BindingFlags.Static | BindingFlags.Public).GetValue(null);

			var inputContext = globalContextType.GetProperty("InputContext", BindingFlags.Instance | BindingFlags.Public).GetValue(currentContext);

			var inputContextOnGameButtonProp = inputContext.GetType().GetProperty("OnGameButton", BindingFlags.Instance | BindingFlags.Public);

			var actionsArgsTypes = inputContextOnGameButtonProp.PropertyType.GenericTypeArguments;

			//we make a new runtime type because with a DynamicMethod, sbox's hotloading system complains about the method not having a declaring type
			var reActionHookAssemblyName = new AssemblyName("ReAction.Hooking");

			var reActionHookAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(reActionHookAssemblyName, AssemblyBuilderAccess.Run);

			var reActionHookModuleBuilder = reActionHookAssemblyBuilder.DefineDynamicModule("ReAction.Hooking");

			var reActionHookTypeBuiler = reActionHookModuleBuilder.DefineType("ReActionButtonCallbackHook", TypeAttributes.Public | TypeAttributes.Class);

			var reActionHookMethodBuilder = reActionHookTypeBuiler.DefineMethod(k_ReActionHookName, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void), actionsArgsTypes);

			//sure? why not
			reActionHookMethodBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(SkipHotloadAttribute).GetConstructor(Type.EmptyTypes), []));
			reActionHookMethodBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(MethodImplAttribute).GetConstructor([typeof(MethodImplOptions)]), [MethodImplOptions.AggressiveInlining]));

			var ilGen = reActionHookMethodBuilder.GetILGenerator();

			//public static void ReActionOnButtonHook(NativeEngine.ButtonCode scanCode, string buttonName, bool pressed)
			//{
			//		ReAction.OnGameButton(scanCode, buttonName, pressed);
			//}

			ilGen.Emit(OpCodes.Ldarg_0);
			ilGen.Emit(OpCodes.Ldarg_1);
			ilGen.Emit(OpCodes.Ldarg_2);

			ilGen.EmitCall(OpCodes.Call, typeof(ReAction).GetMethod(nameof(OnGameButton), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), null);

			ilGen.Emit(OpCodes.Ret);

			reActionHookMethodBuilder.DefineParameter(0, ParameterAttributes.None, "scanCode");
			reActionHookMethodBuilder.DefineParameter(1, ParameterAttributes.None, "buttonName");
			reActionHookMethodBuilder.DefineParameter(2, ParameterAttributes.None, "pressed");

			reActionHookMethodBuilder.SetImplementationFlags(MethodImplAttributes.IL);

			reActionHookModuleBuilder.CreateGlobalFunctions();

			Type reActionHookType = reActionHookTypeBuiler.CreateType();

			var createdMethod = reActionHookType.GetMethod(k_ReActionHookName, (BindingFlags)int.MaxValue);

			var onInputMethod = createdMethod.CreateDelegate(typeof(Action<,,>).MakeGenericType(actionsArgsTypes));

			var onGameButton = inputContextOnGameButtonProp.GetValue(inputContext) as Delegate;

			inputContextOnGameButtonProp.SetValue(inputContext, Delegate.Combine(onGameButton, onInputMethod));
		}
	}
}
