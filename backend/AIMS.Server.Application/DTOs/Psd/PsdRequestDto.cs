using System.ComponentModel.DataAnnotations;

namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// PSD 生成请求传输对象
/// </summary>
public class PsdRequestDto
{
    /// <summary>
    /// 项目名称
    /// </summary>
    [Required]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 用户上下文信息
    /// </summary>
    public UserContextDto UserContext { get; set; } = new();

    /// <summary>
    /// 包装规格配置
    /// </summary>
    [Required]
    public PackagingSpecsDto Specifications { get; set; } = new();

    /// <summary>
    /// 包装素材内容
    /// </summary>
    public PackagingAssetsDto Assets { get; set; } = new();
}