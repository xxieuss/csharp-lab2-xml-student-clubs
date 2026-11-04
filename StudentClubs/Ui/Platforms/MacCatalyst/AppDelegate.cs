using Foundation;
using ObjCRuntime;
using UIKit;

namespace Ui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate {
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void BuildMenu(IUIMenuBuilder builder) {
        base.BuildMenu(builder);
        if (builder.System != UIMenuSystem.MainSystem) return;

        string? close = UIMenuIdentifier.Close.GetConstant()?.ToString();
        if (close != null) builder.RemoveMenu(close);

        string? quit = UIMenuIdentifier.Quit.GetConstant()?.ToString();
        if (quit == null) return;

        UIKeyCommand command = UIKeyCommand.Create("Завершити роботу", null, new Selector("confirmQuit:"),
            "q", UIKeyModifierFlags.Command, null);
        builder.ReplaceMenu(quit, UIMenu.Create("", null, null, UIMenuOptions.DisplayInline,
            new UIMenuElement[] { command }));
    }

    [Export("confirmQuit:")]
    public async void ConfirmQuit(NSObject? sender) => await App.ConfirmQuitAsync();
}