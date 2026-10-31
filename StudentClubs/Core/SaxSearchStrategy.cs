using System.Xml;

namespace Core;

public class SaxSearchStrategy : ISearchStrategy {
    private static readonly XmlReaderSettings Settings = new XmlReaderSettings {
        IgnoreComments = true,
        IgnoreWhitespace = true,
        DtdProcessing = DtdProcessing.Prohibit
    };

    public List<Club> Search(string xml, SearchCriteria criteria) {
        Dictionary<string, string> filters = criteria.Active();
        List<Club> result = new List<Club>();

        using StringReader text = new StringReader(xml);
        using XmlReader reader = XmlReader.Create(text, Settings);

        while (reader.Read()) {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "Club") continue;

            Club club = ReadClub(reader);
            if (Matches(club, filters) && club.ContainsKeyword(criteria.Keyword)) result.Add(club);
        }

        return result;
    }

    private static Club ReadClub(XmlReader reader) {
        Club club = new Club();
        int depth = reader.Depth;
        bool isEmpty = reader.IsEmptyElement;

        while (reader.MoveToNextAttribute()) Assign(club, reader.Name, reader.Value);
        reader.MoveToElement();
        if (isEmpty) return club;

        string current = "";
        while (reader.Read() && reader.Depth > depth) {
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    current = reader.Name;
                    while (reader.MoveToNextAttribute()) Assign(club, reader.Name, reader.Value);
                    reader.MoveToElement();
                    break;
                case XmlNodeType.Text:
                    Assign(club, current, reader.Value.Trim());
                    break;
            }
        }

        return club;
    }

    private static bool Matches(Club club, Dictionary<string, string> filters) {
        foreach (KeyValuePair<string, string> filter in filters) {
            if (!string.Equals(FieldOf(club, filter.Key), filter.Value, StringComparison.Ordinal)) return false;
        }
        return true;
    }

    private static void Assign(Club club, string field, string value) {
        switch (field) {
            case "Name": club.Name = value; break;
            case "Kind": club.Kind = value; break;
            case "Faculty": club.Faculty = value; break;
            case "Department": club.Department = value; break;
            case "Day": club.Day = value; break;
            case "Time": club.Time = value; break;
            case "Course": club.Course = value; break;
            case "Topic": club.Topic = value; break;
            case "Head": club.Head = value; break;
            case "Supervisor": club.Supervisor = value; break;
        }
    }

    private static string FieldOf(Club club, string field) => field switch {
        "Name" => club.Name,
        "Kind" => club.Kind,
        "Faculty" => club.Faculty,
        "Department" => club.Department,
        "Day" => club.Day,
        "Time" => club.Time,
        "Course" => club.Course,
        "Topic" => club.Topic,
        "Head" => club.Head,
        "Supervisor" => club.Supervisor,
        _ => ""
    };
}