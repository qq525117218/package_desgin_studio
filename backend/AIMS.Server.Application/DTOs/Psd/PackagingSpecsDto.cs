using System.ComponentModel.DataAnnotations;

namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 包装规格配置传输对象
/// </summary>
public class PackagingSpecsDto
{
    /// <summary>
    /// 包装物理尺寸
    /// </summary>
    [Required]
    public DimensionsDto Dimensions { get; set; } = new();

    /// <summary>
    /// 印刷出血位及边距配置
    /// </summary>
    [Required]
    public PrintConfigDto PrintConfig { get; set; } = new();
}