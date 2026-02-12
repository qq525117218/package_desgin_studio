namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 包装文本素材集合传输对象
/// </summary>
public class TextAssetsDto
{
    /// <summary>
    /// 主展示面文本内容
    /// </summary>
    public MainPanelDto MainPanel { get; set; } = new();

    /// <summary>
    /// 信息面板文本内容
    /// </summary>
    public InfoPanelDto InfoPanel { get; set; } = new();
}