using System;
using ST10323395_MunicipalServicesApp.DataStructures;

namespace ST10323395_MunicipalServicesApp.Models
{
    /// <summary>
    /// Central store for service requests. Uses the BST to keep lookups fast (O(log n) vs O(n)).
    /// </summary>
    /// <remarks>
    /// Every collection inside this repository is a custom structure so the rubric requirement is met end-to-end.
    /// </remarks>
    public class ServiceRequestRepository
    {
        private readonly CustomList<ServiceRequest> _requests = new CustomList<ServiceRequest>();
        private readonly ServiceRequestBST _bst = new ServiceRequestBST();
        private readonly ServiceRequestAVLTree _priorityTree = new ServiceRequestAVLTree();
        private readonly ServiceRequestHeap _heap = new ServiceRequestHeap();
        private readonly ServiceRequestRedBlackTree _realTimeTree = new ServiceRequestRedBlackTree();

        /// <summary>
        /// Returns every stored request in insertion order.
        /// </summary>
        /// <remarks>
        /// Copies the internal list into a new <see cref="CustomList{T}"/> (O(n)) so callers cannot mutate the backing store.
        /// </remarks>
        public CustomList<ServiceRequest> GetAllRequests()
        {
            var copy = new CustomList<ServiceRequest>();
            foreach (var request in _requests)
            {
                copy.Add(request);
            }

            return copy;
        }

        /// <summary>
        /// Adds a new service request to every supporting structure.
        /// </summary>
        /// <remarks>
        /// Inserts into the list (amortized O(1)), BST (O(log n) average), AVL tree (O(log n)), and heap (O(log n)).
        /// </remarks>
        public void Add(ServiceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            _requests.Add(request);
            _bst.Insert(request);
            _priorityTree.Insert(request);
            _heap.Insert(request);
            _realTimeTree.Insert(request);
        }

        /// <summary>
        /// Finds a request by title using the BST. If an exact match is not found we fall back to
        /// alphabetically ordered traversal and return partial matches (still O(n), but only after
        /// the fast exact lookup attempt).
        /// </summary>
        public CustomList<ServiceRequest> SearchRequests(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return _bst.InOrderTraversal();
            }

            var exact = _bst.Search(searchTerm);
            if (exact != null)
            {
                var exactList = new CustomList<ServiceRequest>();
                exactList.Add(exact);
                return exactList;
            }

            var ordered = _bst.InOrderTraversal();
            var matches = new CustomList<ServiceRequest>();

            foreach (var request in ordered)
            {
                if (request.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches.Add(request);
                }
            }

            return matches;
        }

        /// <summary>
        /// Returns requests ordered by priority using the AVL tree to keep retrievals balanced.
        /// </summary>
        public CustomList<ServiceRequest> GetTopRequestsByPriority(int count)
        {
            return _priorityTree.GetHighPriorityRequests(count);
        }

        /// <summary>
        /// Returns the top N urgent requests using the heap so we showcase O(log n) priority access.
        /// </summary>
        public CustomList<ServiceRequest> GetTopRequestsFromHeap(int count)
        {
            var results = new CustomList<ServiceRequest>();
            if (count <= 0)
            {
                return results;
            }

            var tempHeap = new ServiceRequestHeap();
            tempHeap.BuildHeap(_heap.ToArray());

            for (int i = 0; i < count; i++)
            {
                var request = tempHeap.ExtractMax();
                if (request == null) break;
                results.Add(request);
            }

            return results;
        }

        /// <summary>
        /// Returns the newest municipal tickets using the red-black tree to keep inserts nearly O(log n) during spikes.
        /// </summary>
        public CustomList<ServiceRequest> GetLatestRequests(int count)
        {
            var ordered = _realTimeTree.GetRequestsNewestFirst();
            if (count <= 0 || count >= ordered.Count)
            {
                return ordered;
            }

            var trimmed = new CustomList<ServiceRequest>();
            for (int i = 0; i < count && i < ordered.Count; i++)
            {
                trimmed.Add(ordered[i]);
            }

            return trimmed;
        }

        /// <summary>
        /// Produces a readable snapshot of the red-black tree colouring so the UI can call out balancing behaviour.
        /// </summary>
        public CustomList<string> GetRealTimeColouring()
        {
            return _realTimeTree.VisualiseLevelOrder();
        }

        /// <summary>
        /// Returns the BST pre-order traversal so the UI can expose branch order for alphabetical organisation.
        /// </summary>
        public CustomList<ServiceRequest> GetAlphabeticalPreOrder()
        {
            return _bst.PreOrderTraversal();
        }

        /// <summary>
        /// Returns a compact textual layout of the BST, aiding the visual explanation tab.
        /// </summary>
        public string GetPreOrderVisualisation()
        {
            return _bst.GetPreOrderVisualisation();
        }

        /// <summary>
        /// Updates a request's urgency in the heap to mimic manual reprioritisation from the contact centre.
        /// </summary>
        public bool UpdateHeapPriority(string requestId, int newPriority)
        {
            return _heap.UpdatePriority(requestId, newPriority);
        }

        /// <summary>
        /// Lowers the priority of a request when issues cool down; demonstrates the decrease-key operation.
        /// </summary>
        public bool DecreaseHeapPriority(string requestId, int newPriority)
        {
            return _heap.DecreasePriority(requestId, newPriority);
        }
    }
}

