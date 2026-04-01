using CssApp.Models;
using CssApp.Utils;

namespace CssApp.UI;

/// <summary>
/// 表示領域を管理するクラス
/// </summary>
public class Viewport
{
    public int CurrentRow { get; set; } = 1;
    public int CurrentColumn { get; set; } = 0;
    
    public int ScrollRow { get; private set; } = 1;
    public int ScrollColumn { get; private set; } = 0;

    private readonly Worksheet _worksheet;

    public Viewport(Worksheet worksheet)
    {
        _worksheet = worksheet;
    }

    /// <summary>
    /// カーソルを上に移動
    /// </summary>
    public void MoveUp()
    {
        if (CurrentRow > 1)
        {
            CurrentRow--;
            EnsureVisible();
        }
    }

    /// <summary>
    /// カーソルを下に移動
    /// </summary>
    public void MoveDown()
    {
        if (CurrentRow < ColumnHelper.MaxRows)
        {
            CurrentRow++;
            EnsureVisible();
        }
    }

    /// <summary>
    /// カーソルを左に移動
    /// </summary>
    public void MoveLeft()
    {
        if (CurrentColumn > 0)
        {
            CurrentColumn--;
            EnsureVisible();
        }
    }

    /// <summary>
    /// カーソルを右に移動
    /// </summary>
    public void MoveRight()
    {
        if (CurrentColumn < ColumnHelper.MaxColumns - 1)
        {
            CurrentColumn++;
            EnsureVisible();
        }
    }

    /// <summary>
    /// 行の先頭に移動
    /// </summary>
    public void MoveHome()
    {
        CurrentColumn = 0;
        EnsureVisible();
    }

    /// <summary>
    /// 行の末尾に移動
    /// </summary>
    public void MoveEnd()
    {
        int maxCol = Math.Min(_worksheet.GetMaxUsedColumn() + 1, ColumnHelper.MaxColumns - 1);
        CurrentColumn = maxCol;
        EnsureVisible();
    }

    /// <summary>
    /// ページアップ
    /// </summary>
    public void PageUp(int visibleRows)
    {
        CurrentRow = Math.Max(1, CurrentRow - visibleRows);
        EnsureVisible();
    }

    /// <summary>
    /// ページダウン
    /// </summary>
    public void PageDown(int visibleRows)
    {
        CurrentRow = Math.Min(ColumnHelper.MaxRows, CurrentRow + visibleRows);
        EnsureVisible();
    }

    /// <summary>
    /// 現在のセルが表示領域内にあることを保証
    /// </summary>
    private void EnsureVisible()
    {
        // 垂直スクロール
        if (CurrentRow < ScrollRow)
        {
            ScrollRow = CurrentRow;
        }
        else if (CurrentRow > ScrollRow + GetVisibleRows() - 1)
        {
            ScrollRow = CurrentRow - GetVisibleRows() + 1;
        }

        // 水平スクロール
        if (CurrentColumn < ScrollColumn)
        {
            ScrollColumn = CurrentColumn;
        }
        else
        {
            // 現在の列が表示範囲外の場合、スクロール
            int visibleWidth = 6; // 行番号列の幅
            int col = ScrollColumn;
            bool currentVisible = false;
            
            while (col <= CurrentColumn && visibleWidth < Console.WindowWidth - 1)
            {
                int colWidth = _worksheet.GetColumnWidth(col);
                visibleWidth += colWidth + 1; // +1 for separator
                
                if (col == CurrentColumn && visibleWidth <= Console.WindowWidth - 1)
                {
                    currentVisible = true;
                    break;
                }
                col++;
            }

            if (!currentVisible)
            {
                ScrollColumn = CurrentColumn;
            }
        }
    }

    /// <summary>
    /// 表示可能な行数を取得
    /// </summary>
    public int GetVisibleRows()
    {
        // コンソールの高さ - ヘッダー行(2行) - ステータスバー(1行)
        return Math.Max(5, Console.WindowHeight - 3);
    }

    /// <summary>
    /// 表示可能な幅を取得
    /// </summary>
    public int GetVisibleWidth()
    {
        // コンソールの幅 - 行番号列
        return Math.Max(20, Console.WindowWidth - 6);
    }

    /// <summary>
    /// 表示する列の範囲を取得
    /// </summary>
    public (int StartColumn, int EndColumn) GetVisibleColumnRange()
    {
        int startCol = ScrollColumn;
        int endCol = startCol;
        int width = 6; // 行番号列の幅（"     │"）
        int maxWidth = Console.WindowWidth - 1; // 画面幅 - 1（余裕を持たせる）

        while (endCol < ColumnHelper.MaxColumns)
        {
            int colWidth = _worksheet.GetColumnWidth(endCol);
            int nextWidth = width + colWidth + 1; // +1 for separator '│'
            
            if (nextWidth >= maxWidth)
                break;
            
            width = nextWidth;
            endCol++;
        }

        // 少なくとも1列は表示
        if (endCol == startCol)
            endCol = startCol;
        else
            endCol--; // 最後にインクリメントされた分を戻す

        return (startCol, Math.Min(endCol, ColumnHelper.MaxColumns - 1));
    }

    /// <summary>
    /// 表示する行の範囲を取得
    /// </summary>
    public (int StartRow, int EndRow) GetVisibleRowRange()
    {
        int startRow = ScrollRow;
        int endRow = Math.Min(startRow + GetVisibleRows() - 1, ColumnHelper.MaxRows);
        return (startRow, endRow);
    }

    /// <summary>
    /// 現在のセルアドレスを取得
    /// </summary>
    public CellAddress GetCurrentAddress()
    {
        return new CellAddress(CurrentRow, CurrentColumn);
    }

    /// <summary>
    /// 指定したセルに移動
    /// </summary>
    public void MoveTo(int row, int column)
    {
        if (ColumnHelper.IsValidRowNumber(row) && column >= 0 && column < ColumnHelper.MaxColumns)
        {
            CurrentRow = row;
            CurrentColumn = column;
            EnsureVisible();
        }
    }
}

// Made with Bob
