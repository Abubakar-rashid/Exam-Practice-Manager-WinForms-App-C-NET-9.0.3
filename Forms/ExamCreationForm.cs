using ExamPracticeSystemFull.Models;
using ExamPracticeSystemFull.Services;

namespace ExamPracticeSystemFull.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using ExamPracticeSystemFull.Models; // For Exam, Question, DifficultyLevel
    using ExamPracticeSystemFull.Services; // For ExamService, QuestionService, ValidationService

    public partial class ExamCreationForm : Form
    {
        private readonly ExamService _examService;
        private readonly QuestionService _questionService;

        private List<Question> _availableQuestions; // All questions from the bank
        private List<Question> _selectedQuestions;  // Questions chosen for the current exam

        /// <summary>
        /// Constructor for the ExamCreationForm.
        /// Initializes services and sets up the form.
        /// </summary>
        public ExamCreationForm()
        {
            InitializeComponent();
            _examService = new ExamService();
            _questionService = new QuestionService();
            _selectedQuestions = new List<Question>(); // Initialize empty list for selected questions
            
            LoadAvailableQuestions();
            SetupFormDefaults();
            
        }

        /// <summary>
        /// Sets up default properties for form controls and labels.
        /// </summary>
        private void SetupFormDefaults()
        {
            this.Text = "Create New Exam";
            lblTitle.Text = "Create New Exam";
            lblErrorMessage.Text = "";
            lblErrorMessage.Visible = false;

            // Initialize NumericUpDown controls
            numQuestionCount.Minimum = 1;
            numQuestionCount.Maximum = 100; // Max questions per exam, adjust as needed
            numQuestionCount.Value = 10;    // Default

            numTimeLimit.Minimum = 5;
            numTimeLimit.Maximum = 300; // Max 5 hours
            numTimeLimit.Value = 60;    // Default 60 minutes

            // Populate Category ComboBox for filtering
            var categories = _questionService.GetAllQuestions().Select(q => q.Category).Distinct().OrderBy(c => c).ToList();
            cmbFilterCategory.Items.Add("All Categories"); // Option to show all
            cmbFilterCategory.Items.AddRange(categories.ToArray());
            cmbFilterCategory.SelectedIndex = 0;

            // Populate Difficulty ComboBox for filtering
            
            // cmbFilterDifficulty.DataSource = Enum.GetValues(typeof(DifficultyLevel));
            // cmbFilterDifficulty.Items.Insert(0, "All Difficulties"); // Add "All" option at index 0
            // cmbFilterDifficulty.SelectedIndex = 0; // Select "All Difficulties" by default
            
            var difficultyOptions = new List<object> { "All Difficulties" };
            difficultyOptions.AddRange(Enum.GetValues(typeof(DifficultyLevel)).Cast<object>());

            cmbFilterDifficulty.DataSource = difficultyOptions;
            cmbFilterDifficulty.SelectedIndex = 0;
        }

        /// <summary>
        /// Loads all available questions and populates the filter options.
        /// </summary>
        private void LoadAvailableQuestions()
        {
            try
            {
                _availableQuestions = _questionService.GetAllQuestions();
                FilterAndDisplayQuestions(); // Display based on current filters
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading questions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Filters questions based on selected category and difficulty, then displays them.
        /// </summary>
        private void FilterAndDisplayQuestions()
        {
            lstAvailableQuestions.Items.Clear();
            if (_availableQuestions == null)
                return;
            
            List<Question> filteredQuestions = _availableQuestions;

            string selectedCategory = cmbFilterCategory.SelectedItem?.ToString();
            if (selectedCategory != "All Categories")
            {
                filteredQuestions = filteredQuestions.Where(q => q.Category == selectedCategory).ToList();
            }

            DifficultyLevel? selectedDifficulty = null;
            if (cmbFilterDifficulty.SelectedItem != null && cmbFilterDifficulty.SelectedItem is DifficultyLevel diff)
            {
                selectedDifficulty = diff;
            }

            if (selectedDifficulty.HasValue)
            {
                filteredQuestions = filteredQuestions.Where(q => q.Difficulty == selectedDifficulty.Value).ToList();
            }

            // Add questions to the listbox, excluding those already selected
            foreach (var q in filteredQuestions.Where(q => !_selectedQuestions.Contains(q)))
            {
                lstAvailableQuestions.Items.Add(q); // ToString() of Question will be used
            }
            UpdateQuestionCounts();
        }

        /// <summary>
        /// Updates the displayed counts of available and selected questions.
        /// </summary>
        private void UpdateQuestionCounts()
        {
            lblAvailableQuestionCount.Text = $"Available: {lstAvailableQuestions.Items.Count}";
            lblSelectedQuestionCount.Text = $"Selected: {_selectedQuestions.Count}";
        }


        /// <summary>
        /// Event handler for filter ComboBox changes.
        /// </summary>
        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAndDisplayQuestions();
        }

        /// <summary>
        /// Event handler for adding a question from available to selected list.
        /// </summary>
        private void btnAddQuestion_Click(object sender, EventArgs e)
        {
            if (lstAvailableQuestions.SelectedItem is Question selectedQuestion)
            {
                _selectedQuestions.Add(selectedQuestion);
                lstSelectedQuestions.Items.Add(selectedQuestion);
                lstAvailableQuestions.Items.Remove(selectedQuestion); // Remove from available
                UpdateQuestionCounts();
            }
            else
            {
                ShowErrorMessage("Please select a question from the available list.");
            }
        }

        /// <summary>
        /// Event handler for removing a question from selected to available list.
        /// </summary>
        private void btnRemoveQuestion_Click(object sender, EventArgs e)
        {
            if (lstSelectedQuestions.SelectedItem is Question selectedQuestion)
            {
                _selectedQuestions.Remove(selectedQuestion);
                lstSelectedQuestions.Items.Remove(selectedQuestion);
                // Add back to available if it matches current filters
                if (MatchesCurrentFilters(selectedQuestion))
                {
                    lstAvailableQuestions.Items.Add(selectedQuestion);
                }
                UpdateQuestionCounts();
            }
            else
            {
                ShowErrorMessage("Please select a question from the selected list to remove.");
            }
        }

        /// <summary>
        /// Checks if a question matches the currently applied filters.
        /// </summary>
        private bool MatchesCurrentFilters(Question question)
        {
            string selectedCategory = cmbFilterCategory.SelectedItem?.ToString();
            DifficultyLevel? selectedDifficulty = null;
            if (cmbFilterDifficulty.SelectedItem != null && cmbFilterDifficulty.SelectedItem is DifficultyLevel diff)
            {
                selectedDifficulty = diff;
            }

            bool categoryMatch = (selectedCategory == "All Categories" || question.Category == selectedCategory);
            bool difficultyMatch = (!selectedDifficulty.HasValue || question.Difficulty == selectedDifficulty.Value);

            return categoryMatch && difficultyMatch;
        }

        /// <summary>
        /// Event handler for the Create Exam button click.
        /// Validates input and creates the new exam.
        /// </summary>
        private void btnCreateExam_Click(object sender, EventArgs e)
        {
            string examName = txtExamName.Text.Trim();
            string description = txtDescription.Text.Trim();
            int timeLimit = (int)numTimeLimit.Value;
            int desiredQuestionCount = (int)numQuestionCount.Value;
            string createdBy = UserSession.Instance.GetCurrentUsername();

            // Validate exam name and description
            ValidationResult nameValidation = ValidationService.ValidateTextField(examName, "Exam Name", 3, 100);
            if (!nameValidation.IsValid) { ShowErrorMessage(nameValidation.Message); return; }

            ValidationResult descValidation = ValidationService.ValidateTextField(description, "Description", 0, 500); // Description can be empty
            if (!descValidation.IsValid) { ShowErrorMessage(descValidation.Message); return; }


            // Validate time limit
            ValidationResult timeLimitValidation = ValidationService.ValidateTimeLimit(timeLimit);
            if (!timeLimitValidation.IsValid) { ShowErrorMessage(timeLimitValidation.Message); return; }

            // Check if enough questions are selected
            if (_selectedQuestions.Count == 0)
            {
                ShowErrorMessage("Please select at least one question for the exam.");
                return;
            }
            
            // Check if the number of selected questions matches the desired count
            if (_selectedQuestions.Count != desiredQuestionCount)
            {
                ShowErrorMessage($"The number of selected questions ({_selectedQuestions.Count}) must match the desired question count ({desiredQuestionCount}).");
                return;
            }


            try
            {
                // Extract question IDs from selected questions
                List<int> questionIDs = _selectedQuestions.Select(q => q.ID).ToList();

                // Create the Exam object
                Exam newExam = new Exam(examName, questionIDs, createdBy, timeLimit, description);

                // Save the exam
                bool success = _examService.CreateExam(newExam);

                if (success)
                {
                    MessageBox.Show("Exam created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK; // Indicate success to parent form
                    this.Close();
                }
                else
                {
                    ShowErrorMessage("Failed to create exam. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"An error occurred during exam creation: {ex.Message}");
                Console.WriteLine($"Exam Creation Error: {ex.Message}");
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

        // --- Designer-generated code (typically in ExamCreationForm.Designer.cs) ---
        // You will need to ensure these controls are added to your form in Visual Studio Designer.

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblExamName;
        private System.Windows.Forms.TextBox txtExamName;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.RichTextBox txtDescription;
        private System.Windows.Forms.Label lblQuestionCount;
        private System.Windows.Forms.NumericUpDown numQuestionCount;
        private System.Windows.Forms.Label lblTimeLimit;
        private System.Windows.Forms.NumericUpDown numTimeLimit;
        private System.Windows.Forms.Label lblFilterCategory;
        private System.Windows.Forms.ComboBox cmbFilterCategory;
        private System.Windows.Forms.Label lblFilterDifficulty;
        private System.Windows.Forms.ComboBox cmbFilterDifficulty;
        private System.Windows.Forms.ListBox lstAvailableQuestions;
        private System.Windows.Forms.Label lblAvailableQuestions;
        private System.Windows.Forms.Button btnAddQuestion;
        private System.Windows.Forms.Button btnRemoveQuestion;
        private System.Windows.Forms.ListBox lstSelectedQuestions;
        private System.Windows.Forms.Label lblSelectedQuestions;
        private System.Windows.Forms.Button btnCreateExam;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblErrorMessage;
        private System.Windows.Forms.Label lblAvailableQuestionCount;
        private System.Windows.Forms.Label lblSelectedQuestionCount;


        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblExamName = new System.Windows.Forms.Label();
            this.txtExamName = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.RichTextBox();
            this.lblQuestionCount = new System.Windows.Forms.Label();
            this.numQuestionCount = new System.Windows.Forms.NumericUpDown();
            this.lblTimeLimit = new System.Windows.Forms.Label();
            this.numTimeLimit = new System.Windows.Forms.NumericUpDown();
            this.lblFilterCategory = new System.Windows.Forms.Label();
            this.cmbFilterCategory = new System.Windows.Forms.ComboBox();
            this.lblFilterDifficulty = new System.Windows.Forms.Label();
            this.cmbFilterDifficulty = new System.Windows.Forms.ComboBox();
            this.lstAvailableQuestions = new System.Windows.Forms.ListBox();
            this.lblAvailableQuestions = new System.Windows.Forms.Label();
            this.btnAddQuestion = new System.Windows.Forms.Button();
            this.btnRemoveQuestion = new System.Windows.Forms.Button();
            this.lstSelectedQuestions = new System.Windows.Forms.ListBox();
            this.lblSelectedQuestions = new System.Windows.Forms.Label();
            this.btnCreateExam = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.lblAvailableQuestionCount = new System.Windows.Forms.Label();
            this.lblSelectedQuestionCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numQuestionCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(189, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Create New Exam";
            // 
            // lblExamName
            // 
            this.lblExamName.AutoSize = true;
            this.lblExamName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblExamName.Location = new System.Drawing.Point(20, 70);
            this.lblExamName.Name = "lblExamName";
            this.lblExamName.Size = new System.Drawing.Size(86, 19);
            this.lblExamName.TabIndex = 1;
            this.lblExamName.Text = "Exam Name:";
            // 
            // txtExamName
            // 
            this.txtExamName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExamName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtExamName.Location = new System.Drawing.Point(150, 67);
            this.txtExamName.Name = "txtExamName";
            this.txtExamName.Size = new System.Drawing.Size(450, 25);
            this.txtExamName.TabIndex = 2;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblDescription.Location = new System.Drawing.Point(20, 105);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(82, 19);
            this.lblDescription.TabIndex = 3;
            this.lblDescription.Text = "Description:";
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtDescription.Location = new System.Drawing.Point(150, 102);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(450, 60);
            this.txtDescription.TabIndex = 4;
            this.txtDescription.Text = "";
            // 
            // lblQuestionCount
            // 
            this.lblQuestionCount.AutoSize = true;
            this.lblQuestionCount.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblQuestionCount.Location = new System.Drawing.Point(20, 175);
            this.lblQuestionCount.Name = "lblQuestionCount";
            this.lblQuestionCount.Size = new System.Drawing.Size(110, 19);
            this.lblQuestionCount.TabIndex = 5;
            this.lblQuestionCount.Text = "Question Count:";
            // 
            // numQuestionCount
            // 
            this.numQuestionCount.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numQuestionCount.Location = new System.Drawing.Point(150, 172);
            this.numQuestionCount.Name = "numQuestionCount";
            this.numQuestionCount.Size = new System.Drawing.Size(80, 25);
            this.numQuestionCount.TabIndex = 6;
            // 
            // lblTimeLimit
            // 
            this.lblTimeLimit.AutoSize = true;
            this.lblTimeLimit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTimeLimit.Location = new System.Drawing.Point(250, 175);
            this.lblTimeLimit.Name = "lblTimeLimit";
            this.lblTimeLimit.Size = new System.Drawing.Size(109, 19);
            this.lblTimeLimit.TabIndex = 7;
            this.lblTimeLimit.Text = "Time Limit (min):";
            // 
            // numTimeLimit
            // 
            this.numTimeLimit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.numTimeLimit.Location = new System.Drawing.Point(365, 172);
            this.numTimeLimit.Name = "numTimeLimit";
            this.numTimeLimit.Size = new System.Drawing.Size(80, 25);
            this.numTimeLimit.TabIndex = 8;
            // 
            // lblFilterCategory
            // 
            this.lblFilterCategory.AutoSize = true;
            this.lblFilterCategory.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblFilterCategory.Location = new System.Drawing.Point(20, 210);
            this.lblFilterCategory.Name = "lblFilterCategory";
            this.lblFilterCategory.Size = new System.Drawing.Size(102, 19);
            this.lblFilterCategory.TabIndex = 9;
            this.lblFilterCategory.Text = "Filter Category:";
            // 
            // cmbFilterCategory
            // 
            this.cmbFilterCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterCategory.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbFilterCategory.FormattingEnabled = true;
            this.cmbFilterCategory.Location = new System.Drawing.Point(150, 207);
            this.cmbFilterCategory.Name = "cmbFilterCategory";
            this.cmbFilterCategory.Size = new System.Drawing.Size(150, 25);
            this.cmbFilterCategory.TabIndex = 10;
            this.cmbFilterCategory.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_SelectedIndexChanged);
            // 
            // lblFilterDifficulty
            // 
            this.lblFilterDifficulty.AutoSize = true;
            this.lblFilterDifficulty.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblFilterDifficulty.Location = new System.Drawing.Point(320, 210);
            this.lblFilterDifficulty.Name = "lblFilterDifficulty";
            this.lblFilterDifficulty.Size = new System.Drawing.Size(103, 19);
            this.lblFilterDifficulty.TabIndex = 11;
            this.lblFilterDifficulty.Text = "Filter Difficulty:";
            // 
            // cmbFilterDifficulty
            // 
            this.cmbFilterDifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFilterDifficulty.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbFilterDifficulty.FormattingEnabled = true;
            this.cmbFilterDifficulty.Location = new System.Drawing.Point(430, 207);
            this.cmbFilterDifficulty.Name = "cmbFilterDifficulty";
            this.cmbFilterDifficulty.Size = new System.Drawing.Size(170, 25);
            this.cmbFilterDifficulty.TabIndex = 12;
            this.cmbFilterDifficulty.SelectedIndexChanged += new System.EventHandler(this.cmbFilter_SelectedIndexChanged);
            // 
            // lstAvailableQuestions
            // 
            this.lstAvailableQuestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstAvailableQuestions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstAvailableQuestions.FormattingEnabled = true;
            this.lstAvailableQuestions.ItemHeight = 15;
            this.lstAvailableQuestions.Location = new System.Drawing.Point(20, 265);
            this.lstAvailableQuestions.Name = "lstAvailableQuestions";
            this.lstAvailableQuestions.Size = new System.Drawing.Size(270, 214);
            this.lstAvailableQuestions.TabIndex = 14;
            // 
            // lblAvailableQuestions
            // 
            this.lblAvailableQuestions.AutoSize = true;
            this.lblAvailableQuestions.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblAvailableQuestions.Location = new System.Drawing.Point(20, 243);
            this.lblAvailableQuestions.Name = "lblAvailableQuestions";
            this.lblAvailableQuestions.Size = new System.Drawing.Size(137, 19);
            this.lblAvailableQuestions.TabIndex = 13;
            this.lblAvailableQuestions.Text = "Available Questions:";
            // 
            // btnAddQuestion
            // 
            this.btnAddQuestion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnAddQuestion.Location = new System.Drawing.Point(300, 310);
            this.btnAddQuestion.Name = "btnAddQuestion";
            this.btnAddQuestion.Size = new System.Drawing.Size(30, 30);
            this.btnAddQuestion.TabIndex = 15;
            this.btnAddQuestion.Text = ">";
            this.btnAddQuestion.UseVisualStyleBackColor = true;
            this.btnAddQuestion.Click += new System.EventHandler(this.btnAddQuestion_Click);
            // 
            // btnRemoveQuestion
            // 
            this.btnRemoveQuestion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnRemoveQuestion.Location = new System.Drawing.Point(300, 345);
            this.btnRemoveQuestion.Name = "btnRemoveQuestion";
            this.btnRemoveQuestion.Size = new System.Drawing.Size(30, 30);
            this.btnRemoveQuestion.TabIndex = 16;
            this.btnRemoveQuestion.Text = "<";
            this.btnRemoveQuestion.UseVisualStyleBackColor = true;
            this.btnRemoveQuestion.Click += new System.EventHandler(this.btnRemoveQuestion_Click);
            // 
            // lstSelectedQuestions
            // 
            this.lstSelectedQuestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSelectedQuestions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstSelectedQuestions.FormattingEnabled = true;
            this.lstSelectedQuestions.ItemHeight = 15;
            this.lstSelectedQuestions.Location = new System.Drawing.Point(339, 265);
            this.lstSelectedQuestions.Name = "lstSelectedQuestions";
            this.lstSelectedQuestions.Size = new System.Drawing.Size(260, 214);
            this.lstSelectedQuestions.TabIndex = 18;
            // 
            // lblSelectedQuestions
            // 
            this.lblSelectedQuestions.AutoSize = true;
            this.lblSelectedQuestions.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSelectedQuestions.Location = new System.Drawing.Point(339, 243);
            this.lblSelectedQuestions.Name = "lblSelectedQuestions";
            this.lblSelectedQuestions.Size = new System.Drawing.Size(129, 19);
            this.lblSelectedQuestions.TabIndex = 17;
            this.lblSelectedQuestions.Text = "Selected Questions:";
            // 
            // btnCreateExam
            // 
            this.btnCreateExam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateExam.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCreateExam.Location = new System.Drawing.Point(400, 500);
            this.btnCreateExam.Name = "btnCreateExam";
            this.btnCreateExam.Size = new System.Drawing.Size(90, 35);
            this.btnCreateExam.TabIndex = 19;
            this.btnCreateExam.Text = "Create Exam";
            this.btnCreateExam.UseVisualStyleBackColor = true;
            this.btnCreateExam.Click += new System.EventHandler(this.btnCreateExam_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnCancel.Location = new System.Drawing.Point(510, 500);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 20;
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
            this.lblErrorMessage.Location = new System.Drawing.Point(20, 480);
            this.lblErrorMessage.MaximumSize = new System.Drawing.Size(580, 0);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(76, 15);
            this.lblErrorMessage.TabIndex = 21;
            this.lblErrorMessage.Text = "Error Message";
            this.lblErrorMessage.Visible = false;
            // 
            // lblAvailableQuestionCount
            // 
            this.lblAvailableQuestionCount.AutoSize = true;
            this.lblAvailableQuestionCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblAvailableQuestionCount.Location = new System.Drawing.Point(160, 243);
            this.lblAvailableQuestionCount.Name = "lblAvailableQuestionCount";
            this.lblAvailableQuestionCount.Size = new System.Drawing.Size(64, 15);
            this.lblAvailableQuestionCount.TabIndex = 22;
            this.lblAvailableQuestionCount.Text = "Available: 0";
            // 
            // lblSelectedQuestionCount
            // 
            this.lblSelectedQuestionCount.AutoSize = true;
            this.lblSelectedQuestionCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSelectedQuestionCount.Location = new System.Drawing.Point(470, 243);
            this.lblSelectedQuestionCount.Name = "lblSelectedQuestionCount";
            this.lblSelectedQuestionCount.Size = new System.Drawing.Size(62, 15);
            this.lblSelectedQuestionCount.TabIndex = 23;
            this.lblSelectedQuestionCount.Text = "Selected: 0";
            // 
            // ExamCreationForm
            // 
            this.ClientSize = new System.Drawing.Size(620, 550);
            this.Controls.Add(this.lblSelectedQuestionCount);
            this.Controls.Add(this.lblAvailableQuestionCount);
            this.Controls.Add(this.lblErrorMessage);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreateExam);
            this.Controls.Add(this.lblSelectedQuestions);
            this.Controls.Add(this.lstSelectedQuestions);
            this.Controls.Add(this.btnRemoveQuestion);
            this.Controls.Add(this.btnAddQuestion);
            this.Controls.Add(this.lblAvailableQuestions);
            this.Controls.Add(this.lstAvailableQuestions);
            this.Controls.Add(this.cmbFilterDifficulty);
            this.Controls.Add(this.lblFilterDifficulty);
            this.Controls.Add(this.cmbFilterCategory);
            this.Controls.Add(this.lblFilterCategory);
            this.Controls.Add(this.numTimeLimit);
            this.Controls.Add(this.lblTimeLimit);
            this.Controls.Add(this.numQuestionCount);
            this.Controls.Add(this.lblQuestionCount);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtExamName);
            this.Controls.Add(this.lblExamName);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new System.Drawing.Size(636, 589);
            this.Name = "ExamCreationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Exam Creation";
            this.Load += new System.EventHandler(this.ExamCreationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numQuestionCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void ExamCreationForm_Load(object sender, EventArgs e)
        {
            // Any specific logic on form load can go here.
        }
    }
}