using System;
using System.IO;

namespace Heibroch.DiskCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Cleaning up...");

            var tempPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Local\\Temp";
            if (!Directory.Exists(tempPath))
            {
                Console.WriteLine("Could not find directories to clean up...");
                Console.Read();
                return;
            }

            var files = Directory.GetFiles(tempPath);
            var directories = Directory.GetDirectories(tempPath);
            long spaceFreed = 0;

            Console.WriteLine("Cleaning up files...");
            int deletedFiles = 0;
            int failedDeletedFiles = 0;
            for (int i = 0; i < files.Length; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                if (fileName.StartsWith("MAT_"))
                {
                    TryDeleteFile(files[i], ref deletedFiles, ref failedDeletedFiles);
                    continue;
                }

                if (fileName.StartsWith("~") && fileName.EndsWith(".TMP"))
                {
                    TryDeleteFile(files[i], ref deletedFiles, ref failedDeletedFiles);
                    continue;
                }

                if (fileName.EndsWith("_Monitor_log") || fileName.EndsWith("_Monitor_log.txt"))
                {
                    TryDeleteFile(files[i], ref deletedFiles, ref failedDeletedFiles);
                    continue;
                }

                if (fileName.StartsWith("CVR") && (fileName.EndsWith(".tmp.cvr") || fileName.EndsWith(".tmp")))
                {
                    TryDeleteFile(files[i], ref deletedFiles, ref failedDeletedFiles);
                    continue;
                }
            }

            Console.WriteLine("Cleaning up directories...");
            int deletedDirectories = 0;
            int failedDeletedDirectories = 0;
            for (int i = 0; i < directories.Length; i++)
            {

                var lastIndexOf = directories[i].LastIndexOf("\\") + 1;
                var remainingLength = directories[i].Length - lastIndexOf;
                var directoryName = directories[i].Substring(lastIndexOf, remainingLength);
                if (directoryName.StartsWith("Report."))
                {
                    TryDeleteDirectory(directories[i], ref deletedDirectories, ref failedDeletedDirectories);
                    continue;
                }

                if (directoryName.StartsWith("TCD") && directoryName.EndsWith(".tmp"))
                {
                    TryDeleteDirectory(directories[i], ref deletedDirectories, ref failedDeletedDirectories);
                    continue;
                }
            }

            Console.WriteLine("Deleted files: {0}", deletedFiles);
            Console.WriteLine("Failed to delete file count: {0} ", failedDeletedFiles);
            Console.WriteLine("Deleted directories: {0}", deletedDirectories);
            Console.WriteLine("Failed to delete directory count: {0} ", failedDeletedDirectories);
            Console.WriteLine("Space freed: {0} bytes", spaceFreed);
            Console.Read();
        }

        public static long GetDirectorySize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di);
            }
            return size;
        }

        private static void TryDeleteDirectory(string path, ref int deletedDirectoryCount, ref int failedDirectoryDeleteCount)
        {
            try
            {
                File.Delete(path);
                deletedDirectoryCount++;
            }
            catch (Exception)
            {
                failedDirectoryDeleteCount++;
            }
        }

        private static void TryDeleteFile(string path, ref int deletedFileCount, ref int failedFileDeleteCount)
        {
            try
            {
                File.Delete(path);
                deletedFileCount++;
            }
            catch (Exception)
            {
                failedFileDeleteCount++;
            }
        }
    }
}
