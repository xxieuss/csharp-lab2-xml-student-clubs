using Core;

namespace Ui;

public class ClubCard {
    private const string SeminarKind = "семінар";

    public string Name { get; }
    public string Kind { get; }
    public string Faculty { get; }
    public string Head { get; }
    public string Supervisor { get; }

    public string Initial { get; }

    public bool IsSeminar { get; }

    public IReadOnlyList<string> Chips { get; }

    public ClubCard(Club club) {
        Name = club.Name;
        Kind = club.Kind;
        Faculty = club.Faculty;
        Head = club.Head;
        Supervisor = club.Supervisor;

        IsSeminar = string.Equals(club.Kind, SeminarKind, StringComparison.CurrentCultureIgnoreCase);
        Initial = club.Kind.Length > 0 ? club.Kind.Substring(0, 1).ToUpper() : "?";

        List<string> chips = new List<string> {
            $"{club.Day}, {club.Time}",
            $"{club.Course} курс",
            club.Topic
        };
        if (club.Department.Length > 0) chips.Add(club.Department);
        Chips = chips;
    }
}