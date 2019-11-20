using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DotnetBunnyLogBrowser
{
    public class JobPaths
    {
        private string jobsDirectory, jobsPattern, jobsUrl;
        public JobPaths(string jobsDirectory, string jobsPattern, string jobsUrl)
        {
            this.jobsDirectory = jobsDirectory;
            this.jobsPattern = jobsPattern;
            this.jobsUrl = jobsUrl.TrimEnd('/') + "/";
        }
        private static string PathToName(string path) => path.Substring(path.TrimEnd(Path.DirectorySeparatorChar).LastIndexOf(Path.DirectorySeparatorChar) + 1);
        private static string[] SubdirectoriesNames(string directory, string pattern = "*") => Directory.GetDirectories(directory, pattern).Select(subdir => PathToName(subdir)).ToArray();
        private static int ParseOr(string s, int defaultValue)
        {
            int num;
            bool ok = int.TryParse(s, out num);
            return ok ? num : defaultValue;
        }
        public string[] JobNames { get => SubdirectoriesNames(jobsDirectory, jobsPattern); }
        public int LastBuildNumber(string job) => SubdirectoriesNames(Path.Combine(jobsDirectory, job, "builds")).Select(nameof => ParseOr(nameof, -1)).DefaultIfEmpty(-1).Max();
        public string JobLogfileUrl(string job) => jobsUrl + job + "/ws/results/logfile.log";
        public string TestLogfileUrl(string job, string test) => jobsUrl + job + "/ws/results/logfile-" + test + ".log";
        public string ConsoleUrl(string job, int build) => jobsUrl + job + "/" + build + "/consoleFull";
        public string XmlUrl(string job, int build) => jobsUrl + job + "/builds/" + build + "/build.xml";
        public string JsonUrl(string job, int build) => jobsUrl + job + "/builds/" + build + "/jtreg-report.json";
    }
}