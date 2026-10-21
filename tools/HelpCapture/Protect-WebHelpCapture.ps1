param(
    [Parameter(Mandatory = $true)][string]$ManifestPath
)

# Browser capture bytes remain temporary. Only anonymized JPEGs enter static/help.
# Rectangles use CSS pixels from the visible DOM and scale to the actual bitmap.
Add-Type -AssemblyName System.Drawing
$captures = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
foreach ($capture in $captures) {
    $source = [System.Drawing.Image]::FromFile($capture.rawPath)
    $bitmap = New-Object System.Drawing.Bitmap($source)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(218, 223, 230))
    $maskClip = New-Object System.Drawing.Region([System.Drawing.Rectangle]::new(0, 0, $bitmap.Width, $bitmap.Height))
    try {
        $scaleX = $bitmap.Width / [double]$capture.width
        $scaleY = $bitmap.Height / [double]$capture.height
        foreach ($preserved in $capture.preserveRegions) {
            $maskClip.Exclude([System.Drawing.Rectangle]::new(
                [int]([double]$preserved.x * $scaleX), [int]([double]$preserved.y * $scaleY),
                [int]([double]$preserved.width * $scaleX), [int]([double]$preserved.height * $scaleY)))
        }
        $graphics.SetClip($maskClip, [System.Drawing.Drawing2D.CombineMode]::Replace)
        foreach ($region in $capture.regions) {
            $left = [Math]::Max(0, [Math]::Floor([double]$region.x * $scaleX))
            $top = [Math]::Max(0, [Math]::Floor([double]$region.y * $scaleY))
            $right = [Math]::Min($bitmap.Width, [Math]::Ceiling(([double]$region.x + [double]$region.width) * $scaleX))
            $bottom = [Math]::Min($bitmap.Height, [Math]::Ceiling(([double]$region.y + [double]$region.height) * $scaleY))
            if ($right -gt $left -and $bottom -gt $top) {
                # Opaque masking removes the original text completely, including tiny identifiers.
                $graphics.FillRectangle($brush, [int]$left, [int]$top, [int]($right - $left), [int]($bottom - $top))
            }
        }
        if ($capture.crop) {
            $cropRect = [System.Drawing.Rectangle]::new(
                [int]([double]$capture.crop.x * $scaleX), [int]([double]$capture.crop.y * $scaleY),
                [int]([double]$capture.crop.width * $scaleX), [int]([double]$capture.crop.height * $scaleY))
            $cropped = $bitmap.Clone($cropRect, $bitmap.PixelFormat)
            try { $cropped.Save($capture.outputPath, [System.Drawing.Imaging.ImageFormat]::Jpeg) }
            finally { $cropped.Dispose() }
        }
        else { $bitmap.Save($capture.outputPath, [System.Drawing.Imaging.ImageFormat]::Jpeg) }
        Write-Output ([System.IO.Path]::GetFileName($capture.outputPath))
    }
    finally {
        $brush.Dispose()
        $maskClip.Dispose()
        $graphics.Dispose()
        $bitmap.Dispose()
        $source.Dispose()
    }
}
