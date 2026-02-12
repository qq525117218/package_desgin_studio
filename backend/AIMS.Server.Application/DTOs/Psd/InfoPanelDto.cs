namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 信息面板内容传输对象
/// </summary>
public class InfoPanelDto
{
    /// <summary>
    /// 产品名称
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 保质期
    /// </summary>
    public string ShelfLife { get; set; } = string.Empty;

    /// <summary>
    /// 成分表
    /// </summary>
    public string Ingredients { get; set; } = string.Empty;

    /// <summary>
    /// 制造商名称
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 原产地
    /// </summary>
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// 警告语
    /// </summary>
    public string Warnings { get; set; } = string.Empty;

    /// <summary>
    /// 使用说明
    /// </summary>
    public string Directions { get; set; } = string.Empty;

    /// <summary>
    /// 制造商地址
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// 产品功效
    /// </summary>
    public string Benefits { get; set; } = string.Empty;
}