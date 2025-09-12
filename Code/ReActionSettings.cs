#if SANDBOX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReActionPlugin.ReAction;

namespace ReActionPlugin
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
			this.Actions = ReAction.GetDefaultActions();
		}

		public HashSet<ReAction.Action> Actions
		{
			get;
			set;
		}
	}
}
#endif