#if SANDBOX

namespace ReActionPlugin.Editor
{
	[CustomEditor(typeof(ReAction.Action))]
	public class ActionCustomEditor : ControlObjectWidget
	{
		public ActionCustomEditor(SerializedProperty property) : base(property, false)
		{
			Layout = Layout.Row();
			Layout.Spacing = 4;

			var piss = new Label("Yeah don't do that LOL");

			Layout.Add(piss);
		}
	}
}
#endif