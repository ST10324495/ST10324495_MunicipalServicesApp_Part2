using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp
{
    // Form for managing local events and announcements
    public class FrmLocalEvents : Form
    {
        #region UI Components

        // Main layout components
        private TableLayoutPanel root;
        private Panel card;
        private TableLayoutPanel mainLayout;

        // Header section
        private Label lblTitle;
        private Panel searchPanel;
        private TextBox txtSearch;
        private Button btnSearch;
        private ComboBox cmbCategoryFilter;
        private Button btnClearFilters;

        // Content sections
        private Panel eventsPanel;
        private Panel recommendationsPanel;
        private Panel recentViewsPanel;

        // Events display
        private DataGridView dgvEvents;

        // Recommendations section
        private ListBox lstRecommendations;

        // Recent views section
        private ListBox lstRecentViews;

        #endregion

        #region Color Scheme

        private readonly Color Primary = Color.FromArgb(33, 150, 243);
        private readonly Color PrimaryDark = Color.FromArgb(25, 118, 210);
        private readonly Color AccentGreen = Color.FromArgb(46, 204, 113);
        private readonly Color AccentOrange = Color.FromArgb(230, 126, 34);
        private readonly Color TextDark = Color.FromArgb(52, 73, 94);
        private readonly Color Muted = Color.FromArgb(149, 165, 166);
        private readonly Color PageBg = Color.FromArgb(236, 240, 241);
        private readonly Color CardBorder = Color.FromArgb(230, 234, 238);

        #endregion

        #region Constructor and Initialization

        public FrmLocalEvents()
        {
            InitializeComponent();
            InitializeDataStructures();
            LoadEvents();
            LoadRecommendations();
            LoadRecentViews();
        }
        private void InitializeComponent()
        {
            SuspendLayout();

            // Set up the main form properties
            Text = "Local Events and Announcements - Municipal Services";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = PageBg;
            Font = new Font("Segoe UI", 9F);
            Dock = DockStyle.Fill;
            FormBorderStyle = FormBorderStyle.None;

            // Main container panel that holds everything
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            Controls.Add(mainPanel);

            // Blue header bar at the top
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Primary
            };
            mainPanel.Controls.Add(headerPanel);

            // Title label in the header
            var lblTitle = new Label
            {
                Text = "LOCAL EVENTS",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(lblTitle);

            // Search bar section
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(searchPanel);

            // Text box for searching events
            txtSearch = new TextBox
            {
                Width = 300,
                Height = 30,
                Location = new Point(10, 25),
                Font = new Font("Segoe UI", 10F)
            };
            searchPanel.Controls.Add(txtSearch);

            // Dropdown to filter by category
            cmbCategoryFilter = new ComboBox
            {
                Width = 150,
                Height = 30,
                Location = new Point(320, 25),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            searchPanel.Controls.Add(cmbCategoryFilter);

            // Search button
            btnSearch = new Button
            {
                Text = "Search",
                Width = 80,
                Height = 30,
                Location = new Point(480, 25),
                BackColor = AccentGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            searchPanel.Controls.Add(btnSearch);

            // Clear filters button
            btnClearFilters = new Button
            {
                Text = "Clear",
                Width = 80,
                Height = 30,
                Location = new Point(570, 25),
                BackColor = Muted,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            searchPanel.Controls.Add(btnClearFilters);

            // Main content area where events and recommendations go
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 150, 0, 0)
            };
            mainPanel.Controls.Add(contentPanel);

            // Left side panel for the events table
            var eventsPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 600,
                BackColor = Color.White,
                Padding = new Padding(10, 10, 10, 10)
            };
            contentPanel.Controls.Add(eventsPanel);

            // Label above the events table
            var lblEvents = new Label
            {
                Text = "Upcoming Events",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = TextDark,
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0, 0, 0, 0)
            };
            eventsPanel.Controls.Add(lblEvents);

            // The main events table
            dgvEvents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 10, 0, 0)
            };
            SetupDataGridView();
            dgvEvents.CellDoubleClick += DgvEvents_CellDoubleClick;
            eventsPanel.Controls.Add(dgvEvents);

            // Right side panel for recommendations and recent views
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            contentPanel.Controls.Add(rightPanel);

            // Layout manager to split the right panel in half
            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rightPanel.Controls.Add(rightLayout);

            // Recommendations section header
            var recLabel = new Label
            {
                Text = "Recommended for You",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Height = 25
            };
            rightLayout.Controls.Add(recLabel, 0, 0);

            // List box showing recommended events
            lstRecommendations = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstRecommendations.DoubleClick += LstRecommendations_DoubleClick;
            rightLayout.Controls.Add(lstRecommendations, 0, 1);

            // Recent views section header
            var recentLabel = new Label
            {
                Text = "Recently Viewed",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                Height = 25
            };
            rightLayout.Controls.Add(recentLabel, 0, 2);

            // List box showing recently viewed events
            lstRecentViews = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            lstRecentViews.DoubleClick += LstRecentViews_DoubleClick;
            rightLayout.Controls.Add(lstRecentViews, 0, 3);

            // Wire up all the event handlers
            btnSearch.Click += BtnSearch_Click;
            btnClearFilters.Click += BtnClearFilters_Click;
            txtSearch.KeyPress += TxtSearch_KeyPress;
            cmbCategoryFilter.SelectedIndexChanged += CmbCategoryFilter_SelectedIndexChanged;

            ResumeLayout(false);
        }

        #endregion

        #region UI Creation Methods
        #endregion

        #region Data Management

        private void InitializeDataStructures()
        {
            // Load sample events and set up the category dropdown
            EventRepository.InitializeSampleData();
            PopulateCategoryFilter();
        }

        private void PopulateCategoryFilter()
        {
            // Clear the dropdown and add "All Categories" option
            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add("All Categories");
            
            // Add all unique categories from the events
            foreach (var category in EventRepository.UniqueCategories.OrderBy(c => c))
            {
                cmbCategoryFilter.Items.Add(category);
            }
            
            cmbCategoryFilter.SelectedIndex = 0;
        }

        private void LoadEvents()
        {
            // Get all events and populate the table
            var events = EventRepository.GetAllEvents();
            dgvEvents.Rows.Clear();

            foreach (var eventItem in events)
            {
                dgvEvents.Rows.Add(
                    eventItem.Title,
                    eventItem.Category,
                    eventItem.GetFormattedDate(),
                    eventItem.Location,
                    eventItem.GetAvailabilityStatus(),
                    eventItem
                );
            }
        }

        private void LoadRecommendations()
        {
            // Get recommended events based on search history
            var recommendations = EventRepository.GetRecommendedEvents();
            
            lstRecommendations.BeginUpdate();
            lstRecommendations.Items.Clear();

            if (recommendations.Count == 0)
            {
                // Show helpful message when no recommendations
                lstRecommendations.Items.Add("No recommendations available");
                lstRecommendations.Items.Add("Search for events to get");
                lstRecommendations.Items.Add("personalized recommendations");
            }
            else
            {
                // Display each recommended event
                foreach (var eventItem in recommendations)
                {
                    lstRecommendations.Items.Add($"{eventItem.Title} ({eventItem.Category})");
                }
            }
            
            lstRecommendations.EndUpdate();
            lstRecommendations.Refresh();
        }

        private void LoadRecentViews()
        {
            // Get the last 3 events the user viewed
            var recentEvents = EventRepository.GetRecentlyViewedEvents(3);
            
            lstRecentViews.BeginUpdate();
            lstRecentViews.Items.Clear();

            if (recentEvents.Count == 0)
            {
                // Show helpful message when no recent views
                lstRecentViews.Items.Add("No recently viewed events");
                lstRecentViews.Items.Add("Click on events to view");
                lstRecentViews.Items.Add("them here");
            }
            else
            {
                // Display each recently viewed event
                foreach (var eventItem in recentEvents)
                {
                    lstRecentViews.Items.Add($"{eventItem.Title} ({eventItem.Category})");
                }
            }
            
            lstRecentViews.EndUpdate();
            lstRecentViews.Refresh();
        }

        #endregion

        #region Event Handlers

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            // User clicked the search button
            PerformSearch();
        }

        private void TxtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow searching by pressing Enter key
            if (e.KeyChar == (char)Keys.Enter)
            {
                PerformSearch();
                e.Handled = true;
            }
        }

        private void CmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // User changed the category filter
            ApplyFilters();
        }

        private void BtnClearFilters_Click(object sender, EventArgs e)
        {
            // Clear all search filters and show all events
            txtSearch.Clear();
            cmbCategoryFilter.SelectedIndex = 0;
            LoadEvents();
        }

        private void DgvEvents_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // User double-clicked on an event row
            if (e.RowIndex >= 0)
            {
                var selectedEvent = dgvEvents.Rows[e.RowIndex].Cells[5].Value as Event;
                if (selectedEvent != null)
                {
                    ShowEventDetails(selectedEvent);
                }
            }
        }

        private void LstRecommendations_DoubleClick(object sender, EventArgs e)
        {
            // User double-clicked on a recommendation
            if (lstRecommendations.SelectedIndex >= 0 && lstRecommendations.SelectedItem.ToString() != "No recommendations available")
            {
                var recommendations = EventRepository.GetRecommendedEvents();
                if (lstRecommendations.SelectedIndex < recommendations.Count)
                {
                    ShowEventDetails(recommendations[lstRecommendations.SelectedIndex]);
                }
            }
        }

        private void LstRecentViews_DoubleClick(object sender, EventArgs e)
        {
            // User double-clicked on a recently viewed event
            if (lstRecentViews.SelectedIndex >= 0 && lstRecentViews.SelectedItem.ToString() != "No recently viewed events")
            {
                var recentEvents = EventRepository.GetRecentlyViewedEvents(3);
                if (lstRecentViews.SelectedIndex < recentEvents.Count)
                {
                    ShowEventDetails(recentEvents[lstRecentViews.SelectedIndex]);
                }
            }
        }

        #endregion

        #region Helper Methods

        private void PerformSearch()
        {
            // Get the search term and find matching events
            var searchTerm = txtSearch.Text.Trim();
            var events = string.IsNullOrEmpty(searchTerm) ? 
                EventRepository.GetAllEvents() : 
                EventRepository.SearchEvents(searchTerm);

            ApplyCategoryFilter(events);
            LoadRecommendations();
        }

        private void ApplyFilters()
        {
            // Apply both search and category filters
            var searchTerm = txtSearch.Text.Trim();
            var events = string.IsNullOrEmpty(searchTerm) ? 
                EventRepository.GetAllEvents() : 
                EventRepository.SearchEvents(searchTerm);

            ApplyCategoryFilter(events);
            LoadRecommendations();
        }

        private void ApplyCategoryFilter(List<Event> events)
        {
            // Filter events by selected category if one is chosen
            if (cmbCategoryFilter.SelectedIndex > 0)
            {
                var selectedCategory = cmbCategoryFilter.SelectedItem.ToString();
                events = events.Where(e => e.Category == selectedCategory).ToList();
            }

            DisplayEvents(events);
        }

        private void DisplayEvents(List<Event> events)
        {
            // Clear the table and add filtered events
            dgvEvents.Rows.Clear();

            foreach (var eventItem in events)
            {
                dgvEvents.Rows.Add(
                    eventItem.Title,
                    eventItem.Category,
                    eventItem.GetFormattedDate(),
                    eventItem.Location,
                    eventItem.GetAvailabilityStatus(),
                    eventItem
                );
            }
        }

        private void ShowEventDetails(Event eventItem)
        {
            // Add this event to the recently viewed list
            EventRepository.AddToRecentlyViewed(eventItem);

            // Build the details message
            var details = $"Event Details\n\n" +
                         $"Title: {eventItem.Title}\n" +
                         $"Category: {eventItem.Category}\n" +
                         $"Date: {eventItem.GetFormattedDate()}\n" +
                         $"Location: {eventItem.Location}\n" +
                         $"Status: {eventItem.GetAvailabilityStatus()}\n" +
                         $"Contact: {eventItem.ContactInfo}\n\n" +
                         $"Description:\n{eventItem.Description}\n\n";

            // Add registration info
            if (eventItem.RequiresRegistration)
            {
                var availableSpots = eventItem.GetAvailableSpots();
                if (availableSpots > 0)
                {
                    details += $"Registration Required\nAvailable Spots: {availableSpots}";
                }
                else
                {
                    details += "Registration Required - Event is Full";
                }
            }
            else
            {
                details += "No Registration Required - Open to All";
            }

            // Show the details in a popup
            MessageBox.Show(details, "Event Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadRecentViews();
        }

        private void SetupDataGridView()
        {
            // Set up the basic cell styling
            dgvEvents.DefaultCellStyle.BackColor = Color.White;
            dgvEvents.DefaultCellStyle.ForeColor = TextDark;
            dgvEvents.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            dgvEvents.DefaultCellStyle.SelectionForeColor = TextDark;
            dgvEvents.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgvEvents.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);

            // Style the column headers with blue background
            dgvEvents.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            dgvEvents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvEvents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvEvents.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvEvents.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 4, 4, 4);
            dgvEvents.ColumnHeadersHeight = 32;
            dgvEvents.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Make alternating rows slightly different color
            dgvEvents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            
            // Set row height to prevent gaps
            dgvEvents.RowTemplate.Height = 24;

            // Add all the columns we need
            dgvEvents.Columns.Add("Title", "Event Title");
            dgvEvents.Columns.Add("Category", "Category");
            dgvEvents.Columns.Add("Date", "Date & Time");
            dgvEvents.Columns.Add("Location", "Location");
            dgvEvents.Columns.Add("Status", "Status");
            dgvEvents.Columns.Add("EventObject", "Event Object");
            
            // Hide the EventObject column since it's just for storing the actual Event object
            dgvEvents.Columns["EventObject"].Visible = false;

            // Set how much space each column should take up
            dgvEvents.Columns["Title"].FillWeight = 35;
            dgvEvents.Columns["Category"].FillWeight = 20;
            dgvEvents.Columns["Date"].FillWeight = 20;
            dgvEvents.Columns["Location"].FillWeight = 20;
            dgvEvents.Columns["Status"].FillWeight = 15;

            // Set up color coding for the status column
            dgvEvents.CellFormatting += DgvEvents_CellFormatting;
        }

        private void DgvEvents_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Color code the status column based on availability
            if (dgvEvents.Columns[e.ColumnIndex].Name == "Status")
            {
                string status = e.Value?.ToString();
                switch (status)
                {
                    case "Available":
                        e.CellStyle.ForeColor = AccentGreen;
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    case "Full":
                        e.CellStyle.ForeColor = AccentOrange;
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                    case "Closed":
                        e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                        e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        break;
                }
            }
        }

        private Button MakeButton(string text, Color color)
        {
            // Helper method to create consistently styled buttons
            var button = new Button
            {
                Text = text,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Height = 38,
                Width = 120,
                Margin = new Padding(6)
            };
            button.FlatAppearance.BorderSize = 0;
            button.MouseEnter += delegate { if (button.Enabled) button.BackColor = ControlPaint.Light(color); };
            button.MouseLeave += delegate { if (button.Enabled) button.BackColor = color; };
            return button;
        }

        private static void EnableDoubleBuffer(Control control)
        {
            // Enable double buffering to reduce flickering
            var prop = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (prop != null) prop.SetValue(control, true, null);
        }

        #endregion

        #region Painting Methods

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            // Draw a border around the card
            var r = card.ClientRectangle;
            r.Width -= 1; r.Height -= 1;
            using (var pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, r);
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            // Draw borders around panels
            var pnl = (Panel)sender;
            var r = pnl.ClientRectangle; r.Width -= 1; r.Height -= 1;
            using (var pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, r);
        }

        #endregion
    }
}
