namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 动态图片资源传输对象
/// </summary>
public class DynamicImagesDto
{
    /// <summary>
    /// 条码配置
    /// </summary>
    public BarcodeConfigDto Barcode { get; set; } = new();
}