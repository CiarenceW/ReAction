using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Editor;
using Microsoft.CodeAnalysis;
using ReInput.Editor;

namespace ReInput
{
	[Dock("Editor", "ReInput Actions", "view_list")]
    public class ReInputActionsWidget : Widget
    {
		private Widget ActionsTree { get; set; }

		ScrollArea Scroller;

		private Checkbox exportAsConsts;

		public const string filePath = "ReInput/actions.json";
		public const string defaultFilePath = "ReInput/defaultActions.json";

		public ReInputActionsWidget(Widget parent) : base(parent, false)
		{
			Layout = Layout.Column();

			Name = "ReInputActions";

			Layout.Margin = 4;
			Layout.Spacing = 4;

			CreateLayout();
		}

		void CreateLayout()
		{
			//I gotta be the only person to add a scrollarea to a thing in sbox, ever, right?
			Scroller = new ScrollArea(this);
			Scroller.Name = "Scroller";
			Scroller.Layout = Layout.Column();
			Scroller.FocusMode = FocusMode.None;

			Layout.Add(Scroller, 1);

			Scroller.Canvas = new Widget(Scroller);
			Scroller.Canvas.Name = "ScrollerCanvas";
			Scroller.Canvas.Layout = Layout.Column();
			Scroller.Canvas.Layout.Spacing = 0;

			var mainColumn = Scroller.Canvas.Layout.AddColumn();

			mainColumn.Spacing = 4;
			{
				ActionsTree = new Widget(null);
				ActionsTree.Layout = Layout.Column();
				ActionsTree.Name = "ActionsTree";
				ActionsTree.Layout.Margin = 2;
				ActionsTree.Layout.Spacing = 2;

				ActionsTree.OnPaintOverride += OnPaintOverride;

				Name = "Main Column";

				mainColumn.Add(ActionsTree);

				CreateDefaultActions();
				UpdateActionList();
			};

			Layout.AddSeparator();

			var saveActionsButton = Layout.Add(new Button("Save actions", "logout", this));

			var saveActionsAsDefaultGameActionsButton = Layout.Add(new Button("Set as new game default", "lock", this));

			var exportIndexConstsButton = Layout.Add(new Button.Primary("Export action indices", "open_in_new", this));

			exportAsConsts = Layout.Add(new Checkbox("Export indices as const", this));

			saveActionsButton.Clicked += Save;

			saveActionsButton.ToolTip = "Save actions locally. This is if you like having a weird control scheme, but you prefer the default set not being fucked";

			saveActionsAsDefaultGameActionsButton.Clicked += SaveActionsAsNewDefault;

			saveActionsAsDefaultGameActionsButton.ToolTip = "Sets the current actions as the game's default keybinds";

			exportIndexConstsButton.ToolTip = "Exports all actions' indices to a .cs file, for use with ReInput.ActionTriggered(int)";

			exportAsConsts.ToolTip = "If true, the indices will be public const ints, instead of public static readonly ints";

			exportIndexConstsButton.Clicked += ExportIndexToFile;
		}

		new bool OnPaintOverride()
		{
			Paint.ClearPen();
			Paint.SetBrush(Theme.ControlBackground.WithAlpha(0.5f));
			Paint.DrawRect(ActionsTree.LocalRect);
			return true;
		}

		void Save()
		{
			Save(null);
		}

		[Event("scene.saved")]
		void Save(Scene _)
		{
			Sandbox.FileSystem.Data.CreateDirectory("ReInput");

			if (!Project.Current.Config.TryGetMeta<ReInputActionSettings>("ReInputActions", out var meta))
			{
				meta = new ReInputActionSettings();
			}

			meta.Actions = ReInput.Actions;

			Sandbox.FileSystem.Data.WriteJson(filePath, meta.Serialize());
		}

		void SaveActionsAsNewDefault()
		{
			global::Editor.FileSystem.ProjectSettings.CreateDirectory("ReInput");
			if (!Project.Current.Config.TryGetMeta<ReInputActionSettings>("ReInputActions", out var meta))
			{
				meta = new ReInputActionSettings();
			}

			meta.Actions = ReInput.Actions;

			global::Editor.FileSystem.ProjectSettings.WriteJson(ReInput.defaultFilePath, meta.Serialize());

			Project.Current.Config.SetMeta("ReInputActions", null);
		}

