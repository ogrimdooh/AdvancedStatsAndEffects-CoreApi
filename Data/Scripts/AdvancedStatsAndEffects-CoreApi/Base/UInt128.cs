using ProtoBuf;
using System;

namespace AdvancedStatsAndEffects
{

    [ProtoContract]
    public struct UInt128 : IEquatable<UInt128>
    {

        public static readonly UInt128 Zero = new UInt128(0, 0);

        public static UInt128 Random()
        {
            return Guid.NewGuid().ToUInt128();
        }

        public Guid Id
        {
            get
            {
                return this.ToGuid();
            }
        }

        public string Value
        {
            get
            {
                return Id.ToString();
            }
        }

        [ProtoMember(1)]
        public long p1;

        [ProtoMember(2)]
        public long p2;

        public UInt128(long p1, long p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public bool Equals(UInt128 other)
        {
            return p1 == other.p1 && p2 == other.p2;
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(p1)
                .Hash(p2);
        }

        public override bool Equals(object obj)
        {
            return (obj is UInt128) && Equals((UInt128)obj);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(UInt128 l, UInt128 r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(UInt128 l, UInt128 r)
        {
            return !l.Equals(r);
        }

    }

}
