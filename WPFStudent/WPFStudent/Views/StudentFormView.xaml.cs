using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFStudent.Views
{
    public partial class StudentFormView : UserControl
    {
        public StudentFormView()
        {
            InitializeComponent();
        }
        private void StudentId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }
    }
}
