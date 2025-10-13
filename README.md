# Municipal Services Portal

**Author:** ST10323395  
**Course:** PROG7312 Portfolio of Evidence (POE)  
**Version:** 2.0  
**Date:** 2025

## 📋 Overview

The Municipal Services Portal is a comprehensive Windows Forms application designed to empower citizens with an intuitive platform for reporting and tracking municipal service issues. Built on .NET Framework 4.7.2, this application features a modern, professional interface with responsive design and real-time status tracking capabilities.

## 🚀 Features

### Core Functionality
- **Issue Reporting**: Comprehensive form with location, category, description, and optional file attachment
- **Issue Viewing**: DataGridView display of all submitted issues with sorting and filtering
- **Status Tracking**: Real-time status updates with simulated municipal workflow
- **Local Events & Announcements**: Advanced event management with data structures and recommendation system
- **Responsive Design**: Adaptive layout that works on various screen resolutions (1024x768 to 1920x1080+)

### User Experience
- **Progress Tracking**: Visual progress bar with motivational messages during issue reporting
- **File Attachments**: Support for images and documents up to 10MB
- **Category Management**: 20 predefined categories covering all municipal services
- **Event Management**: Browse, search, and filter local events and announcements
- **Recommendation System**: Personalized event recommendations based on search history
- **Professional UI**: Consistent color scheme and typography throughout

### Technical Features
- **In-Memory Storage**: Session-based data persistence using static collections
- **Advanced Data Structures**: Stacks, queues, dictionaries, and sets for efficient data management
- **Form Validation**: Real-time validation with user-friendly error messages
- **Responsive Layout**: Proper use of Dock and Anchor properties for scaling
- **Event-Driven Updates**: Seamless navigation and data refresh between forms
- **Search & Filter**: Advanced search capabilities with category filtering

## 🛠️ Setup and Installation

### Prerequisites
- **Operating System**: Windows 10/11 (recommended)
- **Framework**: .NET Framework 4.7.2 or later
- **Development Environment**: Visual Studio 2017 or later
- **Hardware**: Minimum 4GB RAM for optimal performance

### Installation Steps

1. **Download the Project**
   ```bash
   git clone [repository-url]
   cd ST10323395_MunicipalServicesApp
   ```

2. **Open in Visual Studio**
   - Launch Visual Studio 2017 or later
   - Open the solution file: `ST10323395_MunicipalServicesApp.sln`
   - Allow Visual Studio to restore NuGet packages if prompted

3. **Build the Solution**
   - Press `Ctrl+Shift+B` or navigate to `Build > Build Solution`
   - Ensure build completes without errors

4. **Run the Application**
   - Press `F5` or click the "Start" button
   - The application will launch with the main menu interface

### Alternative Command Line Build
```bash
# Navigate to project directory
cd ST10323395_MunicipalServicesApp

# Build the solution
msbuild ST10323395_MunicipalServicesApp.sln /p:Configuration=Debug

# Run the executable
.\ST10323395_MunicipalServicesApp\bin\Debug\ST10323395_MunicipalServicesApp.exe
```

## 📖 User Guide

### Getting Started
1. **Launch Application**: Start the application to access the main menu
2. **Navigate**: Use the left sidebar to access different features
3. **Report Issues**: Click "Report Issues" to submit new municipal service problems
4. **View Reports**: Click "View Reported Issues" to see all submitted issues
5. **Check Status**: Click "Service Request Status" to track issue progress
6. **Browse Events**: Click "Local Events and Announcements" to explore community events
7. **Exit Application**: Click "Exit Application" to close the program

### Issue Reporting Workflow
1. **Fill Required Fields**:
   - **Location**: Enter specific address or landmark
   - **Category**: Select from predefined options (Sanitation, Roads, Utilities, etc.)
   - **Description**: Provide detailed description of the issue
2. **Optional Attachment**: Click "Attach File" to add supporting photos or documents
3. **Monitor Progress**: Watch the progress bar and motivational messages
4. **Submit**: Click "Submit Issue" when all required fields are completed
5. **Confirmation**: Review the success message with issue details

### Status Tracking
- **View Status**: All issues display current status (Submitted, In Progress, Resolved, Under Review)
- **Update Status**: Click "Update Status" to simulate real-time status changes
- **Color Coding**: Status indicators use color coding for quick identification
- **Refresh Data**: Status updates are reflected immediately in the interface

### Local Events and Announcements
- **Browse Events**: View all upcoming community events in a responsive DataGridView
- **Search Events**: Search by title, description, category, or location
- **Filter by Category**: Use dropdown to filter events by specific categories
- **Event Details**: Double-click events to view detailed information
- **Recommendations**: Get personalized event recommendations based on search history
- **Recent Views**: Track recently viewed events using stack data structure
- **Registration Info**: View registration requirements and availability status

## 🎨 Design Features

### Color Scheme
- **Primary Blue**: #2196F3 (Headers, active elements)
- **Success Green**: #2ECC71 (Completed actions, positive feedback)
- **Warning Orange**: #E67E22 (Status updates, attention items)
- **Dark Navigation**: #2D3546 (Sidebar background)
- **Light Content**: #ECF0F1 (Main content areas)

