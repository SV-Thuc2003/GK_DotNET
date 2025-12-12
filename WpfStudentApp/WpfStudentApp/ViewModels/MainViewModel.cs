using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WpfStudentApp.Model;

namespace WpfStudentApp.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        // ObservableCollection chứa danh sách sinh viên
        public ObservableCollection<Student> Students { get; set; }

        // ICollectionView hỗ trợ Search/Sort
        private ICollectionView _studentsView;

        // Search Keyword
        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                _studentsView.Refresh(); // tự động filter khi gõ
            }
        }

        // Selected student
        private Student? _selectedStudent = null;
        public Student? SelectedStudent
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

        // Input fields
        private string _inputName = string.Empty;
        public string InputName
        {
            get => _inputName;
            set { _inputName = value; OnPropertyChanged(); }
        }

        private int? _inputAge;
        public int? InputAge
        {
            get => _inputAge;
            set { _inputAge = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand SortAgeCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand ExportJsonCommand { get; }

        // Constructor
        public MainViewModel()
        {
            Students = new ObservableCollection<Student>();

            // Khởi tạo ICollectionView để filter/sort
            _studentsView = CollectionViewSource.GetDefaultView(Students);
            _studentsView.Filter = FilterStudents;

            // Commands
            AddCommand = new RelayCommand(o => AddStudent());
            DeleteCommand = new RelayCommand(o => DeleteStudent(), o => SelectedStudent != null);
            UpdateCommand = new RelayCommand(o => UpdateStudent(), o => SelectedStudent != null);
            SortAgeCommand = new RelayCommand(o => SortByAge());
            ExportCsvCommand = new RelayCommand(o => ExportToCsv());
            ExportJsonCommand = new RelayCommand(o => ExportToJson());
        }

        // --- Logic thêm ---
        private void AddStudent()
        {
            if (string.IsNullOrWhiteSpace(InputName) || InputAge <= 0) return;

            int newId = Students.Count > 0 ? Students.Max(s => s.Id) + 1 : 1;

            Students.Add(new Student { Id = newId, Name = InputName, Age = InputAge.Value });

            InputName = "";
            InputAge = 0;
        }

        // --- Logic xóa ---
        private void DeleteStudent()
        {
            if (SelectedStudent != null) Students.Remove(SelectedStudent);
        }

        // --- Logic cập nhật ---
        private void UpdateStudent()
        {
            if (SelectedStudent != null)
            {
                SelectedStudent.Name = InputName;
                SelectedStudent.Age = InputAge.Value;
                _studentsView.Refresh(); // refresh DataGrid ngay lập tức
            }
        }

        // --- Logic filter/Search ---
        private bool FilterStudents(object item)
        {
            if (string.IsNullOrEmpty(SearchKeyword)) return true;
            var student = (Student)item;
            return student.Name.IndexOf(SearchKeyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // --- Logic sort ---
        private void SortByAge()
        {
            _studentsView.SortDescriptions.Clear();
            _studentsView.SortDescriptions.Add(new SortDescription("Age", ListSortDirection.Ascending));
        }

        // --- Logic xuất file ---
        private void ExportToCsv()
        {
            var lines = new List<string> { "ID,Name,Age" };
            foreach (var s in Students) lines.Add($"{s.Id},{s.Name},{s.Age}");
            File.WriteAllLines("students.csv", lines, System.Text.Encoding.UTF8);
            MessageBox.Show("Đã xuất file students.csv!");
        }

        private void ExportToJson()
        {
            var json = JsonSerializer.Serialize(Students, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("students.json", json);
            MessageBox.Show("Đã xuất file students.json!");
        }

        // --- PropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
