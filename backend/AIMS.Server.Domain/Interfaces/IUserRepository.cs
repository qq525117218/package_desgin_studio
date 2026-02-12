namespace AIMS.Server.Domain.Interfaces;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 验证用户账号密码是否正确
    /// </summary>
    Task<bool> ValidateUserAsync(string username, string password);
}