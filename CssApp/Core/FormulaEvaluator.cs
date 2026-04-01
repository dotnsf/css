using CssApp.Models;

namespace CssApp.Core;

/// <summary>
/// 数式を評価するクラス
/// </summary>
public class FormulaEvaluator
{
    private readonly Worksheet _worksheet;
    private readonly FunctionLibrary _functions;
    private readonly HashSet<CellAddress> _evaluationStack;

    public FormulaEvaluator(Worksheet worksheet)
    {
        _worksheet = worksheet;
        _functions = new FunctionLibrary(worksheet, this);
        _evaluationStack = new HashSet<CellAddress>();
    }

    /// <summary>
    /// 数式を評価
    /// </summary>
    public object? Evaluate(string formula, CellAddress currentCell)
    {
        try
        {
            // 循環参照チェック
            if (_evaluationStack.Contains(currentCell))
            {
                throw new InvalidOperationException("Circular reference detected");
            }

            _evaluationStack.Add(currentCell);

            try
            {
                var tokenizer = new FormulaTokenizer(formula);
                var tokens = tokenizer.Tokenize();
                var parser = new ExpressionParser(tokens, _worksheet, _functions, this);
                return parser.Parse();
            }
            finally
            {
                _evaluationStack.Remove(currentCell);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Circular"))
        {
            throw;
        }
        catch (DivideByZeroException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Formula evaluation error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// ワークシート内の全ての数式を評価
    /// </summary>
    public void EvaluateAll()
    {
        foreach (var (address, cell) in _worksheet.GetAllNonEmptyCells())
        {
            if (cell.HasFormula)
            {
                try
                {
                    var result = Evaluate(cell.Formula!, address);
                    cell.EvaluatedValue = result;
                    cell.Error = CellError.None;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Circular"))
                {
                    cell.Error = CellError.CircularReference;
                    cell.EvaluatedValue = null;
                }
                catch (DivideByZeroException)
                {
                    cell.Error = CellError.DivisionByZero;
                    cell.EvaluatedValue = null;
                }
                catch
                {
                    cell.Error = CellError.GeneralError;
                    cell.EvaluatedValue = null;
                }
            }
        }
    }
}

/// <summary>
/// 式をパースして評価するパーサー
/// </summary>
internal class ExpressionParser
{
    private readonly List<Token> _tokens;
    private int _position;
    private readonly Worksheet _worksheet;
    private readonly FunctionLibrary _functions;
    private readonly FormulaEvaluator _evaluator;

    public ExpressionParser(List<Token> tokens, Worksheet worksheet, FunctionLibrary functions, FormulaEvaluator evaluator)
    {
        _tokens = tokens;
        _position = 0;
        _worksheet = worksheet;
        _functions = functions;
        _evaluator = evaluator;
    }

    private Token Current => _tokens[_position];
    private Token Peek(int offset = 1) => _position + offset < _tokens.Count ? _tokens[_position + offset] : _tokens[^1];

    public object? Parse()
    {
        return ParseComparison();
    }

    private object? ParseComparison()
    {
        var left = ParseExpression();

        while (Current.Type == TokenType.Comparison)
        {
            string op = Current.Value;
            _position++;
            var right = ParseExpression();

            left = EvaluateComparison(left, op, right);
        }

        return left;
    }

    private object? ParseExpression()
    {
        var left = ParseTerm();

        while (Current.Type == TokenType.Operator && (Current.Value == "+" || Current.Value == "-"))
        {
            string op = Current.Value;
            _position++;
            var right = ParseTerm();

            double leftNum = ToNumber(left);
            double rightNum = ToNumber(right);

            left = op == "+" ? leftNum + rightNum : leftNum - rightNum;
        }

        return left;
    }

    private object? ParseTerm()
    {
        var left = ParseFactor();

        while (Current.Type == TokenType.Operator && (Current.Value == "*" || Current.Value == "/"))
        {
            string op = Current.Value;
            _position++;
            var right = ParseFactor();

            double leftNum = ToNumber(left);
            double rightNum = ToNumber(right);

            if (op == "/")
            {
                if (Math.Abs(rightNum) < 0.0000001)
                    throw new DivideByZeroException();
                left = leftNum / rightNum;
            }
            else
            {
                left = leftNum * rightNum;
            }
        }

        return left;
    }

    private object? ParseFactor()
    {
        // 負の数
        if (Current.Type == TokenType.Operator && Current.Value == "-")
        {
            _position++;
            var value = ParseFactor();
            return -ToNumber(value);
        }

        // 正の数（単項プラス）
        if (Current.Type == TokenType.Operator && Current.Value == "+")
        {
            _position++;
            return ParseFactor();
        }

        // 括弧
        if (Current.Type == TokenType.LeftParen)
        {
            _position++;
            var value = ParseComparison();
            if (Current.Type == TokenType.RightParen)
                _position++;
            return value;
        }

        // 数値
        if (Current.Type == TokenType.Number)
        {
            double value = double.Parse(Current.Value);
            _position++;
            return value;
        }

        // 文字列
        if (Current.Type == TokenType.String)
        {
            string value = Current.Value;
            _position++;
            return value;
        }

        // セル参照
        if (Current.Type == TokenType.CellReference)
        {
            string cellRef = Current.Value;
            _position++;
            return ResolveCellReference(cellRef);
        }

        // 関数
        if (Current.Type == TokenType.Function)
        {
            return ParseFunction();
        }

        throw new FormatException($"Unexpected token: {Current}");
    }

    private object? ParseFunction()
    {
        string functionName = Current.Value;
        _position++;

        if (Current.Type != TokenType.LeftParen)
            throw new FormatException($"Expected '(' after function name {functionName}");

        _position++;

        var args = new List<object?>();

        // 引数を解析
        if (Current.Type != TokenType.RightParen)
        {
            while (true)
            {
                // 範囲の場合
                if (Current.Type == TokenType.Range)
                {
                    args.Add(Current.Value);
                    _position++;
                }
                else
                {
                    args.Add(ParseComparison());
                }

                if (Current.Type == TokenType.Comma)
                {
                    _position++;
                    continue;
                }

                break;
            }
        }

        if (Current.Type != TokenType.RightParen)
            throw new FormatException("Expected ')' after function arguments");

        _position++;

        return _functions.CallFunction(functionName, args);
    }

    private object? ResolveCellReference(string cellRef)
    {
        try
        {
            var address = CellAddress.Parse(cellRef);
            var cell = _worksheet.GetCell(address);

            if (cell.HasFormula && cell.EvaluatedValue == null)
            {
                // 数式を評価
                var result = _evaluator.Evaluate(cell.Formula!, address);
                cell.EvaluatedValue = result;
                cell.Error = CellError.None;
            }

            return cell.EvaluatedValue;
        }
        catch
        {
            throw new FormatException($"Invalid cell reference: {cellRef}");
        }
    }

    private bool EvaluateComparison(object? left, string op, object? right)
    {
        // 数値比較を試行
        if (IsNumeric(left) && IsNumeric(right))
        {
            double leftNum = ToNumber(left);
            double rightNum = ToNumber(right);

            return op switch
            {
                ">" => leftNum > rightNum,
                "<" => leftNum < rightNum,
                ">=" => leftNum >= rightNum,
                "<=" => leftNum <= rightNum,
                "=" or "==" => Math.Abs(leftNum - rightNum) < 0.0000001,
                "!=" or "<>" => Math.Abs(leftNum - rightNum) >= 0.0000001,
                _ => throw new FormatException($"Unknown comparison operator: {op}")
            };
        }

        // 文字列比較
        string leftStr = left?.ToString() ?? "";
        string rightStr = right?.ToString() ?? "";

        return op switch
        {
            "=" or "==" => leftStr == rightStr,
            "!=" or "<>" => leftStr != rightStr,
            _ => throw new FormatException($"Cannot compare non-numeric values with operator: {op}")
        };
    }

    private bool IsNumeric(object? value)
    {
        return value is double or int or float or decimal;
    }

    private double ToNumber(object? value)
    {
        if (value == null)
            return 0;

        if (value is double d)
            return d;

        if (value is int i)
            return i;

        if (value is string s && double.TryParse(s, out double result))
            return result;

        if (value is bool b)
            return b ? 1 : 0;

        return 0;
    }
}

// Made with Bob
