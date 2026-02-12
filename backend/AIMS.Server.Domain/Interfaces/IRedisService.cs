using System.Threading.Tasks;

namespace AIMS.Server.Domain.Interfaces;

/// <summary>
/// Redis缓存服务接口
/// 定义Redis基础操作的异步契约
/// </summary>
public interface IRedisService
{
    /// <summary>
    /// 设置缓存键值对（覆盖已有值）
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiry);
    
    /// <summary>
    /// 原子操作：仅当键不存在时设置值 (Set if Not Exists)
    /// </summary>
    Task<bool> SetNxAsync<T>(string key, T value, TimeSpan expiry);
    
    /// <summary>
    /// 获取缓存值
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// 删除指定缓存键
    /// </summary>
    Task RemoveAsync(string key);
}