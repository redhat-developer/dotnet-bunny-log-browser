using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
			//TODO...

			return new List<BunnyJob>(){
				new BunnyJob("dotnet-rpm-rhel7-x64-1.0", true),
				new BunnyJob("dotnet-rpm-rhel7-x64-1.1", true),
				new BunnyJob("dotnet-rpm-rhel7-x64-2.1", false, new List<BunnyTest>(){new BunnyTest("createdump-aspnet", GetExampleLog()), new BunnyTest("bash-completion", "Bash completion is bork and this is the example log.")}),
				new BunnyJob("dotnet-rpm-rhel7-x64-2.2", true)
			};
		}
	}
}
