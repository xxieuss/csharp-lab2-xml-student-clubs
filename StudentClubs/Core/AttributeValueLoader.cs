using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Core;

public class AttributeValueLoader {
    private static readonly XmlReaderSettings Settings = new XmlReaderSettings {
        IgnoreComments = true,
        IgnoreWhitespace = true,
        DtdProcessing = DtdProcessing.Prohibit
    };

    private static readonly string[] WeekDays = {
        "Понеділок", "Вівторок", "Середа", "Четвер", "П'ятниця", "Субота", "Неділя"
    };

    private static readonly StringComparer Ukrainian = StringComparer.Create(new CultureInfo("uk-UA"), false);

    public Dictionary<string, string[]> Load(string xml) {
        XDocument document;
        using (StringReader text = new StringReader(xml))
        using (XmlReader reader = XmlReader.Create(text, Settings)) {
            document = XDocument.Load(reader);
        }

        ILookup<string, string> values = (from club in document.Descendants("Club")
                                          from pair in Pairs(club)
                                          where pair.Value.Length > 0
                                          select pair).ToLookup(pair => pair.Key, pair => pair.Value);

        Dictionary<string, string[]> result = new Dictionary<string, string[]>();
        foreach (string field in SearchCriteria.Fields) result[field] = Sort(field, values[field].Distinct());

        return result;
    }

    private static IEnumerable<KeyValuePair<string, string>> Pairs(XElement club) {
        foreach (XAttribute attribute in club.Attributes())
            yield return new KeyValuePair<string, string>(attribute.Name.LocalName, attribute.Value.Trim());

        foreach (XElement child in club.Elements()) {
            foreach (XAttribute attribute in child.Attributes())
                yield return new KeyValuePair<string, string>(attribute.Name.LocalName, attribute.Value.Trim());

            if (!child.HasElements && !child.HasAttributes)
                yield return new KeyValuePair<string, string>(child.Name.LocalName, child.Value.Trim());
        }
    }

    private static string[] Sort(string field, IEnumerable<string> values) {
        if (field != "Day") return values.OrderBy(value => value, Ukrainian).ToArray();

        return values.OrderBy(DayIndex).ThenBy(day => day, Ukrainian).ToArray();
    }

    private static int DayIndex(string day) {
        int index = Array.IndexOf(WeekDays, day);
        return index >= 0 ? index : WeekDays.Length;
    }
}