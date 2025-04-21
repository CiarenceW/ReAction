using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReInput
{
	public class ReInputSystem : GameObjectSystem<ReInputSystem>
	{
		public ReInputSystem(Scene scene) : base(scene)
		{
			ReInput.Init();
			Listen(Stage.StartUpdate, -10, ReInput.PollInput, "ReInputPollInput");
		}
	}
}
