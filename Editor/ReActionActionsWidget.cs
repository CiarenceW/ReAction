#if SANDBOX
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Editor;
using Microsoft.CodeAnalysis;
using ReAction.Editor;

namespace ReAction
{
	[Dock("Editor", "ReAction Actions", "view_list")]
    public class ReActionActionsWidget : Widget
    {
		private Widget ActionsTree { get; set; }

		ScrollArea Scroller;

		public static Checkbox exportAsConsts;

		public const string filePath = "ReAction/actions.json";
		public const string defaultFilePath = "ReAction/defaultActions.json";

		public ReActionActionsWidget(Widget parent) : base(parent, false)
		{
			Layout = Layout.Column();

			Name = "ReActionActions";

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

				ReAction.CreateDefaultActions();
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

			exportIndexConstsButton.ToolTip = "Exports all actions' indices to a .cs file, for use with ReAction.ActionTriggered(int)";

			exportAsConsts.ToolTip = "If true, the indices will be public const ints, instead of public static readonly ints";

			exportIndexConstsButton.Clicked += ReActionMenu.ExportIndexToFile;
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
			Sandbox.FileSystem.Data.CreateDirectory("ReAction");

			if (!Project.Current.Config.TryGetMeta<ReActionSettings>("ReActionActions", out var meta))
			{
				meta = new ReActionSettings();
			}

			meta.Actions = ReAction.Actions;

			ReAction.SaveToFile(meta);
		}

		void SaveActionsAsNewDefault()
		{
			global::Editor.FileSystem.ProjectSettings.CreateDirectory("ReAction");
			if (!Project.Current.Config.TryGetMeta<ReActionSettings>("ReActionActions", out var meta))
			{
				meta = new ReActionSettings();
			}

			meta.Actions = ReAction.Actions;

			global::Editor.FileSystem.ProjectSettings.WriteJson(ReAction.defaultFilePath, meta.Serialize());

			Project.Current.Config.SetMeta("ReActionActions", null);
		}

		public void UpdateActionList()
		{
			ActionsTree.Layout.Clear(true);

			string lastGroup = null;
			foreach (var group in ReAction.Actions.GroupBy(x => x.Category))
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
				var name = string.IsNullOrEmpty(entry.Text) ? $"Action {ReAction.Actions.Count}" : entry.Text;
				AddAction(new ReAction.Action(name, ReAction.Actions.Count, ReAction.KeyCode.KEY_NONE, ReAction.GamepadInput.None, ReAction.Conditional.Press, lastGroup ?? "Other"), updateDisplay: true);
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
				ReAction.CreateDefaultActions(true);
				UpdateActionList();
			};
		}

		public void AddAction(ReAction.Action action, bool updateDisplay = true)
		{
			ReAction.Actions.Add(action);

			if (updateDisplay)
				UpdateActionList();
		}

		public void RemoveAction(ReAction.Action action)
		{
			ReAction.Actions.Remove(action);

			UpdateActionList();
		}

		public void ClearActions()
		{
			Log.Info("Clearing actions");

			ReAction.Actions.Clear();
		}

		[EditorEvent.Hotload]
		void OnHotLoad()
		{
			Layout.Clear(true);
			CreateLayout();
		}
	}
}
#endif