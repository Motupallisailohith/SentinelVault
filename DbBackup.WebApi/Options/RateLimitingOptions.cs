using System.Collections.Generic;

namespace DbBackup.WebApi.Options
{
    public class RateLimitingOptions
    {
        public List<RateLimitRule> Rules { get; set; } = new List<RateLimitRule>();
    }

    public class RateLimitRule
    {
        public string Path { get; set; } = "";
        public int Limit { get; set; } = 100;
        public string Period { get; set; } = "00:01:00";
    }
}
