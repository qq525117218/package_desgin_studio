using System.ComponentModel.DataAnnotations;

namespace AIMS.Server.Application.DTOs.Document;

/// <summary>
/// Word 文档解析请求传输对象
/// </summary>
public class WordParseRequestDto
{
    /// <summary>
    /// 原始文件名
    /// </summary>
    [Required(ErrorMessage = "文件名不能为空")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件的 Base64 编码内容
    /// </summary>
    [Required(ErrorMessage = "文件内容不能为空")]
    public string FileContentBase64 { get; set; } = string.Empty;
}