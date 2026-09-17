Option Strict On
Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The window <see cref="FormFitHarnessForm"/> opens to prove two things at once: WHERE a themed
''' form lands (the application's screen, never the mouse's) and HOW BIG it comes up (its base,
''' grown to the themed content, never smaller). It reports both on its own face, so the verdict
''' does not depend on the operator remembering where the mouse was.
'''
''' <para>One class for both shapes: a framed dialog and a borderless shell. It inherits
''' <see cref="KBotShellForm"/>, whose extra behaviours only switch on when
''' <c>FormBorderStyle = None</c> -- so the framed variant IS a plain themed form, and the
''' borderless one exercises the WM_GETMINMAXINFO coordination named as risk 1 of the slice.</para>
''' </summary>
Public NotInheritable Class FormFitProbeDialog

    Private ReadOnly _log As Action(Of String)

    ''' <param name="borderless">True = shell shape (no frame), False = framed dialog.</param>
    ''' <param name="startPosition">The WinForms rule to put under test (CenterScreen or CenterParent).</param>
    Public Sub New(borderless As Boolean, startPosition As FormStartPosition, log As Action(Of String))
        _log = log
        InitializeComponent()
        If borderless Then
            FormBorderStyle = FormBorderStyle.None
            Text = "Probă — shell fără chenar"
        End If
        Me.StartPosition = startPosition
    End Sub

    ' Shown: the placement and the fit have both happened (OnLoad ran) -- report them.
    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            Dim scr As Screen = Screen.FromHandle(Handle)
            Dim refScreen As Screen = AppScreen.Reference(Me)
            Dim mouseScreen As Screen = Screen.FromPoint(Control.MousePosition)
            lblEcran.Text = "ecran: " & scr.DeviceName & If(scr.DeviceName = refScreen.DeviceName, " (= referința)", " (≠ referința " & refScreen.DeviceName & ")") &
                            If(scr.DeviceName <> mouseScreen.DeviceName, " · mouse-ul e pe " & mouseScreen.DeviceName, " · mouse-ul e tot aici")
            Dim captured As Size = ThemeFormFit.CapturedClientSize(Me)
            lblMarime.Text = "mărime: client " & ClientSize.Width & "×" & ClientSize.Height &
                             " · baza " & captured.Width & "×" & captured.Height &
                             " · cerere " & SizeText(ThemeFormFit.Demand(Me, pnlCard))
            lblPozitie.Text = "poziție: " & Location.X & "," & Location.Y & " · regula " & StartPosition.ToString() &
                              If(FormBorderStyle = FormBorderStyle.None, " · fără chenar", " · cu chenar")
            _log("probă deschisă: " & lblEcran.Text & " | " & lblMarime.Text & " | " & lblPozitie.Text)
        Catch ex As Exception
            GlobalErrorLog.Write("FormFitProbeDialog.OnShown", ex)
        End Try
    End Sub

    ' Shown modeless (the CenterParent-without-owner case) a DialogResult does not close the
    ' window; the button closes it by hand then.
    Private Sub btnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        If Not Modal Then Close()
    End Sub

    Private Shared Function SizeText(s As Size) As String
        Return s.Width & "×" & s.Height
    End Function

End Class
