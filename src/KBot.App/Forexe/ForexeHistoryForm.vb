Option Strict On
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports GeneralClasses
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' ISTORICUL acțiunilor duse la capăt prin <c>KBot.Forexe</c> (felia 0040) — echivalentul lui
''' <c>HistoryForm</c> din proiectul FOREXE, adus pe regulile casei: o fereastră fără chenar
''' (<see cref="KBotShellForm"/>), controalele declarate în <c>.Designer.vb</c>, zero culori
''' scrise de mână și un <see cref="AdvancedTreeControl"/> în locul lui <c>TreeView</c>.
'''
''' <para><b>Ce arată.</b> O lucrare per rând: ora, numele workflow-ului și rezultatul
''' («Succes» / «Eroare» / «Anulat» / încă în execuție). Desfăcută, lucrarea își arată
''' REZULTATELE — variabilele întoarse de executor, tabelele ca dimensiune + cap de tabel.
''' Selectarea lucrării deschide în dreapta jurnalul ei complet; selectarea unui rezultat arată
''' valoarea lui.</para>
'''
''' <para><b>De unde vin datele.</b> Din <see cref="JobHistoryManager"/>, alimentat de
''' <c>ForexeRunner</c> (o intrare per <c>RunAsync</c>/<c>RunJobAsync</c>) și de
''' <c>RichTextBoxLogger</c>, care pune fiecare linie de jurnal în lucrarea deschisă. Deci
''' istoricul ține exact acțiunile ROBOTULUI — la fel ca jurnalul consolei, care de la felia
''' 0040 nu mai amestecă treburile shell-ului.</para>
'''
''' <para>Ca și consola, fereastra se creează O SINGURĂ DATĂ și nu se distruge: închiderea o
''' ascunde. Istoricul e în memorie și se pierde la ieșirea din K-BOT — de aceea există
''' «Exportă istoricul...».</para>
''' </summary>
Public Class ForexeHistoryForm

    ' Tag-ul nodurilor-copil: valoarea unui rezultat, cu numele lui.
    Private NotInheritable Class RezultatNod
        Public Property Nume As String
        Public Property Valoare As String
    End Class

    Public Sub New()
        InitializeComponent()
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' Iconița e cosmetică — absența ei nu împiedică deschiderea ferestrei.
            GlobalErrorLog.Write("ForexeHistoryForm.New", ex)
        End Try
    End Sub

    ''' <summary>
    ''' (Re)citește istoricul din memorie și reface arborele. Se cheamă la fiecare deschidere a
    ''' ferestrei, nu doar la construcție: fereastra trăiește cât aplicația, iar între două
    ''' deschideri au mai rulat lucrări.
    ''' </summary>
    Public Sub Reincarca()
        Try
            treeJobs.Clear()
            rtbDetalii.Clear()

            Dim lucrari As List(Of JobHistoryItem)
            SyncLock JobHistoryManager.History
                lucrari = New List(Of JobHistoryItem)(JobHistoryManager.History)
            End SyncLock

            If lucrari.Count = 0 Then
                ' Onest: nu inventăm rânduri și nu lăsăm o fereastră goală fără explicație.
                rtbDetalii.Text = "Nicio acțiune FOREXE în această sesiune." & Environment.NewLine &
                                  "Istoricul se umple la conectare și la fiecare descărcare."
                Return
            End If

            Dim p = ThemeManager.Current.Palette
            Dim index As Integer = 0
            ' Cea mai recentă sus — ordinea din HistoryForm.
            For i As Integer = lucrari.Count - 1 To 0 Step -1
                Dim job As JobHistoryItem = lucrari(i)
                Dim nod As AdvancedTreeControl.TreeItem =
                    treeJobs.AddItem($"job_{index}", CaptionLucrare(job), Nothing)
                nod.Tag = job
                nod.NodeForeColor = CuloareStare(job.Status, p)
                nod.Tooltip = TooltipLucrare(job)

                If job.OutputData IsNot Nothing Then
                    Dim k As Integer = 0
                    For Each kvp In job.OutputData
                        Dim copil As AdvancedTreeControl.TreeItem =
                            treeJobs.AddItem($"job_{index}_r{k}", kvp.Key, nod)
                        copil.Tag = New RezultatNod With {
                            .Nume = kvp.Key,
                            .Valoare = If(kvp.Value Is Nothing, String.Empty, kvp.Value.ToString())
                        }
                        k += 1
                    Next
                End If
                index += 1
            Next

            treeJobs.Invalidate()
        Catch ex As Exception
            ' Frontieră publică de UI: logăm și spunem, nu lăsăm o listă pe jumătate fără motiv.
            GlobalErrorLog.Write("ForexeHistoryForm.Reincarca", ex)
            MessageBox.Show(Me, "Istoricul nu s-a putut afișa: " & ex.Message, "Istoric FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' ── Compunerea rândurilor ────────────────────────────────────────────
    ' Ajutoare pure, atinse numai prin Reincarca (deja împachetat) — vezi regula transitivă.

    ' «~~~» e separatorul de caption al arborelui: stânga ora + numele, dreapta rezultatul.
    Private Shared Function CaptionLucrare(job As JobHistoryItem) As String
        Dim nume As String = If(String.IsNullOrWhiteSpace(job.JobName), "(fără nume)", job.JobName)
        Return $"[{job.Timestamp:HH:mm:ss}] {nume} ~~~ {ScurtStare(job.Status)}"
    End Function

    Private Shared Function ScurtStare(status As String) As String
        If String.IsNullOrEmpty(status) Then Return "—"
        If status.StartsWith("In Execuție", StringComparison.OrdinalIgnoreCase) Then Return "rulează"
        Return status
    End Function

    Private Shared Function TooltipLucrare(job As JobHistoryItem) As String
        Dim sb As New StringBuilder()
        sb.AppendLine($"Început: {job.Timestamp:dd.MM.yyyy HH:mm:ss}")
        If job.Durata.HasValue Then
            sb.AppendLine($"Durată: {job.Durata.Value.TotalSeconds:0.0} s")
        Else
            sb.AppendLine("Durată: încă în execuție")
        End If
        sb.Append($"Rezultate: {If(job.OutputData Is Nothing, 0, job.OutputData.Count)}")
        Return sb.ToString()
    End Function

    ' Culoarea stării vine din paletă, nu din constante: verde/roșu/portocaliu de temă.
    Private Shared Function CuloareStare(status As String, p As ThemePalette) As Color
        If String.IsNullOrEmpty(status) Then Return Color.Empty
        Dim s As String = status.ToLowerInvariant()
        If s.Contains("eroare") Then Return p.ErrorColor
        If s.Contains("anulat") Then Return p.WarningColor
        If s.Contains("succes") Then Return p.SuccessColor
        Return Color.Empty   ' încă în execuție — culoarea normală a arborelui
    End Function

    ' Recitește culoarea de stare a rândurilor de lucrare din paleta nouă. Atins numai prin
    ' OnThemeChanged, care e deja împachetat.
    Private Shared Sub RecoloreazaStari(noduri As List(Of AdvancedTreeControl.TreeItem), p As ThemePalette)
        For Each nod In noduri
            Dim job As JobHistoryItem = TryCast(nod.Tag, JobHistoryItem)
            If job IsNot Nothing Then nod.NodeForeColor = CuloareStare(job.Status, p)
            If nod.Children.Count > 0 Then RecoloreazaStari(nod.Children, p)
        Next
    End Sub

    ' ── Selecția ─────────────────────────────────────────────────────────

    Private Sub TreeJobs_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeJobs.NodeMouseUp
        Try
            If pNode Is Nothing Then Return

            Dim job As JobHistoryItem = TryCast(pNode.Tag, JobHistoryItem)
            If job IsNot Nothing Then
                AfiseazaLucrare(job)
                Return
            End If

            Dim rez As RezultatNod = TryCast(pNode.Tag, RezultatNod)
            If rez IsNot Nothing Then AfiseazaRezultat(rez)
        Catch ex As Exception
            ' Frontieră de UI (handler): logăm și înghițim.
            GlobalErrorLog.Write("ForexeHistoryForm.treeJobs_NodeMouseUp", ex)
        End Try
    End Sub

    Private Sub AfiseazaLucrare(job As JobHistoryItem)
        Dim sb As New StringBuilder()
        sb.AppendLine($"LUCRARE: {job.JobName}")
        sb.AppendLine($"Început:  {job.Timestamp:dd.MM.yyyy HH:mm:ss}")
        If job.FinishedAt.HasValue Then
            sb.AppendLine($"Încheiat: {job.FinishedAt.Value:dd.MM.yyyy HH:mm:ss} ({job.Durata.Value.TotalSeconds:0.0} s)")
        Else
            sb.AppendLine("Încheiat: —  (încă în execuție)")
        End If
        sb.AppendLine($"Rezultat: {job.Status}")
        sb.AppendLine(New String("─"c, 60))
        If Not String.IsNullOrWhiteSpace(job.InputData) Then
            sb.AppendLine("CERERE")
            sb.AppendLine(job.InputData)
            sb.AppendLine(New String("─"c, 60))
        End If
        sb.AppendLine("JURNAL")
        sb.Append(job.FullLog.ToString())
        rtbDetalii.Text = sb.ToString()
    End Sub

    Private Sub AfiseazaRezultat(rez As RezultatNod)
        Dim sb As New StringBuilder()
        sb.AppendLine($"REZULTAT: {rez.Nume}")
        sb.AppendLine(New String("─"c, 60))
        sb.Append(rez.Valoare)
        rtbDetalii.Text = sb.ToString()
    End Sub

    ' ── Butoane ──────────────────────────────────────────────────────────

    Private Sub BtnReimprospateaza_Click(sender As Object, e As EventArgs) Handles btnReimprospateaza.Click
        Reincarca()
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Hide()
    End Sub

    Private Sub BtnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            Using sfd As New SaveFileDialog()
                sfd.Title = "Exportă istoricul FOREXE"
                sfd.Filter = "Text (*.txt)|*.txt"
                sfd.FileName = $"IstoricForexe_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                If sfd.ShowDialog(Me) <> DialogResult.OK Then Return

                File.WriteAllText(sfd.FileName, ComponeExport(), New UTF8Encoding(True))
                MessageBox.Show(Me, "Istoricul a fost scris în:" & Environment.NewLine & sfd.FileName,
                                "Istoric FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Using
        Catch ex As Exception
            ' Frontieră de UI, dar cu I/O în spate: logăm detaliul și spunem operatorului de ce.
            GlobalErrorLog.Write("ForexeHistoryForm.btnExport_Click", ex)
            MessageBox.Show(Me, "Istoricul nu s-a putut scrie: " & ex.Message, "Istoric FOREXE",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' Tot istoricul, în ordinea în care s-a întâmplat (nu inversat ca în arbore: un fișier se
    ' citește de la început).
    Private Shared Function ComponeExport() As String
        Dim lucrari As List(Of JobHistoryItem)
        SyncLock JobHistoryManager.History
            lucrari = New List(Of JobHistoryItem)(JobHistoryManager.History)
        End SyncLock

        Dim sb As New StringBuilder()
        sb.AppendLine($"ISTORIC ACȚIUNI FOREXE — export {DateTime.Now:dd.MM.yyyy HH:mm:ss}")
        sb.AppendLine($"{lucrari.Count} lucrări")
        sb.AppendLine()
        For Each job In lucrari
            sb.AppendLine(New String("="c, 78))
            sb.AppendLine($"[{job.Timestamp:dd.MM.yyyy HH:mm:ss}] {job.JobName} — {job.Status}")
            If job.Durata.HasValue Then sb.AppendLine($"Durată: {job.Durata.Value.TotalSeconds:0.0} s")
            sb.AppendLine(New String("="c, 78))
            If Not String.IsNullOrWhiteSpace(job.InputData) Then
                sb.AppendLine("-- CERERE --")
                sb.AppendLine(job.InputData)
            End If
            If job.OutputData IsNot Nothing AndAlso job.OutputData.Count > 0 Then
                sb.AppendLine("-- REZULTATE --")
                For Each kvp In job.OutputData
                    sb.AppendLine($"  {kvp.Key}: {If(kvp.Value Is Nothing, String.Empty, kvp.Value.ToString())}")
                Next
            End If
            sb.AppendLine("-- JURNAL --")
            sb.AppendLine(job.FullLog.ToString())
            sb.AppendLine()
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Închiderea ASCUNDE fereastra, ca la consolă: e creată o singură dată de shell și
    ''' redeschisă de câte ori o cere operatorul.
    ''' </summary>
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Hide()
                Return
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeHistoryForm.OnFormClosing", ex)
        End Try
    End Sub

    ' ── Temă ─────────────────────────────────────────────────────────────

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim schema = ThemeManager.Current
            Dim p = schema.Palette

            ' Fundalul formularului ESTE conturul de 1px al ferestrei (vezi ForexeConsoleForm).
            BackColor = p.BorderColor
            ' Bara despărțitoare a lui SplitContainer se vede prin fundalul lui.
            splitMain.BackColor = p.BorderColor

            rtbDetalii.BackColor = p.InputBackColor
            rtbDetalii.ForeColor = p.TextColor

            ButtonStyles.ApplySecondary(btnReimprospateaza, schema)
            ButtonStyles.ApplySecondary(btnExport, schema)
            ButtonStyles.ApplyPrimary(btnInchide, schema)

            ' Culorile de stare ale rândurilor vin din paletă, deci se recitesc — dar NU refacem
            ' arborele: o reîncărcare ar arunca rândul selectat și jurnalul deschis în dreapta,
            ' adică ar pedepsi operatorul pentru că a schimbat tema.
            RecoloreazaStari(treeJobs.Items, p)
            treeJobs.Invalidate()
        Catch ex As Exception
            ' Frontieră de UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("ForexeHistoryForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
