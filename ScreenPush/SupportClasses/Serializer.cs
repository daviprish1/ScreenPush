using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ScreenPush.SupportClasses
{
    public static class Serializer
    {
        public static string Serialize<T>(T obj)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new Utf8StringWriter();
            using (var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 }))
            {
                xmlserializer.Serialize(writer, obj);
                return stringWriter.ToString();
            }

        }
    }
}
