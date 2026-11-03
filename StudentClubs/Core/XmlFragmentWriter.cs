using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Core;

public class XmlFragmentWriter : IFragmentWriter {
    public string Extension => ".xml";

    public string MimeType => "application/xml";

    public void Write(List<Club> clubs, Stream stream) {
        XDocument document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("StudentClubs", clubs.Select(club => club.ToXElement())));

        XmlWriterSettings settings = new XmlWriterSettings {
            Indent = true,
            Encoding = new UTF8Encoding(false)
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        document.Save(writer);
    }
}