using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;

namespace DotnetBunnyLogBrowser
{
    public class JobLoader
    {
        private string jobsDirectory, jobsPattern, jobsUrl;
        private WebClient client;
        public JobLoader(string jobsDirectory, string jobsPattern, string jobsUrl)
        {
            this.jobsDirectory = jobsDirectory;
            this.jobsPattern = jobsPattern;
            this.jobsUrl = jobsUrl.TrimEnd('/') + "/";
            client = new WebClient();
        }
        private static string PathToName(string path)
        {
            return path.Substring(path.TrimEnd(Path.DirectorySeparatorChar).LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }
        private static string[] SubdirectoriesNames(string directory, string pattern = "*")
        {
            return Directory.GetDirectories(directory, pattern).Select(subdir => PathToName(subdir)).ToArray();
        }
        private static int ParseOr(string s, int defaultValue)
        {
            int num;
            bool ok = int.TryParse(s, out num);
            return ok ? num : defaultValue;
        }
        private BunnyJob LoadJobLogfile(string jobName)
        {
            var buildsNames = SubdirectoriesNames(Path.Combine(jobsDirectory, jobName, "builds"));
            var lastBuild = buildsNames.Select(nameof => ParseOr(nameof, -1)).DefaultIfEmpty(-1).Max();
            string logfile = "";
            try
            {
                logfile = client.DownloadString(jobsUrl + jobName + "/ws/results/logfile.log");
            }
            catch (WebException)
            {
                return new BunnyJob(jobName, lastBuild == -1 ? "" : jobsUrl + jobName + "/" + lastBuild + "/consoleFull", false);
            }
            var errors = logfile.Split("Result: FAIL");
            if (errors.Length <= 1)
            {
                return new BunnyJob(jobName, lastBuild == -1 ? "" : jobsUrl + jobName + "/" + lastBuild + "/consoleFull", true);
            }
            var tests = new List<BunnyTest>(errors.Length - 1);
            for (int j = 0; j < tests.Capacity; ++j)
            {
                var lastLF = errors[j].LastIndexOf('\n');
                var testName = errors[j].Substring(lastLF, errors[j].IndexOf(':', lastLF) - lastLF).Trim();
                try
                {
                    tests.Add(new BunnyTest(testName, client.DownloadString(jobsUrl + jobName + "/ws/results/logfile-" + testName + ".log")));
                }
                catch (WebException)
                {
                    tests.Add(new BunnyTest(testName, ""));
                }
            }
            return new BunnyJob(jobName, lastBuild == -1 ? "" : jobsUrl + jobName + "/" + lastBuild + "/consoleFull", false, tests);
        }
        private BunnyJob LoadJobJson(string jobName)
        {
            var buildsNames = SubdirectoriesNames(Path.Combine(jobsDirectory, jobName, "builds"));
            var lastBuild = buildsNames.Select(nameof => ParseOr(nameof, -1)).DefaultIfEmpty(-1).Max();
            JsonDocument json;
            XDocument xml;
            try
            {
                json = JsonDocument.Parse(client.DownloadString(jobsUrl + jobName + "/" + lastBuild + "/jtreg-report.json"));
                xml = XDocument.Parse(client.DownloadString(jobsUrl + jobName + "/" + lastBuild + "/build.xml"));
            }
            catch (WebException)
            {
                return new BunnyJob(jobName, "", false);
            }
            string name = xml.Root.GetDescendantsAndSelf("displayName").DefaultIfEmpty(jobName).First();
            var root = json.RootElement[0].GetProperty("report");
            if (root.GetProperty("testsPassed").GetInt32() == root.GetProperty("testsTotal").GetInt32())
            {
                return new BunnyJob(name, "", true);
            }
            var tests = new List<BunnyTest>(root.GetProperty("testProblems").GetArrayLength());
            foreach (var problem in root.GetProperty("testProblems").EnumerateArray())
            {
                string log = "";
                foreach (var output in problem.GetProperty("outputs").EnumerateArray())
                {
                    log += output.GetProperty("name").GetString() + ":\n" + output.GetProperty("value").GetString();
                }
                tests.Add(new BunnyTest(problem.GetProperty("name").GetString(), log));
            }
            return new BunnyJob(name, "", false, tests);
        }
        public List<BunnyJob> GetJobs()
        {
            var jobNames = SubdirectoriesNames(jobsDirectory, jobsPattern);
            var jobs = new List<BunnyJob>(jobNames.Length);
            for (int i = 0; i < jobNames.Length; ++i)
            {
                jobs.Add(LoadJobLogfile(jobNames[i]));
            }
            return jobs;
        }
    }
}