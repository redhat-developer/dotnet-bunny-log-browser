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
        private BunnyJob LoadJobLogfile(string jobName, bool showTests, int test)
        {
            int lastBuild = paths.LastBuildNumber(jobName);
            string[] errors;
            try
            {
                errors = client.DownloadString(paths.JobLogfileUrl(jobName)).Split("Result: FAIL");
            }
            catch (WebException)
            {
                return new BunnyJob(jobName, lastBuild == -1 ? "" : paths.ConsoleUrl(jobName, lastBuild), false);
            }
            if (errors.Length <= 1)
            {
                return new BunnyJob(jobName, lastBuild == -1 ? "" : paths.ConsoleUrl(jobName, lastBuild), true);
            }
            if (!showTests)
            {
                return new BunnyJob(jobName, lastBuild == -1 ? "" : paths.ConsoleUrl(jobName, lastBuild), false);
            }
            var tests = new List<BunnyTest>(errors.Length - 1);
            for (int i = 0; i < tests.Capacity; ++i)
            {
                var lastLF = errors[i].LastIndexOf('\n');
                var testName = errors[i].Substring(lastLF, errors[i].IndexOf(':', lastLF) - lastLF).Trim();
                tests.Add(new BunnyTest(testName, ""));
            }
            if (test >= 0)
            {
                try
                {
                    tests[test].Log = client.DownloadString(paths.TestLogfileUrl(jobName, tests[test].Name));
                }
                catch (WebException) {}
            }
            return new BunnyJob(jobName, lastBuild == -1 ? "" : paths.ConsoleUrl(jobName, lastBuild), false, tests);
        }
        private BunnyJob LoadJobJson(string jobName, bool showTests, int test)
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
            if (!showTests)
            {
                return new BunnyJob(name, "", false);
            }
            var tests = new List<BunnyTest>(root.GetProperty("testProblems").GetArrayLength());
            int i = 0;
            foreach (var problem in root.GetProperty("testProblems").EnumerateArray())
            {
                string log = "";
                if (i == test)
                {
                    foreach (var output in problem.GetProperty("outputs").EnumerateArray())
                    {
                        log += output.GetProperty("name").GetString() + " - " + output.GetProperty("value").GetString();
                    }
                }
                tests.Add(new BunnyTest(problem.GetProperty("name").GetString(), log));
                ++i;
            }
            return new BunnyJob(name, "", false, tests);
        }
        public List<BunnyJob> GetJobs(int job, int test, bool use_json)
        {
            var names = paths.JobNames;
            var jobs = new List<BunnyJob>(names.Length);
            for (int i = 0; i < names.Length; ++i)
            {
                jobs.Add(use_json ? LoadJobJson(names[i], i == job, test) : LoadJobLogfile(names[i], job==i, test));
            }
            return jobs;
        }
    }
}