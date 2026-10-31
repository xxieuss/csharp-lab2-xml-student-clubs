namespace Core;

public interface ISearchStrategy {
    List<Club> Search(string xml, SearchCriteria criteria);
}