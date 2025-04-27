namespace ReAction
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;


#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UIElements;
	using UnityEngine;
	using UnityEngine.UIElements;

#endif

	public
#if SANDBOX
		static
#endif
		class ReActionMenu
#if UNITY_EDITOR
		: EditorWindow
#endif
	{

#if UNITY_EDITOR
		private static Toggle exportAsConsts;
#endif


		public static void ExportIndexToFile()
		{
			string path = Path.Combine(
#if SANDBOX
Project.Current.GetCodePath()
#elif UNITY_EDITOR
				Application.dataPath
#endif
, "ReActionConsts.cs");
			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.AutoFlush = false;
					sw.WriteLine("namespace ReAction.Consts.Int");
					sw.WriteLine("{");
					sw.WriteLine("\tpublic static class ReActionConsts");
					sw.WriteLine("\t{");

					foreach (var action in ReAction.Actions)
					{
						string intLine;
						string intDeclaration;
#if SANDBOX
						if (ReActionActionsWidget.exportAsConsts.Value)
#elif UNITY_EDITOR
						if (exportAsConsts.value)
#endif
						{
							intDeclaration = $"public const int {string.Concat(action.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))}";
						}
						else
						{
							intDeclaration = $"public static readonly int {string.Concat(action.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))}";
						}

						intLine = $"\t\t{intDeclaration} = {action.Index};";

						sw.WriteLine(intLine);
					}

					sw.WriteLine("\t}");
					sw.WriteLine("}");

					sw.WriteLine();

					sw.WriteLine("namespace ReAction.Consts.String");
					sw.WriteLine("{");
					sw.WriteLine("\tpublic static class ReActionConsts");
					sw.WriteLine("\t{");

					foreach (var action in ReAction.Actions)
					{
						string stringLine;
						string stringDeclaration;
#if SANDBOX
						if (ReActionActionsWidget.exportAsConsts.Value)
#elif UNITY_EDITOR
						if (exportAsConsts.value)
#endif
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

#if SANDBOX
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

				var convertedAction = new ReAction.Action(action.Name, actionIndex, flippedDic[action.KeyboardCode.ToUpperInvariant()], (ReAction.GamepadInput)action.GamepadCode, true, ReAction.Conditional.Press, action.GroupName);
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
#elif UNITY_EDITOR

		[SerializeField]
		private VisualTreeAsset m_VisualTreeAsset = default;

		[SerializeField]
		private StyleSheet m_StyleSheet = default;

		private ScrollView scrollView;

		[MenuItem("Window/ReAction Actions")]
		public static void ShowExample()
		{
			ReActionMenu wnd = GetWindow<ReActionMenu>();
			wnd.titleContent = new GUIContent("ReAction Actions");
		}

		public void CreateGUI()
		{
			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			scrollView = new ScrollView(ScrollViewMode.Vertical);
			root.Add(scrollView);

			ReAction.CreateDefaultActions();
			UpdateListDisplay();
		}

		void UpdateListDisplay()
		{
			if (ReAction.Actions?.Count() == 0)
			{
				return;
			}

			scrollView.Clear();

			string lastCategory = null;
			Foldout foldout = null;
			int num = 0;
			var reOrderedActions = ReAction.Actions.OrderBy(x => x.Category);
			bool first = true;
			for (int actionIndex = 0; actionIndex < reOrderedActions.Count(); actionIndex++)
			{
				var action = reOrderedActions.ElementAt(actionIndex);

				if (action.Category != lastCategory)
				{
					foldout = new Foldout
					{
						name = action.Category,
						text = action.Category
					};

					foldout.styleSheets.Add(m_StyleSheet);
					foldout.AddToClassList("category-foldout");

					if (first)
					{
						foldout.AddToClassList("first");
						first = false;
					}

					scrollView.Add(foldout);
				}

				var label = new Label
				{
					name = action.Name,
					text = $"{action.Name}: {(action.GamepadInput != ReAction.GamepadInput.None ? $"🎮 {action.GamepadInput}" : "")} {(action.Key != KeyCode.None ? $"⌨️ {action.Key}" : "")} {(action.SecondaryKey != KeyCode.None ? $"⌨️ {action.SecondaryKey}" : "")}",
				};

				label.styleSheets.Add(m_StyleSheet);

				label.AddToClassList("action-label");

				if (num % 2 == 0)
				{
					label.AddToClassList("even");
				}
				else
				{
					label.AddToClassList("odd");
				}

				label.AddManipulator(new Clickable((evt) =>
				{
					ShowActionContextualMenu(action);
				}));

				label.AddManipulator(new ContextualMenuManipulator((evt) =>
				{
					evt.menu.AppendAction("Remove", (e) => RemoveAction(action));
					evt.menu.AppendAction("Edit", (e) => ShowActionContextualMenu(action));
				}));

				foldout.Add(label);

				lastCategory = action.Category;

				num++;
			}

			if (foldout != null)
			{
				foldout.AddToClassList("last");
			}
	

			var box = new Toolbar()
			{
				name = "bottom box"
			};

			box.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
			box.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceAround);

			scrollView.Add(box);

			var newActionName = new TextField()
			{
				name = "newActionName",
			};

			newActionName.style.width = new StyleLength()
			{
				value = new Length()
				{
					unit = LengthUnit.Percent,
					value = 100 / 2
				}
			};

			box.Add(newActionName);

			var newActionButton = new Button()
			{
				name = "newActionButton",
				text = "Add"
			};

			newActionButton.style.backgroundColor = new Color(0.4f, 0.4f, 1f);

			newActionButton.clicked += () =>
			{
				CreateNewAction(new ReAction.Action(string.IsNullOrWhiteSpace(newActionName.text) ? $"New Action {ReAction.Actions.Count}" : newActionName.text, ReAction.Actions.Count, KeyCode.None, ReAction.GamepadInput.None, ReAction.Modifiers.None, ReAction.Conditional.Press, "Other"), true);
			};

			newActionButton.style.width = new StyleLength()
			{
				value = new Length()
				{
					unit = LengthUnit.Percent,
					value = 100 / 4
				}
			};

			box.Add(newActionButton);

			var resetButton = new Button(() =>
			{
				ReAction.CreateDefaultActions(true);
				UpdateListDisplay();
			})
			{
				name = "resetButton",
				text = "Reset",
				tooltip = "Resets to the saved default actions"
			};

			resetButton.style.width = new StyleLength()
			{
				value = new Length()
				{
					unit = LengthUnit.Percent,
					value = 100 / 4
				}
			};

			box.Add(resetButton);

			var saveButton = new Button(ReAction.SaveToFile)
			{
				name = "saveButton",
				text = "Save actions",
				tooltip = "Saves the actions locally, in your persistentDataPath, this is just to let you know that there's a built-in way to do this"
			};

			scrollView.Add(saveButton);

			var saveAsDefault = new Button(SaveActionsAsNewDefault)
			{
				name = "saveAsDefaultsButton",
				text = "Save actions as new default",
				tooltip = "Saves the actions for the game itself, this is only available in the editor, and is how you'll get your actions packaged with your game"

			};

			scrollView.Add(saveAsDefault);

			var exportAsConstsBox = new Box()
			{
				name = "exportAsContsBox"
			};

			exportAsConstsBox.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
			exportAsConstsBox.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceAround);

			scrollView.Add(exportAsConstsBox);

			exportAsConsts = new Toggle("Export as consts?")
			{
				name = "exportAsConstsToggle",
				tooltip = "If this is true, the actions' names and indices will be saved as conts, else, they'll be saved as static readonly"
			};

			exportAsConstsBox.Add(exportAsConsts);

			var exportAsConstsButton = new Button(ExportIndexToFile)
			{
				name = "exportAsConstsButton",
				text = "Export actions names and indices to file",
				tooltip = "Exports the actions' names and indices to a ReActionConsts.cs file, in the root of your Assets folder, this is just for conveniance, you can avoid typos that way"
			};

			exportAsConstsButton.style.flexGrow = 1f;

			exportAsConstsBox.Add(exportAsConstsButton);
		}

		void SaveActionsAsNewDefault()
		{
			var defaultPath = Path.Combine(Application.dataPath, ReAction.defaultFilePath);
			Directory.CreateDirectory(Path.Combine(Application.dataPath, "ReAction"));

			using (FileStream fs = new FileStream(defaultPath, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.Write(ReAction.SaveToJSONSkullEmojiFuckYouUnity());
				}
			}

			Debug.Log(defaultPath);
		}

		void CreateNewAction(ReAction.Action action, bool updateDisplay = true)
		{
			ReAction.Actions.Add(action);

			if (updateDisplay)
				UpdateListDisplay();
		}

		void RemoveAction(ReAction.Action action)
		{
			ReAction.Actions.Remove(action);

			UpdateListDisplay();
		}

		void ShowActionContextualMenu(ReAction.Action action)
		{
			var window = (EditorWindow)ScriptableObject.CreateInstance(typeof(EditorWindow));
			window.name = action.Name;
			window.titleContent = new GUIContent($"{action.Category}: {action.Name}");

			var windowRoot = window.rootVisualElement;

			foreach (var property in action.GetType().GetProperties((BindingFlags)int.MaxValue))
			{
				if (property.SetMethod != null)
				{
					var propertyType = property.PropertyType;

					if (propertyType == typeof(string))
					{
						var stringTextBox = new TextField(property.Name)
						{
							value = property.GetValue(action).ToString(),
							name = propertyType.Name
						};

						stringTextBox.RegisterCallback<ChangeEvent<string>>((piss) =>
						{
							property.SetValue(action, piss.newValue);
							UpdateListDisplay();
						});

						windowRoot.Add(stringTextBox);
					}
					else if (propertyType == typeof(int))
					{
						var intTextBox = new IntegerField(property.Name)
						{
							value = (int)property.GetValue(action),
							name = propertyType.Name
						};

						intTextBox.RegisterCallback<ChangeEvent<int>>((piss) =>
						{
							property.SetValue(action, piss.newValue);
							UpdateListDisplay();
						});

						windowRoot.Add(intTextBox);
					}
					else if (propertyType == typeof(bool))
					{
						var positiveAxisCheckbox = new Toggle(property.Name)
						{
							value = (bool)property.GetValue(action),
							name = property.Name,
						};

						positiveAxisCheckbox.RegisterCallback<ChangeEvent<bool>>((piss) =>
						{
							property.SetValue(action, piss.newValue);
							UpdateListDisplay();
						});

						windowRoot.Add(positiveAxisCheckbox);
					}
					else if (propertyType == typeof(float))
					{
						var floatTextBox = new FloatField(property.Name)
						{
							value = (float)property.GetValue(action),
							name = property.Name
						};

						floatTextBox.RegisterCallback<ChangeEvent<float>>((piss) =>
						{
							property.SetValue(action, piss.newValue);
							UpdateListDisplay();
						});

						windowRoot.Add(floatTextBox);
					}
					else if (propertyType.IsEnum)
					{
						var enumBox = new EnumField((Enum)Enum.GetValues(propertyType).GetValue(0))
						{
							label = property.Name,
							name = propertyType.Name,
							value = (Enum)Enum.Parse(propertyType, property.GetValue(action).ToString())
						};

						enumBox.RegisterCallback<ChangeEvent<Enum>>((piss) =>
						{
							property.SetValue(action, piss.newValue);
							UpdateListDisplay();
						});

						windowRoot.Add(enumBox);
					}
					else
					{
						var label = new Label()
						{
							name = property.Name,
							text = property.GetValue(action).ToString()
						};

						windowRoot.Add(label);
					}
				}
			}

			window.Show();
		}

#endif
	}
}