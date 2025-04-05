using System;
using System.IO;
using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Snippets.Font;

namespace PDFCombiner
{
    internal class Program
    {
        private static string targetPath = @"D:\1To_Print\";
        private static string sourcePath = @"D:\1To_Print\PDFs\";

        private static string resultFileName = "!ToPrint.pdf";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                string sourceDirectory = AppContext.BaseDirectory;
                Console.WriteLine($"Source Directory: {sourceDirectory}");
                Console.WriteLine("Continue? [yes/no]");
                var input = Console.ReadLine();
                if (input == "y" || input == "yes")
                {
                    string titlePath = Path.Combine(sourceDirectory, "Title.pdf");
                    Console.WriteLine($"Title path: {titlePath}\n");

                    string examplePath = Path.Combine(sourceDirectory, "ScretchExample.pdf");
                    Console.WriteLine($"Scretch example path: {examplePath}\n");

                    Console.WriteLine($"Source files path: {sourcePath} \n");

                    var sourceFiles = GetPdfFiles(sourcePath);
                    Console.WriteLine($"{sourceFiles.Count} files were found!\n");

                    if(sourceFiles.Count == 0)
                    {
                        Console.WriteLine($"Обеспечьте наличие файлов в папке {sourceFiles}");
                        Console.ReadKey();
                    }



                    var titlePosition = GetTitlePositionVariant<TitlePosition>("Выберите способ расположения передней страницы (title page) сертификата:");
                    var scratchSamlePosition = GetTitlePositionVariant<ScratchSamlePosition>("Выберите способ расположения страницы с примером расположения наклейки со скретч покрытием(Scretch example page) сертификата:");

                    sourceFiles = AddTitles(sourceFiles, titlePosition, titlePath);
                    sourceFiles = AddSamplePage(sourceFiles, scratchSamlePosition, examplePath);

                    Console.WriteLine("Merging process just has been started...");
                    
                    var targetFilePath = $"{targetPath}!Result.pdf";
                    Console.WriteLine($"Target path: {targetFilePath} \n");
                    MergePDFs(targetFilePath, sourceFiles);

                    Console.WriteLine("Merging saccessfully compited!");
                    Console.WriteLine($"All .pdf files are merged to {resultFileName} and placed into directory {targetPath}.");
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadKey();
            }
            Console.ReadKey();
        }

        static List<string> AddTitles(List<string> sourceFiles, TitlePosition titlePosition, string titlePath)
        {
            switch (titlePosition)
            {
                case TitlePosition.NoTitle:
                    // Ничего не делать
                    break;
                case TitlePosition.AtTheBegining:
                    // Вставить строку titlePath на 0 позицию в списке
                    sourceFiles.Insert(0, titlePath);
                    break;
                case TitlePosition.AtTheEnd:
                    // Вставить строку titlePath в конец списка
                    sourceFiles.Add(titlePath);
                    break;
                case TitlePosition.OneByOneTitleRegular:
                    // Вставить titlePath на 0 позицию и дальше после каждого елемента в списке
                    for (int i = 0; i < sourceFiles.Count; i += 2)
                    {
                        sourceFiles.Insert(i, titlePath);
                    }
                    break;
                case TitlePosition.OneByOneRegularTitle:
                    // Вставить titlePath после каждого елемента в списке
                    for (int i = 1; i < sourceFiles.Count; i += 2)
                    {
                        sourceFiles.Insert(i, titlePath);
                    }
                    sourceFiles.Add(titlePath);
                    break;
            }

            return sourceFiles;
        }

        static List<string> AddSamplePage(List<string> sourceFiles, ScratchSamlePosition samplePosition, string scratchSamlePosition)
        {
            switch (samplePosition)
            {
                case ScratchSamlePosition.NotIncluded:
                    // Ничего не делать
                    break;
                case ScratchSamlePosition.InsertAsFirstPage:
                    // Вставить строку scratchSamlePosition на 0 позицию в списке
                    sourceFiles.Insert(0, scratchSamlePosition);
                    break;
                case ScratchSamlePosition.InsertAsSecondPage:
                    // Вставить строку scratchSamlePosition вторым элементом
                    sourceFiles.Insert(1, scratchSamlePosition);
                    break;
                case ScratchSamlePosition.InsertAsLastPage:
                    // Вставить scratchSamlePosition последним элементом
                    sourceFiles.Add(scratchSamlePosition);
                    break;
                case ScratchSamlePosition.InsertAsFirstBeforeLastPage:
                    // Вставить scratchSamlePosition предпоследним элементом
                    sourceFiles.Insert(sourceFiles.Count - 1, scratchSamlePosition);
                    break;
            }

            return sourceFiles;
        }

        static T GetTitlePositionVariant<T>(string prompt) where T : struct, Enum
        {
            T result = default(T);
            bool isValidInput = false;

            Console.WriteLine(prompt);
            while (!isValidInput)
            {
                foreach (T enumValue in Enum.GetValues(typeof(T)))
                {
                    Console.WriteLine($"{Convert.ToInt32(enumValue)}: {enumValue}");
                }

                Console.Write("Введите номер: ");
                string input = Console.ReadLine();

                if (Enum.TryParse(input, out result))
                {
                    if (Enum.IsDefined(typeof(T), result))
                    {
                        isValidInput = true;
                    }
                    else
                    {
                        Console.WriteLine("Неверный ввод. Пожалуйста, выберите один из вариантов.");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод. Пожалуйста, введите номер из списка.");
                }
            }

            return result;
        }


        public static void MergePDFs(string targetPath, List<string> pdfs)
        {
            using (var targetDoc = new PdfDocument())
            {
                foreach (var pdf in pdfs)
                {
                    using (var pdfDoc = PdfReader.Open(pdf, PdfDocumentOpenMode.Import))
                    {
                        for (var i = 0; i < pdfDoc.PageCount; i++)
                            targetDoc.AddPage(pdfDoc.Pages[i]);
                    }
                }
                targetDoc.Save(targetPath);
            }
        }

        static List<string> GetPdfFiles(string folderPath)
        {
            List<string> pdfFiles = new List<string>();

            try
            {
                // Проверяем, существует ли указанная папка
                if (Directory.Exists(folderPath))
                {
                    // Получаем все файлы в указанной папке с расширением .pdf
                    string[] files = Directory.GetFiles(folderPath, "*.pdf");

                    // Добавляем пути к .pdf файлам в список
                    pdfFiles.AddRange(files);
                }
                else
                {
                    Console.WriteLine("Указанная папка не существует.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            return pdfFiles;
        }
    }

    public enum TitlePosition
    {
        NoTitle = 0,
        AtTheBegining = 1,
        AtTheEnd = 2,
        OneByOneTitleRegular = 3,
        OneByOneRegularTitle = 4,
    }

    public enum ScratchSamlePosition
    {
        NotIncluded = 0,
        InsertAsFirstPage = 1,
        InsertAsSecondPage = 2,
        InsertAsLastPage = 3,
        InsertAsFirstBeforeLastPage = 4,
    }
}