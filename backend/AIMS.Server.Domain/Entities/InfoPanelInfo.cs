namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 信息面板信息实体类
/// 存储产品信息面板展示的各类核心属性
/// </summary>
public class InfoPanelInfo
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
    /// 配料/成分
    /// </summary>
    public string Ingredients { get; set; } = string.Empty;
    
    /// <summary>
    /// 生产厂家
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;
    
    /// <summary>
    /// 产地
    /// </summary>
    public string Origin { get; set; } = string.Empty;
    
    /// <summary>
    /// 注意事项/警示语
    /// </summary>
    public string Warnings { get; set; } = string.Empty;
    
    /// <summary>
    /// 使用/食用说明
    /// </summary>
    public string Directions { get; set; } = string.Empty;
    
    /// <summary>
    /// 详细地址
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// 产品功效/益处
    /// </summary>
    public string Benefits { get; set; } = string.Empty;
}