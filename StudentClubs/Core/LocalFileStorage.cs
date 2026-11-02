namespace Core;

public class LocalFileStorage : IFileStorage {
    private readonly string _folder;

    public LocalFileStorage(string folder) {
        _folder = folder;
    }

    public Task<List<string>> ListFilesAsync(string extension) {
        if (!extension.StartsWith('.')) extension = "." + extension;
        if (!Directory.Exists(_folder)) return Task.FromResult(new List<string>());

        try {
            List<string> names = Directory.EnumerateFiles(_folder)
                .Where(path => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                .Select(path => Path.GetFileName(path))
                .Order(StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            return Task.FromResult(names);
        } catch (UnauthorizedAccessException) {
            throw new StorageException("Немає прав на перегляд робочої теки");
        } catch (IOException ex) {
            throw new StorageException($"Не вдалося переглянути робочу теку: {ex.Message}");
        }
    }

    public async Task<string> ReadTextAsync(string name) {
        string path = Path.IsPathRooted(name) ? name : Path.Combine(_folder, name);

        try {
            return await File.ReadAllTextAsync(path);
        } catch (FileNotFoundException) {
            throw new StorageException($"Файл «{Path.GetFileName(path)}» не знайдено");
        } catch (DirectoryNotFoundException) {
            throw new StorageException("Теку не знайдено");
        } catch (UnauthorizedAccessException) {
            throw new StorageException("Немає прав на читання цього файлу");
        } catch (IOException ex) {
            throw new StorageException($"Не вдалося прочитати файл: {ex.Message}");
        }
    }

    public async Task<string> WriteAsync(Stream content, string name, string mimeType) {
        string fileName = Path.GetFileName(name);
        if (fileName.Length == 0) throw new StorageException("Не вказано назву файлу");

        string path = Path.Combine(_folder, fileName);

        try {
            Directory.CreateDirectory(_folder);
            if (content.CanSeek) content.Position = 0;

            await using FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
            await content.CopyToAsync(file);
            return path;
        } catch (UnauthorizedAccessException) {
            throw new StorageException("Немає прав на запис у цю теку");
        } catch (IOException ex) {
            throw new StorageException($"Не вдалося записати файл: {ex.Message}");
        }
    }
}