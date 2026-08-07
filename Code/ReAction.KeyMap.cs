using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		static KeyState[] keyStates;

		const ButtonCode k_ButtonCodeLength = ButtonCode.MOUSE_LAST + 1;

		static void InitialiseKeyMap()
		{
			keyStates = new KeyState[(int)k_ButtonCodeLength];

			for (int i = (int)ButtonCode.BUTTON_CODE_FIRST; i < (int)k_ButtonCodeLength; i++)
			{
				keyStates[i] = new();
			}
		}

		static void ReinitialiseKeyStates()
		{
			for (int i = 0; i < (int)k_ButtonCodeLength; i++)
			{
				var key = keyStates[i];

				key.StateChanged = false;

#if USE_32BIT_FLOATS_FOR_TIME
				key.PressedFor += Time.Delta;
#else
				key.PressedFor += (Half)Time.Delta;
#endif

#if USE_32BIT_FLOATS_FOR_TIME
				key.ReleasedFor += Time.Delta;
#else
				key.ReleasedFor += (Half)Time.Delta;
#endif
			}
		}
	}
}
