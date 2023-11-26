using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentsInternship.Models
{
    partial class PracticeSchedules
    {
        public string IDStartEndDate
        {
            get
            {
                return $"{ID}|{PracticeStartDate.ToShortDateString()}-{PracticeEndDate.ToShortDateString()}";
            }
        }
        public string StartEndDateString
        {
            get
            {
                return $"{PracticeStartDate.ToShortDateString()}-{PracticeEndDate.ToShortDateString()}";
            }
        }
    }
}
