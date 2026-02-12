namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// PSD 条码配置传输对象
/// </summary>
public class BarcodeConfigDto
{
    /// <summary>
    /// 条码数值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 条码类型 (默认 EAN-13)
    /// </summary>
    public string Type { get; set; } = "EAN-13";

    /// <summary>
    /// 条码图片链接
    /// </summary>
    public string Url { get; set; } = string.Empty;
}