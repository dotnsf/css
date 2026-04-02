using System.Text;
using CssApp.Models;
using CssApp.Utils;

namespace CssApp.UI;

/// <summary>
/// コンソールにスプレッドシートを描画するクラス
/// </summary>
public class ConsoleRenderer
{
    private readonly Worksheet _worksheet;
    private readonly Viewport _viewport;
    
    // 編集モード用の状態
    private bool _isEditMode = false;
    private string _editModeValue = "";
    private int _editModeCursorPos = 0;

    public ConsoleRenderer(Worksheet worksheet, Viewport viewport)
    {
        _worksheet = worksheet;
        _viewport = viewport;
    }

    /// <summary>
    /// 画面全体を描画
    /// </summary>
    public void Render(string statusMessage = "")
    {
        Console.CursorVisible = false;
        
        // カーソルを画面の先頭に移動（スクロールを防ぐ）
        Console.SetCursorPosition(0, 0);

        RenderHeader();
        RenderGrid();
        RenderStatusBar(statusMessage);

        Console.CursorVisible = true;
    }

    /// <summary>
    /// ヘッダー行（列名）を描画
    /// </summary>
    private void RenderHeader()
    {
        var (startCol, endCol) = _viewport.GetVisibleColumnRange();
        var sb = new StringBuilder();

        // 行番号列のスペース
        sb.Append("     │");

        // 列名を描画
        for (int col = startCol; col <= endCol; col++)
        {
            string colName = ColumnHelper.IndexToColumn(col);
            int width = _worksheet.GetColumnWidth(col);
            sb.Append(CenterText(colName, width));
            sb.Append('│');
            
            // 画面幅を超えそうな場合は中断
            if (sb.Length >= Console.WindowWidth - 2)
                break;
        }

        // 行が画面幅を超えないように切り詰める
        string headerLine = sb.ToString();
        if (headerLine.Length > Console.WindowWidth)
        {
            headerLine = headerLine.Substring(0, Console.WindowWidth);
        }
        
        // 行末までクリア
        Console.Write(headerLine.PadRight(Console.WindowWidth));
        Console.SetCursorPosition(0, 1);
        
        // 区切り線
        sb.Clear();
        sb.Append("─────┼");
        for (int col = startCol; col <= endCol; col++)
        {
            int width = _worksheet.GetColumnWidth(col);
            sb.Append(new string('─', width));
            sb.Append('┼');
            
            // 画面幅を超えそうな場合は中断
            if (sb.Length >= Console.WindowWidth - 2)
                break;
        }
        
        string separatorLine = sb.ToString();
        if (separatorLine.Length > Console.WindowWidth)
        {
            separatorLine = separatorLine.Substring(0, Console.WindowWidth);
        }
        
        Console.Write(separatorLine.PadRight(Console.WindowWidth));
        Console.SetCursorPosition(0, 2);
    }

    /// <summary>
    /// グリッド（データ行）を描画
    /// </summary>
    private void RenderGrid()
    {
        var (startRow, endRow) = _viewport.GetVisibleRowRange();
        var (startCol, endCol) = _viewport.GetVisibleColumnRange();

        int currentScreenRow = 2; // ヘッダーの後から開始

        for (int row = startRow; row <= endRow; row++)
        {
            if (currentScreenRow >= Console.WindowHeight - 1)
                break; // ステータスバーの領域を確保

            Console.SetCursorPosition(0, currentScreenRow);
            var sb = new StringBuilder();

            // 行番号
            sb.Append(row.ToString().PadLeft(4));
            sb.Append(" │");

            int currentLineLength = sb.Length;

            // セルを描画
            for (int col = startCol; col <= endCol; col++)
            {
                var cell = _worksheet.GetCell(row, col);
                int width = _worksheet.GetColumnWidth(col);
                
                // 編集モードの場合は編集中の値を表示、それ以外は通常の値
                string displayValue = (_isEditMode && row == _viewport.CurrentRow && col == _viewport.CurrentColumn) 
                    ? _editModeValue 
                    : cell.DisplayValue;

                // 現在のセルをハイライト
                bool isCurrentCell = (row == _viewport.CurrentRow && col == _viewport.CurrentColumn);

                // 画面幅チェック
                if (currentLineLength + width + 1 >= Console.WindowWidth)
                {
                    break; // この列は表示しない
                }

                if (isCurrentCell)
                {
                    // 通常のテキストを出力
                    string normalText = sb.ToString();
                    if (normalText.Length > 0)
                    {
                        Console.Write(normalText);
                        currentLineLength = Console.CursorLeft;
                    }
                    sb.Clear();

                    // 背景色を変更（編集モードでも白文字で表示）
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(TruncateOrPad(displayValue, width));
                    Console.ResetColor();
                    Console.Write('│');
                    currentLineLength = Console.CursorLeft;
                }
                else
                {
                    sb.Append(TruncateOrPad(displayValue, width));
                    sb.Append('│');
                    currentLineLength += width + 1;
                }
            }

            // 残りのテキストを出力
            if (sb.Length > 0)
            {
                string remainingText = sb.ToString();
                if (Console.CursorLeft + remainingText.Length > Console.WindowWidth)
                {
                    remainingText = remainingText.Substring(0, Console.WindowWidth - Console.CursorLeft);
                }
                Console.Write(remainingText);
            }
            
            // 行末までクリア
            int currentPos = Console.CursorLeft;
            if (currentPos < Console.WindowWidth)
            {
                Console.Write(new string(' ', Console.WindowWidth - currentPos));
            }
            
            currentScreenRow++;
        }
        
        // 残りの行をクリア
        while (currentScreenRow < Console.WindowHeight - 1)
        {
            Console.SetCursorPosition(0, currentScreenRow);
            Console.Write(new string(' ', Console.WindowWidth));
            currentScreenRow++;
        }
    }

