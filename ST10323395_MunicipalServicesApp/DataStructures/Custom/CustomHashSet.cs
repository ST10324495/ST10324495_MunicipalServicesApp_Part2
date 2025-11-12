using System;
using System.Collections;
using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Open-addressed hash set that keeps municipal values unique without <c>HashSet&lt;T&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Uses linear probing over an array of slots. Average insert, lookup, and remove operations stay close to O(1),
    /// which works well for the modest data sets in this application (categories, keywords, and visited nodes).
    /// </remarks>
    public class CustomHashSet<T> : IEnumerable<T>
    {
        private const int DefaultCapacity = 7;
        private const float LoadFactorThreshold = 0.7f;

        private Slot[] _slots;
        private int _count;

        /// <summary>
        /// Builds an empty hash set with a small prime capacity.
        /// </summary>
        /// <remarks>
        /// Starting with seven slots keeps clustering low while still fitting the lightweight datasets we manage for municipal services.
        /// </remarks>
        public CustomHashSet()
        {
            _slots = new Slot[DefaultCapacity];
            _count = 0;
        }

        /// <summary>
        /// Copies the contents into a new array.
        /// </summary>
        /// <remarks>
        /// Copying runs in O(n) and is mainly used when a WinForms control insists on array data.
        /// </remarks>
        public T[] ToArray()
        {
            var result = new T[_count];
            var index = 0;

            foreach (var value in this)
            {
                result[index++] = value;
            }

            return result;
        }

        /// <summary>
        /// Number of elements currently stored.
        /// </summary>
        /// <remarks>
        /// Reading the count is O(1) and keeps stats displays quick.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Adds a value to the set if it does not already exist.
        /// </summary>
        /// <remarks>
        /// Performs an average O(1) probe sequence thanks to the load factor guard.
        /// I use it to avoid duplicate categories and keyword suggestions inside the municipal UI.
        /// </remarks>
        public bool Add(T item)
        {
            if (NeedsResize())
            {
                Resize();
            }

            return TryInsert(item, _slots);
        }

        /// <summary>
        /// Checks whether the set already contains the supplied value.
        /// </summary>
        /// <remarks>
        /// Average-case O(1) probe length. This keeps duplicate suppression cheap when reseeding sample data.
        /// </remarks>
        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            var hash = GetBucket(item, _slots.Length);

            for (int i = 0; i < _slots.Length; i++)
            {
                var index = (hash + i) % _slots.Length;
                ref var slot = ref _slots[index];

                if (!slot.Occupied)
                {
                    return false;
                }

                if (slot.HashCode == hash && comparer.Equals(slot.Value, item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes every stored value and resets the capacity.
        /// </summary>
        /// <remarks>
        /// Clear runs in O(n) due to the array reallocation, which is fine during scenario resets.
        /// </remarks>
        public void Clear()
        {
            _slots = new Slot[DefaultCapacity];
            _count = 0;
        }

        private bool NeedsResize()
        {
            return (_count + 1f) / _slots.Length > LoadFactorThreshold;
        }

        private void Resize()
        {
            var newSlots = new Slot[_slots.Length * 2 + 1];
            _count = 0;

            foreach (var slot in _slots)
            {
                if (slot.Occupied)
                {
                    TryInsert(slot.Value, newSlots);
                }
            }

            _slots = newSlots;
        }

        private bool TryInsert(T item, Slot[] slots)
        {
            var comparer = EqualityComparer<T>.Default;
            var hash = GetBucket(item, slots.Length);

            for (int i = 0; i < slots.Length; i++)
            {
                var index = (hash + i) % slots.Length;
                ref var slot = ref slots[index];

                if (!slot.Occupied)
                {
                    slot.Value = item;
                    slot.HashCode = hash;
                    slot.Occupied = true;
                    _count++;
                    return true;
                }

                if (slot.HashCode == hash && comparer.Equals(slot.Value, item))
                {
                    return false;
                }
            }

            return false;
        }

        private static int GetBucket(T item, int size)
        {
            var hashCode = item?.GetHashCode() ?? 0;
            hashCode &= 0x7fffffff;
            return hashCode % size;
        }

        /// <summary>
        /// Iterates over the stored values in slot order.
        /// </summary>
        /// <remarks>
        /// Enumeration is O(n) and is handy when binding unique categories back to the forms.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var slot in _slots)
            {
                if (slot.Occupied)
                {
                    yield return slot.Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private struct Slot
        {
            public int HashCode;
            public T Value;
            public bool Occupied;
        }
    }
}

