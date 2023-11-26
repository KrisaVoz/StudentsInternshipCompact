using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentsInternship.Models
{
    partial class Specialties
    {
        public string NumberNameSpeciality 
        {
            get
            {
                return SpecialityNumber + " - " + SpecialityName;
            } 
        }
    }
}
