namespace AIMS.Server.Domain.Entities;

/// <summary>
/// 包装尺寸信息实体类
/// </summary>
public class PackagingDimensions
{
    /// <summary>
    /// 包装长度 (cm)
    /// </summary>
    public double Length { get; private set; }
    
    /// <summary>
    /// 包装高度 (cm)
    /// </summary>
    public double Height { get; private set; }
    
    /// <summary>
    /// 包装宽度 (cm)
    /// </summary>
    public double Width { get; private set; }
    
    /// <summary>
    /// 左右出血位 (cm)
    /// </summary>
    public double BleedLeftRight { get; private set; }
    
    /// <summary>
    /// 上下出血位 (cm)
    /// </summary>
    public double BleedTopBottom { get; private set; }
    
    /// <summary>
    /// 内部出血位 (cm)
    /// </summary>
    public double InnerBleed { get; private set; }

    /// <summary>
    /// 初始化包装尺寸信息
    /// </summary>
    public PackagingDimensions(double length, double height, double width, 
        double bleedLR, double bleedTB, double innerBleed)
    {
        if (length <= 0 || height <= 0 || width <= 0)
            throw new ArgumentException("尺寸必须大于0");
            
        Length = length;
        Height = height;
        Width = width;
        BleedLeftRight = bleedLR;
        BleedTopBottom = bleedTB;
        InnerBleed = innerBleed;
    }

    /// <summary>
    /// 计算总画布宽度 (cm)
    /// 公式：长度 + 左右出血位 * 2
    /// </summary>
    public double GetTotalWidthCm() => Length + (BleedLeftRight * 2);
        
    /// <summary>
    /// 计算总画布高度 (cm)
    /// 公式：高度 + 上下出血位 * 2
    /// </summary>
    public double GetTotalHeightCm() => Height + (BleedTopBottom * 2);
}