namespace AIMS.Server.Application.DTOs.Psd;

/// <summary>
/// 印刷出血位与边距配置传输对象
/// </summary>
public class PrintConfigDto
{
    /// <summary>
    /// 横向出血位 (cm)
    /// </summary>
    public double BleedX { get; set; } = 0.3;

    /// <summary>
    /// 纵向出血位 (cm)
    /// </summary>
    public double BleedY { get; set; } = 0.3;

    /// <summary>
    /// 内部出血位/安全边距 (cm)
    /// </summary>
    public double BleedInner { get; set; } = 0.2;
    public int ResolutionDpi { get; set; } = 300;
}