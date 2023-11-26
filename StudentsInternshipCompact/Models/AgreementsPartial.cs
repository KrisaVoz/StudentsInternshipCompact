using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentsInternship.Models
{
    partial class Agreements
    {
        public string SpecialitiesListString
        {
            get
            {
                var specialityagreement = AgreementSpeciality.ToList();
                if (specialityagreement.Count > 0)
                {
                    string txt = string.Empty;
                    
                    foreach (var speciality in specialityagreement)
                    {
                        if (speciality.Specialties != null)
                            txt +=$"{speciality.Specialties.SpecialityNumber} - {speciality.Specialties.SpecialityName}\n";
                        else
                        {
                            return "Специальности не найдены или не обновлены. Добавьте или сохраните специальности!";
                        }
                    }
                    txt = txt.TrimEnd('\n');
                    return txt;
                }
                else
                {
                    return "Специальностей не найдено! Добавьте специальности";
                }
            }
        }
    }
}
