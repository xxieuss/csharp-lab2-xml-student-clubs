using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Core;

public class HtmlFragmentWriter : IFragmentWriter {
    private static readonly XmlReaderSettings Settings = new XmlReaderSettings {
        IgnoreComments = true,
        DtdProcessing = DtdProcessing.Prohibit
    };

    private readonly XslCompiledTransform _xslt = new XslCompiledTransform();

    public string Extension => ".html";

    public string MimeType => "text/html";

    public HtmlFragmentWriter(string xsl) {
        using StringReader text = new StringReader(xsl);
        using XmlReader reader = XmlReader.Create(text, Settings);
        _xslt.Load(reader);
    }

    public void Write(List<Club> clubs, Stream stream) {
        string html = Transform(BuildXml(clubs));
        byte[] bytes = new UTF8Encoding(false).GetBytes(html);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string BuildXml(List<Club> clubs) =>
        new XElement("StudentClubs", clubs.Select(club => club.ToXElement())).ToString();

    private string Transform(string xml) {
        using StringReader text = new StringReader(xml);
        using XmlReader input = XmlReader.Create(text, Settings);
        using Utf8StringWriter output = new Utf8StringWriter();

        _xslt.Transform(input, null, output);
        return output.ToString();
    }

    private sealed class Utf8StringWriter : StringWriter {
        public override Encoding Encoding => Encoding.UTF8;
    }
}