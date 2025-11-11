using System;

namespace ST10323395_MunicipalServicesApp.Models
{
    // Captures a single municipal service request so we can show it in the tree later
    public class ServiceRequest
    {
        // A friendly identifier (e.g. ticket number from the municipality system)
        public string RequestId { get; set; } = string.Empty;

        // High level name to display in the UI
        public string Title { get; set; } = string.Empty;

        // Optional longer description for context when the user inspects the request
        public string Description { get; set; }

        // Department that is responsible (Water, Electricity, Waste, etc.)
        public string Department { get; set; } = string.Empty;

        // Sub category within the department (Leaks, Outages, Collections, etc.)
        public string SubCategory { get; set; } = string.Empty;

        // Current status, mirroring what the municipality would report back
        public string Status { get; set; } = "Submitted";

        // When the request was logged. Defaults to now so demo data looks current.
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Priority score (higher means more urgent). We use it for AVL balancing.
        public int Priority { get; set; }
    }
}

