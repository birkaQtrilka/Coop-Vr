using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;        // For JSON parsing
using CsvHelper;             // For CSV parsing
using ClosedXML.Excel;       // For Excel parsing
using System.Globalization;  // For CsvHelper configuration

namespace SteroKitDataPlotter.Core
{
    public class DataPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; } // Optional for 3D data

        // Constructor
        public DataPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}

    //public class DataLoader
    //{
    //    // Unified data structure for parsed content
        

    //    // Entry point to parse any file
    //    public List<DataPoint> ParseFile(string filePath)
    //    {
    //        string extension = Path.GetExtension(filePath).ToLower();
    //        switch (extension)
    //        {
    //            case ".csv":
    //                return ParseCsv(filePath);
    //            case ".json":
    //                return ParseJson(filePath);
    //            case ".xlsx":
    //                return ParseExcel(filePath);
    //            default:
    //                throw new NotSupportedException($"Unsupported file format: {extension}");
    //        }
    //    }

    //    // Parsing CSV files
    //    private List<DataPoint> ParseCsv(string filePath)
    //    {
    //        var dataPoints = new List<DataPoint>();
    //        using (var reader = new StreamReader(filePath))
    //        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    //        {
    //            csv.Read();
    //            csv.ReadHeader();
    //            while (csv.Read())
    //            {
    //                dataPoints.Add(new DataPoint
    //                {
    //                    X = csv.GetField<float>("X"),
    //                    Y = csv.GetField<float>("Y"),
    //                    Z = csv.GetField<float>("Z")
    //                });
    //            }
    //        }
    //        return dataPoints;
    //    }

    //    // Parsing JSON files
    //    private List<DataPoint> ParseJson(string filePath)
    //    {
    //        string jsonData = File.ReadAllText(filePath);
    //        return JsonConvert.DeserializeObject<List<DataPoint>>(jsonData);
    //    }

    //    // Parsing Excel files
    //    private List<DataPoint> ParseExcel(string filePath)
    //    {
    //        var dataPoints = new List<DataPoint>();

    //        try
    //        {
    //            // Load the Excel workbook
    //            using (var workbook = new XLWorkbook(filePath))
    //            {
    //                var worksheet = workbook.Worksheet(1); // Access the first worksheet

    //                // Get all rows with data
    //                var rows = worksheet.RowsUsed();

    //                bool isHeaderRow = true; // Flag for skipping the first (header) row
    //                foreach (var row in rows)
    //                {
    //                    if (isHeaderRow)
    //                    {
    //                        isHeaderRow = false; // Skip the header row
    //                        continue;
    //                    }

    //                    // Parse each row into a DataPoint
    //                    dataPoints.Add(new DataPoint
    //                    {
    //                        X = row.Cell(1).GetValue<float>(),
    //                        Y = row.Cell(2).GetValue<float>(),
    //                        Z = row.Cell(3).GetValue<float>()
    //                    });
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Error parsing Excel file: {ex.Message}");
    //        }

    //        return dataPoints;
    //    }

    //}
//}
