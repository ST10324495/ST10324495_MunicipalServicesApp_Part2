using System;
using System.Drawing;
using System.Windows.Forms;

namespace ST10323395_MunicipalServicesApp
{
    public class LocalEventsForm : Form
    {
        // Application color scheme (matching existing forms)
        private readonly Color Primary = Color.FromArgb(33, 150, 243);
        private readonly Color PrimaryDark = Color.FromArgb(25, 118, 210);
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

        // Placeholder content
        private Panel contentPanel;
        private Label lblPlaceholder;

        // Action buttons
        private FlowLayoutPanel buttonRow;
        private Button btnBackToMenu;

        public LocalEventsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Configure form properties
            Text = "Local Events and Announcements - Municipal Services";
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
                Text = "Local Events and Announcements",
                Dock = DockStyle.Top,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = PrimaryDark,
                Margin = new Padding(0, 0, 0, 14)
            };
            stack.Controls.Add(lblTitle);

            // Create content panel for placeholder message
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(24)
            };
            contentPanel.Paint += ContentPanel_Paint;
            stack.Controls.Add(contentPanel);

            // Create placeholder message
            lblPlaceholder = new Label
            {
                Text = "This section will be implemented in a future update.",
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                ForeColor = Muted,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            contentPanel.Controls.Add(lblPlaceholder);

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

        // Draw card border
        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var r = card.ClientRectangle;
            r.Width -= 1; r.Height -= 1;
            using (var pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, r);
        }

        // Draw content panel border
        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {
            var r = contentPanel.ClientRectangle;
            r.Width -= 1; r.Height -= 1;
            using (var pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, r);
        }
    }
}
