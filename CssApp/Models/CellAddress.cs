using CssApp.Utils;

namespace CssApp.Models;

/// <summary>
/// セルのアドレス（例：A1, B2, ZZ1024）を表すクラス
/// </summary>
public class CellAddress : IEquatable<CellAddress>
{
    public int Row { get; }      // 1-based (1-1024)
    public int Column { get; }   // 0-based (0-701)

    public CellAddress(int row, int column)
    {
        if (!ColumnHelper.IsValidRowNumber(row))
            throw new ArgumentException($"Row number out of range: {row}", nameof(row));
        
        if (column < 0 || column >= ColumnHelper.MaxColumns)
            throw new ArgumentException($"Column index out of range: {column}", nameof(column));

        Row = row;
        Column = column;
    }

    /// <summary>
    /// 文字列（例："A1", "ZZ1024"）からCellAddressを作成
    /// </summary>
    public static CellAddress Parse(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Cell address cannot be empty", nameof(address));

        address = address.Trim().ToUpper();

        // 列名と行番号を分離
        int i = 0;
        while (i < address.Length && char.IsLetter(address[i]))
            i++;

        if (i == 0 || i == address.Length)
            throw new ArgumentException($"Invalid cell address format: {address}", nameof(address));

        string columnName = address.Substring(0, i);
        string rowString = address.Substring(i);

        if (!int.TryParse(rowString, out int row))
            throw new ArgumentException($"Invalid row number in address: {address}", nameof(address));

        int column = ColumnHelper.ColumnToIndex(columnName);

        return new CellAddress(row, column);
    }

    /// <summary>
    /// 文字列からのパースを試行
    /// </summary>
    public static bool TryParse(string address, out CellAddress? result)
    {
        try
        {
            result = Parse(address);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// セルアドレスを文字列形式（例："A1"）に変換
    /// </summary>
    public override string ToString()
    {
        return $"{ColumnHelper.IndexToColumn(Column)}{Row}";
    }

    public bool Equals(CellAddress? other)
    {
        if (other is null) return false;
        return Row == other.Row && Column == other.Column;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CellAddress);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column);
    }

    public static bool operator ==(CellAddress? left, CellAddress? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(CellAddress? left, CellAddress? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// セル範囲（例：A1:B10）を表すクラス
/// </summary>
public class CellRange
{
    public CellAddress Start { get; }
    public CellAddress End { get; }

    public CellRange(CellAddress start, CellAddress end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// 文字列（例："A1:B10"）からCellRangeを作成
    /// </summary>
    public static CellRange Parse(string range)
    {
        if (string.IsNullOrWhiteSpace(range))
            throw new ArgumentException("Cell range cannot be empty", nameof(range));

        var parts = range.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid cell range format: {range}", nameof(range));

        var start = CellAddress.Parse(parts[0].Trim());
        var end = CellAddress.Parse(parts[1].Trim());

        return new CellRange(start, end);
    }

    /// <summary>
    /// 範囲内の全セルアドレスを取得
    /// </summary>
    public IEnumerable<CellAddress> GetCells()
    {
        int minRow = Math.Min(Start.Row, End.Row);
        int maxRow = Math.Max(Start.Row, End.Row);
        int minCol = Math.Min(Start.Column, End.Column);
        int maxCol = Math.Max(Start.Column, End.Column);

        for (int row = minRow; row <= maxRow; row++)
        {
            for (int col = minCol; col <= maxCol; col++)
            {
                yield return new CellAddress(row, col);
            }
        }
    }

    public override string ToString()
    {
        return $"{Start}:{End}";
    }
}

// Made with Bob
