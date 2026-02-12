using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AIMS.Server.Application.DTOs.Plm;

/// <summary>
/// PLM 编码查询请求传输对象
/// </summary>
public class PlmCodeRequestDto
{
    /// <summary>
    /// 目标查询编码
    /// </summary>
    [Required(ErrorMessage = "SKU Code 不能为空")]
    [JsonPropertyName("code")] 
    public string Code { get; set; } = string.Empty;
}