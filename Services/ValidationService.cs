namespace ExamPracticeSystemFull.Services
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class ValidationService
    {
        public static ValidationResult ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return new ValidationResult(false, "Username cannot be empty.");

            if (username.Length < 6 || username.Length > 8)
                return new ValidationResult(false, "Username must be 6-8 characters long.");

            int digitCount = username.Count(char.IsDigit);
            if (digitCount > 2)
                return new ValidationResult(false, "Username can have at most 2 digits.");

            int letterCount = username.Count(char.IsLetter);
            if (letterCount + digitCount != username.Length)
                return new ValidationResult(false, "Username can only contain letters and digits.");

            if (letterCount == 0)
                return new ValidationResult(false, "Username must contain at least one letter.");

            // Check if it's English letters only (ASCII range for letters)
            if (!username.All(c => char.IsLetterOrDigit(c) && (char.IsLetter(c) ? (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') : true)))
                return new ValidationResult(false, "Username contains invalid characters. Only English letters and digits are allowed.");

            return new ValidationResult(true, "Username is valid.");
        }

        public static ValidationResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return new ValidationResult(false, "Password cannot be empty.");

            if (password.Length < 8)
                return new ValidationResult(false, "Password must be at least 8 characters long.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return new ValidationResult(false, "Password must contain at least one uppercase letter.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                return new ValidationResult(false, "Password must contain at least one lowercase letter.");

            if (!Regex.IsMatch(password, @"\d"))
                return new ValidationResult(false, "Password must contain at least one digit.");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
                return new ValidationResult(false, "Password must contain at least one special character.");

            return new ValidationResult(true, "Password is valid.");
        }

        public static ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new ValidationResult(false, "Email cannot be empty.");

            // A more comprehensive regex for email validation
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
                return new ValidationResult(false, "Invalid email format.");

            return new ValidationResult(true, "Email is valid.");
        }

        public static ValidationResult ValidateIDNumber(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
                return new ValidationResult(false, "ID Number cannot be empty.");

            // Example: Assuming ID Number is 5 digits long. Adjust as per actual requirement.
            if (!Regex.IsMatch(idNumber, @"^\d{5}$"))
                return new ValidationResult(false, "ID Number must be a 5-digit number.");

            return new ValidationResult(true, "ID Number is valid.");
        }

        public static ValidationResult ValidateQuestionText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ValidationResult(false, "Question text cannot be empty.");
            if (text.Length < 5)
                return new ValidationResult(false, "Question text must be at least 5 characters long.");
            return new ValidationResult(true, "Question text is valid.");
        }

        public static ValidationResult ValidateOption(string option)
        {
            if (string.IsNullOrWhiteSpace(option))
                return new ValidationResult(false, "Option cannot be empty.");
            if (option.Length < 1) // Minimum length for an option
                return new ValidationResult(false, "Option must be at least 1 character long.");
            return new ValidationResult(true, "Option is valid.");
        }

        public static ValidationResult ValidateCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return new ValidationResult(false, "Category cannot be empty.");
            if (category.Length < 3)
                return new ValidationResult(false, "Category must be at least 3 characters long.");
            return new ValidationResult(true, "Category is valid.");
        }

        public static ValidationResult ValidateExamName(string examName)
        {
            if (string.IsNullOrWhiteSpace(examName))
                return new ValidationResult(false, "Exam name cannot be empty.");
            if (examName.Length < 3)
                return new ValidationResult(false, "Exam name must be at least 3 characters long.");
            return new ValidationResult(true, "Exam name is valid.");
        }

        public static ValidationResult ValidateQuestionCountForExam(int questionCount, int availableQuestions)
        {
            if (questionCount <= 0)
                return new ValidationResult(false, "Number of questions must be greater than 0.");

            if (questionCount > availableQuestions)
                return new ValidationResult(false, $"Cannot create exam with {questionCount} questions. Only {availableQuestions} questions available.");

            if (questionCount > 100) // Arbitrary reasonable limit for an exam
                return new ValidationResult(false, "Cannot create exam with more than 100 questions.");

            return new ValidationResult(true, "Question count is valid.");
        }
        public static ValidationResult ValidateTextField(string input, string fieldName, int minLength, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ValidationResult(false, $"{fieldName} cannot be empty.");
            }

            if (input.Length < minLength)
            {
                return new ValidationResult(false, $"{fieldName} must be at least {minLength} characters long.");
            }

            if (input.Length > maxLength)
            {
                return new ValidationResult(false, $"{fieldName} cannot be longer than {maxLength} characters.");
            }

            return new ValidationResult(true, "");
        }

        public static ValidationResult ValidateTimeLimit(int timeLimit)
        {
            if (timeLimit <= 0)
                return new ValidationResult(false, "Time limit must be greater than 0 minutes.");

            if (timeLimit > 300) // 5 hours max
                return new ValidationResult(false, "Time limit cannot exceed 300 minutes (5 hours).");

            return new ValidationResult(true, "Time limit is valid.");
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }
    }
}