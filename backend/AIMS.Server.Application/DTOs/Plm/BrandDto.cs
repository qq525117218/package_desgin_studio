using Newtonsoft.Json;

namespace AIMS.Server.Application.DTOs.Plm;

/// <summary>
/// PLM 品牌信息传输对象
/// </summary>
public class BrandDto
{
    /// <summary>
    /// 品牌内部 ID
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// 品牌编码
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 品牌名称
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 品牌简称
    /// </summary>
    [JsonProperty("abbr")]
    public string Abbr { get; set; } = string.Empty;

    /// <summary>
    /// 品牌所属类目
    /// </summary>
    [JsonProperty("brand_category_name")]
    public string BrandCategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 所属部门
    /// </summary>
    [JsonProperty("departmentname")]
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// 品牌状态
    /// </summary>
    [JsonProperty("status")]
    public int Status { get; set; }

    /// <summary>
    /// 删除标识
    /// </summary>
    [JsonProperty("is_deleted")]
    public int IsDeleted { get; set; }
}