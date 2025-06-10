namespace ExamPracticeSystemFull.Models
{
    using System;

    /// <summary>
    /// Manages the current logged-in user session using a Singleton pattern.
    /// </summary>
    public class UserSession
    {
        // Singleton instance
        private static UserSession _instance;
        // Lock object for thread-safe instance creation
        private static readonly object _lock = new object();

        // Properties for the current user and login time
        public User CurrentUser { get; private set; }
        public DateTime LoginTime { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        // Private constructor to enforce Singleton pattern
        private UserSession() { }

        /// <summary>
        /// Gets the singleton instance of the UserSession.
        /// </summary>
        public static UserSession Instance
        {
            get
            {
                // Double-checked locking for thread-safe singleton initialization
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserSession();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Logs in a user, setting the CurrentUser and LoginTime.
        /// </summary>
        /// <param name="user">The User object to log in.</param>
        public void Login(User user)
        {
            CurrentUser = user;
            LoginTime = DateTime.Now;
        }

        /// <summary>
        /// Logs out the current user by setting CurrentUser to null.
        /// </summary>
        public void Logout()
        {
            CurrentUser = null;
        }

        /// <summary>
        /// Checks if the current logged-in user is a Lecturer.
        /// </summary>
        /// <returns>True if the user is logged in and is a Lecturer, otherwise false.</returns>
        public bool IsLecturer()
        {
            return IsLoggedIn && CurrentUser.Role == UserRole.Lecturer;
        }

        /// <summary>
        /// Checks if the current logged-in user is a Student.
        /// </summary>
        /// <returns>True if the user is logged in and is a Student, otherwise false.</returns>
        public bool IsStudent()
        {
            return IsLoggedIn && CurrentUser.Role == UserRole.Student;
        }

        /// <summary>
        /// Gets the username of the current logged-in user.
        /// </summary>
        /// <returns>The username if logged in, otherwise an empty string.</returns>
        public string GetCurrentUsername()
        {
            return IsLoggedIn ? CurrentUser.Username : "";
        }

        /// <summary>
        /// Calculates the duration of the current session.
        /// </summary>
        /// <returns>A TimeSpan representing the session duration, or TimeSpan.Zero if not logged in.</returns>
        public TimeSpan GetSessionDuration()
        {
            return IsLoggedIn ? DateTime.Now - LoginTime : TimeSpan.Zero;
        }
    }
}
