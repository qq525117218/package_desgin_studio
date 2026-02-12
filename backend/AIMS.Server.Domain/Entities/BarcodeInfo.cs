namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 条形码信息实体类
/// 用于存储条形码的核心信息
/// </summary>
public class BarcodeInfo
{
    /// <summary>
    /// 条形码值（如EAN-13对应的数字串）
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 条形码类型（默认值：EAN-13）
    /// </summary>
    public string Type { get; set; } = "EAN-13";
    
    /// <summary>
    /// 条形码关联的URL地址
    /// </summary>
    public string Url { get; set; } = string.Empty;
}