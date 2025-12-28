using System;
using System.Collections;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public sealed class LazyVmList<TSource, TVm> : IList, IReadOnlyList<TVm>
        where TSource : class
        where TVm : class
    {
        private readonly IReadOnlyList<TSource> _source;
        private readonly Func<TSource, TVm> _factory;
        private readonly TVm?[] _cache;

        public LazyVmList(IReadOnlyList<TSource> source, Func<TSource, TVm> factory)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _cache = new TVm?[source.Count];
        }

        public int Count => _source.Count;

        public TVm this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_source.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var existing = _cache[index];
                if (existing is not null)
                    return existing;

                var created = _factory(_source[index]);
                _cache[index] = created;
                return created;
            }
        }

        object? IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public IEnumerator<TVm> GetEnumerator()
        {
            for (var i = 0; i < _source.Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsReadOnly => true;
        bool IList.IsReadOnly => true;

        public bool IsFixedSize => true;
        bool IList.IsFixedSize => true;

        public int Add(object? value) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public void Insert(int index, object? value) => throw new NotSupportedException();
        public void Remove(object? value) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();

        public bool Contains(object? value)
        {
            if (value is not TVm vm)
                return false;

            for (var i = 0; i < _cache.Length; i++)
            {
                if (ReferenceEquals(_cache[i], vm))
                    return true;
            }

            return false;
        }

        public int IndexOf(object? value)
        {
            if (value is not TVm vm)
                return -1;

            for (var i = 0; i < _cache.Length; i++)
            {
                if (ReferenceEquals(_cache[i], vm))
                    return i;
            }

            return -1;
        }

        public void CopyTo(Array array, int index)
        {
            for (var i = 0; i < Count; i++)
                array.SetValue(this[i], index + i);
        }

        public bool IsSynchronized => false;
        public object SyncRoot => this;
    }
}
