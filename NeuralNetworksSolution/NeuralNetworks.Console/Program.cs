﻿using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace NeuralNetworks.Console
{
    public static class Program
    {
        static void Main()
        {
            ILogger logger = new ConsoleLogger();
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.json")
                .Build();

            // Get topology.
            int[] topology = configuration.GetSection("topology").Get<int[]>();

            // Read data.
            TrainingData[] data;
            string trainingDataFileName = Directory.GetCurrentDirectory() + "\\trainingData.json";
            if (File.Exists(trainingDataFileName))
                data = TrainingData.ReadFromFile(trainingDataFileName);
            else
            {
                data = TrainingData.GenerateData(topology.First(), topology.Last(), 2000);
                TrainingData.WriteToFile(data, trainingDataFileName);
            }

            // Train network.
            Net net = new Net(topology);
            for (int i = 0; i < data.Length; i++)
            {
                logger.LogLine($"Pass: {i + 1}");
                logger.LogLine($"Input: [{string.Join(", ", data[i].In.Select(_ => _.ToString("F3")))}]");
                logger.LogLine($"Targets: [{string.Join(", ", data[i].Out.Select(_ => _.ToString("F3")))}]");
                net.FeedForward(data[i].In);

                var resultVals = net.GetResults();
                logger.LogLine($"Outputs: [{string.Join(", ", resultVals.Select(_ => _.ToString("F3")))}]");

                net.BackProp(data[i].Out);
                logger.LogLine($"Net average error: {net.RecentAverageError.ToString("F9")}\n");
            }

            logger.LogLine("Done");
            System.Console.Read();
        }
    }
}
