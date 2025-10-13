using System;

namespace ST10323395_MunicipalServicesApp.Models
{
    // Represents a reported issue
    public class Issue
    {
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string AttachmentPath { get; set; }
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        public Issue() { }

        public Issue(string location, string category, string description, string attachmentPath)
        {
            Location = location;
            Category = category;
            Description = description;
            // Handle empty attachment paths gracefully
            AttachmentPath = string.IsNullOrWhiteSpace(attachmentPath) ? string.Empty : attachmentPath;
            DateSubmitted = DateTime.Now;
        }
    }
}
