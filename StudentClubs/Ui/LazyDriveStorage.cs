using Core;

namespace Ui;

public class LazyDriveStorage : IFileStorage {
    private const string SecretFile = "client_secret.json";

    private readonly string _tokenFolder;
    private GoogleDriveStorage? _drive;

    public LazyDriveStorage(string tokenFolder) {
        _tokenFolder = tokenFolder;
    }

    public async Task<List<string>> ListFilesAsync(string extension) {
        GoogleDriveStorage drive = await GetDriveAsync();
        return await drive.ListFilesAsync(extension);
    }

    public async Task<string> ReadTextAsync(string name) {
        GoogleDriveStorage drive = await GetDriveAsync();
        return await drive.ReadTextAsync(name);
    }

    public async Task<string> WriteAsync(Stream content, string name, string mimeType) {
        GoogleDriveStorage drive = await GetDriveAsync();
        return await drive.WriteAsync(content, name, mimeType);
    }

    private async Task<GoogleDriveStorage> GetDriveAsync() {
        if (_drive != null) return _drive;

        if (!await FileSystem.AppPackageFileExistsAsync(SecretFile))
            throw new StorageException($"Не знайдено {SecretFile} у Resources/Raw, тому Google Drive недоступний");

        await using Stream stream = await FileSystem.OpenAppPackageFileAsync(SecretFile);
        using StreamReader reader = new StreamReader(stream);
        string secret = await reader.ReadToEndAsync();

        _drive = new GoogleDriveStorage(secret, _tokenFolder, OpenBrowser);
        return _drive;
    }

    private static void OpenBrowser(string url) =>
        MainThread.BeginInvokeOnMainThread(() => Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred));
}