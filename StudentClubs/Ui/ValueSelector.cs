using Microsoft.Maui.Controls.Shapes;
using UIKit;

namespace Ui;

public class ValueSelector : ContentView {
    private const string NoFileText = "Спочатку відкрийте XML-файл";
    private const double FieldHeight = 54;

    private readonly Label _caption = new Label {
        FontSize = 11,
        LineBreakMode = LineBreakMode.TailTruncation
    };

    private readonly Label _text = new Label {
        FontSize = 13,
        VerticalTextAlignment = TextAlignment.Center,
        LineBreakMode = LineBreakMode.TailTruncation
    };

    private readonly Border _frame = new Border {
        HeightRequest = FieldHeight,
        Padding = new Thickness(12, 7),
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 10 }
    };

    private readonly Button _opener = new Button {
        Text = "",
        BackgroundColor = Colors.Transparent,
        BorderWidth = 0,
        Padding = new Thickness(0),
        HeightRequest = FieldHeight,
        MinimumHeightRequest = 0
    };

    private List<string> _items = new List<string>();
    private int _selectedIndex = -1;
    private string _placeholder = "";

    public string Caption {
        get => _caption.Text;
        set => _caption.Text = value;
    }

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

        Grid layout = new Grid {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) },
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
            RowSpacing = 1
        };
        layout.Add(_caption, 0, 0);
        layout.Add(_text, 0, 1);
        layout.Add(arrow, 1, 0);
        Grid.SetRowSpan(arrow, 2);

        _frame.Content = layout;
        _frame.SetDynamicResource(BackgroundColorProperty, "Field");

        _opener.HandlerChanged += (sender, e) => UpdateMenu();

        Grid root = new Grid();
        root.Add(_frame);
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

        bool filters = _selectedIndex > 0;
        _frame.SetDynamicResource(Border.StrokeProperty, filters ? "Accent" : "CardBorder");
        _caption.SetDynamicResource(Label.TextColorProperty, filters ? "Accent" : "TextDim");

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