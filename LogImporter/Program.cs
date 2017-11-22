using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NLog;

namespace LogImporter
{
    class Program
    {

        private static Logger _logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            Repository.EnsureDatabaseTable();

            var applicationName = ConfigurationManager.AppSettings["ApplicationName"];
            var directory = ConfigurationManager.AppSettings["LogDirectory"];
            
            //create the pipeline blocks
            var readDirectoryBlock = new TransformManyBlock<string, string>((x) => Directory.GetFiles(x, "*.log", SearchOption.TopDirectoryOnly),
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
            var parseBlock = new TransformManyBlock<string, LogFileLine>((x) => LogFileParser.ParseLogFile(applicationName, x), 
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
            var saveBlock = new ActionBlock<LogFileLine>(async (x) => await Repository.AddLog(x), 
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 5 });

            //Connect the pipeline
            readDirectoryBlock.LinkTo(parseBlock, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });
            parseBlock.LinkTo(saveBlock, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });

            // start the workflow
            readDirectoryBlock.Post(directory);
            readDirectoryBlock.Complete();

            //wait for the final block to complete
            saveBlock.Completion.Wait();

            Console.WriteLine("Done. Press any key to close.");
            Console.ReadKey();
        }
    }
}
