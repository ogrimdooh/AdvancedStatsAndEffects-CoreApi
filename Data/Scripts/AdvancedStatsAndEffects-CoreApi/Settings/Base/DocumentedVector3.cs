using ProtoBuf;
using System.Xml;
using System.Xml.Serialization;

namespace AdvancedStatsAndEffects
{

    [ProtoContract(SkipConstructor = true, UseProtoMembersOnly = true)]
    public class DocumentedVector3
    {

        [XmlAttribute("info")]
        public string Info { get; set; }
        [XmlElement]
        public float X { get; set; }
        [XmlElement]
        public float Y { get; set; }
        [XmlElement]
        public float Z { get; set; }

        public DocumentedVector3()
        {

        }

        public DocumentedVector3(float x, float y, float z, string info = "")
        {
            X = x;
            Y = y;
            Z = z;
            Info = info;
        }

    }

}
