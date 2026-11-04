using Core;
using Foundation;
using UIKit;

namespace Ui;

public partial class MainPage : ContentPage {
    private const string AnyValue = "— усі —";
    private const string NotOpened = "не відкрито";
    private const string OpenHint = "Відкрийте XML-файл, і тут з'являться гуртки. Кожна зміна параметрів одразу оновлює результат.";
    private const string ClearedHint = "Результати очищено. Оберіть параметри або натисніть «Пошук», щоб знову побачити гуртки.";

    private const int KeywordDelayMs = 400;

    private readonly ClubController _controller;
    private readonly Dictionary<string, ValueSelector> _selectors;
    private CancellationTokenSource? _keywordDelay;
    private bool _hasResults;
    private bool _resetting;

    public MainPage() {
        string appData = FileSystem.AppDataDirectory;
        _controller = new ClubController(
            new LocalFileStorage(Path.Combine(appData, "Results")),
            new LazyDriveStorage(Path.Combine(appData, "drive-token")));

        InitializeComponent();

        _selectors = new Dictionary<string, ValueSelector> {
            ["Kind"] = KindSelector,
            ["Faculty"] = FacultySelector,
            ["Department"] = DepartmentSelector,
            ["Day"] = DaySelector,
            ["Course"] = CourseSelector,
            ["Topic"] = TopicSelector
        };
        foreach (ValueSelector selector in _selectors.Values) selector.SelectionChanged += OnFilterChanged;

        ResultsLabel.Text = OpenHint;
        LogPathLabel.Text = $"Протокол: {_controller.LogPath}";
        ShowStatus("Відкрийте XML-файл з диска або з Google Drive");
    }

    private async void OnOpenXmlClicked(object? sender, EventArgs e) {
        FileLocation location = LocationOf(sender);

        await RunAsync(sender, "Відкриття XML", async () => {
            string? name = await ChooseFileAsync(location, ".xml", "Оберіть XML-файл");
            if (name == null) return;

            Dictionary<string, string[]> values = await _controller.OpenXmlAsync(location, name);

            FillSelectors(values);
            ResetKeyword();
            XmlFileLabel.Text = Describe(_controller.XmlName, location);
            CloseXmlButton.IsVisible = true;

            int found = Search();
            ShowStatus($"Відкрито {_controller.XmlName}, гуртків у файлі: {found}");
        });
    }

    private async void OnOpenXslClicked(object? sender, EventArgs e) {
        FileLocation location = LocationOf(sender);

        await RunAsync(sender, "Відкриття XSL", async () => {
            string? name = await ChooseFileAsync(location, ".xsl", "Оберіть XSL-файл");
            if (name == null) return;

            await _controller.OpenXslAsync(location, name);

            XslFileLabel.Text = Describe(_controller.XslName, location);
            CloseXslButton.IsVisible = true;
            ShowStatus($"Відкрито {_controller.XslName}");
        });
    }

    private async void OnCloseXmlClicked(object? sender, EventArgs e) {
        string name = _controller.XmlName;
        bool confirmed = await DisplayAlertAsync("Закриття файлу",
            $"Закрити {name}? Результати і параметри пошуку буде очищено.", "Так", "Ні");
        if (!confirmed) return;

        _controller.CloseXml();

        foreach (ValueSelector selector in _selectors.Values) selector.ItemsSource = Array.Empty<string>();
        ResetKeyword();
        ResetResults();
        XmlFileLabel.Text = NotOpened;
        CloseXmlButton.IsVisible = false;
        ShowStatus($"Файл {name} закрито");
    }

    private void OnCloseXslClicked(object? sender, EventArgs e) {
        string name = _controller.XslName;
        _controller.CloseXsl();

        XslFileLabel.Text = NotOpened;
        CloseXslButton.IsVisible = false;
        ShowStatus($"Файл {name} закрито");
    }

    private async void OnFilterChanged(object? sender, EventArgs e) => await RefreshAsync();