    /// <summary>
    /// ステータスバーを描画
    /// </summary>
    private void RenderStatusBar(string message)
    {
        // 画面下部に移動
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        
        Console.BackgroundColor = ConsoleColor.Gray;
        Console.ForegroundColor = ConsoleColor.Black;

        var address = _viewport.GetCurrentAddress();
        string cellInfo = $" {address} ";
        
        if (!string.IsNullOrEmpty(message))
        {
            cellInfo += $"│ {message} ";
        }

        // ステータスバーを画面幅いっぱいに表示
        if (cellInfo.Length > Console.WindowWidth)
        {
            cellInfo = cellInfo.Substring(0, Console.WindowWidth);
        }
        
        string statusBar = cellInfo.PadRight(Console.WindowWidth);
        Console.Write(statusBar);

        Console.ResetColor();
    }

    /// <summary>
    /// 編集モードで画面を描画
    /// </summary>
    public void RenderEditMode(string editValue, int cursorPosition)
    {
        // 編集モードの状態を保存
        _isEditMode = true;
        _editModeValue = editValue;
        _editModeCursorPos = cursorPosition;
        
        // 画面を再描画
        Render($"EDIT: {editValue}");
        
        // カーソルを編集位置に移動
        var (startRow, endRow) = _viewport.GetVisibleRowRange();
        var (startCol, endCol) = _viewport.GetVisibleColumnRange();

        if (_viewport.CurrentRow >= startRow && _viewport.CurrentRow <= endRow &&
            _viewport.CurrentColumn >= startCol && _viewport.CurrentColumn <= endCol)
        {
            // セルの位置を計算
            int screenRow = _viewport.CurrentRow - startRow + 2; // +2 for header
            int screenCol = 6; // 行番号列の幅

            for (int col = startCol; col < _viewport.CurrentColumn; col++)
            {
                screenCol += _worksheet.GetColumnWidth(col) + 1;
            }

            // カーソル位置を計算（編集中のテキスト内）
            int width = _worksheet.GetColumnWidth(_viewport.CurrentColumn);
            int actualCursorPos = Math.Min(cursorPosition, width - 1);
            
            // 画面内に収まる場合のみカーソルを設定
            if (screenCol + actualCursorPos < Console.WindowWidth && screenRow < Console.WindowHeight - 1)
            {
                Console.SetCursorPosition(screenCol + actualCursorPos, screenRow);
                Console.CursorVisible = true;
            }
        }
        
        // 編集モードフラグをリセット
        _isEditMode = false;
    }

    /// <summary>
    /// コマンドモードで画面を描画
    /// </summary>
    public void RenderCommandMode(string commandText)
    {
        Render();
        
        // 画面下部にコマンド入力を表示
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.BackgroundColor = ConsoleColor.DarkYellow;
        Console.ForegroundColor = ConsoleColor.Black;
        
        string prompt = $":{commandText}";
        if (prompt.Length > Console.WindowWidth)
        {
            prompt = prompt.Substring(0, Console.WindowWidth);
        }
        
        Console.Write(prompt.PadRight(Console.WindowWidth));
        
        if (prompt.Length < Console.WindowWidth)
        {
            Console.SetCursorPosition(prompt.Length, Console.WindowHeight - 1);
        }
        
        Console.ResetColor();
        Console.CursorVisible = true;
    }

    /// <summary>
    /// テキストを中央揃え
    /// </summary>
    private string CenterText(string text, int width)
    {
        if (text.Length >= width)
            return text.Substring(0, width);

        int leftPad = (width - text.Length) / 2;
        int rightPad = width - text.Length - leftPad;
        return new string(' ', leftPad) + text + new string(' ', rightPad);
    }

    /// <summary>
    /// テキストを切り詰めるか、パディング
    /// </summary>
    private string TruncateOrPad(string text, int width)
    {
        if (text.Length > width)
        {
            return text.Substring(0, width);
        }
        return text.PadRight(width);
    }

    /// <summary>
    /// エラーメッセージを表示
    /// </summary>
    public void ShowError(string message)
    {
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
        
        string errorMsg = $" ERROR: {message} ";
        if (errorMsg.Length > Console.WindowWidth)
        {
            errorMsg = errorMsg.Substring(0, Console.WindowWidth);
        }
        
        Console.Write(errorMsg.PadRight(Console.WindowWidth));
        Console.ResetColor();
        Thread.Sleep(2000);
    }

    /// <summary>
    /// 情報メッセージを表示
    /// </summary>
    public void ShowInfo(string message)
    {
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.BackgroundColor = ConsoleColor.Green;
        Console.ForegroundColor = ConsoleColor.White;
        
        string infoMsg = $" {message} ";
        if (infoMsg.Length > Console.WindowWidth)
        {
            infoMsg = infoMsg.Substring(0, Console.WindowWidth);
        }
        
        Console.Write(infoMsg.PadRight(Console.WindowWidth));
        Console.ResetColor();
        Thread.Sleep(1500);
    }
}

// Made with Bob
