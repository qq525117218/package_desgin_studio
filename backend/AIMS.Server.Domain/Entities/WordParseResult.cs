namespace AIMS.Server.Domain.Entities;

/// <summary>
/// Word文档解析结果实体类
/// </summary>
public class WordParseResult
{
    /// <summary>
    /// 文档完整文本内容
    /// </summary>
    public string FullText { get; set; } = string.Empty;

    /// <summary>
    /// 文档总页数
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// 文档段落列表
    /// </summary>
    public List<string> Paragraphs { get; set; } = new();

    /// <summary>
    /// 文档元数据键值对（如作者、创建时间等）
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    /// <summary>
    /// 文档表格数据（表→行→列层级结构）
    /// </summary>
    public List<List<List<string>>> Tables { get; set; } = new();
}