    private async void OnMethodChanged(object? sender, CheckedChangedEventArgs e) {
        if (!e.Value || sender is not RadioButton { Value: string value }) return;

        _controller.SelectMethod(Enum.Parse<SearchMethod>(value));
        if (_hasResults) await RefreshAsync();
    }

    private async void OnKeywordChanged(object? sender, TextChangedEventArgs e) {
        if (_resetting) return;

        _keywordDelay?.Cancel();
        CancellationTokenSource delay = new CancellationTokenSource();
        _keywordDelay = delay;

        try {
            await Task.Delay(KeywordDelayMs, delay.Token);
        } catch (OperationCanceledException) {
            return;
        }
        await RefreshAsync();
    }

    private async void OnKeywordCompleted(object? sender, EventArgs e) {
        _keywordDelay?.Cancel();
        await RefreshAsync();
    }

    private async void OnSearchClicked(object? sender, EventArgs e) {
        await RunAsync(sender, "Пошук", () => {
            Search();
            return Task.CompletedTask;
        });
    }

    private async void OnSaveXmlClicked(object? sender, EventArgs e) {
        FileLocation target = SelectedTarget();

        await RunAsync(sender, "Збереження XML", async () => {
            ShowWaiting(target);
            string saved = await _controller.SaveXmlAsync(target, FileNameEntry.Text ?? "");

            ShowStatus($"Збережено {Describe(saved, target)}");
            await DisplayAlertAsync("Збережено", Describe(saved, target), "Гаразд");
        });
    }

    private async void OnTransformClicked(object? sender, EventArgs e) {
        FileLocation target = SelectedTarget();

        await RunAsync(sender, "Трансформація", async () => {
            ShowWaiting(target);
            string saved = await _controller.TransformToHtmlAsync(target, FileNameEntry.Text ?? "");

            ShowStatus($"HTML збережено {Describe(saved, target)}");

            if (target == FileLocation.GoogleDrive) {
                await DisplayAlertAsync("Трансформацію виконано", Describe(saved, target), "Гаразд");
                return;
            }

            bool open = await DisplayAlertAsync("Трансформацію виконано", saved, "Відкрити", "Гаразд");
            if (open) await OpenInDefaultAppAsync(saved);
        });
    }

    private async void OnClearClicked(object? sender, EventArgs e) {
        if (!HasSomethingToClear()) {
            ShowStatus("Очищати нічого");
            return;
        }

        bool confirmed = await DisplayAlertAsync("Очищення", "Очистити результати і параметри пошуку?", "Так", "Ні");
        if (!confirmed) return;

        _controller.Clear();

        foreach (ValueSelector selector in _selectors.Values)
            selector.SelectedIndex = selector.ItemsSource.Count > 0 ? 0 : -1;
        ResetKeyword();
        ResetResults();
        ShowStatus("Результати і параметри пошуку очищено");
    }

    private async void OnExitClicked(object? sender, EventArgs e) => await App.ConfirmQuitAsync();

    private async Task RefreshAsync() {
        if (!_controller.HasXml) return;

        await RunAsync(null, "Пошук", () => {
            Search();
            return Task.CompletedTask;
        });
    }

    private int Search() {
        List<Club> clubs = _controller.Search(ReadCriteria());

        ShowResults(clubs);
        ShowStatus($"{MethodTitle()}, знайдено {clubs.Count}");
        return clubs.Count;
    }

    private async Task RunAsync(object? sender, string title, Func<Task> action) {
        Button? button = sender as Button;
        if (button != null) button.IsEnabled = false;

        try {
            await action();
        } catch (Exception ex) {
            ShowStatus($"{title}: не вдалося");
            await DisplayAlertAsync(title, ex.Message, "Гаразд");
        } finally {
            if (button != null) button.IsEnabled = true;
        }
    }

