using System;
using System.Collections.Generic;
using System.Text;

namespace ReActionPlugin
{
	public static partial class ReAction
	{
		[ConCmd]
		public static void PrintAllActions()
		{
			foreach (var action in m_AllActions)
			{
				ReActionLogger.Info(action.Name);
			}
		}

		[ConCmd]
		public static void PrintAllEnabledActions()
		{
			foreach (var action in m_EnabledActions)
			{
				ReActionLogger.Info(action.Name);
			}
		}

		[ConCmd]
		public static void PrintAllActiveSets()
		{
			foreach (var set in m_ActiveSets)
			{
				ReActionLogger.Info(set);
			}
		}
	}
}
