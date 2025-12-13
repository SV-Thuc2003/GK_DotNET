using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WPFStudent.Data;
using WPFStudent.Model;

namespace WPFStudent.Services
{
    public class StudentService : IStudentService
    {
        public async Task<List<Student>> GetAllAsync()
        {
            using var db = new AppDbContext();
            return await db.Students.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
        }

        public async Task<Student> GetByIdAsync(int id)
        {
            using var db = new AppDbContext();
            return await db.Students.FindAsync(id);
        }

        public async Task<Student> AddAsync(Student s)
        {
            using var db = new AppDbContext();
            db.Students.Add(s);
            await db.SaveChangesAsync();
            return s;
        }

        public async Task<bool> ExistsStudentIdAsync(string studentId, int excludeId = 0)
        {
            using var db = new AppDbContext();
            return await db.Students
                           .AnyAsync(s => s.StudentId == studentId && s.Id != excludeId);
        }

        public async Task UpdateAsync(Student s)
        {
            using var db = new AppDbContext();
            db.Students.Update(s);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = new AppDbContext();
            var s = await db.Students.FindAsync(id);
            if (s != null) { db.Students.Remove(s); await db.SaveChangesAsync(); }
        }

        public async Task ImportFromExcelAsync(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            var ws = workbook.Worksheets.First();
            var list = new List<Student>();
            var rows = ws.RowsUsed().Skip(1);
            foreach (var r in rows)
            {
                var s = new Student
                {
                    StudentId = r.Cell(1).GetString(),
                    Name = r.Cell(2).GetString(),
                    Age = (int)r.Cell(3).GetDouble(),
                    Gender = r.Cell(4).GetString(),
                    MathGrade = r.Cell(5).GetDouble(),
                    PhysicsGrade = r.Cell(6).GetDouble(),
                    ChemistryGrade = r.Cell(7).GetDouble(),
                    ImageBase64 = r.Cell(8).GetString()
                };
                list.Add(s);
            }
            using var db = new AppDbContext();
            await db.Students.AddRangeAsync(list);
            await db.SaveChangesAsync();
        }

        public async Task ExportToExcelAsync(string filePath, IEnumerable<Student> students)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Students");
            ws.Cell(1, 1).Value = "StudentId";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Age";
            ws.Cell(1, 4).Value = "Gender";
            ws.Cell(1, 5).Value = "Math";
            ws.Cell(1, 6).Value = "Physics";
            ws.Cell(1, 7).Value = "Chemistry";
            ws.Cell(1, 8).Value = "ImageBase64";

            var row = 2;
            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = s.StudentId;
                ws.Cell(row, 2).Value = s.Name;
                ws.Cell(row, 3).Value = s.Age;
                ws.Cell(row, 4).Value = s.Gender;
                ws.Cell(row, 5).Value = s.MathGrade;
                ws.Cell(row, 6).Value = s.PhysicsGrade;
                ws.Cell(row, 7).Value = s.ChemistryGrade;
                ws.Cell(row, 8).Value = s.ImageBase64 ?? "";
                row++;
            }
            wb.SaveAs(filePath);
            await Task.CompletedTask;
        }
    }
}
