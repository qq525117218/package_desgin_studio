using AIMS.Server.Application.DTOs.Plm;

namespace AIMS.Server.Application.Services;

public interface IPlmApiService
{
    // 直接返回业务需要的品牌列表，把 PLM 的外壳剥离逻辑留在 Service 内部
    Task<List<BrandDto>> GetBrandListAsync();
    /// <summary>
    /// 获取产品条码
    /// </summary>
    Task<BarCodeDto> GetBarCodeAsync(string code);
    
    /// <summary>
    /// 获取品牌详情
    /// </summary>
    Task<BrandDetailDto> GetBrandDetailAsync(string code);
    
    /// <summary>
    /// 获取品牌详情
    /// </summary>
    Task<ProductInfoDto> GetProductInfoByProductCode(string code);
}