

using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.IO;


namespace Nop.Plugin.Misc.AdvRedirect.Extensions
{
    public static class CsvExtension
    {
        //CsvConfiguration c = new CsvConfiguration(CultureInfo.InvariantCulture);
        //c.Delimiter = ";";
        //c.HasHeaderRecord = header;

        public static string ToCsv<T>(this IEnumerable<T> data, CsvConfiguration conf)
        {
            Stream st = new MemoryStream();
            using (var writer = new StreamWriter(st, System.Text.Encoding.UTF8, leaveOpen: true))
            using (var csv = new CsvWriter(writer, conf))
            {
                csv.WriteRecords(data);
            }

            st.Position = 0;
            using (StreamReader reader = new StreamReader(st, System.Text.Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }

    
}
