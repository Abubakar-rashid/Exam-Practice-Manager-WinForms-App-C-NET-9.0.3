namespace ExamPracticeSystemFull.Models
{
    using System;
    using System.Collections.Generic; // Required for List
    using System.Linq; // Required for LINQ methods like .All() or .Select() if used in future

    /// <summary>
    /// Represents an exam question with multiple choice options, category, and difficulty.
    /// </summary>
    public class Question
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAnswer { get; set; } // Stores the correct option letter (e.g., "A", "B", "C", "D")
        public string Category { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public QuestionType Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Default constructor initializes CreatedDate.
        /// </summary>
        public Question()
        {
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Parameterized constructor for creating a new question.
        /// </summary>
        public Question(string text, string optionA, string optionB, string optionC, string optionD,
                       string correctAnswer, string category, DifficultyLevel difficulty,
                       QuestionType type, string createdBy)
        {
            Text = text;
            OptionA = optionA;
            OptionB = optionB;
            OptionC = optionC;
            OptionD = optionD;
            CorrectAnswer = correctAnswer;
            Category = category;
            Difficulty = difficulty;
            Type = type;
            CreatedBy = createdBy;
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Converts the Question object to a CSV string.
        /// Fields are escaped to handle commas/quotes within text, options, or categories.
        /// </summary>
        /// <returns>A CSV formatted string representation of the question.</returns>
        public string ToCsvString()
        {
            // Escape fields that might contain commas or quotes to ensure correct CSV parsing.
            // ID,Text,OptionA,OptionB,OptionC,OptionD,CorrectAnswer,Category,Difficulty,Type,CreatedBy,CreatedDate
            return $"{ID},{EscapeCsvField(Text)},{EscapeCsvField(OptionA)},{EscapeCsvField(OptionB)}," +
                   $"{EscapeCsvField(OptionC)},{EscapeCsvField(OptionD)},{CorrectAnswer}," +
                   $"{EscapeCsvField(Category)},{Difficulty},{Type},{CreatedBy},{CreatedDate:yyyy-MM-dd HH:mm:ss}";
        }

        /// <summary>
        /// Creates a Question object from a CSV string.
        /// Handles unescaping of fields.
        /// </summary>
        /// <param name="csvLine">The CSV line to parse.</param>
        /// <returns>A Question object, or null if parsing fails.</returns>
        public static Question FromCsvString(string csvLine)
        {
            var values = ParseCsvLine(csvLine); // Use the robust CSV line parser
            if (values.Length >= 12) // Ensure all 12 fields are present
            {
                try
                {
                    return new Question
                    {
                        ID = int.Parse(values[0]),
                        Text = UnescapeCsvField(values[1]),
                        OptionA = UnescapeCsvField(values[2]),
                        OptionB = UnescapeCsvField(values[3]),
                        OptionC = UnescapeCsvField(values[4]),
                        OptionD = UnescapeCsvField(values[5]),
                        CorrectAnswer = values[6],
                        Category = UnescapeCsvField(values[7]),
                        Difficulty = (DifficultyLevel)Enum.Parse(typeof(DifficultyLevel), values[8]),
                        Type = (QuestionType)Enum.Parse(typeof(QuestionType), values[9]),
                        CreatedBy = values[10],
                        CreatedDate = DateTime.Parse(values[11])
                    };
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error parsing question CSV line '{csvLine}': {ex.Message}");
                    return null;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error parsing enum in question CSV line '{csvLine}': {ex.Message}");
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
            if (field.Contains(",") || field.Contains("\""))
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
                    // If currently in quotes and next character is also a quote, it's an escaped quote
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField += '"';
                        i++; // Skip the next quote as it's part of the escape sequence
                    }
                    else // Otherwise, it's a quote delimiter, toggle inQuotes
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes) // If comma and not inside quotes, it's a delimiter
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else // Regular character, append to current field
                {
                    currentField += c;
                }
            }
            
            result.Add(currentField); // Add the last field
            return result.ToArray();
        }

        /// <summary>
        /// Overrides the ToString() method for a user-friendly representation of the question.
        /// </summary>
        /// <returns>A string representing the question ID, text, category, and difficulty.</returns>
        public override string ToString()
        {
            return $"Q{ID}: {Text} ({Category} - {Difficulty})";
        }
    }

    /// <summary>
    /// Enum for defining difficulty levels of questions.
    /// </summary>
    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    /// <summary>
    /// Enum for defining types of questions.
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        FillInTheBlank // Add more types as needed
    }
}
