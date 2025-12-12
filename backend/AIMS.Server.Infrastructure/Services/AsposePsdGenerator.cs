using AIMS.Server.Domain.Entities;
using AIMS.Server.Domain.Interfaces;
using AIMS.Server.Infrastructure.Utils;
using Aspose.PSD;
using Aspose.PSD.FileFormats.Psd;
using Aspose.PSD.FileFormats.Psd.Layers;
using Aspose.PSD.FileFormats.Psd.Layers.FillLayers;
using Aspose.PSD.FileFormats.Psd.Layers.FillSettings;
using Aspose.PSD.FileFormats.Psd.Layers.SmartObjects;
using Aspose.PSD.ImageOptions;
using Aspose.PSD.ProgressManagement;
using System.Text;

// 显式指定 Aspose 类型以解决与 System.Drawing 的冲突
using Color = Aspose.PSD.Color;
using PointF = Aspose.PSD.PointF;
using Rectangle = Aspose.PSD.Rectangle;
using Point = Aspose.PSD.Point;

// 引入 Aspose.Pdf 别名
using AsposePdf = Aspose.Pdf; 
using AsposePdfDevices = Aspose.Pdf.Devices;

namespace AIMS.Server.Infrastructure.Services;

public class AsposePsdGenerator : IPsdGenerator
{
    private const float DPI = 300f; 
    
    // --- 固定图标尺寸 (严格执行) ---
    // 宽度固定为 4.5cm
    private const double ICON_WIDTH_CM = 4.0;
    // 高度限制为 2.5cm
    private const double ICON_HEIGHT_CM = 2.0;

    // Logo 最大限制尺寸
    private const double LOGO_MAX_WIDTH_CM = 2.5;
    private const double LOGO_MAX_HEIGHT_CM = 0.5;

    private static readonly HttpClient _httpClient = new HttpClient();
    
    static AsposePsdGenerator()
    {
        try
        {
            var fontFolders = new List<string>();
            string customFontsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts");
            if (Directory.Exists(customFontsDir)) fontFolders.Add(customFontsDir);

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                fontFolders.Add(@"C:\Windows\Fonts"); 
            else
            {
                fontFolders.Add("/usr/share/fonts");
                fontFolders.Add("/usr/local/share/fonts");
            }

            if (fontFolders.Count > 0) FontSettings.SetFontsFolders(fontFolders.ToArray(), true);
        }
        catch (Exception ex) { Console.WriteLine($"[AsposePsdGenerator] 字体配置警告: {ex.Message}"); }
    }
    
    private struct BarcodeLayout
    {
        public bool Rotate90;
        public Point Position;
        public int Width;
        public int Height;
    }

    public async Task<byte[]> GeneratePsdAsync(PackagingDimensions dim, PackagingAssets assets, Action<int, string>? onProgress = null)
    {
        return await Task.Run(async () => await GenerateInternalAsync(dim, assets, onProgress));
    }

