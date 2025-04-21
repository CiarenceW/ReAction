using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ReInput
{
	public static class ReInputMenu
	{
		[Menu("Editor", "ReInput/RegenerateKeyCodeThing")]
		public static void OpenMyMenu()
		{
			Regenerate();
		}

		[Menu("Editor", "ReInput/ConvertS&BoxActionsToReInputActions")]
		public static void ConvertActions()
		{
			//this is a fucking hackjob lmao, but it works!! whatever!!!

			Sandbox.FileSystem.Data.CreateDirectory("ReInput");

			HashSet<ReInput.Action> convertedActions = new();

			var actions = Sandbox.Input.GetActions();

			InputAction action;

			var flippedDic = new Dictionary<string, ReInput.KeyCode>();

			foreach (var keyVal in ReInput.keyCodeToString)
			{
				if (keyVal.Value == null)
				{
					ReInputLogger.Info($"{keyVal.Key}'s value is null");
				}
				else
				if (!flippedDic.ContainsKey(keyVal.Value.ToUpperInvariant()))
				{
					flippedDic.Add(keyVal.Value.ToUpperInvariant(), keyVal.Key);
				}
			}

			for (int actionIndex = 0; actionIndex < actions.Count(); actionIndex++)
			{
				action = actions.ElementAt(actionIndex);

				var convertedAction = new ReInput.Action(action.Name, actionIndex, flippedDic[action.KeyboardCode.ToUpperInvariant()], (ReInput.GamepadInput)action.GamepadCode, true, ReInput.Modifiers.None, ReInput.Conditional.Press, action.GroupName);
				convertedActions.Add(convertedAction);
			}

			Sandbox.FileSystem.Data.WriteJson("ReInput/convertedActions.json", convertedActions);
		}

		private static void Regenerate()
		{
			var buttonCodeType = typeof(Sandbox.Input).Assembly.GetTypes().Where((t) => t.Name == "ButtonCode").First();

			var enumValuesArray = Enum.GetValues(buttonCodeType);

			var codeToStringInfo = typeof(Sandbox.Input).Assembly.GetTypes().Where((t) => t.Name == "InputSystem").First().GetMethod("CodeToString", (BindingFlags)int.MaxValue);

			using (FileStream fs = new FileStream(Path.Combine(Project.Current.GetRootPath(), "piss.txt"), FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.AutoFlush = false;
					sw.WriteLine("{");

					foreach (var value in enumValuesArray)
					{
						if (value.ToString() == ReInput.KeyCode.KEY_LAST.ToString() || value.GetHashCode() > 313)
						{
							continue;
						}

						sw.WriteLine("\t" + "{ " + $"KeyCode.{value.ToString()}, \"{codeToStringInfo.Invoke(null, [value]).ToString()}\"" + " },");
					}

					sw.WriteLine("};");
					sw.Flush();
				}
			}
		}
	}
}