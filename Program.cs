using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace Coop_Vr
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "D:\\saxion\\ACS\\2nd\\Honour\\Coop-Vr\\Assets\\Documents\\sample_data.csv";

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                // Read each record as a dynamic object
                while (csv.Read())
                {
                    var record = csv.GetRecord<dynamic>() as IDictionary<string, object>;

                    // Extract known keys (X, Y, Z) if present
                    float x = record.ContainsKey("X") ? float.Parse(record["X"].ToString(), CultureInfo.InvariantCulture) : 0;
                    float y = record.ContainsKey("Y") ? float.Parse(record["Y"].ToString(), CultureInfo.InvariantCulture) : 0;
                    float? z = record.ContainsKey("Z") && !string.IsNullOrWhiteSpace(record["Z"].ToString())
                        ? float.Parse(record["Z"].ToString(), CultureInfo.InvariantCulture)
                        : (float?)null;

                    // Create a dictionary for all other keys
                    var extraInfo = new Dictionary<string, string>();
                    foreach (var key in record.Keys)
                    {
                        if (key != "X" && key != "Y" && key != "Z")
                        {
                            extraInfo[key] = record[key]?.ToString();
                        }
                    }

                    // Output data
                    Console.WriteLine($"X: {x}, Y: {y}, Z: {z}");
                    Console.WriteLine("Extra Info:");
                    foreach (var info in extraInfo)
                    {
                        Console.WriteLine($"{info.Key}: {info.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file: {ex.Message}");
            }
        }
    }
}


// Register


//string filePath = "D:\\saxion\\ACS\\2nd\\Honour\\Coop-Vr\\Assets\\Documents\\sample_data.csv"; // Replace with the actual file path

//try
//{
//    var graphData = ParseCsvToGraphData(filePath);

//    foreach (var entry in graphData)
//    {
//        Console.WriteLine($"Title: {entry.Key}");
//        Console.WriteLine($"X: {entry.Value.X}, Y: {entry.Value.Y}, Z: {entry.Value.Z}");
//        Console.WriteLine($"Extra Info: Country: {entry.Value.ExtraInfo["country"]}, City: {entry.Value.ExtraInfo["city"]}, Start Year: {entry.Value.ExtraInfo["start_year"]}");
//        Console.WriteLine();
//    }
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"An error occurred: {ex.Message}");
//}
//// Initialize StereoKit
//SKSettings settings = new SKSettings
//{
//    appName = "Coop_Vr",
//    assetsFolder = "Assets",
//};
//if (!SK.Initialize(settings))
//    Environment.Exit(1);

//ClientStateMachine setup = new();

//while (SK.Step(() =>
//{
//    setup.Update();
//})) ;
//SK.Shutdown();
//setup.StopRunning();
//    }
//}
