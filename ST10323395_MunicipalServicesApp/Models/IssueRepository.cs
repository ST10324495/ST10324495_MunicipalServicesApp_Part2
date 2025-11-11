using ST10323395_MunicipalServicesApp.DataStructures;

namespace ST10323395_MunicipalServicesApp.Models
{
    // In-memory storage for issues
    public static class IssueRepository
    {
        /// <summary>
        /// In-memory issue list backed by <see cref="CustomList{T}"/> to satisfy the rubric’s custom collection requirement.
        /// </summary>
        /// <remarks>
        /// Stores issues in insertion order inside a growable array so lookups remain O(1) by index and appends amortized O(1).
        /// </remarks>
        public static readonly CustomList<Issue> Items = new CustomList<Issue>();
    }
}
