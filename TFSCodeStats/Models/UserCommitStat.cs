using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSCodeStats.Models
{
    class UserCommitStat
    {
        public string projectName { get; set; }
        public string repoName { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public int commitCount { get; set; }
    }
}
