using Newtonsoft.Json;

namespace AIMS.Server.Application.DTOs.Plm;

/// <summary>
/// PLM 产品基础信息传输对象
/// </summary>
public class ProductInfoDto
{
    /// <summary>
    /// 产品详情页链接
    /// </summary>
    [JsonProperty("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 产品主图 URL
    /// </summary>
    [JsonProperty("main_pic")]
    public string? MainPic { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    [JsonProperty("product_name")]
    public string? ProductName { get; set; }

    /// <summary>
    /// 品牌名称
    /// </summary>
    [JsonProperty("brand_name")]
    public string? BrandName { get; set; }

    /// <summary>
    /// 品牌编码
    /// </summary>
    [JsonProperty("brand_code")]
    public string? BrandCode { get; set; }

    /// <summary>
    /// 所属平台名称
    /// </summary>
    [JsonProperty("platform_name")]
    public string? PlatformName { get; set; }

    /// <summary>
    /// 产品状态
    /// </summary>
    [JsonProperty("status")]
    public int? Status { get; set; }
}