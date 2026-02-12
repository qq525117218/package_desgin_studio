namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 主展示面信息传输对象
/// </summary>
public class MainPanelDto
{
    /// <summary>
    /// 品牌名称
    /// </summary>
    public string BrandName { get; set; } = string.Empty;
   
    /// <summary>
    /// 净含量信息 (正面)
    /// </summary>
    public string CapacityInfo { get; set; } = string.Empty;
    
    /// <summary>
    /// 净含量信息 (背面)
    /// </summary>
    public string CapacityInfoBack { get; set; } = string.Empty;

    /// <summary>
    /// 厂商名称
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 厂商地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 产品卖点列表
    /// </summary>
    public List<string> SellingPoints { get; set; } = new();
}