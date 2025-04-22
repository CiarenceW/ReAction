#if SANDBOX
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReAction
{
	public class ReActionSystem : GameObjectSystem<ReActionSystem>
	{
		public ReActionSystem(Scene scene) : base(scene)
		{
			ReAction.Init();
			Listen(Stage.StartUpdate, -10, ReAction.PollInput, "ReActionPollInput");
		}
	}
}
#endif