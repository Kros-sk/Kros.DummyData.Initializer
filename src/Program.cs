using Kros.IO;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Kros.DummyData.Initializer
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var main = new RootCommand
            {
                CreateRunCommand(),
                CreatePreviewCommand(),
                new Command("info").WithHandler(CommandHandler.Create(PrintInfo))
            };

            await main.InvokeAsync(args);
        }

        private static Command CreateRunCommand()
            => new Command("run", "Run posting mock data to your api.")
            {
                new Option<DirectoryInfo>(
                   new string[]{ "--source", "-s"},
                   () => new DirectoryInfo(Environment.CurrentDirectory),
                   "Directory where the initialization data is located."),
                new Option<bool>(
                   new [] { "--verbose", "-v"},
                   () => false,
                   "Verbose")
            }.WithHandler(CommandHandler.Create<DirectoryInfo, bool>(InitializerRun));

        private static Command CreatePreviewCommand()
            => new Command("preview", "Run generating preview data.")
            {
                new Option<DirectoryInfo>(
                   new string[]{ "--source", "-s"},
                   () => new DirectoryInfo(Environment.CurrentDirectory),
                   "Directory where the initialization data is located."),
                new Option<DirectoryInfo>(
                   new string[]{ "--dest", "-d"},
                   () => new DirectoryInfo(PathHelper.BuildPath(Environment.CurrentDirectory, "output")),
                   "Directory where the generated data will be saved.")
            }.WithHandler(CommandHandler.Create<DirectoryInfo, DirectoryInfo>(Preview));

        private async static Task<int> Preview(DirectoryInfo source, DirectoryInfo dest)
        {
            using ILoggerFactory loggerFactory = CreateLoggerFactory(false);
            try
            {
                var context = await InitializerContext.CreateAsync(source, loggerFactory);

                await DataGenerator.GenerateAsync(context, dest);

                return 0;
            }
            catch (Exception ex)
            {
                ILogger logger = loggerFactory.CreateLogger("Initialization");
                logger.LogError(ex, ex.Message);
                return -1;
            }
        }

        private static ILoggerFactory CreateLoggerFactory(bool verbose) => LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                options.TimestampFormat = "hh:mm:ss ";
            }).SetMinimumLevel(verbose? LogLevel.Trace: LogLevel.Information);
        });

        private async static Task<int> InitializerRun(DirectoryInfo source, bool verbose)
        {
            using ILoggerFactory loggerFactory = CreateLoggerFactory(verbose);
            try
            {
                var context = await InitializerContext.CreateAsync(source, loggerFactory);

                await Initializer.RunAsync(context);

                return 0;
            }
            catch (Exception ex)
            {
                ILogger logger = loggerFactory.CreateLogger("Initialization");
                logger.LogError(ex, ex.Message);
                return -1;
            }
        }

        static void PrintInfo()
        {
            Console.WriteLine(@"
  _  _______   ____   _____                 
 | |/ /  __ \ / __ \ / ____|                
 | ' /| |__) | |  | | (___     __ _   ___   
 |  < |  _  /| |  | |\___ \   / _` | / __|  
 | . \| | \ \| |__| |____) | | (_| |_\__ \_ 
 |_|\_\_|  \_\\____/|_____/   \__,_(_)___(_)
                                            
                                            
");
            Console.WriteLine(@"
     _       _                             _    
    | |     | |                           | |   
  __| | __ _| |_ __ _ _ __ ___   ___   ___| | __
 / _` |/ _` | __/ _` | '_ ` _ \ / _ \ / __| |/ /
| (_| | (_| | || (_| | | | | | | (_) | (__|   < 
 \__,_|\__,_|\__\__,_|_| |_| |_|\___/ \___|_|\_\
                                                
                                                
");
        }
    }
}
