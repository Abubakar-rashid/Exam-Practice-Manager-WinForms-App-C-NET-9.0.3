namespace ExamPracticeSystemFull.Models
{
    using System;
    using System.Text.Json;
    using System.Collections.Generic;
    using System.Linq; // Required for LINQ methods like ToDictionary, Select, etc.
    // using System.Web.Script.Serialization; // For Dictionary serialization, assuming it's available in WinForms.
                                        // If not, consider Newtonsoft.Json or custom serialization.
                                        // For simplicity, a basic string-based dictionary serialization is implemented.

    /// <summary>
    /// Stores the results of a student's exam attempt.
    /// </summary>
    public class ExamResult
    {
        public int ResultID { get; set; }
        public string StudentUsername { get; set; }
        public int ExamID { get; set; }
        public double Score { get; set; } // Percentage score
        public DateTime DateTaken { get; set; }
        public int TimeTaken { get; set; } // in seconds
        public Dictionary<int, string> StudentAnswers { get; set; } // QuestionID -> Student's Answer (e.g., "A", "B", "C", "D" or text)
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }

        /// <summary>
        /// Default constructor, initializes StudentAnswers and DateTaken.
        /// </summary>
        public ExamResult()
        {
            StudentAnswers = new Dictionary<int, string>();
            DateTaken = DateTime.Now;
        }

        /// <summary>
        /// Parameterized constructor for creating a new exam result.
        /// Automatically calculates the score.
        /// </summary>
        public ExamResult(string studentUsername, int examID, Dictionary<int, string> studentAnswers,
                         int totalQuestions, int correctAnswers, int timeTaken)
        {
            ResultID = 0; // Will be set by ExamService or a similar service before saving
            StudentUsername = studentUsername;
            ExamID = examID;
            StudentAnswers = studentAnswers ?? new Dictionary<int, string>();
            TotalQuestions = totalQuestions;
            CorrectAnswers = correctAnswers;
            TimeTaken = timeTaken;
            DateTaken = DateTime.Now;
            Score = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;
        }

        /// <summary>
        /// Calculates the percentage score based on correct answers and total questions.
        /// </summary>
        /// <returns>The calculated score as a double.</returns>
        public double CalculatePercentage()
        {
            return TotalQuestions > 0 ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
        }

        /// <summary>
        /// Converts the ExamResult object to a CSV string.
        /// StudentAnswers dictionary is serialized into a string.
        /// </summary>
        /// <returns>A CSV formatted string representation of the exam result.</returns>
        public string ToCsvString()
        {
            // Serialize the StudentAnswers dictionary into a string format.
            // Example format: "1:A;2:C;3:B" for QuestionID:Answer
            // Using a simple colon-separated key-value pair and semicolon-separated entries.
            // If answers themselves can contain colons or semicolons, a more robust serialization (e.g., JSON) is needed.
            string studentAnswersString = string.Join(";", StudentAnswers.Select(kvp => $"{kvp.Key}:{EscapeCsvField(kvp.Value)}"));

            // ResultID,StudentUsername,ExamID,Score,DateTaken,TimeTaken,TotalQuestions,CorrectAnswers,StudentAnswers
            return $"{ResultID},{StudentUsername},{ExamID},{Score:F2},{DateTaken:yyyy-MM-dd HH:mm:ss},{TimeTaken}," +
                   $"{TotalQuestions},{CorrectAnswers},{EscapeCsvField(studentAnswersString)}";
        }

        /// <summary>
        /// Creates an ExamResult object from a CSV string.
        /// Handles deserialization of the StudentAnswers dictionary.
        /// </summary>
        /// <param name="csvLine">The CSV line to parse.</param>
        /// <returns>An ExamResult object, or null if parsing fails.</returns>
        public static ExamResult FromCsvString(string csvLine)
        {
            var values = ParseCsvLine(csvLine); // Use the robust CSV line parser
            if (values.Length >= 9) // Ensure all 9 fields are present
            {
                try
                {
                    // Deserialize the StudentAnswers string back into a Dictionary<int, string>
                    Dictionary<int, string> studentAnswers = new Dictionary<int, string>();
                    string studentAnswersRaw = UnescapeCsvField(values[8]);
                    if (!string.IsNullOrEmpty(studentAnswersRaw))
                    {
                        foreach (var entry in studentAnswersRaw.Split(';'))
                        {
                            var parts = entry.Split(new char[] { ':' }, 2); // Split only on the first colon
                            if (parts.Length == 2 && int.TryParse(parts[0], out int qid))
                            {
                                studentAnswers[qid] = UnescapeCsvField(parts[1]);
                            }
                        }
                    }

                    return new ExamResult
                    {
                        ResultID = int.Parse(values[0]),
                        StudentUsername = values[1],
                        ExamID = int.Parse(values[2]),
                        Score = double.Parse(values[3]),
                        DateTaken = DateTime.Parse(values[4]),
                        TimeTaken = int.Parse(values[5]),
                        TotalQuestions = int.Parse(values[6]),
                        CorrectAnswers = int.Parse(values[7]),
                        StudentAnswers = studentAnswers // Assign the deserialized dictionary
                    };
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error parsing exam result CSV line '{csvLine}': {ex.Message}");
                    return null;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error parsing argument in exam result CSV line '{csvLine}': {ex.Message}");
                    return null;
                }
            }
            return null; // Invalid CSV line
        }

        /// <summary>
        /// Helper method to escape strings for CSV output.
        /// Encloses the field in double quotes if it contains commas or double quotes,
        /// and doubles any existing double quotes.
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains(";") || field.Contains("\n") || field.Contains("\r")) // Added semicolon and newline checks
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        /// <summary>
        /// Helper method to unescape strings from CSV input.
        /// Removes enclosing double quotes and unflattens doubled double quotes.
        /// </summary>
        private static string UnescapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.StartsWith("\"") && field.EndsWith("\""))
            {
                return field.Substring(1, field.Length - 2).Replace("\"\"", "\"");
            }
            return field;
        }

        /// <summary>
        /// Parses a CSV line into an array of strings, handling quoted fields and escaped quotes.
        /// This is a robust CSV parser that can handle commas within quoted fields.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <returns>An array of parsed fields.</returns>
        private static string[] ParseCsvLine(string line)
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
                        currentField += '"';
                        i++; // Skip the next quote as it's part of the escape sequence
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            
            result.Add(currentField);
            return result.ToArray();
        }

        /// <summary>
        /// Overrides the ToString() method for a user-friendly representation of the exam result.
        /// </summary>
        /// <returns>A string representing the score and date taken.</returns>
        public override string ToString()
        {
            return $"Score: {Score:F1}% ({CorrectAnswers}/{TotalQuestions}) - {DateTaken:yyyy-MM-dd}";
        }
    }
}
