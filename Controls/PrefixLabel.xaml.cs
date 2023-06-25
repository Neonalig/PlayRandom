namespace PlayRandom.Controls;

[ContentProperty(nameof(Children))]
public partial class PrefixLabel {
    /// <summary> The text to display in the label. </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(PrefixLabel), new(string.Empty));

    /// <summary> Gets or sets the text to display in the label. </summary>
    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary> The width of the label. </summary>
    public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(nameof(LabelWidth), typeof(double), typeof(PrefixLabel), new(100d));

    /// <summary> Gets or sets the width of the label. </summary>
    public double LabelWidth {
        get => (double)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    /// <summary> The content to display after the label. </summary>
    public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(nameof(Children), typeof(UIElementCollection), typeof(PrefixLabel), new());

    /// <summary> Gets or sets the content to display after the label. </summary>
    public UIElementCollection Children {
        get => (UIElementCollection)GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }

    public PrefixLabel() {
        InitializeComponent();
        DataContext = this;
        Children    = ContentPanel.Children;
    }
}
