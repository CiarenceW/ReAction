using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Editor;

namespace ReActionPlugin.Editor
{
	[Dock( "Editor", "ReAction Actions", "view_list" )]
	public class ReActionActionsWidget : Widget
	{
		private Widget ActionsTree { get; set; }

		ScrollArea Scroller;

		public static Checkbox exportAsConsts;

		static Button saveActionsButton;

		internal static bool actionsNeedSaving;

		public const string filePath = "ReAction/actions.json";
		public const string defaultFilePath = "ReAction/defaultActions.json";

		const string k_SaveActionsButtonName = "Save actions";
		const string k_SaveActionsButtonNeedsSavingName = k_SaveActionsButtonName + "* (needs saving)";

		public ReActionActionsWidget( Widget parent ) : base( parent, false )
		{
			Layout = Layout.Column();

			Name = "ReActionActions";

			Layout.Margin = 4;
			Layout.Spacing = 4;

			CreateLayout();
		}

		void CreateLayout()
		{
			Scroller = new ScrollArea( this );
			Scroller.Name = "Scroller";
			Scroller.Layout = Layout.Column();
			Scroller.FocusMode = FocusMode.None;

			Layout.Add( Scroller, 1 );

			Scroller.Canvas = new Widget( Scroller );
			Scroller.Canvas.Name = "ScrollerCanvas";
			Scroller.Canvas.Layout = Layout.Column();
			Scroller.Canvas.Layout.Spacing = 0;

			var mainColumn = Scroller.Canvas.Layout.AddColumn();

			mainColumn.Spacing = 4;
			{
				ActionsTree = new Widget( null );
				ActionsTree.Layout = Layout.Column();
				ActionsTree.Name = "ActionsTree";
				ActionsTree.Layout.Margin = 2;
				ActionsTree.Layout.Spacing = 2;

				ActionsTree.OnPaintOverride += OnPaintOverride;

				Name = "Main Column";

				mainColumn.Add( ActionsTree );

				ReAction.LoadDefaultActions();
				UpdateActionList();
			}
			;

			Layout.AddSeparator();

			saveActionsButton = Layout.Add( new Button( actionsNeedSaving ? k_SaveActionsButtonNeedsSavingName : k_SaveActionsButtonName, "logout", this ) );

			var exportIndexConstsButton = Layout.Add( new Button.Primary( "Export action indices", "open_in_new", this ) );

			exportAsConsts = Layout.Add( new Checkbox( "Export indices as const", this ) );

			exportAsConsts.Value = true;

			saveActionsButton.Clicked += Save;

			saveActionsButton.ToolTip = "Saves the actions. Will get saved along with all the other ProjectSettings.";

			exportIndexConstsButton.ToolTip = "Exports all actions' names to a .cs file, for use with ReAction.GetAction(string).";

			exportAsConsts.ToolTip = "If true, the indices will be public const ints, instead of public static readonly ints.";

			exportIndexConstsButton.Clicked += ReActionMenu.ExportIndexToFile;
		}

		public static void SetActionsNeedSaving( bool needSaving )
		{
			if ( saveActionsButton.IsValid() )
			{
				if ( needSaving )
				{
					saveActionsButton.Text = k_SaveActionsButtonNeedsSavingName;
					actionsNeedSaving = true;
				}
				else
				{
					saveActionsButton.Text = k_SaveActionsButtonName;
					actionsNeedSaving = false;
				}
			}
		}

		new bool OnPaintOverride()
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.ControlBackground.WithAlpha( 0.5f ) );
			Paint.DrawRect( ActionsTree.LocalRect );
			return true;
		}

		void Save()
		{
			Save( null );
		}

		[Event( "scene.saved" )]
		void Save( Scene _ )
		{
			Sandbox.FileSystem.Data.CreateDirectory( "ReAction" );

			if ( !Project.Current.Config.TryGetMeta<ReActionSettings>( "ReActionActions", out var meta ) )
			{
				meta = new ReActionSettings();
			}

			meta.Actions = ReAction.GetAllActions().ToHashSet();

			EditorUtility.SaveProjectSettings<ReActionSettings>( meta, "ReAction/defaultActions.json" );

			SetActionsNeedSaving( false );
		}

		public void UpdateActionList()
		{
			ActionsTree.Layout.Clear( true );

			string lastGroup = null;
			int actionCount = 0;
			foreach ( var group in ReAction.GetAllActions().GroupBy( x => x.Set ) )
			{
				var collapsibleCategory = ActionsTree.Layout.Add( new CollapsibleCategory( null, group.Key ) { Name = $"Group {group.First().Set}" } );

				foreach ( var action in group )
				{
					collapsibleCategory.Container.Layout.Add( new ActionPanel( action, this ) { Name = $"ActionsPanel {actionCount++}" } );
				}

				collapsibleCategory.StateCookieName = $"inputpage.category.{group.Key}";
				lastGroup = group.Key;
			}

			ActionsTree.Layout.AddSpacingCell( 2 );

			var footer = ActionsTree.Layout.AddRow();
			footer.Margin = new( 4, 1, 4, 3 );
			footer.Spacing = 4;

			var entry = footer.Add( new LineEdit() { MaximumHeight = 24, PlaceholderText = "Add New Action..." }, 2 );

			entry.ReturnPressed += add;

			var btn = footer.Add( new Button.Primary( "Add", "new_label" ), 0 );
			btn.Clicked += add;

			var reset = footer.Add( new Button( "Reset to Default", "restart_alt" ) );
			reset.Clicked += () =>
			{
				ClearActions();
				ReAction.LoadDefaultActions();
				UpdateActionList();

				SetActionsNeedSaving( true );
			};

			void add()
			{
				var name = string.IsNullOrEmpty( entry.Text ) ? $"Action {ReAction.GetAllActions().Length}" : entry.Text;

				ReAction.CreateAction( name, default, default );

				UpdateActionList();

				SetActionsNeedSaving( true );
			}
		}

		public void RemoveAction( ButtonAction action )
		{
			ReAction.UnregisterButtonAction( action );

			UpdateActionList();

			SetActionsNeedSaving( true );
		}

		public void ClearActions()
		{
			Log.Info( "Clearing actions" );

			foreach ( var action in ReAction.GetAllActions() )
			{
				ReAction.UnregisterButtonAction( action );
			}

			SetActionsNeedSaving( true );
		}

		/*[EditorEvent.Hotload]
		void OnHotLoad()
		{
			Layout.Clear(true);
			CreateLayout();
		}*/
	}
}
