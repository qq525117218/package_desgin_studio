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
    
    // --- 固定图标尺寸 ---
    private const double ICON_WIDTH_CM = 10;
    private const double ICON_HEIGHT_CM = 5;

    // Logo 最大限制尺寸
    private const double LOGO_MAX_WIDTH_CM = 8.0;
    private const double LOGO_MAX_HEIGHT_CM = 4.0;

    private static readonly HttpClient _httpClient = new HttpClient();
    
    // 内部结构用于传递计算后的布局信息
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
            // --- 阶段 1: 初始化与基础绘制 ---
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
                var saveOptions = new PsdOptions
                {
                    CompressionMethod = CompressionMethod.RLE,
                    ColorMode = ColorModes.Rgb
                };
                psdImage.Save(tempPsdPath, saveOptions);
            }

            // --- 阶段 3: 处理条形码 ---
            if (assets.Images?.Barcode != null && !string.IsNullOrEmpty(assets.Images.Barcode.Url))
            {
                onProgress?.Invoke(72, "正在下载条形码...");
                
                var pdfBytes = await _httpClient.GetByteArrayAsync(assets.Images.Barcode.Url);
                if (pdfBytes.Length > 0)
                {
                    tempPdfPath = Path.GetTempFileName();
                    await File.WriteAllBytesAsync(tempPdfPath, pdfBytes);

                    onProgress?.Invoke(75, "正在处理条形码智能对象...");
                    // 调用核心处理方法
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
                    onProgress?.Invoke(78, $"正在置入品牌 Logo: {safeBrandName}...");
                    EmbedLogoAsSmartObject(tempPsdPath, logoPath, dim);
                }
                else
                {
                    Console.WriteLine($"[Warning] 未找到品牌 Logo 文件: {logoPath}，跳过置入。");
                }
            }

            // --- 阶段 4: 处理固定图标 ---
            if (File.Exists(fixedIconPath))
            {
                onProgress?.Invoke(82, "正在置入固定图标...");
                EmbedFixedAssetAsSmartObject(tempPsdPath, fixedIconPath, dim);
            }
            else
            {
                Console.WriteLine($"[Warning] 未找到固定图标文件: {fixedIconPath}");
            }

            // --- 阶段 5: 读取最终文件并返回 ---
            onProgress?.Invoke(90, "生成完成，准备输出...");
            
            if (!File.Exists(tempPsdPath))
                throw new FileNotFoundException("生成过程中文件丢失", tempPsdPath);

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
    // ⬇️ 核心条码处理 (v7.4 - 修正旋转判断逻辑: 高度 > 2倍宽度 才旋转)
    // ====================================================================================

    private void EmbedBarcodePdfAsSmartObject(string targetPsdPath, string pdfPath, PackagingDimensions dim)
    {
        if (!File.Exists(pdfPath) || !File.Exists(targetPsdPath)) return;

        string tempOutputPath = targetPsdPath + ".tmp";

        try
        {
            // 1. 获取 PDF 信息
            var pdfDoc = new Aspose.Words.Document(pdfPath);
            var pageInfo = pdfDoc.GetPageInfo(0);
            
            double originalWidthPt = pageInfo.WidthInPoints;
            double originalHeightPt = pageInfo.HeightInPoints;

            using (var targetImage = (PsdImage)Aspose.PSD.Image.Load(targetPsdPath))
            {
                float targetDpiX = (float)targetImage.HorizontalResolution;
                float targetDpiY = (float)targetImage.VerticalResolution;

                // 2. 计算布局 (判定旋转 + 4cm定宽)
                var layout = CalculateBarcodeLayout(dim, originalWidthPt, originalHeightPt, targetDpiX, targetDpiY);

                // 3. 将 PDF 渲染为图像
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

                // 4. 加载为 PSD RasterImage
                using (var loadedImage = (RasterImage)Aspose.PSD.Image.Load(pageImageStream))
                {
                    // 4.1 根据判定结果应用旋转
                    if (layout.Rotate90)
                    {
                        loadedImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }

                    // 4.2 调整大小 (Resizing)
                    if (loadedImage.Width != layout.Width || loadedImage.Height != layout.Height)
                    {
                        loadedImage.Resize(layout.Width, layout.Height, ResizeType.LanczosResample);
                    }

                    // 5. 直接创建图层并写入像素
                    var barcodeLayer = targetImage.AddRegularLayer();
                    barcodeLayer.DisplayName = "BARCODE_TEMP"; 
                    
                    barcodeLayer.Left = layout.Position.X;
                    barcodeLayer.Top = layout.Position.Y;
                    barcodeLayer.Right = layout.Position.X + layout.Width;
                    barcodeLayer.Bottom = layout.Position.Y + layout.Height;

                    var pixels = loadedImage.LoadArgb32Pixels(loadedImage.Bounds);
                    barcodeLayer.SaveArgb32Pixels(new Rectangle(0, 0, layout.Width, layout.Height), pixels);

                    // 6. 转换为智能对象
                    var smartLayer = targetImage.SmartObjectProvider.ConvertToSmartObject(new[] { barcodeLayer });
                    
                    smartLayer.DisplayName = "BARCODE"; 
                    smartLayer.Left = layout.Position.X;
                    smartLayer.Top = layout.Position.Y;
                }

                targetImage.Save(tempOutputPath, new PsdOptions { 
                    CompressionMethod = CompressionMethod.RLE 
                });
            } 

            if (File.Exists(targetPsdPath)) File.Delete(targetPsdPath);
            File.Move(tempOutputPath, targetPsdPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Warning] 条形码渲染失败: {ex.Message}");
            if (File.Exists(tempOutputPath)) try { File.Delete(tempOutputPath); } catch { }
        }
    }

    private BarcodeLayout CalculateBarcodeLayout(PackagingDimensions dim, double origW_Pt, double origH_Pt, float dpiX, float dpiY)
    {
        var layout = new BarcodeLayout();

        // -------------------------------------------------------------
        // 第一步：判定是否为“细长型” (Slender)
        // 修正逻辑：
        // 1. 如果是 6x6 (扁盒子/正方体)，Ratio = 1。如果不希望旋转，Ratio 必须 < 阈值。
        // 2. 如果是细高盒子 (如牙膏盒)，Height >> Width，Ratio 很大。
        // 因此：当 高度 / 宽度 > 2.0 时，才判定为细长型并旋转。
        // -------------------------------------------------------------
        double ratio = (double)dim.Height / dim.Width;
        layout.Rotate90 = ratio > 2.0; 

        // -------------------------------------------------------------
        // 第二步：设定条码的目标宽度为 4cm
        // -------------------------------------------------------------
        int desiredWidthPx = CmToPixels(4.0); // 4cm 对应的像素值

        // 获取 PDF 原始像素尺寸
        double rawPxW = (origW_Pt * dpiX) / 72.0;
        double rawPxH = (origH_Pt * dpiY) / 72.0;

        // 计算缩放比例：无论是否旋转，我们都希望条码的“长边”（阅读方向的宽度）是 4cm
        double scale = (double)desiredWidthPx / rawPxW;

        // 计算出【未旋转】时的目标尺寸
        int scaledW = desiredWidthPx;                // 4cm
        int scaledH = (int)Math.Round(rawPxH * scale); // 对应高度

        // -------------------------------------------------------------
        // 第三步：根据旋转状态，分配最终的图层宽高
        // -------------------------------------------------------------
        if (layout.Rotate90)
        {
            // 旋转后：宽变高，高变宽
            // 此时视觉上的宽度是 scaledH，视觉上的高度是 scaledW (4cm)
            layout.Width = scaledH;       
            layout.Height = scaledW;      
        }
        else
        {
            // 不旋转：保持原样
            layout.Width = scaledW;       // 视觉上的宽度是 4cm
            layout.Height = scaledH;
        }

        // -------------------------------------------------------------
        // 第四步：定位逻辑 (右下角对齐)
        // -------------------------------------------------------------
        var X_px = CmToPixels(dim.Length);
        var Y_px = CmToPixels(dim.Height);
        var Z_px = CmToPixels(dim.Width);
        var A_px = CmToPixels(dim.BleedLeftRight);
        var B_px = CmToPixels(dim.BleedTopBottom);

        int panelStartX = A_px + Z_px + X_px;       
        int panelStartY = B_px + Z_px;              
        int panelEndY = panelStartY + Y_px;         

        // [X轴] 右对齐
        int sideMarginPx = CmToPixels(0.5); 
        int panelEndX = panelStartX + Z_px;
        int destX = panelEndX - sideMarginPx - layout.Width;

        // [Y轴] 底部对齐
        int bottomMarginPx = CmToPixels(0.8); 
        int destY = panelEndY - bottomMarginPx - layout.Height;

        layout.Position = new Point(destX, destY);
        
        return layout;
    }

    // ====================================================================================
    // ⬇️ 其他辅助方法 (保持不变)
    // ====================================================================================

    private void EmbedFixedAssetAsSmartObject(string targetPsdPath, string assetPath, PackagingDimensions dim)
    {
        if (!File.Exists(assetPath)) 
        {
            Console.WriteLine($"[Error] 固定图标源文件不存在: {assetPath}");
            return;
        }
        if (!File.Exists(targetPsdPath)) return;

        string tempOutputPath = targetPsdPath + ".tmp";
        
        using (var targetImage = (PsdImage)Aspose.PSD.Image.Load(targetPsdPath))
        using (var srcPsd = (PsdImage)Aspose.PSD.Image.Load(assetPath))
        {
            float targetDpiX = (float)targetImage.HorizontalResolution;
            float targetDpiY = (float)targetImage.VerticalResolution;
            const double cmToInch = 1.0 / 2.54;

            int maxBoxWidth = Math.Max(1, (int)Math.Round(ICON_WIDTH_CM * cmToInch * targetDpiX));
            int maxBoxHeight = Math.Max(1, (int)Math.Round(ICON_HEIGHT_CM * cmToInch * targetDpiY));

            double scaleX = (double)maxBoxWidth / srcPsd.Width;
            double scaleY = (double)maxBoxHeight / srcPsd.Height;
            double scale = Math.Min(scaleX, scaleY); 

            int newWidth = Math.Max(1, (int)Math.Round(srcPsd.Width * scale));
            int newHeight = Math.Max(1, (int)Math.Round(srcPsd.Height * scale));

            var pos = CalculateFixedIconPosition(targetImage, dim, newWidth, newHeight);
            int destLeft = pos.X;
            int destTop = pos.Y;

            var placeholder = targetImage.AddRegularLayer();
            placeholder.DisplayName = "FixedIcon_Placeholder";
            placeholder.Left = destLeft;
            placeholder.Top = destTop;
            placeholder.Right = destLeft + newWidth;
            placeholder.Bottom = destTop + newHeight;

            var transparentPixels = new int[newWidth * newHeight];
            placeholder.SaveArgb32Pixels(new Aspose.PSD.Rectangle(0, 0, newWidth, newHeight), transparentPixels);

            var smartLayer = targetImage.SmartObjectProvider.ConvertToSmartObject(new[] { placeholder });
            smartLayer.DisplayName = "FixedIcon_SmartObject";

            if (srcPsd.Width != newWidth || srcPsd.Height != newHeight)
            {
                srcPsd.Resize(newWidth, newHeight, ResizeType.LanczosResample);
            }
            
            srcPsd.HorizontalResolution = targetDpiX;
            srcPsd.VerticalResolution = targetDpiY;

            var resolution = new ResolutionSetting(targetDpiX, targetDpiY);
            smartLayer.ReplaceContents(srcPsd, resolution);

            smartLayer.Left = destLeft;
            smartLayer.Top = destTop;

            var saveOptions = new PsdOptions
            {
                CompressionMethod = CompressionMethod.RLE,
                ColorMode = ColorModes.Rgb
            };
            
            targetImage.Save(tempOutputPath, saveOptions);
        }

        if (File.Exists(targetPsdPath)) File.Delete(targetPsdPath);
        File.Move(tempOutputPath, targetPsdPath);
    }

    private void EmbedLogoAsSmartObject(string targetPsdPath, string assetPath, PackagingDimensions dim)
    {
        if (!File.Exists(assetPath)) return;
        if (!File.Exists(targetPsdPath)) return;

        string tempOutputPath = targetPsdPath + ".tmp";

        using (var targetImage = (PsdImage)Aspose.PSD.Image.Load(targetPsdPath))
        using (var srcPsd = (PsdImage)Aspose.PSD.Image.Load(assetPath))
        {
            float targetDpiX = (float)targetImage.HorizontalResolution;
            float targetDpiY = (float)targetImage.VerticalResolution;
            const double cmToInch = 1.0 / 2.54;

            int maxBoxWidth = Math.Max(1, (int)Math.Round(LOGO_MAX_WIDTH_CM * cmToInch * targetDpiX));
            int maxBoxHeight = Math.Max(1, (int)Math.Round(LOGO_MAX_HEIGHT_CM * cmToInch * targetDpiY));

            double scaleX = (double)maxBoxWidth / srcPsd.Width;
            double scaleY = (double)maxBoxHeight / srcPsd.Height;
            double scale = Math.Min(scaleX, scaleY); 

            int newWidth = Math.Max(1, (int)Math.Round(srcPsd.Width * scale));
            int newHeight = Math.Max(1, (int)Math.Round(srcPsd.Height * scale));

            var pos = CalculateLogoPosition(targetImage, dim, newWidth, newHeight);
            int destLeft = pos.X;
            int destTop = pos.Y;

            var placeholder = targetImage.AddRegularLayer();
            placeholder.DisplayName = "BrandLogo_Placeholder";
            placeholder.Left = destLeft;
            placeholder.Top = destTop;
            placeholder.Right = destLeft + newWidth;
            placeholder.Bottom = destTop + newHeight;

            var transparentPixels = new int[newWidth * newHeight];
            placeholder.SaveArgb32Pixels(new Aspose.PSD.Rectangle(0, 0, newWidth, newHeight), transparentPixels);

            var smartLayer = targetImage.SmartObjectProvider.ConvertToSmartObject(new[] { placeholder });
            smartLayer.DisplayName = "BrandLogo_SmartObject";

            if (srcPsd.Width != newWidth || srcPsd.Height != newHeight)
            {
                srcPsd.Resize(newWidth, newHeight, ResizeType.LanczosResample);
            }
            srcPsd.HorizontalResolution = targetDpiX;
            srcPsd.VerticalResolution = targetDpiY;

            var resolution = new ResolutionSetting(targetDpiX, targetDpiY);
            smartLayer.ReplaceContents(srcPsd, resolution);

            smartLayer.Left = destLeft;
            smartLayer.Top = destTop;

            var saveOptions = new PsdOptions
            {
                CompressionMethod = CompressionMethod.RLE,
                ColorMode = ColorModes.Rgb
            };
            targetImage.Save(tempOutputPath, saveOptions);
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
        int destX = centerX - (widthPx / 2) + 300;

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

        int panelLeft = A + (2 * Z) + X;

        int centerX = panelLeft + (X / 2);
        int destX = centerX - (widthPx / 2) + 300;

        int bottomFoldY = B + Z + Y;
        int iconBottomMargin = (int)(Y * 0.28); 
        
        int destY = bottomFoldY - iconBottomMargin - heightPx + 300;

        return new Point(destX, destY);
    }

    private void DrawStructureLayers(PsdImage psdImage, int X, int Y, int Z, int A, int B, int C, Action<int, string>? onProgress)
    {
        CreateShapeLayer(psdImage, "BG", (2 * X) + (2 * Z) + (2 * A), Y + (4 * C), 0, B + Z - (2 * C), Color.White);

        onProgress?.Invoke(15, "正在绘制侧面板...");
        CreateShapeLayer(psdImage, "left", A + X, Y + (4 * C), 0, B + Z - (2 * C), Color.White);
        CreateShapeLayer(psdImage, "front", X, Y + (4 * C), A + Z, B + Z - (2 * C), Color.White);

        onProgress?.Invoke(25, "正在绘制主面板...");
        CreateShapeLayer(psdImage, "right", X, Y + (4 * C), A + Z + X, B + Z - (2 * C), Color.White);
        CreateShapeLayer(psdImage, "back", A + X, Y + (4 * C), A + (2 * Z) + X, B + Z - (2 * C), Color.White);

        onProgress?.Invoke(35, "正在绘制顶底盖...");
        CreateShapeLayer(psdImage, "top", X, Z, A + Z, B, Color.White);
        CreateShapeLayer(psdImage, "bottom", X, Z, A + (2 * Z) + X, B + Z + Y, Color.White);
    }

    private void DrawInfoPanelAssets(PsdImage psdImage, PackagingAssets assets, PackagingDimensions dim)
    {
        var info = assets.Texts.InfoPanel;
        var main = assets.Texts.MainPanel;
        
        if (info == null && main == null) return;

        var X = CmToPixels(dim.Length);
        var Y = CmToPixels(dim.Height);
        var Z = CmToPixels(dim.Width);
        var A = CmToPixels(dim.BleedLeftRight);
        var B = CmToPixels(dim.BleedTopBottom);
        var C = CmToPixels(dim.InnerBleed);

        int topFoldY = B + Z;
        int safeTopMargin = 80; 

        // --- 1. 背面 (Back Panel) ---
        int backStartX = A + (2 * Z) + X;
        int backStartY = topFoldY + safeTopMargin; 
        int backPanelWidth = A + X; 

        int padding = 30;
        int currentY = backStartY;
        int textAreaWidth = backPanelWidth - (2 * padding);
        float fontSize = 6f;

        if (info != null)
        {
            CreateRichTextLayer(psdImage, "PRODUCT_NAME_TXT", "PRODUCT NAME:", info.ProductName,
                new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
            currentY += 60;

            CreateRichTextLayer(psdImage, "INGREDIENTS_TXT", "INGREDIENTS:", info.Ingredients,
                new Rectangle(backStartX + padding, currentY, textAreaWidth, 100), fontSize);
            currentY += 110;

            CreateRichTextLayer(psdImage, "WARNINGS_TXT", "WARNINGS:", info.Warnings,
                new Rectangle(backStartX + padding, currentY, textAreaWidth, 80), fontSize);
            currentY += 90;

            if (main != null)
            {
                CreateRichTextLayer(psdImage, "MANUFACTURER_TXT", "MANUFACTURER:", main.Manufacturer,
                    new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
                currentY += 60;

                CreateRichTextLayer(psdImage, "MANUFACTURER_ADD_TXT", "ADDRESS:", main.Address,
                    new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
                currentY += 60;
            }

            CreateRichTextLayer(psdImage, "SHELF_LIFE_TXT", "SHELF LIFE:", info.ShelfLife,
                new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
            currentY += 60;

            CreateRichTextLayer(psdImage, "MADE_IN _TXT", "MADE IN:", info.Origin,
                new Rectangle(backStartX + padding, currentY, textAreaWidth, 50), fontSize);
            
            if (main != null && !string.IsNullOrWhiteSpace(main.CapacityInfoBack))
            {
                int areaHeight = 100;
                int bottomFoldY = B + Z + Y; 
                int capY = bottomFoldY - padding - areaHeight; 

                CreateRichTextLayer(psdImage, "NET_BACK_TXT", "", main.CapacityInfoBack,
                    new Rectangle(backStartX + padding, capY, textAreaWidth, areaHeight), 10f);
            }
        }

        // --- 2. 右侧面 (Right Side Panel) ---
        int rightStartX = A + Z + X;
        int rightStartY = topFoldY + safeTopMargin; 
        int rightPanelWidth = Z;
        
        int rightCurrentY = rightStartY;
        int rightTextAreaWidth = rightPanelWidth - (2 * padding);

        if (info != null)
        {
            CreateRichTextLayer(psdImage, "SAFEUSE_TXT", "DIRECTIONS:", info.Directions,
                new Rectangle(rightStartX + padding, rightCurrentY, rightTextAreaWidth, 150), fontSize);
            rightCurrentY += 160;

            CreateRichTextLayer(psdImage, "FUNCTIONS&BENEFITS_TXT", "BENEFITS:", info.Benefits,
                new Rectangle(rightStartX + padding, rightCurrentY, rightTextAreaWidth, 150), fontSize);
        }
    }

    private void DrawMainPanelAssets(PsdImage psdImage, PackagingAssets assets, PackagingDimensions dim)
    {
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

        if (!string.IsNullOrWhiteSpace(info.ProductName))
        {
            int titleHeight = 80;
            var rect = new Rectangle(contentStartX, topFoldY + topPadding, contentWidth, titleHeight);
            CreateRichTextLayer(psdImage, "PRODUCT_NAME_FRONT_TXT", "", info.ProductName, rect, 12f);
        }

        if (main.SellingPoints != null && main.SellingPoints.Count > 0)
        {
            int startY = topFoldY + (int)(Y * 0.35); 
            int lineHeight = 60;
            float fontSize = 8f;

            for (int i = 0; i < main.SellingPoints.Count; i++)
            {
                var pointText = main.SellingPoints[i];
                if (string.IsNullOrWhiteSpace(pointText)) continue;

                var textToShow = "• " + pointText;
                var rect = new Rectangle(contentStartX, startY + (i * lineHeight), contentWidth, lineHeight);
                
                CreateRichTextLayer(psdImage, $"SELLINGPOINT#{i + 1}_TXT", "", textToShow, rect, fontSize);
            }
        }

        if (!string.IsNullOrWhiteSpace(main.CapacityInfo))
        {
            int capacityHeight = 100;
            int bottomFoldY = B + Z + Y;
            int currentBottomY = bottomFoldY - 40 - capacityHeight; 
            
            var rect = new Rectangle(contentStartX, currentBottomY, contentWidth, capacityHeight);
            CreateRichTextLayer(psdImage, "NET_FRONT_TXT", "", main.CapacityInfo, rect, 10f);
        }
    }

    private void AddGuidelines(PsdImage psdImage, int X, int Y, int Z, int A, int B, int C)
    {
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
        var magentaColor = Color.FromArgb(255, 0, 108);
        CreateWhiteRectangleLineLayer(psdImage, layerName, new Rectangle(x, y, width, height), magentaColor, lineGroup);
    }

    private void CreateWhiteRectangleLineLayer(PsdImage psdImage, string layerName, Rectangle rect, Color layerColor, LayerGroup lineGroup)
    {
        if (psdImage == null) return;
        if (rect.Width <= 0 || rect.Height <= 0) return;
        try
        {
            var layer = FillLayer.CreateInstance(FillType.Color);
            if (layer == null) return;
            layer.DisplayName = layerName ?? "guideline";
            lineGroup.AddLayer(layer);
            var vectorPath = VectorDataProvider.CreateVectorPathForLayer(layer);
            if (vectorPath == null) return;
            vectorPath.FillColor = layerColor;
            var shape = new PathShapeGen();
            shape.Points.Add(new BezierKnot(new PointF(rect.X, rect.Y), true));
            shape.Points.Add(new BezierKnot(new PointF(rect.X + rect.Width, rect.Y), true));
            shape.Points.Add(new BezierKnot(new PointF(rect.X + rect.Width, rect.Y + rect.Height), true));
            shape.Points.Add(new BezierKnot(new PointF(rect.X, rect.Y + rect.Height), true));
            vectorPath.Shapes.Add(shape);
            VectorDataProvider.UpdateLayerFromVectorPath(layer, vectorPath, true);
            layer.AddLayerMask(null);
        }
        catch (Exception ex) { Console.WriteLine($"CreateWhiteRectangleLineLayer exception: {ex.Message}"); }
    }

    private void CreateShapeLayer(PsdImage psdImage, string layerName, int width, int height, int x, int y, Color layerColor)
    {
        if (psdImage == null) return;
        if (width <= 0 || height <= 0) return;
        try
        {
            var layer = FillLayer.CreateInstance(FillType.Color);
            if (layer == null) return;
            layer.DisplayName = layerName;
            psdImage.AddLayer(layer);
            var vectorPath = VectorDataProvider.CreateVectorPathForLayer(layer);
            if (vectorPath == null) return;
            vectorPath.FillColor = layerColor;
            var shape = new PathShapeGen();
            var p1 = new PointF(x, y);
            var p2 = new PointF(x + width, y);
            var p3 = new PointF(x + width, y + height);
            var p4 = new PointF(x, y + height);
            shape.Points.Add(new BezierKnot(p1, true));
            shape.Points.Add(new BezierKnot(p2, true));
            shape.Points.Add(new BezierKnot(p3, true));
            shape.Points.Add(new BezierKnot(p4, true));
            vectorPath.Shapes.Add(shape);
            VectorDataProvider.UpdateLayerFromVectorPath(layer, vectorPath, true);
            layer.AddLayerMask(null);
        }
        catch (Exception ex) { Console.WriteLine($"Error creating layer {layerName}: {ex.Message}"); }
    }

    private void CreateRichTextLayer(PsdImage psdImage, string layerName, string label, string content, Rectangle rect, float fontSizePt)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        try
        {
            string safeLabel = ToHalfWidth(label);
            string safeContent = ToHalfWidth(content);

            var textLayer = psdImage.AddTextLayer(layerName, rect);
            var textData = textLayer.TextData;
            float fontSizePixels = PtToPixels(fontSizePt);
            string fontName = FontSettings.GetAdobeFontName("Arial") ?? "Arial";

            if (textData.Items.Length > 0)
            {
                textData.Items[0].Style.FontSize = fontSizePixels;
                textData.Items[0].Paragraph.Justification = JustificationMode.Left;

                if (!string.IsNullOrEmpty(safeLabel))
                {
                    var labelPortion = textData.Items[0];
                    labelPortion.Text = safeLabel + " "; 
                    labelPortion.Style.FauxBold = true;  
                    labelPortion.Style.FillColor = Color.Black;
                    labelPortion.Style.FontName = fontName;

                    var contentPortion = textData.ProducePortion();
                    contentPortion.Text = safeContent;
                    contentPortion.Style.FontSize = fontSizePixels;
                    contentPortion.Style.FauxBold = false;
                    contentPortion.Style.FillColor = Color.Black;
                    contentPortion.Style.FontName = fontName;
                    
                    textData.AddPortion(contentPortion);
                }
                else
                {
                    var portion = textData.Items[0];
                    portion.Text = safeContent;
                    portion.Style.FontName = fontName;
                    portion.Style.FontSize = fontSizePixels;
                    portion.Style.FauxBold = false;
                    portion.Style.FillColor = Color.Black;
                }
                textData.UpdateLayerData();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] 创建文本图层 '{layerName}' 失败: {ex.Message}");
        }
    }

    private string ToHalfWidth(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        StringBuilder sb = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            char mappedChar = c switch
            {
                '、' => ',',  
                '，' => ',',  
                '。' => '.',  
                '：' => ':',  
                '；' => ';',  
                '【' => '[',  
                '】' => ']',  
                '「' => '[',  
                '」' => ']',  
                '『' => '[',  
                '』' => ']',  
                '《' => '<',  
                '》' => '>',  
                '（' => '(',  
                '）' => ')',  
                '～' => '~',  
                '〜' => '~',  
                '—' => '-',   
                '–' => '-',   
                '·' => '.',   
                '“' => '"',
                '”' => '"',
                '‘' => '\'',
                '’' => '\'',
                (char)12288 => ' ', 
                _ => c 
            };

            sb.Append(mappedChar);
        }

        string preProcessed = sb.ToString();
        return preProcessed.Normalize(System.Text.NormalizationForm.FormKC);
    }

    private int CmToPixels(double cm) => (int)Math.Round((cm / 2.54) * DPI);
    private float PtToPixels(float pt) => (pt * DPI) / 72f;

    private void CleanupTempFile(string? path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try { File.Delete(path); } catch { /* 忽略删除失败 */ }
        }
    }
}