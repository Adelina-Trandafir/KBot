Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' The advice about the installed Adobe, shown once at K-BOT's start when it is not the free
''' Adobe Acrobat Reader 2025 or newer (<see cref="AdobeProductInfo.IsRecommended"/>). Seen on an
''' older / unofficial Acrobat Pro: the form's own scripts raise «cannot be set because doing so
''' would violate this document's permissions settings» while a document is completed or signed.
''' The same file is clean in the free Reader.
''' <para>The operator turns the advice off from its own window («Da» to «Nu mai arăta»); that writes
''' <see cref="AppSettings.ShowAdobeAdvice"/>, and «Setări → Generale» turns it back on.</para>
''' </summary>
Friend NotInheritable Class AdobeAdvice

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Shows the advice when it is due. Startup boundary: never throws, never stops the window from
    ''' opening. Nothing installed is NOT advised on here (the flows that need Adobe say so themselves).
    ''' </summary>
    Friend Shared Sub ShowIfDue(k_owner As IWin32Window)
        Try
            If Not AppSettings.Current.ShowAdobeAdvice Then Return

            Dim k_path As String = AdobeWindowHosting.ResolveAdobePath()
            If String.IsNullOrEmpty(k_path) Then Return

            Dim k_info As AdobeProductInfo = AdobeProductInfo.Read(k_path)
            If k_info.IsRecommended Then Return

            Dim k_answer As DialogResult = KBotMessage.Show(k_owner, BuildText(k_info),
                                                            "Adobe Acrobat Reader",
                                                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                                                            MessageBoxDefaultButton.Button2)
            If k_answer = DialogResult.Yes Then SwitchOff()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeAdvice.ShowIfDue", ex)
        End Try
    End Sub

    ''' <summary>Pure: the text of the advice for the Adobe that was found.</summary>
    Friend Shared Function BuildText(k_info As AdobeProductInfo) As String
        Dim k_found As String = If(k_info.IsKnown, $"Adobe Acrobat, versiunea {k_info.FileVersion}", "Adobe Acrobat (versiunea nu a putut fi citită)")
        Return $"Pe acest calculator este instalat: {k_found}." & vbCrLf & vbCrLf &
               "K-BOT merge cel mai bine cu Adobe Acrobat Reader gratuit, versiunea 2025 sau mai nouă. " &
               "Cu un Acrobat mai vechi sau neoriginal pot apărea mesaje de eroare la completarea " &
               "sau la semnarea documentelor (de exemplu «cannot be set because doing so would violate " &
               "this document's permissions settings»)." & vbCrLf & vbCrLf &
               "Vrei să nu mai primești acest avertisment? Îl poți porni din nou din Setări, la «Generale»."
    End Function

    ' Writes the switch off. A write that fails is logged and the advice simply comes back next time.
    Private Shared Sub SwitchOff()
        Try
            Dim k_copy As AppSettings = AppSettings.Current.Clone()
            k_copy.ShowAdobeAdvice = False
            k_copy.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeAdvice.SwitchOff", ex)
        End Try
    End Sub

End Class
