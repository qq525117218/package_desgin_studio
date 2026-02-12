using AIMS.Server.Application.DTOs.Psd;

namespace AIMS.Server.Application.Services;

/// <summary>
/// PSD文件生成服务接口
/// 定义PSD文件创建的核心业务逻辑契约
/// </summary>
public interface IPsdService
{
    /// <summary>
    /// 异步创建PSD文件
    /// </summary>
    Task<byte[]> CreatePsdFileAsync(PsdRequestDto dto, Action<int, string>? onProgress = null);
}