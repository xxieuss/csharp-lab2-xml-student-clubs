using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Core;

public class GoogleDriveStorage : IFileStorage {
    private static readonly string[] Scopes = { DriveService.Scope.DriveReadonly, DriveService.Scope.DriveFile };

    private const string AppKey = "createdBy";
    private const string AppValue = "StudentClubs";

    private readonly string _clientSecretJson;
    private readonly string _tokenFolder;
    private readonly Action<string> _openBrowser;
    private UserCredential? _credential;
    private DriveService? _service;

    public GoogleDriveStorage(string clientSecretJson, string tokenFolder, Action<string> openBrowser) {
        _clientSecretJson = clientSecretJson;
        _tokenFolder = tokenFolder;
        _openBrowser = openBrowser;
    }

    public Task<List<string>> ListFilesAsync(string extension) => RunAsync(() => ListNamesAsync(extension));

    public Task<string> ReadTextAsync(string name) => RunAsync(() => DownloadTextAsync(name));

    public Task<string> WriteAsync(Stream content, string name, string mimeType) =>
        RunAsync(() => UploadAsync(content, name, mimeType));

    private async Task<T> RunAsync<T>(Func<Task<T>> request) {
        try {
            await AuthorizeAsync();
            try {
                return await request();
            } catch (TokenResponseException ex) when (IsRejected(ex)) {
                await ForgetTokenAsync();
                await AuthorizeAsync();
                return await request();
            }
        } catch (StorageException) {
            throw;
        } catch (Exception ex) {
            throw new StorageException($"Помилка Google Drive: {ex.Message}");
        }
    }

    private async Task<List<string>> ListNamesAsync(string extension) {
        if (!extension.StartsWith('.')) extension = "." + extension;

        FilesResource.ListRequest request = _service!.Files.List();
        request.Q = "trashed = false and (mimeType contains 'xml' or mimeType contains 'xsl' " +
                    "or mimeType = 'text/plain' or mimeType = 'application/octet-stream')";
        request.Fields = "nextPageToken, files(name)";
        request.PageSize = 1000;

        List<string> names = new List<string>();
        do {
            FileList page = await request.ExecuteAsync();
            foreach (DriveFile file in page.Files) {
                if (file.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase) && !names.Contains(file.Name))
                    names.Add(file.Name);
            }
            request.PageToken = page.NextPageToken;
        } while (request.PageToken != null);

        names.Sort(StringComparer.CurrentCultureIgnoreCase);
        return names;
    }

    private async Task<string> DownloadTextAsync(string name) {
        string? id = await FindFileIdAsync($"name = '{Escape(name)}'");
        if (id == null) throw new StorageException($"На Google Drive немає файлу «{name}»");

        using MemoryStream stream = new MemoryStream();
        IDownloadProgress progress = await _service!.Files.Get(id).DownloadAsync(stream);
        if (progress.Status != DownloadStatus.Completed)
            throw new StorageException($"Не вдалося завантажити файл: {progress.Exception?.Message}");

        stream.Position = 0;
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> UploadAsync(Stream content, string name, string mimeType) {
        if (content.CanSeek) content.Position = 0;

        string? id = await FindFileIdAsync(
            $"name = '{Escape(name)}' and appProperties has {{ key='{AppKey}' and value='{AppValue}' }}");

        IUploadProgress progress;
        if (id == null) {
            DriveFile metadata = new DriveFile {
                Name = name,
                AppProperties = new Dictionary<string, string> { [AppKey] = AppValue }
            };
            progress = await _service!.Files.Create(metadata, content, mimeType).UploadAsync();
        } else {
            progress = await _service!.Files.Update(new DriveFile(), id, content, mimeType).UploadAsync();
        }

        if (progress.Status != UploadStatus.Completed)
            throw new StorageException($"Не вдалося передати файл: {progress.Exception?.Message}");

        return name;
    }

    private static bool IsRejected(TokenResponseException ex) =>
        ex.Error?.Error is "invalid_grant" or "unauthorized_client";

    private async Task ForgetTokenAsync() {
        await new FileDataStore(_tokenFolder, true).ClearAsync();
        _service?.Dispose();
        _service = null;
        _credential = null;
    }

    private async Task AuthorizeAsync() {
        if (_service != null) return;

        try {
            using MemoryStream secrets = new MemoryStream(Encoding.UTF8.GetBytes(_clientSecretJson));

            GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer {
                    ClientSecrets = GoogleClientSecrets.FromStream(secrets).Secrets,
                    Scopes = Scopes,
                    DataStore = new FileDataStore(_tokenFolder, true)
                });

            _credential = await new AuthorizationCodeInstalledApp(flow, new LoopbackCodeReceiver(_openBrowser))
                .AuthorizeAsync("user", CancellationToken.None);

            _service = new DriveService(new BaseClientService.Initializer {
                HttpClientInitializer = _credential,
                ApplicationName = "StudentClubs"
            });
        } catch (StorageException) {
            throw;
        } catch (Exception ex) {
            throw new StorageException($"Не вдалося авторизуватися в Google Drive: {ex.Message}");
        }
    }

    private async Task<string?> FindFileIdAsync(string query) {
        FilesResource.ListRequest request = _service!.Files.List();
        request.Q = $"{query} and trashed = false";
        request.Fields = "files(id, name)";
        request.OrderBy = "modifiedTime desc";
        request.PageSize = 1;

        FileList result = await request.ExecuteAsync();
        return result.Files.Count > 0 ? result.Files[0].Id : null;
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("'", "\\'");
}