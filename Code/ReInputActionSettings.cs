using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReInput.ReInput;

namespace ReInput
{
	public class ReInputActionSettings : ConfigData
	{
		public ReInputActionSettings()
		{
			this.Actions = new();
			InitDefault();
		}

		public void InitDefault()
		{
			this.Actions?.Clear();
			this.Actions = ReInput.DefaultActions;
		}

		public HashSet<ReInput.Action> Actions
		{
			get;
			set;
		}
	}
}
