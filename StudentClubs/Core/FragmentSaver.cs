namespace Core;

public abstract class FragmentSaver {
    protected IFileStorage Storage { get; }

    protected FragmentSaver(IFileStorage storage) {
        ArgumentNullException.ThrowIfNull(storage);
        Storage = storage;
    }

    public async Task<string> SaveAsync(List<Club> clubs, string fileName) {
        await using Stream file = OpenFile();
        IFragmentWriter writer = CreateWriter();

        writer.Write(clubs, file);

        string name = WithExtension(fileName, writer.Extension);
        return await Storage.WriteAsync(file, name, writer.MimeType);
    }

    protected Stream OpenFile() =>
        new FileStream(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), FileMode.CreateNew,
            FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);

    protected abstract IFragmentWriter CreateWriter();

    private static string WithExtension(string fileName, string extension) {
        string name = fileName.Trim();
        if (name.Length == 0) throw new StorageException("Не вказано назву файлу");

        return name.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? name : name + extension;
    }
}