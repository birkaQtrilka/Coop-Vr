using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Coop_Vr.Networking.ServerSide.Components
{

    public class FileHandler
    {
        public string FilePath { get; set; }

        // Constructor to initialize FilePath
        public FileHandler(string filePath)
        {
            FilePath = filePath;
        }


        // Method to read graph points from the CSV file
        public List<GraphPoint> ReadGraphPointsFromCsv()
        {
            var graphPoints = new List<GraphPoint>();
            var records = new List<IDictionary<string, object>>();

            try
            {
                using var reader = new StreamReader(FilePath); // Use the FilePath property
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                while (csv.Read())
                {
                    var record = csv.GetRecord<dynamic>() as IDictionary<string, object>;
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }

                // Calculate influence scores using XRFomula
                var influenceScores = CalculateInfluenceScore(records, 4);

                // Create GraphPoint objects from the influence scores
                foreach (var score in influenceScores)
                {
                    var graphPoint = GraphPoint.FromCsvRecord(score);
                    graphPoints.Add(graphPoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            return graphPoints;
        }

        private IEnumerable<Dictionary<string, object>> CalculateInfluenceScore(List<IDictionary<string, object>> records, int numberOfProject)
        {
            var maxCapacity = records.Max(p => double.TryParse(p["Capacity (kt H2/y)"]?.ToString(), out double capacity) ? capacity : 0);
            var influenceScores = new List<Dictionary<string, object>>();

            //for (int i = 0; i < records.Count(); i++)
            for (int i = 0; i < numberOfProject; i++)
            {
                var project1 = records[i];
                //for (int j = 0; j < records.Count(); j++)
                for (int j = 0; j < numberOfProject; j++)
                {
                    if (i != j)
                    {
                        var project2 = records[j];

                        // Technology similarity (T_ij)
                        double T = project1["Technology"].Equals(project2["Technology"]) ? 1 : 0;

                        // Geography similarity (G_ij)
                        double G = project1["Country"].Equals(project2["Country"]) ? 1 : 0;

                        // Capacity similarity (C_ij)
                        double C = 1 - Math.Abs((double.TryParse(project1["Capacity (kt H2/y)"]?.ToString(), out double capacity1) ? capacity1 : 0) -
                                                (double.TryParse(project2["Capacity (kt H2/y)"]?.ToString(), out double capacity2) ? capacity2 : 0)) / maxCapacity;

                        // Calculate influence score
                        double S = 0.5 * T + 0.3 * G + 0.2 * C;

                        influenceScores.Add(new Dictionary<string, object>
                        {
                            { "Project1", project1["Project Name"] },
                            { "Score", S },
                            { "Technology1", project1["Technology"] },
                            { "Country1", project1["Country"] },
                            { "Capacity1", project1["Capacity (kt H2/y)"] },
                            { "Investment1", project1["Investment Cost (MUSD)"] },
                            { "Date1", project1["Date Online"] },
                            { "Project2", project2["Project Name"] },
                            { "Technology2", project2["Technology"] },
                            { "Country2", project2["Country"] },
                            { "Capacity2", project2["Capacity (kt H2/y)"] },
                            { "Investment2", project2["Investment Cost (MUSD)"] },
                            { "Date2", project2["Date Online"] }
                        });
                    }
                }
            }
            return influenceScores;
        }

        /// <summary>
        /// Scale the graph points to fit within a specified range
        /// </summary>
        /// <param name="points"></param>
        /// <param name="scale"></param>
        public void ScaleGraphPoints(List<GraphPoint> points, float scale)
        {
            if (points == null || !points.Any()) return;

            var (xMin, xMax, yMin, yMax, zMin, zMax) = GetMinMaxValues(points);

            // Apply normalization
            foreach (var point in points)
            {
                point.X = (point.X - xMin) / (xMax - xMin) * scale;
                point.Y = (point.Y - yMin) / (yMax - yMin) * scale;
                point.Z = (point.Z - zMin) / (zMax - zMin) * scale;
            }
        }

        /// <summary>
        /// Reverse the scaling of the graph points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="scale"></param>
        public void ReverseScaleGraphPoints(List<GraphPoint> points, float scale)
        {
            if (points == null || !points.Any()) return;

            var (xMin, xMax, yMin, yMax, zMin, zMax) = GetMinMaxValues(points);

            // Apply normalization
            foreach (var point in points)
            {
                point.X = point.X * (xMax - xMin) / scale + xMin;
                point.Y = point.Y * (yMax - yMin) / scale + yMin;
                point.Z = point.Z * (zMax - zMin) / scale + zMin;
            }
        }

        /// <summary>
        /// Helper method to get min and max values for X, Y, and Z coordinates
        /// </summary>
        private (float xMin, float xMax, float yMin, float yMax, float zMin, float zMax) GetMinMaxValues(List<GraphPoint> points)
        {
            var xValues = points.Select(p => p.X).ToList();
            var yValues = points.Select(p => p.Y).ToList();
            var zValues = points.Select(p => p.Z).ToList();

            float xMin = xValues.Min(), xMax = xValues.Max();
            float yMin = yValues.Min(), yMax = yValues.Max();
            float zMin = zValues.Min(), zMax = zValues.Max();

            return (xMin, xMax, yMin, yMax, zMin, zMax);
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
