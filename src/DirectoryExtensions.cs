using Kros.IO;
using System.IO;

namespace Kros.DummyData.Initializer
{
    internal static class DirectoryExtensions
    {
        public static bool TryGetFilePath(this DirectoryInfo value, string path, out string filePath)
        {
            filePath = PathHelper.BuildPath(value.FullName, path);

            return File.Exists(filePath);
        }
    }
}
