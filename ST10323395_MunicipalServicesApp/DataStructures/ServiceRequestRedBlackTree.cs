using System;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Lightweight red-black tree keyed by request timestamps so new tickets slot in efficiently without re-scans.
    /// </summary>
    /// <remarks>
    /// The colour property drives near-O(log n) inserts even during rapid municipal spikes, mirroring real-time dashboards.
    /// </remarks>
    public class ServiceRequestRedBlackTree
    {
        private sealed class Node
        {
            public Node(ServiceRequest request)
            {
                Request = request ?? throw new ArgumentNullException(nameof(request));
                IsRed = true;
            }

            public ServiceRequest Request { get; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public Node Parent { get; set; }
            public bool IsRed { get; set; }
        }

        private Node _root;

        /// <summary>
        /// Inserts the request using <see cref="ServiceRequest.CreatedOn"/> as the primary key (newest first).
        /// </summary>
        /// <remarks>
        /// Keeps bursts of incoming tickets balanced without depending on <c>SortedSet&lt;T&gt;</c>.
        /// </remarks>
        public void Insert(ServiceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var node = new Node(request);
            InsertNode(node);
            FixInsert(node);
        }

        /// <summary>
        /// Returns the requests sorted from newest to oldest, providing the UI with an instant activity feed.
        /// </summary>
        public CustomList<ServiceRequest> GetRequestsNewestFirst()
        {
            var results = new CustomList<ServiceRequest>();
            ReverseInOrder(_root, results);
            return results;
        }

        /// <summary>
        /// Creates a friendly view of the tree (level-order) that highlights balancing colours for the demo tabs.
        /// </summary>
        public CustomList<string> VisualiseLevelOrder()
        {
            var output = new CustomList<string>();
            if (_root == null)
            {
                return output;
            }

            var queue = new CustomQueue<Node>();
            queue.Enqueue(_root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                output.Add($"{current.Request.Title} [{(current.IsRed ? "Red" : "Black")}]");

                if (current.Left != null)
                {
                    queue.Enqueue(current.Left);
                }

                if (current.Right != null)
                {
                    queue.Enqueue(current.Right);
                }
            }

            return output;
        }

        public void Clear()
        {
            _root = null;
        }

        private void InsertNode(Node node)
        {
            Node parent = null;
            Node current = _root;
            while (current != null)
            {
                parent = current;
                if (IsNewer(node.Request, current.Request))
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            node.Parent = parent;

            if (parent == null)
            {
                _root = node;
            }
            else if (IsNewer(node.Request, parent.Request))
            {
                parent.Left = node;
            }
            else
            {
                parent.Right = node;
            }
        }

        private void FixInsert(Node node)
        {
            while (node != _root && node.Parent.IsRed)
            {
                if (node.Parent == node.Parent.Parent.Left)
                {
                    var uncle = node.Parent.Parent.Right;

                    if (uncle != null && uncle.IsRed)
                    {
                        node.Parent.IsRed = false;
                        uncle.IsRed = false;
                        node.Parent.Parent.IsRed = true;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node == node.Parent.Right)
                        {
                            node = node.Parent;
                            RotateLeft(node);
                        }

                        node.Parent.IsRed = false;
                        node.Parent.Parent.IsRed = true;
                        RotateRight(node.Parent.Parent);
                    }
                }
                else
                {
                    var uncle = node.Parent.Parent.Left;

                    if (uncle != null && uncle.IsRed)
                    {
                        node.Parent.IsRed = false;
                        uncle.IsRed = false;
                        node.Parent.Parent.IsRed = true;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node == node.Parent.Left)
                        {
                            node = node.Parent;
                            RotateRight(node);
                        }

                        node.Parent.IsRed = false;
                        node.Parent.Parent.IsRed = true;
                        RotateLeft(node.Parent.Parent);
                    }
                }
            }

            _root.IsRed = false;
        }

        private void RotateLeft(Node node)
        {
            var pivot = node.Right;
            node.Right = pivot.Left;
            if (pivot.Left != null)
            {
                pivot.Left.Parent = node;
            }

            pivot.Parent = node.Parent;

            if (node.Parent == null)
            {
                _root = pivot;
            }
            else if (node == node.Parent.Left)
            {
                node.Parent.Left = pivot;
            }
            else
            {
                node.Parent.Right = pivot;
            }

            pivot.Left = node;
            node.Parent = pivot;
        }

        private void RotateRight(Node node)
        {
            var pivot = node.Left;
            node.Left = pivot.Right;
            if (pivot.Right != null)
            {
                pivot.Right.Parent = node;
            }

            pivot.Parent = node.Parent;

            if (node.Parent == null)
            {
                _root = pivot;
            }
            else if (node == node.Parent.Right)
            {
                node.Parent.Right = pivot;
            }
            else
            {
                node.Parent.Left = pivot;
            }

            pivot.Right = node;
            node.Parent = pivot;
        }

        private static bool IsNewer(ServiceRequest candidate, ServiceRequest existing)
        {
            int compare = DateTime.Compare(candidate.CreatedOn, existing.CreatedOn);
            if (compare == 0)
            {
                return string.Compare(candidate.RequestId, existing.RequestId, StringComparison.OrdinalIgnoreCase) < 0;
            }

            return compare > 0;
        }

        private static void ReverseInOrder(Node node, CustomList<ServiceRequest> collector)
        {
            if (node == null) return;

            ReverseInOrder(node.Left, collector);
            collector.Add(node.Request);
            ReverseInOrder(node.Right, collector);
        }
    }
}


