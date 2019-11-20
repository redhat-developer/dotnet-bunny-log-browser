using System.Web;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetBunnyLogBrowser.Pages
{
    public class IndexModel : PageModel
    {
        public int[] UrlParameters()
        {
            int job=-1, test=-1;
            if(Request!=null && Request.QueryString!=null && Request.QueryString.HasValue)
            {
                string[] query=Request.QueryString.ToString().Substring(1).Split('&');
                if(query.Length>=1&&query[0].StartsWith("job="))
                {
                    int.TryParse(query[0].Substring(4), out job);
                }
                if(query.Length>=2&&query[1].StartsWith("test="))
                {
                    int.TryParse(query[1].Substring(5), out test);
                }
            }
            return new int[2] {job, test};
        }
        public List<BunnyJob> GetJobs()
        {
			return new JobLoader(JobsConfig.Get().JobsDirectory, JobsConfig.Get().JobsPattern, JobsConfig.Get().JobsURL).GetJobs(JobsConfig.Get().UseJson);
        }
    }
}
