namespace NServiceBus.Transports.FileBased
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    static class HeaderSerializer
    {
        static HeaderSerializer()
        {
            emptyNamespace = new XmlSerializerNamespaces();
            emptyNamespace.Add("", "");
            serializer = new XmlSerializer(typeof(List<Header>), new XmlRootAttribute("Headers"));
        }

        public static string ToXml(Dictionary<string, string> dictionary)
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                var headers = dictionary.Select(x => new Header
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList();
                serializer.Serialize(writer, headers, emptyNamespace);
            }
            return builder.ToString();
        }

        public static Dictionary<string, string> FromString(string value)
        {
            using (var reader = new StringReader(value))
            {
                var list = (List<Header>) serializer.Deserialize(reader);
                return list.ToDictionary(header => header.Key, header => header.Value);
            }
        }

        static XmlSerializer serializer;
        static XmlSerializerNamespaces emptyNamespace;
    }

    /// <summary>
    /// DTO for the serializing headers.
    /// </summary>
    public class Header
    {
        /// <summary>
        /// The header key.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The header value.
        /// </summary>
        public string Value { get; set; }
    }
}