using ExamPracticeSystemFull.Models;
using ExamPracticeSystemFull.Services;

namespace ExamPracticeSystemFull.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;


    public partial class TakeExamForm : Form
    {
        private readonly ExamService _examService;
        private readonly QuestionService _questionService;
        private Exam _currentExam;
        private List<Question> _examQuestions;
        private Dictionary<int, string> _studentAnswers; // Stores QuestionID -> Student's Answer
        private int _currentQuestionIndex;
        private DateTime _examStartTime;
        private int _timeRemainingSeconds; // In seconds

        /// <summary>
        /// Constructor for the TakeExamForm.
        /// Initializes services and loads the specified exam.
        /// </summary>
        /// <param name="examId">The ID of the exam to be taken.</param>
        public TakeExamForm(int examId)
        {
            InitializeComponent();
            _examService = new ExamService();
            _questionService = new QuestionService();
            _studentAnswers = new Dictionary<int, string>();
            _currentQuestionIndex = 0;

            LoadExam(examId);
            SetupExamForm();
        }

        /// <summary>
        /// Loads the exam details and its questions from the services.
        /// </summary>
        /// <param name="examId">The ID of the exam to load.</param>
        private void LoadExam(int examId)
        {
            try
            {
                _currentExam = _examService.GetExamById(examId);
                if (_currentExam == null)
                {
                    MessageBox.Show("Exam not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                _examQuestions = _currentExam.QuestionIDs
                                             .Select(qid => _questionService.GetQuestionById(qid))
                                             .Where(q => q != null) // Filter out any null questions if IDs are invalid
                                             .ToList();

                if (!_examQuestions.Any())
                {
                    MessageBox.Show("This exam contains no valid questions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                _timeRemainingSeconds = _currentExam.TimeLimit * 60; // Convert minutes to seconds
                _examStartTime = DateTime.Now; // Record start time for actual duration calculation
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading exam: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        /// <summary>
        /// Sets up the exam form's initial state, displays the first question, and starts the timer.
        /// </summary>
        private void SetupExamForm()
        {
            this.Text = $"Take Exam: {_currentExam.Name}";
            lblExamTitle.Text = _currentExam.Name;
            lblErrorMessage.Text = "";
            lblErrorMessage.Visible = false;

            DisplayQuestion(); // Display the first question

            // Initialize and start the timer
            examTimer.Interval = 1000; // 1 second
            examTimer.Tick += ExamTimer_Tick;
            examTimer.Start();
        }

        /// <summary>
        /// Event handler for the exam timer tick. Updates the displayed time and handles exam submission on timeout.
        /// </summary>
        private void ExamTimer_Tick(object sender, EventArgs e)
        {
            _timeRemainingSeconds--;

            if (_timeRemainingSeconds <= 0)
            {
                examTimer.Stop();
                MessageBox.Show("Time's up! Your exam will now be submitted.", "Time's Up", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SubmitExam();
            }
            else
            {
                TimeSpan time = TimeSpan.FromSeconds(_timeRemainingSeconds);
                lblTimer.Text = $"Time Remaining: {time.Minutes:D2}:{time.Seconds:D2}";
            }
        }

        /// <summary>
        /// Displays the current question and its options, based on the currentQuestionIndex.
        /// Also manages visibility of option fields based on question type.
        /// </summary>
        private void DisplayQuestion()
        {
            if (_examQuestions == null || !_examQuestions.Any() || _currentQuestionIndex < 0 || _currentQuestionIndex >= _examQuestions.Count)
            {
                // This should not happen if LoadExam is successful, but as a safeguard
                MessageBox.Show("Error: No question to display or invalid index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Question question = _examQuestions[_currentQuestionIndex];
            lblQuestionNumber.Text = $"Question {_currentQuestionIndex + 1} of {_examQuestions.Count}";
            txtQuestionText.Text = question.Text;

            // Hide/Show controls based on question type
            pnlOptions.Visible = false; // Panel for A,B,C,D options
            pnlTrueFalseOptions.Visible = false; // Panel for True/False options
            txtFillInBlankAnswer.Visible = false; // For Fill-in-the-blank

            // Clear previous answers
            ClearOptionSelections();
            txtFillInBlankAnswer.Clear();


            if (question.Type == QuestionType.MultipleChoice)
            {
                pnlOptions.Visible = true;
                rdoOptionA.Text = question.OptionA;
                rdoOptionB.Text = question.OptionB;
                rdoOptionC.Text = question.OptionC;
                rdoOptionD.Text = question.OptionD;
            }
            else if (question.Type == QuestionType.TrueFalse)
            {
                pnlTrueFalseOptions.Visible = true;
                // Options "True" and "False" are hardcoded on these radio buttons
            }
            else if (question.Type == QuestionType.FillInTheBlank)
            {
                txtFillInBlankAnswer.Visible = true;
            }

            // Load student's previously selected answer if it exists
            if (_studentAnswers.ContainsKey(question.ID))
            {
                SetStudentAnswer(question.Type, _studentAnswers[question.ID]);
            }

            UpdateNavigationButtons();
            lblErrorMessage.Visible = false; // Clear messages on new question
        }

        /// <summary>
        /// Sets the student's previously recorded answer on the UI.
        /// </summary>
        private void SetStudentAnswer(QuestionType type, string answer)
        {
            if (type == QuestionType.MultipleChoice)
            {
                switch (answer.ToUpper())
                {
                    case "A": rdoOptionA.Checked = true; break;
                    case "B": rdoOptionB.Checked = true; break;
                    case "C": rdoOptionC.Checked = true; break;
                    case "D": rdoOptionD.Checked = true; break;
                }
            }
            else if (type == QuestionType.TrueFalse)
            {
                if (answer.Equals("TRUE", StringComparison.OrdinalIgnoreCase)) rdoTrue.Checked = true;
                else if (answer.Equals("FALSE", StringComparison.OrdinalIgnoreCase)) rdoFalse.Checked = true;
            }
            else if (type == QuestionType.FillInTheBlank)
            {
                txtFillInBlankAnswer.Text = answer;
            }
        }

        /// <summary>
        /// Clears all radio button selections for options.
        /// </summary>
        private void ClearOptionSelections()
        {
            rdoOptionA.Checked = false;
            rdoOptionB.Checked = false;
            rdoOptionC.Checked = false;
            rdoOptionD.Checked = false;
            rdoTrue.Checked = false;
            rdoFalse.Checked = false;
        }

        /// <summary>
        /// Updates the state of the navigation buttons (Previous and Next).
        /// </summary>
        private void UpdateNavigationButtons()
        {
            btnPrevious.Enabled = (_currentQuestionIndex > 0);
            btnNext.Enabled = (_currentQuestionIndex < _examQuestions.Count - 1);
            btnSubmit.Visible = (_currentQuestionIndex == _examQuestions.Count - 1); // Show submit on last question
        }

        /// <summary>
        /// Captures the student's answer for the current question and stores it.
        /// </summary>
        private void CaptureCurrentAnswer()
        {
            if (_examQuestions == null || !_examQuestions.Any()) return;

            Question currentQuestion = _examQuestions[_currentQuestionIndex];
            string studentAnswer = "";

            if (currentQuestion.Type == QuestionType.MultipleChoice)
            {
                if (rdoOptionA.Checked) studentAnswer = "A";
                else if (rdoOptionB.Checked) studentAnswer = "B";
                else if (rdoOptionC.Checked) studentAnswer = "C";
                else if (rdoOptionD.Checked) studentAnswer = "D";
            }
            else if (currentQuestion.Type == QuestionType.TrueFalse)
            {
                if (rdoTrue.Checked) studentAnswer = "TRUE";
                else if (rdoFalse.Checked) studentAnswer = "FALSE";
            }
            else if (currentQuestion.Type == QuestionType.FillInTheBlank)
            {
                studentAnswer = txtFillInBlankAnswer.Text.Trim();
            }

            _studentAnswers[currentQuestion.ID] = studentAnswer; // Store or update answer
        }

        /// <summary>
        /// Event handler for the Previous button click.
        /// Captures current answer and navigates to the previous question.
        /// </summary>
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            CaptureCurrentAnswer();
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                DisplayQuestion();
            }
        }

        /// <summary>
        /// Event handler for the Next button click.
        /// Captures current answer and navigates to the next question.
        /// </summary>
        private void btnNext_Click(object sender, EventArgs e)
        {
            CaptureCurrentAnswer();
            if (_currentQuestionIndex < _examQuestions.Count - 1)
            {
                _currentQuestionIndex++;
                DisplayQuestion();
            }
        }

        /// <summary>
        /// Event handler for the Submit Exam button click.
        /// Validates answers, calculates score, and saves the exam result.
        /// </summary>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            CaptureCurrentAnswer(); // Capture answer for the last question

            DialogResult confirm = MessageBox.Show("Are you sure you want to submit the exam?", "Confirm Submission", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                SubmitExam();
            }
        }

        /// <summary>
        /// Performs the exam submission process: calculates score, creates ExamResult, and saves it.
        /// </summary>
        private void SubmitExam()
        {
            examTimer.Stop(); // Ensure timer is stopped

            int correctAnswersCount = 0;
            int totalQuestions = _examQuestions.Count;
            int timeTakenSeconds = (int)(DateTime.Now - _examStartTime).TotalSeconds;

            foreach (var question in _examQuestions)
            {
                string studentAnswer = _studentAnswers.ContainsKey(question.ID) ? _studentAnswers[question.ID] : "";

                // Compare answers based on question type
                bool isCorrect = false;
                if (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse)
                {
                    isCorrect = studentAnswer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                }
                else if (question.Type == QuestionType.FillInTheBlank)
                {
                    // For Fill-in-the-blank, a simple case-insensitive match.
                    // Could implement more sophisticated matching (e.g., trimming, multiple correct answers)
                    isCorrect = studentAnswer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                }

                if (isCorrect)
                {
                    correctAnswersCount++;
                }
            }

            // Create ExamResult object
            ExamResult result = new ExamResult(
                UserSession.Instance.GetCurrentUsername(),
                _currentExam.ExamID,
                _studentAnswers,
                totalQuestions,
                correctAnswersCount,
                timeTakenSeconds
            );

            try
            {
                bool success = _examService.SaveExamResult(result);
                if (success)
                {
                    MessageBox.Show($"Exam submitted successfully!\nYour score: {result.Score:F1}% ({result.CorrectAnswers}/{result.TotalQuestions} correct)",
                                    "Exam Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK; // Indicate success to parent form (MainForm)
                    this.Close();
                }
                else
                {
                    ShowErrorMessage("Failed to save exam result. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"An error occurred while saving results: {ex.Message}");
                Console.WriteLine($"Exam Submission Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for the form closing. Stops the timer if the user closes the form prematurely.
        /// </summary>
        private void TakeExamForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (examTimer.Enabled)
            {
                examTimer.Stop();
                DialogResult confirm = MessageBox.Show("Are you sure you want to exit the exam without submitting? Your progress will be lost.", "Exit Exam", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.No)
                {
                    e.Cancel = true; // Prevent closing
                    examTimer.Start(); // Restart timer
                }
            }
        }

        /// <summary>
        /// Displays an error/info message to the user.
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.ForeColor = Color.Red;
            lblErrorMessage.Visible = true;
        }


        // --- Designer-generated code (typically in TakeExamForm.Designer.cs) ---
        // You will need to ensure these controls are added to your form in Visual Studio Designer.

        private System.Windows.Forms.Label lblExamTitle;
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Label lblQuestionNumber;
        private System.Windows.Forms.RichTextBox txtQuestionText;
        private System.Windows.Forms.Panel pnlOptions; // Panel to hold radio buttons A,B,C,D
        private System.Windows.Forms.RadioButton rdoOptionA;
        private System.Windows.Forms.RadioButton rdoOptionB;
        private System.Windows.Forms.RadioButton rdoOptionC;
        private System.Windows.Forms.RadioButton rdoOptionD;
        private System.Windows.Forms.Panel pnlTrueFalseOptions; // Panel to hold True/False radio buttons
        private System.Windows.Forms.RadioButton rdoTrue;
        private System.Windows.Forms.RadioButton rdoFalse;
        private System.Windows.Forms.TextBox txtFillInBlankAnswer;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Label lblErrorMessage;
        private System.Windows.Forms.Timer examTimer; // Timer component


        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container(); // Required for Timer
            this.lblExamTitle = new System.Windows.Forms.Label();
            this.lblTimer = new System.Windows.Forms.Label();
            this.lblQuestionNumber = new System.Windows.Forms.Label();
            this.txtQuestionText = new System.Windows.Forms.RichTextBox();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.rdoOptionD = new System.Windows.Forms.RadioButton();
            this.rdoOptionC = new System.Windows.Forms.RadioButton();
            this.rdoOptionB = new System.Windows.Forms.RadioButton();
            this.rdoOptionA = new System.Windows.Forms.RadioButton();
            this.pnlTrueFalseOptions = new System.Windows.Forms.Panel();
            this.rdoFalse = new System.Windows.Forms.RadioButton();
            this.rdoTrue = new System.Windows.Forms.RadioButton();
            this.txtFillInBlankAnswer = new System.Windows.Forms.TextBox();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.examTimer = new System.Windows.Forms.Timer(this.components); // Initialize timer
            this.pnlOptions.SuspendLayout();
            this.pnlTrueFalseOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblExamTitle
            // 
            this.lblExamTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblExamTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblExamTitle.Location = new System.Drawing.Point(20, 20);
            this.lblExamTitle.Name = "lblExamTitle";
            this.lblExamTitle.Size = new System.Drawing.Size(600, 30);
            this.lblExamTitle.TabIndex = 0;
            this.lblExamTitle.Text = "Exam Name Placeholder";
            this.lblExamTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTimer
            // 
            this.lblTimer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTimer.AutoSize = true;
            this.lblTimer.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTimer.ForeColor = System.Drawing.Color.DarkRed;
            this.lblTimer.Location = new System.Drawing.Point(630, 25);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(130, 21);
            this.lblTimer.TabIndex = 1;
            this.lblTimer.Text = "Time Remaining:";
            // 
            // lblQuestionNumber
            // 
            this.lblQuestionNumber.AutoSize = true;
            this.lblQuestionNumber.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblQuestionNumber.Location = new System.Drawing.Point(20, 70);
            this.lblQuestionNumber.Name = "lblQuestionNumber";
            this.lblQuestionNumber.Size = new System.Drawing.Size(124, 19);
            this.lblQuestionNumber.TabIndex = 2;
            this.lblQuestionNumber.Text = "Question 1 of N";
            // 
            // txtQuestionText
            // 
            this.txtQuestionText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtQuestionText.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtQuestionText.Location = new System.Drawing.Point(20, 100);
            this.txtQuestionText.Name = "txtQuestionText";
            this.txtQuestionText.ReadOnly = true;
            this.txtQuestionText.Size = new System.Drawing.Size(760, 100);
            this.txtQuestionText.TabIndex = 3;
            this.txtQuestionText.Text = "Question text will appear here...";
            // 
            // pnlOptions
            // 
            this.pnlOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOptions.Controls.Add(this.rdoOptionD);
            this.pnlOptions.Controls.Add(this.rdoOptionC);
            this.pnlOptions.Controls.Add(this.rdoOptionB);
            this.pnlOptions.Controls.Add(this.rdoOptionA);
            this.pnlOptions.Location = new System.Drawing.Point(20, 210);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(760, 150);
            this.pnlOptions.TabIndex = 4;
            // 
            // rdoOptionD
            // 
            this.rdoOptionD.AutoSize = true;
            this.rdoOptionD.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rdoOptionD.Location = new System.Drawing.Point(10, 105);
            this.rdoOptionD.Name = "rdoOptionD";
            this.rdoOptionD.Size = new System.Drawing.Size(83, 23);
            this.rdoOptionD.TabIndex = 3;
            this.rdoOptionD.TabStop = true;
            this.rdoOptionD.Text = "Option D";
            this.rdoOptionD.UseVisualStyleBackColor = true;
            // 
            // rdoOptionC
            // 
            this.rdoOptionC.AutoSize = true;
            this.rdoOptionC.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rdoOptionC.Location = new System.Drawing.Point(10, 75);
            this.rdoOptionC.Name = "rdoOptionC";
            this.rdoOptionC.Size = new System.Drawing.Size(82, 23);
            this.rdoOptionC.TabIndex = 2;
            this.rdoOptionC.TabStop = true;
            this.rdoOptionC.Text = "Option C";
            this.rdoOptionC.UseVisualStyleBackColor = true;
            // 
            // rdoOptionB
            // 
            this.rdoOptionB.AutoSize = true;
            this.rdoOptionB.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rdoOptionB.Location = new System.Drawing.Point(10, 45);
            this.rdoOptionB.Name = "rdoOptionB";
            this.rdoOptionB.Size = new System.Drawing.Size(82, 23);
            this.rdoOptionB.TabIndex = 1;
            this.rdoOptionB.TabStop = true;
            this.rdoOptionB.Text = "Option B";
            this.rdoOptionB.UseVisualStyleBackColor = true;
            // 
            // rdoOptionA
            // 
            this.rdoOptionA.AutoSize = true;
            this.rdoOptionA.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rdoOptionA.Location = new System.Drawing.Point(10, 15);
            this.rdoOptionA.Name = "rdoOptionA";
            this.rdoOptionA.Size = new System.Drawing.Size(83, 23);
            this.rdoOptionA.TabIndex = 0;
            this.rdoOptionA.TabStop = true;
            this.rdoOptionA.Text = "Option A";
            this.rdoOptionA.UseVisualStyleBackColor = true;
            // 
            // pnlTrueFalseOptions
            // 
            this.pnlTrueFalseOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTrueFalseOptions.Controls.Add(this.rdoFalse);
            this.pnlTrueFalseOptions.Controls.Add(this.rdoTrue);
            this.pnlTrueFalseOptions.Location = new System.Drawing.Point(20, 210);
            this.pnlTrueFalseOptions.Name = "pnlTrueFalseOptions";
            this.pnlTrueFalseOptions.Size = new System.Drawing.Size(760, 40);
            this.pnlTrueFalseOptions.TabIndex = 5;
            this.pnlTrueFalseOptions.Visible = false;
            // 
            // rdoFalse
            // 
            this.rdoFalse.AutoSize = true;
            this.rdoFalse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rdoFalse.Location = new System.Drawing.Point(90, 10);
            this.rdoFalse.Name = "rdoFalse";
            this.rdoFalse.Size = new System.Drawing.Size(58, 23);
            this.rdoFalse.TabIndex = 1;
            this.rdoFalse.TabStop = true;
            this.rdoFalse.Text = "False";
            this.rdoFalse.UseVisualStyleBackColor = true;
            // 
            // rdoTrue
            // 
            this.rdoTrue.AutoSize = true;
            this.rdoTrue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rdoTrue.Location = new System.Drawing.Point(10, 10);
            this.rdoTrue.Name = "rdoTrue";
            this.rdoTrue.Size = new System.Drawing.Size(53, 23);
            this.rdoTrue.TabIndex = 0;
            this.rdoTrue.TabStop = true;
            this.rdoTrue.Text = "True";
            this.rdoTrue.UseVisualStyleBackColor = true;
            // 
            // txtFillInBlankAnswer
            // 
            this.txtFillInBlankAnswer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFillInBlankAnswer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtFillInBlankAnswer.Location = new System.Drawing.Point(20, 210);
            this.txtFillInBlankAnswer.Name = "txtFillInBlankAnswer";
            this.txtFillInBlankAnswer.Size = new System.Drawing.Size(760, 25);
            this.txtFillInBlankAnswer.TabIndex = 6;
            this.txtFillInBlankAnswer.Visible = false;
            // 
            // btnPrevious
            // 
            this.btnPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPrevious.Location = new System.Drawing.Point(20, 420);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(90, 35);
            this.btnPrevious.TabIndex = 7;
            this.btnPrevious.Text = "<< Previous";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(690, 420);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(90, 35);
            this.btnNext.TabIndex = 8;
            this.btnNext.Text = "Next >>";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnSubmit
            // 
            this.btnSubmit.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnSubmit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSubmit.Location = new System.Drawing.Point(355, 420);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(90, 35);
            this.btnSubmit.TabIndex = 9;
            this.btnSubmit.Text = "Submit Exam";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Visible = false;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblErrorMessage.AutoSize = true;
            this.lblErrorMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblErrorMessage.ForeColor = System.Drawing.Color.Red;
            this.lblErrorMessage.Location = new System.Drawing.Point(20, 380);
            this.lblErrorMessage.MaximumSize = new System.Drawing.Size(760, 0);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(76, 15);
            this.lblErrorMessage.TabIndex = 10;
            this.lblErrorMessage.Text = "Error Message";
            this.lblErrorMessage.Visible = false;
            // 
            // examTimer
            // 
            this.examTimer.Interval = 1000;
            this.examTimer.Tick += new System.EventHandler(this.ExamTimer_Tick);
            // 
            // TakeExamForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.lblErrorMessage);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.txtFillInBlankAnswer);
            this.Controls.Add(this.pnlTrueFalseOptions);
            this.Controls.Add(this.pnlOptions);
            this.Controls.Add(this.txtQuestionText);
            this.Controls.Add(this.lblQuestionNumber);
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.lblExamTitle);
            this.MinimumSize = new System.Drawing.Size(816, 519);
            this.Name = "TakeExamForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Take Exam";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TakeExamForm_FormClosing);
            this.Load += new System.EventHandler(this.TakeExamForm_Load); // Add Load event handler
            this.pnlOptions.ResumeLayout(false);
            this.pnlOptions.PerformLayout();
            this.pnlTrueFalseOptions.ResumeLayout(false);
            this.pnlTrueFalseOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.ComponentModel.IContainer components; // Declare components for the timer.

        private void TakeExamForm_Load(object sender, EventArgs e)
        {
            // Any specific logic on form load can go here.
        }
    }
}
