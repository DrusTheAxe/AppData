// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System.IO;

namespace TrollCaveEnterprises
{
    class FileSystem
    {
        public static void DeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore
            }
        }

        public static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (DirectoryNotFoundException)
            {
                // Ignore
            }
        }
    }
}
