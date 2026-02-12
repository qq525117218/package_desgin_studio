using System.Text.Json.Serialization;

namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 令牌会话实体类
/// </summary>
public class TokenSession
{
    /// <summary>
    /// 会话唯一标识
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 登录用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 用户登录IP地址
    /// </summary>
    public string UserIp { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTime ExpireTime { get; set; }

    /// <summary>
    /// 领域行为：判断会话是否已过期
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.Now > ExpireTime;
    }

    /// <summary>
    /// 领域行为：创建新会话的工厂方法
    /// </summary>
    public static TokenSession Create(string username, string token, string ip, int expireMinutes)
    {
        return new TokenSession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            Username = username,
            AccessToken = token,
            UserIp = ip,
            LoginTime = DateTime.Now,
            ExpireTime = DateTime.Now.AddMinutes(expireMinutes)
        };
    }
}