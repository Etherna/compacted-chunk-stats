using Etherna.BeeNet.Services;
using System.Security.Cryptography;

namespace CompactedChunkStatsGenerator
{
    class Program
    {
        public const int DefaultIterations = 10;
        
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
            // Run with several iteration if required to output stats.
            // Otherwise, run only once for demo.
            var iterations = args.Length == 0 ? DefaultIterations : int.Parse(args[0]);

            foreach (var fileSize in TestFilesSize)
            {
                var totalRequiredDepth_normal = 0;
                var totalTime_normal = new TimeSpan();
                var totalRequiredDepth_compacted = 0;
                var totalTime_compacted = new TimeSpan();

                for (int i = 0; i < iterations; i++)
                {
                    Console.WriteLine($"Testing data {fileSize} bytes length, iteration {i}");
                
                    // Generate random file (in memory).
                    var data = RandomNumberGenerator.GetBytes(fileSize);
                
                    // Test without and with compaction.
                    Console.WriteLine("Without compaction:");
                    var withoutResult = await RunTestAsync(data, false);
                    totalTime_normal += withoutResult.duration;
                    totalRequiredDepth_normal += withoutResult.requiredDepth;
                
                    Console.WriteLine();
                    Console.WriteLine("With compaction:");
                    var withResult = await RunTestAsync(data, true);
                    totalTime_compacted += withResult.duration;
                    totalRequiredDepth_compacted += withResult.requiredDepth;
                
                    Console.WriteLine("-----");
                }

                var averageRequiredDepth_normal = (double)totalRequiredDepth_normal / iterations;
                var averageTime_normal = totalTime_normal / iterations;
                var averageRequiredDepth_compacted = (double)totalRequiredDepth_compacted / iterations;
                var averageTime_compacted = totalTime_compacted / iterations;
                
                Console.WriteLine();
                Console.WriteLine($"  Average duration without compaction: {averageTime_normal} seconds");
                Console.WriteLine($"  Average required depth without compaction: {averageRequiredDepth_normal}");
                Console.WriteLine($"  Average duration with compaction: {averageTime_compacted} seconds");
                Console.WriteLine($"  Average required depth with compaction: {averageRequiredDepth_compacted}");
                Console.WriteLine();
            }
        }

        private static async Task<(int requiredDepth, TimeSpan duration)> RunTestAsync(
            byte[] data,
            bool useCompaction)
        {   
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