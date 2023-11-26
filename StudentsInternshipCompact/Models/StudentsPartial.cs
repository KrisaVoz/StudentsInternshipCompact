using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentsInternship.Models
{
    partial class Students
    {
        public string FullName
        {
            get
            {
                return $"{Name} {Surname} {Patronymic}"; 
            }
        }
    }
}
