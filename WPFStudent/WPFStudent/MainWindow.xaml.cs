using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFStudent.Data;
using WPFStudent.ViewModels;

namespace WPFStudent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            //var listVm = new StudentListViewModel();
            //ListViewControl.DataContext = listVm;

            //var formVm = new StudentFormViewModel(listVm);
            //FormViewControl.DataContext = formVm;

            //_ = listVm.LoadAsync();
        }
    }
}