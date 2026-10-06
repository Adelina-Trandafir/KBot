Option Strict On
Imports Microsoft.Win32
Imports KBot.Common

''' <summary>
''' Reads and writes two checkboxes of Adobe's own Preferences from inside K-BOT:
''' «Show border hover color for fields» (Forms) and «Use modern user interface for signing and
''' Digital ID configuration» (Signatures ▸ Creation). The registry IS the store: nothing is copied
''' into K-BOT's settings, so the Settings page always shows what Adobe will do.
'''
''' An Adobe that is already running keeps the value it read at start and may write its own value
''' back on exit; the change is seen by the next Adobe started.
'''
''' ABSENT value = Adobe's default: hover border on, modern signing UI off (assumption -- the
''' values on the operator's PC are always present, so the absent case was not observed).
''' </summary>
Public NotInheritable Class AdobeUserCheckboxes

    Private Sub New()
    End Sub

    Public Const DefaultFieldHoverBorder As Boolean = True
    Public Const DefaultModernSigningUi As Boolean = False

    ''' <summary>The product root key (AVGeneral's parent) for the installed Adobe.</summary>
    Public Shared Function ProductRoot(Optional k_registry As IRegistryAccess = Nothing) As String
        Try
            Dim k_reg As IRegistryAccess = If(k_registry, New WinRegistryAccess())
            Dim k_res As AdobeHiveResolution = AdobeHiveResolver.Resolve(
                readerHiveExists:=k_reg.KeyExists(AdobeRegistryConstants.AvGeneralReader),
                acrobatHiveExists:=k_reg.KeyExists(AdobeRegistryConstants.AvGeneralAcrobat),
                exePath:=AdobeWindowHosting.ResolveAdobePath())
            Dim k_cut As Integer = k_res.AvGeneralPath.LastIndexOf("\"c)
            Return k_res.AvGeneralPath.Substring(0, k_cut)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUserCheckboxes.ProductRoot", ex)
            Throw
        End Try
    End Function

    Public Shared Function GetFieldHoverBorder(Optional k_registry As IRegistryAccess = Nothing) As Boolean
        Return ReadFlag(k_registry, AdobeRegistryConstants.SubKeyFormsPrefs,
                        AdobeRegistryConstants.ValFieldHoverBorder, DefaultFieldHoverBorder)
    End Function

    Public Shared Sub SetFieldHoverBorder(k_on As Boolean, Optional k_registry As IRegistryAccess = Nothing)
        WriteFlag(k_registry, AdobeRegistryConstants.SubKeyFormsPrefs,
                  AdobeRegistryConstants.ValFieldHoverBorder, k_on)
    End Sub

    Public Shared Function GetModernSigningUi(Optional k_registry As IRegistryAccess = Nothing) As Boolean
        Return ReadFlag(k_registry, AdobeRegistryConstants.SubKeyPubSec,
                        AdobeRegistryConstants.ValModernSigningUi, DefaultModernSigningUi)
    End Function

    Public Shared Sub SetModernSigningUi(k_on As Boolean, Optional k_registry As IRegistryAccess = Nothing)
        WriteFlag(k_registry, AdobeRegistryConstants.SubKeyPubSec,
                  AdobeRegistryConstants.ValModernSigningUi, k_on)
    End Sub

    Private Shared Function ReadFlag(k_registry As IRegistryAccess, k_subKey As String,
                                     k_name As String, k_default As Boolean) As Boolean
        Try
            Dim k_reg As IRegistryAccess = If(k_registry, New WinRegistryAccess())
            Dim k_snap As RegistryValueSnapshot = k_reg.Read(ProductRoot(k_reg) & "\" & k_subKey, k_name)
            If k_snap.Presence = RegPresence.Absent Then Return k_default
            ' DWORD normally; compare the number, not the type (see AdobeUiPreference.IsZero).
            Dim k_parsed As Long
            If Not Long.TryParse(Convert.ToString(k_snap.Value, Globalization.CultureInfo.InvariantCulture),
                                 Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture,
                                 k_parsed) Then Return k_default
            Return k_parsed <> 0
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUserCheckboxes.ReadFlag", ex)
            Throw
        End Try
    End Function

    Private Shared Sub WriteFlag(k_registry As IRegistryAccess, k_subKey As String,
                                 k_name As String, k_on As Boolean)
        Try
            Dim k_reg As IRegistryAccess = If(k_registry, New WinRegistryAccess())
            k_reg.Write(ProductRoot(k_reg) & "\" & k_subKey, k_name, RegistryValueKind.DWord, If(k_on, 1, 0))
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUserCheckboxes.WriteFlag", ex)
            Throw
        End Try
    End Sub

End Class
