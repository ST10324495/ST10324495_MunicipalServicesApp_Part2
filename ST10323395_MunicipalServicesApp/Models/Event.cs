using System;

namespace ST10323395_MunicipalServicesApp.Models
{
    // Represents a local event or announcement
    public class Event
    {
        // Unique identifier for the event
        public int Id { get; set; }

        // Event title
        public string Title { get; set; } = string.Empty;

        // Event description
        public string Description { get; set; } = string.Empty;

        // Event category
        public string Category { get; set; } = string.Empty;

        // Event date and time
        public DateTime EventDate { get; set; }

        // Event location
        public string Location { get; set; } = string.Empty;

        // Contact information
        public string ContactInfo { get; set; } = string.Empty;

        // Maximum attendees allowed
        public int MaxAttendees { get; set; }

        // Current registered attendees
        public int CurrentAttendees { get; set; }

        // Whether registration is required
        public bool RequiresRegistration { get; set; }

        // When the event was created
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Whether the event is active
        public bool IsActive { get; set; } = true;

        public Event() { }
        public Event(string title, string description, string category, DateTime eventDate, 
                    string location, string contactInfo, int maxAttendees, bool requiresRegistration)
        {
            Title = title;
            Description = description;
            Category = category;
            EventDate = eventDate;
            Location = location;
            ContactInfo = contactInfo;
            MaxAttendees = maxAttendees;
            RequiresRegistration = requiresRegistration;
            CurrentAttendees = 0;
            CreatedDate = DateTime.Now;
            IsActive = true;
        }

        public string GetFormattedDate()
        {
            // Format the date nicely for display
            return EventDate.ToString("yyyy-MM-dd HH:mm");
        }

        public string GetShortDescription()
        {
            // Truncate long descriptions to 100 characters
            return Description.Length > 100 ? Description.Substring(0, 100) + "..." : Description;
        }

        public string GetAvailabilityStatus()
        {
            // Check if event is available, full, or closed
            if (!IsActive) return "Closed";
            if (RequiresRegistration && CurrentAttendees >= MaxAttendees) return "Full";
            return "Available";
        }

        public int GetAvailableSpots()
        {
            // Calculate how many spots are left
            if (!RequiresRegistration) return -1;
            return Math.Max(0, MaxAttendees - CurrentAttendees);
        }
    }
}
