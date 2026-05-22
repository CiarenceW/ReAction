using System;
using System.Collections.Generic;
using System.Reflection;
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
	}

}
