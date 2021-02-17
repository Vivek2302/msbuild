using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Build.Collections
{
    /// <summary>
    /// Small, lightweight, read-only IDictionary implementation using two arrays
    /// and O(n) lookup. Requires specifying capacity at construction and does not
    /// support reallocation to increase capacity.
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TValue">Type of values</typeparam>
    internal class SmallDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private TKey[] keys;
        private TValue[] values;

        private const int maxCapacity = 16;

        private int count;

        public SmallDictionary(int capacity)
        {
            keys = new TKey[capacity];
            values = new TValue[capacity];
        }

        public static IDictionary<TKey, TValue> Create(int capacity)
        {
            if (capacity <= maxCapacity)
            {
                return new SmallDictionary<TKey, TValue>(capacity);
            }
            else
            {
                return new Dictionary<TKey, TValue>(capacity);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TryGetValue(key, out var value);
                return value;
            }

            set
            {
                var comparer = KeyComparer;
                for (int i = 0; i < count; i++)
                {
                    if (comparer.Equals(key, keys[i]))
                    {
                        values[i] = value;
                        return;
                    }
                }

                Add(key, value);
            }
        }

        public ICollection<TKey> Keys => keys;

        public ICollection<TValue> Values => values;

        private IEqualityComparer<TKey> KeyComparer => EqualityComparer<TKey>.Default;

        private IEqualityComparer<TValue> ValueComparer => EqualityComparer<TValue>.Default;

        public int Count => keys.Length;

        public bool IsReadOnly => true;

        public void Add(TKey key, TValue value)
        {
            if (count < keys.Length)
            {
                keys[count] = key;
                values[count] = value;
                count += 1;
            }
            else
            {
                throw new InvalidOperationException($"SmallDictionary is at capacity {keys.Length}");
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var keyComparer = KeyComparer;
            var valueComparer = ValueComparer;
            for (int i = 0; i < count; i++)
            {
                if (keyComparer.Equals(item.Key, keys[i]) && valueComparer.Equals(item.Value, values[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            var comparer = KeyComparer;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(key, keys[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            for (int i = 0; i < count; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        public bool Remove(TKey key)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var comparer = KeyComparer;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(key, keys[i]))
                {
                    value = values[i];
                    return true;
                }
            }

            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly SmallDictionary<TKey, TValue> _dictionary;
            private int _position;

            public Enumerator(SmallDictionary<TKey, TValue> dictionary)
            {
                this._dictionary = dictionary;
                this._position = -1;
            }

            public KeyValuePair<TKey, TValue> Current =>
                new KeyValuePair<TKey, TValue>(
                    _dictionary.keys[_position],
                    _dictionary.values[_position]);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                _position += 1;
                return _position < _dictionary.Count;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}