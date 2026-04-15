using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		public static bool KeyPressed(ButtonCode buttonCode)
		{
			return keyStates[(int)buttonCode].Pressed;
		}

		public static bool KeyReleased(ButtonCode buttonCode)
		{
			return keyStates[(int)buttonCode].Released;
		}

		public static bool KeyDown(ButtonCode buttonCode)
		{
			return keyStates[(int)buttonCode].Down;
		}
	}
}