    private async Task<byte[]> GenerateInternalAsync(PackagingDimensions dim, PackagingAssets assets, Action<int, string>? onProgress)
    {
        string? tempPsdPath = null;
        string? tempPdfPath = null;
        
        string fixedIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Templates", "FixedIcon.psd");

        try
        {
            // --- 阶段 1: 初始化 ---
            onProgress?.Invoke(1, "正在初始化画布参数...");

            var X = CmToPixels(dim.Length); 
            var Y = CmToPixels(dim.Height); 
            var Z = CmToPixels(dim.Width);  
            var A = CmToPixels(dim.BleedLeftRight); 
            var B = CmToPixels(dim.BleedTopBottom); 
            var C = CmToPixels(dim.InnerBleed);       

            var totalWidth = (2 * X) + (2 * Z) + (2 * A);
            var calculatedHeight = Y + (2 * Z) + (2 * B) - (4 * C);
            var minRequiredHeight = B + (2 * Z) + Y;
            var totalHeight = Math.Max(calculatedHeight, minRequiredHeight);

            tempPsdPath = Path.GetTempFileName(); 

            // --- 阶段 2: 生成基础 PSD ---
            onProgress?.Invoke(5, "正在创建基础图层...");
            
            using (var psdImage = new PsdImage(totalWidth, totalHeight))
            {
                psdImage.SetResolution(DPI, DPI);

                onProgress?.Invoke(10, "正在绘制刀版结构...");
                DrawStructureLayers(psdImage, X, Y, Z, A, B, C, onProgress);

                onProgress?.Invoke(40, "正在生成智能辅助线...");
                AddGuidelines(psdImage, X, Y, Z, A, B, C);

                onProgress?.Invoke(50, "正在渲染文本信息...");
                DrawInfoPanelAssets(psdImage, assets, dim);
                DrawMainPanelAssets(psdImage, assets, dim);

                onProgress?.Invoke(70, "正在保存中间状态...");
                psdImage.Save(tempPsdPath, new PsdOptions { CompressionMethod = CompressionMethod.RLE, ColorMode = ColorModes.Rgb });
            }

            // --- 阶段 3: 处理条形码 ---
            if (assets.Images?.Barcode != null && !string.IsNullOrEmpty(assets.Images.Barcode.Url))
            {
                onProgress?.Invoke(82, "正在下载条形码...");
                var pdfBytes = await _httpClient.GetByteArrayAsync(assets.Images.Barcode.Url);
                if (pdfBytes.Length > 0)
                {
                    tempPdfPath = Path.GetTempFileName();
                    await File.WriteAllBytesAsync(tempPdfPath, pdfBytes);
                    onProgress?.Invoke(85, "正在处理条形码智能对象...");
                    EmbedBarcodePdfAsSmartObject(tempPsdPath, tempPdfPath, dim);
                }
            }

            // --- 阶段 3.5: 处理动态品牌 Logo ---
            var brandName = assets.Texts?.MainPanel?.BrandName;
            if (!string.IsNullOrWhiteSpace(brandName))
            {
                var safeBrandName = string.Join("_", brandName.Split(Path.GetInvalidFileNameChars()));
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Logo", $"{safeBrandName}.psd");

                if (File.Exists(logoPath))
                {
                    onProgress?.Invoke(88, $"正在置入品牌 Logo: {safeBrandName}...");
                    EmbedLogoAsSmartObject(tempPsdPath, logoPath, dim);
                }
            }

            // --- 阶段 4: 处理固定图标 (修复重点) ---
            if (File.Exists(fixedIconPath))
            {
                onProgress?.Invoke(95, "正在置入固定图标...");
                EmbedFixedAssetAsSmartObject(tempPsdPath, fixedIconPath, dim);
            }
            else
            {
                Console.WriteLine($"[Warning] 未找到固定图标文件: {fixedIconPath}");
            }

            // --- 阶段 5: 输出 ---
            onProgress?.Invoke(99, "生成完成...");
            if (!File.Exists(tempPsdPath)) throw new FileNotFoundException("生成过程中文件丢失", tempPsdPath);

            return await File.ReadAllBytesAsync(tempPsdPath);
        }
        catch (Exception ex)
        {
            onProgress?.Invoke(0, $"生成失败: {ex.Message}");
            throw;
        }
        finally
        {
            CleanupTempFile(tempPsdPath);
            CleanupTempFile(tempPdfPath);
        }
    }

    // ====================================================================================
    // ⬇️ 修复方法：EmbedFixedAssetAsSmartObject
    // 原理：强制像素注入。不再依赖 ReplaceContents，直接把像素写死到图层里，保证尺寸绝对正确。
    // ====================================================================================

