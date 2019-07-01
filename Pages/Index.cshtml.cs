using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Text;
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
				var logfile=client.DownloadString(JobsConfig.Get().JobsURL+job_names[i]+"ws/results/logfile.log");
				var errors=new List<int>(){0};
				while(errors.Last()!=-1)
				{
					errors.Add(logfile.IndexOf("Result: FAIL", errors.Last()+1));
				}
				if(errors.Count<=2)
				{
					jobs.Add(new BunnyJob(job_names[i], true));
					continue;
				}
				var tests=new List<BunnyTest>(errors.Count-2);
				for(int j=tests.Capacity-1; j>=0; --j)
				{
					logfile=logfile.Substring(0, errors[j+1]);
					var last_lf=logfile.LastIndexOf("\n");
					var test_name=logfile.Substring(last_lf, logfile.IndexOf(":", last_lf)-last_lf).Trim();
					tests.Add(new BunnyTest(test_name, client.DownloadString(JobsConfig.Get().JobsURL+job_names[i]+"ws/results/logfile-"+test_name+".log")));
				}
				jobs.Add(new BunnyJob(job_names[i], false, tests));
			}
			return jobs;
		}
	}
}
