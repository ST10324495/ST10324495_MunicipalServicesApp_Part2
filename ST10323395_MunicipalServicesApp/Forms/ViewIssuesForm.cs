using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp
{
    public class ViewIssuesForm : Form
    {
        // Application color scheme (matching existing forms)
        private readonly Color Primary = Color.FromArgb(33, 150, 243);
        private readonly Color PrimaryDark = Color.FromArgb(25, 118, 210);
        private readonly Color TextDark = Color.FromArgb(52, 73, 94);
        private readonly Color PageBg = Color.FromArgb(236, 240, 241);
        private readonly Color CardBorder = Color.FromArgb(230, 234, 238);

        // Form layout components
        private TableLayoutPanel root;
        private Panel card;
        private TableLayoutPanel stack;

        // Form title
        private Label lblTitle;

        // DataGridView for displaying issues
        private DataGridView dgvIssues;

        // Action buttons
        private FlowLayoutPanel buttonRow;
        private Button btnBackToMenu;

        public ViewIssuesForm()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadIssues();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Configure form properties
            Text = "View Issues - Municipal Services";
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
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.Controls.Add(stack);

            // Create form title
            lblTitle = new Label
            {
                Text = "Reported Issues",
                Dock = DockStyle.Top,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = PrimaryDark,
                Margin = new Padding(0, 0, 0, 14)
            };
            stack.Controls.Add(lblTitle);

            // Create DataGridView
            dgvIssues = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 0, 0, 16)
            };
            stack.Controls.Add(dgvIssues);

            // Action buttons
            buttonRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 48,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0, 8, 0, 0)
            };

            btnBackToMenu = MakeButton("Back to Menu", Color.FromArgb(149, 165, 166));
            btnBackToMenu.Click += delegate { Close(); };

            buttonRow.Controls.Add(btnBackToMenu);
            stack.Controls.Add(buttonRow);

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

        private void SetupDataGridView()
        {
            // Configure DataGridView styling to match app theme
            dgvIssues.DefaultCellStyle.BackColor = Color.White;
            dgvIssues.DefaultCellStyle.ForeColor = TextDark;
            dgvIssues.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            dgvIssues.DefaultCellStyle.SelectionForeColor = TextDark;
            dgvIssues.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvIssues.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            // Style column headers
            dgvIssues.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            dgvIssues.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvIssues.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvIssues.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvIssues.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 8, 8, 8);
            dgvIssues.ColumnHeadersHeight = 40;
            dgvIssues.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Style alternating rows
            dgvIssues.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // Add columns
            dgvIssues.Columns.Add("Location", "Location");
            dgvIssues.Columns.Add("Category", "Category");
            dgvIssues.Columns.Add("Description", "Description");
            dgvIssues.Columns.Add("DateSubmitted", "Date Submitted");
            dgvIssues.Columns.Add("AttachmentPath", "Attachment");

            // Set column properties for responsive behavior
            dgvIssues.Columns["Location"].FillWeight = 20;
            dgvIssues.Columns["Category"].FillWeight = 15;
            dgvIssues.Columns["Description"].FillWeight = 40;
            dgvIssues.Columns["DateSubmitted"].FillWeight = 15;
            dgvIssues.Columns["AttachmentPath"].FillWeight = 10;

            // Format date column
            dgvIssues.Columns["DateSubmitted"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";

            // Handle attachment column display
            dgvIssues.CellFormatting += DgvIssues_CellFormatting;
        }

        private void DgvIssues_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Format attachment column to show "Yes" or "No" instead of file path
            if (dgvIssues.Columns[e.ColumnIndex].Name == "AttachmentPath")
            {
                if (e.Value != null && !string.IsNullOrWhiteSpace(e.Value.ToString()))
                {
                    e.Value = "Yes";
                    e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113); // Green for "Yes"
                }
                else
                {
                    e.Value = "No";
                    e.CellStyle.ForeColor = Color.FromArgb(149, 165, 166); // Gray for "No"
                }
            }
        }

        private void LoadIssues()
        {
            // Clear existing data
            dgvIssues.Rows.Clear();

            // Load issues from repository
            foreach (var issue in IssueRepository.Items.ToArray())
            {
                dgvIssues.Rows.Add(
                    issue.Location,
                    issue.Category,
                    issue.Description,
                    issue.DateSubmitted,
                    issue.AttachmentPath
                );
            }

            // Update title to show count and provide feedback
            int issueCount = IssueRepository.Items.Count;
            if (issueCount == 0)
            {
                lblTitle.Text = "No Issues Reported Yet";
                // Show helpful message to user
                MessageBox.Show(
                    "No issues have been reported yet.\n\n" +
                    "To report a new issue:\n" +
                    "1. Click 'Report Issues' in the main menu\n" +
                    "2. Fill out the form with issue details\n" +
                    "3. Submit your report\n\n" +
                    "Your reported issues will appear here for tracking.",
                    "No Issues Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblTitle.Text = $"Reported Issues ({issueCount})";
            }
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
