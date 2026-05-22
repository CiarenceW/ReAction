using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		public static void StartTrappingInput(Action<string[]> keyCallback)
		{
			StartTrappingKeys(keyCallback);
		}
	}
}
