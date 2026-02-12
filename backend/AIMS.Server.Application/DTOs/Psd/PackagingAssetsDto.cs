namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 包装素材集合传输对象
/// </summary>
public class PackagingAssetsDto
{
    /// <summary>
    /// 文本素材集合
    /// </summary>
    public TextAssetsDto Texts { get; set; } = new();

    /// <summary>
    /// 动态图片素材集合
    /// </summary>
    public DynamicImagesDto DynamicImages { get; set; } = new();
}