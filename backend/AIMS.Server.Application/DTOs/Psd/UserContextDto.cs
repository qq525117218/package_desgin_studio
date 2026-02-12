
namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 用户上下文数据传输对象（DTO）
/// 用于在PSD相关业务流程中传递用户上下文信息
/// </summary>
public class UserContextDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 是否生成刀模线（Dieline）
    /// </summary>
 
    public bool GenerateDieline { get; set; } = true;
}