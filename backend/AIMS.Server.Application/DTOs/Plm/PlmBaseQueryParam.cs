namespace AIMS.Server.Application.DTOs.Plm;

/// <summary>
/// PLM 接口基础请求参数基类
/// </summary>
public class PlmBaseQueryParam
{
    /// <summary>
    /// 应用接入标识 Key
    /// </summary>
    public string app_key { get; set; } = string.Empty;

    /// <summary>
    /// 请求时间戳
    /// </summary>
    public string timestamp { get; set; } = string.Empty;

    /// <summary>
    /// 安全校验签名
    /// </summary>
    public string signature { get; set; } = string.Empty;
}