namespace ExamPracticeSystemFull.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using ExamPracticeSystemFull.Models; // For User, UserRole, UserSession
    using ExamPracticeSystemFull.Services; // For all services

    public partial class MainForm : Form
    {
        private readonly UserSession _userSession;
        private readonly UserService _userService;
        private readonly QuestionService _questionService;
        private readonly ExamService _examService;

        /// <summary>
        /// Constructor for the MainForm.
        /// Initializes services and sets up the form based on the current user's role.
        /// </summary>
        public MainForm()
        {
            InitializeComponent(); // Designer-generated method
            _userSession = UserSession.Instance; // Get the singleton user session
            _userService = new UserService();
            _questionService = new QuestionService();
            _examService = new ExamService();
            SetupMainForm(); // Custom setup
        }

        /// <summary>
        /// Sets up the main form's title, user display, and role-based UI.
        /// </summary>
        private void SetupMainForm()
        {
            this.Text = "Exam Management System - Main Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen; // Center the form
            this.WindowState = FormWindowState.Maximized; // Start maximized for better view

            // Display logged-in user information
            lblLoggedInUser.Text = $"Logged in as: {_userSession.GetCurrentUsername()} ({_userSession.CurrentUser.Role})";
            lblLoggedInUser.ForeColor = Color.Blue;

            // Remove all tabs initially so we can add them based on role explicitly
            tabMain.TabPages.Clear();

            ApplyRoleBasedUI(); // Adjust UI based on user role
            PopulateDashboardData(); // Populate initial data for dashboard/statistics
        }

        /// <summary>
        /// Applies UI adjustments based on the logged-in user's role (Lecturer or Student).
        /// </summary>
        private void ApplyRoleBasedUI()
        {
            // Always add the Dashboard tab
            tabMain.TabPages.Add(tabDashboard);

            if (_userSession.IsLecturer())
            {
                // Add Lecturer-specific tabs
                tabMain.TabPages.Add(tabQuestions);
                tabMain.TabPages.Add(tabExams);
                tabMain.TabPages.Add(tabAnalytics);

                // Populate lecturer-specific data
                RefreshLecturerData();
            }
            else if (_userSession.IsStudent())
            {
                // Add Student-specific tabs
                tabMain.TabPages.Add(tabTakeExam);
                tabMain.TabPages.Add(tabMyScores);

                // Populate student-specific data
                RefreshStudentData();
            }
            else
            {
                MessageBox.Show("Invalid user role. Please log in again.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Populates initial dashboard statistics (for lecturers primarily).
        /// </summary>
        private void PopulateDashboardData()
        {
            try
            {
                lblTotalUsers.Text = $"Total Users: {_userService.GetTotalUsersCount()}";
                lblTotalLecturers.Text = $"Lecturers: {_userService.GetLecturersCount()}";
                lblTotalStudents.Text = $"Students: {_userService.GetStudentsCount()}";
                lblTotalQuestions.Text = $"Total Questions: {_questionService.GetQuestionsCount()}";
                lblTotalExams.Text = $"Total Exams: {_examService.GetExamsCount()}";
                lblTotalResults.Text = $"Total Results: {_examService.GetResultsCount()}";

                var categoryStats = _questionService.GetCategoryStatistics();
                lstCategoryStats.Items.Clear();
                foreach (var entry in categoryStats)
                {
                    lstCategoryStats.Items.Add($"{entry.Key}: {entry.Value} questions");
                }

                var difficultyStats = _questionService.GetDifficultyStatistics();
                lstDifficultyStats.Items.Clear();
                foreach (var entry in difficultyStats)
                {
                    lstDifficultyStats.Items.Add($"{entry.Key}: {entry.Value} questions");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Lecturer UI and Logic

        /// <summary>
        /// Refreshes all data relevant to the Lecturer view.
        /// </summary>
        private void RefreshLecturerData()
        {
            PopulateDashboardData(); // Refresh general stats
            LoadQuestionsIntoGrid();
            LoadExamsIntoGrid();
            LoadStudentsIntoGrid(); // For student data analysis
            LoadExamResultsIntoGrid(); // For all exam results
        }

        /// <summary>
        /// Loads all questions into the DataGridView on the Questions tab.
        /// </summary>
        private void LoadQuestionsIntoGrid()
        {
            try
            {
                var questions = _questionService.GetAllQuestions();
                dgvQuestions.DataSource = questions; // Bind the list to the DataGridView
                // Auto-size columns for better readability
                dgvQuestions.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                if (dgvQuestions.Columns.Contains("ID")) dgvQuestions.Columns["ID"].Width = 50; // Smaller ID column
                if (dgvQuestions.Columns.Contains("Text")) dgvQuestions.Columns["Text"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Fill remaining space
                if (dgvQuestions.Columns.Contains("OptionA")) dgvQuestions.Columns["OptionA"].Visible = false; // Hide options from main view if too long
                if (dgvQuestions.Columns.Contains("OptionB")) dgvQuestions.Columns["OptionB"].Visible = false;
                if (dgvQuestions.Columns.Contains("OptionC")) dgvQuestions.Columns["OptionC"].Visible = false;
                if (dgvQuestions.Columns.Contains("OptionD")) dgvQuestions.Columns["OptionD"].Visible = false;
                if (dgvQuestions.Columns.Contains("CorrectAnswer")) dgvQuestions.Columns["CorrectAnswer"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading questions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for the "Add New Question" button.
        /// Opens the QuestionCreationForm in add mode.
        /// </summary>
        private void btnAddQuestion_Click(object sender, EventArgs e)
        {
            QuestionCreationForm form = new QuestionCreationForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadQuestionsIntoGrid(); // Refresh grid after adding/editing
            }
        }

        /// <summary>
        /// Event handler for the "Edit Question" button.
        /// Opens the QuestionCreationForm in edit mode with the selected question.
        /// </summary>
        private void btnEditQuestion_Click(object sender, EventArgs e)
        {
            if (dgvQuestions.SelectedRows.Count > 0)
            {
                var selectedQuestion = dgvQuestions.SelectedRows[0].DataBoundItem as Question;
                if (selectedQuestion != null)
                {
                    QuestionCreationForm form = new QuestionCreationForm(selectedQuestion.ID); // Pass ID for editing
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadQuestionsIntoGrid(); // Refresh grid after adding/editing
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a question to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Event handler for the "Delete Question" button.
        /// Deletes the selected question from the question bank.
        /// </summary>
        private void btnDeleteQuestion_Click(object sender, EventArgs e)
        {
            if (dgvQuestions.SelectedRows.Count > 0)
            {
                var selectedQuestion = dgvQuestions.SelectedRows[0].DataBoundItem as Question;
                if (selectedQuestion != null)
                {
                    DialogResult confirm = MessageBox.Show($"Are you sure you want to delete question ID: {selectedQuestion.ID}?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes)
                    {
                        try
                        {
                            if (_questionService.DeleteQuestion(selectedQuestion.ID))
                            {
                                MessageBox.Show("Question deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadQuestionsIntoGrid(); // Refresh grid
                                PopulateDashboardData(); // Update stats
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete question.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred during deletion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a question to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Loads all exams into the DataGridView on the Exams tab.
        /// </summary>
        private void LoadExamsIntoGrid()
        {
            try
            {
                var exams = _examService.GetAllExams();
                dgvExams.DataSource = exams; // Bind the list to the DataGridView
                dgvExams.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                if (dgvExams.Columns.Contains("ExamID")) dgvExams.Columns["ExamID"].Width = 60;
                if (dgvExams.Columns.Contains("Name")) dgvExams.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (dgvExams.Columns.Contains("QuestionIDs")) dgvExams.Columns["QuestionIDs"].Visible = false; // Raw IDs might not be useful directly
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading exams: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for the "Create New Exam" button.
        /// Opens the ExamCreationForm.
        /// </summary>
        private void btnCreateExam_Click(object sender, EventArgs e)
        {
            ExamCreationForm form = new ExamCreationForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadExamsIntoGrid(); // Refresh grid after creating exam
                PopulateDashboardData(); // Update stats
            }
        }

        /// <summary>
        /// Event handler for the "View Exam Details" button (for lecturers).
        /// Could open a form to show questions in an exam.
        /// </summary>
        private void btnViewExamDetails_Click(object sender, EventArgs e)
        {
            if (dgvExams.SelectedRows.Count > 0)
            {
                var selectedExam = dgvExams.SelectedRows[0].DataBoundItem as Exam;
                if (selectedExam != null)
                {
                    // Create a simple message box or a new form to display exam details
                    string details = $"Exam ID: {selectedExam.ExamID}\n" +
                                     $"Name: {selectedExam.Name}\n" +
                                     $"Description: {selectedExam.Description}\n" +
                                     $"Time Limit: {selectedExam.TimeLimit} minutes\n" +
                                     $"Questions: {selectedExam.QuestionIDs.Count}\n" +
                                     $"Created By: {selectedExam.CreatedBy}\n" +
                                     $"Created Date: {selectedExam.CreatedDate:yyyy-MM-dd HH:mm}";
                    MessageBox.Show(details, $"Exam Details: {selectedExam.Name}", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Optionally, open a form to display the actual questions
                    // new ExamDetailsForm(selectedExam.ExamID).ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Please select an exam to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Loads all exam results into the DataGridView on the Analytics tab.
        /// </summary>
        private void LoadExamResultsIntoGrid()
        {
            try
            {
                var results = _examService.GetAllExamResults();
                dgvExamResults.DataSource = results;
                dgvExamResults.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                if (dgvExamResults.Columns.Contains("ResultID")) dgvExamResults.Columns["ResultID"].Width = 60;
                if (dgvExamResults.Columns.Contains("StudentUsername")) dgvExamResults.Columns["StudentUsername"].Width = 100;
                if (dgvExamResults.Columns.Contains("ExamID")) dgvExamResults.Columns["ExamID"].Width = 60;
                if (dgvExamResults.Columns.Contains("Score")) dgvExamResults.Columns["Score"].Width = 80;
                if (dgvExamResults.Columns.Contains("DateTaken")) dgvExamResults.Columns["DateTaken"].Width = 120;
                if (dgvExamResults.Columns.Contains("TimeTaken")) dgvExamResults.Columns["TimeTaken"].Width = 90;
                if (dgvExamResults.Columns.Contains("StudentAnswers")) dgvExamResults.Columns["StudentAnswers"].Visible = false; // Hide raw dictionary
                if (dgvExamResults.Columns.Contains("TotalQuestions")) dgvExamResults.Columns["TotalQuestions"].Width = 100;
                if (dgvExamResults.Columns.Contains("CorrectAnswers")) dgvExamResults.Columns["CorrectAnswers"].Width = 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading exam results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads all students into the DataGridView on the Analytics tab (for lecturer to select for analysis).
        /// </summary>
        private void LoadStudentsIntoGrid()
        {
            try
            {
                var students = _userService.GetUsersByRole(UserRole.Student);
                dgvStudentsForAnalysis.DataSource = students.Select(s => new { s.Username, s.Email, s.IDNumber, s.CreatedDate }).ToList();
                dgvStudentsForAnalysis.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students for analysis: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for the "Analyze Student Data" button.
        /// Opens the StudentDataAnalysisForm for the selected student.
        /// </summary>
        private void btnAnalyzeStudent_Click(object sender, EventArgs e)
        {
            if (dgvStudentsForAnalysis.SelectedRows.Count > 0)
            {
                // Retrieve the username from the selected row (assuming Username is the first column or accessible)
                var selectedStudentUsername = dgvStudentsForAnalysis.SelectedRows[0].Cells["Username"].Value?.ToString();

                if (!string.IsNullOrEmpty(selectedStudentUsername))
                {
                    StudentDataAnalysisForm form = new StudentDataAnalysisForm(selectedStudentUsername);
                    form.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Please select a student to analyze.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Student UI and Logic

        /// <summary>
        /// Refreshes all data relevant to the Student view.
        /// </summary>
        private void RefreshStudentData()
        {
            LoadAvailableExamsForStudent();
            LoadMyScoresIntoGrid();
        }

        /// <summary>
        /// Loads available exams that the student can take.
        /// </summary>
        private void LoadAvailableExamsForStudent()
        {
            try
            {
                var exams = _examService.GetAllExams();
                dgvAvailableExams.DataSource = exams;
                dgvAvailableExams.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                if (dgvAvailableExams.Columns.Contains("ExamID")) dgvAvailableExams.Columns["ExamID"].Width = 60;
                if (dgvAvailableExams.Columns.Contains("Name")) dgvAvailableExams.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (dgvAvailableExams.Columns.Contains("QuestionIDs")) dgvAvailableExams.Columns["QuestionIDs"].Visible = false;
                if (dgvAvailableExams.Columns.Contains("CreatedBy")) dgvAvailableExams.Columns["CreatedBy"].Visible = false;
                if (dgvAvailableExams.Columns.Contains("CreatedDate")) dgvAvailableExams.Columns["CreatedDate"].Visible = false;
                if (dgvAvailableExams.Columns.Contains("Description")) dgvAvailableExams.Columns["Description"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading available exams: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for the "Take Exam" button (for students).
        /// Opens the TakeExamForm with the selected exam.
        /// </summary>
        private void btnTakeExam_Click(object sender, EventArgs e)
        {
            if (dgvAvailableExams.SelectedRows.Count > 0)
            {
                var selectedExam = dgvAvailableExams.SelectedRows[0].DataBoundItem as Exam;
                if (selectedExam != null)
                {
                    TakeExamForm form = new TakeExamForm(selectedExam.ExamID);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadMyScoresIntoGrid(); // Refresh scores after completing an exam
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an exam to take.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Loads the logged-in student's past exam results.
        /// </summary>
        private void LoadMyScoresIntoGrid()
        {
            try
            {
                var myResults = _examService.GetExamResultsForStudent(_userSession.GetCurrentUsername());
                dgvMyScores.DataSource = myResults;
                dgvMyScores.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                if (dgvMyScores.Columns.Contains("ResultID")) dgvMyScores.Columns["ResultID"].Width = 60;
                if (dgvMyScores.Columns.Contains("ExamID")) dgvMyScores.Columns["ExamID"].Width = 60;
                if (dgvMyScores.Columns.Contains("Score")) dgvMyScores.Columns["Score"].Width = 80;
                if (dgvMyScores.Columns.Contains("DateTaken")) dgvMyScores.Columns["DateTaken"].Width = 120;
                if (dgvMyScores.Columns.Contains("TimeTaken")) dgvMyScores.Columns["TimeTaken"].Width = 90;
                if (dgvMyScores.Columns.Contains("StudentAnswers")) dgvMyScores.Columns["StudentAnswers"].Visible = false; // Hide raw dictionary
                if (dgvMyScores.Columns.Contains("TotalQuestions")) dgvMyScores.Columns["TotalQuestions"].Width = 100;
                if (dgvMyScores.Columns.Contains("CorrectAnswers")) dgvMyScores.Columns["CorrectAnswers"].Width = 100;
                if (dgvMyScores.Columns.Contains("StudentUsername")) dgvMyScores.Columns["StudentUsername"].Visible = false; // This is 'My Scores' view, so username is implicit
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading your scores: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for the "View My Exam Details" button (for students).
        /// Could open a form to show the questions and their answers for a past exam.
        /// </summary>
        private void btnViewMyExamDetails_Click(object sender, EventArgs e)
        {
            if (dgvMyScores.SelectedRows.Count > 0)
            {
                var selectedResult = dgvMyScores.SelectedRows[0].DataBoundItem as ExamResult;
                if (selectedResult != null)
                {
                    // You might want to open a new form to display the exam details including
                    // the questions, correct answers, and student's submitted answers.
                    // For now, a simple message box as an example.
                    string details = $"Result ID: {selectedResult.ResultID}\n" +
                                     $"Exam ID: {selectedResult.ExamID}\n" +
                                     $"Score: {selectedResult.Score:F1}%\n" +
                                     $"Correct: {selectedResult.CorrectAnswers}/{selectedResult.TotalQuestions}\n" +
                                     $"Date Taken: {selectedResult.DateTaken:yyyy-MM-dd HH:mm}\n" +
                                     $"Time Taken: {selectedResult.TimeTaken} seconds";

                    // You could also iterate through selectedResult.StudentAnswers and retrieve the actual questions
                    // from _questionService to display a comprehensive review.
                    // Example: new ExamReviewForm(selectedResult.ResultID).ShowDialog();

                    MessageBox.Show(details, "My Exam Result Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an exam result to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        /// <summary>
        /// Event handler for the Logout button.
        /// Logs out the current user and returns to the LoginForm.
        /// </summary>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            _userSession.Logout(); // Clear the session
            MessageBox.Show("You have been logged out.", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Re-open LoginForm and close current MainForm
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }

        /// <summary>
        /// Event handler for the MainForm closing.
        /// Ensures the application exits if this is the last form.
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If the application is shutting down (e.g., if this is the last form)
            if (Application.OpenForms.Count == 0)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Event handler for the tab control's SelectedIndexChanged event.
        /// Refreshes data on a tab when it becomes active.
        /// </summary>
        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_userSession.IsLecturer())
            {
                if (tabMain.SelectedTab == tabDashboard)
                {
                    PopulateDashboardData();
                }
                else if (tabMain.SelectedTab == tabQuestions)
                {
                    LoadQuestionsIntoGrid();
                }
                else if (tabMain.SelectedTab == tabExams)
                {
                    LoadExamsIntoGrid();
                }
                else if (tabMain.SelectedTab == tabAnalytics)
                {
                    LoadExamResultsIntoGrid();
                    LoadStudentsIntoGrid();
                }
            }
            else if (_userSession.IsStudent())
            {
                if (tabMain.SelectedTab == tabTakeExam)
                {
                    LoadAvailableExamsForStudent();
                }
                else if (tabMain.SelectedTab == tabMyScores)
                {
                    LoadMyScoresIntoGrid();
                }
            }
        }


        // --- Designer-generated code (typically in MainForm.Designer.cs) ---
        // You will need to ensure these controls are added to your form in Visual Studio Designer.

        private System.Windows.Forms.Label lblLoggedInUser;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.TabControl tabMain;

        // Dashboard Tab (for both roles or general info)
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.Label lblTotalUsers;
        private System.Windows.Forms.Label lblTotalLecturers;
        private System.Windows.Forms.Label lblTotalStudents;
        private System.Windows.Forms.Label lblTotalQuestions;
        private System.Windows.Forms.Label lblTotalExams;
        private System.Windows.Forms.Label lblTotalResults;
        private System.Windows.Forms.Label lblCategoryStats;
        private System.Windows.Forms.ListBox lstCategoryStats;
        private System.Windows.Forms.Label lblDifficultyStats;
        private System.Windows.Forms.ListBox lstDifficultyStats;

        // Lecturer Tabs
        private System.Windows.Forms.TabPage tabQuestions;
        private System.Windows.Forms.DataGridView dgvQuestions;
        private System.Windows.Forms.Button btnAddQuestion;
        private System.Windows.Forms.Button btnEditQuestion;
        private System.Windows.Forms.Button btnDeleteQuestion;

        private System.Windows.Forms.TabPage tabExams;
        private System.Windows.Forms.DataGridView dgvExams;
        private System.Windows.Forms.Button btnCreateExam;
        private System.Windows.Forms.Button btnViewExamDetails;

        private System.Windows.Forms.TabPage tabAnalytics;
        private System.Windows.Forms.DataGridView dgvExamResults;
        private System.Windows.Forms.DataGridView dgvStudentsForAnalysis;
        private System.Windows.Forms.Button btnAnalyzeStudent;
        private System.Windows.Forms.Label lblAllResults;
        private System.Windows.Forms.Label lblStudentsForAnalysis;


        // Student Tabs
        private System.Windows.Forms.TabPage tabTakeExam;
        private System.Windows.Forms.DataGridView dgvAvailableExams;
        private System.Windows.Forms.Button btnTakeExam;

        private System.Windows.Forms.TabPage tabMyScores;
        private System.Windows.Forms.DataGridView dgvMyScores;
        private System.Windows.Forms.Button btnViewMyExamDetails;


        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblLoggedInUser = new System.Windows.Forms.Label();
            btnLogout = new System.Windows.Forms.Button();
            tabMain = new System.Windows.Forms.TabControl();
            tabDashboard = new System.Windows.Forms.TabPage();
            lstDifficultyStats = new System.Windows.Forms.ListBox();
            lblDifficultyStats = new System.Windows.Forms.Label();
            lstCategoryStats = new System.Windows.Forms.ListBox();
            lblCategoryStats = new System.Windows.Forms.Label();
            lblTotalResults = new System.Windows.Forms.Label();
            lblTotalExams = new System.Windows.Forms.Label();
            lblTotalQuestions = new System.Windows.Forms.Label();
            lblTotalStudents = new System.Windows.Forms.Label();
            lblTotalLecturers = new System.Windows.Forms.Label();
            lblTotalUsers = new System.Windows.Forms.Label();
            tabQuestions = new System.Windows.Forms.TabPage();
            btnDeleteQuestion = new System.Windows.Forms.Button();
            btnEditQuestion = new System.Windows.Forms.Button();
            btnAddQuestion = new System.Windows.Forms.Button();
            dgvQuestions = new System.Windows.Forms.DataGridView();
            tabExams = new System.Windows.Forms.TabPage();
            btnViewExamDetails = new System.Windows.Forms.Button();
            btnCreateExam = new System.Windows.Forms.Button();
            dgvExams = new System.Windows.Forms.DataGridView();
            tabAnalytics = new System.Windows.Forms.TabPage();
            lblStudentsForAnalysis = new System.Windows.Forms.Label();
            lblAllResults = new System.Windows.Forms.Label();
            btnAnalyzeStudent = new System.Windows.Forms.Button();
            dgvStudentsForAnalysis = new System.Windows.Forms.DataGridView();
            dgvExamResults = new System.Windows.Forms.DataGridView();
            tabTakeExam = new System.Windows.Forms.TabPage();
            btnTakeExam = new System.Windows.Forms.Button();
            dgvAvailableExams = new System.Windows.Forms.DataGridView();
            tabMyScores = new System.Windows.Forms.TabPage();
            btnViewMyExamDetails = new System.Windows.Forms.Button();
            dgvMyScores = new System.Windows.Forms.DataGridView();
            tabMain.SuspendLayout();
            tabDashboard.SuspendLayout();
            tabQuestions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvQuestions).BeginInit();
            tabExams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvExams).BeginInit();
            tabAnalytics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStudentsForAnalysis).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvExamResults).BeginInit();
            tabTakeExam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAvailableExams).BeginInit();
            tabMyScores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMyScores).BeginInit();
            SuspendLayout();
            // 
            // lblLoggedInUser
            // 
            lblLoggedInUser.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            lblLoggedInUser.AutoSize = true;
            lblLoggedInUser.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            lblLoggedInUser.Location = new System.Drawing.Point(-3, 5);
            lblLoggedInUser.Name = "lblLoggedInUser";
            lblLoggedInUser.Size = new System.Drawing.Size(122, 17);
            lblLoggedInUser.TabIndex = 0;
            lblLoggedInUser.Text = "Logged in as: User";
            // 
            // btnLogout
            // 
            btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            btnLogout.Location = new System.Drawing.Point(731, 5);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new System.Drawing.Size(75, 23);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // tabMain
            // 
            tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            tabMain.Controls.Add(tabDashboard);
            tabMain.Controls.Add(tabQuestions);
            tabMain.Controls.Add(tabExams);
            tabMain.Controls.Add(tabAnalytics);
            tabMain.Controls.Add(tabTakeExam);
            tabMain.Controls.Add(tabMyScores);
            tabMain.Location = new System.Drawing.Point(12, 38);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new System.Drawing.Size(794, 550);
            tabMain.TabIndex = 2;
            tabMain.SelectedIndexChanged += tabMain_SelectedIndexChanged;
            // 
            // tabDashboard
            // 
            tabDashboard.Controls.Add(lstDifficultyStats);
            tabDashboard.Controls.Add(lblDifficultyStats);
            tabDashboard.Controls.Add(lstCategoryStats);
            tabDashboard.Controls.Add(lblCategoryStats);
            tabDashboard.Controls.Add(lblTotalResults);
            tabDashboard.Controls.Add(lblTotalExams);
            tabDashboard.Controls.Add(lblTotalQuestions);
            tabDashboard.Controls.Add(lblTotalStudents);
            tabDashboard.Controls.Add(lblTotalLecturers);
            tabDashboard.Controls.Add(lblTotalUsers);
            tabDashboard.Location = new System.Drawing.Point(4, 24);
            tabDashboard.Name = "tabDashboard";
            tabDashboard.Padding = new System.Windows.Forms.Padding(3);
            tabDashboard.Size = new System.Drawing.Size(786, 522);
            tabDashboard.TabIndex = 0;
            tabDashboard.Text = "Dashboard";
            tabDashboard.UseVisualStyleBackColor = true;
            // 
            // lstDifficultyStats
            // 
            lstDifficultyStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left));
            lstDifficultyStats.FormattingEnabled = true;
            lstDifficultyStats.Location = new System.Drawing.Point(220, 200);
            lstDifficultyStats.Name = "lstDifficultyStats";
            lstDifficultyStats.Size = new System.Drawing.Size(200, 154);
            lstDifficultyStats.TabIndex = 9;
            // 
            // lblDifficultyStats
            // 
            lblDifficultyStats.AutoSize = true;
            lblDifficultyStats.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblDifficultyStats.Location = new System.Drawing.Point(220, 178);
            lblDifficultyStats.Name = "lblDifficultyStats";
            lblDifficultyStats.Size = new System.Drawing.Size(131, 19);
            lblDifficultyStats.TabIndex = 8;
            lblDifficultyStats.Text = "Difficulty Statistics";
            // 
            // lstCategoryStats
            // 
            lstCategoryStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left));
            lstCategoryStats.FormattingEnabled = true;
            lstCategoryStats.Location = new System.Drawing.Point(20, 200);
            lstCategoryStats.Name = "lstCategoryStats";
            lstCategoryStats.Size = new System.Drawing.Size(180, 154);
            lstCategoryStats.TabIndex = 7;
            // 
            // lblCategoryStats
            // 
            lblCategoryStats.AutoSize = true;
            lblCategoryStats.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblCategoryStats.Location = new System.Drawing.Point(20, 178);
            lblCategoryStats.Name = "lblCategoryStats";
            lblCategoryStats.Size = new System.Drawing.Size(134, 19);
            lblCategoryStats.TabIndex = 6;
            lblCategoryStats.Text = "Category Statistics";
            // 
            // lblTotalResults
            // 
            lblTotalResults.AutoSize = true;
            lblTotalResults.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblTotalResults.Location = new System.Drawing.Point(20, 140);
            lblTotalResults.Name = "lblTotalResults";
            lblTotalResults.Size = new System.Drawing.Size(88, 19);
            lblTotalResults.TabIndex = 5;
            lblTotalResults.Text = "Total Results:";
            // 
            // lblTotalExams
            // 
            lblTotalExams.AutoSize = true;
            lblTotalExams.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblTotalExams.Location = new System.Drawing.Point(20, 115);
            lblTotalExams.Name = "lblTotalExams";
            lblTotalExams.Size = new System.Drawing.Size(83, 19);
            lblTotalExams.TabIndex = 4;
            lblTotalExams.Text = "Total Exams:";
            // 
            // lblTotalQuestions
            // 
            lblTotalQuestions.AutoSize = true;
            lblTotalQuestions.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblTotalQuestions.Location = new System.Drawing.Point(20, 90);
            lblTotalQuestions.Name = "lblTotalQuestions";
            lblTotalQuestions.Size = new System.Drawing.Size(107, 19);
            lblTotalQuestions.TabIndex = 3;
            lblTotalQuestions.Text = "Total Questions:";
            // 
            // lblTotalStudents
            // 
            lblTotalStudents.AutoSize = true;
            lblTotalStudents.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblTotalStudents.Location = new System.Drawing.Point(20, 65);
            lblTotalStudents.Name = "lblTotalStudents";
            lblTotalStudents.Size = new System.Drawing.Size(66, 19);
            lblTotalStudents.TabIndex = 2;
            lblTotalStudents.Text = "Students:";
            // 
            // lblTotalLecturers
            // 
            lblTotalLecturers.AutoSize = true;
            lblTotalLecturers.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblTotalLecturers.Location = new System.Drawing.Point(20, 40);
            lblTotalLecturers.Name = "lblTotalLecturers";
            lblTotalLecturers.Size = new System.Drawing.Size(68, 19);
            lblTotalLecturers.TabIndex = 1;
            lblTotalLecturers.Text = "Lecturers:";
            // 
            // lblTotalUsers
            // 
            lblTotalUsers.AutoSize = true;
            lblTotalUsers.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblTotalUsers.Location = new System.Drawing.Point(20, 15);
            lblTotalUsers.Name = "lblTotalUsers";
            lblTotalUsers.Size = new System.Drawing.Size(86, 19);
            lblTotalUsers.TabIndex = 0;
            lblTotalUsers.Text = "Total Users:";
            // 
            // tabQuestions
            // 
            tabQuestions.Controls.Add(btnDeleteQuestion);
            tabQuestions.Controls.Add(btnEditQuestion);
            tabQuestions.Controls.Add(btnAddQuestion);
            tabQuestions.Controls.Add(dgvQuestions);
            tabQuestions.Location = new System.Drawing.Point(4, 24);
            tabQuestions.Name = "tabQuestions";
            tabQuestions.Padding = new System.Windows.Forms.Padding(3);
            tabQuestions.Size = new System.Drawing.Size(786, 522);
            tabQuestions.TabIndex = 1;
            tabQuestions.Text = "Question Bank";
            tabQuestions.UseVisualStyleBackColor = true;
            // 
            // btnDeleteQuestion
            // 
            btnDeleteQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnDeleteQuestion.Location = new System.Drawing.Point(200, 470);
            btnDeleteQuestion.Name = "btnDeleteQuestion";
            btnDeleteQuestion.Size = new System.Drawing.Size(90, 30);
            btnDeleteQuestion.TabIndex = 3;
            btnDeleteQuestion.Text = "Delete Question";
            btnDeleteQuestion.UseVisualStyleBackColor = true;
            btnDeleteQuestion.Click += btnDeleteQuestion_Click;
            // 
            // btnEditQuestion
            // 
            btnEditQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnEditQuestion.Location = new System.Drawing.Point(105, 470);
            btnEditQuestion.Name = "btnEditQuestion";
            btnEditQuestion.Size = new System.Drawing.Size(90, 30);
            btnEditQuestion.TabIndex = 2;
            btnEditQuestion.Text = "Edit Question";
            btnEditQuestion.UseVisualStyleBackColor = true;
            btnEditQuestion.Click += btnEditQuestion_Click;
            // 
            // btnAddQuestion
            // 
            btnAddQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnAddQuestion.Location = new System.Drawing.Point(10, 470);
            btnAddQuestion.Name = "btnAddQuestion";
            btnAddQuestion.Size = new System.Drawing.Size(90, 30);
            btnAddQuestion.TabIndex = 1;
            btnAddQuestion.Text = "Add Question";
            btnAddQuestion.UseVisualStyleBackColor = true;
            btnAddQuestion.Click += btnAddQuestion_Click;
            // 
            // dgvQuestions
            // 
            dgvQuestions.AllowUserToAddRows = false;
            dgvQuestions.AllowUserToDeleteRows = false;
            dgvQuestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvQuestions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvQuestions.Location = new System.Drawing.Point(10, 10);
            dgvQuestions.MultiSelect = false;
            dgvQuestions.Name = "dgvQuestions";
            dgvQuestions.ReadOnly = true;
            dgvQuestions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvQuestions.Size = new System.Drawing.Size(766, 450);
            dgvQuestions.TabIndex = 0;
            // 
            // tabExams
            // 
            tabExams.Controls.Add(btnViewExamDetails);
            tabExams.Controls.Add(btnCreateExam);
            tabExams.Controls.Add(dgvExams);
            tabExams.Location = new System.Drawing.Point(4, 24);
            tabExams.Name = "tabExams";
            tabExams.Padding = new System.Windows.Forms.Padding(3);
            tabExams.Size = new System.Drawing.Size(786, 522);
            tabExams.TabIndex = 2;
            tabExams.Text = "Exam Management";
            tabExams.UseVisualStyleBackColor = true;
            // 
            // btnViewExamDetails
            // 
            btnViewExamDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnViewExamDetails.Location = new System.Drawing.Point(105, 470);
            btnViewExamDetails.Name = "btnViewExamDetails";
            btnViewExamDetails.Size = new System.Drawing.Size(120, 30);
            btnViewExamDetails.TabIndex = 2;
            btnViewExamDetails.Text = "View Details";
            btnViewExamDetails.UseVisualStyleBackColor = true;
            btnViewExamDetails.Click += btnViewExamDetails_Click;
            // 
            // btnCreateExam
            // 
            btnCreateExam.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnCreateExam.Location = new System.Drawing.Point(10, 470);
            btnCreateExam.Name = "btnCreateExam";
            btnCreateExam.Size = new System.Drawing.Size(90, 30);
            btnCreateExam.TabIndex = 1;
            btnCreateExam.Text = "Create Exam";
            btnCreateExam.UseVisualStyleBackColor = true;
            btnCreateExam.Click += btnCreateExam_Click;
            // 
            // dgvExams
            // 
            dgvExams.AllowUserToAddRows = false;
            dgvExams.AllowUserToDeleteRows = false;
            dgvExams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvExams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvExams.Location = new System.Drawing.Point(10, 10);
            dgvExams.MultiSelect = false;
            dgvExams.Name = "dgvExams";
            dgvExams.ReadOnly = true;
            dgvExams.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvExams.Size = new System.Drawing.Size(766, 450);
            dgvExams.TabIndex = 0;
            // 
            // tabAnalytics
            // 
            tabAnalytics.Controls.Add(lblStudentsForAnalysis);
            tabAnalytics.Controls.Add(lblAllResults);
            tabAnalytics.Controls.Add(btnAnalyzeStudent);
            tabAnalytics.Controls.Add(dgvStudentsForAnalysis);
            tabAnalytics.Controls.Add(dgvExamResults);
            tabAnalytics.Location = new System.Drawing.Point(4, 24);
            tabAnalytics.Name = "tabAnalytics";
            tabAnalytics.Padding = new System.Windows.Forms.Padding(3);
            tabAnalytics.Size = new System.Drawing.Size(786, 522);
            tabAnalytics.TabIndex = 3;
            tabAnalytics.Text = "Analytics";
            tabAnalytics.UseVisualStyleBackColor = true;
            // 
            // lblStudentsForAnalysis
            // 
            lblStudentsForAnalysis.AutoSize = true;
            lblStudentsForAnalysis.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblStudentsForAnalysis.Location = new System.Drawing.Point(10, 270);
            lblStudentsForAnalysis.Name = "lblStudentsForAnalysis";
            lblStudentsForAnalysis.Size = new System.Drawing.Size(152, 19);
            lblStudentsForAnalysis.TabIndex = 4;
            lblStudentsForAnalysis.Text = "Students for Analysis:";
            // 
            // lblAllResults
            // 
            lblAllResults.AutoSize = true;
            lblAllResults.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblAllResults.Location = new System.Drawing.Point(10, 10);
            lblAllResults.Name = "lblAllResults";
            lblAllResults.Size = new System.Drawing.Size(121, 19);
            lblAllResults.TabIndex = 3;
            lblAllResults.Text = "All Exam Results:";
            // 
            // btnAnalyzeStudent
            // 
            btnAnalyzeStudent.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnAnalyzeStudent.Location = new System.Drawing.Point(10, 470);
            btnAnalyzeStudent.Name = "btnAnalyzeStudent";
            btnAnalyzeStudent.Size = new System.Drawing.Size(120, 30);
            btnAnalyzeStudent.TabIndex = 2;
            btnAnalyzeStudent.Text = "Analyze Student";
            btnAnalyzeStudent.UseVisualStyleBackColor = true;
            btnAnalyzeStudent.Click += btnAnalyzeStudent_Click;
            // 
            // dgvStudentsForAnalysis
            // 
            dgvStudentsForAnalysis.AllowUserToAddRows = false;
            dgvStudentsForAnalysis.AllowUserToDeleteRows = false;
            dgvStudentsForAnalysis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvStudentsForAnalysis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStudentsForAnalysis.Location = new System.Drawing.Point(10, 292);
            dgvStudentsForAnalysis.MultiSelect = false;
            dgvStudentsForAnalysis.Name = "dgvStudentsForAnalysis";
            dgvStudentsForAnalysis.ReadOnly = true;
            dgvStudentsForAnalysis.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvStudentsForAnalysis.Size = new System.Drawing.Size(766, 170);
            dgvStudentsForAnalysis.TabIndex = 1;
            // 
            // dgvExamResults
            // 
            dgvExamResults.AllowUserToAddRows = false;
            dgvExamResults.AllowUserToDeleteRows = false;
            dgvExamResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvExamResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvExamResults.Location = new System.Drawing.Point(10, 32);
            dgvExamResults.MultiSelect = false;
            dgvExamResults.Name = "dgvExamResults";
            dgvExamResults.ReadOnly = true;
            dgvExamResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvExamResults.Size = new System.Drawing.Size(766, 230);
            dgvExamResults.TabIndex = 0;
            // 
            // tabTakeExam
            // 
            tabTakeExam.Controls.Add(btnTakeExam);
            tabTakeExam.Controls.Add(dgvAvailableExams);
            tabTakeExam.Location = new System.Drawing.Point(4, 24);
            tabTakeExam.Name = "tabTakeExam";
            tabTakeExam.Padding = new System.Windows.Forms.Padding(3);
            tabTakeExam.Size = new System.Drawing.Size(786, 522);
            tabTakeExam.TabIndex = 4;
            tabTakeExam.Text = "Take Exam";
            tabTakeExam.UseVisualStyleBackColor = true;
            // 
            // btnTakeExam
            // 
            btnTakeExam.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnTakeExam.Location = new System.Drawing.Point(10, 470);
            btnTakeExam.Name = "btnTakeExam";
            btnTakeExam.Size = new System.Drawing.Size(90, 30);
            btnTakeExam.TabIndex = 1;
            btnTakeExam.Text = "Take Exam";
            btnTakeExam.UseVisualStyleBackColor = true;
            btnTakeExam.Click += btnTakeExam_Click;
            // 
            // dgvAvailableExams
            // 
            dgvAvailableExams.AllowUserToAddRows = false;
            dgvAvailableExams.AllowUserToDeleteRows = false;
            dgvAvailableExams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvAvailableExams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAvailableExams.Location = new System.Drawing.Point(10, 10);
            dgvAvailableExams.MultiSelect = false;
            dgvAvailableExams.Name = "dgvAvailableExams";
            dgvAvailableExams.ReadOnly = true;
            dgvAvailableExams.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvAvailableExams.Size = new System.Drawing.Size(766, 450);
            dgvAvailableExams.TabIndex = 0;
            // 
            // tabMyScores
            // 
            tabMyScores.Controls.Add(btnViewMyExamDetails);
            tabMyScores.Controls.Add(dgvMyScores);
            tabMyScores.Location = new System.Drawing.Point(4, 24);
            tabMyScores.Name = "tabMyScores";
            tabMyScores.Padding = new System.Windows.Forms.Padding(3);
            tabMyScores.Size = new System.Drawing.Size(786, 522);
            tabMyScores.TabIndex = 5;
            tabMyScores.Text = "My Scores";
            tabMyScores.UseVisualStyleBackColor = true;
            // 
            // btnViewMyExamDetails
            // 
            btnViewMyExamDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left));
            btnViewMyExamDetails.Location = new System.Drawing.Point(10, 470);
            btnViewMyExamDetails.Name = "btnViewMyExamDetails";
            btnViewMyExamDetails.Size = new System.Drawing.Size(120, 30);
            btnViewMyExamDetails.TabIndex = 1;
            btnViewMyExamDetails.Text = "View Details";
            btnViewMyExamDetails.UseVisualStyleBackColor = true;
            btnViewMyExamDetails.Click += btnViewMyExamDetails_Click;
            // 
            // dgvMyScores
            // 
            dgvMyScores.AllowUserToAddRows = false;
            dgvMyScores.AllowUserToDeleteRows = false;
            dgvMyScores.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvMyScores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMyScores.Location = new System.Drawing.Point(10, 10);
            dgvMyScores.MultiSelect = false;
            dgvMyScores.Name = "dgvMyScores";
            dgvMyScores.ReadOnly = true;
            dgvMyScores.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvMyScores.Size = new System.Drawing.Size(766, 450);
            dgvMyScores.TabIndex = 0;
            // 
            // MainForm
            // 
            ClientSize = new System.Drawing.Size(818, 600);
            Controls.Add(tabMain);
            Controls.Add(btnLogout);
            Controls.Add(lblLoggedInUser);
            Text = "Main Dashboard";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            tabMain.ResumeLayout(false);
            tabDashboard.ResumeLayout(false);
            tabDashboard.PerformLayout();
            tabQuestions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvQuestions).EndInit();
            tabExams.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvExams).EndInit();
            tabAnalytics.ResumeLayout(false);
            tabAnalytics.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStudentsForAnalysis).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvExamResults).EndInit();
            tabTakeExam.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvAvailableExams).EndInit();
            tabMyScores.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMyScores).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initial load of data when the form becomes visible
            // ApplyRoleBasedUI();
        }
    }
}
