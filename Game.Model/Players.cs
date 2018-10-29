using System;
using ProtoBuf;
namespace Game.Model
{
    [ProtoContract(SkipConstructor = true)]
    public class Players
    {
        [ProtoMember(1)]
        public string PlayerName
        {
            get;
            set;
        }
        [ProtoMember(2)]
        public int ConnectionId
        {
            get;
            set;
        }
        [ProtoMember(3)]
        public EventTypes gameEventType
        {
            get;
            set;
        }
        [ProtoMember(4)]
        public Move PlayerMoveCoordinates
        {
            get;
            set;
        }

    }
}
