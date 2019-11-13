using System;
using System.Collections;
using System.Collections.Generic;

namespace DotnetBunnyLogBrowser
{
	public class BunnyJob
	{
		public string DisplayName;
		public string Name;
		public string ConsoleURL;
		public bool Passed;
		public List<BunnyTest> FailedTests;

		public BunnyJob(string name, string consoleUrl, bool passed, List<BunnyTest> failedTests = null)
		{
			this.DisplayName = name;
			this.Name = name.Replace('.','-');
			this.ConsoleURL = consoleUrl;
			this.Passed = passed;
			this.FailedTests = failedTests;
		}

	}
}
