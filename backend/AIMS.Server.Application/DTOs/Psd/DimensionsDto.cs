using System.ComponentModel.DataAnnotations;

namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 包装尺寸参数传输对象
/// </summary>
public class DimensionsDto
{
    /// <summary>
    /// 长度 (cm)
    /// </summary>
    [Range(0.1, 1000)]
    public double Length { get; set; }
    
    /// <summary>
    /// 宽度 (cm)
    /// </summary>
    [Range(0.1, 1000)]
    public double Width { get; set; }
    
    /// <summary>
    /// 高度 (cm)
    /// </summary>
    [Range(0.1, 1000)]
    public double Height { get; set; }
}