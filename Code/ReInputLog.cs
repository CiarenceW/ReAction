namespace ReInput
{
	public class ReInputLog() : Sandbox.Diagnostics.Logger("ReInput")
	{
		/// <summary>
		/// Shorthand for calling <see cref="Sandbox.Diagnostics.Logger.Info(object)"/> with <code>$"{fieldName}: {field}</code>
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field. Pro-tip: use nameof(field).
		/// </param>
		/// <param name="field">
		/// The field whose info will be logged. do not fucking ToString() it or I WILL kill you
		/// </param>
		public void QuickInfo(string fieldName, object field)
		{
			Info($"{fieldName}: {field}");
		}
	}

	public static class GlobalReInputLog
    {
		public static ReInputLog ReInputLogger
		{
			get;
		} = new ReInputLog();
    }
}
