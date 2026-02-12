using Newtonsoft.Json;

namespace AIMS.Server.Application.DTOs.Plm;

/// <summary>
/// 品牌详情信息
/// </summary>
public class BrandDetailDto
{
    /// <summary>
    /// 品牌名称
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 品牌简称
    /// </summary>
    [JsonProperty("abbr")]
    public string? Abbr { get; set; }

    /// <summary>
    /// 品牌类目名称
    /// </summary>
    [JsonProperty("brand_category_names")]
    public string? BrandCategoryNames { get; set; }

    /// <summary>
    /// SRM 供应商 ID
    /// </summary>
    [JsonProperty("srm_company_supplier_id")]
    public int? SrmCompanySupplierId { get; set; }

    /// <summary>
    /// 供应商名称
    /// </summary>
    [JsonProperty("supplier_name")]
    public string? SupplierName { get; set; }

    /// <summary>
    /// 品牌 Logo 路径
    /// </summary>
    [JsonProperty("logo")]
    public string? Logo { get; set; }

    /// <summary>
    /// 品牌状态
    /// </summary>
    [JsonProperty("status")]
    public int? Status { get; set; }

    /// <summary>
    /// 默认制造商
    /// </summary>
    [JsonProperty("defaultManufacturer")]
    public ManufacturerDto? DefaultManufacturer { get; set; }

    /// <summary>
    /// 欧盟授权代表
    /// </summary>
    [JsonProperty("agent_ec")]
    public BrandAgentDto? AgentEc { get; set; }

    /// <summary>
    /// 英国授权代表
    /// </summary>
    [JsonProperty("agent_uk")]
    public BrandAgentDto? AgentUk { get; set; }

    /// <summary>
    /// 美国授权代表
    /// </summary>
    [JsonProperty("agent_us")]
    public BrandAgentDto? AgentUs { get; set; }

    /// <summary>
    /// 加拿大授权代表
    /// </summary>
    [JsonProperty("agent_ca")]
    public BrandAgentDto? AgentCa { get; set; }
}

/// <summary>
/// 制造商信息
/// </summary>
public class ManufacturerDto
{
    /// <summary>
    /// 制造商名称
    /// </summary>
    [JsonProperty("manufacturer_name")]
    public string? ManufacturerName { get; set; }

    /// <summary>
    /// 制造商英文名
    /// </summary>
    [JsonProperty("manufacturer_english_name")]
    public string? ManufacturerEnglishName { get; set; }

    /// <summary>
    /// 制造商地址
    /// </summary>
    [JsonProperty("manufacturer_address")]
    public string? ManufacturerAddress { get; set; }

    /// <summary>
    /// 制造商英文地址
    /// </summary>
    [JsonProperty("manufacturer_english_address")]
    public string? ManufacturerEnglishAddress { get; set; }

    /// <summary>
    /// 国家/地区
    /// </summary>
    [JsonProperty("country_region_name")]
    public string? CountryRegionName { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    [JsonProperty("contact_name")]
    public string? ContactName { get; set; }

    /// <summary>
    /// 电话号码
    /// </summary>
    [JsonProperty("phone_number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    [JsonProperty("mailbox")]
    public string? Mailbox { get; set; }
}

/// <summary>
/// 代理商信息
/// </summary>
public class BrandAgentDto
{
    /// <summary>
    /// 代理商名称
    /// </summary>
    [JsonProperty("agent_name")]
    public string? AgentName { get; set; }

    /// <summary>
    /// 代理商类型
    /// </summary>
    [JsonProperty("agent_type_name")]
    public string? AgentTypeName { get; set; }

    /// <summary>
    /// 代理商地址
    /// </summary>
    [JsonProperty("agent_address")]
    public string? AgentAddress { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    [JsonProperty("contact_name")]
    public string? ContactName { get; set; }

    /// <summary>
    /// 电话号码
    /// </summary>
    [JsonProperty("phone_number")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 电子邮箱
    /// </summary>
    [JsonProperty("mailbox")]
    public string? Mailbox { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [JsonProperty("postal_code")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// 省/州
    /// </summary>
    [JsonProperty("province")]
    public string? Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [JsonProperty("city")]
    public string? City { get; set; }
}