namespace ReActionPlugin
{
	public static partial class ReAction
	{
		static ReAction()
		{
			InitialiseKeyMap();
		}

		static readonly HashSet<ButtonCode> m_PressedKeys = new();
		static readonly HashSet<ButtonCode> m_ReleasedKeys = new();

		static readonly HashSet<ButtonAction> m_AllActions = new();
		static readonly HashSet<ButtonAction> m_EnabledActions = new();

		static readonly HashSet<string> m_ActiveSets = ["General"];

		/// <summary>
		/// Like <see cref="Input.AnalogLook"/> but good
		/// </summary>
		public static Angles Look => m_AnalogLook;

		static Angles m_AnalogLook;

		public static Vector3 Move { get; private set; }

		public static Modifiers ActiveModifiers { get; private set; }

		static ButtonAction MissingAction { get; } = new ButtonAction("missing", default, default, null, false, Conditional.None);

		/// <summary>
		/// Formats the modifiers' strings, like the following: <code>LShift + LMeta + </code>
		/// </summary>
		/// <param name="modifiers"></param>
		/// <returns></returns>
		public static string FormatModifiersString(Modifiers modifiers)
		{
			string str = "";

			for (byte b = 1; b > 0; b <<= 1)
			{
				if (((byte)modifiers & b) != 0)
				{
					str += (Modifiers)b + " + ";
				}
			}

			return str;
		}

		public static void OnGameButton(ButtonCode scanCode, string buttonName, bool pressed)
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

				if (keyStates[(int)bind.Key].ReleasedFor < bind.TimeOut)
				{
					action.ConditionalsState |= Conditional.Mash;
				}

				if (!((bind.Conditional & Conditional.ReleaseToggle) == Conditional.ReleaseToggle) && !((bind.Conditional & Conditional.LongPressToggle) == Conditional.LongPressToggle) && !((bind.Conditional & Conditional.DoubleTapToggle) == Conditional.DoubleTapToggle))
				{
					action.ConditionalsState ^= Conditional.Toggle;
				}
			}

