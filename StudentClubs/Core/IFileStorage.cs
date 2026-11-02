namespace Core;

public interface IFileStorage {
    Task<List<string>> ListFilesAsync(string extension);

    Task<string> ReadTextAsync(string name);

    Task<string> WriteAsync(Stream content, string name, string mimeType);
}