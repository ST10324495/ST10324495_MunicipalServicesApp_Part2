using System;
using System.Collections;
using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Growable array-backed list used to keep municipal records in insertion order without relying on <c>List&lt;T&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Items are stored inside a resizable array so indexed lookups and assignments are O(1), while appends are amortized O(1).
    /// The structure is used throughout the app to hold ordered collections such as events, search history, and service requests.
    /// </remarks>
    public class CustomList<T> : IEnumerable<T>
    {
        private const int DefaultCapacity = 4;
        private T[] _items;
        private int _count;

        ///<summary>
        /// Creates an empty list with the default starting capacity.
        /// </summary>
        /// <remarks>
        /// Starts with a small array so everyday municipal collections grow gradually while keeping appends amortized O(1).
        /// </remarks>
        public CustomList()
        {
            _items = new T[DefaultCapacity];
            _count = 0;
        }

        /// <summary>
        /// Creates a list with an explicit starting capacity.
        /// </summary>
        /// <remarks>
        /// Capacity control helps when I know roughly how many municipal items will be loaded up front, keeping resizing to O(1).
        /// </remarks>
        public CustomList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative.");
            }

            _items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
            _count = 0;
        }

        /// <summary>
        /// Current number of stored elements.
        /// </summary>
        /// <remarks>
        /// Retrieving the count is O(1) because it is maintained as items are added or removed.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Provides indexed access to stored elements.
        /// </summary>
        /// <remarks>
        /// Direct indexing is O(1) because the underlying storage is an array. It keeps the UI responsive when showing event details by position.
        /// </remarks>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for this list.");
                }

                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for this list.");
                }

                _items[index] = value;
            }
        }

        /// <summary>
        /// Appends a new element to the end of the list.
        /// </summary>
        /// <remarks>
        /// Because the structure grows its array by doubling, repeated adds stay amortized O(1). That keeps batch event imports fast.
        /// </remarks>
        public void Add(T item)
        {
            EnsureCapacity(_count + 1);
            _items[_count++] = item;
        }

        /// <summary>
        /// Appends every element from another sequence.
        /// </summary>
        /// <remarks>
        /// Each add is still amortized O(1). I use it when copying pre-seeded municipal data into the UI.
        /// </remarks>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Checks whether the list already contains a value.
        /// </summary>
        /// <remarks>
        /// Performs a linear scan (O(n)) which is acceptable for the relatively small collections displayed on each form.
        /// </remarks>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(_items[i], item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all elements while keeping the allocated buffer for reuse.
        /// </summary>
        /// <remarks>
        /// Clearing is O(n) due to the array wipe, but that cost only appears when resetting demo data between rubric checks.
        /// </remarks>
        public void Clear()
        {
            if (_count == 0) return;

            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        /// <summary>
        /// Copies the contents into a brand new array.
        /// </summary>
        /// <remarks>
        /// Copying runs in O(n). I fall back to arrays when a WinForms control insists on array input rather than custom enumerables.
        /// </remarks>
        public T[] ToArray()
        {
            var result = new T[_count];
            Array.Copy(_items, result, _count);
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
            if (_count > 0)
            {
                Array.Copy(_items, newArray, _count);
            }

            _items = newArray;
        }

        /// <summary>
        /// Returns an iterator that walks the list in insertion order.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n) and keeps the display logic simple when binding municipal data to UI controls.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

