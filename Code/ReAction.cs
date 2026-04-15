using System.ComponentModel;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		static ReAction()
		{
			InitialiseKeyMap();
		}

		static HashSet<ButtonCode> m_PressedKeys = new();
		static HashSet<ButtonCode> m_ReleasedKeys = new();

		static HashSet<ButtonAction> m_AllActions = new();
		static HashSet<ButtonAction> m_EnabledActions = new();

		static HashSet<string> m_Sets = ["general"];
		static Dictionary<ButtonAction.Bind, float> m_TappedButtons = new();
		static HashSet<string> m_ActiveSets = ["general"];

		/// <summary>
		/// Like <see cref="Input.AnalogLook"/> but good
		/// </summary>
		public static Angles Look => m_AnalogLook;

		static Angles m_AnalogLook;

		public static Modifiers ActiveModifiers
		{
			get; private set;
		}

		static void OnGameButton(ButtonCode scanCode, bool pressed)
		{
			keyStates[(int)scanCode].Down = pressed;

			switch (scanCode)
			{
				case ButtonCode.KEY_LSHIFT:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.LShift) : (ActiveModifiers & ~Modifiers.LShift);
					break;
				case ButtonCode.KEY_LCONTROL:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.LCtrl) : (ActiveModifiers & ~Modifiers.LCtrl);
					break;
				case ButtonCode.KEY_LWIN:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.LMeta) : (ActiveModifiers & ~Modifiers.LMeta);
					break;
				case ButtonCode.KEY_LALT:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.LAlt) : (ActiveModifiers & ~Modifiers.LAlt);
					break;
				case ButtonCode.KEY_RALT:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.RAlt) : (ActiveModifiers & ~Modifiers.RAlt);
					break;
				case ButtonCode.KEY_RWIN:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.RMeta) : (ActiveModifiers & ~Modifiers.RMeta);
					break;
				case ButtonCode.KEY_RCONTROL:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.RCtrl) : (ActiveModifiers & ~Modifiers.RCtrl);
					break;
				case ButtonCode.KEY_RSHIFT:
					ActiveModifiers = pressed ? (ActiveModifiers | Modifiers.RShift) : (ActiveModifiers & ~Modifiers.RShift);
					break;
				default:
				break;
			}

			if (pressed)
			{
				foreach (var action in m_EnabledActions)
				{
					if (action.Primary.Key == scanCode)
					{
						CheckPressedKey(action, ref action.m_Primary);
					}

					if (action.Secondary.Key == scanCode)
					{
						CheckPressedKey(action, ref action.m_Secondary);
					}
				}
			}
			else
			{
				foreach (var action in m_EnabledActions)
				{
					if (action.Primary.Key == scanCode)
					{
						CheckReleasedKey(action, ref action.m_Primary);
					}

					if (action.Secondary.Key == scanCode)
					{
						CheckReleasedKey(action, ref action.m_Secondary);
					}
				}
			}

			static void CheckPressedKey(ButtonAction action, ref ButtonAction.Bind bind)
			{
				action.ConditionalsState |= Conditional.Press;

				action.ConditionalsState |= Conditional.Continuous;

				if (keyStates[(int)bind.Key].TimeSinceReleased < bind.TimeOut)
				{
					action.ConditionalsState |= Conditional.Mash;
				}

				action.ConditionalsState ^= Conditional.Toggle;
			}

			static void CheckReleasedKey(ButtonAction action, ref ButtonAction.Bind bind)
			{
				action.ConditionalsState |= Conditional.Release;

				bind.LongPressed = false;

				if (keyStates[(int)bind.Key].TimeSincePressed < bind.TimeOut)
				{
					action.ConditionalsState |= Conditional.Tap;

					if (m_TappedButtons.TryGetValue(bind, out var time))
					{
						if (time < bind.TimeOut && !bind.DoubleTapped)
						{
							action.ConditionalsState |= Conditional.DoubleTap;

							bind.DoubleTapped = true;
						}
						else
						{
							time = bind.TimeOut;

							bind.DoubleTapped = false;
						}
					}
					else
					{
						m_TappedButtons[bind] = Time.Now;
					}
				}

				action.ConditionalsState &= ~Conditional.Continuous;
			}
		}

		static void RegisterButtonAction(ButtonAction buttonAction)
		{
			foreach (var action in m_AllActions)
			{
				if (buttonAction.Name == action.Name)
				{
					return;
				}
			}

			m_AllActions.Add(buttonAction);

			m_Sets.Add(buttonAction.Set);

			UpdateActionEnabled(buttonAction);
		}

		static void UnregisterButtonAction(ButtonAction buttonAction)
		{
			m_AllActions.Remove(buttonAction);
			m_EnabledActions.Remove(buttonAction);
		}

		static void UpdateActionEnabled(ButtonAction buttonAction)
		{
			if (buttonAction.Enabled)
			{
				if (m_ActiveSets.Contains(buttonAction.Set))
				{
					m_EnabledActions.Add(buttonAction);
				}
			}
			else
			{
				m_EnabledActions.Remove(buttonAction);

				buttonAction.ConditionalsState = Conditional.None;
			}
		}

		static void RefreshActionLists()
		{
			foreach (var action in m_EnabledActions)
			{
				action.ConditionalsState &= ~Conditional.Press;

				action.ConditionalsState &= ~Conditional.Release;

				action.ConditionalsState &= ~Conditional.Tap;

				action.ConditionalsState &= ~Conditional.LongPress;

				action.ConditionalsState &= ~Conditional.DoubleTap;

				CheckMash(action, ref action.m_Primary);
				CheckMash(action, ref action.m_Secondary);

				CheckLongPress(action, ref action.m_Primary);
				CheckLongPress(action, ref action.m_Secondary);
			}

			static void CheckLongPress(ButtonAction action, ref ButtonAction.Bind bind)
			{
				if (bind.TimeOut == 0)
				{
					return;
				}

				if (keyStates[(int)bind.Key].TimeSincePressed >= bind.TimeOut && !bind.LongPressed)
				{
					action.ConditionalsState |= Conditional.LongPress;

					bind.LongPressed = true;
				}
			}

			static void CheckMash(ButtonAction action, ref ButtonAction.Bind bind)
			{
				if (bind.TimeOut == 0)
				{
					return;
				}

				if (keyStates[(int)bind.Key].TimeSinceStateChange >= bind.TimeOut)
				{
					action.ConditionalsState &= ~Conditional.Mash;
				}
			}
		}

		internal static ButtonAction CreateAction(string name, ButtonAction.Bind primary, ButtonAction.Bind secondary, string set, bool enabled, Conditional allowedConditionals)
		{
			ButtonAction action = new(name, primary, secondary, set, enabled, allowedConditionals);
			RegisterButtonAction(action);

			return action;
		}

		public static ButtonAction[] GetAllActions()
		{
			return m_AllActions.ToArray();
		}

		public static ButtonAction[] GetEnabledActions()
		{
			return m_EnabledActions.ToArray();
		}

		public static string[] GetActiveSets()
		{
			return m_ActiveSets.ToArray();
		}

		public static ButtonAction GetAction(string actionName, bool complainOnMissing = false)
		{
			foreach (var action in m_AllActions)
			{
				if (action.Name == actionName)
				{
					return action;
				}
			}

			if (complainOnMissing)
			{
				ReActionLogger.Warning($"{actionName} does not exists, lmao");
			}

			return null;
		}

		public static void SetActionSetActive(string setName, bool active)
		{
			if (setName != "general")
			{
				if (m_Sets.Contains(setName))
				{
					if (active)
					{
						m_ActiveSets.Add(setName);

						foreach (var action in m_AllActions)
						{
							if (action.Set == setName)
							{
								m_EnabledActions.Add(action);
							}
						}
					}
					else
					{
						m_ActiveSets.Remove(setName);

						foreach (var action in m_AllActions)
						{
							if (action.Set == setName)
							{
								m_EnabledActions.Remove(action);
							}
						}
					}
				}
			}
		}

		static void ProcessAnalogLook()
		{
			m_AnalogLook = default;

			if (!Input.MouseCursorVisible)
			{
				if (Input.GetAnalog(InputAnalog.RightStickX) == 0 && Input.GetAnalog(InputAnalog.RightStickY) == 0)
				{
					m_AnalogLook = new Angles(Input.MouseDelta.y * Preferences.Sensitivity, -Input.MouseDelta.x * Preferences.Sensitivity, 0f);

					if (Preferences.InvertMousePitch)
					{
						m_AnalogLook.pitch = -m_AnalogLook.pitch;
					}

					if (Preferences.InvertMouseYaw)
					{
						m_AnalogLook.yaw = -m_AnalogLook.yaw;
					}
				}
				else
				{
					m_AnalogLook = new Angles(Input.GetAnalog(InputAnalog.RightStickY) * Time.Delta * Preferences.ControllerLookPitchSpeed, -(Input.GetAnalog(InputAnalog.RightStickX) * Time.Delta * Preferences.ControllerLookYawSpeed), 0f);
				}
			}
		}

		internal static void Frame()
		{
			ReinitialiseKeyStates();

			ProcessAnalogLook();
		}

		internal static void FrameEnd()
		{
			RefreshActionLists();
		}
	}
}