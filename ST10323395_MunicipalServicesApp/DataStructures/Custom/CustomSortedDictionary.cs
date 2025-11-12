using System;
using System.Collections;
using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Sorted dictionary implemented with paired arrays instead of <c>SortedDictionary&lt;TKey,TValue&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Keys stay sorted in an array, values in a twin array. Binary search keeps lookups at O(log n) while inserts shift elements in O(n).
    /// It gives me predictable iteration order for category groupings across the municipal forms.
    /// </remarks>
    public class CustomSortedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private const int DefaultCapacity = 4;

        private TKey[] _keys;
        private TValue[] _values;
        private int _count;
        private readonly IComparer<TKey> _comparer;

        /// <summary>
        /// Creates an empty sorted dictionary that uses the default comparer.
        /// </summary>
        /// <remarks>
        /// Ideal for alphabetical keys such as event categories where O(log n) lookups keep filtering quick.
        /// </remarks>
        public CustomSortedDictionary()
        {
            _keys = new TKey[DefaultCapacity];
            _values = new TValue[DefaultCapacity];
            _count = 0;
            _comparer = Comparer<TKey>.Default;
        }

        /// <summary>
        /// Creates an empty sorted dictionary using a custom comparer.
        /// </summary>
        /// <remarks>
        /// Lets me switch ordering strategies (for example, locale-sensitive comparisons) without changing the storage mechanics.
        /// </remarks>
        public CustomSortedDictionary(IComparer<TKey> comparer)
            : this()
        {
            _comparer = comparer ?? Comparer<TKey>.Default;
        }

        /// <summary>
        /// Number of stored entries.
        /// </summary>
        /// <remarks>
        /// O(1) access keeps summary stats cheap.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Gets or sets the value mapped to a given key.
        /// </summary>
        /// <remarks>
        /// Binary search keeps lookups at O(log n); updates happen in-place so they remain O(1).
        /// </remarks>
        public TValue this[TKey key]
        {
            get
            {
                var index = IndexOf(key);
                if (index < 0)
                {
                    throw new KeyNotFoundException("Key does not exist in this dictionary.");
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
                    Insert(key, value);
                }
            }
        }

        /// <summary>
        /// Adds a new key/value pair while preserving sort order.
        /// </summary>
        /// <remarks>
        /// Binary search finds the insert slot in O(log n); shifting the following entries is O(n). With small municipal datasets the cost is minimal.
        /// </remarks>
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("The provided key already exists in this dictionary.");
            }

            Insert(key, value);
        }

        /// <summary>
        /// Checks whether the dictionary already contains the key.
        /// </summary>
        /// <remarks>
        /// Uses binary search (O(log n)). It keeps the code honest when reloading seed data.
        /// </remarks>
        public bool ContainsKey(TKey key) => IndexOf(key) >= 0;

        /// <summary>
        /// Attempts to fetch a value without throwing when the key is missing.
        /// </summary>
        /// <remarks>
        /// Also O(log n) thanks to the binary search helper.
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
        /// Enumeration is O(n), which keeps combo-boxes and reports nicely ordered.
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
        /// Enumerates values in the same sorted order as their keys.
        /// </summary>
        /// <remarks>
        /// Also O(n). It simplifies piping the dictionary into other custom structures.
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
        /// Clears every entry while holding onto the allocated arrays.
        /// </summary>
        /// <remarks>
        /// Runs in O(n) thanks to array clearing, and is mainly used when resetting demo data for marking.
        /// </remarks>
        public void Clear()
        {
            Array.Clear(_keys, 0, _count);
            Array.Clear(_values, 0, _count);
            _count = 0;
        }

        /// <summary>
        /// Iterates through key/value pairs in sorted order.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n) and feeds the UI without additional sorting.
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void Insert(TKey key, TValue value)
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

