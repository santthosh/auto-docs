using System;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.Xml;

namespace Autodocs.ServiceModel.Web
{
    public class JsonBodyWriter :BodyWriter
    {
        readonly object instance;
        readonly Type type;

        public JsonBodyWriter(object instance, Type type)
            : base(true)
        {
            this.instance = instance;
            this.type = type;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(type);
            serializer.WriteObject(writer, instance);
        }
    }
}
