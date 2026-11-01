using Core;

string xml = File.ReadAllText("../SampleData/clubs.xml");

SearchCriteria criteria = new SearchCriteria();
criteria.Set("Faculty", "Фізичний факультет");

DomSearchStrategy dom = new DomSearchStrategy();
ClubSearcher searcher = new ClubSearcher(new SaxSearchStrategy());

Console.WriteLine($"SAX:  {searcher.Search(xml, criteria).Count}");
searcher.SetStrategy(dom);
Console.WriteLine($"DOM:  {searcher.Search(xml, criteria).Count}  {dom.LastQuery}");
searcher.SetStrategy(new LinqSearchStrategy());
Console.WriteLine($"LINQ: {searcher.Search(xml, criteria).Count}");