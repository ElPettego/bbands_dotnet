public class CsvParser 
{
    public static decimal[] ParseCsvColumn(string filepath, int columns, int column, int limit)
    {
        var csvData = File.ReadAllLines(filepath)
                          .SelectMany(line => line.Split(","))
                          .ToArray();
        var len = csvData.Count();
        var rows = len / columns;
        // Console.WriteLine(len);
        decimal[] res = new decimal[limit]; 
        for (int i = rows - limit; i < rows; i++)
        {
            // Console.WriteLine(csvData[i * columns + column]);
            res[i + limit - rows] = decimal.Parse(csvData[i * columns + column].Replace(".", ","));
        }
        return res;
    }
}