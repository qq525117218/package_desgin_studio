namespace AIMS.Server.Application.DTOs.Health;

/// <summary>
/// 系统健康检查响应数据传输对象
/// </summary>
public class HealthCheckDto
{
    /// <summary>
    /// 系统总体健康状态
    /// </summary>
    public string Status { get; set; } = "Unknown"; 

    /// <summary>
    /// 检查执行时间戳
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// 各组件健康状态列表
    /// </summary>
    public List<ComponentHealth> Components { get; set; } = new();
}