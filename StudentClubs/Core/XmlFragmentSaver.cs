namespace Core;

public class XmlFragmentSaver : FragmentSaver {
    public XmlFragmentSaver(IFileStorage storage) : base(storage) { }

    protected override IFragmentWriter CreateWriter() => new XmlFragmentWriter();
}