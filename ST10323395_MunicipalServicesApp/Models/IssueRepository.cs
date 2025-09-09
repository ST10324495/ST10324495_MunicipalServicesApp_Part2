using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.Models
{
    /// <summary>
    /// In-memory storage for municipal service issues during application session.
    /// </summary>
    public static class IssueRepository
    {
        public static readonly List<Issue> Items = new List<Issue>();
    }
}
