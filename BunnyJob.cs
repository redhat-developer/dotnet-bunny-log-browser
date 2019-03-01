using System;
using System.Collections;
using System.Collections.Generic;

namespace DotnetBunnyLogBrowser
{
	public class BunnyJob
	{
		public string DisplayName;
		public string Name;
		public bool Passed;
		public List<BunnyTest> FailedTests;

		public BunnyJob(string name, bool passed, List<BunnyTest> failedTests = null)
		{
			this.DisplayName = name;
			this.Name = name.Replace('.','-');
			this.Passed = passed;
			this.FailedTests = failedTests;
		}

	}

	public class BunnyJobList : IDisposable, IEnumerable
	{
		private List<BunnyJob> List;

		public BunnyJobList(List<BunnyJob> list)
		{
			this.List = list;
		}

		public void Dispose()
		{
		}

		public IEnumerator GetEnumerator()
		{
			return this.List.GetEnumerator();
		}
	}
}
