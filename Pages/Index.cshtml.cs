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
        public int?[] UrlParameters()
        {
            int? job=null, test=null;
            if(Request!=null && Request.QueryString!=null && Request.QueryString.HasValue)
            {
                string[] query=Request.QueryString.ToString().Substring(1).Split('&');
                if(query.Length>=1&&query[0].StartsWith("job="))
                {
                    job=int.Parse(query[0].Substring(4));
                }
                if(query.Length>=2&&query[1].StartsWith("test="))
                {
                    test=int.Parse(query[1].Substring(5));
                }
            }
            return new int?[2] {job, test};
        }
        public List<BunnyJob> GetJobs()
        {
			return new JobLoader(JobsConfig.Get().JobsDirectory, JobsConfig.Get().JobsPrefix + "*", JobsConfig.Get().JobsURL).GetJobs();
        }
    }
}
