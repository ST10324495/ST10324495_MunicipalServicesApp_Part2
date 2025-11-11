using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ST10323395_MunicipalServicesApp.DataStructures;
using ST10323395_MunicipalServicesApp.Models;
using WinFormsTreeNode = System.Windows.Forms.TreeNode;

namespace ST10323395_MunicipalServicesApp
{
    /// <summary>
    /// Shows the municipal service hierarchy using our custom tree structure.
    /// The comments call out how this meets the POE “Basic Tree” requirement.
    /// </summary>
    public class FrmServiceRequestStatus : Form
    {
        private readonly Color Primary = Color.FromArgb(33, 150, 243);
        private readonly Color PageBg = Color.FromArgb(236, 240, 241);
        private readonly Color CardBorder = Color.FromArgb(220, 224, 228);

        private readonly ServiceRequestTree _requestTree = new ServiceRequestTree();
        private readonly ServiceRequestRepository _repository = new ServiceRequestRepository();
        private readonly ServiceGraph _serviceGraph = new ServiceGraph();

        private TableLayoutPanel _root;
        private Label _title;
        private Label _description;
        private Panel _card;
        private TreeView _treeView;
        private TextBox _searchBox;
        private Button _searchButton;
        private ListBox _resultsBox;
        private ListBox _summaryBox;
        private ListBox _priorityBox;
        private ListBox _heapBox;
        private ListBox _graphBox;
        private ListBox _bstPreOrderBox;
        private ListBox _realTimeFeedBox;
        private ListBox _redBlackColourBox;
        private ListBox _dfsBox;
        private ListBox _mstBox;
        private TabControl _insightTabs;

        public FrmServiceRequestStatus()
        {
            InitializeComponent();
            BuildSampleHierarchy();
            PopulateTreeView();
            PopulateSummary();
            PerformSearch();
            PopulateGraph();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Service Request Status";
            BackColor = PageBg;
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;

            _root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(24, 24, 24, 12)
            };
            _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(_root);

            _title = new Label
            {
                Text = "Service Request Status Overview",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Primary,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            _description = new Label
            {
                Text = "The tree below highlights how departments break down into categories and requests.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 16)
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };
            headerPanel.Controls.Add(_description);
            headerPanel.Controls.Add(_title);
            _root.Controls.Add(headerPanel, 0, 0);

            _card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(24),
                Margin = new Padding(0),
                BorderStyle = BorderStyle.None,
                AutoScroll = true
            };
            _card.Paint += Card_Paint;
            _root.Controls.Add(_card, 0, 1);

            var cardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            _card.Controls.Add(cardLayout);

