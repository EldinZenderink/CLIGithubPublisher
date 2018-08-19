using System;
using System.Collections.Generic;
using System.Text;

namespace CLIGithubPublisher
{
    class ReleaseJSON
    {
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
    }
}
