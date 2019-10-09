using System.Collections.Generic;

namespace DotnetBunnyLogBrowser
{
    interface IJobLoader
    {
        List<BunnyJob> GetJobs();
    }
}