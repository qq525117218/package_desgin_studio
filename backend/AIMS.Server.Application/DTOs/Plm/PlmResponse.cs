using Newtonsoft.Json;

namespace AIMS.Server.Application.DTOs.Plm;

/// <summary>
/// PLM 第三方接口通用响应封装类
/// </summary>
public class PlmResponse<T>
{
    /// <summary>
    /// 响应状态码
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 请求是否成功
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonProperty("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 请求追踪 ID
    /// </summary>
    [JsonProperty("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// 业务数据载荷
    /// </summary>
    [JsonProperty("data")]
    public T? Data { get; set; }
}