using System.Collections.Generic;

namespace AdvancedStatsAndEffects
{
    public struct HashCode
    {
        private readonly int _value;

        public HashCode(int value)
        {
            _value = value;
        }

        public static HashCode Start { get; } = new HashCode(17);

        public static implicit operator int(HashCode hash)
        {
            return hash._value;
        }

        public HashCode Hash<T>(T obj)
        {
            var h = EqualityComparer<T>.Default.GetHashCode(obj);
            return unchecked(new HashCode((_value * 31) + h));
        }

        public override int GetHashCode()
        {
            return _value;
        }
    }

}
