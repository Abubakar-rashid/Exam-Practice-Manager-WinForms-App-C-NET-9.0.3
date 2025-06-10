namespace ExamPracticeSystemFull.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an exam with a list of question IDs, metadata, and time limit.
    /// </summary>
    public class Exam
    {
        public int ExamID { get; set; }
        public string Name { get; set; }
        public List<int> QuestionIDs { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TimeLimit { get; set; } // in minutes
        public string Description { get; set; }

        /// <summary>
        /// Default constructor, initializes QuestionIDs and CreatedDate.
        /// </summary>
        public Exam()
        {
            QuestionIDs = new List<int>();
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Parameterized constructor for creating a new exam.
        /// </summary>
        public Exam(string name, List<int> questionIDs, string createdBy, int timeLimit = 60, string description = "")
        {
            Name = name;
            QuestionIDs = questionIDs ?? new List<int>(); // Ensure QuestionIDs is not null
            CreatedBy = createdBy;
            TimeLimit = timeLimit;
            Description = description;
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Converts the Exam object to a CSV string.
        /// QuestionIDs are joined by semicolons. Name and Description are escaped.
        /// </summary>
        /// <returns>A CSV formatted string representation of the exam.</returns>
        public string ToCsvString()
        {
            // Join QuestionIDs with a semicolon
            string questionIdString = string.Join(";", QuestionIDs);

            // ExamID,Name,QuestionIDs,CreatedBy,CreatedDate,TimeLimit,Description
            return $"{ExamID},{EscapeCsvField(Name)},{EscapeCsvField(questionIdString)},{CreatedBy}," +
                   $"{CreatedDate:yyyy-MM-dd HH:mm:ss},{TimeLimit},{EscapeCsvField(Description)}";
        }

        /// <summary>
        /// Creates an Exam object from a CSV string.
        /// Handles unescaping of fields and parsing of QuestionIDs.
        /// </summary>
        /// <param name="csvLine">The CSV line to parse.</param>
        /// <returns>An Exam object, or null if parsing fails.</returns>
        public static Exam FromCsvString(string csvLine)
        {
            var values = ParseCsvLine(csvLine); // Use the robust CSV line parser
            if (values.Length >= 7) // Ensure all 7 fields are present
            {
                try
                {
                    // Parse QuestionIDs string back into a List<int>
                    List<int> parsedQuestionIDs = new List<int>();
                    string questionIDsRaw = UnescapeCsvField(values[2]);
                    if (!string.IsNullOrEmpty(questionIDsRaw))
                    {
                        parsedQuestionIDs = questionIDsRaw.Split(';')
                                                        .Select(int.Parse)
                                                        .ToList();
                    }

                    return new Exam
                    {
                        ExamID = int.Parse(values[0]),
                        Name = UnescapeCsvField(values[1]),
                        QuestionIDs = parsedQuestionIDs,
                        CreatedBy = values[3],
                        CreatedDate = DateTime.Parse(values[4]),
                        TimeLimit = int.Parse(values[5]),
                        Description = UnescapeCsvField(values[6])
                    };
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error parsing exam CSV line '{csvLine}': {ex.Message}");
                    return null;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error parsing argument in exam CSV line '{csvLine}': {ex.Message}");
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
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r")) // Added newline checks
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
        /// Overrides the ToString() method for a user-friendly representation of the exam.
        /// </summary>
        /// <returns>A string representing the exam name and number of questions.</returns>
        public override string ToString()
        {
            return $"{Name} ({QuestionIDs.Count} questions)";
        }
    }
}
