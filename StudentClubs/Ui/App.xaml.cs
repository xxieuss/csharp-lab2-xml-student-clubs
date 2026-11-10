using Microsoft.Maui.Handlers;
using UIKit;

namespace Ui;

public partial class App : Application {
    private const string WindowTitle = "Студентські гуртки та семінари";

    private static bool _confirming;

    public App() {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;

        EntryHandler.Mapper.AppendToMapping("NoNativeFrame", (handler, view) =>
            handler.PlatformView.BorderStyle = UITextBorderStyle.None);
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        Window window = new Window(new AppShell()) {
            Title = WindowTitle,
            MinimumWidth = 1100,
            MinimumHeight = 720
        };
        window.Created += (sender, e) => DisableCloseButton(window);
        return window;
    }

    public static async Task ConfirmQuitAsync() {
        if (_confirming || Current?.Windows.FirstOrDefault()?.Page is not Page page) return;

        _confirming = true;
        try {
            bool quit = await page.DisplayAlertAsync("Завершення роботи",
                "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
            if (quit) Current?.Quit();
        } finally {
            _confirming = false;
        }
    }

    private static void DisableCloseButton(Window window) {
        if (!OperatingSystem.IsMacCatalystVersionAtLeast(16)) return;
        if (window.Handler?.PlatformView is not UIWindow platformWindow) return;

        UISceneWindowingBehaviors? behaviors = platformWindow.WindowScene?.WindowingBehaviors;
        if (behaviors != null) behaviors.Closable = false;
    }
}