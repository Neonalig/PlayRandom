namespace PlayRandom.Controls;

public partial class FilePicker {
    public FilePicker() {
        InitializeComponent();
        DataContext   = this;
        BrowseCommand = new RelayCommand(Browse);
    }

    /// <summary> The selected file. </summary>
    public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(nameof(SelectedPath), typeof(string), typeof(FilePicker));

    /// <summary> Gets or sets the selected file. </summary>
    public string SelectedPath {
        get => (string)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }

    /// <summary> The command to browse for a file. </summary>
    public ICommand BrowseCommand { get; }

    /// <summary> The text of the browse button. </summary>
    public static readonly DependencyProperty BrowseTextProperty = DependencyProperty.Register(nameof(BrowseText), typeof(string), typeof(FilePicker), new() { DefaultValue = "Browse..." });

    /// <summary> Gets or sets the text of the browse button. </summary>
    public string BrowseText {
        get => (string)GetValue(BrowseTextProperty);
        set => SetValue(BrowseTextProperty, value);
    }

    /// <summary> The title of the browse dialog. </summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(FilePicker), new() { DefaultValue = "Select a file" });

    /// <summary> Gets or sets the title of the browse dialog. </summary>
    public string Title {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary> The filter of the browse dialog. </summary>
    public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FilePicker), new() { DefaultValue = "All files (*.*)|*.*" });

    /// <summary> Gets or sets the filter of the browse dialog. </summary>
    public string Filter {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    /// <summary> The filter index of the browse dialog. </summary>
    public static readonly DependencyProperty FilterIndexProperty = DependencyProperty.Register(nameof(FilterIndex), typeof(int), typeof(FilePicker), new() { DefaultValue = 1 });

    /// <summary> Gets or sets the filter index of the browse dialog. </summary>
    public int FilterIndex {
        get => (int)GetValue(FilterIndexProperty);
        set => SetValue(FilterIndexProperty, value);
    }

    /// <summary> Browses for a file. </summary>
    void Browse() {
        VistaOpenFileDialog Dialog = new() {
            Title            = Title,
            InitialDirectory = string.IsNullOrEmpty(SelectedPath) ? Environment.CurrentDirectory : Path.GetDirectoryName(SelectedPath),
            FileName         = string.IsNullOrEmpty(SelectedPath) ? string.Empty : Path.GetFileName(SelectedPath),
            Filter           = Filter,
            FilterIndex      = FilterIndex
        };
        if (Dialog.ShowDialog() == true) {
            SelectedPath = Dialog.FileName;
        }
    }

    void TextBox_OnKeyDown( object Sender, KeyEventArgs E ) {
        if (E.Key is not Key.Enter or not Key.Return) {
            return;
        }
        SubmitButton.Focus();
    }
}
