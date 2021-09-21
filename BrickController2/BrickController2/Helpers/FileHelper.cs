using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BrickController2.Helpers
{
    public static class FileHelper
    {
        private static Regex _filenameWithoutExtensionPattern = new Regex(@"^\w+(\w\s)*");

        public static string CreationFileExtension => "bc2c";
        public static string ControllerProfileFileExtension => "bc2p";
        public static string SequenceFileExtension => "bc2s";

        public static Dictionary<string, string> EnumerateDirectoryFilesToFilenameMap(string directoryPath, string searchPattern)
        {
            var filePaths = Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);

            return filePaths.ToDictionary(fp => Path.GetFileNameWithoutExtension(fp), fp => fp);
        }

        public static bool FilenameValidator(string filename)
        {
            return _filenameWithoutExtensionPattern.IsMatch(filename);
        }
    }
}
