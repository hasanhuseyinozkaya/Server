using System;
using ProtoBuf;
using System.Xml;
using System.IO;
namespace Game.Model
{
    public class SerializationHelper
    {
        public SerializationHelper()
        {
        }

        public static byte[] ProtoSerialize<T>(T record) where T : class
        {
            if (null == record) return null;

            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, record);
                    return stream.ToArray();
                }
            }
            catch
            {
                // Log error
                throw;
            }
        }



        public static T ProtoDeserialize<T>(byte[] data) where T : class
        {
            if (null == data) return null;

            try
            {
                using (var stream = new MemoryStream(data))
                {
                    stream.Position = 0;
                    return Serializer.Deserialize<T>(stream);
                }
            }
            catch
            {
                // Log error
                throw;
            }
        }

    }
}
