using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentsInternship.Models
{
    partial class PracticeDistributions
    {
        public string StudentsList
        {
            get
            {
                string txt = string.Empty;
                foreach (var distribution in StudentsDistribution)
                {
                    txt +=($"{distribution.Students.FullName} | {distribution.Students.Phone}\n");
                }
                txt = txt.TrimEnd('\n');
                return txt;
            }
        }
    }
}
