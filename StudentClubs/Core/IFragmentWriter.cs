namespace Core;

public interface IFragmentWriter {
    string Extension { get; }

    string MimeType { get; }

    void Write(List<Club> clubs, Stream stream);
}