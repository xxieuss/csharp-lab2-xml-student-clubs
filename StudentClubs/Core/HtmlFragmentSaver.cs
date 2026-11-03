namespace Core;

public class HtmlFragmentSaver : FragmentSaver {
    private readonly string _xsl;

    public HtmlFragmentSaver(IFileStorage storage, string xsl) : base(storage) {
        if (string.IsNullOrWhiteSpace(xsl)) throw new ArgumentException("Не задано XSL-документ", nameof(xsl));
        _xsl = xsl;
    }

    protected override IFragmentWriter CreateWriter() => new HtmlFragmentWriter(_xsl);
}