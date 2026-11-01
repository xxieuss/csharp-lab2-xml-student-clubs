namespace Core;

public class ClubSearcher {
    private ISearchStrategy _strategy;

    public ClubSearcher(ISearchStrategy strategy) {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategy = strategy;
    }

    public void SetStrategy(ISearchStrategy strategy) {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategy = strategy;
    }

    public List<Club> Search(string xml, SearchCriteria criteria) => _strategy.Search(xml, criteria);
}