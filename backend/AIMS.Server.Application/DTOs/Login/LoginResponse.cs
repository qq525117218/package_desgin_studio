namespace AIMS.Server.Application.DTOs;

/// <summary>
/// 用户登录成功响应数据
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 身份验证令牌 (JWT)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTime ExpireAt { get; set; }
}