using Core;

string xml = File.ReadAllText("../SampleData/clubs.xml");
DomSearchStrategy strategy = new DomSearchStrategy();

SearchCriteria criteria = new SearchCriteria();
criteria.Set("Faculty", "Факультет комп'ютерних наук та кібернетики");
criteria.Set("Course", "2");

List<Club> found = strategy.Search(xml, criteria);
Console.WriteLine(strategy.LastQuery);
Console.WriteLine($"Знайдено: {found.Count}");
foreach (Club club in found) Console.WriteLine($"  {club.Name}");

criteria.Clear();
criteria.Set("Kind", "семінар");
Console.WriteLine($"{strategy.Search(xml, criteria).Count} — {strategy.LastQuery}");

criteria.Clear();
Console.WriteLine($"{strategy.Search(xml, criteria).Count} — {strategy.LastQuery}");