namespace CssApp.Models;

/// <summary>
/// セルのエラータイプ
/// </summary>
public enum CellError
{
    None,
    DivisionByZero,    // #DIV/0!
    InvalidReference,  // #REF!
    InvalidName,       // #NAME?
    ValueError,        // #VALUE!
    CircularReference, // #CIRCULAR!
    GeneralError      // #ERROR!
}

/// <summary>
/// スプレッドシートのセルを表すクラス
/// </summary>
public class Cell
{
    /// <summary>
    /// セルの生の値（ユーザーが入力した文字列）
    /// </summary>
    public string RawValue { get; set; }

    /// <summary>
    /// 数式（=で始まる場合）
    /// </summary>
    public string? Formula { get; private set; }

    /// <summary>
    /// 評価された値（数値、文字列、またはnull）
    /// </summary>
    public object? EvaluatedValue { get; set; }

    /// <summary>
    /// エラー状態
    /// </summary>
    public CellError Error { get; set; }

    /// <summary>
    /// 数式を持つかどうか
    /// </summary>
    public bool HasFormula => Formula != null;

    /// <summary>
    /// 表示用の文字列を取得
    /// </summary>
    public string DisplayValue
    {
        get
        {
            if (Error != CellError.None)
            {
                return Error switch
                {
                    CellError.DivisionByZero => "#DIV/0!",
                    CellError.InvalidReference => "#REF!",
                    CellError.InvalidName => "#NAME?",
                    CellError.ValueError => "#VALUE!",
                    CellError.CircularReference => "#CIRCULAR!",
                    _ => "#ERROR!"
                };
            }

            if (EvaluatedValue == null)
                return "";

            return EvaluatedValue.ToString() ?? "";
        }
    }

    public Cell(string rawValue = "")
    {
        RawValue = rawValue;
        UpdateFormula();
        EvaluatedValue = rawValue;
        Error = CellError.None;
    }

    /// <summary>
    /// セルの値を設定
    /// </summary>
    public void SetValue(string value)
    {
        RawValue = value;
        UpdateFormula();
        Error = CellError.None;
        
        // 数式でない場合は、そのまま評価値として設定
        if (!HasFormula)
        {
            // 数値として解釈できるか試行
            if (double.TryParse(value, out double numValue))
            {
                EvaluatedValue = numValue;
            }
            else
            {
                EvaluatedValue = value;
            }
        }
    }

    /// <summary>
    /// 数式を更新
    /// </summary>
    private void UpdateFormula()
    {
        if (!string.IsNullOrEmpty(RawValue) && RawValue.StartsWith("="))
        {
            Formula = RawValue.Substring(1); // = を除いた部分
        }
        else
        {
            Formula = null;
        }
    }

    /// <summary>
    /// セルが空かどうか
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(RawValue);

    /// <summary>
    /// 数値を取得（数値でない場合は0を返す）
    /// </summary>
    public double GetNumericValue()
    {
        if (EvaluatedValue is double d)
            return d;
        
        if (EvaluatedValue is int i)
            return i;
        
        if (EvaluatedValue is string s && double.TryParse(s, out double result))
            return result;
        
        return 0;
    }

    /// <summary>
    /// セルをクリア
    /// </summary>
    public void Clear()
    {
        RawValue = "";
        Formula = null;
        EvaluatedValue = null;
        Error = CellError.None;
    }

    public override string ToString()
    {
        return DisplayValue;
    }
}

// Made with Bob
