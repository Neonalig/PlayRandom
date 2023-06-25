namespace PlayRandom.Controls;

public partial class FolderPicker {
    public FolderPicker() {
        InitializeComponent();
        DataContext   = this;
        BrowseCommand = new RelayCommand(Browse);
    }

    /// <summary> The selected folder. </summary>
    public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(nameof(SelectedPath), typeof(string), typeof(FolderPicker));

    /// <summary> Gets or sets the selected folder. </summary>
    public string SelectedPath {
        get => (string)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }

    /// <summary> The command to browse for a folder. </summary>
    public ICommand BrowseCommand { get; }

    /// <summary> The text of the browse button. </summary>
    public static readonly DependencyProperty BrowseTextProperty = DependencyProperty.Register(nameof(BrowseText), typeof(string), typeof(FolderPicker), new() { DefaultValue = "Browse..." });

    /// <summary> Gets or sets the text of the browse button. </summary>
    public string BrowseText {
        get => (string)GetValue(BrowseTextProperty);
        set => SetValue(BrowseTextProperty, value);
    }

    /// <summary> The title of the browse dialog. </summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(FolderPicker), new() { DefaultValue = "Select a folder" });

    /// <summary> Gets or sets the title of the browse dialog. </summary>
    public string Title {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    static string WithSeparator( string Path ) {
        if (string.IsNullOrEmpty(Path)) {
            return Path;
        }
        return Path[^1] == System.IO.Path.DirectorySeparatorChar ? Path : $"{Path}{System.IO.Path.DirectorySeparatorChar}";
    }

    /// <summary> Browses for a folder. </summary>
    void Browse() {
        VistaFolderBrowserDialog Dialog = new() {
            Description            = Title,
            UseDescriptionForTitle = true,
            SelectedPath           = string.IsNullOrEmpty(SelectedPath) ? Environment.CurrentDirectory : WithSeparator(SelectedPath)
        };
        if (Dialog.ShowDialog() == true) {
            SelectedPath = Dialog.SelectedPath;
        }
    }

    void TextBox_OnKeyDown( object Sender, KeyEventArgs E ) {
        if (E.Key is not Key.Enter or not Key.Return) {
            return;
        }
        SubmitButton.Focus();
    }
}
