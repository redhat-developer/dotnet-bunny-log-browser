using System.Collections.Generic;
using System.Linq;
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
        private BunnyJob LoadJobLogfile(string jobName, bool showTests, int test, int lastBuild)
        {
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
            var tests = new BunnyTest[errors.Length - 1];
            for (int i = 0; i < tests.Length; ++i)
            {
                var lastLF = errors[i].LastIndexOf('\n');
                var testName = errors[i].Substring(lastLF, errors[i].IndexOf(':', lastLF) - lastLF).Trim();
                tests[i] = new BunnyTest(testName, "");
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
        static private (string name, bool success) ParseXml(string xml)
        {
            XElement el = XElement.Parse(xml.Substring(xml.IndexOf('>')+1));
            return (el.DescendantsAndSelf("displayName").First().Value, el.DescendantsAndSelf("result").First().Value == "SUCCESS");
        }
        private (BunnyJob, bool) LoadJobJson(string jobName, bool showTests, int test, int lastBuild)
        {
            (string name, bool success) xmlvalue;
            try
            {
                xmlvalue = ParseXml(client.DownloadString(paths.XmlUrl(jobName, lastBuild)));
            }
            catch
            {
                xmlvalue = (jobName, false);
            }
            JsonDocument json;
            try
            {
                json = JsonDocument.Parse(client.DownloadString(paths.JsonUrl(jobName, lastBuild)));
            }
            catch (WebException)
            {
                return (new BunnyJob(xmlvalue.name, "", xmlvalue.success), true);
            }
            var root = json.RootElement[0].GetProperty("report");
            if (root.GetProperty("testsPassed").GetInt32() == root.GetProperty("testsTotal").GetInt32())
            {
                return (new BunnyJob(xmlvalue.name, "", true), false);
            }
            if (!showTests)
            {
                return (new BunnyJob(xmlvalue.name, "", false), false);
            }
            var tests = new BunnyTest[root.GetProperty("testProblems").GetArrayLength()];
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
                tests[i] = new BunnyTest(problem.GetProperty("name").GetString(), log);
                ++i;
            }
            return (new BunnyJob(xmlvalue.name, "", false, tests), false);
        }
        public BunnyJob[] GetJobs(int job, int test, bool useJson)
        {
            var names = paths.JobNames;
            var jobs = new BunnyJob[names.Length];
            for (int i = 0; i < names.Length; ++i)
            {
                int lastBuild = paths.LastBuildNumber(names[i]);
                bool useLogfile = true;
                if (useJson)
                {
                    (jobs[i], useLogfile) = LoadJobJson(names[i], job == i, test, lastBuild);
                }
                if (useLogfile)
                {
                    jobs[i] = LoadJobLogfile(names[i], job==i, test, lastBuild);
                }
            }
            return jobs;
        }
    }
}