# VC00-04 step 2: render layout JSON (tools/out/layouts) to PNG sketches in VERCON/SCHITE (System.Drawing, no Access involved).
param([string]$Only = '')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot
$layDir = Join-Path $PSScriptRoot 'out\layouts'
$outDir = Join-Path $root 'SCHITE'
New-Item -ItemType Directory -Force $outDir | Out-Null

function AccColor($v, $def) {
  if ($null -eq $v -or $v -eq '') { return $def }
  $n = 0L
  if (-not [long]::TryParse([string]$v, [ref]$n)) { return $def }
  if ($n -lt 0) { return $def }
  return [System.Drawing.Color]::FromArgb(255, [int]($n -band 255), [int](($n -shr 8) -band 255), [int](($n -shr 16) -band 255))
}

$gutter = 78
$tabX = 0; $tabY = 0; $pgIdx = 0
foreach ($f in Get-ChildItem $layDir -Filter *.json) {
  if ($Only -and $f.BaseName -ne $Only) { continue }
  $l = Get-Content $f.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
  $totalH = 0; foreach ($s in $l.sections) { $totalH += [int]$s.height }
  if ($totalH -lt 600) { $totalH = 600 }
  $w = [double]$l.width; if ($w -lt 1500) { $w = 1500 }
  $sc = [math]::Min(0.1, 900.0 / $w)
  if ($totalH * $sc -gt 1500) { $sc = 1500.0 / $totalH }
  if ($sc -lt 0.02) { $sc = 0.02 }
  $titleH = 34
  $bw = [math]::Max(560, [int]($w * $sc) + $gutter + 12)
  $bh = [int]($totalH * $sc) + $titleH + 10 + 18 * ($l.sections.Count)
  $bmp = New-Object System.Drawing.Bitmap ([int]$bw), ([int]$bh)
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.SmoothingMode = 'AntiAlias'; $g.TextRenderingHint = 'AntiAliasGridFit'
  $g.Clear([System.Drawing.Color]::White)
  $fTitle = New-Object System.Drawing.Font 'Segoe UI', 10, ([System.Drawing.FontStyle]::Bold)
  $fSec = New-Object System.Drawing.Font 'Segoe UI', 7
  $kindRo = if ($l.kind -eq 'form') { 'formular' } else { 'raport' }
  $g.DrawString("$kindRo $($l.name)  |  $($l.caption)  |  [schita din export, nu functionalitate implementata]", $fTitle, [System.Drawing.Brushes]::Black, 4, 4)
  $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 120, 120, 120)), 1
  $dash = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 150, 150, 150)), 1
  $dash.DashStyle = 'Dash'
  $y0 = $titleH
  $secBack = @{ FormHeader = '#E8EEF7'; PageHeaderSection = '#F2F2F2'; Detail = '#FFFFFF'; PageFooterSection = '#F2F2F2'; FormFooter = '#E8EEF7' }
  foreach ($s in $l.sections) {
    $sh = [math]::Max(1, [int]([double]$s.height * $sc))
    $rect = New-Object System.Drawing.Rectangle ($gutter), $y0, ([int]($w * $sc)), $sh
    $bg = AccColor $s.back ([System.Drawing.Color]::FromArgb(255, 250, 250, 250))
    if ($s.back -eq $null -or $s.back -eq '') { $bg = [System.Drawing.Color]::FromArgb(255, 252, 252, 252) }
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $bg), $rect)
    $g.DrawRectangle($pen, $rect)
    $secLabel = ([string]$s.name -replace 'Section$', '') + ' ' + [string]$s.height
    $g.DrawString($secLabel, $fSec, [System.Drawing.Brushes]::DarkSlateGray, 2, $y0 + 1)
    foreach ($c in $s.controls) {
      $cx = $gutter + [int]([double]$c.x * $sc); $cy = $y0 + [int]([double]$c.y * $sc)
      $cw = [math]::Max(2, [int]([double]$c.w * $sc)); $ch = [math]::Max(2, [int]([double]$c.h * $sc))
      $px = [math]::Max(6.5, [math]::Min(14, [double]$c.fs * 1.333 * ($sc * 15)))
      $style = if ([int]$c.fw -ge 700) { [System.Drawing.FontStyle]::Bold } else { [System.Drawing.FontStyle]::Regular }
      $fnt = New-Object System.Drawing.Font 'Segoe UI', ($px * 0.75), $style
      $fore = AccColor $c.fc ([System.Drawing.Color]::Black)
      $br = New-Object System.Drawing.SolidBrush $fore
      $r = New-Object System.Drawing.RectangleF $cx, $cy, $cw, $ch
      $sf = New-Object System.Drawing.StringFormat; $sf.Trimming = 'EllipsisCharacter'; $sf.FormatFlags = 'NoWrap'
      $pp = if ($c.vis) { $pen } else { $dash }
      switch ($c.t) {
        'Label' { $txt = $c.cap; if (-not $txt) { $txt = '' }
                  if ($c.bs -eq '1' -and $c.bc) { $g.FillRectangle((New-Object System.Drawing.SolidBrush (AccColor $c.bc ([System.Drawing.Color]::White))), $cx, $cy, $cw, $ch) }
                  $g.DrawString($txt, $fnt, $br, $r, $sf) }
        'TextBox' { $g.FillRectangle([System.Drawing.Brushes]::White, $cx, $cy, $cw, $ch); $g.DrawRectangle($pp, $cx, $cy, $cw, $ch)
                    $txt = if ($c.cs) { $c.cs } else { $c.n }
                    $g.DrawString("[$txt]", $fnt, [System.Drawing.Brushes]::SteelBlue, $r, $sf) }
        'ComboBox' { $g.FillRectangle([System.Drawing.Brushes]::White, $cx, $cy, $cw, $ch); $g.DrawRectangle($pp, $cx, $cy, $cw, $ch)
                     $g.FillRectangle([System.Drawing.Brushes]::LightGray, ($cx + $cw - 9), $cy, 9, $ch)
                     $txt = if ($c.cs) { $c.cs } else { $c.n }
                     $g.DrawString("[$txt] v", $fnt, [System.Drawing.Brushes]::SteelBlue, (New-Object System.Drawing.RectangleF $cx, $cy, ($cw - 9), $ch), $sf) }
        'ListBox' { $g.FillRectangle([System.Drawing.Brushes]::White, $cx, $cy, $cw, $ch); $g.DrawRectangle($pp, $cx, $cy, $cw, $ch)
                    $g.DrawString("lista: $($c.n)", $fnt, [System.Drawing.Brushes]::SteelBlue, $r, $sf) }
        'CheckBox' { $g.DrawRectangle($pp, $cx, $cy, [math]::Min($ch, 9), [math]::Min($ch, 9)); $g.DrawString("[x] $($c.n)", $fnt, [System.Drawing.Brushes]::SteelBlue, (New-Object System.Drawing.RectangleF ($cx + 10), $cy, ($cw), $ch), $sf) }
        'CommandButton' { $g.FillRectangle([System.Drawing.Brushes]::Gainsboro, $cx, $cy, $cw, $ch); $g.DrawRectangle($pen, $cx, $cy, $cw, $ch)
                          $sfc = New-Object System.Drawing.StringFormat; $sfc.Alignment = 'Center'; $sfc.LineAlignment = 'Center'; $sfc.Trimming = 'EllipsisCharacter'
                          $txt = if ($c.cap) { $c.cap } else { $c.n }
                          $g.DrawString($txt, $fnt, [System.Drawing.Brushes]::Black, $r, $sfc) }
        'Subform' { $g.DrawRectangle($dash, $cx, $cy, $cw, $ch); $g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(40, 70, 130, 180))), $cx, $cy, $cw, $ch)
                    $src = if ($c.so) { $c.so } else { $c.n }
                    $g.DrawString("subformular: $src", $fnt, [System.Drawing.Brushes]::DarkBlue, $r, $sf) }
        'Tab' { $g.DrawRectangle($pen, $cx, $cy, $cw, $ch); $g.DrawString("tab: $($c.n)", $fnt, [System.Drawing.Brushes]::DimGray, $r, $sf); $tabX = $cx; $tabY = $cy; $pgIdx = 0 }
        'Page' { $lbl = if ($c.cap) { $c.cap } else { $c.n }; $lx = $tabX + 4 + $pgIdx * 92; $g.FillRectangle([System.Drawing.Brushes]::Gold, $lx, ($tabY + 12), 90, 13); $g.DrawRectangle($pen, $lx, ($tabY + 12), 90, 13); $g.DrawString($lbl, $fSec, [System.Drawing.Brushes]::Black, (New-Object System.Drawing.RectangleF $lx, ($tabY + 12), 90, 13), $sf); $pgIdx++ }
        'Rectangle' { $g.DrawRectangle($pp, $cx, $cy, $cw, $ch) }
        'Line' { $g.DrawLine($pen, $cx, $cy, ($cx + $cw), ($cy + $ch)) }
        'Image' { $g.DrawRectangle($dash, $cx, $cy, $cw, $ch); $g.DrawString('img', $fnt, [System.Drawing.Brushes]::Gray, $r, $sf) }
        'OptionGroup' { $g.DrawRectangle($pp, $cx, $cy, $cw, $ch); $g.DrawString("grup: $($c.n)", $fnt, [System.Drawing.Brushes]::SteelBlue, $r, $sf) }
        'ToggleButton' { $g.DrawRectangle($pen, $cx, $cy, $cw, $ch); $g.DrawString($c.cap, $fnt, $br, $r, $sf) }
        default { $g.DrawRectangle($dash, $cx, $cy, $cw, $ch); $g.DrawString("$($c.t) $($c.n)", $fnt, [System.Drawing.Brushes]::Gray, $r, $sf) }
      }
      $fnt.Dispose(); $br.Dispose()
    }
    $y0 += $sh + 2
  }
  $g.DrawString("legenda: [camp] = control legat; v = lista derulanta; linie intrerupta = ascuns/subformular; dimensiuni din export (twips), scara $([math]::Round($sc*15,2)):1", $fSec, [System.Drawing.Brushes]::Gray, 4, ($bh - 14))
  $g.Dispose()
  $bmp.Save((Join-Path $outDir ($f.BaseName + '.png')), [System.Drawing.Imaging.ImageFormat]::Png)
  $bmp.Dispose()
}
"done: " + (Get-ChildItem $outDir -Filter *.png).Count + " PNG"
