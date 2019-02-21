using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSCodeStats.Models
{
    class UserStat
    {
        public string projectName { get; set; }
        public string repoName { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public int linesAdded{ get; set; }
        public int linesRemoved { get; set; }
        public int linesModified { get; set; }
        public int filesAdded { get; set; }
        public int filesModified { get; set; }
        public int filesDeleted { get; set; }
    }
}
