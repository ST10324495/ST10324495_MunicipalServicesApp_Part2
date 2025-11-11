using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace ST10323395_MunicipalServicesApp
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class MainMenuForm : Form
    {
        // UI Layout components
        private Panel topBar;
        private Panel leftNav;
        private Panel contentHost;


        // Page title display
        private Label lblTitleChip;

        // Navigation buttons
        private Button btnReportIssues, btnViewIssues, btnLocalEvents, btnServiceStatus, btnExit;

        // Color scheme
        private readonly Color Primary = Color.FromArgb(33, 150, 243);
        private readonly Color PrimaryDark = Color.FromArgb(25, 118, 210);
        private readonly Color PageBg = Color.FromArgb(210, 215, 220);
        private readonly Color NavBg = Color.FromArgb(45, 53, 70);
        private readonly Color NavItem = Color.FromArgb(59, 70, 92);
        private readonly Color NavHover = Color.FromArgb(80, 96, 128);
        private readonly Color NavDown = Color.FromArgb(70, 86, 115);
        private readonly Color NavActive = Color.FromArgb(77, 182, 172);
        private readonly Color CardBorder = Color.FromArgb(230, 234, 238);

        public MainMenuForm()
        {
            InitializeComponent();

            // Maintain title chip position during resize
            leftNav.SizeChanged += (s, e) => RealignTitleChip();
            topBar.SizeChanged += (s, e) => RealignTitleChip();
            RealignTitleChip();

            lblTitleChip.Text = string.Empty;
        }

        private void InitializeComponent()
        {
            // Configure form scaling and appearance
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96f, 96f);

            // Set form properties
            Text = "Municipal Services App";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1180, 760);
            MinimumSize = new Size(1000, 680);
            FormBorderStyle = FormBorderStyle.Sizable;
            BackColor = PageBg;
            DoubleBuffered = true;

            // Create top navigation bar
            topBar = new Panel { Dock = DockStyle.Top, Height = 64, MinimumSize = new Size(0, 64) };
            topBar.Paint += TopBar_Paint;
            Controls.Add(topBar);

            // Create page title display
            lblTitleChip = new Label
            {
                AutoSize = true,
                Text = "REPORT ISSUES",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11.5f, FontStyle.Bold),
                Padding = new Padding(12, 6, 12, 6),
                BackColor = Color.Transparent,
                Location = new Point(24, 16)
            };
            lblTitleChip.Paint += TitleChip_Paint;
            topBar.Controls.Add(lblTitleChip);

            // Create left navigation panel
            leftNav = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = NavBg, MinimumSize = new Size(200, 0) };
            Controls.Add(leftNav);

            // Create navigation button container
            var navStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(12, 18, 12, 12),
            };
            leftNav.Controls.Add(navStack);

            // Create navigation buttons
            btnReportIssues = MakeNavButton("  Report Issues");
            btnViewIssues = MakeNavButton("  View Reported Issues");
            btnServiceStatus = MakeNavButton("  Service Request Status");
            btnLocalEvents = MakeNavButton("  Local Events and Announcements");
            btnExit = MakeNavButton("  Exit Application");

            // Set up button click events
            btnReportIssues.Click += (s, e) =>
            {
                ActivateNav(btnReportIssues, "Report Issues");
                OpenReportIssuePage();
            };
            btnViewIssues.Click += (s, e) =>
            {
                ActivateNav(btnViewIssues, "View Issues");
                OpenViewIssuesPage();
            };
            btnServiceStatus.Click += (s, e) =>
            {
                ActivateNav(btnServiceStatus, "Service Status");
                OpenServiceStatusPage();
            };
            btnLocalEvents.Click += (s, e) =>
            {
                ActivateNav(btnLocalEvents, "Local Events");
                OpenLocalEventsPage();
            };
            btnExit.Click += (s, e) =>
            {
                var result = MessageBox.Show("Are you sure you want to exit the application?", "Exit Application",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };

            // Add buttons to navigation
            navStack.Controls.Add(btnReportIssues);
            navStack.Controls.Add(btnViewIssues);
            navStack.Controls.Add(btnServiceStatus);
            navStack.Controls.Add(btnLocalEvents);
            navStack.Controls.Add(btnExit);

            // Create main content area
            contentHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            contentHost.Paint += ContentHost_PaintBorder;
            Controls.Add(contentHost);
            contentHost.BringToFront();
        }

        // Button creation methods

        private Button MakeNavButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Width = 196,
                Height = 44,
                Margin = new Padding(0, 6, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.White,
                BackColor = NavItem,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = NavHover;
            b.FlatAppearance.MouseDownBackColor = NavDown;
            return b;
        }

        // Navigation and page management
        private void ActivateNav(Button target, string pageTitle)
        {
            // Reset all navigation buttons to default color
            foreach (Control c in leftNav.Controls)
            {
                if (c is FlowLayoutPanel fp)
                {
                    foreach (Control cc in fp.Controls)
                        if (cc is Button b) b.BackColor = NavItem;
                }
            }
            // Highlight active button
            if (target.Enabled) target.BackColor = NavActive;

            // Update page title
            lblTitleChip.Text = pageTitle.ToUpperInvariant();
            lblTitleChip.Invalidate();
            RealignTitleChip();
        }

        // Display placeholder message for disabled features
        private void ShowPlaceholder(string message)
        {
            contentHost.Controls.Clear();

            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(24) };
            panel.Paint += ContentCardBorder;
            var lbl = new Label
            {
                Text = message,
                AutoSize = true,
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.FromArgb(70, 80, 95),
                Left = 24,
                Top = 24
            };
            panel.Controls.Add(lbl);
            contentHost.Controls.Add(panel);
        }

        // Open the issue reporting form
        private void OpenReportIssuePage()
        {
            contentHost.Controls.Clear();
            var page = new ReportIssueForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            contentHost.Controls.Add(page);
            page.Show();
        }

        // Open the view issues form
        private void OpenViewIssuesPage()
        {
            contentHost.Controls.Clear();
            var page = new ViewIssuesForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            contentHost.Controls.Add(page);
            page.Show();
        }

        // Open the service status form
        private void OpenServiceStatusPage()
        {
            contentHost.Controls.Clear();
            var page = new FrmServiceRequestStatus
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            contentHost.Controls.Add(page);
            page.Show();
        }

        // Open the local events form
        private void OpenLocalEventsPage()
        {
            contentHost.Controls.Clear();
            var page = new FrmLocalEvents
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            contentHost.Controls.Add(page);
            page.Show();
        }

        // Position title chip based on navigation width
        private void RealignTitleChip()
        {
            int navWidth = leftNav?.Width ?? 230;
            lblTitleChip.Left = navWidth + 24;
            lblTitleChip.Top = Math.Max(0, (topBar.Height - lblTitleChip.Height) / 2);
        }

        // Custom painting methods
        private void TopBar_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle r = topBar.ClientRectangle;

            using (var lg = new LinearGradientBrush(r, Primary, PrimaryDark, 0f))
                g.FillRectangle(lg, r);

            // Add top highlight effect
            var gloss = new Rectangle(r.X, r.Y, r.Width, r.Height / 2 + 1);
            using (var glossBrush = new LinearGradientBrush(gloss,
                Color.FromArgb(80, Color.White), Color.FromArgb(0, Color.White),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(glossBrush, gloss);
            }

            // Add bottom shadow
            var shadow = new Rectangle(r.X, r.Bottom - 7, r.Width, 7);
            using (var sh = new LinearGradientBrush(shadow,
                Color.FromArgb(120, 0, 0, 0), Color.FromArgb(0, 0, 0, 0),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(sh, shadow);
            }

            // Add bottom highlight line
            using (var pen = new Pen(Color.FromArgb(40, Color.White)))
                g.DrawLine(pen, r.X, r.Bottom - 1, r.Right, r.Bottom - 1);
        }

        private void TitleChip_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var lbl = (Label)sender;
            var rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);

            using (var path = Rounded(rect, 10))
            using (var b = new SolidBrush(Color.FromArgb(42, 255, 255, 255)))
            using (var p = new Pen(Color.FromArgb(80, 255, 255, 255)))
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }
        }

        // Draw content area border
        private void ContentHost_PaintBorder(object sender, PaintEventArgs e)
        {
            var r = contentHost.ClientRectangle;
            r.Width -= 1; r.Height -= 1;
            using (var pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, r);
        }

        // Draw panel border
        private void ContentCardBorder(object sender, PaintEventArgs e)
        {
            var pnl = (Panel)sender;
            var r = pnl.ClientRectangle; r.Width -= 1; r.Height -= 1;
            using (var pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, r);
        }


        // Utility methods
        private static GraphicsPath Rounded(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
