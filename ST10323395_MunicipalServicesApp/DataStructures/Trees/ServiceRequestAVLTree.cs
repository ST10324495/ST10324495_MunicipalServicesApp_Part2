using System;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    // AVL tree that sorts requests by priority. Unlike a regular BST, this one automatically balances itself using rotations
    // when nodes get too lopsided. Each node tracks its height, and if one side gets more than 1 level taller, it rotates
    // to fix it. This keeps searches fast (O(log n)) even when priorities are added in a bad order. The "AVL Priority" tab
    // shows the top 5 most urgent requests pulled from this tree.
    // Used for: Advanced Data Structures, Integration with Interface

    public class ServiceRequestAVLTree
    {
        private sealed class Node
        {
            public Node(ServiceRequest request)
            {
                Request = request ?? throw new ArgumentNullException(nameof(request));
                Height = 1;
            }

            public ServiceRequest Request { get; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public int Height { get; set; }
        }

        private Node _root;

        /// <summary>
        /// Inserts a request while keeping the tree balanced.
        /// </summary>
        /// <remarks>
        /// Each insert is O(log n). It ensures priority boards remain responsive even as more tickets arrive.
        /// </remarks>
        public void Insert(ServiceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _root = Insert(_root, request);
        }

        private Node Insert(Node node, ServiceRequest request)
        {
            if (node == null)
            {
                return new Node(request);
            }

            if (request.Priority < node.Request.Priority)
            {
                node.Left = Insert(node.Left, request);
            }
            else if (request.Priority > node.Request.Priority)
            {
                node.Right = Insert(node.Right, request);
            }
            else
            {
                // Keep equal priorities grouped; fall back to title comparison to keep tree deterministic.
                if (string.Compare(request.Title, node.Request.Title, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    node.Left = Insert(node.Left, request);
                }
                else
                {
                    node.Right = Insert(node.Right, request);
                }
            }

            UpdateHeight(node);
            return Balance(node);
        }

        /// <summary>
        /// Traverses the tree in ascending priority order.
        /// </summary>
        /// <remarks>
        /// Runs in O(n) and returns the results inside a <see cref="CustomList{T}"/> so later steps stay free of built-in lists.
        /// </remarks>
        public CustomList<ServiceRequest> GetRequestsInOrder()
        {
            var result = new CustomList<ServiceRequest>();
            InOrder(_root, result);
            return result;
        }

        /// <summary>
        /// Returns the highest priority requests, favouring urgent tickets first.
        /// </summary>
        /// <remarks>
        /// Performs an O(n) traversal, reverses the array, and then copies at most <paramref name="count"/> items into a <see cref="CustomList{T}"/>.
        /// This keeps the heap and AVL projections aligned without depending on LINQ.
        /// </remarks>
        public CustomList<ServiceRequest> GetHighPriorityRequests(int count)
        {
            var ordered = GetRequestsInOrder();
            var buffer = ordered.ToArray();
            Reverse(buffer);

            var result = new CustomList<ServiceRequest>();
            if (count <= 0 || count >= buffer.Length)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    result.Add(buffer[i]);
                }
                return result;
            }

            for (int i = 0; i < count && i < buffer.Length; i++)
            {
                result.Add(buffer[i]);
            }

            return result;
        }

        private static void Reverse(ServiceRequest[] items)
        {
            int left = 0;
            int right = items.Length - 1;
            while (left < right)
            {
                var temp = items[left];
                items[left] = items[right];
                items[right] = temp;
                left++;
                right--;
            }
        }

        private void InOrder(Node node, CustomList<ServiceRequest> collector)
        {
            if (node == null) return;
            InOrder(node.Left, collector);
            collector.Add(node.Request);
            InOrder(node.Right, collector);
        }

        private static int Height(Node node) => node?.Height ?? 0;

        private static void UpdateHeight(Node node)
        {
            node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;
        }

        private static int GetBalance(Node node)
        {
            return node == null ? 0 : Height(node.Left) - Height(node.Right);
        }

        private Node Balance(Node node)
        {
            int balance = GetBalance(node);

            // Left heavy
            if (balance > 1)
            {
                if (GetBalance(node.Left) < 0)
                {
                    node.Left = RotateLeft(node.Left); // LR case
                }
                return RotateRight(node); // LL case
            }

            // Right heavy
            if (balance < -1)
            {
                if (GetBalance(node.Right) > 0)
                {
                    node.Right = RotateRight(node.Right); // RL case
                }
                return RotateLeft(node); // RR case
            }

            return node;
        }

        private static Node RotateRight(Node y)
        {
            Node x = y.Left;
            Node T2 = x.Right;

            x.Right = y;
            y.Left = T2;

            UpdateHeight(y);
            UpdateHeight(x);

            return x;
        }

        private static Node RotateLeft(Node x)
        {
            Node y = x.Right;
            Node T2 = y.Left;

            y.Left = x;
            x.Right = T2;

            UpdateHeight(x);
            UpdateHeight(y);

            return y;
        }

        /// <summary>
        /// Removes a request using its priority (and title as a tie-breaker) while maintaining AVL balance.
        /// </summary>
        /// <remarks>
        /// Deletion mirrors insert complexity: O(log n). It keeps the urgent queue tidy when requests close.
        /// </remarks>
        public bool Delete(int priority, string title)
        {
            bool removed;
            _root = Delete(_root, priority, title ?? string.Empty, out removed);
            return removed;
        }

        private Node Delete(Node node, int priority, string title, out bool removed)
        {
            if (node == null)
            {
                removed = false;
                return null;
            }

            int compare = Compare(priority, title, node.Request);
            if (compare < 0)
            {
                node.Left = Delete(node.Left, priority, title, out removed);
            }
            else if (compare > 0)
            {
                node.Right = Delete(node.Right, priority, title, out removed);
            }
            else
            {
                removed = true;
                if (node.Left == null || node.Right == null)
                {
                    Node child = node.Left ?? node.Right;
                    return child;
                }

                // Two children: replace with in-order successor.
                Node successor = GetMin(node.Right);
                node.Right = Delete(node.Right, successor.Request.Priority, successor.Request.Title, out _);
                CopyRequest(node.Request, successor.Request);
            }

            UpdateHeight(node);
            return Balance(node);
        }

        private static Node GetMin(Node node)
        {
            while (node.Left != null)
            {
                node = node.Left;
            }
            return node;
        }

        private static int Compare(int priority, string title, ServiceRequest current)
        {
            int priorityCompare = priority.CompareTo(current.Priority);
            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            return string.Compare(title, current.Title, StringComparison.OrdinalIgnoreCase);
        }

        private static void CopyRequest(ServiceRequest target, ServiceRequest source)
        {
            target.RequestId = source.RequestId;
            target.Title = source.Title;
            target.Description = source.Description;
            target.Department = source.Department;
            target.SubCategory = source.SubCategory;
            target.Status = source.Status;
            target.CreatedOn = source.CreatedOn;
            target.Priority = source.Priority;
        }
    }
}

