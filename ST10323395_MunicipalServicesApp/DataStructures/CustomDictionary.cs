using System;
using System.Collections;
using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Array-backed dictionary that keeps key/value pairs aligned without relying on <c>Dictionary&lt;TKey,TValue&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Keys and values are stored in parallel arrays, giving O(n) lookups but O(1) index-based updates.
    /// The structure covers small mappings such as relationships and recommendation scores inside the municipal system.
    /// </remarks>
    public class CustomDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private const int DefaultCapacity = 4;

        private TKey[] _keys;
        private TValue[] _values;
        private int _count;

        /// <summary>
        /// Creates an empty dictionary with a modest starting capacity.
        /// </summary>
        /// <remarks>
        /// The fixed-size arrays grow on demand, keeping inserts amortized O(1) for the small datasets we manage.
        /// </remarks>
        public CustomDictionary()
        {
            _keys = new TKey[DefaultCapacity];
            _values = new TValue[DefaultCapacity];
            _count = 0;
        }

        /// <summary>
        /// Number of key/value pairs currently stored.
        /// </summary>
        /// <remarks>
        /// Access is O(1) since the count is tracked as items are added or removed.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <remarks>
        /// Lookups perform a linear scan (O(n)). That trade-off is acceptable for the small mappings in this project.
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
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Adds a new key/value pair to the dictionary.
        /// </summary>
        /// <remarks>
        /// Insertions run in amortized O(1) thanks to the dynamic capacity growth; lookups during duplication checks are O(n).
        /// </remarks>
        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException("The provided key already exists in this dictionary.");
            }

            EnsureCapacity(_count + 1);
            _keys[_count] = key;
            _values[_count] = value;
            _count++;
        }

        /// <summary>
        /// Attempts to retrieve a value without raising exceptions for missing keys.
        /// </summary>
        /// <remarks>
        /// Performs an O(n) scan. It keeps the code tidy whenever I simply need to know whether a related record exists.
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
        /// Checks whether the dictionary contains a given key.
        /// </summary>
        /// <remarks>
        /// Runs in O(n). The small datasets tracked here (departments, recommendations) make that overhead negligible.
        /// </remarks>
        public bool ContainsKey(TKey key)
        {
            return IndexOf(key) >= 0;
        }

        /// <summary>
        /// Removes a key/value pair if present.
        /// </summary>
        /// <remarks>
        /// Removal involves shifting the tail of the arrays, so it is O(n). It is still cheap given the limited data sizes.
        /// </remarks>
        public bool Remove(TKey key)
        {
            var index = IndexOf(key);
            if (index < 0)
            {
                return false;
            }

            _count--;
            if (index < _count)
            {
                Array.Copy(_keys, index + 1, _keys, index, _count - index);
                Array.Copy(_values, index + 1, _values, index, _count - index);
            }

            _keys[_count] = default(TKey);
            _values[_count] = default(TValue);
            return true;
        }

        /// <summary>
        /// Clears all entries while keeping the buffer for reuse.
        /// </summary>
        /// <remarks>
        /// The clear operation is O(n) due to array wipes, and is used when resetting the sample municipal data.
        /// </remarks>
        public void Clear()
        {
            Array.Clear(_keys, 0, _count);
            Array.Clear(_values, 0, _count);
            _count = 0;
        }

        /// <summary>
        /// Returns an enumerable view over the stored keys.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n) and feeds combo-boxes that present friendly category names.
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
        /// Returns an enumerable view over the stored values.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n). I lean on it whenever a form wants all related records for rendering.
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
        /// Iterates over the key/value pairs.
        /// </summary>
        /// <remarks>
        /// Enumeration runs in O(n) and feeds higher-level loops that need both keys and values together.
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
            var comparer = EqualityComparer<TKey>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(_keys[i], key))
                {
                    return i;
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

            Resize(newCapacity);
        }

        private void Resize(int newCapacity)
        {
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

