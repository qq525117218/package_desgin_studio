using System.ComponentModel.DataAnnotations;

namespace AIMS.Server.Application.DTOs;

/// <summary>
/// 用户登录请求传输对象
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户账号
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}