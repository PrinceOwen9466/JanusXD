using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JanusXD.Shell.Models
{
    public class JanusContext
    {
        public static readonly string GitIgnore = ".gitignore";
        public static readonly string JanusIgnore = ".janusignore";

        public string ProjectName { get; set; }

        public string WorkDirectory { get; set; }

        public string SourceDirectory { get; set; }
        public string DestinationDirectory { get; set; }
        public DateTime SessionStart { get; set; } = DateTime.Now;
        public DateTime SessionEnd { get; set; }

        public bool UseGitIgnore { get; set; } = true;
        public bool UseJanusIgnore { get; set; } = true;

        public long MaxSize { get; set; } = 5120;

        public string IgnoreFilePattern
        {
            get
            {
                string pattern = "";
                if (UseGitIgnore) pattern += GitIgnore;
                if (UseJanusIgnore) pattern += $"|{JanusIgnore}";

                return pattern;
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(SourceDirectory) || string.IsNullOrWhiteSpace(DestinationDirectory))
                return false;

            return true;
        }
    }
}
