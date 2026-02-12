using System;
using System.Threading.Tasks;
using Aspose.Words;
using Aspose.PSD;
using AIMS.Server.Infrastructure.Services; // 引用 Generator 所在的命名空间
using AIMS.Server.Infrastructure.Utils;

namespace AIMS.Server.Infrastructure.Services;

public static class AsposePreheater
{
    /// <summary>
    /// 执行 Aspose 核心库的预热（加载 DLL、校验 License、构建字体缓存）
    /// </summary>
    public static async Task PreloadAsync()
    {
        Console.WriteLine("[AsposePreheater] 开始预热 Aspose 引擎...");
        
        await Task.Run(() =>
        {
            try
            {
                // 1. 强制触发 AsposePsdGenerator 的静态构造函数
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(AsposePsdGenerator).TypeHandle);
                Console.WriteLine("[AsposePreheater] 字体配置与缓存构建完成");

                // 2. 预热 Aspose.Words (排版引擎)
                var doc = new Document();
                doc.Range.Replace("warmup", "start"); 
                Console.WriteLine("[AsposePreheater] Aspose.Words 引擎就绪");

                // 3. 预热 Aspose.PSD (图形引擎)
                using (var psd = new Aspose.PSD.FileFormats.Psd.PsdImage(10, 10))
                {
                    var graphics = new Aspose.PSD.Graphics(psd);
                    graphics.Clear(Aspose.PSD.Color.White); // 触发绘图指令
                    var w = psd.Width;
                }
                Console.WriteLine("[AsposePreheater] Aspose.PSD 图形引擎就绪");
                
                Console.WriteLine("[AsposePreheater] ✅ 所有预热任务成功完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AsposePreheater] ⚠️ 警告: 预热过程中发生错误 (服务仍将启动): {ex.Message}");
            }
        });
    }
}