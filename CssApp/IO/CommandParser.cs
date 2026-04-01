namespace CssApp.IO;

/// <summary>
/// コマンドの種類
/// </summary>
public enum CommandType
{
    None,
    Save,           // :w [filename]
    Open,           // :o filename
    New,            // :n
    Quit,           // :q
    ForceQuit,      // :q!
    SaveAndQuit,    // :wq [filename]
    SetWidth        // :width N
}

/// <summary>
/// パースされたコマンド
/// </summary>
public class Command
{
    public CommandType Type { get; set; }
    public string? Argument { get; set; }

    public Command(CommandType type, string? argument = null)
    {
        Type = type;
        Argument = argument;
    }
}

/// <summary>
/// コマンド文字列をパースするクラス
/// </summary>
public class CommandParser
{
    /// <summary>
    /// コマンド文字列をパース
    /// </summary>
    public static Command Parse(string commandString)
    {
        if (string.IsNullOrWhiteSpace(commandString))
            return new Command(CommandType.None);

        commandString = commandString.Trim();

        // コロンで始まる場合は削除
        if (commandString.StartsWith(':'))
            commandString = commandString.Substring(1).Trim();

        if (string.IsNullOrWhiteSpace(commandString))
            return new Command(CommandType.None);

        // コマンドと引数を分離
        var parts = commandString.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0].ToLower();
        string? arg = parts.Length > 1 ? parts[1].Trim() : null;

        return cmd switch
        {
            "w" or "write" or "save" => new Command(CommandType.Save, arg),
            "o" or "open" => new Command(CommandType.Open, arg),
            "n" or "new" => new Command(CommandType.New),
            "q!" or "quit!" => new Command(CommandType.ForceQuit),
            "q" or "quit" or "exit" => new Command(CommandType.Quit),
            "wq" or "x" => new Command(CommandType.SaveAndQuit, arg),
            "width" when int.TryParse(arg, out _) => new Command(CommandType.SetWidth, arg),
            _ => throw new ArgumentException($"Unknown command: {cmd}")
        };
    }

    /// <summary>
    /// コマンドのパースを試行
    /// </summary>
    public static bool TryParse(string commandString, out Command? command)
    {
        try
        {
            command = Parse(commandString);
            return true;
        }
        catch
        {
            command = null;
            return false;
        }
    }
}

// Made with Bob
