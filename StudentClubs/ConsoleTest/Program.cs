using Core;

string xml = File.ReadAllText("../SampleData/clubs.xml");

SearchCriteria criteria = new SearchCriteria();
criteria.Set("Kind", "гурток");
criteria.Set("Day", "Вівторок");

ISearchStrategy[] strategies = { new SaxSearchStrategy(), new DomSearchStrategy(), new LinqSearchStrategy() };
foreach (ISearchStrategy strategy in strategies) {
    List<Club> found = strategy.Search(xml, criteria);
    Console.WriteLine($"{strategy.GetType().Name}: {found.Count}");
    foreach (Club club in found) Console.WriteLine($"  {club.Name}");
}