using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		//not really raw input but, whateverrrrrrrr :)
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

		//REALLY not raw input but ummmmmmmmmmmmmmmmmmm :)
		public static bool ControllerButtonPressed(int deviceId, Controller.ControllerButton button)
		{
			return controllerButtonStates[GetControllexIndexForDeviceId(deviceId)][(int)button].Pressed;
		}

		public static bool ControllerButtonDown(int deviceId, Controller.ControllerButton button)
		{
			return controllerButtonStates[GetControllexIndexForDeviceId(deviceId)][(int)button].Down;
		}

		public static bool ControllerButtonReleased(int deviceId, Controller.ControllerButton button)
		{
			return controllerButtonStates[GetControllexIndexForDeviceId(deviceId)][(int)button].Released;
		}
	}
}
