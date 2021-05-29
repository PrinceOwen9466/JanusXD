using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JanusXD.Shell.Models
{
    public class JanusContext
    {
        public string SourceDirectory { get; set; }
        public string DestinationFile { get; set; }
        public DateTime SessionStart { get; set; } = DateTime.Now;
        public DateTime SessionEnd { get; set; }

        public bool DisregardGitIgnore { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(SourceDirectory) || string.IsNullOrWhiteSpace(DestinationFile))
                return false;

            return true;
        }
    }
}
