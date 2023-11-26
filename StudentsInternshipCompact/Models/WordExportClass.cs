using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;
using Xceed.Document.NET;
using Font = Xceed.Document.NET.Font;
using System.Diagnostics;

namespace StudentsInternship.Models
{
    static class WordExportClass
    {
        public static void WordExport(string filename, List<StudentsDistribution> studentsDistributionsList)
        {
            try
            {
                var doc = DocX.Create(filename);
                //Formatting Title  
                string title = "УВЕДОМЛЕНИЕ" + Environment.NewLine + "о направлении на практику";
                //Formatting Title  
                // like using this we can set font family, font size, position, font color etc

                Formatting titleFormat = new Formatting();
                //Specify font family  
                titleFormat.FontFamily = new Font("Times New Roman");
                //Specify font size  
                titleFormat.Size = 14D;
                titleFormat.Position = 0;
                titleFormat.Bold = true;


                //Text  
                string textParagraph = "Прошу принять для прохождения практики следующих обучающихся:";

                //Formatting Text Paragraph  
                Formatting textParagraphFormat = new Formatting();
                //font family  
                textParagraphFormat.FontFamily = new Font("Times New Roman");
                //font size  
                textParagraphFormat.Size = 12D;
                //Spaces between characters  
                //Insert title  
                Paragraph paragraphTitle = doc.InsertParagraph(title, false, titleFormat);
                paragraphTitle.Alignment = Alignment.center;
                paragraphTitle.LineSpacingAfter = 12f;
                //Insert text  
                Paragraph textBeforeTable = doc.InsertParagraph(textParagraph, false, textParagraphFormat);
                textBeforeTable.Alignment = Alignment.left;
                textBeforeTable.LineSpacingAfter = 0;
                textBeforeTable.IndentationFirstLine = 35.4f;

                doc.InsertParagraph();
                //Create Table
                Table t = doc.AddTable(studentsDistributionsList.Count + 1, 6);
                t.Alignment = Alignment.center;
                // Fill cells by adding text.  
                // First row
                t.Rows[0].Cells[0].Paragraphs.First().Append($"№{Environment.NewLine}п/п", textParagraphFormat);
                t.Rows[0].Cells[1].Paragraphs.First().Append($"ФИО обучающегося{Environment.NewLine}контактный телефон", textParagraphFormat);
                t.Rows[0].Cells[2].Paragraphs.First().Append($"Направление подготовки{Environment.NewLine}(профессия)", textParagraphFormat);
                t.Rows[0].Cells[3].Paragraphs.First().Append($"Планируемый срок начала и окончания прохождения практики", textParagraphFormat);
                t.Rows[0].Cells[4].Paragraphs.First().Append($"Тематика практики{Environment.NewLine}(ПМ, МДК)", textParagraphFormat);
                t.Rows[0].Cells[5].Paragraphs.First().Append($"Наставник Ф.И.О{Environment.NewLine}Должность{Environment.NewLine}Контактный телефон", textParagraphFormat);
                t.Rows[0].Cells[0].Paragraphs.First().Alignment = Alignment.center;
                t.Rows[0].Cells[1].Paragraphs.First().Alignment = Alignment.center;
                t.Rows[0].Cells[2].Paragraphs.First().Alignment = Alignment.center;
                t.Rows[0].Cells[0].Paragraphs.First().Alignment = Alignment.center;
                t.Rows[0].Cells[3].Paragraphs.First().Alignment = Alignment.center;
                t.Rows[0].Cells[4].Paragraphs.First().Alignment = Alignment.center;
                t.Rows[0].Cells[5].Paragraphs.First().Alignment = Alignment.center;

                int i = 1;
                int mergeStart = 1;
                var practiceScheduleCurrent = new PracticeSchedules();
                foreach (var student in studentsDistributionsList)
                {
                    if (i == 1)
                    {
                        practiceScheduleCurrent = student.PracticeDistributions.PracticeSchedules;
                    }
                    t.Rows[i].Cells[0].Paragraphs.First().Append(i.ToString(), textParagraphFormat);
                    t.Rows[i].Cells[1].Paragraphs.First().Append(student.Students.FullName + Environment.NewLine + student.Students.Phone, textParagraphFormat);
                    t.Rows[i].Cells[0].Paragraphs.First().Alignment = Alignment.center;
                    t.Rows[i].Cells[1].Paragraphs.First().Alignment = Alignment.center;
                    t.Rows[i].Cells[2].Paragraphs.First().Alignment = Alignment.center;
                    t.Rows[i].Cells[0].Paragraphs.First().Alignment = Alignment.center;
                    t.Rows[i].Cells[3].Paragraphs.First().Alignment = Alignment.center;
                    t.Rows[i].Cells[4].Paragraphs.First().Alignment = Alignment.center;
                    t.Rows[i].Cells[5].Paragraphs.First().Alignment = Alignment.center;

                    if (student.PracticeDistributions.PracticeSchedules != practiceScheduleCurrent || i == studentsDistributionsList.Count)
                    {
                        t.MergeCellsInColumn(2, mergeStart, i);
                        t.MergeCellsInColumn(3, mergeStart, i);
                        t.MergeCellsInColumn(4, mergeStart, i);
                        t.Rows[mergeStart].Cells[2].Paragraphs.First().Append(practiceScheduleCurrent.Groups.Specialties.NumberNameSpeciality, textParagraphFormat);
                        t.Rows[mergeStart].Cells[3].Paragraphs.First().Append(practiceScheduleCurrent.StartEndDateString, textParagraphFormat);
                        t.Rows[mergeStart].Cells[4].Paragraphs.First().Append(practiceScheduleCurrent.PracticeSubjects.PracticeSubjectName, textParagraphFormat);
                        mergeStart = i++;
                        practiceScheduleCurrent = student.PracticeDistributions.PracticeSchedules;
                    }
                    i++;
                }
                doc.InsertTable(t);

                doc.Save();
                Console.Read();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
