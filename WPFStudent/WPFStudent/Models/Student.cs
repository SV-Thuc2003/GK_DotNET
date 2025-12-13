using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFStudent.Model
{
    public class Student
    {

        public int Id { get; set; } // Auto tăng
        public string StudentId { get; set; } // Mã sinh viên
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }

        //Image student
        public string ImageBase64 { get; set; }

        // Grade point average
        public double MathGrade { get; set; }
        public double PhysicsGrade { get; set; }
        public double ChemistryGrade { get; set; }

        //GPA calculation
        public double GPA => Math.Round((MathGrade + PhysicsGrade + ChemistryGrade) / 3, 2);

        // Sort 
        public string Rank
        {
            get
            {
                if (GPA >= 8.0) return "Excellent";
                else if (GPA >= 6.5) return "Good";
                else if (GPA >= 5.0) return "Average";
                else return "Poor";
            }
        }
    }
}