		void ExportIndexToFile()
		{
			using (FileStream fs = new FileStream(Path.Combine(Project.Current.GetCodePath(), "ReInputConsts.cs"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.AutoFlush = false;
					sw.WriteLine("namespace ReInput.Consts");
					sw.WriteLine("{");
					sw.WriteLine("\tpublic static class ReInputConsts");
					sw.WriteLine("\t{");
					foreach (var action in ReInput.Actions)
					{
						sw.WriteLine($"\t \t{(exportAsConsts.Value ? $"public const int {string.Concat(action.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))}" : $"public static readonly int {string.Concat(action.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries))}")} = {action.Index};");
					}
					sw.WriteLine("\t}");
					sw.Write("}");
					sw.Flush();
				}
			}
		}

		void CreateDefaultActions(bool toDefault = false)
		{
			if (ReInput.Actions.Count > 0)
				return;

			if (!toDefault)
			{
				if (Sandbox.FileSystem.Data.FileExists(filePath))
				{
					ReInputLogger.Info("Found locally saved set, applying to the thing, yeah");

					var settings = new ReInputActionSettings();
					settings.Deserialize(Sandbox.FileSystem.Data.ReadAllText(filePath));

					ReInput.Actions = settings.Actions;


					foreach (var action in ReInput.Actions)
					{
						ReInputLogger.Info($"{action.Name}: {action.Index}");
					}
					return;
				}
				else
				{
					ReInputLogger.Info("Couldn't find locally saved set");
				}
			}

			if (global::Editor.FileSystem.ProjectSettings.DirectoryExists("ReInput") && global::Editor.FileSystem.ProjectSettings.FileExists(defaultFilePath))
			{
				if (ProjectSettings.Get<ReInputActionSettings>(defaultFilePath) != null)
				{
					ReInputLogger.Info("Found default set, applying " + global::Editor.FileSystem.ProjectSettings.GetFullPath(defaultFilePath));

					ReInput.Actions = new(ProjectSettings.Get<ReInputActionSettings>(defaultFilePath).Actions);

					foreach (var action in ReInput.Actions)
					{
						ReInputLogger.Info($"{action.Name}: {action.Index}");
					}

					return;
				}
			}

			ReInputLogger.Info("Action list is null, populating with defaults");

			ReInput.Actions = new HashSet<ReInput.Action>(ReInput.DefaultActions);
		}

		public void UpdateActionList()
		{
			ActionsTree.Layout.Clear(true);

			string lastGroup = null;
			foreach (var group in ReInput.Actions.GroupBy(x => x.Category))
			{
				var collapsibleCategory = ActionsTree.Layout.Add(new CollapsibleCategory(null, group.Key) { Name = $"Group {group.First().Category}"});

				foreach (var action in group)
				{
					collapsibleCategory.Container.Layout.Add(new ActionPanel(action, this) { Name = $"ActionsPanel {action.Index}"});
				}

				collapsibleCategory.StateCookieName = $"inputpage.category.{group.Key}";
				lastGroup = group.Key;
			}

			ActionsTree.Layout.AddSpacingCell(2);

			var footer = ActionsTree.Layout.AddRow();
			footer.Margin = new(4, 1, 4, 3);
			footer.Spacing = 4;

			var entry = footer.Add(new LineEdit() { MaximumHeight = 24, PlaceholderText = "Add New Action..." }, 2);

			var add = () =>
			{
				var name = string.IsNullOrEmpty(entry.Text) ? $"Action {ReInput.Actions.Count}" : entry.Text;
				AddAction(new ReInput.Action(name, ReInput.Actions.Count, ReInput.KeyCode.KEY_NONE, ReInput.GamepadInput.None, ReInput.Modifiers.None, ReInput.Conditional.Press, lastGroup ?? "Other"), updateDisplay: true);
			};

			entry.ReturnPressed += add;

			var btn = footer.Add(new Button.Primary("Add", "new_label"), 0);
			btn.Clicked += add;

			var clear = footer.Add(new Button("Clear", "auto_fix_normal"));
			clear.Clicked += () =>
			{
				ClearActions();

				// Add one action to start out with
				add();
			};

			var reset = footer.Add(new Button("Reset to Default", "restart_alt"));
			reset.Clicked += () =>
			{
				ClearActions();
				CreateDefaultActions(true);
				UpdateActionList();
			};
		}

		public void AddAction(ReInput.Action action, bool updateDisplay = true)
		{
			ReInput.Actions.Add(action);

			if (updateDisplay)
				UpdateActionList();
		}

		public void RemoveAction(ReInput.Action action)
		{
			ReInput.Actions.Remove(action);

			UpdateActionList();
		}

		public void ClearActions()
		{
			Log.Info("Clearing actions");

			ReInput.Actions.Clear();
		}

		[EditorEvent.Hotload]
		void OnHotLoad()
		{
			Layout.Clear(true);
			CreateLayout();
		}
	}
}
