using Kros.IO;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    internal static class DataGenerator
    {
        public async static Task GenerateAsync(InitializerContext context, DirectoryInfo destination)
        {
            context.Logger.LogInformation("Generating start.");

            await foreach (Request request in context.GetRequestsAsync())
            {
                foreach (RepeatDefinition repeat in request.Repeats)
                {
                    await foreach ((FileInfo fileInfo, string content) in context.GetFiles(request, repeat))
                    {
                        string directory = PathHelper.BuildPath(destination.FullName, request.Directory.Name, repeat.Name);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        string path = PathHelper.BuildPath(directory, fileInfo.Name);
                        await File.WriteAllTextAsync(path, content);
                    }
                }
            }

            context.Logger.LogInformation("Generating end.");
        }
    }
}
