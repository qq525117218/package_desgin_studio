namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 文案素材聚合类
/// </summary>
public class TextAssets
{
    /// <summary>
    /// 主面板文案信息
    /// </summary>
    public MainPanelInfo MainPanel { get; set; } = new();

    /// <summary>
    /// 信息面板文案信息
    /// </summary>
    public InfoPanelInfo InfoPanel { get; set; } = new();
}