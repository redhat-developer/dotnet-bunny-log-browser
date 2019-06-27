using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Text;


namespace DotnetBunnyLogBrowser.Pages
{
	public class IndexModel : PageModel
	{
		public string GetExampleLog() => "createdump-aspnet log:\n" + //TODO: delete example log
			       "\n" +
			       "+ version=2.1.504\n" +
			       "+ dotnet new web --force\n" +
			       "The template \"ASP.NET Core Empty\" was created successfully.\n" +
			       "\n" +
			       "Processing post-creation actions...\n" +
			       "Running 'dotnet restore' on /mnt/ramdisk/dotnet-bunny/dotnet-regular-tests/createdump-aspnet/createdump-aspnet.csproj...\n" +
			       "Restoring packages for /mnt/ramdisk/dotnet-bunny/dotnet-regular-tests/createdump-aspnet/createdump-aspnet.csproj...\n" +
			       "Generating MSBuild file /mnt/ramdisk/dotnet-bunny/dotnet-regular-tests/createdump-aspnet/obj/createdump-aspnet.csproj.nuget.g.props.\n" +
			       "    Generating MSBuild file /mnt/ramdisk/dotnet-bunny/dotnet-regular-tests/createdump-aspnet/obj/createdump-aspnet.csproj.nuget.g.targets.\n" +
			       "    Restore completed in 1.22 sec for /mnt/ramdisk/dotnet-bunny/dotnet-regular-tests/createdump-aspnet/createdump-aspnet.csproj.\n" +
			       "\n" +
			       "Restore succeeded.\n" +
			       "\n" +
			       "+ dotnet tool install -g dotnet-dev-certs\n" +
			       "Tool 'dotnet-dev-certs' is already installed.\n" +
			       "+ true\n" +
			       "+ dotnet dev-certs https\n" +
			       "Cannot find command 'dotnet dev-certs', please run the following command to install\n" +
			       "\n" +
			       "dotnet tool install --global dotnet-dev-certs";

		public List<BunnyJob> GetJobs()
		{
			string jobs_directory="/home/rblazek/Documents/dotnet-bunny-log-browser/TestData/" /*"http://hydra.brq.redhat.com:8080/view/"*/;
			var job_names=Directory.GetDirectories(jobs_directory, "dotnet-rpm*");
			var jobs=new List<BunnyJob>(job_names.Length);

			for(int i=0; i<job_names.Length; ++i)
			{
				job_names[i]=job_names[i].Substring(job_names[i].LastIndexOf('/')+1);
				var logfile=System.IO.File.ReadAllText(jobs_directory+job_names[i]+"/ws/results/logfile.log", Encoding.UTF8);
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
					tests.Add(new BunnyTest(test_name, System.IO.File.ReadAllText(jobs_directory+job_names[i]+"/ws/results/logfile-"+test_name+".log", Encoding.UTF8)));
				}
				jobs.Add(new BunnyJob(job_names[i], false, tests));
			}
			return jobs;
//			return new List<BunnyJob>(){
//				new BunnyJob("dotnet-rpm-rhel7-x64-1.0", true),
//				new BunnyJob("dotnet-rpm-rhel7-x64-1.1", true),
//				new BunnyJob("dotnet-rpm-rhel7-x64-2.1", false, new List<BunnyTest>(){new BunnyTest("createdump-aspnet", GetExampleLog()), new BunnyTest("bash-completion", "Bash completion is bork and this is the example log.")}),
//				new BunnyJob("dotnet-rpm-rhel7-x64-2.2", true)
//			};
		}
	}
}
