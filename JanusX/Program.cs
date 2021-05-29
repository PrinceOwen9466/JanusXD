using HtmlAgilityPack;
using JanusXD.Shell.Extensions;
using JanusXD.Shell.Models;
using MAB.DotIgnore;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace JanusXD.Shell
{
    partial class Program
    {
        static void Main(string[] args)
        {
            JanusContext context = new JanusContext();

            if (args.HasArgument(out string source, "--source", "-s"))
            {
                if (!Directory.Exists(source))
                    throw new Exception("Source directory does not appear to exist");

                context.SourceDirectory = source;
            }

            if (args.HasArgument(out string destination, "--dest", "-d"))
                context.DestinationFile = destination;

            if (args.HasArgument(out _, "--configure", "-c"))
                Configure();

            if (args.HasArgument(out _, "--help", "-h"))
            {
                DisplayInstructions();
                return;
            }

            if (!context.IsValid())
            {
                DisplayInstructions();
                throw new Exception("Not enough information to get started");
            }

            Generate(context);
        }

        static void DisplayInstructions()
        {
            Console.WriteLine("JanusXD");
            Console.WriteLine("Source code to PDF generator");
            Console.WriteLine("");
            Console.WriteLine("");


            Console.WriteLine("USAGE: ");
            Console.WriteLine("");
            Console.WriteLine("janusxd --source /path/to/source --dest /path/to/pdf");

            Console.WriteLine("--source, -s\t\tSource Directory");
            Console.WriteLine("--dest, -d\t\tDestination PDF File");
            Console.WriteLine("--configure, -c\t\tInteractive Configuration");
            Console.WriteLine("--disregard-gitignore\t\tDisregard git ignore files during generation");
            Console.WriteLine("--ignore, -i\t\tCollection of files and extensions that should be ignored. Usage: \"*.sln, *.csproj, file/to/ignore.ig\"");
            Console.WriteLine("--help, -h\t\tDisplay instructions");
            Console.WriteLine("");
        }

        static void Generate(JanusContext context)
        {
            var spinner = ConsoleSpinner.Instance;

            Console.WriteLine("================================================");
            Console.WriteLine("JanusXD Source Code Generation Session");
            Console.WriteLine($"Start Time: {context.SessionStart}");
            Console.WriteLine();
            Console.WriteLine();

            spinner.Activate(message: "Generating Document", delay: 500);
            

            HtmlDocument document = new HtmlDocument();
            string htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PageTemplate.html");

            document.LoadHtml(File.ReadAllText(htmlPath));
            // DocumentNode.SelectSingleNode("//body").InnerText
            var body = document.DocumentNode.SelectSingleNode("//body");

            bool configuredIgnore = false;
            string directory = context.SourceDirectory;
            List<IgnoreList> ignoreCollection = new List<IgnoreList>();
            
            var defaultList = new IgnoreList();
            defaultList.AddRule("/.git");

            int count = 0;

            foreach (var file in DirectoryHelper.FindAccessibleFiles(context.SourceDirectory, "*", true, null, null))
            {   
                string fileName = Path.GetFileName(file);
                if (fileName == ".gitignore") continue;

                string relative = Path.GetRelativePath(context.SourceDirectory, file);
                if (ignoreCollection.Any(x => x.IsIgnored(relative, false)))
                    continue;

                string newDir = Path.GetDirectoryName(file);
                if (directory != newDir || !configuredIgnore)
                {
                    directory = newDir;
                    configuredIgnore = true;

                    var ignoreFiles = Directory.EnumerateFiles(directory, ".gitignore");
                    

                    if (ignoreFiles.Any())
                    {
                        ignoreCollection.Clear();

                        ignoreCollection.Add(defaultList);

                        foreach (var ignoreFile in ignoreFiles)
                            ignoreCollection.Add(new IgnoreList(ignoreFile));
                    }
                }

                if (ignoreCollection.Any(x => x.IsIgnored(relative, false)))
                    continue;

                spinner.SetMessage($"Generating for: {relative}");

                HtmlNode section = document.CreateElement("section");
                section.AddClass("flex");
                body.AppendChild(section);

                HtmlNode heading = document.CreateElement("h2");
                heading.InnerHtml = Path.GetFileName(file);
                heading.AddClass("ml-auto");

                section.AppendChild(heading);

                HtmlNode pre = document.CreateElement("pre");
                pre.AddClass("prettyprint");
                section.AppendChild(pre);

                HtmlNode code = document.CreateElement("xmp");
                pre.AppendChild(code);

                var inner = document.CreateTextNode();
                inner.InnerHtml = File.ReadAllText(file);

                code.AppendChild(inner);
            }

            //Directory.GetFiles(,)


            //string temp = Path.GetTempFileName();
            //document.Save(File.OpenWrite(temp));

            string html = document.DocumentNode.OuterHtml;
            File.WriteAllText("Sample.html", html);

            string root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", html);


            //PdfGenerator.
            PdfDocument pdf = PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4, 20);
            pdf.Save(File.Create(context.DestinationFile), true);

            string fullPath = Path.GetFullPath(context.DestinationFile);
            Console.WriteLine($"Successfully generated document ({fullPath})");

            spinner.Deactivate();
        }
    }
}
