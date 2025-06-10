namespace ExamPracticeSystemFull.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExamPracticeSystemFull.Models; // Ensure this namespace is correct based on your project structure

    public class UserService
    {
        private readonly CsvDataService _csvService;
        private const string UsersFileName = "users.csv";

        public UserService()
        {
            _csvService = new CsvDataService();
            InitializeUsersFile();
        }

        private void InitializeUsersFile()
        {
            if (!_csvService.FileExists(UsersFileName))
            {
                // Create default admin user
                var adminUser = new User("admin1", "Admin123!", UserRole.Lecturer, "admin@exam.com", "12345");
                var studentUser = new User("student1", "Student1!", UserRole.Student, "student@exam.com", "54321");
                
                var lines = new List<string>
                {
                    "Username,Password,Role,Email,IDNumber,CreatedDate",
                    adminUser.ToCsvString(),
                    studentUser.ToCsvString()
                };
                
                _csvService.WriteAllLines(UsersFileName, lines);
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            try
            {
                var users = GetAllUsers();
                return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error authenticating user: {ex.Message}");
            }
        }

        public bool RegisterUser(User user)
        {
            try
            {
                var users = GetAllUsers();
                if (users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    return false; // Username already exists
                }
                _csvService.AppendLine(UsersFileName, user.ToCsvString());
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error registering user: {ex.Message}");
            }
        }

        public List<User> GetAllUsers()
        {
            try
            {
                var lines = _csvService.ReadAllLines(UsersFileName);
                // Skip header line
                return lines.Skip(1).Select(User.FromCsvString).Where(u => u != null).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading all users: {ex.Message}");
            }
        }

        public User GetUserByUsername(string username)
        {
            try
            {
                return GetAllUsers().FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user by username: {ex.Message}");
            }
        }

        public List<User> GetUsersByRole(UserRole role)
        {
            try
            {
                return GetAllUsers().Where(u => u.Role == role).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting users by role: {ex.Message}");
            }
        }

        public bool UpdateUser(User updatedUser)
        {
            try
            {
                var users = GetAllUsers();
                var existingUser = users.FirstOrDefault(u => u.Username.Equals(updatedUser.Username, StringComparison.OrdinalIgnoreCase));
                if (existingUser != null)
                {
                    // Find the index of the existing user
                    int index = users.IndexOf(existingUser);
                    // Replace the existing user with the updated one
                    users[index] = updatedUser;
                    SaveAllUsers(users);
                    return true;
                }
                return false; // User not found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user: {ex.Message}");
            }
        }

        public bool DeleteUser(string username)
        {
            try
            {
                var users = GetAllUsers();
                int initialCount = users.Count;
                users.RemoveAll(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (users.Count < initialCount)
                {
                    SaveAllUsers(users);
                    return true;
                }
                return false; // User not found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user: {ex.Message}");
            }
        }

        public bool ChangePassword(string username, string newPassword)
        {
            try
            {
                var user = GetUserByUsername(username);
                if (user == null) return false;

                user.Password = newPassword;
                return UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error changing password: {ex.Message}");
            }
        }

        private void SaveAllUsers(List<User> users)
        {
            var lines = new List<string> { "Username,Password,Role,Email,IDNumber,CreatedDate" };
            lines.AddRange(users.Select(u => u.ToCsvString()));
            _csvService.WriteAllLines(UsersFileName, lines);
        }

        public int GetTotalUsersCount()
        {
            try
            {
                return GetAllUsers().Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user count: {ex.Message}");
            }
        }

        public int GetStudentsCount()
        {
            try
            {
                return GetUsersByRole(UserRole.Student).Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting student count: {ex.Message}");
            }
        }

        public int GetLecturersCount()
        {
            try
            {
                return GetUsersByRole(UserRole.Lecturer).Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting lecturer count: {ex.Message}");
            }
        }
    }
}
