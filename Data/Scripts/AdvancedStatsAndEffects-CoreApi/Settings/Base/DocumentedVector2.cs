using ProtoBuf;
using System.Xml;
using System.Xml.Serialization;

namespace AdvancedStatsAndEffects
{
    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class DocumentedVector2
    {

        [XmlAttribute("info")]
        public string Info { get; set; }
        [XmlElement]
        public float X { get; set; }
        [XmlElement]
        public float Y { get; set; }

        public DocumentedVector2()
        {

        }

        public DocumentedVector2(float x, float y, string info = "")
        {
            X = x;
            Y = y;
            Info = info;
        }

    }

}
