using Newtonsoft.Json;

namespace AIMS.Server.Application.DTOs.Plm;

/// <summary>
/// PLM 条码信息传输对象
/// </summary>
/// <remarks>
/// 用于接收 PLM 系统返回的条形码数据及图片路径。
/// </remarks>
public class BarCodeDto
{
    /// <summary>
    /// 条形码数值
    /// </summary>
    [JsonProperty("bar_code")]
    public string BarCode { get; set; } = string.Empty;

    /// <summary>
    /// 条形码图片路径或 URL
    /// </summary>
    [JsonProperty("bar_code_path")]
    public string BarCodePath { get; set; } = string.Empty;
}