    private async Task<string?> ChooseFileAsync(FileLocation location, string extension, string title) {
        if (location == FileLocation.Local) {
            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = title });
            return file?.FullPath;
        }

        ShowStatus("Зв'язок з Google Drive... Якщо відкрився браузер, дозвольте доступ");
        List<string> names = await _controller.ListFilesAsync(location, extension);

        if (names.Count == 0) {
            ShowStatus($"На Google Drive немає файлів {extension}");
            await DisplayAlertAsync("Google Drive", $"На Google Drive немає файлів {extension}", "Гаразд");
            return null;
        }

        string choice = await DisplayActionSheetAsync(title, "Скасувати", null, names.ToArray());
        return names.Contains(choice) ? choice : null;
    }

    private void FillSelectors(Dictionary<string, string[]> values) {
        foreach (KeyValuePair<string, ValueSelector> pair in _selectors) {
            List<string> items = new List<string> { AnyValue };
            items.AddRange(values[pair.Key]);

            pair.Value.ItemsSource = items;
            pair.Value.SelectedIndex = 0;
        }
    }

    private SearchCriteria ReadCriteria() {
        SearchCriteria criteria = new SearchCriteria();

        foreach (KeyValuePair<string, ValueSelector> pair in _selectors) {
            ValueSelector selector = pair.Value;
            criteria.Set(pair.Key, selector.SelectedIndex > 0 ? selector.SelectedItem : null);
        }
        criteria.Keyword = KeywordEntry.Text?.Trim() ?? "";

        return criteria;
    }

    private void ShowResults(List<Club> clubs) {
        ResultsTitleLabel.Text = $"РЕЗУЛЬТАТИ · {MethodTitle()} · ЗНАЙДЕНО {clubs.Count}";

        string query = _controller.LastQuery;
        QueryLabel.Text = $"XPath: {query}";
        QueryLabel.IsVisible = query.Length > 0;

        _hasResults = true;
        ResultsLabel.Text = clubs.Count == 0
            ? "За заданими параметрами нічого не знайдено."
            : string.Join("\n\n", clubs.Select((club, i) => $"{i + 1}. {club}"));
    }

    private void ResetResults() {
        _hasResults = false;
        ResultsTitleLabel.Text = "РЕЗУЛЬТАТИ";
        QueryLabel.Text = "";
        QueryLabel.IsVisible = false;
        ResultsLabel.Text = _controller.HasXml ? ClearedHint : OpenHint;
    }

    private void ResetKeyword() {
        _keywordDelay?.Cancel();
        _resetting = true;
        KeywordEntry.Text = "";
        _resetting = false;
    }

    private bool HasSomethingToClear() =>
        _hasResults
        || _selectors.Values.Any(selector => selector.SelectedIndex > 0)
        || !string.IsNullOrWhiteSpace(KeywordEntry.Text);

    private void ShowWaiting(FileLocation target) =>
        ShowStatus(target == FileLocation.GoogleDrive
            ? "Передаю файл на Google Drive... Якщо відкрився браузер, дозвольте доступ"
            : "Зберігаю файл...");

    private void ShowStatus(string text) => StatusLabel.Text = text;

    private FileLocation SelectedTarget() =>
        DriveTargetRadio.IsChecked ? FileLocation.GoogleDrive : FileLocation.Local;

    private static FileLocation LocationOf(object? sender) =>
        sender is Button { CommandParameter: string value } && Enum.TryParse(value, out FileLocation location)
            ? location
            : FileLocation.Local;

    private static async Task OpenInDefaultAppAsync(string path) {
        bool opened = await UIApplication.SharedApplication.OpenUrlAsync(NSUrl.FromFilename(path), new UIApplicationOpenUrlOptions());
        if (!opened) throw new IOException($"Не вдалося відкрити {path}");
    }

    private static string Describe(string name, FileLocation location) =>
        location == FileLocation.GoogleDrive ? $"{name} (Google Drive)" : name;

    private string MethodTitle() => _controller.Method switch {
        SearchMethod.Dom => "DOM API",
        SearchMethod.Linq => "LINQ to XML",
        _ => "SAX API"
    };
}