    private void EmbedFixedAssetAsSmartObject(string targetPsdPath, string assetPath, PackagingDimensions dim)
    {
        if (!File.Exists(assetPath)) return;
        if (!File.Exists(targetPsdPath)) return;

        string tempOutputPath = targetPsdPath + ".tmp";
        
        try 
        {
            using (var targetImage = (PsdImage)Aspose.PSD.Image.Load(targetPsdPath))
            using (var srcPsd = (PsdImage)Aspose.PSD.Image.Load(assetPath))
            {
                // 1. 强制计算 300 DPI 下的目标像素值
                // 4.5cm * 300 / 2.54 = ~531 px
                // 2.5cm * 300 / 2.54 = ~295 px
                int targetWidthPx = CmToPixels(ICON_WIDTH_CM);
                int targetHeightPx = CmToPixels(ICON_HEIGHT_CM);

                // 2. 计算缩放比例 (保持原图比例，fit inside 4.5x2.5)
                double ratioX = (double)targetWidthPx / srcPsd.Width;
                double ratioY = (double)targetHeightPx / srcPsd.Height;
                // 使用较小的比例，确保完全放入框内。如果需要填满且允许裁剪，可用 Math.Max
                double scale = Math.Min(ratioX, ratioY);

                // 算出最终的实际像素尺寸
                int finalWidth = (int)Math.Round(srcPsd.Width * scale);
                int finalHeight = (int)Math.Round(srcPsd.Height * scale);

                // 3. 强制重采样源图片 (在内存中把图片拉大/缩小到这个像素值)
                if (srcPsd.Width != finalWidth || srcPsd.Height != finalHeight)
                {
                    srcPsd.Resize(finalWidth, finalHeight, ResizeType.LanczosResample);
                }

                // 4. 计算定位
                var pos = CalculateFixedIconPosition(targetImage, dim, finalWidth, finalHeight);
                
                // 5. 创建图层并直接写入像素 (Pixel Injection)
                // 这一步绕过了 SmartObject ReplaceContents 带来的 DPI 缩放歧义
                var fixedIconLayer = targetImage.AddRegularLayer();
                fixedIconLayer.DisplayName = "FixedIcon_PixelData";
                fixedIconLayer.Left = pos.X;
                fixedIconLayer.Top = pos.Y;
                fixedIconLayer.Right = pos.X + finalWidth;
                fixedIconLayer.Bottom = pos.Y + finalHeight;

                // 提取像素并写入
                var pixels = srcPsd.LoadArgb32Pixels(srcPsd.Bounds);
                fixedIconLayer.SaveArgb32Pixels(new Rectangle(0, 0, finalWidth, finalHeight), pixels);

                // 6. 将这个已经是正确尺寸的图层转为智能对象
                var smartLayer = targetImage.SmartObjectProvider.ConvertToSmartObject(new[] { fixedIconLayer });
                smartLayer.DisplayName = "FixedIcon";
                // 此时 smartLayer 的尺寸就是 finalWidth x finalHeight，即约 4.5cm 宽

                targetImage.Save(tempOutputPath, new PsdOptions { CompressionMethod = CompressionMethod.RLE });
            }

            if (File.Exists(targetPsdPath)) File.Delete(targetPsdPath);
            File.Move(tempOutputPath, targetPsdPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] 置入固定图标失败: {ex.Message}");
            // 出错时不覆盖原文件，保持部分成功的结果
        }
    }

    // ====================================================================================
    // ⬇️ 其他核心逻辑 (保持不变)
    // ====================================================================================

