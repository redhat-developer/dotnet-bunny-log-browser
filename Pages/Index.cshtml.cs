using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetBunnyLogBrowser.Pages
{
    public class IndexModel : PageModel
    {
        public List<BunnyJob> GetJobs()
        {
			return new LogfileJobLoader(JobsConfig.Get().JobsDirectory, JobsConfig.Get().JobsPrefix + "*", JobsConfig.Get().JobsURL).GetJobs();
        }
    }
}
