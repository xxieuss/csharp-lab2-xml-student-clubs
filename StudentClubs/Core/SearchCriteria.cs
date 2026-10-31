namespace Core;

public class SearchCriteria {
    public static readonly IReadOnlyList<string> Fields = new[] {
        "Name", "Kind", "Faculty", "Department", "Day", "Time", "Course", "Topic", "Head", "Supervisor"
    };

    private readonly Dictionary<string, string> _filters = new Dictionary<string, string>();

    public string Keyword { get; set; } = "";

    public void Set(string attribute, string? value) {
        if (!Fields.Contains(attribute)) throw new ArgumentException($"Невідоме поле для пошуку: {attribute}");

        if (string.IsNullOrWhiteSpace(value)) _filters.Remove(attribute);
        else _filters[attribute] = value.Trim();
    }

    public string Get(string attribute) => _filters.TryGetValue(attribute, out string? value) ? value : "";

    public Dictionary<string, string> Active() => new Dictionary<string, string>(_filters);

    public bool IsEmpty() => _filters.Count == 0 && Keyword.Trim().Length == 0;

    public void Clear() {
        _filters.Clear();
        Keyword = "";
    }

    public override string ToString() {
        if (IsEmpty()) return "без фільтрів";

        List<string> parts = new List<string>();
        foreach (string field in Fields) {
            if (_filters.TryGetValue(field, out string? value)) parts.Add($"«{field}» – {value}");
        }
        if (Keyword.Trim().Length > 0) parts.Add($"«Keyword» – {Keyword.Trim()}");

        return string.Join(", ", parts);
    }
}