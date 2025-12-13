using System.Collections.Generic;
using System.Threading.Tasks;
using WPFStudent.Model;

namespace WPFStudent.Services
{
    public interface IStudentService
    {
        Task<List<Student>> GetAllAsync();
        Task<Student> GetByIdAsync(int id);
        Task<Student> AddAsync(Student s);
        Task<bool> ExistsStudentIdAsync(string studentId, int excludeId = 0);
        Task UpdateAsync(Student s);
        Task DeleteAsync(int id);
        Task ImportFromExcelAsync(string filePath);
        Task ExportToExcelAsync(string filePath, IEnumerable<Student> students);
    }
}
