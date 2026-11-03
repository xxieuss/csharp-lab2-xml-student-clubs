using Microsoft.Maui.Controls.Shapes;
using UIKit;

namespace Ui;

public class ValueSelector : ContentView {
    private const string NoFileText = "Спочатку відкрийте XML-файл";

    private readonly Label _text = new Label {
        FontSize = 13,
        VerticalTextAlignment = TextAlignment.Center,
        LineBreakMode = LineBreakMode.TailTruncation
    };

    private readonly Button _opener = new Button {
        Text = "",
        BackgroundColor = Colors.Transparent,
        BorderWidth = 0,
        Padding = new Thickness(0),
        HeightRequest = 36,
        MinimumHeightRequest = 0
    };

    private List<string> _items = new List<string>();
    private int _selectedIndex = -1;
    private string _placeholder = "";

    public string Placeholder {
        get => _placeholder;
        set {
            _placeholder = value;
            Refresh();
        }
    }

    public IReadOnlyList<string> ItemsSource {
        get => _items;
        set {
            _items = new List<string>(value);
            SelectedIndex = _items.Count > 0 ? 0 : -1;
        }
    }

    public int SelectedIndex {
        get => _selectedIndex;
        set {
            _selectedIndex = value >= 0 && value < _items.Count ? value : -1;
            Refresh();
        }
    }

    public string? SelectedItem => _selectedIndex >= 0 ? _items[_selectedIndex] : null;

    public event EventHandler? SelectionChanged;

    public ValueSelector() {
        Label arrow = new Label { Text = "▾", FontSize = 13, VerticalTextAlignment = TextAlignment.Center };
        arrow.SetDynamicResource(Label.TextColorProperty, "TextDim");

        Grid row = new Grid {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        row.Add(_text, 0, 0);
        row.Add(arrow, 1, 0);

        Border frame = new Border {
            Content = row,
            HeightRequest = 36,
            Padding = new Thickness(10, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 6 }
        };
        frame.SetDynamicResource(BackgroundColorProperty, "Field");
        frame.SetDynamicResource(Border.StrokeProperty, "CardBorder");

        _opener.HandlerChanged += (sender, e) => UpdateMenu();

        Grid root = new Grid();
        root.Add(frame);
        root.Add(_opener);

        Content = root;
        Refresh();
    }

    private void Choose(int index) {
        if (index == _selectedIndex) return;

        SelectedIndex = index;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Refresh() {
        bool empty = _selectedIndex < 0;
        _text.Text = empty ? _placeholder : _items[_selectedIndex];
        _text.SetDynamicResource(Label.TextColorProperty, empty ? "TextDim" : "Text");

        UpdateMenu();
    }

    private void UpdateMenu() {
        if (_opener.Handler?.PlatformView is not UIButton button) return;

        List<UIMenuElement> actions = new List<UIMenuElement>();

        if (_items.Count == 0) {
            UIAction hint = UIAction.Create(NoFileText, null, null, _ => { });
            hint.Attributes = UIMenuElementAttributes.Disabled;
            actions.Add(hint);
        }

        for (int i = 0; i < _items.Count; i++) {
            int index = i;
            UIAction action = UIAction.Create(_items[i], null, null, _ => Choose(index));
            if (i == _selectedIndex) action.State = UIMenuElementState.On;
            actions.Add(action);
        }

        button.Menu = UIMenu.Create("", actions.ToArray());
        button.ShowsMenuAsPrimaryAction = true;
    }
}