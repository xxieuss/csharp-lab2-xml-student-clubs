using System.Text;
using System.Xml.Linq;

namespace Core;

public class Club {
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Faculty { get; set; } = "";
    public string Department { get; set; } = "";
    public string Day { get; set; } = "";
    public string Time { get; set; } = "";
    public string Course { get; set; } = "";
    public string Topic { get; set; } = "";
    public string Head { get; set; } = "";
    public string Supervisor { get; set; } = "";

    public bool ContainsKeyword(string word) {
        string needle = Normalize(word.Trim());
        if (needle.Length == 0) return true;

        string[] fields = { Name, Kind, Faculty, Department, Day, Time, Course, Topic, Head, Supervisor };
        foreach (string field in fields) {
            if (Normalize(field).Contains(needle, StringComparison.CurrentCultureIgnoreCase)) return true;
        }
        return false;
    }

    public XElement ToXElement() {
        XElement club = new XElement("Club",
            new XAttribute("Name", Name),
            new XAttribute("Kind", Kind),
            new XAttribute("Faculty", Faculty));

        if (Department.Length > 0) club.Add(new XAttribute("Department", Department));

        club.Add(
            new XElement("Schedule", new XAttribute("Day", Day), new XAttribute("Time", Time)),
            new XElement("Orientation", new XAttribute("Course", Course), new XAttribute("Topic", Topic)),
            new XElement("Head", Head),
            new XElement("Supervisor", Supervisor));

        return club;
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{Name} ({Kind})");
        sb.AppendLine($"    Факультет: {Faculty}");
        if (Department.Length > 0) sb.AppendLine($"    Кафедра: {Department}");
        sb.AppendLine($"    Засідання: {Day}, {Time}");
        sb.AppendLine($"    Орієнтація: {Course} курс, {Topic}");
        sb.AppendLine($"    Староста: {Head}");
        sb.Append($"    Керівник: {Supervisor}");
        return sb.ToString();
    }

    private static string Normalize(string text) => text.Replace('’', '\'').Replace('ʼ', '\'');
}