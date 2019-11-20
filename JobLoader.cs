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
        private JobPaths paths;
        private WebClient client;
        public JobLoader(string jobsDirectory, string jobsPattern, string jobsUrl)
        {
            paths = new JobPaths(jobsDirectory, jobsPattern, jobsUrl);
            client = new WebClient();
        }
        private BunnyJob LoadJobLogfile(string jobName)
        {
            int lastBuild = paths.LastBuildNumber(jobName);
            string logfile = "";
            try
            {
                logfile = client.DownloadString(paths.JobLogfileUrl(jobName));
            }
            catch (WebException)
            {
                return new BunnyJob(jobName, lastBuild == -1 ? "" : paths.ConsoleUrl(jobName, lastBuild), false);
            }
            var errors = logfile.Split("Result: FAIL");
            if (errors.Length <= 1)
            {
                return new BunnyJob(jobName, lastBuild == -1 ? "" : paths.ConsoleUrl(jobName, lastBuild), true);
            }
            var tests = new List<BunnyTest>(errors.Length - 1);
            for (int i = 0; i < tests.Capacity; ++i)
            {
                var lastLF = errors[i].LastIndexOf('\n');
                var testName = errors[i].Substring(lastLF, errors[i].IndexOf(':', lastLF) - lastLF).Trim();
                try
                {
                    tests.Add(new BunnyTest(testName, client.DownloadString(paths.TestLogfileUrl(jobName, testName))));
                }
                catch (WebException)
                {
                    tests.Add(new BunnyTest(testName, ""));
                }
            }
            return new BunnyJob(jobName, lastBuild == -1 ? "" : paths.ConsoleUrl(jobName, lastBuild), false, tests);
        }
        private BunnyJob LoadJobJson(string jobName)
        {
            int lastBuild = paths.LastBuildNumber(jobName);
            string name = jobName;
            bool success = false;
            JsonDocument json;
            try
            {
                string xml_text=client.DownloadString(paths.XmlUrl(jobName, lastBuild));
                XElement xml = XElement.Parse(xml_text.Substring(xml_text.IndexOf('>')+1));
                name = xml.DescendantsAndSelf("displayName").First().Value;
                success = (xml.DescendantsAndSelf("result").First().Value == "SUCCESS");
            }
            catch {}
            try
            {
                json = JsonDocument.Parse(client.DownloadString(paths.JsonUrl(jobName, lastBuild)));
            }
            catch (WebException)
            {
                return new BunnyJob(name, "", success);
            }
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
                    log += output.GetProperty("name").GetString() + " - " + output.GetProperty("value").GetString();
                }
                tests.Add(new BunnyTest(problem.GetProperty("name").GetString(), log));
            }
            return new BunnyJob(name, "", false, tests);
        }
        public List<BunnyJob> GetJobs(bool use_json)
        {
            var jobNames = paths.JobNames;
            var jobs = new List<BunnyJob>(jobNames.Length);
            foreach (var jobName in jobNames)
            {
                jobs.Add(use_json ? LoadJobJson(jobName) : LoadJobLogfile(jobName));
            }
            return jobs;
        }
    }
}