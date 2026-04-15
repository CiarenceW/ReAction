namespace ReActionPlugin
{
	public class ReActionSystem : GameObjectSystem<ReActionSystem>
	{
		public ReActionSystem(Scene scene) : base(scene)
		{
			Listen(Stage.StartUpdate, int.MinValue, ReAction.Frame, "ReActionFrameStart");
			Listen(Stage.FinishUpdate, int.MaxValue, ReAction.FrameEnd, "ReActionFrameEnd");
		}
	}
}