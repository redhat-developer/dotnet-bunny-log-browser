namespace DotnetBunnyLogBrowser
{
	public class BunnyTest
	{
		public string DisplayName;
		public string Name;
		public string Log;

		public BunnyTest(string name, string log)
		{
			this.DisplayName = name;
			this.Name = name.Replace('.','-');
			this.Log = log;
		}
	}
}
