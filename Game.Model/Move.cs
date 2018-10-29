using System;
using ProtoBuf;
namespace Game.Model
{

    [ProtoContract(SkipConstructor = true)]
    public class Move
    {

        [ProtoMember(1)]
        public float X
        {
            get;
            set;
        }
        [ProtoMember(2)]
        public float Y
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public float Z
        {
            get;
            set;
        }

    }
}
