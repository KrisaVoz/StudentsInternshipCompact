//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StudentsInternship.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class StudentsDistribution
    {
        public int PracticeDistributionsID { get; set; }
        public int StudentID { get; set; }
        public Nullable<System.DateTime> DateOfAdding { get; set; }
    
        public virtual PracticeDistributions PracticeDistributions { get; set; }
        public virtual Students Students { get; set; }
    }
}
