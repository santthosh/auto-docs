using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Autodocs.ServiceModel.Web
{
    internal class TextBodyWriter : BodyWriter
    {
        readonly byte[] messageBytes;

        public TextBodyWriter(string message) : base(true)
        {
            messageBytes = Encoding.UTF8.GetBytes(message);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {           
            writer.WriteStartElement("Binary");
            writer.WriteBase64(messageBytes, 0, messageBytes.Length);
            writer.WriteEndElement();
        }
    }
}
