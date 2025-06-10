using ExamPracticeSystemFull.Models;
using ExamPracticeSystemFull.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ExamPracticeSystemFull.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    public partial class QuestionCreationForm : Form
    {
        private readonly QuestionService _questionService;
        private int _questionIdToEdit; // To store the ID of the question being edited, 0 for new
        private Question _currentQuestion; // Stores the question object being edited/created

        /// <summary>
        /// Constructor for creating a new question.
        /// </summary>
        public QuestionCreationForm()
        {
            InitializeComponent();
            _questionService = new QuestionService();
            _questionIdToEdit = 0; // Indicates a new question
            SetupFormForNewQuestion();
        }

        /// <summary>
        /// Constructor for editing an existing question.
        /// </summary>
        /// <param name="questionId">The ID of the question to edit.</param>
        public QuestionCreationForm(int questionId)
        {
            InitializeComponent();
            _questionService = new QuestionService();
            _questionIdToEdit = questionId; // Set the ID for editing
            SetupFormForEditQuestion();
        }

        /// <summary>
        /// Sets up the form for creating a new question.
        /// </summary>
        private void SetupFormForNewQuestion()
        {
            this.Text = "Create New Question";
            lblTitle.Text = "Create New Question";
            lblErrorMessage.Text = "";
            lblErrorMessage.Visible = false;
            btnSave.Text = "Add Question";

            // Populate Category ComboBox with example categories
            // You might want to load these from a file or a distinct list of existing categories
            cmbCategory.Items.AddRange(new string[] { "Mathematics", "Science", "History", "Programming", "General Knowledge" });
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0; // Select first by default

            // Populate Difficulty ComboBox
            cmbDifficulty.DataSource = Enum.GetValues(typeof(DifficultyLevel));
            cmbDifficulty.SelectedItem = DifficultyLevel.Medium; // Default difficulty

            // Populate Question Type ComboBox
            cmbQuestionType.DataSource = Enum.GetValues(typeof(QuestionType));
            cmbQuestionType.SelectedItem = QuestionType.MultipleChoice; // Default type

            EnableDisableOptionFields(); // Ensure correct option fields are visible/editable
        }

        /// <summary>
        /// Sets up the form for editing an existing question.
        /// Loads the question's data into the form controls.
        /// </summary>
        private void SetupFormForEditQuestion()
        {
            this.Text = "Edit Question";
            lblTitle.Text = "Edit Question";
            btnSave.Text = "Save Changes";

            _currentQuestion = _questionService.GetQuestionById(_questionIdToEdit);

            if (_currentQuestion == null)
            {
                MessageBox.Show("Question not found for editing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            // Populate fields with existing question data
            txtQuestionText.Text = _currentQuestion.Text;
            txtOptionA.Text = _currentQuestion.OptionA;
            txtOptionB.Text = _currentQuestion.OptionB;
            txtOptionC.Text = _currentQuestion.OptionC;
            txtOptionD.Text = _currentQuestion.OptionD;

            // Populate Category ComboBox
            cmbCategory.Items.AddRange(new string[] { "Mathematics", "Science", "History", "Programming", "General Knowledge" });
            if (!cmbCategory.Items.Contains(_currentQuestion.Category))
            {
                cmbCategory.Items.Add(_currentQuestion.Category); // Add if not already present
            }
            cmbCategory.SelectedItem = _currentQuestion.Category;

            // Populate Difficulty ComboBox
            cmbDifficulty.DataSource = Enum.GetValues(typeof(DifficultyLevel));
            cmbDifficulty.SelectedItem = _currentQuestion.Difficulty;

            // Populate Question Type ComboBox
            cmbQuestionType.DataSource = Enum.GetValues(typeof(QuestionType));
            cmbQuestionType.SelectedItem = _currentQuestion.Type;

            SetCorrectAnswerRadioButton(_currentQuestion.CorrectAnswer);
            EnableDisableOptionFields(); // Adjust fields based on loaded type
        }

        /// <summary>
        /// Sets the correct answer radio button based on the provided answer string.
        /// </summary>
        private void SetCorrectAnswerRadioButton(string correctAnswer)
        {
            switch (correctAnswer.ToUpper())
            {
                case "A": rdoCorrectA.Checked = true; break;
                case "B": rdoCorrectB.Checked = true; break;
                case "C": rdoCorrectC.Checked = true; break;
                case "D": rdoCorrectD.Checked = true; break;
                case "TRUE": rdoCorrectTrue.Checked = true; break;
                case "FALSE": rdoCorrectFalse.Checked = true; break;
                default: break; // For Fill-in-the-blank or other types, no radio button needed.
            }
        }

        /// <summary>
        /// Gets the selected correct answer from the radio buttons.
        /// </summary>
        /// <returns>The string representation of the correct answer (e.g., "A", "TRUE").</returns>
        private string GetSelectedCorrectAnswer()
        {
            if (rdoCorrectA.Checked) return "A";
            if (rdoCorrectB.Checked) return "B";
            if (rdoCorrectC.Checked) return "C";
            if (rdoCorrectD.Checked) return "D";
            if (rdoCorrectTrue.Checked) return "TRUE";
            if (rdoCorrectFalse.Checked) return "FALSE";
            return ""; // For fill-in-the-blank, the text itself is the answer
        }

        /// <summary>
        /// Enables/disables and shows/hides option fields based on the selected question type.
        /// </summary>
        private void EnableDisableOptionFields()
        {
            QuestionType selectedType = (QuestionType)cmbQuestionType.SelectedItem;

            bool isMultipleChoice = (selectedType == QuestionType.MultipleChoice);
            bool isTrueFalse = (selectedType == QuestionType.TrueFalse);
            bool isFillInTheBlank = (selectedType == QuestionType.FillInTheBlank);

            // Hide all options first
            lblOptionA.Visible = isMultipleChoice || isTrueFalse;
            txtOptionA.Visible = isMultipleChoice || isTrueFalse;
            rdoCorrectA.Visible = isMultipleChoice;
            lblOptionB.Visible = isMultipleChoice;
            txtOptionB.Visible = isMultipleChoice;
            rdoCorrectB.Visible = isMultipleChoice;
            lblOptionC.Visible = isMultipleChoice;
            txtOptionC.Visible = isMultipleChoice;
            rdoCorrectC.Visible = isMultipleChoice;
            lblOptionD.Visible = isMultipleChoice;
            txtOptionD.Visible = isMultipleChoice;
            rdoCorrectD.Visible = isMultipleChoice;

            // True/False specific controls
            lblCorrectTrueFalse.Visible = isTrueFalse;
            pnlTrueFalseOptions.Visible = isTrueFalse; // Panel holding True/False radio buttons
            rdoCorrectTrue.Visible = isTrueFalse;
            rdoCorrectFalse.Visible = isTrueFalse;

            // Fill-in-the-blank specific: Question Text is the answer input directly
            // No specific fields for this, the main text box is used.
            // The correct answer for FillInTheBlank will be the actual text entered in txtQuestionText

            // For MultipleChoice: ensure A, B, C, D radio buttons are visible
            rdoCorrectA.Visible = isMultipleChoice;
            rdoCorrectB.Visible = isMultipleChoice;
            rdoCorrectC.Visible = isMultipleChoice;
            rdoCorrectD.Visible = isMultipleChoice;

            // Manage the visibility of the Correct Answer label for multi-choice options
            lblCorrectAnswer.Visible = isMultipleChoice;
            lblCorrectAnswerFillInBlank.Visible = isFillInTheBlank;
            txtCorrectAnswerFillInBlank.Visible = isFillInTheBlank;

            // Clear options if switching type to avoid confusion
            if (!isMultipleChoice)
            {
                txtOptionA.Text = "";
                txtOptionB.Text = "";
                txtOptionC.Text = "";
                txtOptionD.Text = "";
                rdoCorrectA.Checked = false;
                rdoCorrectB.Checked = false;
                rdoCorrectC.Checked = false;
                rdoCorrectD.Checked = false;
            }
            if (!isTrueFalse)
            {
                rdoCorrectTrue.Checked = false;
                rdoCorrectFalse.Checked = false;
            }
            if (!isFillInTheBlank)
            {
                txtCorrectAnswerFillInBlank.Text = "";
            }
        }

        /// <summary>
        /// Event handler for the Question Type ComboBox selection change.
        /// Adjusts visibility of option fields.
        /// </summary>
        private void cmbQuestionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableOptionFields();
        }

        /// <summary>
        /// Event handler for the Save/Add Question button click.
        /// Validates input and saves/updates the question.
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            string questionText = txtQuestionText.Text.Trim();
            string optionA = txtOptionA.Text.Trim();
            string optionB = txtOptionB.Text.Trim();
            string optionC = txtOptionC.Text.Trim();
            string optionD = txtOptionD.Text.Trim();
            string category = cmbCategory.Text.Trim(); // Allows user to type new category
            DifficultyLevel difficulty = (DifficultyLevel)cmbDifficulty.SelectedItem;
            QuestionType type = (QuestionType)cmbQuestionType.SelectedItem;
            string createdBy = UserSession.Instance.GetCurrentUsername(); // Get current lecturer username

            // Validate common fields
            ValidationResult textValidation = ValidationService.ValidateTextField(questionText, "Question Text", 10, 1000);
            if (!textValidation.IsValid) { ShowErrorMessage(textValidation.Message); return; }

            ValidationResult categoryValidation = ValidationService.ValidateTextField(category, "Category", 3, 50);
            if (!categoryValidation.IsValid) { ShowErrorMessage(categoryValidation.Message); return; }

            string correctAnswer = "";

            if (type == QuestionType.MultipleChoice)
            {
                // Validate options for MultipleChoice
                ValidationResult optAValidation = ValidationService.ValidateTextField(optionA, "Option A", 1, 250);
                if (!optAValidation.IsValid) { ShowErrorMessage(optAValidation.Message); return; }
                ValidationResult optBValidation = ValidationService.ValidateTextField(optionB, "Option B", 1, 250);
                if (!optBValidation.IsValid) { ShowErrorMessage(optBValidation.Message); return; }
                ValidationResult optCValidation = ValidationService.ValidateTextField(optionC, "Option C", 1, 250);
                if (!optCValidation.IsValid) { ShowErrorMessage(optCValidation.Message); return; }
                ValidationResult optDValidation = ValidationService.ValidateTextField(optionD, "Option D", 1, 250);
                if (!optDValidation.IsValid) { ShowErrorMessage(optDValidation.Message); return; }

                correctAnswer = GetSelectedCorrectAnswer();
                if (string.IsNullOrEmpty(correctAnswer))
                {
                    ShowErrorMessage("Please select the correct answer (A, B, C, or D).");
                    return;
                }
            }
            else if (type == QuestionType.TrueFalse)
            {
                correctAnswer = GetSelectedCorrectAnswer();
                if (string.IsNullOrEmpty(correctAnswer))
                {
                    ShowErrorMessage("Please select the correct answer (True or False).");
                    return;
                }
                // Clear other options if True/False selected
                optionA = "True";
                optionB = "False";
                optionC = "";
                optionD = "";
            }
            else if (type == QuestionType.FillInTheBlank)
            {
                correctAnswer = txtCorrectAnswerFillInBlank.Text.Trim();
                ValidationResult fibAnswerValidation = ValidationService.ValidateTextField(correctAnswer, "Correct Answer", 1, 250);
                if (!fibAnswerValidation.IsValid) { ShowErrorMessage(fibAnswerValidation.Message); return; }
                
                // For Fill-in-the-blank, options A, B, C, D are not applicable
                optionA = "";
                optionB = "";
                optionC = "";
                optionD = "";
            }


            try
            {
                bool success;
                if (_questionIdToEdit == 0) // New question
                {
                    Question newQuestion = new Question(questionText, optionA, optionB, optionC, optionD,
                                                       correctAnswer, category, difficulty, type, createdBy);
                    success = _questionService.AddQuestion(newQuestion);
                }
                else // Edit existing question
                {
                    _currentQuestion.Text = questionText;
                    _currentQuestion.OptionA = optionA;
                    _currentQuestion.OptionB = optionB;
                    _currentQuestion.OptionC = optionC;
                    _currentQuestion.OptionD = optionD;
                    _currentQuestion.CorrectAnswer = correctAnswer;
                    _currentQuestion.Category = category;
                    _currentQuestion.Difficulty = difficulty;
                    _currentQuestion.Type = type;
                    // CreatedBy and CreatedDate remain the same for edits
                    success = _questionService.UpdateQuestion(_currentQuestion);
                }

                if (success)
                {
                    MessageBox.Show("Question saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK; // Indicate success to parent form
                    this.Close();
                }
                else
                {
                    ShowErrorMessage("Failed to save question. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"An error occurred: {ex.Message}");
                Console.WriteLine($"Question Save Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for the Cancel button click.
        /// Closes the form without saving.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // Indicate cancellation
            this.Close();
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

        // --- Designer-generated code (typically in QuestionCreationForm.Designer.cs) ---
        // You will need to ensure these controls are added to your form in Visual Studio Designer.

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblQuestionText;
        private System.Windows.Forms.RichTextBox txtQuestionText; // RichTextBox for multiline text
        private System.Windows.Forms.Label lblOptionA;
        private System.Windows.Forms.TextBox txtOptionA;
        private System.Windows.Forms.Label lblOptionB;
        private System.Windows.Forms.TextBox txtOptionB;
        private System.Windows.Forms.Label lblOptionC;
        private System.Windows.Forms.TextBox txtOptionC;
        private System.Windows.Forms.Label lblOptionD;
        private System.Windows.Forms.TextBox txtOptionD;
        private System.Windows.Forms.Label lblCorrectAnswer;
        private System.Windows.Forms.RadioButton rdoCorrectA;
        private System.Windows.Forms.RadioButton rdoCorrectB;
        private System.Windows.Forms.RadioButton rdoCorrectC;
        private System.Windows.Forms.RadioButton rdoCorrectD;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblDifficulty;
        private System.Windows.Forms.ComboBox cmbDifficulty;
        private System.Windows.Forms.Label lblQuestionType;
        private System.Windows.Forms.ComboBox cmbQuestionType;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblErrorMessage;
        private System.Windows.Forms.Label lblCorrectTrueFalse; // Label for True/False correct answer
        private System.Windows.Forms.RadioButton rdoCorrectTrue;
        private System.Windows.Forms.RadioButton rdoCorrectFalse;
        private System.Windows.Forms.Panel pnlTrueFalseOptions; // Panel to group True/False radio buttons
        private System.Windows.Forms.Label lblCorrectAnswerFillInBlank;
        private System.Windows.Forms.TextBox txtCorrectAnswerFillInBlank;


        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblQuestionText = new System.Windows.Forms.Label();
            this.txtQuestionText = new System.Windows.Forms.RichTextBox();
            this.lblOptionA = new System.Windows.Forms.Label();
            this.txtOptionA = new System.Windows.Forms.TextBox();
            this.lblOptionB = new System.Windows.Forms.Label();
            this.txtOptionB = new System.Windows.Forms.TextBox();
            this.lblOptionC = new System.Windows.Forms.Label();
            this.txtOptionC = new System.Windows.Forms.TextBox();
            this.lblOptionD = new System.Windows.Forms.Label();
            this.txtOptionD = new System.Windows.Forms.TextBox();
            this.lblCorrectAnswer = new System.Windows.Forms.Label();
            this.rdoCorrectA = new System.Windows.Forms.RadioButton();
            this.rdoCorrectB = new System.Windows.Forms.RadioButton();
            this.rdoCorrectC = new System.Windows.Forms.RadioButton();
            this.rdoCorrectD = new System.Windows.Forms.RadioButton();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblDifficulty = new System.Windows.Forms.Label();
            this.cmbDifficulty = new System.Windows.Forms.ComboBox();
            this.lblQuestionType = new System.Windows.Forms.Label();
            this.cmbQuestionType = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.lblCorrectTrueFalse = new System.Windows.Forms.Label();
            this.rdoCorrectTrue = new System.Windows.Forms.RadioButton();
            this.rdoCorrectFalse = new System.Windows.Forms.RadioButton();
            this.pnlTrueFalseOptions = new System.Windows.Forms.Panel();
            this.lblCorrectAnswerFillInBlank = new System.Windows.Forms.Label();
            this.txtCorrectAnswerFillInBlank = new System.Windows.Forms.TextBox();
            this.pnlTrueFalseOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(214, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Create New Question";
            // 
            // lblQuestionText
            // 
            this.lblQuestionText.AutoSize = true;
            this.lblQuestionText.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblQuestionText.Location = new System.Drawing.Point(20, 70);
            this.lblQuestionText.Name = "lblQuestionText";
            this.lblQuestionText.Size = new System.Drawing.Size(95, 19);
            this.lblQuestionText.TabIndex = 1;
            this.lblQuestionText.Text = "Question Text:";
            // 
            // txtQuestionText
            // 
            this.txtQuestionText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtQuestionText.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtQuestionText.Location = new System.Drawing.Point(150, 67);
            this.txtQuestionText.Name = "txtQuestionText";
            this.txtQuestionText.Size = new System.Drawing.Size(450, 80);
            this.txtQuestionText.TabIndex = 2;
            this.txtQuestionText.Text = "";
            // 
            // lblOptionA
            // 
            this.lblOptionA.AutoSize = true;
            this.lblOptionA.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblOptionA.Location = new System.Drawing.Point(20, 160);
            this.lblOptionA.Name = "lblOptionA";
            this.lblOptionA.Size = new System.Drawing.Size(69, 19);
            this.lblOptionA.TabIndex = 3;
            this.lblOptionA.Text = "Option A:";
            // 
            // txtOptionA
            // 
            this.txtOptionA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOptionA.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtOptionA.Location = new System.Drawing.Point(150, 157);
            this.txtOptionA.Name = "txtOptionA";
            this.txtOptionA.Size = new System.Drawing.Size(350, 25);
            this.txtOptionA.TabIndex = 4;
            // 
            // lblOptionB
            // 
            this.lblOptionB.AutoSize = true;
            this.lblOptionB.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblOptionB.Location = new System.Drawing.Point(20, 195);
            this.lblOptionB.Name = "lblOptionB";
            this.lblOptionB.Size = new System.Drawing.Size(68, 19);
            this.lblOptionB.TabIndex = 5;
            this.lblOptionB.Text = "Option B:";
            // 
            // txtOptionB
            // 
            this.txtOptionB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOptionB.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtOptionB.Location = new System.Drawing.Point(150, 192);
            this.txtOptionB.Name = "txtOptionB";
            this.txtOptionB.Size = new System.Drawing.Size(350, 25);
            this.txtOptionB.TabIndex = 6;
            // 
            // lblOptionC
            // 
            this.lblOptionC.AutoSize = true;
            this.lblOptionC.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblOptionC.Location = new System.Drawing.Point(20, 230);
            this.lblOptionC.Name = "lblOptionC";
            this.lblOptionC.Size = new System.Drawing.Size(68, 19);
            this.lblOptionC.TabIndex = 7;
            this.lblOptionC.Text = "Option C:";
            // 
            // txtOptionC
            // 
            this.txtOptionC.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOptionC.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtOptionC.Location = new System.Drawing.Point(150, 227);
            this.txtOptionC.Name = "txtOptionC";
            this.txtOptionC.Size = new System.Drawing.Size(350, 25);
            this.txtOptionC.TabIndex = 8;
            // 
            // lblOptionD
            // 
            this.lblOptionD.AutoSize = true;
            this.lblOptionD.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblOptionD.Location = new System.Drawing.Point(20, 265);
            this.lblOptionD.Name = "lblOptionD";
            this.lblOptionD.Size = new System.Drawing.Size(70, 19);
            this.lblOptionD.TabIndex = 9;
            this.lblOptionD.Text = "Option D:";
            // 
            // txtOptionD
            // 
            this.txtOptionD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOptionD.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtOptionD.Location = new System.Drawing.Point(150, 262);
            this.txtOptionD.Name = "txtOptionD";
            this.txtOptionD.Size = new System.Drawing.Size(350, 25);
            this.txtOptionD.TabIndex = 10;
            // 
            // lblCorrectAnswer
            // 
            this.lblCorrectAnswer.AutoSize = true;
            this.lblCorrectAnswer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCorrectAnswer.Location = new System.Drawing.Point(500, 160);
            this.lblCorrectAnswer.Name = "lblCorrectAnswer";
            this.lblCorrectAnswer.Size = new System.Drawing.Size(65, 19);
            this.lblCorrectAnswer.TabIndex = 11;
            this.lblCorrectAnswer.Text = "Correct:";
            // 
            // rdoCorrectA
            // 
            this.rdoCorrectA.AutoSize = true;
            this.rdoCorrectA.Location = new System.Drawing.Point(510, 160);
            this.rdoCorrectA.Name = "rdoCorrectA";
            this.rdoCorrectA.Size = new System.Drawing.Size(32, 19);
            this.rdoCorrectA.TabIndex = 12;
            this.rdoCorrectA.TabStop = true;
            this.rdoCorrectA.Text = "A";
            this.rdoCorrectA.UseVisualStyleBackColor = true;
            // 
            // rdoCorrectB
            // 
            this.rdoCorrectB.AutoSize = true;
            this.rdoCorrectB.Location = new System.Drawing.Point(510, 195);
            this.rdoCorrectB.Name = "rdoCorrectB";
            this.rdoCorrectB.Size = new System.Drawing.Size(31, 19);
            this.rdoCorrectB.TabIndex = 13;
            this.rdoCorrectB.TabStop = true;
            this.rdoCorrectB.Text = "B";
            this.rdoCorrectB.UseVisualStyleBackColor = true;
            // 
            // rdoCorrectC
            // 
            this.rdoCorrectC.AutoSize = true;
            this.rdoCorrectC.Location = new System.Drawing.Point(510, 230);
            this.rdoCorrectC.Name = "rdoCorrectC";
            this.rdoCorrectC.Size = new System.Drawing.Size(31, 19);
            this.rdoCorrectC.TabIndex = 14;
            this.rdoCorrectC.TabStop = true;
            this.rdoCorrectC.Text = "C";
            this.rdoCorrectC.UseVisualStyleBackColor = true;
            // 
            // rdoCorrectD
            // 
            this.rdoCorrectD.AutoSize = true;
            this.rdoCorrectD.Location = new System.Drawing.Point(510, 265);
            this.rdoCorrectD.Name = "rdoCorrectD";
            this.rdoCorrectD.Size = new System.Drawing.Size(32, 19);
            this.rdoCorrectD.TabIndex = 15;
            this.rdoCorrectD.TabStop = true;
            this.rdoCorrectD.Text = "D";
            this.rdoCorrectD.UseVisualStyleBackColor = true;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCategory.Location = new System.Drawing.Point(20, 300);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(67, 19);
            this.lblCategory.TabIndex = 16;
            this.lblCategory.Text = "Category:";
            // 
            // cmbCategory
            // 
            this.cmbCategory.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Location = new System.Drawing.Point(150, 297);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(200, 25);
            this.cmbCategory.TabIndex = 17;
            // 
            // lblDifficulty
            // 
            this.lblDifficulty.AutoSize = true;
            this.lblDifficulty.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblDifficulty.Location = new System.Drawing.Point(20, 335);
            this.lblDifficulty.Name = "lblDifficulty";
            this.lblDifficulty.Size = new System.Drawing.Size(68, 19);
            this.lblDifficulty.TabIndex = 18;
            this.lblDifficulty.Text = "Difficulty:";
            // 
            // cmbDifficulty
            // 
            this.cmbDifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDifficulty.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbDifficulty.FormattingEnabled = true;
            this.cmbDifficulty.Location = new System.Drawing.Point(150, 332);
            this.cmbDifficulty.Name = "cmbDifficulty";
            this.cmbDifficulty.Size = new System.Drawing.Size(200, 25);
            this.cmbDifficulty.TabIndex = 19;
            // 
            // lblQuestionType
            // 
            this.lblQuestionType.AutoSize = true;
            this.lblQuestionType.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblQuestionType.Location = new System.Drawing.Point(20, 370);
            this.lblQuestionType.Name = "lblQuestionType";
            this.lblQuestionType.Size = new System.Drawing.Size(99, 19);
            this.lblQuestionType.TabIndex = 20;
            this.lblQuestionType.Text = "Question Type:";
            // 
            // cmbQuestionType
            // 
            this.cmbQuestionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbQuestionType.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbQuestionType.FormattingEnabled = true;
            this.cmbQuestionType.Location = new System.Drawing.Point(150, 367);
            this.cmbQuestionType.Name = "cmbQuestionType";
            this.cmbQuestionType.Size = new System.Drawing.Size(200, 25);
            this.cmbQuestionType.TabIndex = 21;
            this.cmbQuestionType.SelectedIndexChanged += new System.EventHandler(this.cmbQuestionType_SelectedIndexChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSave.Location = new System.Drawing.Point(400, 440);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 35);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnCancel.Location = new System.Drawing.Point(510, 440);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblErrorMessage.AutoSize = true;
            this.lblErrorMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblErrorMessage.ForeColor = System.Drawing.Color.Red;
            this.lblErrorMessage.Location = new System.Drawing.Point(20, 410);
            this.lblErrorMessage.MaximumSize = new System.Drawing.Size(580, 0);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(76, 15);
            this.lblErrorMessage.TabIndex = 24;
            this.lblErrorMessage.Text = "Error Message";
            this.lblErrorMessage.Visible = false;
            // 
            // lblCorrectTrueFalse
            // 
            this.lblCorrectTrueFalse.AutoSize = true;
            this.lblCorrectTrueFalse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCorrectTrueFalse.Location = new System.Drawing.Point(500, 160);
            this.lblCorrectTrueFalse.Name = "lblCorrectTrueFalse";
            this.lblCorrectTrueFalse.Size = new System.Drawing.Size(65, 19);
            this.lblCorrectTrueFalse.TabIndex = 25;
            this.lblCorrectTrueFalse.Text = "Correct:";
            this.lblCorrectTrueFalse.Visible = false;
            // 
            // rdoCorrectTrue
            // 
            this.rdoCorrectTrue.AutoSize = true;
            this.rdoCorrectTrue.Location = new System.Drawing.Point(0, 3);
            this.rdoCorrectTrue.Name = "rdoCorrectTrue";
            this.rdoCorrectTrue.Size = new System.Drawing.Size(50, 19);
            this.rdoCorrectTrue.TabIndex = 26;
            this.rdoCorrectTrue.TabStop = true;
            this.rdoCorrectTrue.Text = "True";
            this.rdoCorrectTrue.UseVisualStyleBackColor = true;
            // 
            // rdoCorrectFalse
            // 
            this.rdoCorrectFalse.AutoSize = true;
            this.rdoCorrectFalse.Location = new System.Drawing.Point(56, 3);
            this.rdoCorrectFalse.Name = "rdoCorrectFalse";
            this.rdoCorrectFalse.Size = new System.Drawing.Size(53, 19);
            this.rdoCorrectFalse.TabIndex = 27;
            this.rdoCorrectFalse.TabStop = true;
            this.rdoCorrectFalse.Text = "False";
            this.rdoCorrectFalse.UseVisualStyleBackColor = true;
            // 
            // pnlTrueFalseOptions
            // 
            this.pnlTrueFalseOptions.Controls.Add(this.rdoCorrectTrue);
            this.pnlTrueFalseOptions.Controls.Add(this.rdoCorrectFalse);
            this.pnlTrueFalseOptions.Location = new System.Drawing.Point(510, 180);
            this.pnlTrueFalseOptions.Name = "pnlTrueFalseOptions";
            this.pnlTrueFalseOptions.Size = new System.Drawing.Size(110, 25);
            this.pnlTrueFalseOptions.TabIndex = 28;
            this.pnlTrueFalseOptions.Visible = false;
            // 
            // lblCorrectAnswerFillInBlank
            // 
            this.lblCorrectAnswerFillInBlank.AutoSize = true;
            this.lblCorrectAnswerFillInBlank.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCorrectAnswerFillInBlank.Location = new System.Drawing.Point(20, 160);
            this.lblCorrectAnswerFillInBlank.Name = "lblCorrectAnswerFillInBlank";
            this.lblCorrectAnswerFillInBlank.Size = new System.Drawing.Size(114, 19);
            this.lblCorrectAnswerFillInBlank.TabIndex = 29;
            this.lblCorrectAnswerFillInBlank.Text = "Correct Answer:";
            this.lblCorrectAnswerFillInBlank.Visible = false;
            // 
            // txtCorrectAnswerFillInBlank
            // 
            this.txtCorrectAnswerFillInBlank.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCorrectAnswerFillInBlank.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtCorrectAnswerFillInBlank.Location = new System.Drawing.Point(150, 157);
            this.txtCorrectAnswerFillInBlank.Name = "txtCorrectAnswerFillInBlank";
            this.txtCorrectAnswerFillInBlank.Size = new System.Drawing.Size(350, 25);
            this.txtCorrectAnswerFillInBlank.TabIndex = 30;
            this.txtCorrectAnswerFillInBlank.Visible = false;
            // 
            // QuestionCreationForm
            // 
            this.ClientSize = new System.Drawing.Size(620, 500);
            this.Controls.Add(this.txtCorrectAnswerFillInBlank);
            this.Controls.Add(this.lblCorrectAnswerFillInBlank);
            this.Controls.Add(this.pnlTrueFalseOptions);
            this.Controls.Add(this.lblCorrectTrueFalse);
            this.Controls.Add(this.lblErrorMessage);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.cmbQuestionType);
            this.Controls.Add(this.lblQuestionType);
            this.Controls.Add(this.cmbDifficulty);
            this.Controls.Add(this.lblDifficulty);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.rdoCorrectD);
            this.Controls.Add(this.rdoCorrectC);
            this.Controls.Add(this.rdoCorrectB);
            this.Controls.Add(this.rdoCorrectA);
            this.Controls.Add(this.lblCorrectAnswer);
            this.Controls.Add(this.txtOptionD);
            this.Controls.Add(this.lblOptionD);
            this.Controls.Add(this.txtOptionC);
            this.Controls.Add(this.lblOptionC);
            this.Controls.Add(this.txtOptionB);
            this.Controls.Add(this.lblOptionB);
            this.Controls.Add(this.txtOptionA);
            this.Controls.Add(this.lblOptionA);
            this.Controls.Add(this.txtQuestionText);
            this.Controls.Add(this.lblQuestionText);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new System.Drawing.Size(636, 539);
            this.Name = "QuestionCreationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Question Creation";
            this.Load += new System.EventHandler(this.QuestionCreationForm_Load);
            this.pnlTrueFalseOptions.ResumeLayout(false);
            this.pnlTrueFalseOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void QuestionCreationForm_Load(object sender, EventArgs e)
        {
            // Initial setup on load to ensure proper state
            EnableDisableOptionFields();
        }
    }
}
