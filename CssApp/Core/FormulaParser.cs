using CssApp.Models;

namespace CssApp.Core;

/// <summary>
/// トークンの種類
/// </summary>
public enum TokenType
{
    Number,
    CellReference,
    Range,
    Function,
    Operator,
    LeftParen,
    RightParen,
    Comma,
    String,
    Comparison,
    End
}

/// <summary>
/// トークン
/// </summary>
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString() => $"{Type}: {Value}";
}

/// <summary>
/// 数式をトークンに分解するトークナイザー
/// </summary>
public class FormulaTokenizer
{
    private readonly string _formula;
    private int _position;

    public FormulaTokenizer(string formula)
    {
        _formula = formula ?? "";
        _position = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_position < _formula.Length)
        {
            char current = _formula[_position];

            // 空白をスキップ
            if (char.IsWhiteSpace(current))
            {
                _position++;
                continue;
            }

            // 数値
            if (char.IsDigit(current) || (current == '.' && _position + 1 < _formula.Length && char.IsDigit(_formula[_position + 1])))
            {
                tokens.Add(ReadNumber());
                continue;
            }

            // 文字列（ダブルクォート）
            if (current == '"')
            {
                tokens.Add(ReadString());
                continue;
            }

            // セル参照または関数
            if (char.IsLetter(current))
            {
                tokens.Add(ReadIdentifier());
                continue;
            }

            // 演算子と記号
            switch (current)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                    tokens.Add(new Token(TokenType.Operator, current.ToString()));
                    _position++;
                    break;

                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    _position++;
                    break;

                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    _position++;
                    break;

                case ',':
                    tokens.Add(new Token(TokenType.Comma, ","));
                    _position++;
                    break;

                case '>':
                case '<':
                case '=':
                case '!':
                    tokens.Add(ReadComparison());
                    break;

                default:
                    throw new FormatException($"Unexpected character: {current}");
            }
        }

        tokens.Add(new Token(TokenType.End, ""));
        return tokens;
    }

    private Token ReadNumber()
    {
        int start = _position;
        bool hasDecimal = false;

        while (_position < _formula.Length)
        {
            char c = _formula[_position];
            if (char.IsDigit(c))
            {
                _position++;
            }
            else if (c == '.' && !hasDecimal)
            {
                hasDecimal = true;
                _position++;
            }
            else
            {
                break;
            }
        }

        return new Token(TokenType.Number, _formula.Substring(start, _position - start));
    }

    private Token ReadString()
    {
        _position++; // Skip opening quote
        int start = _position;

        while (_position < _formula.Length && _formula[_position] != '"')
        {
            _position++;
        }

        string value = _formula.Substring(start, _position - start);
        
        if (_position < _formula.Length)
            _position++; // Skip closing quote

        return new Token(TokenType.String, value);
    }

    private Token ReadIdentifier()
    {
        int start = _position;

        // 文字を読み取る
        while (_position < _formula.Length && char.IsLetter(_formula[_position]))
        {
            _position++;
        }

        string identifier = _formula.Substring(start, _position - start).ToUpper();

        // 数字が続く場合はセル参照
        if (_position < _formula.Length && char.IsDigit(_formula[_position]))
        {
            while (_position < _formula.Length && char.IsDigit(_formula[_position]))
            {
                _position++;
            }

            string cellRef = _formula.Substring(start, _position - start);

            // 範囲かチェック（A1:B10）
            if (_position < _formula.Length && _formula[_position] == ':')
            {
                _position++; // Skip ':'
                int rangeStart = start;
                
                // 終了セルを読み取る
                while (_position < _formula.Length && (char.IsLetter(_formula[_position]) || char.IsDigit(_formula[_position])))
                {
                    _position++;
                }

                string range = _formula.Substring(rangeStart, _position - rangeStart);
                return new Token(TokenType.Range, range);
            }

            return new Token(TokenType.CellReference, cellRef);
        }

        // 関数名
        return new Token(TokenType.Function, identifier);
    }

    private Token ReadComparison()
    {
        int start = _position;
        _position++;

        // >=, <=, ==, != などの2文字演算子をチェック
        if (_position < _formula.Length && (_formula[_position] == '=' || _formula[start] == '!' && _formula[_position] == '='))
        {
            _position++;
        }

        return new Token(TokenType.Comparison, _formula.Substring(start, _position - start));
    }
}

// Made with Bob
