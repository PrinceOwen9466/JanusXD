using JanusXD.Shell.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JanusXD.Shell.Models
{
    public class AppArgument
    {
        public string[] Instances { get; }
        public string Description { get; }

        public AppArgument(string description, string instance)
        {
            Description = description;

            string shortcut = "";
            foreach (var c in instance)
            {
                if (!char.IsLetter(c)) continue;

                shortcut = c.ToString();
                break;
            }

            Instances = new string[] { $"--{instance}", $"-{shortcut}" };
        }

        public AppArgument(string description, string instance, string shortcut)
        {
            Description = description;
            Instances = new string[] { $"--{instance}", $"-{shortcut}" };
        }

        #region Methods

        public override string ToString()
        {
            string args = string.Join(", ", Instances).PadRight(40);
            return $"{args}{Description}";
        }

        #endregion

        public static AppArgument SourceDirectory { get; } = new AppArgument("Source Directory", "source");
        public static AppArgument DestinationFolder { get; } = new AppArgument("Destination Folder", "dest");
        public static AppArgument Configure { get; } = new AppArgument("Interactive Configuration", "configure");
        public static AppArgument ProjectName { get; } = new AppArgument("Set Project Name", "project");
        public static AppArgument Help { get; } = new AppArgument("Help Instructions", "help");
        public static AppArgument DisregardGitIgnore { get; } = new AppArgument("Don't ignore paths included in .gitignore files", "disregard-git-ignore", "dg");
        public static AppArgument DisregardJanusIgnore { get; } = new AppArgument("Don't ignore paths included in .janusignore files", "disregard-janus-ignore", "dj");

        public static AppArgument MaxPageSize { get; } = new AppArgument("Maximum size (in MB) of each html document", "max-size");

        public static AppArgument[] All { get; } = new AppArgument[]
        {
            SourceDirectory, 
            DestinationFolder,
            Configure,
            ProjectName,
            DisregardGitIgnore,
            DisregardJanusIgnore,
            MaxPageSize,
            Help,
        };
    }

    public static class ArgumentExtensions
    {
        public static bool HasArgument(this string[] source, AppArgument argument, out string nextValue)
        {
            if (source.ContainsAnyInvariant(out int index, argument.Instances))
                nextValue = source.ElementAtOrDefault(index + 1);
            else
            {
                nextValue = null;
                return false;
            }

            return true;
        }

        public static bool HasArgument(this string[] source, AppArgument argument)
            => source.ContainsAnyInvariant(out int _, argument.Instances);
    }
}
