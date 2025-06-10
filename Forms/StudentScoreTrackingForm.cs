using ExamPracticeSystemFull.Models; // For ExamResult, UserSession
using ExamPracticeSystemFull.Services; // For ExamService


namespace ExamPracticeSystemFull.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    

    public partial class StudentScoreTrackingForm : Form
    {
        private readonly ExamService _examService;
        private readonly UserSession _userSession;
        private string _studentUsername; // The username of the student whose scores are being tracked
        private readonly QuestionService _questionService = new QuestionService();

        /// <summary>
        /// Constructor for the StudentScoreTrackingForm.
        /// Initializes services and loads the student's exam results.
        /// </summary>
        public StudentScoreTrackingForm()
        {
            InitializeComponent();
            _examService = new ExamService();
            _userSession = UserSession.Instance;
            _studentUsername = _userSession.GetCurrentUsername(); // Get the currently logged-in student

            SetupForm();
            LoadMyScoresIntoGrid();
        }

        /// <summary>
        /// Sets up the form's title and initial display.
        /// </summary>
        private void SetupForm()
        {
            this.Text = $"My Exam Scores - {_studentUsername}";
            lblTitle.Text = $"My Exam Scores for {_studentUsername}";
            lblErrorMessage.Text = "";
            lblErrorMessage.Visible = false;
        }

        /// <summary>
        /// Loads the logged-in student's past exam results into the DataGridView.
        /// </summary>
        private void LoadMyScoresIntoGrid()
        {
            try
            {
                var myResults = _examService.GetExamResultsForStudent(_studentUsername);
                dgvMyScores.DataSource = myResults;

                // Configure DataGridView columns for better display
                dgvMyScores.AutoGenerateColumns = true; // Let it auto-generate columns first
                dgvMyScores.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

                // Hide columns that are not directly relevant to a quick overview
                if (dgvMyScores.Columns.Contains("ResultID")) dgvMyScores.Columns["ResultID"].Width = 60;
                if (dgvMyScores.Columns.Contains("StudentUsername")) dgvMyScores.Columns["StudentUsername"].Visible = false; // It's implicit here
                if (dgvMyScores.Columns.Contains("ExamID")) dgvMyScores.Columns["ExamID"].Width = 60;
                if (dgvMyScores.Columns.Contains("Score")) dgvMyScores.Columns["Score"].Width = 80;
                if (dgvMyScores.Columns.Contains("DateTaken")) dgvMyScores.Columns["DateTaken"].Width = 120;
                if (dgvMyScores.Columns.Contains("TimeTaken")) dgvMyScores.Columns["TimeTaken"].Width = 90;
                if (dgvMyScores.Columns.Contains("TotalQuestions")) dgvMyScores.Columns["TotalQuestions"].Width = 100;
                if (dgvMyScores.Columns.Contains("CorrectAnswers")) dgvMyScores.Columns["CorrectAnswers"].Width = 100;
                if (dgvMyScores.Columns.Contains("StudentAnswers")) dgvMyScores.Columns["StudentAnswers"].Visible = false; // Raw dictionary is not user-friendly
                
                // Add a column for Exam Name if you want to display it
                // This would require fetching the Exam object for each result, potentially slow for many results.
                // For simplicity, we are just displaying the raw ExamID.
                // Or you could add a computed property to ExamResult model like:
                // public string ExamName => new ExamService().GetExamById(ExamID)?.Name ?? "Unknown Exam";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading your scores: {ex.Message}");
                Console.WriteLine($"StudentScoreTrackingForm Load Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for the "View Details" button click.
        /// Displays comprehensive details of the selected exam result.
        /// </summary>
        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            if (dgvMyScores.SelectedRows.Count > 0)
            {
                var selectedResult = dgvMyScores.SelectedRows[0].DataBoundItem as ExamResult;
                if (selectedResult != null)
                {
                    // Fetch the original exam and questions to provide a detailed review
                    Exam exam = _examService.GetExamById(selectedResult.ExamID);
                    if (exam == null)
                    {
                        MessageBox.Show("Original exam for this result not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string detailedReport = $"Exam Name: {exam.Name}\n\n";
                    detailedReport += $"Score: {selectedResult.Score:F1}% ({selectedResult.CorrectAnswers}/{selectedResult.TotalQuestions} Correct)\n";
                    detailedReport += $"Date Taken: {selectedResult.DateTaken:yyyy-MM-dd HH:mm}\n";
                    detailedReport += $"Time Taken: {TimeSpan.FromSeconds(selectedResult.TimeTaken):mm\\:ss}\n\n"; // Format as MM:SS

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
                                detailedReport += $"  Your Answer: {studentAnswer}\n";
                                detailedReport += $"  Correct Answer: {q.CorrectAnswer}\n\n";
                            }
                            else if (q.Type == QuestionType.TrueFalse)
                            {
                                detailedReport += $"  Your Answer: {studentAnswer}\n";
                                detailedReport += $"  Correct Answer: {q.CorrectAnswer}\n\n";
                            }
                            else if (q.Type == QuestionType.FillInTheBlank)
                            {
                                detailedReport += $"  Your Answer: {studentAnswer}\n";
                                detailedReport += $"  Correct Answer: {q.CorrectAnswer}\n\n";
                            }
                        }
                        questionNum++;
                    }

                    // For long reports, consider a new dialog or scrollable control.
                    // For now, a scrollable MessageBox workaround:
                    // Create a custom message box dialog or a new form for better display of long text.
                    // Example: new FormWithRichTextBox("Exam Review", detailedReport).ShowDialog();
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

        // --- Designer-generated code (typically in StudentScoreTrackingForm.Designer.cs) ---
        // You will need to ensure these controls are added to your form in Visual Studio Designer.

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.DataGridView dgvMyScores;
        private System.Windows.Forms.Button btnViewDetails;
        private System.Windows.Forms.Label lblErrorMessage;

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.dgvMyScores = new System.Windows.Forms.DataGridView();
            this.btnViewDetails = new System.Windows.Forms.Button();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMyScores)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(167, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "My Exam Scores";
            // 
            // dgvMyScores
            // 
            this.dgvMyScores.AllowUserToAddRows = false;
            this.dgvMyScores.AllowUserToDeleteRows = false;
            this.dgvMyScores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvMyScores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMyScores.Location = new System.Drawing.Point(20, 70);
            this.dgvMyScores.MultiSelect = false;
            this.dgvMyScores.Name = "dgvMyScores";
            this.dgvMyScores.ReadOnly = true;
            this.dgvMyScores.RowTemplate.Height = 25;
            this.dgvMyScores.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMyScores.Size = new System.Drawing.Size(760, 350);
            this.dgvMyScores.TabIndex = 1;
            // 
            // btnViewDetails
            // 
            this.btnViewDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnViewDetails.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnViewDetails.Location = new System.Drawing.Point(20, 435);
            this.btnViewDetails.Name = "btnViewDetails";
            this.btnViewDetails.Size = new System.Drawing.Size(120, 35);
            this.btnViewDetails.TabIndex = 2;
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
            this.lblErrorMessage.Location = new System.Drawing.Point(150, 445);
            this.lblErrorMessage.MaximumSize = new System.Drawing.Size(630, 0);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(76, 15);
            this.lblErrorMessage.TabIndex = 3;
            this.lblErrorMessage.Text = "Error Message";
            this.lblErrorMessage.Visible = false;
            // 
            // StudentScoreTrackingForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.lblErrorMessage);
            this.Controls.Add(this.btnViewDetails);
            this.Controls.Add(this.dgvMyScores);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new System.Drawing.Size(816, 519);
            this.Name = "StudentScoreTrackingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "My Exam Scores";
            this.Load += new System.EventHandler(this.StudentScoreTrackingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMyScores)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void StudentScoreTrackingForm_Load(object sender, EventArgs e)
        {
            // Any specific logic on form load can go here.
        }
    }
}
