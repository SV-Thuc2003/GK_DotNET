using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfStudentApp.Model;

namespace WpfStudentApp.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Student> Students { get; set; }

        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();

                if (value != null)
                {
                    InputName = value.Name;
                    InputAge = value.Age;
                }
            }
        }

        private string _inputName;
        public string InputName
        {
            get => _inputName;
            set { _inputName = value; OnPropertyChanged(); }
        }

        private int _inputAge;
        public int InputAge
        {
            get => _inputAge;
            set { _inputAge = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateCommand { get; }

        public MainViewModel()
        {
            Students = new ObservableCollection<Student>();

            AddCommand = new RelayCommand(o => AddStudent());
            DeleteCommand = new RelayCommand(o => DeleteStudent(), o => SelectedStudent != null);
            UpdateCommand = new RelayCommand(o => UpdateStudent(), o => SelectedStudent != null);
        }

        private void AddStudent()
        {
            if (string.IsNullOrWhiteSpace(InputName) || InputAge <= 0)
                return;

            Students.Add(new Student { Name = InputName, Age = InputAge });

            InputName = "";
            InputAge = 0;
        }

        private void DeleteStudent()
        {
            if (SelectedStudent != null)
                Students.Remove(SelectedStudent);
        }

        private void UpdateStudent()
        {
            if (SelectedStudent != null)
            {
                SelectedStudent.Name = InputName;
                SelectedStudent.Age = InputAge;
                OnPropertyChanged(nameof(Students));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
