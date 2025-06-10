using ExamPracticeSystemFull.Models; // For ExamResult, Question, Exam
using ExamPracticeSystemFull.Services; // For ExamService, QuestionService

namespace ExamPracticeSystemFull.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    

    public partial class StudentDataAnalysisForm : Form
    {
        private readonly ExamService _examService;
        private readonly QuestionService _questionService;
        private string _studentUsername;
        private List<ExamResult> _studentResults;

        /// <summary>
        /// Constructor for the StudentDataAnalysisForm.
        /// Initializes services and loads data for the specified student.
        /// </summary>
        /// <param name="studentUsername">The username of the student to analyze.</param>
        public StudentDataAnalysisForm(string studentUsername)
        {
            InitializeComponent();
            _examService = new ExamService();
            _questionService = new QuestionService();
            _studentUsername = studentUsername;

            SetupForm();
            LoadStudentResults();
            DisplayAnalytics();
        }

        /// <summary>
        /// Sets up the form's title and initial display.
        /// </summary>
        private void SetupForm()
        {
            this.Text = $"Student Data Analysis: {_studentUsername}";
            lblTitle.Text = $"Analysis for Student: {_studentUsername}";
            lblErrorMessage.Text = "";
            lblErrorMessage.Visible = false;
        }

        /// <summary>
        /// Loads all exam results for the specified student.
        /// </summary>
        private void LoadStudentResults()
        {
            try
            {
                _studentResults = _examService.GetExamResultsForStudent(_studentUsername);
                dgvStudentResults.DataSource = _studentResults;

                // Configure DataGridView columns
                dgvStudentResults.AutoGenerateColumns = true;
                dgvStudentResults.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

                if (dgvStudentResults.Columns.Contains("ResultID")) dgvStudentResults.Columns["ResultID"].Width = 60;
                if (dgvStudentResults.Columns.Contains("StudentUsername")) dgvStudentResults.Columns["StudentUsername"].Visible = false;
                if (dgvStudentResults.Columns.Contains("ExamID")) dgvStudentResults.Columns["ExamID"].Width = 60;
                if (dgvStudentResults.Columns.Contains("Score")) dgvStudentResults.Columns["Score"].Width = 80;
                if (dgvStudentResults.Columns.Contains("DateTaken")) dgvStudentResults.Columns["DateTaken"].Width = 120;
                if (dgvStudentResults.Columns.Contains("TimeTaken")) dgvStudentResults.Columns["TimeTaken"].Width = 90;
                if (dgvStudentResults.Columns.Contains("TotalQuestions")) dgvStudentResults.Columns["TotalQuestions"].Width = 100;
                if (dgvStudentResults.Columns.Contains("CorrectAnswers")) dgvStudentResults.Columns["CorrectAnswers"].Width = 100;
                if (dgvStudentResults.Columns.Contains("StudentAnswers")) dgvStudentResults.Columns["StudentAnswers"].Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading student results: {ex.Message}");
                Console.WriteLine($"StudentDataAnalysisForm Load Results Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates and displays overall analytics for the student's performance.
        /// </summary>
        private void DisplayAnalytics()
        {
            try
            {
                lblExamsTakenCount.Text = $"Exams Taken: {_studentResults.Count}";

                if (_studentResults.Any())
                {
                    double averageScore = _studentResults.Average(r => r.Score);
                    lblAverageScore.Text = $"Average Score: {averageScore:F1}%";

                    // You can add more complex analytics here, e.g.:
                    // - Best/Worst score
                    // - Most difficult categories
                    // - Time spent per exam
                    // - Progress over time (if dates are considered)
                }
                else
                {
                    lblAverageScore.Text = "No exam results available.";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error calculating analytics: {ex.Message}");
                Console.WriteLine($"StudentDataAnalysisForm Analytics Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for the "View Details" button click.
        /// Displays comprehensive details of the selected exam result, similar to student's own view.
        /// </summary>
        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            if (dgvStudentResults.SelectedRows.Count > 0)
            {
                var selectedResult = dgvStudentResults.SelectedRows[0].DataBoundItem as ExamResult;
                if (selectedResult != null)
                {
                    Exam exam = _examService.GetExamById(selectedResult.ExamID);
                    if (exam == null)
                    {
                        MessageBox.Show("Original exam for this result not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string detailedReport = $"Exam Name: {exam.Name}\n\n";
                    detailedReport += $"Score: {selectedResult.Score:F1}% ({selectedResult.CorrectAnswers}/{selectedResult.TotalQuestions} Correct)\n";
                    detailedReport += $"Date Taken: {selectedResult.DateTaken:yyyy-MM-dd HH:mm}\n";
                    detailedReport += $"Time Taken: {TimeSpan.FromSeconds(selectedResult.TimeTaken):mm\\:ss}\n\n";

                    detailedReport += "--- Question Review ---\n";
                    int questionNum = 1;
                    foreach (var questionId in exam.QuestionIDs)
                    {
                        Question q = _questionService.GetQuestionById(questionId);
                        if (q != null)
                        {
                            selectedResult.StudentAnswers.TryGetValue(q.ID, out string studentAnswer);
                            studentAnswer = studentAnswer ?? "[No Answer]";

                            detailedReport += $"Q{questionNum}. {q.Text}\n";
                            if (q.Type == QuestionType.MultipleChoice)
                            {
                                detailedReport += $"  A: {q.OptionA}\n";
                                detailedReport += $"  B: {q.OptionB}\n";
                                detailedReport += $"  C: {q.OptionC}\n";
                                detailedReport += $"  D: {q.OptionD}\n";
                                detailedReport += $"  Student Answer: {studentAnswer}\n";
                                detailedReport += $"  Correct Answer: {q.CorrectAnswer}\n\n";
                            }
                            else if (q.Type == QuestionType.TrueFalse)
                            {
                                detailedReport += $"  Student Answer: {studentAnswer}\n";
                                detailedReport += $"  Correct Answer: {q.CorrectAnswer}\n\n";
                            }
                            else if (q.Type == QuestionType.FillInTheBlank)
                            {
                                detailedReport += $"  Student Answer: {studentAnswer}\n";
                                detailedReport += $"  Correct Answer: {q.CorrectAnswer}\n\n";
                            }
                        }
                        questionNum++;
                    }
                    MessageBox.Show(detailedReport, $"Exam Review: {exam.Name}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an exam result from the list to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.ForeColor = Color.Red;
            lblErrorMessage.Visible = true;
        }

        // --- Designer-generated code (typically in StudentDataAnalysisForm.Designer.cs) ---
        // You will need to ensure these controls are added to your form in Visual Studio Designer.

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblExamsTakenCount;
        private System.Windows.Forms.Label lblAverageScore;
        private System.Windows.Forms.DataGridView dgvStudentResults;
        private System.Windows.Forms.Button btnViewDetails;
        private System.Windows.Forms.Label lblErrorMessage;

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblExamsTakenCount = new System.Windows.Forms.Label();
            this.lblAverageScore = new System.Windows.Forms.Label();
            this.dgvStudentResults = new System.Windows.Forms.DataGridView();
            this.btnViewDetails = new System.Windows.Forms.Button();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudentResults)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(262, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Analysis for Student: [N/A]";
            // 
            // lblExamsTakenCount
            // 
            this.lblExamsTakenCount.AutoSize = true;
            this.lblExamsTakenCount.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblExamsTakenCount.Location = new System.Drawing.Point(20, 70);
            this.lblExamsTakenCount.Name = "lblExamsTakenCount";
            this.lblExamsTakenCount.Size = new System.Drawing.Size(100, 19);
            this.lblExamsTakenCount.TabIndex = 1;
            this.lblExamsTakenCount.Text = "Exams Taken: 0";
            // 
            // lblAverageScore
            // 
            this.lblAverageScore.AutoSize = true;
            this.lblAverageScore.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblAverageScore.Location = new System.Drawing.Point(20, 95);
            this.lblAverageScore.Name = "lblAverageScore";
            this.lblAverageScore.Size = new System.Drawing.Size(101, 19);
            this.lblAverageScore.TabIndex = 2;
            this.lblAverageScore.Text = "Average Score: ";
            // 
            // dgvStudentResults
            // 
            this.dgvStudentResults.AllowUserToAddRows = false;
            this.dgvStudentResults.AllowUserToDeleteRows = false;
            this.dgvStudentResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvStudentResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStudentResults.Location = new System.Drawing.Point(20, 130);
            this.dgvStudentResults.MultiSelect = false;
            this.dgvStudentResults.Name = "dgvStudentResults";
            this.dgvStudentResults.ReadOnly = true;
            this.dgvStudentResults.RowTemplate.Height = 25;
            this.dgvStudentResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStudentResults.Size = new System.Drawing.Size(760, 300);
            this.dgvStudentResults.TabIndex = 3;
            // 
            // btnViewDetails
            // 
            this.btnViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnViewDetails.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnViewDetails.Location = new System.Drawing.Point(20, 445);
            this.btnViewDetails.Name = "btnViewDetails";
            this.btnViewDetails.Size = new System.Drawing.Size(120, 35);
            this.btnViewDetails.TabIndex = 4;
            this.btnViewDetails.Text = "View Details";
            this.btnViewDetails.UseVisualStyleBackColor = true;
            this.btnViewDetails.Click += new System.EventHandler(this.btnViewDetails_Click);
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblErrorMessage.AutoSize = true;
            this.lblErrorMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblErrorMessage.ForeColor = System.Drawing.Color.Red;
            this.lblErrorMessage.Location = new System.Drawing.Point(150, 455);
            this.lblErrorMessage.MaximumSize = new System.Drawing.Size(630, 0);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(76, 15);
            this.lblErrorMessage.TabIndex = 5;
            this.lblErrorMessage.Text = "Error Message";
            this.lblErrorMessage.Visible = false;
            // 
            // StudentDataAnalysisForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.lblErrorMessage);
            this.Controls.Add(this.btnViewDetails);
            this.Controls.Add(this.dgvStudentResults);
            this.Controls.Add(this.lblAverageScore);
            this.Controls.Add(this.lblExamsTakenCount);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new System.Drawing.Size(816, 539);
            this.Name = "StudentDataAnalysisForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Student Data Analysis";
            this.Load += new System.EventHandler(this.StudentDataAnalysisForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStudentResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void StudentDataAnalysisForm_Load(object sender, EventArgs e)
        {
            // Any specific logic on form load can go here.
        }
    }
}
