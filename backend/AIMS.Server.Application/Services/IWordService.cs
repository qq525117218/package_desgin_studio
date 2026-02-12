using AIMS.Server.Application.DTOs.Document;

namespace AIMS.Server.Application.Services;

/// <summary>
/// Word文档处理服务接口
/// </summary>
public interface IWordService
{
    /// <summary>
    /// 异步解析Word文档
    /// </summary>
    Task<WordParseResponseDto> ParseWordDocumentAsync(WordParseRequestDto request);
}