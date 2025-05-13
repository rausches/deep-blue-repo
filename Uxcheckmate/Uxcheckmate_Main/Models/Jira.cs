using System;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Models
{
    public class JiraProjectSearchResult
    {
        public List<JiraProject> Values { get; set; }
    }

    public class JiraProject
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
    }
    
    public class JiraOAuthResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
    
    public class JiraCloudSite
    {
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
    }
}
