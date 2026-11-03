using System.Xml;
using System.Xml.Xsl;

namespace Core;

public class ClubController {
    private readonly Dictionary<SearchMethod, ISearchStrategy> _strategies;
    private readonly Dictionary<FileLocation, IFileStorage> _storages;
    private readonly ClubSearcher _searcher;
    private readonly AttributeValueLoader _loader = new AttributeValueLoader();

    private string _xmlContent = "";
    private string _xslContent = "";
    private bool _searched;

    public SearchCriteria Criteria { get; private set; } = new SearchCriteria();
    public List<Club> Results { get; private set; } = new List<Club>();
    public SearchMethod Method { get; private set; } = SearchMethod.Sax;
    public string XmlName { get; private set; } = "";
    public string XslName { get; private set; } = "";
    public bool HasXml => _xmlContent.Length > 0;
    public bool HasXsl => _xslContent.Length > 0;

    public string LastQuery =>
        Method == SearchMethod.Dom && _strategies[Method] is DomSearchStrategy dom ? dom.LastQuery : "";

    public string LogPath => EventLogger.Instance.LogPath;

    public ClubController(IFileStorage local, IFileStorage drive) {
        _storages = new Dictionary<FileLocation, IFileStorage> {
            [FileLocation.Local] = local,
            [FileLocation.GoogleDrive] = drive
        };
        _strategies = new Dictionary<SearchMethod, ISearchStrategy> {
            [SearchMethod.Sax] = new SaxSearchStrategy(),
            [SearchMethod.Dom] = new DomSearchStrategy(),
            [SearchMethod.Linq] = new LinqSearchStrategy()
        };
        _searcher = new ClubSearcher(_strategies[Method]);
    }

    public Task<List<string>> ListFilesAsync(FileLocation location, string extension) =>
        _storages[location].ListFilesAsync(extension);

    public async Task<Dictionary<string, string[]>> OpenXmlAsync(FileLocation location, string name) {
        string text = await _storages[location].ReadTextAsync(name);

        Dictionary<string, string[]> values;
        try {
            values = _loader.Load(text);
        } catch (XmlException ex) {
            throw new InvalidDataException($"Файл не є коректним XML: {ex.Message}");
        }

        _xmlContent = text;
        XmlName = Path.GetFileName(name);
        Results = new List<Club>();
        _searched = false;
        return values;
    }

    public async Task OpenXslAsync(FileLocation location, string name) {
        string text = await _storages[location].ReadTextAsync(name);
        ValidateXsl(text);

        _xslContent = text;
        XslName = Path.GetFileName(name);
    }

    public void CloseXml() {
        _xmlContent = "";
        XmlName = "";
        Clear();
    }

    public void CloseXsl() {
        _xslContent = "";
        XslName = "";
    }

    public void SelectMethod(SearchMethod method) {
        Method = method;
        _searcher.SetStrategy(_strategies[method]);
    }

    public List<Club> Search(SearchCriteria criteria) {
        if (!HasXml) throw new InvalidOperationException("Спочатку відкрийте XML-файл");

        Criteria = criteria;
        Results = _searcher.Search(_xmlContent, criteria);
        _searched = true;

        EventLogger.Instance.Log(LogLevel.Filtering,
            $"Параметри: {criteria}. Спосіб: {MethodName(Method)}. Знайдено: {Results.Count}");
        return Results;
    }

    public async Task<string> SaveXmlAsync(FileLocation location, string fileName) {
        string saved = await ExportAsync(new XmlFragmentSaver(_storages[location]), fileName);

        EventLogger.Instance.Log(LogLevel.Saving,
            $"Збережено відфільтрований фрагмент у файл {Where(location, saved)}");
        return saved;
    }

    public async Task<string> TransformToHtmlAsync(FileLocation location, string fileName) {
        if (!HasXsl) throw new InvalidOperationException("Спочатку відкрийте XSL-файл");

        string saved = await ExportAsync(new HtmlFragmentSaver(_storages[location], _xslContent), fileName);

        EventLogger.Instance.Log(LogLevel.Transformation, $"Збережено у файл {Where(location, saved)}");
        return saved;
    }

    public void Clear() {
        Criteria = new SearchCriteria();
        Results = new List<Club>();
        _searched = false;
    }

    private async Task<string> ExportAsync(FragmentSaver saver, string fileName) {
        if (!_searched) throw new InvalidOperationException("Спочатку виконайте пошук");
        return await saver.SaveAsync(Results, fileName);
    }

    private static void ValidateXsl(string xsl) {
        try {
            using StringReader text = new StringReader(xsl);
            using XmlReader reader = XmlReader.Create(text, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
            new XslCompiledTransform().Load(reader);
        } catch (XsltException ex) {
            throw new InvalidDataException($"Файл не є коректним XSL-документом: {ex.Message}");
        } catch (XmlException ex) {
            throw new InvalidDataException($"Файл не є коректним XSL-документом: {ex.Message}");
        }
    }

    private static string Where(FileLocation location, string saved) =>
        location == FileLocation.GoogleDrive ? $"{saved} (Google Drive)" : saved;

    private static string MethodName(SearchMethod method) => method switch {
        SearchMethod.Sax => "SAX API",
        SearchMethod.Dom => "DOM API",
        SearchMethod.Linq => "LINQ to XML",
        _ => method.ToString()
    };
}