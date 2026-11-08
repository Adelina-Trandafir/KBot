Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The operator's list of Adobe script alerts that K-BOT closes by itself (slice 0078-03, pass
''' 06): <see cref="AppSettings.AdobeTrappedAlerts"/>, read by <see cref="AdobeScriptAlertFilter"/>.
'''
''' <para><b>Why a list.</b> The forms use the same «Warning: JavaScript Window» box both for
''' script errors (noise, closed) and for telling the operator something («Validarea s-a
''' terminat cu succes!...»). The trap pressed OK on all of them, and the operator never saw
''' the second kind (operator, 24.09.2026). Now only what matches a line here is closed.</para>
'''
''' <para><b>One regular expression per line.</b> A broken line is refused at save with its
''' line number, so the store only ever holds valid patterns. The test box checks a pasted
''' message against the lines AS TYPED, before saving.</para>
''' </summary>
Public Class AdobeMesajeForm

    ''' <summary>One line for the status band after a save; empty when the dialog was abandoned.</summary>
    Public ReadOnly Property Rezumat As String
        Get
            Return _rezumat
        End Get
    End Property
    Private _rezumat As String = String.Empty

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub AdobeMesajeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ScrieRandurile(AppSettings.Current.AdobeTrappedAlerts)
            ActualizeazaProba()
            ActiveControl = txtReguli
        Catch ex As Exception
            ' UI boundary (Load): log and swallow -- a throw would take the opening down.
            GlobalErrorLog.Write("AdobeMesajeForm.AdobeMesajeForm_Load", ex)
        End Try
    End Sub

    Private Sub ScrieRandurile(patterns As IEnumerable(Of String))
        txtReguli.Text = String.Join(Environment.NewLine, If(patterns, Enumerable.Empty(Of String)()))
    End Sub

    ' The non-blank lines of the box, trimmed, in order.
    Private Function RanduriNegoale() As List(Of String)
        Return SplitLines(txtReguli.Text).Where(Function(l) l.Trim().Length > 0).Select(Function(l) l.Trim()).ToList()
    End Function

    Private Shared Function SplitLines(text As String) As String()
        Return If(text, String.Empty).Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ChrW(10))
    End Function

    ' "line N: reason" for every broken line (N counts every line, blank ones too, as the box shows).
    Private Function RanduriGresite() As List(Of String)
        Dim errors As New List(Of String)()
        Dim lines As String() = SplitLines(txtReguli.Text)
        For i As Integer = 0 To lines.Length - 1
            If lines(i).Trim().Length = 0 Then Continue For
            Dim why As String = AdobeScriptAlertFilter.CheckPattern(lines(i))
            If why.Length > 0 Then errors.Add($"rândul {i + 1} («{lines(i).Trim()}»): {why}")
        Next
        Return errors
    End Function

    Private Sub TxtReguli_TextChanged(sender As Object, e As EventArgs) Handles txtReguli.TextChanged
        Try
            ActualizeazaProba()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeMesajeForm.TxtReguli_TextChanged", ex)
        End Try
    End Sub

    Private Sub TxtProba_TextChanged(sender As Object, e As EventArgs) Handles txtProba.TextChanged
        Try
            ActualizeazaProba()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeMesajeForm.TxtProba_TextChanged", ex)
        End Try
    End Sub

    ' What the pasted message would get with the lines as typed now.
    Private Sub ActualizeazaProba()
        Dim p As ThemePalette = ThemeManager.Current?.Palette
        Dim errors As List(Of String) = RanduriGresite()
        If errors.Count > 0 Then
            lblRezultat.Text = "Expresie greșită — " & errors(0)
            If p IsNot Nothing Then lblRezultat.ForeColor = p.ErrorColor
            Return
        End If
        If String.IsNullOrWhiteSpace(txtProba.Text) Then
            lblRezultat.Text = $"{RanduriNegoale().Count} expresii în listă."
            If p IsNot Nothing Then lblRezultat.ForeColor = p.TextDimColor
            Return
        End If
        Dim rule As String = AdobeScriptAlertFilter.FirstMatch(txtProba.Text, AdobeScriptAlertFilter.Compile(RanduriNegoale()))
        If rule Is Nothing Then
            lblRezultat.Text = "Rămâne pe ecran — nicio expresie nu se potrivește."
            If p IsNot Nothing Then lblRezultat.ForeColor = p.SuccessColor
        Else
            lblRezultat.Text = $"Se închide automat — se potrivește cu «{rule}»."
            If p IsNot Nothing Then lblRezultat.ForeColor = p.WarningColor
        End If
    End Sub

    Private Sub BtnImplicite_Click(sender As Object, e As EventArgs) Handles btnImplicite.Click
        Try
            ScrieRandurile(AppSettings.DefaultAdobeTrappedAlerts())
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeMesajeForm.BtnImplicite_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Refuses a broken line (with its number); an empty list is asked about first, because it
    ''' means every script error stays on screen for the operator to close.
    ''' </summary>
    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Dim errors As List(Of String) = RanduriGresite()
            If errors.Count > 0 Then
                KBotMessage.Show(Me, "Lista nu a fost salvată. Corectează:" & vbLf & String.Join(vbLf, errors),
                                 "Mesaje de script Adobe", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim patterns As List(Of String) = RanduriNegoale()
            If patterns.Count = 0 AndAlso
               KBotMessage.Show(Me, "Lista este goală: niciun mesaj Adobe nu va mai fi închis automat, nici erorile de script. Salvezi așa?",
                                "Mesaje de script Adobe", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            Dim copie As AppSettings = AppSettings.Current.Clone()
            copie.AdobeTrappedAlerts = patterns
            copie.Save()

            _rezumat = $"Lista mesajelor Adobe închise automat a fost salvată ({patterns.Count} expresii). Se aplică de la următorul mesaj."
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeMesajeForm.BtnSalveaza_Click", ex)
            KBotMessage.Show(Me, "Lista nu a putut fi salvată: " & ex.Message, "Mesaje de script Adobe",
                             MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyMain.BackColor = p.SurfaceAltColor
            tlyCampuri.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblIntro, lblReguli, lblProba}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            lblRezultat.BackColor = Color.Transparent
            ButtonStyles.ApplySecondary(btnImplicite, scheme)
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            ActualizeazaProba()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeMesajeForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
