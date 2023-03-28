using ProtoBuf;
using System;

namespace AdvancedStatsAndEffects
{
    [ProtoContract]
    public class UniqueNameId : UId<string, string>, IEquatable<UniqueNameId>
    {

        public static readonly UniqueNameId Empty = new UniqueNameId();

        public static UniqueNameId Parse(string id)
        {
            UniqueNameId output;
            if (!TryParse(id, out output))
                throw new ArgumentException("The provided type does not conform to a entity ID.", "id");
            return output;
        }

        public static bool TryParse(string id, out UniqueNameId definitionId)
        {
            if (string.IsNullOrEmpty(id))
            {
                definitionId = new UniqueNameId();
                return false;
            }

            var slashIndex = id.IndexOf('-');
            if (slashIndex == -1)
            {
                definitionId = new UniqueNameId();
                return false;
            }
            var typeId = id.Substring(0, slashIndex).Trim();
            var subtypeId = id.Substring(slashIndex + 1).Trim();
            if (subtypeId == "(null)")
                subtypeId = null;
            definitionId = new UniqueNameId(typeId, subtypeId);
            return true;
        }

        public UniqueNameId()
            : base()
        {

        }

        public UniqueNameId(string typeId, string subtypeId)
            : base(typeId, subtypeId)
        {
        }

        public bool Equals(UniqueNameId other)
        {
            return typeId == other.typeId && subtypeId == other.subtypeId;
        }

        public override bool Equals(object obj)
        {
            return (obj is UniqueNameId) && Equals((UniqueNameId)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(UniqueNameId l, UniqueNameId r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(UniqueNameId l, UniqueNameId r)
        {
            return !l.Equals(r);
        }

    }

}
