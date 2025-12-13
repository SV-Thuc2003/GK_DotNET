using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFStudent.Model;
using WPFStudent.Services;

namespace WPFStudent.ViewModels
{
    public class StudentFormViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly IStudentService _service;
        private readonly StudentListViewModel _listVm;

        public int Id { get; set; }
        public string StudentId { get; set; }  // <- Validate thêm
        private string _name;
        public string Name { get => _name; set { _name = value; Raise(); } }
        public int Age { get; set; } = 18;
        public string Gender { get; set; } = "Nam";

        private double _mathGrade;
        public double MathGrade
        {
            get => _mathGrade;
            set { _mathGrade = value; Raise(); Raise(nameof(GPA)); Raise(nameof(Rank)); }
        }

        private double _physicsGrade;
        public double PhysicsGrade
        {
            get => _physicsGrade;
            set { _physicsGrade = value; Raise(); Raise(nameof(GPA)); Raise(nameof(Rank)); }
        }

        private double _chemistryGrade;
        public double ChemistryGrade
        {
            get => _chemistryGrade;
            set { _chemistryGrade = value; Raise(); Raise(nameof(GPA)); Raise(nameof(Rank)); }
        }

        public string ImageBase64 { get; set; }
        public byte[] ImageBytes => string.IsNullOrEmpty(ImageBase64) ? null : Convert.FromBase64String(ImageBase64);

        public double GPA => Math.Round((MathGrade + PhysicsGrade + ChemistryGrade) / 3, 2);

        public string Rank => GPA >= 8 ? "Excellent" :
                              GPA >= 6.5 ? "Good" :
                              GPA >= 5 ? "Average" : "Poor";

        public RelayCommand SaveCommand { get; }
        public RelayCommand UploadImageCommand { get; }
        public RelayCommand ResetCommand { get; }

        public StudentFormViewModel(StudentListViewModel listVm) : this(listVm, new StudentService()) { }

        public StudentFormViewModel(StudentListViewModel listVm, IStudentService service)
        {
            _service = service;
            _listVm = listVm;
            _listVm.OnStudentSelected += Load;
            SaveCommand = new RelayCommand(async o => await SaveAsync(), o => CanSave());
            UploadImageCommand = new RelayCommand(o => UploadImage());
            ResetCommand = new RelayCommand(o => Reset());
        }

        private void Load(Student s)
        {
            if (s == null) return;
            Id = s.Id;
            StudentId = s.StudentId;
            Name = s.Name;
            Age = s.Age;
            Gender = s.Gender;
            MathGrade = s.MathGrade;
            PhysicsGrade = s.PhysicsGrade;
            ChemistryGrade = s.ChemistryGrade;
            ImageBase64 = s.ImageBase64;
            Raise(""); // refresh all binding
        }

        private void UploadImage()
        {
            var dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                var bytes = File.ReadAllBytes(dlg.FileName);
                ImageBase64 = Convert.ToBase64String(bytes);
                Raise(nameof(ImageBase64));
                Raise(nameof(ImageBytes));
            }
        }

        private bool CanSave() => string.IsNullOrWhiteSpace(Error);

        private async Task SaveAsync()
        {
            if (!CanSave())
            {
                MessageBox.Show(Error, "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check exists trước khi đẩy xuống DB
            if (await _service.ExistsStudentIdAsync(StudentId, Id))
            {
                MessageBox.Show("Mã sinh viên đã tồn tại!", "Trùng MSSV", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var s = new Student
            {
                Id = Id,
                StudentId = StudentId,
                Name = Name,
                Age = Age,
                Gender = Gender,
                MathGrade = MathGrade,
                PhysicsGrade = PhysicsGrade,
                ChemistryGrade = ChemistryGrade,
                ImageBase64 = ImageBase64
            };

            try
            {
                if (Id == 0)
                {
                    await _service.AddAsync(s);
                    _listVm.Students.Add(s);
                }
                else
                {
                    await _service.UpdateAsync(s);

                    var exist = _listVm.Students.FirstOrDefault(x => x.Id == s.Id);
                    if (exist != null)
                    {
                        var idx = _listVm.Students.IndexOf(exist);
                        _listVm.Students[idx] = s;
                    }
                }

                MessageBox.Show("Lưu thành công!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                _listVm.StudentsView.Refresh();
                Reset();
            }
            catch (Exception ex)
            {
                // Bắt mọi lỗi ghi DB
                MessageBox.Show($"Lỗi khi lưu:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Reset()
        {
            Id = 0;
            StudentId = string.Empty;
            Name = string.Empty;
            Age = 18;
            Gender = "Nam";
            MathGrade = PhysicsGrade = ChemistryGrade = 0;
            ImageBase64 = null;
            Raise("");
        }

        // IDataErrorInfo validation
        public string Error
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StudentId)) return "StudentId is required.";
                if (string.IsNullOrWhiteSpace(Name)) return "Name is required.";
                if (Age <= 0 || Age > 120) return "Age invalid.";
                if (MathGrade < 0 || MathGrade > 10) return "Math grade 0-10.";
                if (PhysicsGrade < 0 || PhysicsGrade > 10) return "Physics grade 0-10.";
                if (ChemistryGrade < 0 || ChemistryGrade > 10) return "Chemistry grade 0-10.";
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(StudentId) when string.IsNullOrWhiteSpace(StudentId) => "StudentId is required.",
                    nameof(Name) when string.IsNullOrWhiteSpace(Name) => "Name is required.",
                    nameof(Age) when Age <= 0 || Age > 120 => "Age invalid.",
                    _ => null
                };
            }
        }
    }
}
