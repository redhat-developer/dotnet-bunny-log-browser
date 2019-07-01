using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Net;


namespace DotnetBunnyLogBrowser.Pages
{
    public class IndexModel : PageModel
	{
		public List<BunnyJob> GetJobs()
		{
            var job_names=Directory.GetDirectories(JobsConfig.Get().JobsDirectory, JobsConfig.Get().JobsPrefix+"*");
			var jobs=new List<BunnyJob>(job_names.Length);
			var client=new WebClient();

			for(int i=0; i<job_names.Length; ++i)
			{
				job_names[i]=job_names[i].Substring(job_names[i].LastIndexOf('/')+1);
				var build_names=Directory.GetDirectories(JobsConfig.Get().JobsDirectory+job_names[i]+"/builds");
				int last_build=-1;
				for(int j=0; j<build_names.Length; ++j)
				{
					int num=0;
					if(int.TryParse(build_names[i].Substring(build_names[i].LastIndexOf('/')+1), out num))
					{
						last_build=last_build>num?last_build:num;
					}
				}
				string logfile="";
				try
				{
					logfile=client.DownloadString(JobsConfig.Get().JobsURL+job_names[i]+"/ws/results/logfile.log");
				}
				catch(WebException)
				{
					jobs.Add(new BunnyJob(job_names[i], last_build==-1?"":JobsConfig.Get().JobsURL+job_names[i]+"/"+last_build+"/console", false));
					continue;
				}
				var errors=new List<int>(){0};
				while(errors.Last()!=-1)
				{
					errors.Add(logfile.IndexOf("Result: FAIL", errors.Last()+1));
				}
				if(errors.Count<=2)
				{
					jobs.Add(new BunnyJob(job_names[i], last_build==-1?"":JobsConfig.Get().JobsURL+job_names[i]+"/"+last_build+"/console", true));
					continue;
				}
				var tests=new List<BunnyTest>(errors.Count-2);
				for(int j=tests.Capacity-1; j>=0; --j)
				{
					logfile=logfile.Substring(0, errors[j+1]);
					var last_lf=logfile.LastIndexOf("\n");
					var test_name=logfile.Substring(last_lf, logfile.IndexOf(":", last_lf)-last_lf).Trim();
					try
					{
						tests.Add(new BunnyTest(test_name, client.DownloadString(JobsConfig.Get().JobsURL+job_names[i]+"/ws/results/logfile-"+test_name+".log")));
					}
					catch(WebException)
					{
						tests.Add(new BunnyTest(test_name, ""));
					}
				}
				jobs.Add(new BunnyJob(job_names[i], last_build==-1?"":JobsConfig.Get().JobsURL+job_names[i]+"/"+last_build+"/console", false, tests));
			}
			return jobs;
		}
	}
}
