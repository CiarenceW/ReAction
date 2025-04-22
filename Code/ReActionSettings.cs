#if SANDBOX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReAction.ReAction;

namespace ReAction
{
	public class ReActionSettings : ConfigData
	{
		public ReActionSettings()
		{
			this.Actions = new();
			InitDefault();
		}

		public void InitDefault()
		{
			this.Actions?.Clear();
			this.Actions = ReAction.DefaultActions;
		}

		public HashSet<ReAction.Action> Actions
		{
			get;
			set;
		}
	}
}
#endif