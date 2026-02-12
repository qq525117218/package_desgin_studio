namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 包装素材聚合根 (文案、图片、条码)
/// </summary>
public class PackagingAssets
{
    /// <summary>
    /// 包装相关的文案素材信息
    /// </summary>
    public TextAssets Texts { get; set; } = new();

    /// <summary>
    /// 包装相关的动态图片素材信息（含条码）
    /// </summary>
    public DynamicImages Images { get; set; } = new();
}