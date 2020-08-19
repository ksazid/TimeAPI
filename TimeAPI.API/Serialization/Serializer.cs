using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Xml;
namespace TimeAPI.API.Serialization
{
    internal class Serializer : ISerializer<string>
    {
        public T Deserialize<T>(string xmlString)
        {
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "Dynamic";
            xRoot.IsNullable = true;


            var serializer = new XmlSerializer(typeof(T), xRoot);
            using (TextReader textReader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(textReader);
            }
        }

        public string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            using (StringWriter writer = new Utf8StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }

        }


        internal class Utf8StringWriter : StringWriter
        {
            public override System.Text.Encoding Encoding { get { return System.Text.Encoding.UTF8; } }
        }
    }
}
