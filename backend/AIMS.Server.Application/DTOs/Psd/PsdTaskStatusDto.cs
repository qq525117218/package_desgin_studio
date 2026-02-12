namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// PSD 处理任务状态传输对象
/// </summary>
public class PsdTaskStatusDto
{
    /// <summary>
    /// 任务唯一标识 ID
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务执行进度 (0-100)
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// 任务状态 (Pending, Processing, Completed, Failed)
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// 状态描述或错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 生成成功后的文件下载地址
    /// </summary>
    public string? DownloadUrl { get; set; }
}