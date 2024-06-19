using Etherna.BeeNet.Services;
using System.Security.Cryptography;

namespace CompactedChunkStatsGenerator
{
    class Program
    {
        public static readonly int[] TestFilesSize =
        [
            1024 * 1024 * 100,       //100MB
            1024 * 1024 * 200,       //200MB
            1024 * 1024 * 500,       //500MB
            1024 * 1024 * 1024,      //1GB
            1024 * 1024 * 1023 * 2   //~2GB
        ];
        
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("CSV output file argument is required");
                return;
            }

            var outputCsvPath = args[0];

            foreach (var fileSize in TestFilesSize)
            {
                // Generate random file (in memory).
                var data = RandomNumberGenerator.GetBytes(fileSize);
                
                // Test without and with compaction.
                var withoutResult = await RunTestAsync(data, false);
                var withResult = await RunTestAsync(data, true);
            }
        }

        private static async Task<(int requiredDepth, TimeSpan duration)> RunTestAsync(
            byte[] data,
            bool useCompaction)
        {
            Console.WriteLine($"Testing data {data.Length} bytes length, {(useCompaction ? "with" : "without")} compaction");
            
            var start = DateTime.UtcNow;
        
            var fileService = new CalculatorService();
            var result = await fileService.EvaluateFileUploadAsync(
                data,
                "text/plain",
                "testFile.txt",
                useCompaction ? 100 : 0);
        
            var duration = DateTime.UtcNow - start;
            
            Console.WriteLine($"Process took {duration.TotalSeconds} seconds");
            Console.WriteLine($"Required depth: {result.RequiredPostageBatchDepth}");

            return (result.RequiredPostageBatchDepth, duration);
        }
    }
}