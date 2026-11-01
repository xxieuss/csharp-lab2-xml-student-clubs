using System.Xml;
using System.Xml.Linq;

namespace Core;

public class LinqSearchStrategy : ISearchStrategy {
    private static readonly XmlReaderSettings Settings = new XmlReaderSettings {
        IgnoreComments = true,
        IgnoreWhitespace = true,
        DtdProcessing = DtdProcessing.Prohibit
    };

    private static readonly Dictionary<string, Func<XElement, string>> Selectors =
        new Dictionary<string, Func<XElement, string>> {
            ["Name"] = club => (string?)club.Attribute("Name") ?? "",
            ["Kind"] = club => (string?)club.Attribute("Kind") ?? "",
            ["Faculty"] = club => (string?)club.Attribute("Faculty") ?? "",
            ["Department"] = club => (string?)club.Attribute("Department") ?? "",
            ["Day"] = club => (string?)club.Element("Schedule")?.Attribute("Day") ?? "",
            ["Time"] = club => (string?)club.Element("Schedule")?.Attribute("Time") ?? "",
            ["Course"] = club => (string?)club.Element("Orientation")?.Attribute("Course") ?? "",
            ["Topic"] = club => (string?)club.Element("Orientation")?.Attribute("Topic") ?? "",
            ["Head"] = club => club.Element("Head")?.Value.Trim() ?? "",
            ["Supervisor"] = club => club.Element("Supervisor")?.Value.Trim() ?? ""
        };

    public List<Club> Search(string xml, SearchCriteria criteria) {
        XDocument document;
        using (StringReader text = new StringReader(xml))
        using (XmlReader reader = XmlReader.Create(text, Settings)) {
            document = XDocument.Load(reader);
        }

        return (from element in BuildQuery(document, criteria)
                let club = ToClub(element)
                where club.ContainsKeyword(criteria.Keyword)
                select club).ToList();
    }

    private static IEnumerable<XElement> BuildQuery(XDocument document, SearchCriteria criteria) {
        IEnumerable<XElement> query = from club in document.Descendants("Club") select club;

        foreach (KeyValuePair<string, string> filter in criteria.Active()) {
            Func<XElement, string> field = Selectors[filter.Key];
            string value = filter.Value;
            query = query.Where(club => field(club) == value);
        }

        return query;
    }

    private static Club ToClub(XElement element) => new Club {
        Name = Selectors["Name"](element),
        Kind = Selectors["Kind"](element),
        Faculty = Selectors["Faculty"](element),
        Department = Selectors["Department"](element),
        Day = Selectors["Day"](element),
        Time = Selectors["Time"](element),
        Course = Selectors["Course"](element),
        Topic = Selectors["Topic"](element),
        Head = Selectors["Head"](element),
        Supervisor = Selectors["Supervisor"](element)
    };
}