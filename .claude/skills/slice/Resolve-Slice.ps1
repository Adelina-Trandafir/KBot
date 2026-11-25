<#
  Resolve-Slice.ps1 - resolves the placeholders of a slice spec to the first free number.
  Read-only: it only reads files (git ls-files, ReadAllText). It writes nothing.

  Usage:  powershell -NoProfile -File .claude\skills\slice\Resolve-Slice.ps1 -Spec 'SLICE-MEnn-mm'

  Spec forms (the SLICE- prefix is optional, letters of the family are upper case):
    SLICE-ME01-50      fixed slice, fixed sub-slice  -> only reports whether it is already used
    SLICE-ME01-mm      fixed slice, first free sub-slice
    SLICE-MEnn-50      first free ME number (ME03...), sub-slice kept as typed
    SLICE-MEnn-mm      first free ME number, sub-slice 01 (a new slice has no sub-slices)
    SLICE-nnEF-mm      digits first, letters last (00EF family)
    SLICE-nnnn-mm      first free 4-digit slice
    SLICE-nnnn         slice only, no sub-slice
  Placeholders: nn / mm / nnnn (also NN / MM / NNNN).

  "Free" = the number appears NOWHERE in the project (file names and file contents of every
  text file git knows, minus .claude/, Surse/, MariaDB_Schema/, any AGENTS.md). Gaps that are
  mentioned somewhere (0021 absorbed by 0078, 0038/0039 in code comments) therefore count as taken.
  The search is deliberately loose: a false hit only skips a number, it never reuses one.
#>
param([Parameter(Mandatory = $true)][string]$Spec)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path

# ---------- parse the spec ----------
$s = $Spec.Trim() -replace '(?i)^slice-', ''
$a = $s
$b = $null
if ($s -match '^(?<a>.+)-(?<b>mm|MM|\d{2,3})$') { $a = $Matches['a']; $b = $Matches['b'] }

$fam = $null
$kind = $null
if ($a -match '^(?i:nnnn)$') { $kind = 'num' }
elseif ($a -cmatch '^(?<f>[A-Za-z]+)(?<p>nn|NN)$') { $kind = 'prefix'; $fam = $Matches['f'].ToUpper() }
elseif ($a -cmatch '^(?<p>nn|NN)(?<f>[A-Za-z]+)$') { $kind = 'suffix'; $fam = $Matches['f'].ToUpper() }
elseif ($a -match '^[A-Za-z0-9]{2,6}$') { $kind = 'fixed'; $a = $a.ToUpper() }
else { throw "Cannot read the slice part '$a' of '$Spec'." }

$subIsPlaceholder = ($null -ne $b) -and ($b -match '^(?i:mm)$')
$subFixed = $null
if (($null -ne $b) -and -not $subIsPlaceholder) { $subFixed = [int]$b }

# ---------- file list ----------
$textExt = @('.md', '.vb', '.py', '.js', '.css', '.html', '.htm', '.sql', '.ps1', '.psm1', '.json', '.xml', '.txt',
    '.vbproj', '.csproj', '.iss', '.cs', '.sln', '.props', '.targets', '.config', '.yml', '.yaml', '.resx', '.bat', '.cmd', '.sh', '.ini')
