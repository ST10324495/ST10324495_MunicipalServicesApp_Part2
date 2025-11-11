using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp
{
    public class ServiceStatusForm : Form
    {
        // Application color scheme (matching existing forms)
        private readonly Color Primary = Color.FromArgb(33, 150, 243);
        private readonly Color PrimaryDark = Color.FromArgb(25, 118, 210);
        private readonly Color AccentGreen = Color.FromArgb(46, 204, 113);
        private readonly Color AccentOrange = Color.FromArgb(230, 126, 34);
        private readonly Color AccentRed = Color.FromArgb(231, 76, 60);
        private readonly Color TextDark = Color.FromArgb(52, 73, 94);
        private readonly Color PageBg = Color.FromArgb(236, 240, 241);
        private readonly Color CardBorder = Color.FromArgb(230, 234, 238);

        // Form layout components
        private TableLayoutPanel root;
        private Panel card;
        private TableLayoutPanel stack;

        // Form title
        private Label lblTitle;

        // DataGridView for displaying issues with status
        private DataGridView dgvStatus;

        // Action buttons
        private FlowLayoutPanel buttonRow;
        private Button btnRefreshStatus;
        private Button btnBackToMenu;

        // Random number generator for status simulation
        private Random random = new Random();

        // Status options
        private readonly string[] statusOptions = { "Submitted", "In Progress", "Resolved", "Under Review" };

        public ServiceStatusForm()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadIssuesWithStatus();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Configure form properties
            Text = "Service Request Status - Municipal Services";
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
                Text = "Service Request Status",
                Dock = DockStyle.Top,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = PrimaryDark,
                Margin = new Padding(0, 0, 0, 14)
            };
            stack.Controls.Add(lblTitle);

            // Create DataGridView
            dgvStatus = new DataGridView
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
            stack.Controls.Add(dgvStatus);

            // Action buttons
            buttonRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 48,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0, 8, 0, 0)
            };

            btnRefreshStatus = MakeButton("Update Status", AccentOrange);
            btnRefreshStatus.Click += BtnRefreshStatus_Click;

            btnBackToMenu = MakeButton("Back to Menu", Color.FromArgb(149, 165, 166));
            btnBackToMenu.Click += delegate { Close(); };

            buttonRow.Controls.Add(btnBackToMenu);
            buttonRow.Controls.Add(btnRefreshStatus);
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
            dgvStatus.DefaultCellStyle.BackColor = Color.White;
            dgvStatus.DefaultCellStyle.ForeColor = TextDark;
            dgvStatus.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            dgvStatus.DefaultCellStyle.SelectionForeColor = TextDark;
            dgvStatus.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvStatus.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            // Style column headers
            dgvStatus.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            dgvStatus.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStatus.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvStatus.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvStatus.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 8, 8, 8);
            dgvStatus.ColumnHeadersHeight = 40;
            dgvStatus.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Style alternating rows
            dgvStatus.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            // Add columns
            dgvStatus.Columns.Add("Location", "Location");
            dgvStatus.Columns.Add("Category", "Category");
            dgvStatus.Columns.Add("Description", "Description");
            dgvStatus.Columns.Add("DateSubmitted", "Date Submitted");
            dgvStatus.Columns.Add("Status", "Status");

            // Set column properties for responsive behavior
            dgvStatus.Columns["Location"].FillWeight = 20;
            dgvStatus.Columns["Category"].FillWeight = 15;
            dgvStatus.Columns["Description"].FillWeight = 35;
            dgvStatus.Columns["DateSubmitted"].FillWeight = 15;
            dgvStatus.Columns["Status"].FillWeight = 15;

            // Format date column
            dgvStatus.Columns["DateSubmitted"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";

            // Handle status column formatting
            dgvStatus.CellFormatting += DgvStatus_CellFormatting;
        }

        private void DgvStatus_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Format status column with appropriate colors
            if (dgvStatus.Columns[e.ColumnIndex].Name == "Status")
            {
                string status = e.Value?.ToString();
                switch (status)
                {
                    case "Submitted":
                        e.CellStyle.ForeColor = AccentOrange;
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    case "In Progress":
                        e.CellStyle.ForeColor = Primary;
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    case "Resolved":
                        e.CellStyle.ForeColor = AccentGreen;
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    case "Under Review":
                        e.CellStyle.ForeColor = AccentRed;
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                }
            }
        }

        private void LoadIssuesWithStatus()
        {
            // Clear existing data
            dgvStatus.Rows.Clear();

            // Load issues from repository with random statuses
            foreach (var issue in IssueRepository.Items.ToArray())
            {
                string randomStatus = statusOptions[random.Next(statusOptions.Length)];
                dgvStatus.Rows.Add(
                    issue.Location,
                    issue.Category,
                    issue.Description,
                    issue.DateSubmitted,
                    randomStatus
                );
            }

            // Update title to show count and provide feedback
            int issueCount = IssueRepository.Items.Count;
            if (issueCount == 0)
            {
                lblTitle.Text = "No Service Requests to Track";
                // Show helpful message to user
                MessageBox.Show(
                    "No service requests are available for status tracking.\n\n" +
                    "To create a service request:\n" +
                    "1. Click 'Report Issues' in the main menu\n" +
                    "2. Submit a new issue report\n" +
                    "3. Return here to track its status\n\n" +
                    "Service request statuses will be simulated for demonstration purposes.",
                    "No Service Requests Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblTitle.Text = $"Service Request Status ({issueCount} requests)";
            }
        }

        private void BtnRefreshStatus_Click(object sender, EventArgs e)
        {
            // Simulate status updates by randomly changing statuses
            foreach (DataGridViewRow row in dgvStatus.Rows)
            {
                if (row.Cells["Status"].Value != null)
                {
                    string newStatus = statusOptions[random.Next(statusOptions.Length)];
                    row.Cells["Status"].Value = newStatus;
                }
            }

            // Show enhanced feedback to user
            int totalIssues = dgvStatus.Rows.Count;
            MessageBox.Show(
                "Status Update Complete!\n\n" +
                $"Updated {totalIssues} service request(s)\n" +
                "Status changes have been applied\n" +
                "Check the color-coded status indicators below\n\n" +
                "Status Legend:\n" +
                "Submitted - New requests awaiting review\n" +
                "In Progress - Currently being addressed\n" +
                "Resolved - Successfully completed\n" +
                "Under Review - Being evaluated by staff",
                "Service Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
