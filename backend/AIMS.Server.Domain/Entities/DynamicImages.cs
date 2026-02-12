namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 动态图片信息实体类
/// 存储与动态图片关联的各类信息
/// </summary>
public class DynamicImages
{
    /// <summary>
    /// 动态图片关联的条形码信息
    /// 默认为初始化后的空条形码信息对象
    /// </summary>
    public BarcodeInfo Barcode { get; set; } = new();
}