			static void CheckReleasedKey(ButtonAction action, ref ButtonAction.Bind bind)
			{
				action.ConditionalsState |= Conditional.Release;

				if ((bind.Conditional & Conditional.ReleaseToggle) == Conditional.ReleaseToggle)
				{
					action.ConditionalsState ^= Conditional.Toggle;
				}

				bind.LongPressed = false;

				if (keyStates[(int)bind.Key].PressedFor < bind.TimeOut)
				{
					action.ConditionalsState |= Conditional.Tap;

					bind.CountTappedTime = true;

					if (bind.TappedFor < bind.TimeOut && !bind.DoubleTapped)
					{
						action.ConditionalsState |= Conditional.DoubleTap;

						bind.DoubleTapped = true;

						if ((bind.Conditional & Conditional.DoubleTapToggle) == Conditional.DoubleTapToggle)
						{
							action.ConditionalsState ^= Conditional.Toggle;
						}
					}
					else
					{
						bind.DoubleTapped = false;
					}

#if USE_32BIT_FLOATS_FOR_TIME
					bind.TappedFor = 0f;
#else
					bind.TappedFor = Half.Zero;
#endif
				}
				else
				{
					bind.CountTappedTime = false;
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

			UpdateActionEnabled(buttonAction);
		}

		/// <summary>
		/// Removes an action, you should stop using it
		/// </summary>
		/// <param name="buttonAction"></param>
		public static void UnregisterButtonAction(ButtonAction buttonAction)
		{
			//in case an action with the same name is present but not the same exact action, for some reason *cough cough* fuck ass editor piss of shit *cough cough*

			var action = GetAction(buttonAction.Name);

			m_AllActions.Remove(action);
			m_EnabledActions.Remove(action);
		}

		internal static void UpdateActionEnabled(ButtonAction buttonAction)
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

		public static void TrapKeys(Action<string> onKeysTrappedCallback)
		{
			
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

				if (action.m_Primary.CountTappedTime)
				{
#if USE_32BIT_FLOATS_FOR_TIME
					action.m_Primary.TappedFor += Time.Delta;
#else
					action.m_Primary.TappedFor += (Half)Time.Delta;
#endif
				}

				if (action.m_Secondary.CountTappedTime)
				{
#if USE_32BIT_FLOATS_FOR_TIME
					action.m_Secondary.TappedFor += Time.Delta;
#else
					action.m_Secondary.TappedFor += (Half)Time.Delta;
#endif
				}

				CheckMash(action, ref action.m_Primary);
				CheckMash(action, ref action.m_Secondary);

				CheckLongPress(action, ref action.m_Primary);
				CheckLongPress(action, ref action.m_Secondary);
			}

			static void CheckLongPress(ButtonAction action, ref ButtonAction.Bind bind)
			{
#if USE_32BIT_FLOATS_FOR_TIME
				if (bind.TimeOut == 0f)
#else
				if (bind.TimeOut == Half.Zero)
#endif
				{
					return;
				}

				if (keyStates[(int)bind.Key].PressedFor >= bind.TimeOut && !bind.LongPressed)
				{
					action.ConditionalsState |= Conditional.LongPress;

					bind.LongPressed = true;

					if ((bind.Conditional & Conditional.LongPressToggle) == Conditional.LongPressToggle)
					{
						bind.Conditional ^= Conditional.Toggle;
					}
				}
			}

			static void CheckMash(ButtonAction action, ref ButtonAction.Bind bind)
			{
#if USE_32BIT_FLOATS_FOR_TIME
				if (bind.TimeOut == 0f)
#else
				if (bind.TimeOut == Half.Zero)
#endif
				{
					return;
				}

				if (keyStates[(int)bind.Key].ChangedStateFor >= bind.TimeOut)
				{
					action.ConditionalsState &= ~Conditional.Mash;
				}
			}
		}

		/// <summary>
		/// Creates a new <see cref="ButtonAction"/>, registers it to the list, and returns it. <br/>
		/// You probably shouldn't be using this at runtime, but hey, you do you
		/// </summary>
		/// <inheritdoc cref="ButtonAction(string, ButtonAction.Bind, ButtonAction.Bind, string, bool, Conditional)"/>
		/// <returns>The newly created <see cref="ButtonAction"/></returns>
		public static ButtonAction CreateAction(string name, ButtonAction.Bind primary, ButtonAction.Bind secondary, string set = "General", bool enabled = true, Conditional allowedConditionals = Conditional.All)
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

			return MissingAction;
		}

		public static void SetActionSetActive(string setName, bool active)
		{
			if (setName != "General")
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

		static void ProcessAnalogMove()
		{
			var move = Vector3.Zero;

			if (GetAction("Forward"))
			{
				move += Vector3.Forward;
			}

			if (GetAction("Backward"))
			{
				move += Vector3.Backward;
			}

			if (GetAction("Left"))
			{
				move += Vector3.Left;
			}

			if (GetAction("Right"))
			{
				move += Vector3.Right;
			}

			Move = move;
		}

		static void UpdateButtonActions()
		{
			foreach (var action in m_EnabledActions)
			{
				action.Active =
					(((action.ConditionalsState & action.Primary.Conditional) != Conditional.None) && (action.Primary.Modifiers == Modifiers.None || (ActiveModifiers & action.Primary.Modifiers) != Modifiers.None)) ||
					(((action.ConditionalsState & action.Secondary.Conditional) != Conditional.None) && (action.Secondary.Modifiers == Modifiers.None || (ActiveModifiers & action.Secondary.Modifiers) != Modifiers.None));
			}
		}

		internal static void Frame()
		{
			ReinitialiseKeyStates();

			UpdateButtonActions();

			ProcessAnalogLook();

			ProcessAnalogMove();
		}

		internal static void FrameEnd()
		{
			RefreshActionLists();
		}
	}
}