namespace ExamPracticeSystemFull.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExamPracticeSystemFull.Models; // Ensure this namespace is correct

    /// <summary>
    /// Manages exam creation, retrieval, and exam result persistence.
    /// Interacts with CsvDataService and QuestionService.
    /// </summary>
    public class ExamService
    {
        private readonly CsvDataService _csvService;
        private readonly QuestionService _questionService;
        private const string ExamsFileName = "exams.csv";
        private const string ExamResultsFileName = "exam_results.csv";

        /// <summary>
        /// Initializes a new instance of the ExamService class.
        /// </summary>
        public ExamService()
        {
            _csvService = new CsvDataService();
            _questionService = new QuestionService(); // Dependency on QuestionService
            InitializeExamFiles();
        }

        /// <summary>
        /// Ensures that `exams.csv` and `exam_results.csv` files exist with their respective headers.
        /// </summary>
        private void InitializeExamFiles()
        {
            // Initialize exams.csv
            if (!_csvService.FileExists(ExamsFileName))
            {
                var examLines = new List<string>
                {
                    "ExamID,Name,QuestionIDs,CreatedBy,CreatedDate,TimeLimit,Description"
                };
                _csvService.WriteAllLines(ExamsFileName, examLines);
            }

            // Initialize exam_results.csv
            if (!_csvService.FileExists(ExamResultsFileName))
            {
                var resultLines = new List<string>
                {
                    "ResultID,StudentUsername,ExamID,Score,DateTaken,TimeTaken,TotalQuestions,CorrectAnswers,StudentAnswers"
                };
                _csvService.WriteAllLines(ExamResultsFileName, resultLines);
            }
        }

        /// <summary>
        /// Creates a new exam and saves it to the CSV file.
        /// </summary>
        /// <param name="exam">The Exam object to create.</param>
        /// <returns>True if the exam was created successfully, false otherwise.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during exam creation.</exception>
        public bool CreateExam(Exam exam)
        {
            try
            {
                exam.ExamID = GetNextExamId();
                _csvService.AppendLine(ExamsFileName, exam.ToCsvString());
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating exam: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all exams from the CSV file.
        /// </summary>
        /// <returns>A list of all Exam objects.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public List<Exam> GetAllExams()
        {
            try
            {
                var lines = _csvService.ReadAllLines(ExamsFileName);
                return lines.Skip(1).Select(Exam.FromCsvString).Where(e => e != null).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading all exams: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves an exam by its unique ID.
        /// </summary>
        /// <param name="examId">The ID of the exam to retrieve.</param>
        /// <returns>The Exam object if found, otherwise null.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public Exam GetExamById(int examId)
        {
            try
            {
                return GetAllExams().FirstOrDefault(e => e.ExamID == examId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting exam by ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing exam in the CSV file.
        /// </summary>
        /// <param name="updatedExam">The Exam object with updated information.</param>
        /// <returns>True if the exam was updated successfully, false otherwise.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during update.</exception>
        public bool UpdateExam(Exam updatedExam)
        {
            try
            {
                var exams = GetAllExams();
                var existingExam = exams.FirstOrDefault(e => e.ExamID == updatedExam.ExamID);
                if (existingExam != null)
                {
                    int index = exams.IndexOf(existingExam);
                    exams[index] = updatedExam;
                    SaveAllExams(exams);
                    return true;
                }
                return false; // Exam not found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating exam: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an exam from the CSV file by its ID.
        /// </summary>
        /// <param name="examId">The ID of the exam to delete.</param>
        /// <returns>True if the exam was deleted successfully, false otherwise.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during deletion.</exception>
        public bool DeleteExam(int examId)
        {
            try
            {
                var exams = GetAllExams();
                int initialCount = exams.Count;
                exams.RemoveAll(e => e.ExamID == examId);
                if (exams.Count < initialCount)
                {
                    SaveAllExams(exams);
                    return true;
                }
                return false; // Exam not found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting exam: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a list of questions for a new exam based on specified criteria.
        /// </summary>
        /// <param name="numberOfQuestions">The desired number of questions.</param>
        /// <param name="category">Optional: Filter by category (null or empty for no filter).</param>
        /// <param name="difficulty">Optional: Filter by difficulty (null for no filter).</param>
        /// <returns>A list of Question IDs selected for the exam.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during question selection.</exception>
        public List<int> GenerateExamQuestions(int numberOfQuestions, string category = null, DifficultyLevel? difficulty = null)
        {
            try
            {
                var availableQuestions = _questionService.GetAllQuestions();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(category))
                {
                    availableQuestions = availableQuestions.Where(q => q.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (difficulty.HasValue)
                {
                    availableQuestions = availableQuestions.Where(q => q.Difficulty == difficulty.Value).ToList();
                }

                // Shuffle and select questions
                var random = new Random();
                var selectedQuestions = availableQuestions.OrderBy(q => random.Next())
                                                         .Take(numberOfQuestions)
                                                         .Select(q => q.ID)
                                                         .ToList();

                if (selectedQuestions.Count < numberOfQuestions)
                {
                    throw new Exception($"Could not find enough questions matching criteria. Found {selectedQuestions.Count} out of {numberOfQuestions} requested.");
                }

                return selectedQuestions;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating exam questions: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves an exam result to the CSV file.
        /// Assigns a new unique ResultID before saving.
        /// </summary>
        /// <param name="result">The ExamResult object to save.</param>
        /// <returns>True if the result was saved successfully, false otherwise.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during result saving.</exception>
        public bool SaveExamResult(ExamResult result)
        {
            try
            {
                result.ResultID = GetNextResultId();
                _csvService.AppendLine(ExamResultsFileName, result.ToCsvString());
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving exam result: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all exam results.
        /// </summary>
        /// <returns>A list of all ExamResult objects.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public List<ExamResult> GetAllExamResults()
        {
            try
            {
                var lines = _csvService.ReadAllLines(ExamResultsFileName);
                return lines.Skip(1).Select(ExamResult.FromCsvString).Where(r => r != null).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading all exam results: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves exam results for a specific student.
        /// </summary>
        /// <param name="studentUsername">The username of the student.</param>
        /// <returns>A list of ExamResult objects for the specified student.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public List<ExamResult> GetExamResultsForStudent(string studentUsername)
        {
            try
            {
                return GetAllExamResults()
                       .Where(r => r.StudentUsername.Equals(studentUsername, StringComparison.OrdinalIgnoreCase))
                       .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting exam results for student {studentUsername}: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all exam results for a specific exam.
        /// </summary>
        /// <param name="examId">The ID of the exam.</param>
        /// <returns>A list of ExamResult objects for the specified exam.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during retrieval.</exception>
        public List<ExamResult> GetExamResultsForExam(int examId)
        {
            try
            {
                return GetAllExamResults()
                       .Where(r => r.ExamID == examId)
                       .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting exam results for exam ID {examId}: {ex.Message}");
            }
        }


        /// <summary>
        /// Gets the next available unique ID for a new exam.
        /// </summary>
        /// <returns>The next available exam ID.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during ID generation.</exception>
        private int GetNextExamId()
        {
            try
            {
                var exams = GetAllExams();
                return exams.Count > 0 ? exams.Max(e => e.ExamID) + 1 : 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting next exam ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the next available unique ID for a new exam result.
        /// </summary>
        /// <returns>The next available result ID.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during ID generation.</exception>
        private int GetNextResultId()
        {
            try
            {
                var results = GetAllExamResults();
                return results.Count > 0 ? results.Max(r => r.ResultID) + 1 : 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting next result ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the entire list of exams back to the CSV file.
        /// </summary>
        /// <param name="exams">The list of exams to save.</param>
        private void SaveAllExams(List<Exam> exams)
        {
            var lines = new List<string> { "ExamID,Name,QuestionIDs,CreatedBy,CreatedDate,TimeLimit,Description" };
            lines.AddRange(exams.Select(e => e.ToCsvString()));
            _csvService.WriteAllLines(ExamsFileName, lines);
        }

        /// <summary>
        /// Gets the total count of exams.
        /// </summary>
        /// <returns>The total number of exams.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during count retrieval.</exception>
        public int GetExamsCount()
        {
            try
            {
                return GetAllExams().Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting exams count: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the total count of exam results.
        /// </summary>
        /// <returns>The total number of exam results.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during count retrieval.</exception>
        public int GetResultsCount()
        {
            try
            {
                return GetAllExamResults().Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting results count: {ex.Message}");
            }
        }
    }
}
