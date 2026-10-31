using Core;

string xml = File.ReadAllText("../SampleData/clubs.xml");
ISearchStrategy strategy = new SaxSearchStrategy();

SearchCriteria criteria = new SearchCriteria();
criteria.Set("Faculty", "Факультет комп'ютерних наук та кібернетики");
criteria.Set("Course", "2");

List<Club> found = strategy.Search(xml, criteria);
Console.WriteLine($"Знайдено: {found.Count}");
foreach (Club club in found) Console.WriteLine($"  {club.Name}");

criteria.Keyword = "логік";
Console.WriteLine(strategy.Search(xml, criteria).Count);

Console.WriteLine(strategy.Search(xml, new SearchCriteria()).Count);