using ProtoBuf;

namespace AdvancedStatsAndEffects
{

    [ProtoContract]
    public class CommandExtraParam
    {

        [ProtoMember(1)]
        public string id;

        [ProtoMember(2)]
        public string data;

    }

    [ProtoContract]
    public class CommandExtraParams
    {

        [ProtoMember(1)]
        public CommandExtraParam[] extraParams;

    }

    [ProtoContract]
    public class Command
    {

        [ProtoMember(1)]
        public ulong sender;

        [ProtoMember(2)]
        public string[] content;

        [ProtoMember(2)]
        public byte[] data;

        public Command() { }

        public Command(ulong sender, params string[] content)
        {
            this.sender = sender;
            this.content = content;
        }

    }

}