Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' The bank statements (slices 0057 / 0080 / 0095-02) -- the left icon of the tree footer
''' (direct download), «Meniu → Extrase» (the «Extrase de cont» window), and the download +
''' import shared with the Extrase view and that window (slice 0086 split out of KbotForm.vb).
''' </summary>
Partial Public Class KbotForm

    ' The «Extrase de cont» window opened from the header menu (slice 0095-02); one at a time.
    Private _extraseForm As ExtraseForm

    ''' <summary>
    ''' The LEFT icon of the tree footer downloads the SNM bank statements directly again
    ''' (slice 0095-02, operator 29.09.2026), as in slice 0057. The «Extrase de cont» window it
    ''' opened since 0080-03 moved to the header menu («Meniu → Extrase»).
    ''' </summary>
    Private Async Sub Tree_FooterLeftIconClicked(e As MouseEventArgs) Handles tree.FooterLeftIconClicked
        Try
            If Await DescarcaExtraseAsync(Me) Then ReloadExtraseView()
        Catch ex As Exception
            ' UI boundary (async void): log and swallow; DescarcaExtraseAsync told the operator.
            GlobalErrorLog.Write("MainForm.Tree_FooterLeftIconClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' «Meniu → Extrase» (slice 0095-02): the «Extrase de cont» window, standalone (modeless) like
    ''' the nomenclatoare windows; asking again brings the open one to the front. When it closes,
    ''' the open Extrase view reloads -- the window may have imported statements.
    ''' </summary>
    Private Sub DeschideExtrasele()
        Try
            If _extraseForm IsNot Nothing AndAlso Not _extraseForm.IsDisposed Then
                _extraseForm.Activate()
                Return
            End If
            _extraseForm = New ExtraseForm(_apiClient,
                                           Function(op) WithReauth(Of ExtraseInfo)(op),
                                           Function(owner) DescarcaExtraseAsync(owner))
            AddHandler _extraseForm.FormClosed,
                Sub()
                    _extraseForm = Nothing
                    ReloadExtraseView()
                End Sub
            _extraseForm.Show(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideExtrasele", ex)
            KBotMessage.Show(Me, "Fereastra extraselor de cont nu s-a putut deschide: " & ex.Message,
                            "Extrase de cont", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' The open Extrase view (if any) follows an import.
    Private Sub ReloadExtraseView()
        If IsDisposed Then Return
        Dim vedere As IAngajamentView = Nothing
        If _views.TryGetValue("extrase", vedere) Then TryCast(vedere, ExtraseView)?.Reincarca()
    End Sub

    ''' <summary>
    ''' Download the SNM bank statements from FOREXE and send them to the import (slice 0057;
    ''' shared since 0080-02 by the Extrase view and the «Extrase de cont» window). Returns
    ''' True when the import wrote at least one statement or operation, so the caller knows to
    ''' reload. Every failure is told to the operator here, over <paramref name="owner"/>.
    ''' </summary>
    ''' <remarks>
    ''' Two steps, like downloading one angajament: the robot brings the PDFs and unwraps
    ''' the XML from them, then the server reads the XML and writes FX_Extrase_F / _H /
    ''' FX_Extrase. The first step does not depend on the second -- if the import falls
    ''' over, the PDFs stay on disk and can be retried; the second run skips what is
    ''' already written.
    ''' </remarks>
    Friend Async Function DescarcaExtraseAsync(owner As IWin32Window) As Task(Of Boolean)
        Try
            Dim extrase As List(Of ExtrasDescarcat)
            busyBar.Running = True
            Try
                ' The last imported statement's date stops the walk through the inbox.
                ' Read through the same re-login net as the rest of the shell; if that
                ' read fails, the robot takes the whole inbox (slower, but correct).
                extrase = Await _controller.DownloadExtraseAsync(
                    Function(ct) WithReauth(Of Date?)(Function() _apiClient.GetUltimaDataExtrasAsync(ct)))
            Finally
                busyBar.Running = False
            End Try

            ' Nothing = the robot did not start, or it failed. LastFailure is empty only
            ' when the operator gave up themselves -- no box for that.
            If extrase Is Nothing Then
                ShowForexeFailure("Extrase de cont")
                Return False
            End If
            If extrase.Count = 0 Then
                ' Nothing new is not an error: no box (operator, 28.09.2026).
                OperatorLog.Write("MainForm.DescarcaExtraseAsync", "Extrase de cont",
                                  "Nu există extrase noi de descărcat.")
                Return False
            End If

            Return Await ImportaExtraseAsync(extrase, owner)
        Catch ex As Exception
            ' Called from UI handlers that cannot re-throw: log it and say why.
            GlobalErrorLog.Write("MainForm.DescarcaExtraseAsync", ex)
            KBotMessage.Show(owner, "Descărcarea extraselor de cont a eșuat: " & ex.Message,
                            "Extrase de cont", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' The statements' second step: the downloaded package goes to
    ''' <c>/api/forexe/extrase/import</c>, and the server reads the XML and writes the
    ''' tables. Its warnings (missing nomenclatoare) are SHOWN, not swallowed: without
    ''' those lookups every account header stays without a unit, and that has to be seen
    ''' now rather than months later.
    ''' </summary>
    Private Async Function ImportaExtraseAsync(extrase As List(Of ExtrasDescarcat),
                                               owner As IWin32Window) As Task(Of Boolean)
        Try
            Dim pentruServer As New List(Of ExtrasPentruImport)()
            For Each x As ExtrasDescarcat In extrase
                pentruServer.Add(New ExtrasPentruImport() With {
                    .PdfFisier = x.PdfFisier,
                    .DataFisier = x.DataFisier,
                    .XmlContent = x.XmlContent,
                    .CaleLocala = x.CaleLocala
                })
            Next

            Dim rezultat As ImportExtraseRezultat
            busyBar.Running = True
            Try
                rezultat = Await WithReauth(Of ImportExtraseRezultat)(
                    Function() _apiClient.ImportaExtraseAsync(pentruServer, CancellationToken.None))
            Finally
                busyBar.Running = False
            End Try

            Dim mesaj As New Text.StringBuilder()
            mesaj.AppendLine($"Extrase descărcate: {extrase.Count}.")
            mesaj.AppendLine($"Importate: {rezultat.Importate} · sărite (deja cunoscute): {rezultat.Sarite}.")
            mesaj.AppendLine($"Operațiuni scrise: {rezultat.Randuri}.")
            ' A clean import is SILENT (operator, 28.09.2026): the figures go to the operator
            ' log only. The box appears only when the server sent warnings.
            If rezultat.Avertismente.Count > 0 Then
                mesaj.AppendLine()
                mesaj.AppendLine("Avertismente de la server:")
                For Each a As String In rezultat.Avertismente
                    mesaj.AppendLine(" - " & a)
                Next
                KBotMessage.Show(owner, mesaj.ToString(), "Extrase de cont",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Else
                OperatorLog.Write("MainForm.ImportaExtraseAsync", "Extrase de cont", mesaj.ToString())
            End If
            Return rezultat.Importate > 0 OrElse rezultat.Randuri > 0
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ImportaExtraseAsync", ex)
            KBotMessage.Show(owner,
                "Importul extraselor a eșuat: " & ex.Message & Environment.NewLine &
                "PDF-urile au rămas pe disc — o nouă apăsare reia doar ce lipsește.",
                "Extrase de cont", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End Try
    End Function
End Class
