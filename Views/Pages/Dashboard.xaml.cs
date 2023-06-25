using PlayRandom.ViewModels;

namespace PlayRandom.Views.Pages;

public partial class Dashboard {
    public Dashboard() {
        InitializeComponent();
    }

    DashboardViewModel ViewModel => (DashboardViewModel)DataContext; // NOTE: Technically violates MVVM, but sometimes that's necessary

    void Selector_OnSelectionChanged( object Sender, SelectionChangedEventArgs E ) {
        int Index = ((Selector)Sender).SelectedIndex;
        ViewModel.PlaybackCommand.Execute(Index);
    }

    void PlaybackOption_OnClick( object Sender, RoutedEventArgs E ) {
        ButtonBase Button = (ButtonBase)Sender;

        // Retrieve the listbox parent
        ListBoxItem? Item = Button.FindAncestor<ListBoxItem>(); // Definitely violates MVVM, but the proper alternative involves a lot of bindings and is a lot more complicated, harder to maintain, harder to understand, and badly inefficient
        if (Item == null) {
            Debug.Fail("Could not find ListBoxItem parent");
            return;
        }

        ListBox? ListBox = Item.FindAncestor<ListBox>();
        if (ListBox == null) {
            Debug.Fail("Could not find ListBox parent");
            return;
        }

        // Retrieve the index of the listbox item, then use it to invoke the command
        int Index = ListBox.ItemContainerGenerator.IndexFromContainer(Item);
        ViewModel.PlaybackCommand.Execute(Index);
    }
}
