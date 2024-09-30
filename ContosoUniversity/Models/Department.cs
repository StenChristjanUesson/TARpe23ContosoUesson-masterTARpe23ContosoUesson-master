using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }
        [StringLength(50, MinimumLength =3)]
        public string Name { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName ="Money")]
        public decimal Budget { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime StartTime { get; set; } 
        
        /*
         * kaks oma andmetüüpi osakonna jaoks
         */
        public Student? StudentGrades { get; set; }//Minu isiklikud hinded.
        [Display(Name = "This students Grades are:")]
        public string? Personality { get; set; }//Minu õpilaste iseloomu esindavad näited.
        public int? InstructorID { get; set; }
        [Timestamp]
        public byte? RowVersion { get; set; }//Sometype of timestamp
        public Instructor? Administrator { get; set; }
        public ICollection<Course>? Courses { get; set; }
    }
}
