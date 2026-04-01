using System.Text;
using CssApp.Models;

namespace CssApp.IO;

/// <summary>
/// CSV ファイルの読み書きを行うクラス
/// </summary>
public class CsvHandler
{
    /// <summary>
    /// CSV ファイルを読み込む
    /// </summary>
    public static void LoadFromFile(string filePath, Worksheet worksheet)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        worksheet.Clear();

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        
        for (int row = 0; row < lines.Length && row < 1024; row++)
        {
            var values = ParseCsvLine(lines[row]);
            
            for (int col = 0; col < values.Count && col < 702; col++)
            {
                if (!string.IsNullOrEmpty(values[col]))
                {
                    worksheet.SetCell(row + 1, col, values[col]);
                }
            }
        }
    }

    /// <summary>
    /// CSV ファイルに保存
    /// </summary>
    public static void SaveToFile(string filePath, Worksheet worksheet)
    {
        var lines = new List<string>();
        int maxRow = worksheet.GetMaxUsedRow();
        int maxCol = worksheet.GetMaxUsedColumn();

        for (int row = 1; row <= maxRow; row++)
        {
            var values = new List<string>();
            
            for (int col = 0; col <= maxCol; col++)
            {
                var cell = worksheet.GetCell(row, col);
                string value = cell.IsEmpty ? "" : cell.RawValue;
                values.Add(EscapeCsvValue(value));
            }

            // 末尾の空セルを削除
            while (values.Count > 0 && string.IsNullOrEmpty(values[^1]))
            {
                values.RemoveAt(values.Count - 1);
            }

            lines.Add(string.Join(",", values));
        }

        // 末尾の空行を削除
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        File.WriteAllLines(filePath, lines, Encoding.UTF8);
    }

    /// <summary>
    /// CSV 行をパース
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // エスケープされたダブルクォート
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    // クォートの開始/終了
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // フィールドの区切り
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString());
        return values;
    }

    /// <summary>
    /// CSV 値をエスケープ
    /// </summary>
    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // カンマ、ダブルクォート、改行を含む場合はクォートで囲む
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            // ダブルクォートをエスケープ
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        return value;
    }
}

// Made with Bob
