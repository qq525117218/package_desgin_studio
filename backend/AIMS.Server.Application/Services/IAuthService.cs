using AIMS.Server.Application.DTOs;

namespace AIMS.Server.Application.Services;

/// <summary>
/// 认证服务接口
/// 定义用户登录、登出等认证相关核心业务逻辑契约
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 处理用户登录请求
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, string clientIp);

    /// <summary>
    /// 处理用户登出请求
    /// </summary>
    Task LogoutAsync(string token);
}