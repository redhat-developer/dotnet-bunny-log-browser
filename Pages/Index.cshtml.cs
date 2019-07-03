using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Net;


namespace DotnetBunnyLogBrowser.Pages
{
    public class IndexModel : PageModel
    {
        private static string PathToName(string path, char slash = '/')
        {
            return path.Substring(path.LastIndexOf(slash) + 1);
        }
        private static string[] SubdirectoriesNames(string directory, string pattern = "*")
        {
            return Directory.GetDirectories(directory, pattern).Select(subdir => PathToName(subdir)).ToArray();
        }
        private static int ParseOr(string s, int default_value)
        {
            int num;
            bool ok = int.TryParse(s, out num);
            return ok ? num : default_value;
        }
        public List<BunnyJob> GetJobs()
        {
            var jobNames = SubdirectoriesNames(JobsConfig.Get().JobsDirectory, JobsConfig.Get().JobsPrefix + "*");
            var jobs = new List<BunnyJob>(jobNames.Length);
            var client = new WebClient();

            for (int i = 0; i < jobNames.Length; ++i)
            {
                var buildsNames = SubdirectoriesNames(JobsConfig.Get().JobsDirectory + jobNames[i] + "/builds");
                var lastBuild = buildsNames.Select(nameof => ParseOr(nameof, -1)).DefaultIfEmpty(-1).Max();
                string logfile = "";
                try
                {
                    logfile = client.DownloadString(JobsConfig.Get().JobsURL + jobNames[i] + "/ws/results/logfile.log");
                }
                catch (WebException)
                {
                    jobs.Add(new BunnyJob(jobNames[i], lastBuild == -1 ? "" : JobsConfig.Get().JobsURL + jobNames[i] + "/" + lastBuild + "/console", false));
                    continue;
                }
                var errors = logfile.Split("Result: FAIL");
                if (errors.Length <= 1)
                {
                    jobs.Add(new BunnyJob(jobNames[i], lastBuild == -1 ? "" : JobsConfig.Get().JobsURL + jobNames[i] + "/" + lastBuild + "/console", true));
                    continue;
                }
                var tests = new List<BunnyTest>(errors.Length - 1);
                for (int j = 0; j < tests.Capacity; ++j)
                {
                    var lastLF = errors[j].LastIndexOf('\n');
                    var testName = errors[j].Substring(lastLF, errors[j].IndexOf(':', lastLF) - lastLF).Trim();
                    try
                    {
                        tests.Add(new BunnyTest(testName, client.DownloadString(JobsConfig.Get().JobsURL + jobNames[i] + "/ws/results/logfile-" + testName + ".log")));
                    }
                    catch (WebException)
                    {
                        tests.Add(new BunnyTest(testName, ""));
                    }
                }
                jobs.Add(new BunnyJob(jobNames[i], lastBuild == -1 ? "" : JobsConfig.Get().JobsURL + jobNames[i] + "/" + lastBuild + "/console", false, tests));
            }
            return jobs;
        }
    }
}
