using System;
using System.Collections;
using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Circular-buffer queue that keeps municipal interactions in first-in-first-out order without <c>Queue&lt;T&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Internally stores elements in a ring buffer (array plus head/tail indices) so enqueues and dequeues stay O(1).
    /// It drives features like “recent searches” where order matters but built-in collections are off limits.
    /// </remarks>
    public class CustomQueue<T> : IEnumerable<T>
    {
        private const int DefaultCapacity = 4;
        private T[] _items;
        private int _head;
        private int _tail;
        private int _count;

        /// <summary>
        /// Builds an empty queue ready to buffer user actions.
        /// </summary>
        /// <remarks>
        /// Preallocates a small array so the first few enqueues are O(1) without additional work.
        /// </remarks>
        public CustomQueue()
        {
            _items = new T[DefaultCapacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// Current number of entries waiting in the queue.
        /// </summary>
        /// <remarks>
        /// Count is maintained eagerly, so reading it is O(1). The UI uses it to cap search history.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Adds a new item to the tail of the queue.
        /// </summary>
        /// <remarks>
        /// Enqueue is amortized O(1) thanks to the ring buffer, keeping user-entered search terms flowing smoothly.
        /// </remarks>
        public void Enqueue(T item)
        {
            EnsureCapacity(_count + 1);
            _items[_tail] = item;
            _tail = (_tail + 1) % _items.Length;
            _count++;
        }

        /// <summary>
        /// Removes and returns the item at the head of the queue.
        /// </summary>
        /// <remarks>
        /// Dequeue is O(1) because it simply advances the head index. Perfect for consuming notifications in order.
        /// </remarks>
        public T Dequeue()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Cannot dequeue from an empty queue.");
            }

            var value = _items[_head];
            _items[_head] = default(T);
            _head = (_head + 1) % _items.Length;
            _count--;
            return value;
        }

        /// <summary>
        /// Peeks at the head item without removing it.
        /// </summary>
        /// <remarks>
        /// Peek is O(1) and allows quick previews, for example when the UI displays the next pending search.
        /// </remarks>
        public T Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Cannot peek an empty queue.");
            }

            return _items[_head];
        }

        /// <summary>
        /// Clears all items while retaining the allocated buffer.
        /// </summary>
        /// <remarks>
        /// Clearing is O(n) because the array is reset, but that cost only appears when reloading demo data.
        /// </remarks>
        public void Clear()
        {
            if (_count == 0) return;

            Array.Clear(_items, 0, _items.Length);
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// Checks whether the queue contains a specific item.
        /// </summary>
        /// <remarks>
        /// Runs in O(n) via enumeration, which is acceptable for short municipal search histories.
        /// </remarks>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            foreach (var entry in this)
            {
                if (comparer.Equals(entry, item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Copies the queue into a new array ordered from oldest to newest entry.
        /// </summary>
        /// <remarks>
        /// The copy is O(n). WinForms data-binding occasionally insists on arrays, so this keeps that integration tidy.
        /// </remarks>
        public T[] ToArray()
        {
            var result = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _items[(_head + i) % _items.Length];
            }

            return result;
        }

        private void EnsureCapacity(int target)
        {
            if (_items.Length >= target) return;

            var newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if (newCapacity < target)
            {
                newCapacity = target;
            }

            var newArray = new T[newCapacity];

            for (int i = 0; i < _count; i++)
            {
                newArray[i] = _items[(_head + i) % _items.Length];
            }

            _items = newArray;
            _head = 0;
            _tail = _count % _items.Length;
        }

        /// <summary>
        /// Iterates through the queue in FIFO order.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n) and is used whenever I need to rebuild the queue after trimming duplicates.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[(_head + i) % _items.Length];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

