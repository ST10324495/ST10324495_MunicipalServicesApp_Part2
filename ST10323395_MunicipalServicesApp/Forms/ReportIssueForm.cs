using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp
{
    // Form for reporting municipal service issues
    public class ReportIssueForm : Form
    {
        // Application color scheme
        private readonly Color Primary = Color.FromArgb(33, 150, 243);
        private readonly Color PrimaryDark = Color.FromArgb(25, 118, 210);
        private readonly Color AccentGreen = Color.FromArgb(46, 204, 113);
        private readonly Color AccentBlue = Color.FromArgb(52, 152, 219);
        private readonly Color TextDark = Color.FromArgb(52, 73, 94);
        private readonly Color Muted = Color.FromArgb(149, 165, 166);
        private readonly Color PageBg = Color.FromArgb(236, 240, 241);
        private readonly Color CardBorder = Color.FromArgb(230, 234, 238);

        // Form layout components
        private TableLayoutPanel root;
        private Panel card;
        private TableLayoutPanel stack;

        // Form title
        private Label lblTitle;

        // Progress tracking components
        private Panel progressTrack;
        private Panel progressFill;
        private Label lblEngagement;

        // Input field components
        private TableLayoutPanel lcGrid;
        private Label lblLocation, lblCategory;
        private TextBox txtLocation;
        private ComboBox cmbCategory;

        // Description input
        private TableLayoutPanel descRow;
        private Label lblDescription;
        private RichTextBox rtbDescription;

        // File attachment controls
        private TableLayoutPanel attachRow, attachBar;
        private Label lblAttachment;
        private TextBox txtAttachment;
        private Button btnAttachMedia;

        // Action buttons
        private FlowLayoutPanel buttonRow;
        private Button btnSubmit, btnBackToMenu;

        private OpenFileDialog openFileDialog;

        // Progress encouragement messages (Gamification)
        private readonly string[] encouragementMessages = {
            "Great start! Keep going!",
            "You're making progress!",
            "Almost there!",
            "Excellent work!",
            "Perfect! You're ready to submit!"
        };

        public ReportIssueForm()
        {
            InitializeComponent();
            SetupForm();
            UpdateEngagement();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Configure form properties
            Text = "Report Issue - Municipal Services";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = PageBg;
            Font = new Font("Segoe UI", 9F);
            Dock = DockStyle.Fill;
            FormBorderStyle = FormBorderStyle.None;

            // Create responsive layout container
            root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                Padding = new Padding(24)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);

            // Create main content card
            card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(28),
                AutoScroll = false
            };
            card.Paint += Card_Paint;
            EnableDoubleBuffer(card);
            root.Controls.Add(card, 0, 0);

            // Create vertical content layout
            stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1
            };
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Title
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Progress
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Location/Category
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Description (expandable)
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Attachment
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Buttons
            card.Controls.Add(stack);

            // Create form title
            lblTitle = new Label
            {
                Text = "Report an Issue",
                Dock = DockStyle.Top,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = PrimaryDark,
                Margin = new Padding(0, 0, 0, 14)
            };
            stack.Controls.Add(lblTitle);

            // Create progress tracking section
            var progressBlock = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 18)
            };

            // Create progress bar
            progressTrack = new Panel
            {
                Height = 16,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(230, 230, 230),
                Margin = new Padding(0, 0, 0, 8)
            };
            progressFill = new Panel
            {
                Height = 16,
                Width = 0,
                BackColor = AccentGreen
            };
            progressTrack.Controls.Add(progressFill);

            // Create progress message label
            lblEngagement = new Label
            {
                Text = "Complete the form to see your progress!",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Muted,
                Height = 18
            };

            progressBlock.Controls.Add(progressTrack, 0, 0);
            progressBlock.Controls.Add(lblEngagement, 0, 1);
            stack.Controls.Add(progressBlock);

            // Create location and category input section
            lcGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            lcGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            lcGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            lcGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            lcGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

            lblLocation = MakeLabel("Location:");
            txtLocation = new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 6)
            };
            txtLocation.TextChanged += delegate { UpdateEngagement(); };

            lblCategory = MakeLabel("Category:");
            cmbCategory = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 6)
            };
            cmbCategory.SelectedIndexChanged += delegate { UpdateEngagement(); };

            lcGrid.Controls.Add(lblLocation, 0, 0);
            lcGrid.Controls.Add(txtLocation, 1, 0);
            lcGrid.Controls.Add(lblCategory, 0, 1);
            lcGrid.Controls.Add(cmbCategory, 1, 1);
            stack.Controls.Add(lcGrid);

            // Create description input section
            descRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Margin = new Padding(0, 0, 0, 8)
            };
            descRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            descRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            lblDescription = MakeLabel("Description:");
            rtbDescription = new RichTextBox
            {
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                MinimumSize = new Size(0, 100)
            };
            rtbDescription.TextChanged += delegate { UpdateEngagement(); };

            descRow.Controls.Add(lblDescription, 0, 0);
            descRow.Controls.Add(rtbDescription, 1, 0);
            stack.Controls.Add(descRow);

            // File attachment controls
            attachRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            attachRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            attachRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            lblAttachment = MakeLabel("Media Attachment:");

            // File selection bar with textbox and button
            attachBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            attachBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            attachBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));

            txtAttachment = new TextBox
            {
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };
            btnAttachMedia = MakeButton("Attach File", AccentBlue);
            btnAttachMedia.Dock = DockStyle.Fill;
            btnAttachMedia.Click += BtnAttachMedia_Click;

            attachBar.Controls.Add(txtAttachment, 0, 0);
            attachBar.Controls.Add(btnAttachMedia, 1, 0);
            attachRow.Controls.Add(lblAttachment, 0, 0);
            attachRow.Controls.Add(attachBar, 1, 0);
            stack.Controls.Add(attachRow);

            // Action buttons
            buttonRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 48,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0, 8, 0, 0)
            };
            btnSubmit = MakeButton("Submit Issue", AccentGreen);
            btnSubmit.Enabled = false;  // Enabled when form is complete
            btnSubmit.Click += BtnSubmit_Click;

            btnBackToMenu = MakeButton("Back to Menu", Color.FromArgb(149, 165, 166));
            btnBackToMenu.Click += delegate { Close(); };

            buttonRow.Controls.Add(btnSubmit);
            buttonRow.Controls.Add(btnBackToMenu);
            stack.Controls.Add(buttonRow);

            // File selection dialog
            openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Documents|*.pdf;*.doc;*.docx;*.txt|All Files|*.*",
                Title = "Select Media File"
            };

            AcceptButton = btnSubmit;
            CancelButton = btnBackToMenu;

            ResumeLayout(false);
        }

        // UI helper methods
        private static void EnableDoubleBuffer(Control c)
        {
            // Enable double buffering for smooth rendering
            var prop = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (prop != null) prop.SetValue(c, true, null);
        }

        private Label MakeLabel(string text)
        {
            // Create consistent form labels
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
        }

        private Button MakeButton(string text, Color color)
        {
            // Create consistent action buttons
            var b = new Button
            {
                Text = text,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Height = 38,
                Width = 180,
                Margin = new Padding(6)
            };
            b.FlatAppearance.BorderSize = 0;
            // Add hover effects
            b.MouseEnter += delegate { if (b.Enabled) b.BackColor = ControlPaint.Light(color); };
            b.MouseLeave += delegate { if (b.Enabled) b.BackColor = color; };
            return b;
        }

        // Initialize form with category options
        private void SetupForm()
        {
            cmbCategory.Items.AddRange(new[]
            {
                "Sanitation",
                "Roads",
                "Potholes",
                "Street Lighting",
                "Water & Sewage",
                "Electricity / Power",
                "Waste Collection",
                "Illegal Dumping",
                "Parks & Recreation",
                "Noise Complaint",
                "Animal Control",
                "Public Safety",
                "Graffiti & Vandalism",
                "Traffic Signals & Signage",
                "Public Transport",
                "Tree / Vegetation",
                "Flooding / Stormwater",
                "Parking",
                "Utilities",
                "Other"
            });
            txtAttachment.Text = string.Empty;
        }


        // Progress tracking system
        private void UpdateEngagement()
        {
            // Count completed required fields
            int completed = 0;
            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) completed++;
            if (cmbCategory.SelectedIndex >= 0) completed++;
            if (!string.IsNullOrWhiteSpace(rtbDescription.Text)) completed++;

            // Calculate and update progress bar
            int percent = Math.Max(0, Math.Min(100, (int)(completed / 3.0 * 100)));
            int trackWidth = Math.Max(0, progressTrack.ClientSize.Width);
            progressFill.Width = (int)(trackWidth * (percent / 100.0));

            // Update encouragement message
            if (completed == 0)
            {
                lblEngagement.Text = "Complete the form to see your progress!";
                lblEngagement.ForeColor = Muted;
            }
            else if (completed < 3)
            {
                lblEngagement.Text = encouragementMessages[completed - 1];
                lblEngagement.ForeColor = AccentGreen;
            }
            else
            {
                lblEngagement.Text = encouragementMessages[encouragementMessages.Length - 1];
                lblEngagement.ForeColor = Color.FromArgb(39, 174, 96);
            }

            // Enable submit button when form is complete
            btnSubmit.Enabled = completed == 3;
        }

        private void BtnAttachMedia_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    
                    // Check file size limit
                    if (fileInfo.Length > 10 * 1024 * 1024)
                    {
                        MessageBox.Show(
                            "❌ File size exceeds the 10MB limit.\n\n" +
                            $"Current file size: {(fileInfo.Length / (1024.0 * 1024.0)):F1} MB\n" +
                            "Please select a smaller file or compress the current file.",
                            "File Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if file exists and is accessible
                    if (!fileInfo.Exists)
                    {
                        MessageBox.Show("❌ The selected file could not be found or accessed.", "File Not Found",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Update UI with selected file
                    txtAttachment.Text = fileInfo.Name; // Show only filename, not full path
                    btnAttachMedia.Text = "Change File";
                    btnAttachMedia.BackColor = AccentGreen;
                    
                    // Show success feedback
                    MessageBox.Show(
                        $"File attached successfully!\n\n" +
                        $"File: {fileInfo.Name}\n" +
                        $"Size: {(fileInfo.Length / (1024.0 * 1024.0)):F1} MB\n" +
                        $"Type: {fileInfo.Extension.ToUpper()}",
                        "File Attached", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ An error occurred while selecting the file:\n\n{ex.Message}",
                    "File Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            // Check all required fields and show helpful error messages
            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("❌ Please enter the location where the issue occurred.", "Location Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLocation.Focus();
                return;
            }

            if (cmbCategory.SelectedIndex < 0)
            {
                MessageBox.Show("❌ Please select a category for the issue.", "Category Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(rtbDescription.Text))
            {
                MessageBox.Show("❌ Please provide a detailed description of the issue.", "Description Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rtbDescription.Focus();
                return;
            }

            if (rtbDescription.Text.Trim().Length < 10)
            {
                MessageBox.Show("❌ Please provide a more detailed description (at least 10 characters).", "Description Too Short",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rtbDescription.Focus();
                return;
            }

            // Create and store issue record
            var issue = new Issue(
                txtLocation.Text.Trim(),
                cmbCategory.SelectedItem.ToString(),
                rtbDescription.Text.Trim(),
                string.IsNullOrWhiteSpace(openFileDialog.FileName) ? string.Empty : openFileDialog.FileName);

            IssueRepository.Items.Add(issue);

            // Show enhanced success message with more details
            string attachmentInfo = string.IsNullOrWhiteSpace(issue.AttachmentPath) ? "None" : "File attached";
            MessageBox.Show(
                "Issue reported successfully!\n\n" +
                "Location: " + issue.Location + "\n" +
                "Category: " + issue.Category + "\n" +
                "Attachment: " + attachmentInfo + "\n" +
                "Date Submitted: " + issue.DateSubmitted.ToString("yyyy-MM-dd HH:mm") + "\n\n" +
                "Your issue has been logged and will be reviewed by municipal services. " +
                "You can track its status using the 'Service Request Status' menu.",
                "Issue Reported Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reset form for next submission
            txtLocation.Clear();
            cmbCategory.SelectedIndex = -1;
            rtbDescription.Clear();
            txtAttachment.Clear();
            btnAttachMedia.Text = "Attach File";
            btnAttachMedia.BackColor = AccentBlue;
            openFileDialog.FileName = string.Empty;
            UpdateEngagement();
        }

        // Draw card border
        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var r = card.ClientRectangle;
            r.Width -= 1; r.Height -= 1;
            using (var pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, r);
        }
    }
}
