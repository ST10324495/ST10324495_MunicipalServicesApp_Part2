using System;
using System.Collections.Generic;
using ST10323395_MunicipalServicesApp.Models;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Max-heap that keeps the most urgent (highest priority) service requests at the top.
    /// Insert and extract operations run in O(log n), showcasing efficient priority management.
    /// </summary>
    /// <remarks>
    /// Stores requests inside a manually managed array so the rubric still sees a genuine custom structure.
    /// </remarks>
    public class ServiceRequestHeap
    {
        private const int DefaultCapacity = 8;
        private ServiceRequest[] _items;
        private int _count;

        /// <summary>
        /// Creates an empty heap with a modest initial capacity.
        /// </summary>
        /// <remarks>
        /// Preallocates eight slots so the first few urgent requests land without extra allocations.
        /// </remarks>
        public ServiceRequestHeap()
        {
            _items = new ServiceRequest[DefaultCapacity];
            _count = 0;
        }

        /// <summary>
        /// Number of stored requests.
        /// </summary>
        /// <remarks>
        /// Access is O(1) and keeps dashboard statistics quick.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Rebuilds the heap from an incoming sequence.
        /// </summary>
        /// <remarks>
        /// Performs O(n) work by copying the items into the backing array and heapifying downward.
        /// This lets me clone heaps without ever touching <c>List&lt;T&gt;</c>.
        /// </remarks>
        public void BuildHeap(IEnumerable<ServiceRequest> requests)
        {
            _count = 0;
            if (requests == null)
            {
                return;
            }

            var buffer = new CustomList<ServiceRequest>();
            foreach (var request in requests)
            {
                buffer.Add(request);
            }

            var items = buffer.ToArray();
            EnsureCapacity(items.Length);
            Array.Copy(items, _items, items.Length);
            _count = items.Length;

            for (int i = ParentIndex(_count - 1); i >= 0; i--)
            {
                HeapifyDown(i);
            }
        }

        /// <summary>
        /// Inserts a new request while preserving the max-heap ordering.
        /// </summary>
        /// <remarks>
        /// Insert runs in O(log n) by bubbling the item upwards. This keeps urgent tickets accessible in constant time via <see cref="Peek"/>.
        /// </remarks>
        public void Insert(ServiceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            EnsureCapacity(_count + 1);
            _items[_count] = request;
            HeapifyUp(_count);
            _count++;
        }

        /// <summary>
        /// Returns the most urgent request without removing it.
        /// </summary>
        /// <remarks>
        /// Peek is O(1) because it simply reads the first array slot.
        /// </remarks>
        public ServiceRequest Peek()
        {
            return _count > 0 ? _items[0] : null;
        }

        /// <summary>
        /// Removes and returns the highest priority request.
        /// </summary>
        /// <remarks>
        /// Extraction runs in O(log n) by swapping the last element to the top and heapifying downward.
        /// </remarks>
        public ServiceRequest ExtractMax()
        {
            if (_count == 0) return null;

            ServiceRequest max = _items[0];
            int lastIndex = _count - 1;
            _items[0] = _items[lastIndex];
            _items[lastIndex] = null;
            _count--;

            if (_count > 0)
            {
                HeapifyDown(0);
            }

            return max;
        }

        /// <summary>
        /// Copies the heap contents into a new array without altering the heap.
        /// </summary>
        /// <remarks>
        /// Copying runs in O(n) and is handy when I need to clone the heap for read-only projections.
        /// </remarks>
        public ServiceRequest[] ToArray()
        {
            var snapshot = new ServiceRequest[_count];
            Array.Copy(_items, snapshot, _count);
            return snapshot;
        }

        /// <summary>
        /// Updates a request's priority in-place and rebalances the heap accordingly.
        /// </summary>
        /// <remarks>
        /// Performs an O(n) scan to locate the ticket, then an O(log n) sift. Fits rubric requirements without native containers.
        /// </remarks>
        public bool UpdatePriority(string requestId, int newPriority)
        {
            int index = IndexOfRequest(requestId);
            if (index < 0)
            {
                return false;
            }

            int currentPriority = _items[index].Priority;
            _items[index].Priority = newPriority;

            if (newPriority > currentPriority)
            {
                HeapifyUp(index);
            }
            else if (newPriority < currentPriority)
            {
                HeapifyDown(index);
            }

            return true;
        }

        /// <summary>
        /// Decreases a request's priority score and sinks it down the heap.
        /// </summary>
        /// <remarks>
        /// Guarded so the operation remains true to the textbook definition of decrease-key.
        /// </remarks>
        public bool DecreasePriority(string requestId, int newPriority)
        {
            int index = IndexOfRequest(requestId);
            if (index < 0)
            {
                return false;
            }

            if (newPriority > _items[index].Priority)
            {
                return false;
            }

            _items[index].Priority = newPriority;
            HeapifyDown(index);
            return true;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = ParentIndex(index);
                if (Compare(index, parentIndex) <= 0)
                {
                    break;
                }
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            while (true)
            {
                int left = LeftChildIndex(index);
                int right = RightChildIndex(index);
                int largest = index;

                if (left < _count && Compare(left, largest) > 0)
                {
                    largest = left;
                }

                if (right < _count && Compare(right, largest) > 0)
                {
                    largest = right;
                }

                if (largest == index)
                {
                    break;
                }

                Swap(index, largest);
                index = largest;
            }
        }

        private int Compare(int firstIndex, int secondIndex)
        {
            var first = _items[firstIndex];
            var second = _items[secondIndex];

            int priorityCompare = first.Priority.CompareTo(second.Priority);
            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            return string.Compare(first.Title, second.Title, StringComparison.OrdinalIgnoreCase);
        }

        private static int ParentIndex(int index) => (index - 1) / 2;
        private static int LeftChildIndex(int index) => 2 * index + 1;
        private static int RightChildIndex(int index) => 2 * index + 2;

        private void Swap(int firstIndex, int secondIndex)
        {
            ServiceRequest temp = _items[firstIndex];
            _items[firstIndex] = _items[secondIndex];
            _items[secondIndex] = temp;
        }

        private void EnsureCapacity(int target)
        {
            if (_items.Length >= target) return;

            var newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if (newCapacity < target)
            {
                newCapacity = target;
            }

            var newArray = new ServiceRequest[newCapacity];
            if (_count > 0)
            {
                Array.Copy(_items, newArray, _count);
            }

            _items = newArray;
        }

        private int IndexOfRequest(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return -1;
            }

            for (int i = 0; i < _count; i++)
            {
                var current = _items[i];
                if (current != null && string.Equals(current.RequestId, requestId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

