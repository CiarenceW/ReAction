using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		/// <summary>
		/// Did this key start getting pressed this frame?
		/// </summary>
		public static bool KeyPressed( ButtonCode buttonCode )
		{
			return keyStates[(int)buttonCode].Pressed;
		}

		/// <summary>
		/// Did this key get released this frame?
		/// </summary>
		public static bool KeyReleased( ButtonCode buttonCode )
		{
			return keyStates[(int)buttonCode].Released;
		}

		/// <summary>
		/// Is this key being held down?
		/// </summary>
		public static bool KeyDown( ButtonCode buttonCode )
		{
			return keyStates[(int)buttonCode].Down;
		}

		//REALLY not raw input but ummmmmmmmmmmmmmmmmmm :)
		/// <summary>
		/// Did this button on the controller start getting pressed this frame?
		/// </summary>
		/// <param name="deviceId">The deviceId of the <seealso cref="Controller"/>, can be found with <seealso cref="Controller.DeviceId"/>.</param>
		/// <param name="button">The controller's button.</param>
		public static bool ControllerButtonPressed( int deviceId, ControllerButton button )
		{
			return ReAction.extraPerControllerData[GetControllexIndexForDeviceId( deviceId )].buttonState[(int)button].Pressed;
		}

		/// <summary>
		/// Is this button on the controller being held?
		/// </summary>
		/// <param name="deviceId">The deviceId of the <seealso cref="Controller"/>, can be found with <seealso cref="Controller.DeviceId"/>.</param>
		/// <param name="button">The controller's button.</param>
		public static bool ControllerButtonDown( int deviceId, ControllerButton button )
		{
			return ReAction.extraPerControllerData[GetControllexIndexForDeviceId( deviceId )].buttonState[(int)button].Down;
		}

		/// <summary>
		/// Did this button on the controller get released this frame?
		/// </summary>
		/// <param name="deviceId">The deviceId of the <seealso cref="Controller"/>, can be found with <seealso cref="Controller.DeviceId"/>.</param>
		/// <param name="button">The controller's button.</param>
		public static bool ControllerButtonReleased( int deviceId, ControllerButton button )
		{
			return ReAction.extraPerControllerData[GetControllexIndexForDeviceId( deviceId )].buttonState[(int)button].Released;
		}
	}
}
