using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Threading.Tasks;
using WPFStudent.Model;
using WPFStudent.Services;
using Microsoft.Win32;
using System.Collections.Generic;

namespace WPFStudent.ViewModels
{
    public class StudentListViewModel : BaseViewModel
    {
        private readonly IStudentService _service;

        public ObservableCollection<Student> Students { get; } = new ObservableCollection<Student>();
        public ICollectionView StudentsView { get; }

        private Student _selectedStudent;
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set { _selectedStudent = value; Raise(); OnStudentSelected?.Invoke(value); }
        }

        public event Action<Student> OnStudentSelected;

        // Search/filter/sort/paging props
        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { _searchKeyword = value; Raise(); StudentsView.Refresh(); }
        }

        private string _genderFilter = "All";
        public string GenderFilter { get => _genderFilter; set { _genderFilter = value; Raise(); StudentsView.Refresh(); } }

        // Paging
        private int _pageSize = 10;
        public int PageSize { get => _pageSize; set { _pageSize = value; Raise(); UpdatePaging(); } }

        private int _pageIndex = 1;
        public int PageIndex { get => _pageIndex; set { _pageIndex = Math.Max(1, value); Raise(); UpdatePaging(); } }

        public int TotalPages { get; private set; }

        // Commands
        public RelayCommand LoadCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SortByAgeCommand { get; }
        public RelayCommand ImportCommand { get; }
        public RelayCommand ExportCommand { get; }
        public RelayCommand PrevPageCommand { get; }
        public RelayCommand NextPageCommand { get; }

        public StudentListViewModel() : this(new StudentService()) {
            _ = LoadAsync();
        }

        public StudentListViewModel(IStudentService service)
        {
            _service = service;
            StudentsView = CollectionViewSource.GetDefaultView(Students);
            StudentsView.Filter = FilterPredicate;
            StudentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Name), ListSortDirection.Ascending));

            LoadCommand = new RelayCommand(async o => await LoadAsync());
            AddCommand = new RelayCommand(o => { /* navigate to add via MainVM */ });
            EditCommand = new RelayCommand(o => { /* navigate to edit */ }, o => SelectedStudent != null);
            DeleteCommand = new RelayCommand(async o => await DeleteSelectedAsync(), o => SelectedStudent != null);
            SortByAgeCommand = new RelayCommand(o => ToggleSortAge());
            ImportCommand = new RelayCommand(async o => await ImportAsync());
            ExportCommand = new RelayCommand(async o => await ExportAsync());
            PrevPageCommand = new RelayCommand(o => { if (PageIndex > 1) PageIndex--; }, o => PageIndex > 1);
            NextPageCommand = new RelayCommand(o => { if (PageIndex < TotalPages) PageIndex++; }, o => PageIndex < TotalPages);
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not Student s) return false;

            // search
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var q = SearchKeyword.Trim().ToLower();
                if (!(s.Name?.ToLower().Contains(q) == true || s.StudentId?.ToLower().Contains(q) == true))
                    return false;
            }

            // gender filter
            if (GenderFilter != "All" && !string.Equals(s.Gender, GenderFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            // Paging handled via index range
            int index = Students.IndexOf(s);
            int start = (PageIndex - 1) * PageSize;
            return index >= start && index < start + PageSize;
        }

        public async Task LoadAsync()
        {
            Students.Clear();
            var list = await _service.GetAllAsync();
            foreach (var s in list) Students.Add(s);

            UpdatePaging();
        }

        private void UpdatePaging()
        {
            int total = Students.Count;
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;
            if (PageIndex > TotalPages) PageIndex = TotalPages;
            Raise(nameof(TotalPages));
            StudentsView.Refresh();
        }

        private void ToggleSortAge()
        {
            var sd = StudentsView.SortDescriptions.FirstOrDefault();
            StudentsView.SortDescriptions.Clear();
            if (sd.PropertyName == nameof(Student.Age) && sd.Direction == ListSortDirection.Ascending)
                StudentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Age), ListSortDirection.Descending));
            else
                StudentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Age), ListSortDirection.Ascending));
        }

        private async Task DeleteSelectedAsync()
        {
            if (SelectedStudent == null) return;
            // Show dialog via UI (Material DialogHost) - here assume confirmed
            await _service.DeleteAsync(SelectedStudent.Id);
            Students.Remove(SelectedStudent);
            SelectedStudent = null;
            UpdatePaging();
        }

        private async Task ImportAsync()
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
            if (dlg.ShowDialog() == true)
            {
                await _service.ImportFromExcelAsync(dlg.FileName);
                await LoadAsync();
            }
        }

        private async Task ExportAsync()
        {
            var dlg = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "students.xlsx" };
            if (dlg.ShowDialog() == true)
            {
                await _service.ExportToExcelAsync(dlg.FileName, Students);
            }
        }
    }
}

