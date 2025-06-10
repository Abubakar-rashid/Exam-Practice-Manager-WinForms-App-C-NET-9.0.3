namespace ExamPracticeSystemFull.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExamPracticeSystemFull.Models; // Ensure this namespace is correct

    // /// <summary>
    // /// Manages CRUD operations for Question objects, including persistence to CSV.
    // /// Provides methods for adding, updating, deleting, retrieving questions,
    // /// and fetching statistics.
    // /// </summary>
    public class QuestionService
    {
        private readonly CsvDataService _csvService;
        private const string QuestionsFileName = "questions.csv";

        /// <summary>
        /// Initializes a new instance of the QuestionService class.
        /// </summary>
        public QuestionService()
        {
            _csvService = new CsvDataService();
            InitializeQuestionsFile();
        }

        /// <summary>
        /// Ensures the questions.csv file exists and has the correct header.
        /// </summary>
        private void InitializeQuestionsFile()
        {
            if (!_csvService.FileExists(QuestionsFileName))
            {
                var lines = new List<string>
                {
                    "ID,Text,OptionA,OptionB,OptionC,OptionD,CorrectAnswer,Category,Difficulty,Type,CreatedBy,CreatedDate"
                };
                _csvService.WriteAllLines(QuestionsFileName, lines);
            }
        }

        /// <summary>
        /// Adds a new question to the question bank.
        /// Assigns a new unique ID to the question before saving.
        /// </summary>
        /// <param name="question">The Question object to add.</param>
        /// <returns>True if the question was added successfully, false otherwise.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during question addition.</exception>
        public bool AddQuestion(Question question)
        {
            try
            {
                question.ID = GetNextQuestionId();
                _csvService.AppendLine(QuestionsFileName, question.ToCsvString());
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding question: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all questions from the question bank.
        /// </summary>
        /// <returns>A list of all Question objects.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public List<Question> GetAllQuestions()
        {
            try
            {
                var lines = _csvService.ReadAllLines(QuestionsFileName);
                // Skip header line and parse each line into a Question object
                return lines.Skip(1).Select(Question.FromCsvString).Where(q => q != null).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading all questions: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a question by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the question to retrieve.</param>
        /// <returns>The Question object if found, otherwise null.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public Question GetQuestionById(int id)
        {
            try
            {
                return GetAllQuestions().FirstOrDefault(q => q.ID == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting question by ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing question in the question bank.
        /// </summary>
        /// <param name="updatedQuestion">The Question object with updated information.</param>
        /// <returns>True if the question was updated successfully, false otherwise.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during update.</exception>
        public bool UpdateQuestion(Question updatedQuestion)
        {
            try
            {
                var questions = GetAllQuestions();
                var existingQuestion = questions.FirstOrDefault(q => q.ID == updatedQuestion.ID);
                if (existingQuestion != null)
                {
                    // Find the index of the existing question
                    int index = questions.IndexOf(existingQuestion);
                    // Replace the existing question with the updated one
                    questions[index] = updatedQuestion;
                    SaveAllQuestions(questions);
                    return true;
                }
                return false; // Question not found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating question: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a question from the question bank by its ID.
        /// </summary>
        /// <param name="id">The ID of the question to delete.</param>
        /// <returns>True if the question was deleted successfully, false otherwise.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during deletion.</exception>
        public bool DeleteQuestion(int id)
        {
            try
            {
                var questions = GetAllQuestions();
                int initialCount = questions.Count;
                questions.RemoveAll(q => q.ID == id);
                if (questions.Count < initialCount)
                {
                    SaveAllQuestions(questions);
                    return true;
                }
                return false; // Question not found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting question: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the next available unique ID for a new question.
        /// </summary>
        /// <returns>The next available question ID.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during ID generation.</exception>
        private int GetNextQuestionId()
        {
            try
            {
                var questions = GetAllQuestions();
                // If there are no questions, start from 1, otherwise, max ID + 1
                return questions.Count > 0 ? questions.Max(q => q.ID) + 1 : 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting next question ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the entire list of questions back to the CSV file.
        /// </summary>
        /// <param name="questions">The list of questions to save.</param>
        private void SaveAllQuestions(List<Question> questions)
        {
            var lines = new List<string> { "ID,Text,OptionA,OptionB,OptionC,OptionD,CorrectAnswer,Category,Difficulty,Type,CreatedBy,CreatedDate" };
            lines.AddRange(questions.Select(q => q.ToCsvString()));
            _csvService.WriteAllLines(QuestionsFileName, lines);
        }

        /// <summary>
        /// Retrieves questions filtered by category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>A list of questions belonging to the specified category.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public List<Question> GetQuestionsByCategory(string category)
        {
            try
            {
                return GetAllQuestions().Where(q => q.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting questions by category: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves questions filtered by difficulty level.
        /// </summary>
        /// <param name="difficulty">The difficulty level to filter by.</param>
        /// <returns>A list of questions with the specified difficulty.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public List<Question> GetQuestionsByDifficulty(DifficultyLevel difficulty)
        {
            try
            {
                return GetAllQuestions().Where(q => q.Difficulty == difficulty).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting questions by difficulty: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the total count of all questions.
        /// </summary>
        /// <returns>The total number of questions.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during count retrieval.</exception>
        public int GetQuestionsCount()
        {
            try
            {
                return GetAllQuestions().Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total questions count: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the count of questions for a specific category.
        /// </summary>
        /// <param name="category">The category to count questions for.</param>
        /// <returns>The number of questions in the specified category.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during count retrieval.</exception>
        public int GetQuestionsCountByCategory(string category)
        {
            try
            {
                return GetQuestionsByCategory(category).Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting questions count by category: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the count of questions for a specific difficulty level.
        /// </summary>
        /// <param name="difficulty">The difficulty level to count questions for.</param>
        /// <returns>The number of questions at the specified difficulty level.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during count retrieval.</exception>
        public int GetQuestionsCountByDifficulty(DifficultyLevel difficulty)
        {
            try
            {
                return GetQuestionsByDifficulty(difficulty).Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting questions count by difficulty: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates statistics on question categories (category name to count).
        /// </summary>
        /// <returns>A dictionary mapping category names to the count of questions in that category.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during statistics generation.</exception>
        public Dictionary<string, int> GetCategoryStatistics()
        {
            try
            {
                var questions = GetAllQuestions();
                return questions.GroupBy(q => q.Category)
                              .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting category statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates statistics on question difficulty levels (difficulty level to count).
        /// </summary>
        /// <returns>A dictionary mapping difficulty levels to the count of questions at that level.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during statistics generation.</exception>
        public Dictionary<DifficultyLevel, int> GetDifficultyStatistics()
        {
            try
            {
                var questions = GetAllQuestions();
                return questions.GroupBy(q => q.Difficulty)
                              .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting difficulty statistics: {ex.Message}");
            }
        }
    }
}
