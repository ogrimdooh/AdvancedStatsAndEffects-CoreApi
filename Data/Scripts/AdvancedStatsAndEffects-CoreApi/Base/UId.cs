using ProtoBuf;

namespace AdvancedStatsAndEffects
{
    [ProtoContract]
    public abstract class UId<T, K>
    {

        private UInt128 uniqueId;
        public UInt128 UniqueId
        {
            get
            {
                return GetUniqueId();
            }
        }

        public string Value
        {
            get
            {
                return string.Format("{0}-{1}", typeId, subtypeId);
            }
        }

        [ProtoMember(1)]
        public readonly T typeId;

        [ProtoMember(2)]
        public readonly K subtypeId;

        public UId()
        {

        }

        public UId(T typeId, K subtypeId)
        {
            this.typeId = typeId;
            this.subtypeId = subtypeId;
            uniqueId = UInt128.Zero;
        }

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return GetUniqueId().GetHashCode();
        }

        public UInt128 GetUniqueId()
        {
            if (uniqueId == UInt128.Zero)
                uniqueId = new UInt128(HashCode.Start.Hash(typeId), HashCode.Start.Hash(subtypeId));
            return uniqueId;
        }

    }

}
