Option Strict On
Imports System.Collections.Generic
Imports Microsoft.Win32
Imports KBot.Common

''' <summary>
''' The ONE Adobe preference the shipping code writes (slice 0078-02).
'''
''' WHY THIS IS AN EXCEPTION to the rule in <see cref="AdobeReaderHost"/> («never write an Adobe
''' preference»): Acrobat DC's own «Save As» screen -- the one offering Adobe cloud storage -- is
''' not a standard Windows dialog, so <see cref="AdobeSaveTrap"/> cannot fill it, and the operator
''' could then save a signed document anywhere. <c>bToggleCustomSaveExperience = 1</c> («Show
''' online storage when saving files» OFF) makes Adobe use the standard Windows dialog instead.
''' It changes only WHICH dialog Adobe shows when saving, nothing about how documents look or
''' open, and it is written only when a signing session starts -- never on a plain preview.
'''
''' !!! UNVERIFIED !!! The value name is the one quoted by Adobe community threads; it has not
''' been checked against the Adobe build on the operator's machine. The first on-screen run must
''' confirm it in adobe_preview.log (the trap logs every dialog it sees).
''' </summary>
Public NotInheritable Class AdobePrefs

    Private Sub New()
    End Sub

    Public Const ValToggleCustomSave As String = "bToggleCustomSaveExperience"

    ''' <summary>
    ''' Sets <c>bToggleCustomSaveExperience = 1</c> under every AVGeneral hive that exists (Reader
    ''' and/or Acrobat), or under the hive matching the installed exe when neither exists yet.
    ''' Returns what it did, for the log. Never throws -- a failed write only means the trap may
    ''' meet Adobe's own screen, which it then logs and the operator is told about.
    ''' </summary>
    Public Shared Function EnsureStandardSaveDialog(Optional registry As IRegistryAccess = Nothing) As String
        Try
            Dim reg As IRegistryAccess = If(registry, New WinRegistryAccess())
            Dim readerExists As Boolean = reg.KeyExists(AdobeRegistryConstants.AvGeneralReader)
            Dim acrobatExists As Boolean = reg.KeyExists(AdobeRegistryConstants.AvGeneralAcrobat)
            Dim targets As New List(Of String)()
            If readerExists Then targets.Add(AdobeRegistryConstants.AvGeneralReader)
            If acrobatExists Then targets.Add(AdobeRegistryConstants.AvGeneralAcrobat)
            If targets.Count = 0 Then
                targets.Add(AdobeHiveResolver.Resolve(False, False, AdobeWindowHosting.ResolveAdobePath()).AvGeneralPath)
            End If

            Dim done As New List(Of String)()
            For Each path As String In targets
                Dim current As RegistryValueSnapshot = reg.Read(path, ValToggleCustomSave)
                If current.Presence = RegPresence.Present AndAlso
                   current.Kind = RegistryValueKind.DWord AndAlso
                   Convert.ToInt32(current.Value) = 1 Then
                    done.Add($"{path}: deja 1")
                    Continue For
                End If
                reg.Write(path, ValToggleCustomSave, RegistryValueKind.DWord, 1)
                done.Add($"{path}: {current} -> 1")
            Next
            Return "Dialog standard de salvare Adobe: " & String.Join("; ", done)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePrefs.EnsureStandardSaveDialog", ex)
            Return "ATENȚIE: preferința Adobe pentru dialogul standard de salvare NU a putut fi scrisă."
        End Try
    End Function

End Class
