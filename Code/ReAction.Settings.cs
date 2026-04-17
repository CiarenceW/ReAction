using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		/// <summary>
		/// Loads the project's default actions
		/// </summary>
		public static void LoadDefaultActions()
		{
			var settins = ProjectSettings.Get<ReActionSettings>("ReAction/defaultActions.json");

			foreach (var action in settins.Actions)
			{
				RegisterButtonAction(action);
			}
		}

		/// <summary>
		/// Executes the method and registers all returned actions <br/>
		/// Use this for loading the user's saved actions
		/// </summary>
		/// <param name="loadFunction"></param>
		public static void LoadUserSettings(Func<ReActionSettings> loadFunction)
		{
			var settings = loadFunction();

			foreach (var action in settings.Actions)
			{
				RegisterButtonAction(action);
			}
		}
	}

	public class ReActionSettings : ConfigData
	{
		//this gets skipped when deserialised
		public ReActionSettings()
		{
			Actions = 
			 new(
				[
					new ("Forward", new ButtonAction.Bind(ButtonCode.KEY_W, Conditional.Continuous), default, "Movement"),
					new ("Backward", new ButtonAction.Bind(ButtonCode.KEY_S, Conditional.Continuous), default, "Movement"),
					new ("Left", new ButtonAction.Bind(ButtonCode.KEY_A, Conditional.Continuous), default, "Movement"),
					new ("Right", new ButtonAction.Bind(ButtonCode.KEY_D, Conditional.Continuous), default, "Movement"),
					new ("Jump", new ButtonAction.Bind(ButtonCode.KEY_SPACE, Conditional.Press), default, "Movement"),
					new ("Run", new ButtonAction.Bind(ButtonCode.KEY_LSHIFT, Conditional.Continuous), default, "Movement"),
					new ("Walk", new ButtonAction.Bind(ButtonCode.KEY_LALT, Conditional.Continuous), default, "Movement"),
					new ("Duck", new ButtonAction.Bind(ButtonCode.KEY_LCONTROL, Conditional.Toggle), default, "Movement"),
					new ("Attack1", new ButtonAction.Bind(ButtonCode.MouseLeft, Conditional.Tap), default, "Actions"),
					new ("Attack2", new ButtonAction.Bind(ButtonCode.MouseRight, Conditional.Tap), default, "Actions"),
					new ("Reload", new ButtonAction.Bind(ButtonCode.KEY_R, Conditional.Press), default, "Actions"),
					new ("Use", new ButtonAction.Bind(ButtonCode.KEY_E, Conditional.Press), default, "Actions"),
					new ("Flashlight", new ButtonAction.Bind(ButtonCode.KEY_F, Conditional.Press), default, "Actions"),
				]
				);
		}

		public HashSet<ButtonAction> Actions { get; set; }
	}
}
