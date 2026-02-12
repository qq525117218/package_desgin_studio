using System.Reflection;
using AIMS.Server.Application.Services;
using AIMS.Server.Domain.Interfaces;
using AIMS.Server.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIMS.Server.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    /// <summary>
    /// 注册基础设施层的服务（包括 Aspose License 初始化）
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 1. 注册 Aspose 服务实现
        services.AddScoped<IPsdGenerator, AsposePsdGenerator>();
        services.AddScoped<IWordParser, AsposeWordParser>();
        services.AddScoped<IWordService, WordService>();

        // 2. 初始化 License 
        InitAsposeLicense();

        return services;
    }

    private static void InitAsposeLicense()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // 资源名称规则：默认命名空间.文件夹名.文件名
            var resourceName = "AIMS.Server.Infrastructure.Licenses.Aspose.Total.NET.lic";

            // 1. 获取 License 资源流
            Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
            
            if (resourceStream == null)
            {
                var allResources = assembly.GetManifestResourceNames();
                var foundName = allResources.FirstOrDefault(x => x.EndsWith("Aspose.Total.NET.lic"));
                if (foundName != null)
                {
                    resourceName = foundName;
                    resourceStream = assembly.GetManifestResourceStream(foundName);
                    Console.WriteLine($"[System] 自动定位到 License 资源: {resourceName}");
                }
            }

            if (resourceStream == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("==================================================================");
                Console.WriteLine("[Warning] 未找到 Aspose License 嵌入资源！");
                Console.WriteLine("          Aspose 所有组件将以【评估模式】运行 (会有水印/红色文字)。");
                Console.WriteLine("==================================================================");
                Console.ResetColor();
                return;
            }

            // 2. 关键步骤：将资源流复制到 MemoryStream
            // 这样可以重复读取同一个流给不同的组件使用 (PSD, Words, PDF...)
            using (var ms = new MemoryStream())
            {
                resourceStream.CopyTo(ms);
                resourceStream.Dispose(); // 复制完成后释放原始资源流

                Console.WriteLine("----------- Aspose License Status -----------");

                // --- 初始化 Aspose.PSD ---
                try
                {
                    ms.Position = 0; // 重置流位置
                    var psdLic = new Aspose.PSD.License();
                    psdLic.SetLicense(ms);
                    Console.WriteLine(" [PSD]   License: ✅ Success");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" [PSD]   License: ❌ Failed ({ex.Message})");
                    Console.ResetColor();
                }

                // --- 初始化 Aspose.Words ---
                try
                {
                    ms.Position = 0; // 重置流位置
                    var wordsLic = new Aspose.Words.License();
                    wordsLic.SetLicense(ms);
                    Console.WriteLine(" [Words] License: ✅ Success");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" [Words] License: ❌ Failed ({ex.Message})");
                    Console.ResetColor();
                }

                // --- 初始化 Aspose.PDF ---
                try
                {
                    ms.Position = 0; // 重置流位置
                    var pdfLic = new Aspose.Pdf.License();
                    pdfLic.SetLicense(ms);
                    Console.WriteLine(" [PDF]   License: ✅ Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [PDF]   License: ⚠️ Skipped or Failed ({ex.Message})");
                }
                
                Console.WriteLine("---------------------------------------------");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Fatal Error] Aspose License 初始化过程发生严重异常: {ex.Message}");
            Console.ResetColor();
        }
    }
}