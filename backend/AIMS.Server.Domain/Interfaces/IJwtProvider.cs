namespace AIMS.Server.Domain.Interfaces;

/// <summary>
/// JWT令牌提供器接口
/// 定义生成JWT访问令牌的核心契约
/// </summary>
public interface IJwtProvider
{
    /// <summary>
    /// 生成JWT访问令牌
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="expireMinutes">令牌过期分钟数</param>
    /// <returns>生成的JWT令牌字符串</returns>
    string GenerateToken(string username, int expireMinutes);
}