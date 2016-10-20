using System;
using System.IO;

namespace Heibroch.DiskCleaner
{
    class Program
    {
        private static int currentlyInUseDirectoryCount;
        private static int currentlyInUseFileCount;
        private static int accessDeniedFileCount;
        private static long spaceFreed = 0;
        private static int deletedFileCount = 0;
        private static int failedFileDeleteCount = 0;
        private static int deletedDirectoryCount = 0;
        private static int failedDirectoryDeleteCount = 0;

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


            Console.WriteLine("Cleaning up files...");
            for (int i = 0; i < files.Length; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                if (fileName.StartsWith("MAT_"))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }

                if (fileName.StartsWith("~") && fileName.EndsWith(".TMP"))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }

                if (fileName.EndsWith("_Monitor_log") || fileName.EndsWith("_Monitor_log.txt"))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }

                if (fileName.StartsWith("CVR") && (fileName.EndsWith(".tmp.cvr") || fileName.EndsWith(".tmp")))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }

                if (fileName.StartsWith("dev") && (fileName.EndsWith(".tmp")))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }

                if (fileName.StartsWith("DEL") && (fileName.EndsWith(".tmp")))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }

                if (fileName.StartsWith("Report") && fileName.EndsWith(".diagsession"))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }

                if (fileName.StartsWith("version-"))
                {
                    TryDeleteFile(files[i]);
                    continue;
                }
            }

            Console.WriteLine("Cleaning up directories...");
            for (int i = 0; i < directories.Length; i++)
            {

                var lastIndexOf = directories[i].LastIndexOf("\\") + 1;
                var remainingLength = directories[i].Length - lastIndexOf;
                var directoryName = directories[i].Substring(lastIndexOf, remainingLength);
                if (directoryName.StartsWith("Report."))
                {
                    TryDeleteDirectory(directories[i]);
                    continue;
                }

                if (directoryName.StartsWith("TCD") && directoryName.EndsWith(".tmp"))
                {
                    TryDeleteDirectory(directories[i]);
                    continue;
                }
            }

            Console.WriteLine("Deleted files: {0}", deletedFileCount);
            Console.WriteLine("Failed to delete file count: {0} ", failedFileDeleteCount);
            Console.WriteLine("Failed to delete file due to access rights: {0}", accessDeniedFileCount);
            Console.WriteLine("Deleted directories: {0}", deletedDirectoryCount);
            Console.WriteLine("Failed to delete directory count: {0} ", failedDirectoryDeleteCount);
            Console.WriteLine("Space freed: {0} MB", spaceFreed/1024/1024);
            Console.Read();
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                var rootDirectoryInfo = new DirectoryInfo(path);

                foreach (var directoryInfo in rootDirectoryInfo.GetDirectories())
                {
                    TryDeleteDirectory(directoryInfo.FullName);
                }

                foreach (var fileInfo in rootDirectoryInfo.GetFiles())
                {
                    TryDeleteFile(fileInfo.FullName);
                }
                
                Directory.Delete(path, true);
                deletedDirectoryCount++;
                Console.WriteLine("Deleted directory: " + path);
            }
            catch (Exception ex)
            {
                failedDirectoryDeleteCount++;
                if (ex.Message.Contains("being used by another"))
                {
                    currentlyInUseDirectoryCount++;
                    return;
                }
                Console.WriteLine("Deletion of directory failed: " + path);
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                var fileSize = fileInfo.Length;

                File.Delete(path);
                deletedFileCount++;
                
                Console.WriteLine("Deleted file: " + path);
                spaceFreed += fileSize;
            }
            catch (Exception ex)
            {
                failedFileDeleteCount++;
                if (ex.Message.Contains("being used by another"))
                {
                    currentlyInUseFileCount++;
                    return;
                }
                if (ex.Message.Contains("is denied"))
                {
                    accessDeniedFileCount++;
                    return;
                }
                Console.WriteLine("Deletion of file failed: " + path);
            }
        }
    }
}
