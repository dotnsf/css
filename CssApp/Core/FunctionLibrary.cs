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
            "COUNTIF" => CountIf(args),
            "SUMIF" => SumIf(args),
            "VLOOKUP" => VLookup(args),
            "IFERROR" => IfError(args),
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
    /// COUNTIF関数 - 条件を満たすセルの個数を数える
    /// COUNTIF(range, criteria)
    /// </summary>
    private double CountIf(List<object?> args)
    {
        if (args.Count < 2)
            throw new ArgumentException("COUNTIF function requires 2 arguments");

        if (args[0] is not string rangeStr || !rangeStr.Contains(':'))
            throw new ArgumentException("First argument must be a range");

        var criteria = args[1]?.ToString() ?? "";
        int count = 0;

        var range = CellRange.Parse(rangeStr);
        foreach (var address in range.GetCells())
        {
            var cell = _worksheet.GetCell(address);
            
            // 数式セルの場合は評価
            object? value = cell.EvaluatedValue;
            if (cell.HasFormula && value == null)
            {
                try
                {
                    value = _evaluator.Evaluate(cell.Formula!, address);
                    cell.EvaluatedValue = value;
                }
                catch
                {
                    continue;
                }
            }

            if (MatchesCriteria(value, criteria))
                count++;
        }

        return count;
    }

    /// <summary>
    /// SUMIF関数 - 条件を満たすセルの合計を計算
    /// SUMIF(range, criteria, [sum_range])
    /// </summary>
    private double SumIf(List<object?> args)
    {
        if (args.Count < 2)
            throw new ArgumentException("SUMIF function requires at least 2 arguments");

        if (args[0] is not string rangeStr || !rangeStr.Contains(':'))
            throw new ArgumentException("First argument must be a range");

        var criteria = args[1]?.ToString() ?? "";
        
        // sum_rangeが指定されている場合
        string? sumRangeStr = args.Count > 2 ? args[2]?.ToString() : null;
        
        double sum = 0;
        var range = CellRange.Parse(rangeStr);
        var cells = range.GetCells().ToList();

        CellRange? sumRange = null;
        List<CellAddress>? sumCells = null;
        
        if (sumRangeStr != null && sumRangeStr.Contains(':'))
        {
            sumRange = CellRange.Parse(sumRangeStr);
            sumCells = sumRange.GetCells().ToList();
            
            // 範囲のサイズが一致しない場合はエラー
            if (cells.Count != sumCells.Count)
                throw new ArgumentException("Range and sum_range must have the same size");
        }

        for (int i = 0; i < cells.Count; i++)
        {
            var address = cells[i];
            var cell = _worksheet.GetCell(address);
            
            // 数式セルの場合は評価
            object? value = cell.EvaluatedValue;
            if (cell.HasFormula && value == null)
            {
                try
                {
                    value = _evaluator.Evaluate(cell.Formula!, address);
                    cell.EvaluatedValue = value;
                }
                catch
                {
                    continue;
                }
            }

            if (MatchesCriteria(value, criteria))
            {
                // sum_rangeが指定されている場合は対応するセルの値を加算
                if (sumCells != null)
                {
                    var sumCell = _worksheet.GetCell(sumCells[i]);
                    object? sumValue = sumCell.EvaluatedValue;
                    
                    if (sumCell.HasFormula && sumValue == null)
                    {
                        try
                        {
                            sumValue = _evaluator.Evaluate(sumCell.Formula!, sumCells[i]);
                            sumCell.EvaluatedValue = sumValue;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    
                    if (TryGetNumber(sumValue, out double num))
                        sum += num;
                }
                else
                {
                    // sum_rangeが指定されていない場合は条件範囲の値を加算
                    if (TryGetNumber(value, out double num))
                        sum += num;
                }
            }
        }

        return sum;
    }

    /// <summary>
    /// VLOOKUP関数 - 垂直検索
    /// VLOOKUP(lookup_value, table_array, col_index_num, [range_lookup])
    /// </summary>
    private object? VLookup(List<object?> args)
    {
        if (args.Count < 3)
            throw new ArgumentException("VLOOKUP function requires at least 3 arguments");

        var lookupValue = args[0];
        
        if (args[1] is not string tableRangeStr || !tableRangeStr.Contains(':'))
            throw new ArgumentException("Second argument must be a range");

        if (!TryGetNumber(args[2], out double colIndexDouble))
            throw new ArgumentException("Third argument must be a number");

        int colIndex = (int)colIndexDouble;
        if (colIndex < 1)
            throw new ArgumentException("Column index must be >= 1");

        bool exactMatch = args.Count > 3 ? !ToBoolean(args[3]) : true;

        var tableRange = CellRange.Parse(tableRangeStr);
        int startRow = Math.Min(tableRange.Start.Row, tableRange.End.Row);
        int endRow = Math.Max(tableRange.Start.Row, tableRange.End.Row);
        int startCol = Math.Min(tableRange.Start.Column, tableRange.End.Column);
        int endCol = Math.Max(tableRange.Start.Column, tableRange.End.Column);

        // 列インデックスが範囲内かチェック
        if (startCol + colIndex - 1 > endCol)
            throw new ArgumentException("Column index is out of range");

        // 各行の最初の列で検索
        for (int row = startRow; row <= endRow; row++)
        {
            var cell = _worksheet.GetCell(row, startCol);
            object? cellValue = cell.EvaluatedValue;
            
            if (cell.HasFormula && cellValue == null)
            {
                try
                {
                    cellValue = _evaluator.Evaluate(cell.Formula!, new CellAddress(row, startCol));
                    cell.EvaluatedValue = cellValue;
                }
                catch
                {
                    continue;
                }
            }

            bool isMatch = false;
            
            if (exactMatch)
            {
                // 完全一致
                isMatch = AreEqual(cellValue, lookupValue);
            }
            else
            {
                // 近似一致（lookupValue以下の最大値）
                if (TryGetNumber(cellValue, out double cellNum) && 
                    TryGetNumber(lookupValue, out double lookupNum))
                {
                    isMatch = cellNum <= lookupNum;
                }
            }

            if (isMatch)
            {
                // 対応する列の値を返す
                var resultCell = _worksheet.GetCell(row, startCol + colIndex - 1);
                object? resultValue = resultCell.EvaluatedValue;
                
                if (resultCell.HasFormula && resultValue == null)
                {
                    try
                    {
                        resultValue = _evaluator.Evaluate(resultCell.Formula!, new CellAddress(row, startCol + colIndex - 1));
                        resultCell.EvaluatedValue = resultValue;
                    }
                    catch
                    {
                        return "#ERROR!";
                    }
                }
                
                if (exactMatch)
                    return resultValue;
                
                // 近似一致の場合は最後にマッチした値を保持
                if (row == endRow || !exactMatch)
                    return resultValue;
            }
            else if (!exactMatch && row > startRow)
            {
                // 近似一致で現在の値が大きすぎる場合、前の行の値を返す
                var resultCell = _worksheet.GetCell(row - 1, startCol + colIndex - 1);
                object? resultValue = resultCell.EvaluatedValue;
                
                if (resultCell.HasFormula && resultValue == null)
                {
                    try
                    {
                        resultValue = _evaluator.Evaluate(resultCell.Formula!, new CellAddress(row - 1, startCol + colIndex - 1));
                        resultCell.EvaluatedValue = resultValue;
                    }
                    catch
                    {
                        return "#ERROR!";
                    }
                }
                
                return resultValue;
            }
        }

        return "#N/A";
    }

    /// <summary>
    /// IFERROR関数 - エラー処理
    /// IFERROR(value, value_if_error)
    /// </summary>
    private object? IfError(List<object?> args)
    {
        if (args.Count < 2)
            throw new ArgumentException("IFERROR function requires 2 arguments");

        var value = args[0];
        var valueIfError = args[1];

        // 値がエラーかチェック
        if (value is string str && (str.StartsWith("#") || str == "ERROR"))
            return valueIfError;

        return value;
    }

    /// <summary>
    /// 値が条件に一致するかチェック
    /// </summary>
    private bool MatchesCriteria(object? value, string criteria)
    {
        if (string.IsNullOrEmpty(criteria))
            return false;

        // 比較演算子をチェック
        if (criteria.StartsWith(">="))
        {
            var compareValue = criteria.Substring(2);
            if (TryGetNumber(value, out double num1) && double.TryParse(compareValue, out double num2))
                return num1 >= num2;
        }
        else if (criteria.StartsWith("<="))
        {
            var compareValue = criteria.Substring(2);
            if (TryGetNumber(value, out double num1) && double.TryParse(compareValue, out double num2))
                return num1 <= num2;
        }
        else if (criteria.StartsWith(">"))
        {
            var compareValue = criteria.Substring(1);
            if (TryGetNumber(value, out double num1) && double.TryParse(compareValue, out double num2))
                return num1 > num2;
        }
        else if (criteria.StartsWith("<"))
        {
            var compareValue = criteria.Substring(1);
            if (TryGetNumber(value, out double num1) && double.TryParse(compareValue, out double num2))
                return num1 < num2;
        }
        else if (criteria.StartsWith("<>"))
        {
            var compareValue = criteria.Substring(2);
            return !AreEqual(value, compareValue);
        }
        else if (criteria.StartsWith("="))
        {
            var compareValue = criteria.Substring(1);
            return AreEqual(value, compareValue);
        }
        else
        {
            // 演算子がない場合は等価比較
            return AreEqual(value, criteria);
        }

        return false;
    }

    /// <summary>
    /// 2つの値が等しいかチェック
    /// </summary>
    private bool AreEqual(object? value1, object? value2)
    {
        if (value1 == null && value2 == null)
            return true;
        
        if (value1 == null || value2 == null)
            return false;

        // 数値として比較
        if (TryGetNumber(value1, out double num1) && TryGetNumber(value2, out double num2))
            return Math.Abs(num1 - num2) < 0.0000001;

        // 文字列として比較
        return value1.ToString() == value2.ToString();
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
