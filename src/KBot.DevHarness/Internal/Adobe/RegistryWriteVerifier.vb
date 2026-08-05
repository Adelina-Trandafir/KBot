Option Strict On
Imports System.Globalization
Imports Microsoft.Win32
' RegistryValueSnapshot / RegPresence au trecut în KBot.Controls (felia 0024-03), ca previzualizarea
' livrată să poată folosi ACELAȘI mecanism de instantaneu ca bancul, nu o a doua copie a lui.
Imports KBot.Controls

' Verdict of comparing what was INTENDED against what the registry actually holds afterwards
' (slice 0023). Pure: the caller does the read-back and hands the snapshot in.
Public NotInheritable Class WriteVerification

    Public ReadOnly Property Matches As Boolean
    Public ReadOnly Property Message As String

    Private Sub New(matches As Boolean, message As String)
        Me.Matches = matches
        Me.Message = message
    End Sub

    Public Shared Function Ok(message As String) As WriteVerification
        Return New WriteVerification(True, message)
    End Function

    Public Shared Function Failed(message As String) As WriteVerification
        Return New WriteVerification(False, message)
    End Function

End Class

''' <summary>
''' Compares an intended write against the value read back from the registry.
''' A preference that will not stick is a result worth STOPPING for — Adobe rewrites its
''' preferences on exit and some values are simply refused, which is exactly the class of failure
''' that produced four "successful" runs that tested nothing.
''' </summary>
Public NotInheritable Class RegistryWriteVerifier

    Private Sub New()
    End Sub

    Public Shared Function Verify(path As String, intent As UserPrefIntent,
                                  actual As RegistryValueSnapshot) As WriteVerification
        Dim where As String = $"{path}\{intent.Name}"

        If intent.Action = UserPrefAction.Delete Then
            If actual Is Nothing OrElse actual.Presence = RegPresence.Absent Then
                Return WriteVerification.Ok($"HKCU șters: {where} (verificat)")
            End If
            Return WriteVerification.Failed(
                $"EȘEC: {where} — cerut (șters), citit {Describe(actual)}")
        End If

        If actual Is Nothing OrElse actual.Presence = RegPresence.Absent Then
            Return WriteVerification.Failed(
                $"EȘEC: {where} — cerut {intent.RequestedText()}, citit (absent)")
        End If

        If actual.Kind <> intent.Kind Then
            Return WriteVerification.Failed(
                $"EȘEC: {where} — cerut {intent.RequestedText()} ({intent.Kind}), " &
                $"citit {Describe(actual)} ({actual.Kind})")
        End If

        If Not SameValue(intent.Value, actual.Value) Then
            Return WriteVerification.Failed(
                $"EȘEC: {where} — cerut {intent.RequestedText()}, citit {Describe(actual)}")
        End If

        Return WriteVerification.Ok($"HKCU scris: {where} = {Describe(actual)} ({actual.Kind}) (verificat)")
    End Function

    ' DWORDs surface as Integer, strings as String; compare on the invariant text so a boxed
    ' Integer and a Long holding the same number do not read as a mismatch.
    Private Shared Function SameValue(intended As Object, actual As Object) As Boolean
        Return String.Equals(Text(intended), Text(actual), StringComparison.Ordinal)
    End Function

    Private Shared Function Text(v As Object) As String
        If v Is Nothing Then Return ""
        Return Convert.ToString(v, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function Describe(snap As RegistryValueSnapshot) As String
        If snap Is Nothing OrElse snap.Presence = RegPresence.Absent Then Return "(absent)"
        Return Text(snap.Value)
    End Function

End Class
