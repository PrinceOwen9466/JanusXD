using JanusXD.Shell.Extensions;
using RestSharp;
using Sharprompt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JanusXD.Shell
{
    static class Endpoints
    {
        public static string CodeThemes = "https://api.github.com/repos/jmblog/color-themes-for-google-code-prettify/git/trees/master?recursive=1";
    }

    partial class Program
    {
        static readonly string[] ConfigureOptions = new string[]
        {
            "Configure Theme",
            "Configure Header",
            "Go Back"
        };

        static void Configure()
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to the Configuration Section");

            //List<string> options = new List<string>
            var options = ConfigureOptions.Select((x, i) => (Index: i, Value: x));
            
            int ans = ConfigureOptions.SelectOption("Select the component you would like to configure");
            
            switch (ans)
            {
                case 1:
                    ConfigureTheme();
                    break;
            }
        }

        static void ConfigureTheme()
        {
            ConsoleSpinner.Instance.Activate();

            RestClient client = new RestClient();
            Thread.Sleep(2000);
            //new RestRequest(CodeT, Method.GET)
        }
    }
}
