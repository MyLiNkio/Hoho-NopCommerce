
using System.Globalization;
using CsvHelper;
using System.Drawing.Imaging;
using CertificateCodesGenerator;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VoucherCodesGenerator
{
    internal class Program
    {
        private static string saveFolderPath = "D:\\1To_Print\\";
        private static string saveQRpath = saveFolderPath + "QRCodes\\";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("This program generates QRCodes for each certificate and adds path to the QR codes into the !Result data file");
            Console.WriteLine();

            string sourceDirectory = AppContext.BaseDirectory;

            string fileName;
            var filePath = GetSourceFilePath(sourceDirectory, out fileName);

            try
            {
                if (!Directory.Exists(saveQRpath))
                {
                    Directory.CreateDirectory(saveQRpath);
                }

                var csvService = new CsvService();
                var vouchers = csvService.ReadCsvFile<PrintableVoucherData>(filePath);
                Console.WriteLine("CSV file is opened...");

                SaveImages(vouchers, ImageFormat.Png);
                Console.WriteLine($"All QR Codes were written to {saveQRpath}");
                Console.WriteLine();
                var resultFilePath = saveFolderPath + $"!Result_{fileName}.csv";
                csvService.WriteToCsvFile(vouchers, resultFilePath);
                Console.WriteLine("!Result .csv was written...");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"Done! All data was written to folder: {saveFolderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
            Console.ReadKey();
        }

        private static string GetSourceFilePath(string currentDirectory, out string fileName)
        {
            // Шаг 1: Получить все файлы типа .csv из текущей папки
            string[] csvFiles = Directory.GetFiles(currentDirectory, "*.csv");

            // Создаем словарь для хранения порядкового номера файла и его имени
            Dictionary<int, string> filesDictionary = new Dictionary<int, string>();

            // Заполняем словарь
            for (int i = 0; i < csvFiles.Length; i++)
            {
                filesDictionary.Add(i + 1, Path.GetFileName(csvFiles[i]));
            }

            // Шаг 2: Предложить пользователю выбрать номер файла для открытия
            Console.WriteLine("Выберите номер файла для открытия:");

            foreach (var entry in filesDictionary)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }

            // Шаг 3: Получаем выбранный пользователем порядковый номер файла
            int selectedFileNumber;
            while (true)
            {
                Console.WriteLine();
                Console.Write("Введите номер файла: ");
                if (int.TryParse(Console.ReadLine(), out selectedFileNumber))
                {
                    if (filesDictionary.TryGetValue(selectedFileNumber, out var name))
                    {
                        fileName = name;
                        break;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Файл с указанным номером не найден. Попробуйте снова.");
                    }
                }
                else
                {
                    Console.WriteLine("Некорректный ввод. Введите целое число.");
                }
            }

            // Получаем путь к выбранному файлу
            var result = csvFiles[selectedFileNumber - 1];
            return result;
        }

        public static void SaveImages(IEnumerable<PrintableVoucherData> vouchers, ImageFormat format)
        {
            foreach (var voucher in vouchers)
            {
                var data = QRCodesManager.QRCodeCreator.GenerateUrlQRCode(voucher.QRData);

                var path = $"{saveQRpath}{voucher.CardNumber}.{format.ToString().ToLower()}";

                voucher.QRCode = path;
                File.WriteAllBytes(path, data);
            }
        }
    }


}