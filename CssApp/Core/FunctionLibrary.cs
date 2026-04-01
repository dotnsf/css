using CssApp.Models;

namespace CssApp.Core;

/// <summary>
/// スプレッドシート関数を提供するライブラリ
/// </summary>
public class FunctionLibrary
{
    private readonly Worksheet _worksheet;
    private readonly FormulaEvaluator _evaluator;

    public FunctionLibrary(Worksheet worksheet, FormulaEvaluator evaluator)
    {
        _worksheet = worksheet;
        _evaluator = evaluator;
    }

    /// <summary>
    /// 関数を呼び出す
    /// </summary>
    public object? CallFunction(string functionName, List<object?> args)
    {
        return functionName.ToUpper() switch
        {
            "SUM" => Sum(args),
            "AVERAGE" => Average(args),
            "MIN" => Min(args),
            "MAX" => Max(args),
            "IF" => If(args),
            "COUNT" => Count(args),
            "COUNTA" => CountA(args),
            _ => throw new InvalidOperationException($"Unknown function: {functionName}")
        };
    }

    /// <summary>
    /// SUM関数 - 合計を計算
    /// </summary>
    private double Sum(List<object?> args)
    {
        var numbers = GetNumbers(args);
        return numbers.Sum();
    }

    /// <summary>
    /// AVERAGE関数 - 平均を計算
    /// </summary>
    private double Average(List<object?> args)
    {
        var numbers = GetNumbers(args);
        if (numbers.Count == 0)
            return 0;
        return numbers.Average();
    }

    /// <summary>
    /// MIN関数 - 最小値を取得
    /// </summary>
    private double Min(List<object?> args)
    {
        var numbers = GetNumbers(args);
        if (numbers.Count == 0)
            return 0;
        return numbers.Min();
    }

    /// <summary>
    /// MAX関数 - 最大値を取得
    /// </summary>
    private double Max(List<object?> args)
    {
        var numbers = GetNumbers(args);
        if (numbers.Count == 0)
            return 0;
        return numbers.Max();
    }

    /// <summary>
    /// IF関数 - 条件分岐
    /// IF(condition, trueValue, falseValue)
    /// </summary>
    private object? If(List<object?> args)
    {
        if (args.Count < 2)
            throw new ArgumentException("IF function requires at least 2 arguments");

        bool condition = ToBoolean(args[0]);
        
        if (condition)
            return args[1];
        
        return args.Count > 2 ? args[2] : null;
    }

    /// <summary>
    /// COUNT関数 - 数値の個数を数える
    /// </summary>
    private double Count(List<object?> args)
    {
        var numbers = GetNumbers(args);
        return numbers.Count;
    }

    /// <summary>
    /// COUNTA関数 - 空でないセルの個数を数える
    /// </summary>
    private double CountA(List<object?> args)
    {
        int count = 0;

        foreach (var arg in args)
        {
            if (arg is string rangeStr && rangeStr.Contains(':'))
            {
                var range = CellRange.Parse(rangeStr);
                foreach (var address in range.GetCells())
                {
                    var cell = _worksheet.GetCell(address);
                    if (!cell.IsEmpty)
                        count++;
                }
            }
            else if (arg != null)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 引数から数値のリストを取得
    /// </summary>
    private List<double> GetNumbers(List<object?> args)
    {
        var numbers = new List<double>();

        foreach (var arg in args)
        {
            // 範囲の場合
            if (arg is string rangeStr && rangeStr.Contains(':'))
            {
                try
                {
                    var range = CellRange.Parse(rangeStr);
                    foreach (var address in range.GetCells())
                    {
                        var cell = _worksheet.GetCell(address);
                        
                        // 数式セルの場合は評価
                        if (cell.HasFormula && cell.EvaluatedValue == null)
                        {
                            try
                            {
                                var result = _evaluator.Evaluate(cell.Formula!, address);
                                cell.EvaluatedValue = result;
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        if (TryGetNumber(cell.EvaluatedValue, out double num))
                        {
                            numbers.Add(num);
                        }
                    }
                }
                catch
                {
                    // 範囲のパースに失敗した場合は無視
                }
            }
            // 単一の値
            else if (TryGetNumber(arg, out double num))
            {
                numbers.Add(num);
            }
        }

        return numbers;
    }

    /// <summary>
    /// 値を数値に変換を試行
    /// </summary>
    private bool TryGetNumber(object? value, out double result)
    {
        result = 0;

        if (value == null)
            return false;

        if (value is double d)
        {
            result = d;
            return true;
        }

        if (value is int i)
        {
            result = i;
            return true;
        }

        if (value is float f)
        {
            result = f;
            return true;
        }

        if (value is decimal dec)
        {
            result = (double)dec;
            return true;
        }

        if (value is string s && double.TryParse(s, out double parsed))
        {
            result = parsed;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 値をブール値に変換
    /// </summary>
    private bool ToBoolean(object? value)
    {
        if (value == null)
            return false;

        if (value is bool b)
            return b;

        if (value is double d)
            return Math.Abs(d) > 0.0000001;

        if (value is int i)
            return i != 0;

        if (value is string s)
        {
            if (bool.TryParse(s, out bool result))
                return result;
            
            // 非空文字列はtrue
            return !string.IsNullOrWhiteSpace(s);
        }

        return true;
    }
}

// Made with Bob
