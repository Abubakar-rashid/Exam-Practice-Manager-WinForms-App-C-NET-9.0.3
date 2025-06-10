namespace ExamPracticeSystemFull.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class CsvDataService
    {
        private readonly string _dataFolder;

        public CsvDataService(string dataFolder = "Data")
        {
            _dataFolder = dataFolder;
            EnsureDataFolderExists();
        }

        private void EnsureDataFolderExists()
        {
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }
        }

        public List<string> ReadAllLines(string fileName)
        {
            string filePath = Path.Combine(_dataFolder, fileName);
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose(); // Create empty file if it doesn't exist
                return new List<string>();
            }

            try
            {
                return File.ReadAllLines(filePath).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading from file {fileName}: {ex.Message}", ex);
            }
        }

        public void WriteAllLines(string fileName, IEnumerable<string> lines)
        {
            string filePath = Path.Combine(_dataFolder, fileName);
            try
            {
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing to file {fileName}: {ex.Message}", ex);
            }
        }

        public void AppendLine(string fileName, string line)
        {
            string filePath = Path.Combine(_dataFolder, fileName);
            try
            {
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error appending to file {fileName}: {ex.Message}", ex);
            }
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(Path.Combine(_dataFolder, fileName));
        }

        public long GetFileSize(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_dataFolder, fileName);
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    return fileInfo.Length;
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting file size for {fileName}: {ex.Message}");
            }
        }

        public DateTime GetLastModifiedTime(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_dataFolder, fileName);
                if (File.Exists(filePath))
                {
                    return File.GetLastWriteTime(filePath);
                }
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting last modified time for {fileName}: {ex.Message}");
            }
        }

        // --- Shared Static CSV Parsing and Escaping Utilities ---

        /// <summary>
        /// Parses a single CSV line, handling quoted fields and escaped quotes within fields.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <returns>An array of strings representing the fields in the CSV line.</returns>
        public static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Encountered a double quote inside a quoted field, it's an escaped quote
                        currentField += '"';
                        i++; // Skip the next quote as it's part of the escape sequence
                    }
                    else
                    {
                        // Toggle inQuotes state (start or end of a quoted field)
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // If comma and not inside quotes, it's a delimiter
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    // Regular character, append to current field
                    currentField += c;
                }
            }

            result.Add(currentField); // Add the last field
            return result.ToArray();
        }

        /// <summary>
        /// Escapes a string for CSV format by enclosing it in double quotes
        /// and doubling any internal double quotes, if the string contains
        /// commas, double quotes, or newlines.
        /// </summary>
        /// <param name="field">The string field to escape.</param>
        /// <returns>The escaped string suitable for CSV.</returns>
        public static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return "";
            }
            // Check if the field needs escaping (contains comma, double quote, or newline)
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                // Double any existing double quotes and enclose in quotes
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field; // No escaping needed
        }
    }
}