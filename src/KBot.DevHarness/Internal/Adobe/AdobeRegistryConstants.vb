Option Strict On

' Verified registry key/value names for the Adobe right-hand-pane (RHP) harness (slice 0023).
' These are transcribed from the plan §2 (Adobe Enterprise Toolkit + Adobe community threads).
' NONE of them has been tested on a real machine — see the worklog §"unverified". Code must use
' THESE names, never substitutes from memory: an invented key name is worse than an absent one.
Public NotInheritable Class AdobeRegistryConstants

    Private Sub New()
    End Sub

    ' Hive prefixes as spelled by reg.exe / RegistryKey.OpenSubKey callers below.
    Public Const HkcuPrefix As String = "HKEY_CURRENT_USER"
    Public Const HklmPrefix As String = "HKEY_LOCAL_MACHINE"

    ' §2.1 — right-hand pane, per user (COMMUNITY-SOURCED, unreliable on the modern viewer).
    ' Two candidate AVGeneral hives; which one applies depends on the product/build.
    Public Const AvGeneralReader As String = "HKEY_CURRENT_USER\Software\Adobe\Acrobat Reader\DC\AVGeneral"
    Public Const AvGeneralAcrobat As String = "HKEY_CURRENT_USER\Software\Adobe\Adobe Acrobat\DC\AVGeneral"

    Public Const ValExpandRhp As String = "bExpandRHPInViewer"      ' REG_DWORD 0
    Public Const ValRhpSticky As String = "bRHPSticky"             ' REG_DWORD 1
    Public Const ValRhpViewMode As String = "aDefaultRHPViewMode_L" ' REG_SZ "Collapsed"
    Public Const RhpViewModeCollapsed As String = "Collapsed"

    ' §2.2 — viewer generation, per user (Adobe-documented). Same AVGeneral hive.
    Public Const ValEnableAv2 As String = "bEnableAv2"            ' REG_DWORD 0=classic, 1=modern

    ' §2.3 — upsell & services, machine-wide, elevation required (Adobe-documented).
    Public Const ProductReader As String = "Acrobat Reader"       ' <product> for FeatureLockDown
    Public Const ProductAcrobat As String = "Adobe Acrobat"
    Public Const PolicyVersion As String = "DC"                   ' <version> on current tracks
    Public Const ValSuppressUpsell As String = "bAcroSuppressUpsell"          ' REG_DWORD 1
    Public Const ValToggleServices As String = "bToggleAdobeDocumentServices" ' REG_DWORD 1

    ' HKLM\SOFTWARE\Policies\Adobe\<product>\<version>\FeatureLockDown
    Public Shared Function FeatureLockDownPath(product As String) As String
        Return $"{HklmPrefix}\SOFTWARE\Policies\Adobe\{product}\{PolicyVersion}\FeatureLockDown"
    End Function

    ' …\FeatureLockDown\cServices — NOT created by the installer; create it when writing.
    Public Shared Function CServicesPath(product As String) As String
        Return FeatureLockDownPath(product) & "\cServices"
    End Function

End Class