    private void EmbedBarcodePdfAsSmartObject(string targetPsdPath, string pdfPath, PackagingDimensions dim)
    {
        if (!File.Exists(pdfPath) || !File.Exists(targetPsdPath)) return;

        string tempOutputPath = targetPsdPath + ".tmp";
        try
        {
            var pdfDoc = new Aspose.Words.Document(pdfPath);
            var pageInfo = pdfDoc.GetPageInfo(0);
            
            using (var targetImage = (PsdImage)Aspose.PSD.Image.Load(targetPsdPath))
            {
                float targetDpiX = (float)targetImage.HorizontalResolution;
                float targetDpiY = (float)targetImage.VerticalResolution;

                var layout = CalculateBarcodeLayout(dim, pageInfo.WidthInPoints, pageInfo.HeightInPoints, targetDpiX, targetDpiY);

                var saveOptions = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png)
                {
                    PageSet = new Aspose.Words.Saving.PageSet(0), 
                    Resolution = targetDpiX,                        
                    UseHighQualityRendering = true,
                    PaperColor = System.Drawing.Color.Transparent 
                };

                using var pageImageStream = new MemoryStream();
                pdfDoc.Save(pageImageStream, saveOptions);
                pageImageStream.Position = 0;

                using (var loadedImage = (RasterImage)Aspose.PSD.Image.Load(pageImageStream))
                {
                    if (layout.Rotate90) loadedImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    if (loadedImage.Width != layout.Width || loadedImage.Height != layout.Height)
                        loadedImage.Resize(layout.Width, layout.Height, ResizeType.LanczosResample);

                    var barcodeLayer = targetImage.AddRegularLayer();
                    barcodeLayer.Left = layout.Position.X;
                    barcodeLayer.Top = layout.Position.Y;
                    barcodeLayer.Right = layout.Position.X + layout.Width;
                    barcodeLayer.Bottom = layout.Position.Y + layout.Height;
                    barcodeLayer.SaveArgb32Pixels(new Rectangle(0, 0, layout.Width, layout.Height), loadedImage.LoadArgb32Pixels(loadedImage.Bounds));

                    var smartLayer = targetImage.SmartObjectProvider.ConvertToSmartObject(new[] { barcodeLayer });
                    smartLayer.DisplayName = "BARCODE"; 
                    smartLayer.Left = layout.Position.X;
                    smartLayer.Top = layout.Position.Y;
                }
                targetImage.Save(tempOutputPath, new PsdOptions { CompressionMethod = CompressionMethod.RLE });
            } 
            if (File.Exists(targetPsdPath)) File.Delete(targetPsdPath);
            File.Move(tempOutputPath, targetPsdPath);
        }
        catch (Exception ex) { Console.WriteLine($"[Warning] 条形码渲染失败: {ex.Message}"); }
    }

    private BarcodeLayout CalculateBarcodeLayout(PackagingDimensions dim, double origW_Pt, double origH_Pt, float dpiX, float dpiY)
    {
        var layout = new BarcodeLayout();
        double ratio = (double)dim.Height / dim.Width;
        layout.Rotate90 = ratio >= 2.0; 

        int desiredWidthPx = CmToPixels(4.0); 
        double rawPxW = (origW_Pt * dpiX) / 72.0;
        double rawPxH = (origH_Pt * dpiY) / 72.0;
        double scale = (double)desiredWidthPx / rawPxW;

        int scaledW = desiredWidthPx;                
        int scaledH = (int)Math.Round(rawPxH * scale); 

        if (layout.Rotate90) { layout.Width = scaledH; layout.Height = scaledW; }
        else { layout.Width = scaledW; layout.Height = scaledH; }

        var X_px = CmToPixels(dim.Length);
        var Y_px = CmToPixels(dim.Height);
        var Z_px = CmToPixels(dim.Width);
        var A_px = CmToPixels(dim.BleedLeftRight);
        var B_px = CmToPixels(dim.BleedTopBottom);

        int panelStartX = A_px + Z_px + X_px;        
        int panelStartY = B_px + Z_px;               
        int panelEndY = panelStartY + Y_px;          

        int sideMarginPx = CmToPixels(0.5); 
        int panelEndX = panelStartX + Z_px;
        int destX = panelEndX - sideMarginPx - layout.Width;

        int bottomMarginPx = CmToPixels(0.8); 
        int destY = panelEndY - bottomMarginPx - layout.Height;

        layout.Position = new Point(destX, destY);
        return layout;
    }

    private void EmbedLogoAsSmartObject(string targetPsdPath, string assetPath, PackagingDimensions dim)
    {
        // 这里的逻辑也建议使用 Pixel Injection 模式以确保万无一失，但为了保持代码改动最小化，
        // 且 Logo 问题未被报告，暂保持原样。如果 Logo 也变小了，请使用 EmbedFixedAssetAsSmartObject 相同的方法。
        if (!File.Exists(assetPath)) return;
        string tempOutputPath = targetPsdPath + ".tmp";
        using (var targetImage = (PsdImage)Aspose.PSD.Image.Load(targetPsdPath))
        using (var srcPsd = (PsdImage)Aspose.PSD.Image.Load(assetPath))
        {
            float targetDpiX = (float)targetImage.HorizontalResolution;
            const double cmToInch = 1.0 / 2.54;
            int maxBoxWidth = Math.Max(1, (int)Math.Round(LOGO_MAX_WIDTH_CM * cmToInch * targetDpiX));
            double scale = (double)maxBoxWidth / srcPsd.Width;
            
            // 简单限制高度
            if (srcPsd.Height * scale > CmToPixels(LOGO_MAX_HEIGHT_CM)) 
                scale = (double)CmToPixels(LOGO_MAX_HEIGHT_CM) / srcPsd.Height;

            int newWidth = (int)Math.Round(srcPsd.Width * scale);
            int newHeight = (int)Math.Round(srcPsd.Height * scale);

            if (srcPsd.Width != newWidth || srcPsd.Height != newHeight)
                srcPsd.Resize(newWidth, newHeight, ResizeType.LanczosResample);

            var pos = CalculateLogoPosition(targetImage, dim, newWidth, newHeight);
            
            var logoLayer = targetImage.AddRegularLayer();
            logoLayer.Left = pos.X;
            logoLayer.Top = pos.Y;
            logoLayer.Right = pos.X + newWidth;
            logoLayer.Bottom = pos.Y + newHeight;
            logoLayer.SaveArgb32Pixels(new Rectangle(0, 0, newWidth, newHeight), srcPsd.LoadArgb32Pixels(srcPsd.Bounds));

            var smartLayer = targetImage.SmartObjectProvider.ConvertToSmartObject(new[] { logoLayer });
            smartLayer.DisplayName = "BrandLogo";
            
            targetImage.Save(tempOutputPath, new PsdOptions { CompressionMethod = CompressionMethod.RLE });
        }
        if (File.Exists(targetPsdPath)) File.Delete(targetPsdPath);
        File.Move(tempOutputPath, targetPsdPath);
    }

    private Point CalculateLogoPosition(PsdImage psdImage, PackagingDimensions dim, int widthPx, int heightPx)
    {
        var X = CmToPixels(dim.Length);
        var Y = CmToPixels(dim.Height);
        var Z = CmToPixels(dim.Width);
        var A = CmToPixels(dim.BleedLeftRight);
        var B = CmToPixels(dim.BleedTopBottom);
        int panelLeft = A + Z;
        int centerX = panelLeft + (X / 2);
        int destX = centerX - (widthPx / 2) ;
        int topFoldY = B + Z;
        int marginTop = (int)(Y * 0.12);
        int destY = topFoldY + marginTop;
        return new Point(destX, destY);
    }

    private Point CalculateFixedIconPosition(PsdImage psdImage, PackagingDimensions dim, int widthPx, int heightPx)
    {
        var X = CmToPixels(dim.Length);
        var Y = CmToPixels(dim.Height);
        var Z = CmToPixels(dim.Width);
        var A = CmToPixels(dim.BleedLeftRight);
        var B = CmToPixels(dim.BleedTopBottom);

        // 背面面板 (Back Panel)
        // 位置: A (粘口) + Z (左侧) + X (正面) + Z (右侧) = 背面起始
        // 注意：这里的顺序取决于你的盒型结构，通常顺序是: 粘口-左-正-右-背
        // 代码中你原本用的是 A + 2*Z + X，这对应的是第4个面
        int panelLeft = A + (2 * Z) + X;
        int panelWidth = X;

        int margin = CmToPixels(0.5);
        int destX = (panelLeft + panelWidth) - widthPx - margin; // 右对齐

        int bottomFoldY = B + Z + Y;
        int destY = bottomFoldY - heightPx - margin; // 底部对齐

        return new Point(destX, destY);
    }

    private void DrawStructureLayers(PsdImage psdImage, int X, int Y, int Z, int A, int B, int C, Action<int, string>? onProgress)
    {
        // ... (保持原样) ...
        CreateShapeLayer(psdImage, "BG", (2 * X) + (2 * Z) + (2 * A), Y + (4 * C), 0, B + Z - (2 * C), Color.White);
        CreateShapeLayer(psdImage, "left", A + X, Y + (4 * C), 0, B + Z - (2 * C), Color.White);
        CreateShapeLayer(psdImage, "front", X, Y + (4 * C), A + Z, B + Z - (2 * C), Color.White);
        CreateShapeLayer(psdImage, "right", X, Y + (4 * C), A + Z + X, B + Z - (2 * C), Color.White);
        CreateShapeLayer(psdImage, "back", A + X, Y + (4 * C), A + (2 * Z) + X, B + Z - (2 * C), Color.White);
        CreateShapeLayer(psdImage, "top", X, Z, A + Z, B, Color.White);
        CreateShapeLayer(psdImage, "bottom", X, Z, A + (2 * Z) + X, B + Z + Y, Color.White);
    }

    private void DrawInfoPanelAssets(PsdImage psdImage, PackagingAssets assets, PackagingDimensions dim)
    {
        // ... (保持原样，省略以节省空间，确保使用上面的完整逻辑) ...
        // 请保留原方法内容
        var info = assets.Texts.InfoPanel;
        var main = assets.Texts.MainPanel;
        if (info == null && main == null) return;
        var X = CmToPixels(dim.Length);
        var Z = CmToPixels(dim.Width);
        var A = CmToPixels(dim.BleedLeftRight);
        var B = CmToPixels(dim.BleedTopBottom);
        int topFoldY = B + Z;
        int safeTopMargin = 80;
        int backStartX = A + (2 * Z) + X;
        int backStartY = topFoldY + safeTopMargin;
        int backPanelWidth = A + X;
        int padding = 30;
        int currentY = backStartY;
        int textAreaWidth = backPanelWidth - (2 * padding);
        float fontSize = 6f;

        if (info != null)
        {
            CreateRichTextLayer(psdImage, "PRODUCT_NAME_TXT", "PRODUCT NAME:", info.ProductName, new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
            currentY += 60;
            CreateRichTextLayer(psdImage, "INGREDIENTS_TXT", "INGREDIENTS:", info.Ingredients, new Rectangle(backStartX + padding, currentY, textAreaWidth, 100), fontSize);
            currentY += 110;
            CreateRichTextLayer(psdImage, "WARNINGS_TXT", "WARNINGS:", info.Warnings, new Rectangle(backStartX + padding, currentY, textAreaWidth, 80), fontSize);
            currentY += 90;
            if (main != null) {
                CreateRichTextLayer(psdImage, "MANUFACTURER_TXT", "MANUFACTURER:", main.Manufacturer, new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
                currentY += 60;
                CreateRichTextLayer(psdImage, "MANUFACTURER_ADD_TXT", "ADDRESS:", main.Address, new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
                currentY += 60;
            }
            CreateRichTextLayer(psdImage, "SHELF_LIFE_TXT", "SHELF LIFE:", info.ShelfLife, new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
            currentY += 60;
            CreateRichTextLayer(psdImage, "MADE_IN _TXT", "MADE IN:", info.Origin, new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
            currentY += 60;
            if (main != null && !string.IsNullOrWhiteSpace(main.CapacityInfoBack))
                CreateRichTextLayer(psdImage, "NET_BACK_TXT", "", main.CapacityInfoBack, new Rectangle(backStartX + padding, currentY, textAreaWidth, 60), 6f, true);
        }
        int rightStartX = A + Z + X;
        int rightStartY = topFoldY + safeTopMargin;
        int rightPanelWidth = Z;
        int rightCurrentY = rightStartY;
        int rightTextAreaWidth = rightPanelWidth - (2 * padding);
        if (info != null) {
            CreateRichTextLayer(psdImage, "SAFEUSE_TXT", "DIRECTIONS:", info.Directions, new Rectangle(rightStartX + padding, rightCurrentY, rightTextAreaWidth, 150), fontSize);
            rightCurrentY += 160;
            CreateRichTextLayer(psdImage, "FUNCTIONS&BENEFITS_TXT", "BENEFITS:", info.Benefits, new Rectangle(rightStartX + padding, rightCurrentY, rightTextAreaWidth, 150), fontSize);
        }
    }

    private void DrawMainPanelAssets(PsdImage psdImage, PackagingAssets assets, PackagingDimensions dim)
    {
        // ... (保持原样) ...
        var main = assets.Texts.MainPanel;
        var info = assets.Texts.InfoPanel;
        if (main == null) return;
        var X = CmToPixels(dim.Length);
        var Y = CmToPixels(dim.Height);
        var Z = CmToPixels(dim.Width);
        var A = CmToPixels(dim.BleedLeftRight);
        var B = CmToPixels(dim.BleedTopBottom);
        int panelStartX = A + Z;
        int topFoldY = B + Z;
        int topPadding = 80;
        int contentWidth = X - (2 * 40);
        int contentStartX = panelStartX + 40;
        if (!string.IsNullOrWhiteSpace(info.ProductName)) {
            CreateRichTextLayer(psdImage, "PRODUCT_NAME_FRONT_TXT", "", info.ProductName, new Rectangle(contentStartX, topFoldY + topPadding, contentWidth, 80), 12f);
        }
        if (main.SellingPoints != null && main.SellingPoints.Count > 0) {
            int startY = topFoldY + (int)(Y * 0.35);
            int lineHeight = 60;
            for (int i = 0; i < main.SellingPoints.Count; i++) {
                if (string.IsNullOrWhiteSpace(main.SellingPoints[i])) continue;
                CreateRichTextLayer(psdImage, $"SELLINGPOINT#{i + 1}_TXT", "", "• " + main.SellingPoints[i], new Rectangle(contentStartX, startY + (i * lineHeight), contentWidth, lineHeight), 8f);
            }
        }
        if (!string.IsNullOrWhiteSpace(main.CapacityInfo)) {
            int bottomFoldY = B + Z + Y;
            CreateRichTextLayer(psdImage, "NET_FRONT_TXT", "", main.CapacityInfo, new Rectangle(contentStartX, bottomFoldY - 40 - 100, contentWidth, 100), 10f);
        }
    }

    private void AddGuidelines(PsdImage psdImage, int X, int Y, int Z, int A, int B, int C)
    {
        // ... (保持原样) ...
        var horizontalWidth = 2 * X + 2 * Z + 2 * A;
        var verticalHeight = Y + 2 * Z + 2 * B - 4 * C;
        var topIndex = psdImage.Layers.Length;
        var lineGroup = psdImage.AddLayerGroup("GuidelineGroup", topIndex, false);
        CreateGuidelineLayer(psdImage, "guideline_Y001", 0, B, horizontalWidth, 1, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_Y002", 0, B + Z - 2 * C, horizontalWidth, 1, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_Y003", 0, B + Z, horizontalWidth, 1, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_Y004", 0, B + Z + Y, horizontalWidth, 1, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_Y005", 0, B + Z + Y + 2 * C, horizontalWidth, 1, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_Y006", 0, B + Z + Y + Z, horizontalWidth, 1, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_X001", A, 0, 1, verticalHeight, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_X002", A + Z, 0, 1, verticalHeight, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_X003", A + Z + X, 0, 1, verticalHeight, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_X004", A + 2 * Z + X, 0, 1, verticalHeight, lineGroup);
        CreateGuidelineLayer(psdImage, "guideline_X006", A + 2 * Z + 2 * X, 0, 1, verticalHeight, lineGroup);
    }

    private void CreateGuidelineLayer(PsdImage psdImage, string layerName, int x, int y, int width, int height, LayerGroup lineGroup)
    {
        CreateWhiteRectangleLineLayer(psdImage, layerName, new Rectangle(x, y, width, height), Color.FromArgb(255, 0, 108), lineGroup);
    }

    private void CreateWhiteRectangleLineLayer(PsdImage psdImage, string layerName, Rectangle rect, Color layerColor, LayerGroup lineGroup)
    {
        if (psdImage == null || rect.Width <= 0 || rect.Height <= 0) return;
        try {
            var layer = FillLayer.CreateInstance(FillType.Color);
            layer.DisplayName = layerName;
            lineGroup.AddLayer(layer);
            var vectorPath = VectorDataProvider.CreateVectorPathForLayer(layer);
            vectorPath.FillColor = layerColor;
            var shape = new PathShapeGen();
            shape.Points.Add(new BezierKnot(new PointF(rect.X, rect.Y), true));
            shape.Points.Add(new BezierKnot(new PointF(rect.X + rect.Width, rect.Y), true));
            shape.Points.Add(new BezierKnot(new PointF(rect.X + rect.Width, rect.Y + rect.Height), true));
            shape.Points.Add(new BezierKnot(new PointF(rect.X, rect.Y + rect.Height), true));
            vectorPath.Shapes.Add(shape);
            VectorDataProvider.UpdateLayerFromVectorPath(layer, vectorPath, true);
            layer.AddLayerMask(null);
        } catch {}
    }

    private void CreateShapeLayer(PsdImage psdImage, string layerName, int width, int height, int x, int y, Color layerColor)
    {
        if (psdImage == null || width <= 0 || height <= 0) return;
        try {
            var layer = FillLayer.CreateInstance(FillType.Color);
            layer.DisplayName = layerName;
            psdImage.AddLayer(layer);
            var vectorPath = VectorDataProvider.CreateVectorPathForLayer(layer);
            vectorPath.FillColor = layerColor;
            var shape = new PathShapeGen();
            shape.Points.Add(new BezierKnot(new PointF(x, y), true));
            shape.Points.Add(new BezierKnot(new PointF(x + width, y), true));
            shape.Points.Add(new BezierKnot(new PointF(x + width, y + height), true));
            shape.Points.Add(new BezierKnot(new PointF(x, y + height), true));
            vectorPath.Shapes.Add(shape);
            VectorDataProvider.UpdateLayerFromVectorPath(layer, vectorPath, true);
            layer.AddLayerMask(null);
        } catch {}
    }

    private void CreateRichTextLayer(PsdImage psdImage, string layerName, string label, string content, Rectangle rect, float fontSizePt, bool contentBold = false)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        try {
            string safeLabel = ToHalfWidth(label);
            string safeContent = ToHalfWidth(content);
            var textLayer = psdImage.AddTextLayer(layerName, rect);
            var textData = textLayer.TextData;
            float fontSizePixels = PtToPixels(fontSizePt);
            string fontName = FontSettings.GetAdobeFontName("Arial") ?? "Arial";
            if (textData.Items.Length > 0) {
                textData.Items[0].Style.FontSize = fontSizePixels;
                textData.Items[0].Paragraph.Justification = JustificationMode.Left;
                if (!string.IsNullOrEmpty(safeLabel)) {
                    var labelPortion = textData.Items[0];
                    labelPortion.Text = safeLabel + " ";
                    labelPortion.Style.FauxBold = true;
                    labelPortion.Style.FillColor = Color.Black;
                    labelPortion.Style.FontName = fontName;
                    var contentPortion = textData.ProducePortion();
                    contentPortion.Text = safeContent;
                    contentPortion.Style.FontSize = fontSizePixels;
                    contentPortion.Style.FauxBold = contentBold;
                    contentPortion.Style.FillColor = Color.Black;
                    contentPortion.Style.FontName = fontName;
                    textData.AddPortion(contentPortion);
                } else {
                    var portion = textData.Items[0];
                    portion.Text = safeContent;
                    portion.Style.FontName = fontName;
                    portion.Style.FontSize = fontSizePixels;
                    portion.Style.FauxBold = contentBold;
                    portion.Style.FillColor = Color.Black;
                }
                textData.UpdateLayerData();
            }
        } catch (Exception ex) { Console.WriteLine($"[Error] 文本层失败: {ex.Message}"); }
    }

    private string ToHalfWidth(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        StringBuilder sb = new StringBuilder(input.Length);
        foreach (char c in input) {
            char mappedChar = c switch { '、' => ',', '，' => ',', '。' => '.', '：' => ':', '；' => ';', '【' => '[', '】' => ']', '「' => '[', '」' => ']', '『' => '[', '』' => ']', '《' => '<', '》' => '>', '（' => '(', '）' => ')', '～' => '~', '〜' => '~', '—' => '-', '–' => '-', '·' => '.', '“' => '"', '”' => '"', '‘' => '\'', '’' => '\'', (char)12288 => ' ', _ => c };
            sb.Append(mappedChar);
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormKC);
    }

    private int CmToPixels(double cm) => (int)Math.Round((cm / 2.54) * DPI);
    private float PtToPixels(float pt) => (pt * DPI) / 72f;
    private void CleanupTempFile(string? path) { if (!string.IsNullOrEmpty(path) && File.Exists(path)) try { File.Delete(path); } catch {} }
}