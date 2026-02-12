namespace AIMS.Server.Application.DTOs.Document;

/// <summary>
/// 文档元数据传输对象
/// </summary>
/// <remarks>
/// 包含解析文档的基础属性信息。
/// </remarks>
public class DocumentMetadataDto
{
    /// <summary>
    /// 原始文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文档总页数
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// 文档作者
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 文档标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
}