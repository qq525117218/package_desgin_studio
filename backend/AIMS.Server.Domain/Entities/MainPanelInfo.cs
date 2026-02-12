namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 主面板信息实体类
/// </summary>
public class MainPanelInfo
{
    /// <summary>
    /// 品牌名称
    /// </summary>
    public string BrandName { get; set; } = string.Empty;

    /// <summary>
    /// 容量信息（正面）
    /// </summary>
    public string CapacityInfo { get; set; } = string.Empty;

    /// <summary>
    /// 容量信息（背面）
    /// </summary>
    public string CapacityInfoBack { get; set; } = string.Empty;

    /// <summary>
    /// 生产厂家
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 详细地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 产品卖点列表
    /// </summary>
    public List<string> SellingPoints { get; set; } = new();
}