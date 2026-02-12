namespace AIMS.Server.Application.DTOs.Health;

/// <summary>
/// 系统组件健康状态信息
/// </summary>
public class ComponentHealth
{
    /// <summary>
    /// 组件名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 健康状态
    /// </summary>

    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 状态描述信息
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 检查耗时
    /// </summary>
    public string Duration { get; set; } = string.Empty;
}