using CssApp.Utils;

namespace CssApp.Models;

/// <summary>
/// ワークシート（セルのグリッド）を管理するクラス
/// </summary>
public class Worksheet
{
    private readonly Dictionary<CellAddress, Cell> _cells;
    private readonly Dictionary<int, int> _columnWidths;
    private const int DefaultColumnWidth = 10;

    public Worksheet()
    {
        _cells = new Dictionary<CellAddress, Cell>();
        _columnWidths = new Dictionary<int, int>();
    }

    /// <summary>
    /// 指定されたセルを取得（存在しない場合は新規作成）
    /// </summary>
    public Cell GetCell(int row, int column)
    {
        var address = new CellAddress(row, column);
        return GetCell(address);
    }

    /// <summary>
    /// 指定されたセルを取得（存在しない場合は新規作成）
    /// </summary>
    public Cell GetCell(CellAddress address)
    {
        if (!_cells.TryGetValue(address, out var cell))
        {
            cell = new Cell();
            _cells[address] = cell;
        }
        return cell;
    }

    /// <summary>
    /// セルに値を設定
    /// </summary>
    public void SetCell(int row, int column, string value)
    {
        var cell = GetCell(row, column);
        cell.SetValue(value);
    }

    /// <summary>
    /// セルに値を設定
    /// </summary>
    public void SetCell(CellAddress address, string value)
    {
        var cell = GetCell(address);
        cell.SetValue(value);
    }

    /// <summary>
    /// セルが存在するかチェック（空でないセルが存在するか）
    /// </summary>
    public bool HasCell(CellAddress address)
    {
        return _cells.TryGetValue(address, out var cell) && !cell.IsEmpty;
    }

    /// <summary>
    /// 列幅を取得
    /// </summary>
    public int GetColumnWidth(int column)
    {
        return _columnWidths.TryGetValue(column, out var width) ? width : DefaultColumnWidth;
    }

    /// <summary>
    /// 列幅を設定
    /// </summary>
    public void SetColumnWidth(int column, int width)
    {
        if (width < 3)
            width = 3; // 最小幅
        if (width > 50)
            width = 50; // 最大幅

        _columnWidths[column] = width;
    }

    /// <summary>
    /// 全てのセルをクリア
    /// </summary>
    public void Clear()
    {
        _cells.Clear();
        _columnWidths.Clear();
    }

    /// <summary>
    /// 空でないセルの数を取得
    /// </summary>
    public int GetNonEmptyCellCount()
    {
        return _cells.Count(kvp => !kvp.Value.IsEmpty);
    }

    /// <summary>
    /// 使用されている最大行を取得
    /// </summary>
    public int GetMaxUsedRow()
    {
        if (_cells.Count == 0)
            return 1;

        return _cells.Where(kvp => !kvp.Value.IsEmpty)
                     .Select(kvp => kvp.Key.Row)
                     .DefaultIfEmpty(1)
                     .Max();
    }

    /// <summary>
    /// 使用されている最大列を取得
    /// </summary>
    public int GetMaxUsedColumn()
    {
        if (_cells.Count == 0)
            return 0;

        return _cells.Where(kvp => !kvp.Value.IsEmpty)
                     .Select(kvp => kvp.Key.Column)
                     .DefaultIfEmpty(0)
                     .Max();
    }

    /// <summary>
    /// 全ての非空セルを取得
    /// </summary>
    public IEnumerable<(CellAddress Address, Cell Cell)> GetAllNonEmptyCells()
    {
        return _cells.Where(kvp => !kvp.Value.IsEmpty)
                     .Select(kvp => (kvp.Key, kvp.Value));
    }

    /// <summary>
    /// 指定範囲のセルを取得
    /// </summary>
    public IEnumerable<Cell> GetCellsInRange(CellRange range)
    {
        foreach (var address in range.GetCells())
        {
            yield return GetCell(address);
        }
    }

    /// <summary>
    /// セルを削除（空にする）
    /// </summary>
    public void DeleteCell(CellAddress address)
    {
        if (_cells.TryGetValue(address, out var cell))
        {
            cell.Clear();
        }
    }
}

// Made with Bob