$all = & git -C $root ls-files -co --exclude-standard
$files = @()
foreach ($f in $all) {
    $n = $f.Replace('\', '/')
    if ($n -like '.claude/*' -or $n -like 'Surse/*' -or $n -like 'MariaDB_Schema/*') { continue }
    if ([IO.Path]::GetFileName($n) -ieq 'AGENTS.md') { continue }
    if ($textExt -notcontains [IO.Path]::GetExtension($n).ToLower()) { continue }
    $files += $n
}

# ---------- regexes ----------
$dash = [char]0x2013; $emd = [char]0x2014; $ell = [char]0x2026
$rangeSep = "(?:/|\.\.|$ell|$dash|$emd)"
$slicePat = $null
$resolvedSlice = $a
$sliceUsed = New-Object 'System.Collections.Generic.HashSet[int]'
switch ($kind) {
    'num'    { $slicePat = [regex]'(?<![A-Za-z0-9])(\d{4})(?![A-Za-z0-9])' }
    'prefix' { $slicePat = [regex]("(?<![A-Za-z0-9])$fam(\d{2})(?![0-9])") }
    'suffix' { $slicePat = [regex]("(?<![A-Za-z0-9])(\d{2})$fam(?![A-Za-z0-9])") }
}

$needSubScan = ($kind -eq 'fixed') -and ($subIsPlaceholder -or ($null -ne $subFixed))
$subPat = $null
$subUsed = New-Object 'System.Collections.Generic.HashSet[int]'
$subWhere = @{}
if ($needSubScan) {
    $subPat = [regex]("(?<![A-Za-z0-9])$a-(\d{2,3})(?!\d)(?:\s*$rangeSep\s*(\d{2,3})(?!\d))?")
}

# ---------- one pass over names + contents ----------
$announce = [regex]'(?i)(next free[^:\r\n]{0,30}:\s*\**\s*)[A-Za-z0-9-]+(?:,\s*[A-Za-z0-9-]+)*'
foreach ($f in $files) {
    $text = $f + "`n"
    $item = Get-Item -LiteralPath (Join-Path $root $f) -ErrorAction SilentlyContinue
    if ($null -eq $item) { continue }   # tracked by Git but deleted in the working tree
    $len = $item.Length
    if ($len -gt 5MB) { continue }
    try { $text += [IO.File]::ReadAllText((Join-Path $root $f)) } catch { continue }
    # "Next free number: 0131" announces a free number, it does not use it: blank those values out.
    $text = $announce.Replace($text, '$1')

    if ($null -ne $slicePat) {
        foreach ($m in $slicePat.Matches($text)) { [void]$sliceUsed.Add([int]$m.Groups[1].Value) }
    }
    if ($null -ne $subPat) {
        foreach ($m in $subPat.Matches($text)) {
            $lo = [int]$m.Groups[1].Value
            $hi = $lo
            if ($m.Groups[2].Success) {
                $e = [int]$m.Groups[2].Value
                if ($e -gt $lo -and ($e - $lo) -lt 100) { $hi = $e }
            }
            for ($i = $lo; $i -le $hi; $i++) {
                [void]$subUsed.Add($i)
                if (-not $subWhere.ContainsKey($i)) { $subWhere[$i] = New-Object System.Collections.Generic.List[string] }
                if ($subWhere[$i].Count -lt 4 -and -not $subWhere[$i].Contains($f)) { $subWhere[$i].Add($f) }
            }
        }
    }
}

# ---------- resolve ----------
$sliceNote = ''
if ($kind -ne 'fixed') {
    $max = 99; $w = 2
    if ($kind -eq 'num') { $max = 9999; $w = 4 }
    $n = 1
    while ($n -le $max -and $sliceUsed.Contains($n)) { $n++ }
    if ($n -gt $max) { throw 'No free number left.' }
    $digits = $n.ToString().PadLeft($w, '0')
    if ($kind -eq 'num') { $resolvedSlice = $digits }
    elseif ($kind -eq 'prefix') { $resolvedSlice = "$fam$digits" }
    else { $resolvedSlice = "$digits$fam" }
    $sliceNote = "first free of $($sliceUsed.Count) numbers found in use"
    if ($kind -ne 'num' -and $sliceUsed.Count -gt 0) {
        $hi = ($sliceUsed | Measure-Object -Maximum).Maximum
        if ($n -lt $hi) { $sliceNote += "; GAP: highest in use is $($hi.ToString().PadLeft(2, '0')), so a next-after-highest reading would give $(($hi + 1).ToString().PadLeft(2, '0'))" }
    }
}

$resolvedSub = $null
$subNote = ''
if ($null -ne $subFixed) {
    $resolvedSub = $subFixed.ToString().PadLeft(2, '0')
    if ($needSubScan) {
        if ($subUsed.Contains($subFixed)) { $subNote = 'ALREADY IN USE in: ' + ($subWhere[$subFixed] -join ', ') }
        else { $subNote = 'not used anywhere yet' }
    }
    elseif ($kind -ne 'fixed') { $subNote = 'new slice, kept as typed' }
}
elseif ($subIsPlaceholder) {
    if ($kind -ne 'fixed') { $resolvedSub = '01'; $subNote = 'new slice has no sub-slices yet' }
    else {
        $n = 1
        while ($subUsed.Contains($n)) { $n++ }
        $resolvedSub = $n.ToString().PadLeft(2, '0')
        $subNote = "first free of $($subUsed.Count) sub-numbers found in use under $a"
        if ($subUsed.Count -gt 0) {
            $hi = ($subUsed | Measure-Object -Maximum).Maximum
            if ($n -lt $hi) { $subNote += "; GAP: highest in use is $($hi.ToString().PadLeft(2, '0')), so a next-after-highest reading would give $(($hi + 1).ToString().PadLeft(2, '0'))" }
        }
    }
}

$full = "SLICE-$resolvedSlice"
if ($null -ne $resolvedSub) { $full += "-$resolvedSub" }

"SPEC      : $Spec"
"RESOLVED  : $full"
"SLICE     : $resolvedSlice" + $(if ($sliceNote) { "   ($sliceNote)" } else { '' })
if ($null -ne $resolvedSub) { "SUB       : $resolvedSub   ($subNote)" }

# ---------- what the status files say is the "next free" number ----------
$statusFiles = @('docs/worklog/KBOT_STATUS.md', 'VERCON/VERCON_STATUS.md', 'ADECHIT_STATUS.md')
$statusFiles += (Get-ChildItem -LiteralPath (Join-Path $root 'docs/worklog/state') -Filter *.md | ForEach-Object { 'docs/worklog/state/' + $_.Name })
$cond = switch ($kind) {
    'num'    { 'Next free slice number' }
    'prefix' { "(?<![A-Za-z0-9])$fam\d{2}" }
    'suffix' { "(?<![A-Za-z0-9])\d{2}$fam(?![A-Za-z0-9])" }
    default  { "(?<![A-Za-z0-9])$a(?![A-Za-z0-9])" }
}
$hint = [regex]'(?i)(next free|urm.torul.{0,40}liber|urm.toarea subfelie|next code)'
$shown = 0
foreach ($f in $statusFiles) {
    $p = Join-Path $root $f
    if (-not (Test-Path -LiteralPath $p)) { continue }
    foreach ($line in [IO.File]::ReadAllLines($p)) {
        if ($hint.IsMatch($line) -and [regex]::IsMatch($line, $cond)) {
            if ($shown -lt 5) {
                $t = $line.Trim(); if ($t.Length -gt 240) { $t = $t.Substring(0, 240) + '...' }
                "STATUS    : [$f] $t"
            }
            $shown++
        }
    }
}
if ($shown -eq 0) { 'STATUS    : (no "next free" line found for this family)' }
