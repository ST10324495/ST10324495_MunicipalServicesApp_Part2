using System;
using System.Collections;
using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Ordered key/value list that keeps keys sorted inside paired arrays instead of using <c>SortedList&lt;TKey,TValue&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Keys live in a sorted array with values stored in a matching array. Binary search keeps lookups at O(log n) while inserts shift elements in O(n).
    /// It suits the municipal schedules perfectly because iteration is already chronological.
    /// </remarks>
    public class CustomSortedList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private const int DefaultCapacity = 4;

        private TKey[] _keys;
        private TValue[] _values;
        private int _count;
        private readonly IComparer<TKey> _comparer;

        /// <summary>
        /// Creates an empty sorted list using the default comparer.
        /// </summary>
        /// <remarks>
        /// Provides O(log n) lookups right out of the box for date-ordered municipal events.
        /// </remarks>
        public CustomSortedList()
        {
            _keys = new TKey[DefaultCapacity];
            _values = new TValue[DefaultCapacity];
            _count = 0;
            _comparer = Comparer<TKey>.Default;
        }

        /// <summary>
        /// Creates an empty sorted list that honours a custom comparer.
        /// </summary>
        /// <remarks>
        /// Lets me switch between alphabetical and chronological comparisons without changing the underlying storage.
        /// </remarks>
        public CustomSortedList(IComparer<TKey> comparer)
            : this()
        {
            _comparer = comparer ?? Comparer<TKey>.Default;
        }

        /// <summary>
        /// Number of stored pairs.
        /// </summary>
        /// <remarks>
        /// Count access is O(1) and keeps UI totals responsive.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Gets or sets the value mapped to a key.
        /// </summary>
        /// <remarks>
        /// Uses binary search for lookups, so fetches are O(log n) while updates remain O(1).
        /// </remarks>
        public TValue this[TKey key]
        {
            get
            {
                var index = IndexOf(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException("Key does not exist in this list.");
                }

                return _values[index];
            }
            set
            {
                var index = IndexOf(key);
                if (index >= 0)
                {
                    _values[index] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Determines whether the key already exists.
        /// </summary>
        /// <remarks>
        /// Powered by binary search (O(log n)), which keeps the checks fast for our short schedules.
        /// </remarks>
        public bool ContainsKey(TKey key) => IndexOf(key) >= 0;

        /// <summary>
        /// Inserts a new key/value pair while keeping the list sorted.
        /// </summary>
        /// <remarks>
        /// After finding the insertion slot (O(log n)) it shifts following items (O(n)). With our small data sets that performs well.
        /// </remarks>
        public void Add(TKey key, TValue value)
        {
            EnsureCapacity(_count + 1);

            var index = _count;
            while (index > 0 && _comparer.Compare(_keys[index - 1], key) > 0)
            {
                _keys[index] = _keys[index - 1];
                _values[index] = _values[index - 1];
                index--;
            }

            _keys[index] = key;
            _values[index] = value;
            _count++;
        }

        /// <summary>
        /// Attempts to locate the value for the supplied key.
        /// </summary>
        /// <remarks>
        /// Binary search keeps this at O(log n) and avoids exceptions when a key is optional.
        /// </remarks>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = _values[index];
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Enumerates keys in sorted order.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n). The UI binds directly to keep dropdowns ordered.
        /// </remarks>
        public IEnumerable<TKey> Keys
        {
            get
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _keys[i];
                }
            }
        }

        /// <summary>
        /// Enumerates the stored values following the sorted key order.
        /// </summary>
        /// <remarks>
        /// Also O(n); I use it to flatten event groups into the grids without extra sorting.
        /// </remarks>
        public IEnumerable<TValue> Values
        {
            get
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _values[i];
                }
            }
        }

        /// <summary>
        /// Removes every entry but keeps the allocated buffers ready for reuse.
        /// </summary>
        /// <remarks>
        /// Runs in O(n) due to array clearing. It happens only when reseeding sample data.
        /// </remarks>
        public void Clear()
        {
            Array.Clear(_keys, 0, _count);
            Array.Clear(_values, 0, _count);
            _count = 0;
        }

        /// <summary>
        /// Iterates key/value pairs in sorted order.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n) and drives reports that walk through the municipal schedule.
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int IndexOf(TKey key)
        {
            var low = 0;
            var high = _count - 1;

            while (low <= high)
            {
                var mid = (low + high) / 2;
                var comparison = _comparer.Compare(_keys[mid], key);

                if (comparison == 0)
                {
                    return mid;
                }

                if (comparison < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return -1;
        }

        private void EnsureCapacity(int target)
        {
            if (_keys.Length >= target) return;

            var newCapacity = _keys.Length == 0 ? DefaultCapacity : _keys.Length * 2;
            if (newCapacity < target)
            {
                newCapacity = target;
            }

            var newKeys = new TKey[newCapacity];
            var newValues = new TValue[newCapacity];

            if (_count > 0)
            {
                Array.Copy(_keys, newKeys, _count);
                Array.Copy(_values, newValues, _count);
            }

            _keys = newKeys;
            _values = newValues;
        }
    }
}

