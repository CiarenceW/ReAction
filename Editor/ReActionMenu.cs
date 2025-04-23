namespace ReAction
{
#if SANDBOX
	public static class ReActionMenu
	{
		[Menu("Editor", "ReAction/RegenerateKeyCodeThing")]
		public static void OpenMyMenu()
		{
			Regenerate();
		}

		[Menu("Editor", "ReAction/ConvertS&BoxActionsToReActionActions")]
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

				var convertedAction = new ReAction.Action(action.Name, actionIndex, flippedDic[action.KeyboardCode.ToUpperInvariant()], (ReAction.GamepadInput)action.GamepadCode, true, ReAction.Modifiers.None, ReAction.Conditional.Press, action.GroupName);
				convertedActions.Add(convertedAction);
			}

			Sandbox.FileSystem.Data.WriteJson("ReAction/convertedActions.json", convertedActions);
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
						if (value.ToString() == ReAction.KeyCode.KEY_LAST.ToString() || value.GetHashCode() > 313)
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
#elif UNITY_EDITOR || UNITY_STANDALONE
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UIElements;

	public class ReActionMenu : EditorWindow
	{
		[SerializeField]
		private VisualTreeAsset m_VisualTreeAsset = default;

		[MenuItem("Window/UI Toolkit/ReAction")]
		public static void ShowExample()
		{
			ReActionMenu wnd = GetWindow<ReActionMenu>();
			wnd.titleContent = new GUIContent("ReActionMenu");
		}

		public void CreateGUI()
		{
			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			// VisualElements objects can contain other VisualElement following a tree hierarchy.
			VisualElement label = new Label("Hello World! From C#");
			root.Add(label);

			// Instantiate UXML
			VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
			root.Add(labelFromUXML);
		}
	}

#endif
}