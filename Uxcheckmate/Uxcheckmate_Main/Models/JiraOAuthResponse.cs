using System;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Models
{
    public class JiraOAuthResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
}