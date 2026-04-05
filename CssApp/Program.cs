using System.Text;
using CssApp.Core;
using CssApp.IO;
using CssApp.Models;
using CssApp.UI;

namespace CssApp;

/// <summary>
/// アプリケーションのモード
/// </summary>
enum AppMode
{
    Navigation,
    Edit,
    Command,
    Menu
}

class Program
{
    private static Worksheet _worksheet = new();
    private static Viewport _viewport = new(_worksheet);
    private static ConsoleRenderer _renderer = new(_worksheet, _viewport);
    private static FormulaEvaluator _evaluator = new(_worksheet);
    
    private static AppMode _mode = AppMode.Navigation;
    private static string _currentFile = "";
    private static bool _isModified = false;
    private static bool _shouldQuit = false;

    // 編集モード用
    private static string _editValue = "";
    private static string _originalValue = "";
    private static int _editCursorPos = 0;

    // コマンドモード用
    private static string _commandText = "";

    // メニューモード用
    private static int _menuSelectedIndex = 0;

    static void Main(string[] args)
    {
        try
        {
            // コンソールの設定
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            // コマンドライン引数からファイルを開く
            if (args.Length > 0)
            {
                string filePath = args[0];
                if (File.Exists(filePath))
                {
                    try
                    {
                        CsvHandler.LoadFromFile(filePath, _worksheet);
                        _currentFile = filePath;
                        _evaluator.EvaluateAll();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading file: {ex.Message}");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
            }

            // メインループ
            _renderer.Render();

            while (!_shouldQuit)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    HandleInput(key);
                }

                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        finally
        {
            Console.Clear();
            Console.CursorVisible = true;
        }
    }

    static void HandleInput(ConsoleKeyInfo key)
    {
        try
        {
            switch (_mode)
            {
                case AppMode.Navigation:
                    HandleNavigationInput(key);
                    break;
                case AppMode.Edit:
                    HandleEditInput(key);
                    break;
                case AppMode.Command:
                    HandleCommandInput(key);
                    break;
                case AppMode.Menu:
                    HandleMenuInput(key);
                    break;
            }
        }
        catch (Exception ex)
        {
            _renderer.ShowError(ex.Message);
            _renderer.Render();
        }
    }

    static void HandleNavigationInput(ConsoleKeyInfo key)
    {
        // Vi-like キーバインド (h, j, k, l)
        if (key.KeyChar == 'h')
        {
            _viewport.MoveLeft();
            _renderer.Render();
            return;
        }
        else if (key.KeyChar == 'j')
        {
            _viewport.MoveDown();
            _renderer.Render();
            return;
        }
        else if (key.KeyChar == 'k')
        {
            _viewport.MoveUp();
            _renderer.Render();
            return;
        }
        else if (key.KeyChar == 'l')
        {
            _viewport.MoveRight();
            _renderer.Render();
            return;
        }
        else if (key.KeyChar == 'i')
        {
            // Vi-like: i キーで編集モードに入る
            EnterEditMode();
            return;
        }

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                _viewport.MoveUp();
                _renderer.Render();
                break;

            case ConsoleKey.DownArrow:
                _viewport.MoveDown();
                _renderer.Render();
                break;

            case ConsoleKey.LeftArrow:
                _viewport.MoveLeft();
                _renderer.Render();
                break;

            case ConsoleKey.RightArrow:
                _viewport.MoveRight();
                _renderer.Render();
                break;

            case ConsoleKey.Home:
                _viewport.MoveHome();
                _renderer.Render();
                break;

            case ConsoleKey.End:
                _viewport.MoveEnd();
                _renderer.Render();
                break;

            case ConsoleKey.PageUp:
                _viewport.PageUp(_viewport.GetVisibleRows());
                _renderer.Render();
                break;

            case ConsoleKey.PageDown:
                _viewport.PageDown(_viewport.GetVisibleRows());
                _renderer.Render();
                break;

            case ConsoleKey.Enter:
                EnterEditMode();
                break;

            case ConsoleKey.Oem1 when key.KeyChar == ':': // コロンキー
            case ConsoleKey.Oem7 when key.KeyChar == ':': // 別のキーボードレイアウト
                EnterCommandMode();
                break;

            case ConsoleKey.Oem2 when key.KeyChar == '/': // スラッシュキー
            case ConsoleKey.Divide when key.KeyChar == '/': // テンキーのスラッシュ
                EnterMenuMode();
                break;

            case ConsoleKey.Delete:
                DeleteCurrentCell();
                break;
        }
    }

    static void HandleEditInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Enter:
                ConfirmEdit();
                break;

            case ConsoleKey.Escape:
                CancelEdit();
                break;

            case ConsoleKey.Backspace:
                if (_editCursorPos > 0)
                {
                    _editValue = _editValue.Remove(_editCursorPos - 1, 1);
                    _editCursorPos--;
                    _renderer.RenderEditMode(_editValue, _editCursorPos);
                }
                break;

            case ConsoleKey.Delete:
                if (_editCursorPos < _editValue.Length)
                {
                    _editValue = _editValue.Remove(_editCursorPos, 1);
                    _renderer.RenderEditMode(_editValue, _editCursorPos);
                }
                break;

            case ConsoleKey.LeftArrow:
                if (_editCursorPos > 0)
                {
                    _editCursorPos--;
                    _renderer.RenderEditMode(_editValue, _editCursorPos);
                }
                break;

            case ConsoleKey.RightArrow:
                if (_editCursorPos < _editValue.Length)
                {
                    _editCursorPos++;
                    _renderer.RenderEditMode(_editValue, _editCursorPos);
                }
                break;

            case ConsoleKey.Home:
                _editCursorPos = 0;
                _renderer.RenderEditMode(_editValue, _editCursorPos);
                break;

            case ConsoleKey.End:
                _editCursorPos = _editValue.Length;
                _renderer.RenderEditMode(_editValue, _editCursorPos);
                break;

            default:
                if (!char.IsControl(key.KeyChar))
                {
                    _editValue = _editValue.Insert(_editCursorPos, key.KeyChar.ToString());
                    _editCursorPos++;
                    _renderer.RenderEditMode(_editValue, _editCursorPos);
                }
                break;
        }
    }

    static void HandleCommandInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Enter:
                ExecuteCommand();
                break;

            case ConsoleKey.Escape:
                CancelCommand();
                break;

            case ConsoleKey.Backspace:
                if (_commandText.Length > 0)
                {
                    _commandText = _commandText.Substring(0, _commandText.Length - 1);
                    _renderer.RenderCommandMode(_commandText);
                }
                break;

            default:
                if (!char.IsControl(key.KeyChar))
                {
                    _commandText += key.KeyChar;
                    _renderer.RenderCommandMode(_commandText);
                }
                break;
        }
    }

    static void EnterEditMode()
    {
        _mode = AppMode.Edit;
        var cell = _worksheet.GetCell(_viewport.CurrentRow, _viewport.CurrentColumn);
        _originalValue = cell.RawValue;
        _editValue = cell.RawValue;
        _editCursorPos = _editValue.Length;
        _renderer.RenderEditMode(_editValue, _editCursorPos);
    }

    static void ConfirmEdit()
    {
        var cell = _worksheet.GetCell(_viewport.CurrentRow, _viewport.CurrentColumn);
        cell.SetValue(_editValue);
        
        if (_editValue != _originalValue)
        {
            _isModified = true;
            _evaluator.EvaluateAll();
        }

        _mode = AppMode.Navigation;
        _renderer.Render();
    }

    static void CancelEdit()
    {
        _mode = AppMode.Navigation;
        _renderer.Render();
    }

    static void DeleteCurrentCell()
    {
        var address = _viewport.GetCurrentAddress();
        _worksheet.DeleteCell(address);
        _isModified = true;
        _evaluator.EvaluateAll();
        _renderer.Render();
    }

    static void EnterCommandMode()
    {
        _mode = AppMode.Command;
        _commandText = "";
        _renderer.RenderCommandMode(_commandText);
    }

    static void ExecuteCommand()
    {
        try
        {
            var command = CommandParser.Parse(_commandText);

            switch (command.Type)
            {
                case CommandType.Save:
                    SaveFile(command.Argument);
                    break;

                case CommandType.Open:
                    OpenFile(command.Argument);
                    break;

                case CommandType.New:
                    NewFile();
                    break;

                case CommandType.Quit:
                    QuitApplication();
                    break;

                case CommandType.ForceQuit:
                    ForceQuitApplication();
                    break;

                case CommandType.SaveAndQuit:
                    SaveFile(command.Argument);
                    _shouldQuit = true;
                    break;

                case CommandType.SetWidth:
                    SetColumnWidth(command.Argument);
                    break;
            }
        }
        catch (Exception ex)
        {
            _renderer.ShowError(ex.Message);
        }

        _mode = AppMode.Navigation;
        _renderer.Render();
    }

    static void HandleMenuInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.K: // Vi-like
                if (_menuSelectedIndex > 0)
                {
                    _menuSelectedIndex--;
                    _renderer.RenderMenuMode(_menuSelectedIndex);
                }
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.J: // Vi-like
                if (_menuSelectedIndex < 3) // 0-3 (4 items)
                {
                    _menuSelectedIndex++;
                    _renderer.RenderMenuMode(_menuSelectedIndex);
                }
                break;

            case ConsoleKey.Enter:
                ExecuteMenuSelection();
                break;

            case ConsoleKey.Escape:
                CancelMenu();
                break;

            // ショートカットキー
            case ConsoleKey.O:
                _menuSelectedIndex = 0;
                ExecuteMenuSelection();
                break;

            case ConsoleKey.W:
                _menuSelectedIndex = 1;
                ExecuteMenuSelection();
                break;

            case ConsoleKey.Q:
                _menuSelectedIndex = 3;
                ExecuteMenuSelection();
                break;
        }
    }

    static void EnterMenuMode()
    {
        _mode = AppMode.Menu;
        _menuSelectedIndex = 0;
        _renderer.RenderMenuMode(_menuSelectedIndex);
    }

    static void ExecuteMenuSelection()
    {
        _mode = AppMode.Command;
        
        // 選択されたメニュー項目に応じてコマンドテキストを設定
        switch (_menuSelectedIndex)
        {
            case 0: // 読み込み(O)
                _commandText = "o ";
                break;
            case 1: // 保存(W)
                _commandText = "w ";
                break;
            case 2: // 列幅変更(Width)
                _commandText = "width ";
                break;
            case 3: // 終了(Q)
                _commandText = "q";
                break;
        }
        
        _renderer.RenderCommandMode(_commandText);
    }

    static void CancelMenu()
    {
        _mode = AppMode.Navigation;
        _renderer.Render();
    }

    static void CancelCommand()
    {
        _mode = AppMode.Navigation;
        _renderer.Render();
    }

    static void SaveFile(string? filename)
    {
        string filePath = filename ?? _currentFile;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("No filename specified");
        }

        CsvHandler.SaveToFile(filePath, _worksheet);
        _currentFile = filePath;
        _isModified = false;
        _renderer.ShowInfo($"Saved to {filePath}");
    }

    static void OpenFile(string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("No filename specified");
        }

        if (_isModified)
        {
            // 簡易的な確認（本来はY/N入力を実装すべき）
            _renderer.ShowError("Unsaved changes will be lost!");
            Thread.Sleep(1000);
        }

        CsvHandler.LoadFromFile(filename, _worksheet);
        _currentFile = filename;
        _isModified = false;
        _evaluator.EvaluateAll();
        _viewport.MoveTo(1, 0);
        _renderer.ShowInfo($"Opened {filename}");
    }

    static void NewFile()
    {
        if (_isModified)
        {
            _renderer.ShowError("Unsaved changes will be lost!");
            Thread.Sleep(1000);
        }

        _worksheet.Clear();
        _currentFile = "";
        _isModified = false;
        _viewport.MoveTo(1, 0);
        _renderer.ShowInfo("New spreadsheet created");
    }

    static void QuitApplication()
    {
        if (_isModified)
        {
            _renderer.ShowError("Unsaved changes! Use :wq to save and quit, or :q! to force quit");
            Thread.Sleep(2000);
            return;
        }

        _shouldQuit = true;
    }

    static void ForceQuitApplication()
    {
        // 強制終了：未保存の変更があっても終了
        _shouldQuit = true;
    }

    static void SetColumnWidth(string? widthStr)
    {
        if (string.IsNullOrWhiteSpace(widthStr) || !int.TryParse(widthStr, out int width))
        {
            throw new ArgumentException("Invalid width value");
        }

        _worksheet.SetColumnWidth(_viewport.CurrentColumn, width);
        _renderer.ShowInfo($"Column width set to {width}");
    }
}

// Made with Bob
