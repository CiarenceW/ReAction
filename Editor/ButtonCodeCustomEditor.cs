using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReActionPlugin
{
	[CustomEditor(typeof(ReActionPlugin.ButtonCode))]
	class ButtonCodeCustomEditor : ControlWidget
	{
		readonly static MethodInfo VirtualKeyToButtonCodeMethod = typeof(Sandbox.Input).Assembly.GetType("NativeEngine.InputSystem", true).GetMethod("VirtualKeyToButtonCode", BindingFlags.Static | BindingFlags.NonPublic);

		bool isTrapping;

		Button coolAssButton;

		public ButtonCodeCustomEditor(SerializedProperty property) : base(property)
		{
			base.Layout = Layout.Column();
			base.Layout.Spacing = 2;

			coolAssButton = new Button(((ButtonCode)property.As.Int).GetString(), "input");

			coolAssButton.MouseClick += OnMouse;

			Layout.Add(coolAssButton);
		}

		void OnMouse()
		{
			if (isTrapping)
			{
				//gets ignored otherwise :(
				OnBindPressed(ButtonCode.MouseLeft);
			}
			else
			{
				isTrapping = true;

				coolAssButton.Text = "trapping....";
			}
		}

		protected override void OnMouseReleased(MouseEvent e)
		{
			//fucking ARM
			//e.Button is a bitmask, however, the values are in the same order as the mouse codes in ButtonCode, so we can do some epic friggin intrinsincs!! epic win
			if (Bmi1.IsSupported)
				OnBindPressed((ButtonCode)(Bmi1.TrailingZeroCount((uint)e.Button) + (uint)ButtonCode.MOUSE_FIRST));
			else
			if (ArmBase.IsSupported)
				OnBindPressed((ButtonCode)(31u - (ArmBase.LeadingZeroCount((int)e.Button)) + (int)ButtonCode.MOUSE_FIRST));
			else
				Log.Info("fuck you");
		}

		protected override void OnKeyRelease(KeyEvent e)
		{
			OnBindPressed((ButtonCode)VirtualKeyToButtonCodeMethod.Invoke(null, [(int)e.NativeKeyCode]));
		}

		void OnBindPressed(ButtonCode button)
		{
			if (isTrapping)
			{
				//force the bind to be assigned to the thing. doesn't actually apply the key otherwise and I can't be bothered to find out why
				if (SerializedProperty.Parent.IsValid() && SerializedProperty.Parent.ParentProperty.Parent.IsValid())
				{
					bool isPrimaryBind = SerializedProperty.Parent.ParentProperty.Name == "Primary";

					ButtonAction action = (ButtonAction)SerializedProperty.Parent.ParentProperty.Parent.Targets.First();

					ButtonAction.Bind bind = isPrimaryBind ? action.Primary : action.Secondary;
					bind.Key = button;

					coolAssButton.Text = button.GetString();

					if (isPrimaryBind)
					{
						action.Primary = bind;
					}
					else
					{
						action.Secondary = bind;
					}

					isTrapping = false;
				}
			}
		}

		//gets the engine display string which in turns ""localises"" the key for your layout, otherwise shows blanks if you input keys that aren't alphanumeric
		static string ConvolutedAssNativeButtonCodeToString(int nativeKeyCode)
		{
			//I don't want this shit in the non-editor stuff
			return ((ButtonCode)VirtualKeyToButtonCodeMethod.Invoke(null, [nativeKeyCode])).GetString();
		}
	}
}
