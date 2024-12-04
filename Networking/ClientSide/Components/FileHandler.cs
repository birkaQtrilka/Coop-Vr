using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Coop_Vr.Networking.ClientSide.Components
{
    public class GraphPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float? Z { get; set; }
        public Dictionary<string, string> ExtraInfo { get; set; }

        // Factory method to create a GraphPoint from a CSV record
        public static GraphPoint FromCsvRecord(IDictionary<string, object> record)
        {
            var graphPoint = new GraphPoint
            {
                X = record.ContainsKey("X") ? float.Parse(record["X"].ToString(), CultureInfo.InvariantCulture) : 0,
                Y = record.ContainsKey("Y") ? float.Parse(record["Y"].ToString(), CultureInfo.InvariantCulture) : 0,
                Z = record.ContainsKey("Z") && !string.IsNullOrWhiteSpace(record["Z"].ToString())
                    ? float.Parse(record["Z"].ToString(), CultureInfo.InvariantCulture)
                    : (float?)null,
                ExtraInfo = new Dictionary<string, string>()
            };

            foreach (var key in record.Keys)
            {
                if (key != "X" && key != "Y" && key != "Z")
                {
                    graphPoint.ExtraInfo[key] = record[key]?.ToString();
                }
            }

            return graphPoint;
        }
    }
    public class FileHandler
    {
        public string FilePath { get; set; }
        public static List<GraphPoint> ReadGraphPointsFromCsv(ref string filePath)
        {
            var graphPoints = new List<GraphPoint>();

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                while (csv.Read())
                {
                    var record = csv.GetRecord<dynamic>() as IDictionary<string, object>;
                    if (record != null)
                    {
                        var graphPoint = GraphPoint.FromCsvRecord(record);
                        graphPoints.Add(graphPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            return graphPoints;
        }

        public static void ScaleGraphPoints(List<GraphPoint> points)
        {
            // Extract values for scaling
            var xValues = points.Select(p => p.X).ToList();
            var yValues = points.Select(p => p.Y).ToList();
            var zValues = points.Where(p => p.Z.HasValue).Select(p => p.Z.Value).ToList();

            // Calculate min and max for normalization
            float xMin = xValues.Min(), xMax = xValues.Max();
            float yMin = yValues.Min(), yMax = yValues.Max();
            float zMin = zValues.Count > 0 ? zValues.Min() : 0;
            float zMax = zValues.Count > 0 ? zValues.Max() : 1;

            // Apply normalization
            foreach (var point in points)
            {
                point.X = (point.X - xMin) / (xMax - xMin);
                point.Y = (point.Y - yMin) / (yMax - yMin);
                if (point.Z.HasValue)
                {
                    point.Z = (point.Z.Value - zMin) / (zMax - zMin);
                }
            }
        }

        public void WriteToFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                Console.WriteLine("File written successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public string ReadFromFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                Console.WriteLine("File read successfully.");
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
