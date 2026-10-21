param(
    [Parameter(Mandatory = $true)][string]$ManifestPath
)

# Factual browser captures: strong, flattened blur, with no opaque replacement.
# Raw captures and the private region manifest stay temporary.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;

public static class HelpCaptureBlur {
    public static byte[] Strong(byte[] pixels, int width, int height, int stride) {
            var temporary = new byte[pixels.Length];
            const int radius = 24;
            // Three separable box passes approximate a wide Gaussian blur.
            for (int pass = 0; pass < 3; pass++) {
                for (int y = 0; y < height; y++) {
                    int[] sums = new int[4];
                    for (int dx = -radius; dx <= radius; dx++) {
                        int index = y * stride + Math.Max(0, Math.Min(width - 1, dx)) * 4;
                        for (int c = 0; c < 4; c++) sums[c] += pixels[index + c];
                    }
                    for (int x = 0; x < width; x++) {
                        int index = y * stride + x * 4;
                        for (int c = 0; c < 4; c++) temporary[index + c] = (byte)(sums[c] / (2 * radius + 1));
                        int remove = y * stride + Math.Max(0, x - radius) * 4;
                        int add = y * stride + Math.Min(width - 1, x + radius + 1) * 4;
                        for (int c = 0; c < 4; c++) sums[c] += pixels[add + c] - pixels[remove + c];
                    }
                }
                for (int x = 0; x < width; x++) {
                    int[] sums = new int[4];
                    for (int dy = -radius; dy <= radius; dy++) {
                        int index = Math.Max(0, Math.Min(height - 1, dy)) * stride + x * 4;
                        for (int c = 0; c < 4; c++) sums[c] += temporary[index + c];
                    }
                    for (int y = 0; y < height; y++) {
                        int index = y * stride + x * 4;
                        for (int c = 0; c < 4; c++) pixels[index + c] = (byte)(sums[c] / (2 * radius + 1));
                        int remove = Math.Max(0, y - radius) * stride + x * 4;
                        int add = Math.Min(height - 1, y + radius + 1) * stride + x * 4;
                        for (int c = 0; c < 4; c++) sums[c] += temporary[add + c] - temporary[remove + c];
                    }
                }
            }
        return pixels;
    }
}
'@

$captures = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
foreach ($capture in $captures) {
    $source = [System.Drawing.Bitmap]::FromFile($capture.rawPath)
    $bitmap = [System.Drawing.Bitmap]::new($source)
    $blurred = [System.Drawing.Bitmap]::new($bitmap.Width, $bitmap.Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $blurGraphics = [System.Drawing.Graphics]::FromImage($blurred)
    try { $blurGraphics.DrawImageUnscaled($bitmap, 0, 0) }
    finally { $blurGraphics.Dispose() }
    $bounds = [System.Drawing.Rectangle]::new(0, 0, $blurred.Width, $blurred.Height)
    $data = $blurred.LockBits($bounds, [System.Drawing.Imaging.ImageLockMode]::ReadWrite, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $pixels = [byte[]]::new($data.Stride * $data.Height)
        [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $pixels, 0, $pixels.Length)
        $pixels = [HelpCaptureBlur]::Strong($pixels, $data.Width, $data.Height, $data.Stride)
        [System.Runtime.InteropServices.Marshal]::Copy($pixels, 0, $data.Scan0, $pixels.Length)
    }
    finally { $blurred.UnlockBits($data) }
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $clip = [System.Drawing.Region]::new([System.Drawing.Rectangle]::new(0, 0, $bitmap.Width, $bitmap.Height))
    try {
        $scaleX = $bitmap.Width / [double]$capture.width
        $scaleY = $bitmap.Height / [double]$capture.height
        foreach ($preserved in $capture.preserveRegions) {
            $clip.Exclude([System.Drawing.Rectangle]::new(
                [int]($preserved.x * $scaleX), [int]($preserved.y * $scaleY),
                [int]($preserved.width * $scaleX), [int]($preserved.height * $scaleY)))
        }
        $graphics.SetClip($clip, [System.Drawing.Drawing2D.CombineMode]::Replace)
        foreach ($region in $capture.regions) {
            $left = [Math]::Max(0, [Math]::Floor([double]$region.x * $scaleX))
            $top = [Math]::Max(0, [Math]::Floor([double]$region.y * $scaleY))
            $right = [Math]::Min($bitmap.Width, [Math]::Ceiling(([double]$region.x + [double]$region.width) * $scaleX))
            $bottom = [Math]::Min($bitmap.Height, [Math]::Ceiling(([double]$region.y + [double]$region.height) * $scaleY))
            if ($right -gt $left -and $bottom -gt $top) {
                $rectangle = [System.Drawing.Rectangle]::new($left, $top, $right - $left, $bottom - $top)
                $graphics.DrawImage($blurred, $rectangle, $rectangle, [System.Drawing.GraphicsUnit]::Pixel)
            }
        }
        if ($capture.crop) {
            $cropRect = [System.Drawing.Rectangle]::new(
                [int]($capture.crop.x * $scaleX), [int]($capture.crop.y * $scaleY),
                [int]($capture.crop.width * $scaleX), [int]($capture.crop.height * $scaleY))
            $cropped = $bitmap.Clone($cropRect, $bitmap.PixelFormat)
            try { $cropped.Save($capture.outputPath, [System.Drawing.Imaging.ImageFormat]::Jpeg) }
            finally { $cropped.Dispose() }
        }
        else { $bitmap.Save($capture.outputPath, [System.Drawing.Imaging.ImageFormat]::Jpeg) }
        Write-Output ([System.IO.Path]::GetFileName($capture.outputPath))
    }
    finally {
        $clip.Dispose()
        $graphics.Dispose()
        $blurred.Dispose()
        $bitmap.Dispose()
        $source.Dispose()
    }
}