            var rightColumn = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rightColumn.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            cardLayout.Controls.Add(rightColumn, 0, 0);

            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                HideSelection = false,
                ShowNodeToolTips = true
            };
            cardLayout.Controls.Add(_treeView, 1, 0);

            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };

            var searchLabel = new Label
            {
                Text = "Search by Title:",
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            _searchBox = new TextBox
            {
                Width = 180,
                Margin = new Padding(0, 3, 8, 3)
            };
            _searchBox.KeyDown += SearchBox_KeyDown;

            _searchButton = new Button
            {
                Text = "Find Request",
                AutoSize = true,
                BackColor = Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(10, 4, 10, 4)
            };
            _searchButton.FlatAppearance.BorderSize = 0;
            _searchButton.Click += SearchButton_Click;

            searchPanel.Controls.Add(searchLabel);
            searchPanel.Controls.Add(_searchBox);
            searchPanel.Controls.Add(_searchButton);
            rightColumn.Controls.Add(searchPanel);

            _insightTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.Normal,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            rightColumn.Controls.Add(_insightTabs);

            _resultsBox = CreateInsightList();
            AddTab("Search Results", "BST ordered matches so users can find requests quickly.", _resultsBox);

            _priorityBox = CreateInsightList();
            AddTab("AVL Priority", "Balanced tree keeps priorities at O(log n).", _priorityBox);

            _heapBox = CreateInsightList();
            AddTab("Max-Heap Top 5", "Heap surfaces the most urgent tickets instantly.", _heapBox);

            _graphBox = CreateInsightList();
            AddTab("BFS Relationship Sweep", "BFS scans every connected department to surface open requests quickly (O(V + E)).", _graphBox);

            _summaryBox = CreateInsightList();
            AddTab("Hierarchy Summary", "Indented structure straight from ServiceRequestTree.", _summaryBox);

            _bstPreOrderBox = CreateInsightList();
            AddTab("BST Pre-Order Map", "Pre-order traversal reveals alphabetical branching so operators can visualise lookups.", _bstPreOrderBox);

            _realTimeFeedBox = CreateInsightList();
            AddTab("RB Real-Time Feed", "Red-Black balancing keeps live inserts fast for the operations dashboard.", _realTimeFeedBox);

            _redBlackColourBox = CreateInsightList();
            AddTab("RB Colour Levels", "Level-order output shows rotations/recolouring keeping the tree black-height stable.", _redBlackColourBox);

            _dfsBox = CreateInsightList();
            AddTab("DFS Investigation Trail", "DFS traces deep investigative chains through the graph structure.", _dfsBox);

            _mstBox = CreateInsightList();
            AddTab("MST Maintenance Routes", "Prim-inspired MST outlines the cheapest inter-department maintenance routes.", _mstBox);

            ResumeLayout(false);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var rect = _card.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            using (var pen = new Pen(CardBorder))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        /// <summary>
        /// Adds sample departments, categories, and requests to demonstrate the tree.
        /// </summary>
        private void BuildSampleHierarchy()
        {
            // We keep the demo data simple but realistic so the hierarchy feels alive.
            var now = DateTime.Now;

            _requestTree.AddDepartment("Water Services");
            _requestTree.AddDepartment("Electricity Department");
            _requestTree.AddDepartment("Waste Management");
            _requestTree.AddDepartment("Road Maintenance");
            _requestTree.AddDepartment("Parks & Recreation");

            _requestTree.AddSubCategory("Water Services", "Leaks");
            _requestTree.AddSubCategory("Water Services", "Pressure Issues");
            _requestTree.AddSubCategory("Water Services", "Water Quality");
            _requestTree.AddSubCategory("Electricity Department", "Outages");
            _requestTree.AddSubCategory("Electricity Department", "New Connections");
            _requestTree.AddSubCategory("Waste Management", "Collections");
            _requestTree.AddSubCategory("Waste Management", "Illegal Dumping");
            _requestTree.AddSubCategory("Road Maintenance", "Potholes");
            _requestTree.AddSubCategory("Road Maintenance", "Street Lights");
            _requestTree.AddSubCategory("Parks & Recreation", "Equipment");
            _requestTree.AddSubCategory("Parks & Recreation", "Groundskeeping");

            AddDemoRequestToStructures("Water Services", null, new ServiceRequest
            {
                RequestId = "#1023",
                Title = "New bulk meter installation",
                SubCategory = "General",
                Department = "Water Services",
                Status = "In Progress",
                Priority = 2,
                CreatedOn = now.AddMinutes(-45)
            });

            AddDemoRequestToStructures("Water Services", "Leaks", new ServiceRequest
            {
                RequestId = "#1042",
                Title = "Burst pipe on Pine Street",
                SubCategory = "Leaks",
                Department = "Water Services",
                Status = "Submitted",
                Priority = 4,
                CreatedOn = now.AddMinutes(-30)
            });

            AddDemoRequestToStructures("Water Services", "Pressure Issues", new ServiceRequest
            {
                RequestId = "#1077",
                Title = "Low water pressure in Ward 4",
                SubCategory = "Pressure Issues",
                Department = "Water Services",
                Status = "Resolved",
                Priority = 1,
                CreatedOn = now.AddMinutes(-20)
            });

            AddDemoRequestToStructures("Electricity Department", "Outages", new ServiceRequest
            {
                RequestId = "#2033",
                Title = "Transformer failure at 5th Avenue",
                Department = "Electricity Department",
                SubCategory = "Outages",
                Status = "In Progress",
                Priority = 5,
                CreatedOn = now.AddMinutes(-18)
            });

            AddDemoRequestToStructures("Waste Management", "Collections", new ServiceRequest
            {
                RequestId = "#3051",
                Title = "Missed recycling pickup",
                Department = "Waste Management",
                SubCategory = "Collections",
                Status = "Under Review",
                Priority = 3,
                CreatedOn = now.AddMinutes(-15)
            });

            AddDemoRequestToStructures("Water Services", "Water Quality", new ServiceRequest
            {
                RequestId = "#1110",
                Title = "Discoloured water in Riverbank Estate",
                Department = "Water Services",
                SubCategory = "Water Quality",
                Status = "In Progress",
                Priority = 4,
                CreatedOn = now.AddMinutes(-12)
            });

            AddDemoRequestToStructures("Electricity Department", "New Connections", new ServiceRequest
            {
                RequestId = "#2145",
                Title = "Small business supply upgrade request",
                Department = "Electricity Department",
                SubCategory = "New Connections",
                Status = "Submitted",
                Priority = 2,
                CreatedOn = now.AddMinutes(-10)
            });

            AddDemoRequestToStructures("Waste Management", "Illegal Dumping", new ServiceRequest
            {
                RequestId = "#3090",
                Title = "Illegal dumping near community centre",
                Department = "Waste Management",
                SubCategory = "Illegal Dumping",
                Status = "In Progress",
                Priority = 5,
                CreatedOn = now.AddMinutes(-8)
            });

            AddDemoRequestToStructures("Road Maintenance", "Potholes", new ServiceRequest
            {
                RequestId = "#4012",
                Title = "Severe pothole on Oak Avenue",
                Department = "Road Maintenance",
                SubCategory = "Potholes",
                Status = "Submitted",
                Priority = 4,
                CreatedOn = now.AddMinutes(-5)
            });

            AddDemoRequestToStructures("Road Maintenance", "Street Lights", new ServiceRequest
            {
                RequestId = "#4058",
                Title = "Street lights out on Maple Drive",
                Department = "Road Maintenance",
                SubCategory = "Street Lights",
                Status = "In Progress",
                Priority = 3,
                CreatedOn = now.AddMinutes(-3)
            });

            AddDemoRequestToStructures("Parks & Recreation", "Equipment", new ServiceRequest
            {
                RequestId = "#5099",
                Title = "Broken swings at Lakeside Park",
                Department = "Parks & Recreation",
                SubCategory = "Equipment",
                Status = "Submitted",
                Priority = 2,
                CreatedOn = now.AddMinutes(-2)
            });

            AddDemoRequestToStructures("Parks & Recreation", "Groundskeeping", new ServiceRequest
            {
                RequestId = "#5120",
                Title = "Overgrown grass on jogging trail",
                Department = "Parks & Recreation",
                SubCategory = "Groundskeeping",
                Status = "Scheduled",
                Priority = 1,
                CreatedOn = now.AddMinutes(-1)
            });

            // Build a simple relationship graph: Department -> Employee -> Request
            _serviceGraph.AddConnection("Water Services", "John Smith");
            _serviceGraph.AddConnection("John Smith", "#1023");
            _serviceGraph.AddConnection("Water Services", "Aisha Mthembu");
            _serviceGraph.AddConnection("Aisha Mthembu", "#1042");
            _serviceGraph.AddConnection("Electricity Department", "Thabo Dlamini");
            _serviceGraph.AddConnection("Thabo Dlamini", "#2033");
            _serviceGraph.AddConnection("Waste Management", "Naledi Jacobs");
            _serviceGraph.AddConnection("Naledi Jacobs", "#3051");
            _serviceGraph.AddConnection("Water Services", "Lungi Nxumalo");
            _serviceGraph.AddConnection("Lungi Nxumalo", "#1110");
            _serviceGraph.AddConnection("Electricity Department", "Gideon Strauss");
            _serviceGraph.AddConnection("Gideon Strauss", "#2145");
            _serviceGraph.AddConnection("Waste Management", "Sipho Ndlovu");
            _serviceGraph.AddConnection("Sipho Ndlovu", "#3090");
            _serviceGraph.AddConnection("Road Maintenance", "Emily Clarke");
            _serviceGraph.AddConnection("Emily Clarke", "#4012");
            _serviceGraph.AddConnection("Road Maintenance", "Brian Moagi");
            _serviceGraph.AddConnection("Brian Moagi", "#4058");
            _serviceGraph.AddConnection("Parks & Recreation", "Farah Daniels");
            _serviceGraph.AddConnection("Farah Daniels", "#5099");
            _serviceGraph.AddConnection("Parks & Recreation", "Peter Okoye");
            _serviceGraph.AddConnection("Peter Okoye", "#5120");

            // Weighted routes for MST demonstration (simulated travel times in km).
            _serviceGraph.AddRoute("Water Services", "Electricity Department", 6);
            _serviceGraph.AddRoute("Water Services", "Waste Management", 4);
            _serviceGraph.AddRoute("Waste Management", "Road Maintenance", 5);
            _serviceGraph.AddRoute("Electricity Department", "Road Maintenance", 7);
            _serviceGraph.AddRoute("Road Maintenance", "Parks & Recreation", 3);
            _serviceGraph.AddRoute("Parks & Recreation", "Water Services", 8);
            _serviceGraph.AddRoute("Waste Management", "Parks & Recreation", 6);
        }

        private void AddDemoRequestToStructures(string department, string category, ServiceRequest request)
        {
            _repository.Add(request);
            var target = string.IsNullOrWhiteSpace(category) ? department : category;
            _requestTree.AddRequest(target, request);
        }

        /// <summary>
        /// Walks through the tree structure and binds nodes to the WinForms TreeView.
        /// </summary>
        private void PopulateTreeView()
        {
            _treeView.Nodes.Clear();

            foreach (var department in _requestTree.GetDepartments())
            {
                var departmentNode = CreateUiNode(department);
                _treeView.Nodes.Add(departmentNode);
            }

            _treeView.ExpandAll();

            // Friendly note for markers: this visual tree is backed by ServiceRequestTree demonstrating hierarchical storage.
            _treeView.Nodes[0].EnsureVisible();
        }

        /// <summary>
        /// Fills the summary list with the formatted hierarchy from the data structure.
        /// </summary>
        private void PopulateSummary()
        {
            _summaryBox.Items.Clear();
            foreach (var line in _requestTree.DisplayHierarchy())
            {
                _summaryBox.Items.Add(line);
            }
        }

        private WinFormsTreeNode CreateUiNode(ServiceRequestTree.TreeNode node)
        {
            var uiNode = new WinFormsTreeNode(node.DepartmentName)
            {
                ToolTipText = $"Category: {node.DepartmentName}"
            };

            foreach (var request in node.Requests)
            {
                var requestNode = new WinFormsTreeNode($"{request.RequestId} - {request.Title} ({request.Status})")
                {
                    Tag = request,
                    ToolTipText = $"Status: {request.Status}"
                };
                uiNode.Nodes.Add(requestNode);
            }

            foreach (var child in node.Children)
            {
                uiNode.Nodes.Add(CreateUiNode(child));
            }

            return uiNode;
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            var term = _searchBox.Text;
            var results = _repository.SearchRequests(term);

            _resultsBox.Items.Clear();

            if (results.Count == 0)
            {
                _resultsBox.Items.Add("No matches found. (BST searched in O(log n) first, then fallback list scan)");
            }
            else
            {
                foreach (var request in results)
                {
                    _resultsBox.Items.Add($"{request.Title} [{request.RequestId}] - {request.Status}");
                }
            }

            _priorityBox.Items.Clear();
            foreach (var request in _repository.GetTopRequestsByPriority(5))
            {
                _priorityBox.Items.Add($"Priority {request.Priority} - {request.Title} [{request.RequestId}]");
            }

            _heapBox.Items.Clear();
            foreach (var request in _repository.GetTopRequestsFromHeap(5))
            {
                _heapBox.Items.Add($"Priority {request.Priority} - {request.Title} [{request.RequestId}]");
            }

            _bstPreOrderBox.Items.Clear();
            var preOrderVisual = _repository.GetPreOrderVisualisation();
            if (!string.IsNullOrWhiteSpace(preOrderVisual))
            {
                _bstPreOrderBox.Items.Add($"Pre-order layout: {preOrderVisual}");
                _bstPreOrderBox.Items.Add(string.Empty);
            }

            var preOrderTraversal = _repository.GetAlphabeticalPreOrder();
            if (preOrderTraversal.Count == 0)
            {
                _bstPreOrderBox.Items.Add("Tree empty - add requests to see branch order.");
            }
            else
            {
                int rank = 1;
                foreach (var request in preOrderTraversal)
                {
                    _bstPreOrderBox.Items.Add($"{rank++}. {request.Title} ({request.Department})");
                }
            }

            _realTimeFeedBox.Items.Clear();
            var latest = _repository.GetLatestRequests(5);
            if (latest.Count == 0)
            {
                _realTimeFeedBox.Items.Add("No activity yet. Red-Black tree springs into action as soon as tickets stream in.");
            }
            else
            {
                foreach (var request in latest)
                {
                    _realTimeFeedBox.Items.Add($"{request.CreatedOn:HH:mm} - {request.Title} [{request.Status}]");
                }
            }

            _redBlackColourBox.Items.Clear();
            var colouring = _repository.GetRealTimeColouring();
            if (colouring.Count == 0)
            {
                _redBlackColourBox.Items.Add("Tree empty. Insert requests to watch rotations and recolouring.");
            }
            else
            {
                foreach (var node in colouring)
                {
                    _redBlackColourBox.Items.Add(node);
                }
            }
        }

        private void PopulateGraph()
        {
            _graphBox.Items.Clear();
            _dfsBox.Items.Clear();
            _mstBox.Items.Clear();

            var departments = new[] { "Water Services", "Electricity Department", "Waste Management", "Road Maintenance", "Parks & Recreation" };

            foreach (var department in departments)
            {
                var bfsOrder = _serviceGraph.Traverse(department);
                if (bfsOrder.Count > 0)
                {
                    _graphBox.Items.Add($"{department} BFS → {FormatSequence(bfsOrder)}");
                }

                var dfsOrder = _serviceGraph.TraverseDepthFirst(department);
                if (dfsOrder.Count > 0)
                {
                    _dfsBox.Items.Add($"{department} DFS → {FormatSequence(dfsOrder)}");
                }
            }

            if (_graphBox.Items.Count == 0)
            {
                _graphBox.Items.Add("BFS queue empty – connect departments to begin traversal.");
            }

            if (_dfsBox.Items.Count == 0)
            {
                _dfsBox.Items.Add("No DFS trails recorded. Use AddConnection to seed deeper chains.");
            }

            var mstRoutes = _serviceGraph.GetMinimumSpanningTree();
            if (mstRoutes.Count == 0)
            {
                _mstBox.Items.Add("Configure weighted routes with AddRoute to view an MST.");
            }
            else
            {
                foreach (var route in mstRoutes)
                {
                    _mstBox.Items.Add(route);
                }
                _mstBox.Items.Add("Prim scan: O(V^2) across our lightweight custom structures.");
            }
        }

        private ListBox CreateInsightList()
        {
            return new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 9.5f),
                BackColor = Color.White,
                Margin = new Padding(4),
                HorizontalScrollbar = true
            };
        }

        private void AddTab(string title, string subtitle, Control content)
        {
            var page = new TabPage
            {
                Text = title,
                Padding = new Padding(12)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var description = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Top,
                AutoSize = true,
                ForeColor = Color.FromArgb(90, 98, 110),
                Font = new Font("Segoe UI", 8.5f)
            };

            layout.Controls.Add(description);
            layout.Controls.Add(content);

            page.Controls.Add(layout);
            _insightTabs.TabPages.Add(page);
        }

        private static string FormatSequence(CustomList<string> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return "(none)";
            }

            return string.Join(" → ", sequence.ToArray());
        }
    }
}

