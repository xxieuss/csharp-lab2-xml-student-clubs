using System.Xml;
using Core;

string path = args.Length > 0 ? args[0] : Path.Combine("..", "SampleData", "clubs.xml");
string xml;
try {
    xml = File.ReadAllText(path);
} catch (IOException ex) {
    Console.WriteLine($"Не вдалося прочитати файл {path}: {ex.Message}");
    return;
}

Console.WriteLine($"Файл: {Path.GetFullPath(path)}");
Console.WriteLine();
Console.WriteLine("Значення для списків пошуку:");
Dictionary<string, string[]> values = new AttributeValueLoader().Load(xml);
foreach (string field in new[] { "Kind", "Faculty", "Day", "Course", "Topic" })
    Console.WriteLine($"  {field,-8} {string.Join(", ", values[field])}");

DomSearchStrategy dom = new DomSearchStrategy();
Dictionary<SearchMethod, ISearchStrategy> strategies = new Dictionary<SearchMethod, ISearchStrategy> {
    [SearchMethod.Sax] = new SaxSearchStrategy(),
    [SearchMethod.Dom] = dom,
    [SearchMethod.Linq] = new LinqSearchStrategy()
};
ClubSearcher searcher = new ClubSearcher(strategies[SearchMethod.Sax]);
bool allSame = true;

SearchCriteria Make(string keyword, params (string Field, string Value)[] filters) {
    SearchCriteria criteria = new SearchCriteria { Keyword = keyword };
    foreach ((string field, string value) in filters) criteria.Set(field, value);
    return criteria;
}

List<Club> Run(string title, SearchCriteria criteria) {
    Console.WriteLine();
    Console.WriteLine($"{title}");
    Console.WriteLine($"Параметри: {criteria}");

    List<Club>? first = null;
    foreach (SearchMethod method in Enum.GetValues<SearchMethod>()) {
        searcher.SetStrategy(strategies[method]);
        List<Club> found = searcher.Search(xml, criteria);

        string query = method == SearchMethod.Dom ? $"   XPath: {dom.LastQuery}" : "";
        Console.WriteLine($"  {method,-5} знайдено {found.Count}{query}");

        if (first == null) first = found;
        else if (!found.Select(c => c.Name).SequenceEqual(first.Select(c => c.Name))) allSame = false;
    }

    foreach (Club club in first!) Console.WriteLine($"    • {club.Name}");
    return first;
}

const string Fcsc = "Факультет комп'ютерних наук та кібернетики";

Run("Без фільтрів", Make(""));
Run("Один факультет", Make("", ("Faculty", Fcsc)));
Run("Факультет і курс", Make("", ("Faculty", Fcsc), ("Course", "2")));
Run("Семінари по четвергах", Make("", ("Kind", "семінар"), ("Day", "Четвер")));
List<Club> physics = Run("Кафедра (атрибут є не в усіх записах)", Make("", ("Department", "Кафедра теоретичної фізики")));
Run("Ключове слово без фільтрів", Make("олімп"));
Run("Ключове слово і фільтр", Make("логік", ("Kind", "семінар")));
Run("Нічого не знайдено", Make("", ("Faculty", "Фізичний факультет"), ("Course", "2")));

Console.WriteLine();
Console.WriteLine("Запис у полі виводу");
if (physics.Count > 0) Console.WriteLine(physics[0]);

Console.WriteLine();
Console.WriteLine("Зіпсований XML");
try {
    searcher.Search("<StudentClubs><Club Name='X'>", new SearchCriteria());
} catch (XmlException ex) {
    Console.WriteLine($"  XmlException: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine(allSame
    ? "Усі три стратегії дали однакові результати."
    : "УВАГА: стратегії дали різні результати!");