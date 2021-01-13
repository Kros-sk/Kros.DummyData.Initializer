using Kros.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kros.DummyData.Initializer
{
    internal static class DirectoryExtensions
    {
        public static bool TryGetFilePath(this DirectoryInfo value, string path, out string filePath)
        {
            filePath = PathHelper.BuildPath(value.FullName, path);

            return File.Exists(filePath);
        }

        public static IEnumerable<FileInfo> GetJsonFiles(this DirectoryInfo value)
            => value.GetFiles("*.json").Where(f => !f.Name.StartsWith(Constants.Comment));
    }
}
