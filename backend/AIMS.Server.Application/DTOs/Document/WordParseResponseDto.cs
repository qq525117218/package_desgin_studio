namespace AIMS.Server.Application.DTOs.Document;

/// <summary>
/// Word 文档解析响应传输对象
/// </summary>
public class WordParseResponseDto
{
    /// <summary>
    /// 文档元数据
    /// </summary>
    public DocumentMetadataDto Meta { get; set; } = new();

    /// <summary>
    /// 解析后的结构化产品内容
    /// </summary>
    public ProductContentDto Content { get; set; } = new();
    
    /// <summary>
    /// 原始表格数据 (三维数组)
    /// </summary>
    public List<List<List<string>>> RawTables { get; set; } = new();
}