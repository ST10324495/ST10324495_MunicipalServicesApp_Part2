using System;
using System.Collections;
using System.Collections.Generic;

namespace ST10323395_MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Array-backed stack used to track recent municipal actions without depending on <c>Stack&lt;T&gt;</c>.
    /// </summary>
    /// <remarks>
    /// Internally stores items in a resizable array, so pushes and pops run in amortized O(1).
    /// The structure powers “recently viewed” lists in the UI where LIFO behaviour matches user expectations.
    /// </remarks>
    public class CustomStack<T> : IEnumerable<T>
    {
        private const int DefaultCapacity = 4;
        private T[] _items;
        private int _count;

        /// <summary>
        /// Creates an empty stack with a small initial capacity.
        /// </summary>
        /// <remarks>
        /// Starts with four slots because municipal demo interactions stay small, keeping allocations low while preserving O(1) pushes.
        /// </remarks>
        public CustomStack()
        {
            _items = new T[DefaultCapacity];
            _count = 0;
        }

        /// <summary>
        /// Number of items currently stored on the stack.
        /// </summary>
        /// <remarks>
        /// Count access is O(1) thanks to the maintained field, which keeps UI counters cheap.
        /// </remarks>
        public int Count => _count;

        /// <summary>
        /// Places a new item on the top of the stack.
        /// </summary>
        /// <remarks>
        /// Uses a dynamic array so the amortized cost stays O(1); perfect for recording every previewed event or request.
        /// </remarks>
        public void Push(T item)
        {
            EnsureCapacity(_count + 1);
            _items[_count++] = item;
        }

        /// <summary>
        /// Removes and returns the most recent item.
        /// </summary>
        /// <remarks>
        /// Pop is O(1) because it simply decrements the count. I use it when unwinding temporary stacks during deduplication.
        /// </remarks>
        public T Pop()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Cannot pop from an empty stack.");
            }

            var index = --_count;
            var value = _items[index];
            _items[index] = default(T);
            return value;
        }

        /// <summary>
        /// Returns the most recent item without removing it.
        /// </summary>
        /// <remarks>
        /// Peek is O(1) since it just reads the last array slot, letting the UI preview the latest action instantly.
        /// </remarks>
        public T Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Cannot peek an empty stack.");
            }

            return _items[_count - 1];
        }

        /// <summary>
        /// Removes every item while keeping the allocated buffer for reuse.
        /// </summary>
        /// <remarks>
        /// Clearing runs in O(n) because it wipes the array, but this only happens when reloading demo scenarios between rubric checks.
        /// </remarks>
        public void Clear()
        {
            if (_count == 0) return;

            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        /// <summary>
        /// Reports whether any element matches the supplied predicate.
        /// </summary>
        /// <remarks>
        /// Performs a linear scan (O(n)), which is fine for short interaction histories such as recently viewed events.
        /// </remarks>
        public bool Any(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            for (int i = 0; i < _count; i++)
            {
                if (predicate(_items[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Copies the contents to an array ordered from top to bottom.
        /// </summary>
        /// <remarks>
        /// The copy is O(n) and gives WinForms controls a familiar structure when they expect an array.
        /// </remarks>
        public T[] ToArray()
        {
            var result = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _items[_count - 1 - i];
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
            if (_count > 0)
            {
                Array.Copy(_items, newArray, _count);
            }

            _items = newArray;
        }

        /// <summary>
        /// Iterates from the top of the stack downwards.
        /// </summary>
        /// <remarks>
        /// Enumeration runs in O(n) and is handy when I need to rebuild the stack after temporary adjustments.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _count - 1; i >= 0; i--)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

