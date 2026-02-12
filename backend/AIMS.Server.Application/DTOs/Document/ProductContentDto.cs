namespace AIMS.Server.Application.DTOs.Document;

/// <summary>
/// 产品内容数据传输对象
/// </summary>
public class ProductContentDto
{
    /// <summary>
    /// 产品名称
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 产品成分信息
    /// </summary>
    public IngredientsDto Ingredients { get; set; } = new();

    /// <summary>
    /// 警告语
    /// </summary>
    public string Warnings { get; set; } = string.Empty;

    /// <summary>
    /// 保质期
    /// </summary>
    public string ShelfLife { get; set; } = string.Empty;

    /// <summary>
    /// 制造商名称
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 制造商地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 原产国
    /// </summary>
    public string CountryOfOrigin { get; set; } = string.Empty;

    /// <summary>
    /// 建议使用方法
    /// </summary>
    public string Directions { get; set; } = string.Empty;
    
    /// <summary>
    /// 产品功效与优势
    /// </summary>
    public string Benefits { get; set; } = string.Empty;
}