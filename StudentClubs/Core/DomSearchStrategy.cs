using System.Xml;

namespace Core;

public class DomSearchStrategy : ISearchStrategy {
    private static readonly XmlReaderSettings Settings = new XmlReaderSettings {
        IgnoreComments = true,
        IgnoreWhitespace = true,
        DtdProcessing = DtdProcessing.Prohibit
    };

    private static readonly Dictionary<string, string> Paths = new Dictionary<string, string> {
        ["Name"] = "@Name",
        ["Kind"] = "@Kind",
        ["Faculty"] = "@Faculty",
        ["Department"] = "@Department",
        ["Day"] = "Schedule/@Day",
        ["Time"] = "Schedule/@Time",
        ["Course"] = "Orientation/@Course",
        ["Topic"] = "Orientation/@Topic",
        ["Head"] = "Head",
        ["Supervisor"] = "Supervisor"
    };

    public string LastQuery { get; private set; } = "";

    public List<Club> Search(string xml, SearchCriteria criteria) {
        XmlDocument document = new XmlDocument();
        using (StringReader text = new StringReader(xml))
        using (XmlReader reader = XmlReader.Create(text, Settings)) {
            document.Load(reader);
        }

        LastQuery = BuildXPath(criteria);

        List<Club> result = new List<Club>();
        XmlNodeList? nodes = document.SelectNodes(LastQuery);
        if (nodes == null) return result;

        foreach (XmlNode node in nodes) {
            Club club = ToClub(node);
            if (club.ContainsKeyword(criteria.Keyword)) result.Add(club);
        }

        return result;
    }

    private static string BuildXPath(SearchCriteria criteria) {
        List<string> conditions = new List<string>();
        foreach (KeyValuePair<string, string> filter in criteria.Active())
            conditions.Add($"{Paths[filter.Key]}={Literal(filter.Value)}");

        return conditions.Count == 0 ? "//Club" : $"//Club[{string.Join(" and ", conditions)}]";
    }

    private static Club ToClub(XmlNode node) => new Club {
        Name = Value(node, "Name"),
        Kind = Value(node, "Kind"),
        Faculty = Value(node, "Faculty"),
        Department = Value(node, "Department"),
        Day = Value(node, "Day"),
        Time = Value(node, "Time"),
        Course = Value(node, "Course"),
        Topic = Value(node, "Topic"),
        Head = Value(node, "Head"),
        Supervisor = Value(node, "Supervisor")
    };

    private static string Value(XmlNode node, string field) =>
        node.SelectSingleNode(Paths[field])?.InnerText.Trim() ?? "";

    private static string Literal(string value) {
        if (!value.Contains('\'')) return $"'{value}'";
        if (!value.Contains('"')) return $"\"{value}\"";
        return "concat('" + value.Replace("'", "', \"'\", '") + "')";
    }
}