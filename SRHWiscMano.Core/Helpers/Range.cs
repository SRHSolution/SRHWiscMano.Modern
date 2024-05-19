namespace SRHWiscMano.Core.Helpers
{
    public static class Range
    {
        public static Range<T> Create<T>(T value1, T value2) where T : IEquatable<T>, IComparable<T> => new(value1, value2);
    }

    /// <summary>
    /// Range는 [,) 범위를 갖는다. 영역은 start를 포함, end는 포함하지 않는다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Range<T> : IEquatable<Range<T>> where T : IEquatable<T>, IComparable<T>
    {
        private readonly T start;
        private readonly T end;

        
        public Range(T start, T end)
        {
            this.start = start;
            this.end = end;
        }

        public Range(Range<T> other)
          : this(other.start, other.end)
        {
        }

        public T Start => this.start;

        public T End => this.end;

        /// <summary>
        /// start, end 중에 작은 숫자를 찾는다.
        /// </summary>
        public T Lesser => !this.IsForward ? this.end : this.start;

        /// <summary>
        /// start, end 중에 큰 숫자를 찾는다
        /// </summary>
        public T Greater => !this.IsForward ? this.start : this.end;

        /// <summary>
        /// start가 end보다 작으면, 정방향인지 확인
        /// </summary>
        public bool IsForward => this.start.CompareTo(this.end) <= 0;

        public static bool operator ==(Range<T> range1, Range<T> range2) => Range<T>.CheckEquals(range1, range2);

        public static bool operator !=(Range<T> range1, Range<T> range2) => !Range<T>.CheckEquals(range1, range2);

        public override bool Equals(object obj) => Range<T>.CheckEquals(this, obj as Range<T>);

        public bool Equals(Range<T> other) => Range<T>.CheckEquals(this, other);

        public override int GetHashCode() => (int)HashCode.Compose<T>(this.Start).And<T>(this.End);

        public override string ToString() => string.Format("{0}, {1}", (object)this.Start, (object)this.End);

        public bool Contains(T value) => this.Contains(value, true, false);

        public bool Contains(T value, bool inclusiveStart, bool inclusiveEnd)
        {
            bool flag1 = inclusiveStart ? this.Start.CompareTo(value) <= 0 : this.Start.CompareTo(value) < 0;
            bool flag2 = inclusiveEnd ? value.CompareTo(this.End) <= 0 : value.CompareTo(this.End) < 0;
            return flag1 && flag2;
        }

        public bool Overlaps(Range<T> other) => this.Overlaps(other, true);

        public bool Overlaps(Range<T> other, bool inclusive) => this.Contains(other.Start, inclusive, false) || this.Contains(other.End, inclusive, false) || other.Contains(this.Start, inclusive, false) || other.Contains(this.End, inclusive, false) || this.Equals(other);

        public Range<T> Intersection(Range<T> other)
        {
            T lesser1 = this.Lesser;
            T greater1 = this.Greater;
            T lesser2 = other.Lesser;
            T greater2 = other.Greater;
            if (lesser2.CompareTo(greater1) >= 0 || greater2.CompareTo(lesser1) <= 0)
                return (Range<T>)null;
            T obj1 = lesser2.CompareTo(lesser1) < 0 ? lesser1 : lesser2;
            T obj2 = greater2.CompareTo(greater1) > 0 ? greater1 : greater2;
            return !this.IsForward ? new Range<T>(obj2, obj1) : new Range<T>(obj1, obj2);
        }

        public Range<T> Union(Range<T> other)
        {
            T obj1 = this.Lesser.CompareTo(other.Lesser) <= 0 ? this.Lesser : other.Lesser;
            T obj2 = this.Greater.CompareTo(other.Greater) > 0 ? this.Greater : other.Greater;
            return !this.IsForward ? new Range<T>(obj2, obj1) : new Range<T>(obj1, obj2);
        }

        public Range<T> Reverse() => new(this.end, this.start);

        public Range<TResult> Map<TResult>(Func<T, TResult> selector) where TResult : IEquatable<TResult>, IComparable<TResult> => new(selector(this.start), selector(this.end));

        private static bool CheckEquals(Range<T> range1, Range<T> range2)
        {
            if (object.ReferenceEquals((object)range1, (object)range2))
                return true;
            return !object.ReferenceEquals((object)range1, (object)null) && !object.ReferenceEquals((object)range2, (object)null) && range1.Start.Equals(range2.Start) && range1.End.Equals(range2.End);
        }
    }
}
