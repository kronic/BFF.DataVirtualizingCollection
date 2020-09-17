using System.Windows;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels;

namespace BFF.DataVirtualizingCollection.Sample.View.Views
{
    public partial class MainWindowView
    {
        public static Visibility IsDebug
        {
#if DEBUG
            get { return Visibility.Visible; }
#else
            get { return Visibility.Collapsed; }
#endif
        }

        public MainWindowView(IMainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            DataContext = mainWindowViewModel;
        }
    }
}