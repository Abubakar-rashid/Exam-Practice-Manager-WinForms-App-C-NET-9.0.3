using ExamPracticeSystemFull.Services;

namespace ExamPracticeSystemFull.Models
{
    using System;
    using System.Linq;

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public string Email { get; set; }
        public string IDNumber { get; set; }
        public DateTime CreatedDate { get; set; }

        public User()
        {
            CreatedDate = DateTime.Now;
        }

        public User(string username, string password, UserRole role, string email, string idNumber)
        {
            Username = username;
            Password = password;
            Role = role;
            Email = email;
            IDNumber = idNumber;
            CreatedDate = DateTime.Now;
        }

        public string ToCsvString()
        {
            // Assuming no commas/quotes in Username, Password, Role, IDNumber for simplicity in this model.
            // Email might contain commas, so let's escape it for robustness.
            return $"{Username},{Password},{Role},{CsvDataService.EscapeCsvField(Email)},{IDNumber},{CreatedDate:yyyy-MM-dd HH:mm:ss}";
        }

        public static User FromCsvString(string csvLine)
        {
            var values = CsvDataService.ParseCsvLine(csvLine); // Using shared ParseCsvLine from CsvDataService
            if (values.Length >= 6)
            {
                try
                {
                    return new User
                    {
                        Username = values[0],
                        Password = values[1],
                        Role = (UserRole)Enum.Parse(typeof(UserRole), values[2]),
                        Email = values[3],
                        IDNumber = values[4],
                        CreatedDate = DateTime.Parse(values[5])
                    };
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error parsing user CSV line '{csvLine}': {ex.Message}");
                    return null;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error parsing user role in CSV line '{csvLine}': {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public override string ToString()
        {
            return $"{Username} ({Role})";
        }
    }

    public enum UserRole
    {
        Student,
        Lecturer
    }
}