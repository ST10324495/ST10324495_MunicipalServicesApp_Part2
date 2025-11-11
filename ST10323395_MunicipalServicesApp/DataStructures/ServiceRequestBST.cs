using System;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Binary search tree that organises service requests alphabetically without relying on <c>SortedDictionary</c>.
    /// </summary>
    /// <remarks>
    /// Nodes are connected manually; inserts and searches are O(h) where h is tree height (O(log n) when balanced).
    /// The structure keeps lookups fast for the service request status form.
    /// </remarks>
    public class ServiceRequestBST
    {
        private sealed class Node
        {
            public Node(ServiceRequest request)
            {
                Request = request ?? throw new ArgumentNullException(nameof(request));
            }

            public ServiceRequest Request { get; private set; }
            public Node Left { get; set; }
            public Node Right { get; set; }

            public void Update(ServiceRequest request)
            {
                Request = request ?? throw new ArgumentNullException(nameof(request));
            }
        }

        private Node _root;

        /// <summary>
        /// Inserts a request using its Title as the key. Titles are compared alphabetically.
        /// </summary>
        /// <remarks>
        /// Operation runs in O(h). It keeps new municipal tickets discoverable without scanning entire lists.
        /// </remarks>
        public void Insert(ServiceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (_root == null)
            {
                _root = new Node(request);
                return;
            }

            InsertInternal(_root, request);
        }

        private static void InsertInternal(Node current, ServiceRequest request)
        {
            int compare = string.Compare(request.Title, current.Request.Title, StringComparison.OrdinalIgnoreCase);

            // If the title already exists we simply replace the stored request
            if (compare == 0)
            {
                current.Update(request);
                return;
            }

            if (compare < 0)
            {
                if (current.Left == null)
                {
                    current.Left = new Node(request);
                }
                else
                {
                    InsertInternal(current.Left, request);
                }
            }
            else
            {
                if (current.Right == null)
                {
                    current.Right = new Node(request);
                }
                else
                {
                    InsertInternal(current.Right, request);
                }
            }
        }

        /// <summary>
        /// Searches the tree for a request with the specified title (case-insensitive).
        /// </summary>
        /// <remarks>
        /// Runs in O(h). This gives the UI instant lookups when operators search for a ticket by name.
        /// </remarks>
        public ServiceRequest Search(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return SearchInternal(_root, title.Trim());
        }

        private static ServiceRequest SearchInternal(Node node, string title)
        {
            if (node == null) return null;

            int compare = string.Compare(title, node.Request.Title, StringComparison.OrdinalIgnoreCase);
            if (compare == 0)
            {
                return node.Request;
            }

            if (compare < 0)
            {
                return SearchInternal(node.Left, title);
            }

            return SearchInternal(node.Right, title);
        }

        /// <summary>
        /// Returns every request in alphabetical order using an in-order traversal.
        /// </summary>
        /// <remarks>
        /// Performs O(n) work while keeping memory usage low. The results land inside a <see cref="CustomList{T}"/> so later stages stay off built-in lists.
        /// </remarks>
        public CustomList<ServiceRequest> InOrderTraversal()
        {
            var results = new CustomList<ServiceRequest>();
            TraverseInOrder(_root, results);
            return results;
        }

        private static void TraverseInOrder(Node node, CustomList<ServiceRequest> collector)
        {
            if (node == null) return;

            TraverseInOrder(node.Left, collector);
            collector.Add(node.Request);
            TraverseInOrder(node.Right, collector);
        }

        /// <summary>
        /// Removes the request associated with the supplied title.
        /// </summary>
        /// <remarks>
        /// Deletion runs in O(h). It keeps the alphabetical catalogue tidy as items are resolved or reassigned.
        /// </remarks>
        public bool Delete(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            bool removed;
            _root = DeleteInternal(_root, title.Trim(), out removed);
            return removed;
        }

        private static Node DeleteInternal(Node node, string title, out bool removed)
        {
            if (node == null)
            {
                removed = false;
                return null;
            }

            int compare = string.Compare(title, node.Request.Title, StringComparison.OrdinalIgnoreCase);
            if (compare < 0)
            {
                node.Left = DeleteInternal(node.Left, title, out removed);
                return node;
            }

            if (compare > 0)
            {
                node.Right = DeleteInternal(node.Right, title, out removed);
                return node;
            }

            // Match found.
            removed = true;

            if (node.Left == null && node.Right == null)
            {
                return null;
            }

            if (node.Left == null)
            {
                return node.Right;
            }

            if (node.Right == null)
            {
                return node.Left;
            }

            // Two children: replace with in-order successor.
            Node successorParent = node;
            Node successor = node.Right;
            while (successor.Left != null)
            {
                successorParent = successor;
                successor = successor.Left;
            }

            if (successorParent != node)
            {
                successorParent.Left = successor.Right;
                successor.Right = node.Right;
            }

            successor.Left = node.Left;
            return successor;
        }

        /// <summary>
        /// Produces a pre-order traversal so the UI can visualise the branching order.
        /// </summary>
        public CustomList<ServiceRequest> PreOrderTraversal()
        {
            var results = new CustomList<ServiceRequest>();
            TraversePreOrder(_root, results);
            return results;
        }

        /// <summary>
        /// Offers a quick textual snapshot of the current tree layout.
        /// </summary>
        public string GetPreOrderVisualisation()
        {
            var titles = new CustomList<string>();
            CollectPreOrderTitles(_root, titles);
            return string.Join(" → ", titles.ToArray());
        }

        private static void TraversePreOrder(Node node, CustomList<ServiceRequest> collector)
        {
            if (node == null) return;

            collector.Add(node.Request);
            TraversePreOrder(node.Left, collector);
            TraversePreOrder(node.Right, collector);
        }

        private static void CollectPreOrderTitles(Node node, CustomList<string> titles)
        {
            if (node == null) return;

            titles.Add(node.Request.Title);
            CollectPreOrderTitles(node.Left, titles);
            CollectPreOrderTitles(node.Right, titles);
        }
    }
}

