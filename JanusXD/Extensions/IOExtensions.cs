using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JanusXD.Shell.Extensions
{
    public static class IOExtensions
    {
        static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Read the entirety of a stream to a byte array
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <remarks>
        /// Original Source: https://stackoverflow.com/a/6586039/8058709
        /// </remarks>
        /// <returns>Byte array contents of the stream</returns>
        public static byte[] ReadFully(this Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static bool TryCopy(string sourcePath, string destPath, out Exception exception, bool overwrite = false, bool logErrors = true)
        {
            try
            {
                string directory = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                File.Copy(sourcePath, destPath, overwrite);
            }
            catch (Exception ex)
            {
                if (logErrors) Logger.Error(ex);
                exception = ex;
                return false;
            }

            exception = null;
            return true;
        }

        public static bool TryDelete(string path, out Exception exception, bool logErrors = true)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                if (logErrors) Logger.Error(ex);

                exception = ex;
                return false;
            }

            exception = null;
            return true;
        }

        public static bool TryDeleteDirectory(string path, out Exception exception, bool logErrors = true)
        {
            exception = null;
            bool result = false;

            try
            {
                DirectoryInfo directory = new DirectoryInfo(path);

                if (!directory.Exists) return false;

                foreach (var dir in directory.EnumerateDirectories())
                {
                    result = TryDeleteDirectory(dir.FullName, out exception, logErrors);
                    if (!result) break;
                }

                foreach (var file in directory.EnumerateFiles())
                {
                    result = TryDelete(file.FullName, out exception, logErrors);
                    if (!result) break;
                };
            }
            catch (Exception ex)
            {
                if (logErrors) Logger.Error(ex);

                exception = ex;
                return false;
            }


            return result;
        }

        /// <summary>
        /// Easy and safe way to create multiple directories. 
        /// </summary>
        /// <param name="directories">The set of directories to create</param>
        public static void CreateDirectories(params string[] directories)
        {
            if (directories == null || directories.Length <= 0) return;

            foreach (var directory in directories)
                try
                {
                    if (Directory.Exists(directory)) continue;

                    Directory.CreateDirectory(directory);
                    Logger.Info("A new directory has been created ({0})", directory);
                }
                catch (Exception e)
                {
                    Logger.Error("Error while creating directory {0} - {1}", directory, e);
                }
        }

        public static void ClearDirectory(string directory, bool removeDirectory = false)
        {
            if (string.IsNullOrWhiteSpace(directory)) return;

            foreach (var d in Directory.EnumerateDirectories(directory))
                ClearDirectory(d, true);

            foreach (var file in Directory.EnumerateFiles(directory, "*"))
                try { File.Delete(file); }
                catch (Exception e) { Logger.Error("Failed to delete {0}\n", file, e); }

            if (removeDirectory)
                try { Directory.Delete(directory); }
                catch (Exception ex) { Logger.Error("An error occured while attempting to remove a directory ({0})\n{1}", directory, ex); }
        }

        public static void CloneDirectory(string directory, string destination, bool overwrite = false, bool logErrors = true)
        {
            foreach (var source in Directory.EnumerateFiles(directory))
            {
                string relative = Path.GetRelativePath(directory, source);
                string dest = Path.Combine(destination, relative);

                TryCopy(source, dest, out Exception _, overwrite, logErrors);
            }
        }
    }
}
