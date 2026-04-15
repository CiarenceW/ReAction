using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		static KeyState[] keyStates;

		const ButtonCode k_ButtonCodeLength = ButtonCode.MOUSE_LAST;

		static void InitialiseKeyMap()
		{
			keyStates = new KeyState[(int)k_ButtonCodeLength];

			for (ButtonCode i = ButtonCode.BUTTON_CODE_FIRST; i < k_ButtonCodeLength; i++)
			{
				keyStates[(int)i] = new();
			}
		}

		static void ReinitialiseKeyStates()
		{
			for (int i = 0; i < (int)ButtonCode.MOUSE_LAST; i++)
			{
				keyStates[i].StateChanged = false;
			}
		}
	}
}
