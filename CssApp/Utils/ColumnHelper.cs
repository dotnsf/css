namespace CssApp.Utils;

/// <summary>
/// 列名（A-ZZ）とインデックス（0-701）の変換を行うヘルパークラス
/// </summary>
public static class ColumnHelper
{
    public const int MaxColumns = 702; // A-ZZ
    public const int MaxRows = 1024;

    /// <summary>
    /// 列名（A, B, ..., Z, AA, AB, ..., ZZ）をインデックス（0-701）に変換
    /// </summary>
    public static int ColumnToIndex(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            throw new ArgumentException("Column name cannot be empty", nameof(columnName));

        columnName = columnName.ToUpper();
        int result = 0;

        foreach (char c in columnName)
        {
            if (c < 'A' || c > 'Z')
                throw new ArgumentException($"Invalid column name: {columnName}", nameof(columnName));

            result = result * 26 + (c - 'A' + 1);
        }

        result--; // Convert to 0-based index

        if (result < 0 || result >= MaxColumns)
            throw new ArgumentException($"Column index out of range: {columnName}", nameof(columnName));

        return result;
    }

    /// <summary>
    /// インデックス（0-701）を列名（A, B, ..., Z, AA, AB, ..., ZZ）に変換
    /// </summary>
    public static string IndexToColumn(int index)
    {
        if (index < 0 || index >= MaxColumns)
            throw new ArgumentException($"Column index out of range: {index}", nameof(index));

        index++; // Convert to 1-based for calculation
        string result = "";

        while (index > 0)
        {
            int remainder = (index - 1) % 26;
            result = (char)('A' + remainder) + result;
            index = (index - 1) / 26;
        }

        return result;
    }

    /// <summary>
    /// 列名が有効かどうかを検証
    /// </summary>
    public static bool IsValidColumnName(string columnName)
    {
        try
        {
            ColumnToIndex(columnName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 行番号が有効かどうかを検証（1-1024）
    /// </summary>
    public static bool IsValidRowNumber(int row)
    {
        return row >= 1 && row <= MaxRows;
    }
}

// Made with Bob
