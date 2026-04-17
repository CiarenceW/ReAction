namespace ReActionPlugin.Editor
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;

	public static class ReActionMenu
	{
		public static void ExportIndexToFile()
		{
			string path = Path.Combine(Project.Current.GetCodePath(), "ReActionConsts.cs");

			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.AutoFlush = false;

					sw.WriteLine("namespace ReActionPlugin.Consts.String");
					sw.WriteLine("{");
					sw.WriteLine("\tpublic static class ReActionConsts");
					sw.WriteLine("\t{");

					foreach (var action in ReAction.GetAllActions())
					{
						string stringLine;
						string stringDeclaration;
						if (ReActionActionsWidget.exportAsConsts.Value)
						{
							stringDeclaration = $"public const string {string.Concat(action.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))}";
						}
						else
						{
							stringDeclaration = $"public static readonly string {string.Concat(action.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))}";
						}

						stringLine = $"\t\t{stringDeclaration} = \"{action.Name}\";";

						sw.WriteLine(stringLine);
					}

					sw.WriteLine("\t}");
					sw.Write("}");
					sw.Flush();
				}
			}
		}

		/*[Menu("Editor", "ReAction/ConvertS&BoxActionsToReActionActions")]
		public static void ConvertActions()
		{
			//this is a fucking hackjob lmao, but it works!! whatever!!!

			Sandbox.FileSystem.Data.CreateDirectory("ReAction");

			HashSet<ReAction.Action> convertedActions = new();

			var actions = Sandbox.Input.GetActions();

			InputAction action;

			var flippedDic = new Dictionary<string, ReAction.KeyCode>();

			foreach (var keyVal in ReAction.keyCodeToString)
			{
				if (keyVal.Value == null)
				{
					ReActionLogger.Info($"{keyVal.Key}'s value is null");
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

				var convertedAction = Activator.CreateInstance(typeof(ReAction.Action), (action.Name, actionIndex, flippedDic[action.KeyboardCode.ToUpperInvariant()], (ReAction.GamepadInput)action.GamepadCode, true, ReAction.Conditional.Press, action.GroupName)) as ReAction.Action;
				convertedActions.Add(convertedAction);
			}

			Sandbox.FileSystem.Data.WriteJson("ReAction/convertedActions.json", convertedActions);
		}*/

		/*private static void Regenerate()
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
						if (value.ToString() == ReAction.ButtonCode.KEY_LAST.ToString() || value.GetHashCode() > 313)
						{
							continue;
						}

						sw.WriteLine("\t" + "{ " + $"KeyCode.{value.ToString()}, \"{codeToStringInfo.Invoke(null, [value]).ToString()}\"" + " },");
					}

					sw.WriteLine("};");
					sw.Flush();
				}
			}
		}*/
	}
}