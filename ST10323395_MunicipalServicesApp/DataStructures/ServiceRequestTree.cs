using System;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Basic tree that models the municipal department hierarchy without leaning on built-in collections.
    /// </summary>
    /// <remarks>
    /// Each node keeps child references inside a <see cref="CustomList{T}"/> so traversals stay O(n) and inserts O(1).
    /// The layout feeds directly into the service status form where inspectors visualise departments and their requests.
    /// </remarks>
    public class ServiceRequestTree
    {
        /// <summary>
        /// Internal node representation used by the tree.
        /// </summary>
        public class TreeNode
        {
            public TreeNode(string departmentName)
            {
                DepartmentName = departmentName;
                Children = new CustomList<TreeNode>();
                Requests = new CustomList<ServiceRequest>();
            }

            // Name of the department or category represented by this node
            public string DepartmentName { get; }

            // Child nodes (sub departments, categories, or specific request groupings)
            public CustomList<TreeNode> Children { get; }

            // Optional list of requests that belong directly under this category
            public CustomList<ServiceRequest> Requests { get; }
        }

        // We keep a hidden root node so the public API stays nice and simple
        private readonly TreeNode _root = new TreeNode("ROOT");

        /// <summary>
        /// Adds a new top level department if it does not exist already.
        /// </summary>
        /// <remarks>
        /// Inserts run in O(1) because they simply append to the root node's custom list.
        /// </remarks>
        public void AddDepartment(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                throw new ArgumentException("Department name cannot be empty.", nameof(departmentName));
            }

            if (FindNode(_root, departmentName) != null)
            {
                return;
            }

            _root.Children.Add(new TreeNode(departmentName));
        }

        /// <summary>
        /// Adds a new sub category under an existing department or category.
        /// </summary>
        /// <remarks>
        /// Locates the parent via depth-first search (worst case O(n)), then appends the new child in O(1).
        /// </remarks>
        public void AddSubCategory(string parentDepartment, string subCategory)
        {
            if (string.IsNullOrWhiteSpace(parentDepartment))
            {
                throw new ArgumentException("Parent department cannot be empty.", nameof(parentDepartment));
            }

            if (string.IsNullOrWhiteSpace(subCategory))
            {
                throw new ArgumentException("Sub category cannot be empty.", nameof(subCategory));
            }

            var parentNode = FindNode(_root, parentDepartment);
            if (parentNode == null)
            {
                throw new InvalidOperationException($"Parent department '{parentDepartment}' does not exist.");
            }

            if (ContainsDepartment(parentNode.Children, subCategory))
            {
                return;
            }

            parentNode.Children.Add(new TreeNode(subCategory));
        }

        /// <summary>
        /// Adds a request to the specified category (department or sub category).
        /// </summary>
        /// <remarks>
        /// After locating the target node (O(n) in the worst case), the request is appended in O(1).
        /// This keeps request grouping fast enough for live updates.
        /// </remarks>
        public void AddRequest(string category, ServiceRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category cannot be empty.", nameof(category));
            }

            var node = FindNode(_root, category);
            if (node == null)
            {
                throw new InvalidOperationException($"Category '{category}' does not exist. Please add it before assigning requests.");
            }

            node.Requests.Add(request);
        }

        /// <summary>
        /// Recursively searches the tree for all requests that belong to a department/sub category.
        /// </summary>
        /// <remarks>
        /// Performs a depth-first traversal (O(n)) and returns the matches inside a <see cref="CustomList{T}"/>.
        /// </remarks>
        public CustomList<ServiceRequest> GetRequestsByDepartment(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                throw new ArgumentException("Department cannot be empty.", nameof(department));
            }

            var node = FindNode(_root, department);
            if (node == null)
            {
                return new CustomList<ServiceRequest>();
            }

            var results = new CustomList<ServiceRequest>();
            CollectRequests(node, results);
            return results;
        }

        /// <summary>
        /// Builds a simple textual representation of the hierarchy. 
        /// Useful when we need to populate a TreeView or present a summary.
        /// </summary>
        /// <remarks>
        /// Traversal is O(n) and the resulting list mirrors the logical hierarchy displayed on the form.
        /// </remarks>
        public CustomList<string> DisplayHierarchy()
        {
            var lines = new CustomList<string>();
            foreach (var department in _root.Children)
            {
                DisplayNode(department, lines, 0);
            }
            return lines;
        }

        /// <summary>
        /// Exposes the top level departments so the UI can bind to them.
        /// We return a read only view to keep the tree consistent.
        /// </summary>
        /// <remarks>
        /// Returns a shallow copy stored in a <see cref="CustomList{T}"/>. Calls are O(n) because we copy the references.
        /// </remarks>
        public CustomList<TreeNode> GetDepartments()
        {
            var departments = new CustomList<TreeNode>();
            foreach (var department in _root.Children)
            {
                departments.Add(department);
            }

            return departments;
        }

        private void DisplayNode(TreeNode node, CustomList<string> lines, int depth)
        {
            var indent = new string(' ', depth * 2);
            lines.Add($"{indent}{node.DepartmentName}");

            foreach (var request in node.Requests)
            {
                lines.Add($"{indent}  - {request.RequestId}: {request.Title} ({request.Status})");
            }

            foreach (var child in node.Children)
            {
                DisplayNode(child, lines, depth + 1);
            }
        }

        private static void CollectRequests(TreeNode node, CustomList<ServiceRequest> collector)
        {
            collector.AddRange(node.Requests);
            foreach (var child in node.Children)
            {
                CollectRequests(child, collector);
            }
        }

        private static TreeNode FindNode(TreeNode current, string name)
        {
            if (string.Equals(current.DepartmentName, name, StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }

            foreach (var child in current.Children)
            {
                var result = FindNode(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static bool ContainsDepartment(CustomList<TreeNode> children, string name)
        {
            foreach (var child in children)
            {
                if (string.Equals(child.DepartmentName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

