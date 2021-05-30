using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JanusXD.Shell.Extensions
{
    public static class DirectoryHelper
    {
        public static bool CanWrite(string directory)
        {
            bool canWrite = false;
            try
            {
                var random = Path.Combine(directory, Path.GetRandomFileName());
                using (var stream = File.Create(random, 1)) { }
                canWrite = true;
                File.Delete(directory);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} is not accessable\n{1}", directory, e);
            }
            return canWrite;
        }

        public static IEnumerable<string> FindAccessibleFiles(string path, string pattern = "*", bool recursive = true, ManualResetEvent busy = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                yield break;

            // TODO: Make sure that the important directory is skipped

            busy?.WaitOne();
            //busy?.WaitOne(200);

            Console.WriteLine("File ({0})", path);
            var list = new List<string>();

            if (File.Exists(path))
            {
                string p = string.Empty;
                try { p = Path.GetFullPath(path); }
                catch { }

                if (!string.IsNullOrWhiteSpace(p))
                    yield return p;
                yield break;
            }

            if (!Directory.Exists(path))
                yield break;

            DirectoryInfo top = null;
            try { top = new DirectoryInfo(path); }
            catch { }

            if (top == null) yield return null;

            IEnumerator<FileInfo> files;

            try
            {
                if (pattern.Contains('|') || pattern.Contains(','))
                {
                    var array = pattern.Split('|', ' ', '\0', ',');
                    HashSet<string> hash = new HashSet<string>(array);

                    files = top.EnumerateFiles("*").Where(s => hash.Contains(s.Extension.ToLower())).GetEnumerator();//.Select(s => s.Extension == "");//.SelectMany(s => s == "");//.GetEnumerator();
                }
                else if (pattern == "*") files = top.EnumerateFiles("*").GetEnumerator();
                else files = top.EnumerateFiles("*").Where(s => s.Extension.ToLower() == pattern).GetEnumerator();
            }
            catch { files = null; }

            while (true)
            {
                FileInfo file = null;
                try
                {
                    if (files == null) break;
                    if (!(bool)files?.MoveNext()) break;
                    file = files.Current;
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (PathTooLongException) { continue; }

                yield return file.FullName;
            }

            if (!recursive) yield break;

            IEnumerator<DirectoryInfo> directories;
            try { directories = top.EnumerateDirectories("*").GetEnumerator(); }
            catch { directories = null; }

            while (true)
            {
                try
                {
                    if (directories == null) break;
                    if (!(bool)directories?.MoveNext()) break;
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (PathTooLongException) { continue; }

                foreach (var subpath in FindAccessibleFiles(directories?.Current.FullName, pattern, recursive, busy, cancellationToken))
                {
                    yield return subpath;
                }
            }
        }

        public static IEnumerable<string> FindAccessibleFiles(string path, string pattern = "*", bool recursive = true, ManualResetEvent busy = null, BackgroundWorker worker = null)
        {
            if (worker != null && worker.CancellationPending)
                yield break;

            // TODO: Make sure that the important directory is skipped
            busy?.WaitOne();
            busy?.WaitOne(200);

            //Console.WriteLine("File ({0})", path);
            var list = new List<string>();

            if (File.Exists(path))
            {
                string p = string.Empty;
                try { p = Path.GetFullPath(path); }
                catch { }

                if (!string.IsNullOrWhiteSpace(p))
                    yield return p;
                yield break;
            }

            if (!Directory.Exists(path))
                yield break;

            DirectoryInfo top = null;
            try { top = new DirectoryInfo(path); }
            catch { }

            if (top == null) yield return null;

            IEnumerator<FileInfo> files;

            try
            {
                if (pattern.Contains('|') || pattern.Contains(','))
                {
                    var array = pattern.Split('|', ' ', '\0', ',');
                    HashSet<string> hash = new HashSet<string>(array);

                    files = top.EnumerateFiles("*").Where(s => hash.Contains(s.Extension.ToLower())).GetEnumerator();//.Select(s => s.Extension == "");//.SelectMany(s => s == "");//.GetEnumerator();
                }
                else if (pattern == "*") files = top.EnumerateFiles("*").GetEnumerator();
                else files = top.EnumerateFiles("*").Where(s => s.Extension.ToLower() == pattern).GetEnumerator();
            }
            catch { files = null; }

            while (true)
            {
                FileInfo file = null;
                try
                {
                    if (files == null) break;
                    if (!(bool)files?.MoveNext()) break;
                    file = files.Current;
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (PathTooLongException) { continue; }

                yield return file.FullName;
            }

            if (!recursive) yield break;

            IEnumerator<DirectoryInfo> directories;
            try { directories = top.EnumerateDirectories("*").GetEnumerator(); }
            catch { directories = null; }

            while (true)
            {
                try
                {
                    if (directories == null) break;
                    if (!(bool)directories?.MoveNext()) break;
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (PathTooLongException) { continue; }

                foreach (var subpath in FindAccessibleFiles(directories?.Current.FullName, pattern, recursive, busy, worker))
                {
                    yield return subpath;
                }
            }
        }
    }
}
