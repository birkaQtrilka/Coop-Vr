using System;
using System.IO;

namespace Coop_Vr.Networking.ClientSide.Components
{
    internal class FileHandler
    {
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
