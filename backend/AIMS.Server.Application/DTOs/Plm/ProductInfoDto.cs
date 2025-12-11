using Newtonsoft.Json;

public class ProductInfoDto
{
    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("main_pic")]
    public string? MainPic { get; set; }

    [JsonProperty("product_name")]
    public string? ProductName { get; set; }

    // ✅ [修正] JSON返回的是 "PMOOX" (字符串)，这里必须是 string，原先的 int? 会报错
    [JsonProperty("brand_name")]
    public string? BrandName { get; set; }

    [JsonProperty("brand_code")]
    public string? BrandCode { get; set; }

    [JsonProperty("platform_name")]
    public string? PlatformName { get; set; }

    // JSON 中未返回 status 字段，保持 nullable 即可，反序列化时会自动为 null
    [JsonProperty("status")]
    public int? Status { get; set; }
}