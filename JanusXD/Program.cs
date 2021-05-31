using HtmlAgilityPack;
using JanusXD.Shell.Extensions;
using JanusXD.Shell.Models;
using MAB.DotIgnore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JanusXD.Shell
{
    partial class Program
    {
        static void Main(string[] args)
        {
            JanusContext context = new JanusContext();

            if (args.HasArgument(AppArgument.SourceDirectory, out string source))
            {
                if (!Directory.Exists(source))
                    throw new Exception("Source directory does not appear to exist");

                context.SourceDirectory = source;
            }

            if (args.HasArgument(AppArgument.DestinationFolder, out string destination))
                context.DestinationDirectory = destination;

            if (args.HasArgument(AppArgument.Configure))
                Configure();

            if (args.HasArgument(AppArgument.ProjectName, out string projectName))
                context.ProjectName = projectName;

            if (args.HasArgument(AppArgument.Help))
            {
                DisplayInstructions();
                return;
            }

            if (args.HasArgument(AppArgument.DisregardGitIgnore))
                context.UseGitIgnore = false;

            if (args.HasArgument(AppArgument.DisregardJanusIgnore))
                context.UseJanusIgnore = false;

            if (args.HasArgument(AppArgument.MaxPageSize, out string maxSize))
            {
                if (!long.TryParse(maxSize, out long size))
                {
                    Console.WriteLine("Max page size must be a number.");
                    DisplayInstructions();
                    return;
                }

                if (size <= 0)
                {
                    Console.WriteLine("Max page size must be a positive integer");
                    DisplayInstructions();
                    return;
                }

                context.MaxSize = size;
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
            Console.WriteLine("janusxd --source /path/to/source/code --dest /output/folder");

            for (int i = 0; i < AppArgument.All.Length; i++)
                Console.WriteLine(AppArgument.All[i]);
        }

        static bool PrepareDirectory(JanusContext context)
        {
            if (string.IsNullOrWhiteSpace(context.ProjectName))
                context.ProjectName = Path.GetDirectoryName(context.SourceDirectory).Split("\\").LastOrDefault();

            string workDir = "";

            try
            {
                workDir = Path.Combine(AppBase.DATA_DIR, context.ProjectName);
                IOExtensions.CloneDirectory(AppBase.WWWROOT, workDir);
            }
            catch { }

            int attempts = 0;
            while (!Directory.Exists(workDir))
            {
                if (++attempts > 3)
                {
                    return false;
                }

                workDir = Path.Combine(AppBase.DATA_DIR, Path.GetRandomFileName());
                IOExtensions.CloneDirectory(AppBase.WWWROOT, workDir);
            }

            string template = Path.Combine(workDir, "PageTemplate.html");
            IOExtensions.TryDelete(template, out _);

            context.WorkDirectory = workDir;
            return true;
        }

        static void Generate(JanusContext context)
        {
            var spinner = ConsoleSpinner.Instance;

            if (!PrepareDirectory(context))
            {
                Console.WriteLine("Failed to setup project directory. Please run JanusXD with elevated permissions to give it another try.");
                return;
            }

            Console.WriteLine("================================================");
            Console.WriteLine("JanusXD Source Code Generation Session");
            Console.WriteLine($"Start Time: {context.SessionStart}");
            Console.WriteLine();
            Console.WriteLine();

            spinner.Activate(message: "Generating Document", delay: 100, type: SpinnerType.Cross);

            //IOExtensions.

            HtmlDocument document = null; HtmlNode body = null;
            Action PrepareDocument = () =>
            {
                document = new HtmlDocument();
                string htmlPath = Path.Combine(AppBase.WWWROOT, "PageTemplate.html");
                document.LoadHtml(File.ReadAllText(htmlPath));
                body = document.DocumentNode.SelectSingleNode("//body");
            };

            Action SaveDocument = () =>
            {
                string outputPath = Path.Combine(context.WorkDirectory, $"Page-{context.Page}.html");
                using (var stream = File.CreateText(outputPath))
                    stream.Write(context.Content);

                context.Page++;
            };

            PrepareDocument();

            bool configuredIgnore = false;
            string directory = context.SourceDirectory;
            List<IgnoreList> ignoreCollection = new List<IgnoreList>();
            
            var defaultList = new IgnoreList();
            defaultList.AddRule("/.git");

            foreach (var file in DirectoryHelper.FindAccessibleFiles(context.SourceDirectory, "*", true, null, null))
            {   
                string fileName = Path.GetFileName(file);
                if (fileName == JanusContext.GitIgnore || fileName == JanusContext.JanusIgnore) continue;

                string relative = Path.GetRelativePath(context.SourceDirectory, file);
                if (ignoreCollection.Any(x => x.IsIgnored(relative, false)))
                    continue;

                string newDir = Path.GetDirectoryName(file);
                if (directory != newDir || !configuredIgnore)
                {
                    directory = newDir;
                    configuredIgnore = true;

                    
                    var ignoreFiles = DirectoryHelper.FindAccessibleFiles(context.SourceDirectory,
                        context.IgnoreFilePattern, false, null, null);

                    if (ignoreFiles.Any())
                    {
                        if (ignoreFiles.Count() == 1 && !ignoreFiles.Any(x => x == JanusContext.JanusIgnore))
                            ignoreCollection.Clear();

                        ignoreCollection.Add(defaultList);

                        foreach (var ignoreFile in ignoreFiles)
                            ignoreCollection.Add(new IgnoreList(ignoreFile));
                    }
                }

                if (ignoreCollection.Any(x => x.IsIgnored(relative, false)))
                    continue;

                spinner.SetMessage($"Reading: {relative}");

                HtmlNode section = document.CreateElement("section");
                section.AddClass("flex flex-col mx-10");

                HtmlNode heading = document.CreateElement("h3");
                heading.InnerHtml = Path.GetFileName(file);
                heading.AddClass("text-xl text-sans my-0 mt-8");

                HtmlNode subHeading = document.CreateElement("h6");
                subHeading.InnerHtml = relative;
                subHeading.AddClass("text-xs opacity-50 text-sans -mt-1 mb-1 text-primary");

                section.AppendChild(heading);
                section.AppendChild(subHeading);

                HtmlNode pre = document.CreateElement("pre");
                pre.AddClass("prettyprint");
                section.AppendChild(pre);

                HtmlNode code = document.CreateElement("xmp");
                pre.AppendChild(code);

                var inner = document.CreateTextNode();
                inner.InnerHtml = File.ReadAllText(file);

                code.AppendChild(inner);

                string node = section.OuterHtml;

                int proposedSize = (context.Content.Length + node.Length) * sizeof(char);
                long maxSize = context.MaxSize * 1000000;

                if (proposedSize > maxSize && context.Content.Length >= 0)
                {
                    SaveDocument();
                    PrepareDocument();
                }

                body.AppendChild(section);
                context.Content = document.DocumentNode.OuterHtml;
            }

            SaveDocument();

            string fullPath = Path.GetFullPath(context.DestinationDirectory);

            spinner.SetMessage("Finalizing generation. Please wait");
            
            IOExtensions.CloneDirectory(context.WorkDirectory, context.DestinationDirectory, true);
            IOExtensions.ClearDirectory(context.WorkDirectory, true);

            Console.WriteLine($"Successfully generated to directory => ({fullPath})");

            spinner.Deactivate();
        }

        //static void RenderAndPrint(JanusContext context)
        //{

        //}
    }
}
