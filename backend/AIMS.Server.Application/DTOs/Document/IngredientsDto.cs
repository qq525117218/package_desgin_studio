namespace AIMS.Server.Application.DTOs.Document;

/// <summary>
/// 成分解析数据传输对象
/// </summary>
public class IngredientsDto
{
    /// <summary>
    /// 活性成分列表
    /// </summary>
    public string ActiveIngredients { get; set; } = string.Empty;

    /// <summary>
    /// 非活性成分列表
    /// </summary>
    public string InactiveIngredients { get; set; } = string.Empty;
    
    /// <summary>
    /// 原始成分文本
    /// </summary>
    public string RawText { get; set; } = string.Empty;
}