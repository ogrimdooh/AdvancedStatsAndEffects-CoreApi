using ProtoBuf;
using System.Xml;
using System.Xml.Serialization;

namespace AdvancedStatsAndEffects
{
    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class DocumentedVector4
    {

        [XmlAttribute("info")]
        public string Info { get; set; }
        [XmlElement]
        public float X { get; set; }
        [XmlElement]
        public float Y { get; set; }
        [XmlElement]
        public float Z { get; set; }
        [XmlElement]
        public float W { get; set; }

        public DocumentedVector4()
        {

        }

        public DocumentedVector4(float x, float y, float z, float w, string info = "")
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            Info = info;
        }

    }

}