### Typography
- **Font Family**: Segoe UI (Professional, readable)
- **Headers**: Bold, 18pt for form titles
- **Body Text**: Regular, 10pt for content
- **Labels**: Bold, 10pt for form labels

### Layout Principles
- **Responsive Design**: Adapts to any window size
- **Consistent Spacing**: 24px padding, 8px margins
- **Card-Based Layout**: Clean, modern appearance
- **Professional Styling**: Rounded corners, subtle shadows

## 🔧 Technical Architecture

### Project Structure
```
ST10323395_MunicipalServicesApp/
├── Forms/
│   ├── MainMenuForm.cs          # Main application interface with navigation
│   ├── ReportIssueForm.cs       # Issue reporting with validation
│   ├── ViewIssuesForm.cs        # Issue display and management
│   ├── ServiceStatusForm.cs     # Status tracking and updates
│   ├── FrmLocalEvents.cs        # Local events with data structures
│   └── LocalEventsForm.cs       # Placeholder form (legacy)
├── Models/
│   ├── Event.cs                 # Event data model
│   ├── EventRepository.cs       # Event data structures and management
│   ├── Issue.cs                 # Issue data model
│   └── IssueRepository.cs       # In-memory issue storage
├── Properties/
│   └── AssemblyInfo.cs         # Application metadata
├── Program.cs                   # Application entry point
└── App.config                   # Configuration settings
```

### Data Management
- **Issue Model**: Issue class with properties for location, category, description, attachment, and timestamp
- **Event Model**: Event class with comprehensive properties for event management
- **Issue Repository**: Static List<Issue> for session-based storage
- **Event Repository**: Advanced data structures for event management
- **Validation**: Client-side validation with real-time feedback
- **Persistence**: In-memory storage (data cleared on application exit)

### Advanced Data Structures (Part 2)
- **SortedList<DateTime, List<Event>>**: Stores events ordered by date (replaces PriorityQueue)
- **Stack<Event>**: Recently viewed events (LIFO - Last In, First Out)
- **SortedDictionary<string, List<Event>>**: Events categorized and sorted by category
- **HashSet<string>**: Unique event categories and dates
- **Queue<string>**: Recent search terms (FIFO - First In, First Out)
- **Dictionary<string, List<Event>>**: Related events for recommendation system

### Form Architecture
- **MainMenuForm**: Navigation hub with responsive sidebar and content area
- **Child Forms**: Embedded forms that load within the main content area
- **Event Handling**: Proper event-driven architecture with clean separation of concerns
- **Responsive Layout**: TableLayoutPanel and Dock properties for scaling
- **FrmLocalEvents**: Advanced event management with data structures and recommendation system

## 🐛 Troubleshooting

### Common Issues

**Application Won't Start**
- Ensure .NET Framework 4.7.2 is installed
- Check that Visual Studio has restored all NuGet packages
- Verify the solution builds without errors

**UI Doesn't Resize Properly**
- Ensure the main window is not maximized when testing resize
- Check that FormBorderStyle is set to Sizable
- Verify minimum window size is not too large for your screen

**Forms Don't Load**
- Ensure all form files are included in the project
- Check that namespaces are consistent across all files
- Verify that the project file includes all form references

**DataGridView Issues**
- Ensure AutoSizeColumnsMode is set to Fill
- Check that FillWeight properties are set for responsive columns
- Verify that data binding is working correctly

**File Attachment Problems**
- Check file size is under 10MB limit
- Ensure file types are supported (images, documents)
- Verify file path is accessible

### Performance Tips
- **Memory Usage**: Application uses in-memory storage; restart periodically for large datasets
- **File Attachments**: Large files may slow down the interface
- **Window Resizing**: Resize smoothly for best performance

## 📸 Screenshots

*Screenshots go here*

### Main Menu Interface
- Professional navigation sidebar
- Responsive content area
- Clean, modern design

### Issue Reporting Form
- Progress tracking with motivational messages
- File attachment capability
- Real-time validation feedback

### Issue Viewing Interface
- DataGridView with responsive columns
- Color-coded status indicators
- Professional table styling

### Status Tracking Page
- Real-time status updates
- Color-coded status indicators
- Update functionality

## 🔮 Future Enhancements

- **Database Integration**: Persistent storage with SQL Server or SQLite
- **User Authentication**: Login system with user profiles
- **Email Notifications**: Automated status update notifications
- **Mobile App**: Cross-platform mobile version
- **Advanced Reporting**: Analytics and reporting dashboard
- **Integration**: Connect with municipal management systems

## 📞 Support

**Student Information:**
- **Student ID**: ST10323395
- **Course**: PROG7312 Portfolio of Evidence
- **Institution**: [University Name]

**Technical Support:**
- Check the troubleshooting section above
- Ensure all prerequisites are met
- Verify the application builds successfully

## 📄 License

This project is developed for educational purposes as part of university coursework for PROG7312 Portfolio of Evidence.

---

**Note**: This application is designed as a demonstration of modern Windows Forms development practices, responsive design principles, and professional UI/UX implementation. All code follows industry standards and best practices for maintainability and scalability.