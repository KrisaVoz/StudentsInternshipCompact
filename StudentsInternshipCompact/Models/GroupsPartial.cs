using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentsInternship.Models
{
    partial class Groups
    {
        public string NumberOrganisationText
        {
            get
            {
                return $"{GroupNumberName} | {EducationalOrganisations.Name}";
            }
        }
    }
}
