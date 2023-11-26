using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using StudentsInternship.Models;

namespace StudentsInternship
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static StudentsIntershipDBEntities Context { get; } = new StudentsIntershipDBEntities();


        //This code is in model and it's creating the database if it's not exists I've put it here if model erase that code somehow
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

/*        class MyContextInitializer : CreateDatabaseIfNotExists<StudentsIntershipDBEntities>
        {
            protected override void Seed(StudentsIntershipDBEntities db)
            {
                AgreementTypes type1 = new AgreementTypes { AgreementTypeName = "Краткосрочный" };
                AgreementTypes type2 = new AgreementTypes { AgreementTypeName = "Долгосрочный" };
                AgreementTypes type3 = new AgreementTypes { AgreementTypeName = "Бессрочный" };
                AgreementTypes type4 = new AgreementTypes { AgreementTypeName = "Иной" };

                db.AgreementTypes.Add(type1);
                db.AgreementTypes.Add(type2);
                db.AgreementTypes.Add(type3);
                db.AgreementTypes.Add(type4);
                db.SaveChanges();
            }
        }

        public partial class StudentsIntershipDBEntities : DbContext
        {
            static StudentsIntershipDBEntities()
            {
                Database.SetInitializer<StudentsIntershipDBEntities>(new MyContextInitializer());
            }*/
    }
}
