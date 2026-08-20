Imports System.Collections.Generic
Imports System.Data
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Ecranul utilitarului de migrare, cu cei cinci pași în ordinea în care îi face
''' operatorul:
''' <list type="number">
''' <item><b>Sursa</b> — unitatea (din registrul AVACONT), anul, baza țintă de pe
''' MariaDB și fișierul FOREXE al anului, de pe stație.</item>
''' <item><b>Împingerea</b> — fișierul urcă pe server, în bucăți, cu amprentă.</item>
''' <item><b>Tabelele</b> — serverul numără rândurile fiecărui tabel din fișier;
''' cele fără rânduri rămân NEBIFATE, ca operatorul să nu pornească scrierea
''' pentru nimic.</item>
''' <item><b>Analiza</b> — serverul citește tabelele bifate și le măsoară. Nu
''' scrie nimic.</item>
''' <item><b>Rularea</b> — «Rulează» pornește doar dacă analiza n-a găsit nimic;
''' «Forțează rularea» pornește când singurele probleme sunt de integritate, și
''' atunci sare peste rândurile vinovate. Problemele de tip sau de dimensiune
''' opresc amândouă butoanele.</item>
''' </list>
'''
''' Un fișier FOREXE poate purta MAI MULTE unități; se scriu doar rândurile
''' unității bazei alese. Cine e unitatea aia se află din fișierul însuși
''' (FX_Angajamente poartă și <c>IdUnitate</c>, și <c>DC</c>; FX_Indicatori
''' poartă <c>IdUnitate</c>), deci nu mai există niciun fișier de rutare pe lângă.
'''
''' Migratorul NU deschide niciun fișier Access și nicio conexiune MariaDB: din
''' .NET nu se referă niciun driver Access — nici OleDb, nici ACE, nici COM.
''' </summary>
Public Class MigratorForm

    Private ReadOnly _client As MigrareApiClient
    Private _dcs As List(Of AvacontDc)
    Private _analizaId As String
    Private _raport As RaportAnaliza
    Private _busy As Boolean

    ''' <summary>
    ''' Clientul vine gata conectat din <see cref="ConnectForm"/>. Formularul îl
    ''' și eliberează la închidere — e ultimul care îl folosește.
    ''' </summary>
    Public Sub New(client As MigrareApiClient, baze As List(Of BazaInfo))
        If client Is Nothing Then Throw New ArgumentNullException(NameOf(client))
        _client = client

        InitializeComponent()

        Try
            If baze IsNot Nothing Then
                For Each b As BazaInfo In baze
                    cboBaza.Items.Add(b)
                Next
            End If

            _dcs = AvacontRegistry.ReadDcs()
            For Each dc As AvacontDc In _dcs
                cboDc.Items.Add(dc)
            Next

            If cboDc.Items.Count > 0 Then
                cboDc.SelectedIndex = 0
            Else
                lblStare.Text = "Registrul nu conține nicio unitate AVACONT pe stația asta. " &
                                "Completează manual căile fișierelor."
                cboAn.Text = Date.Today.Year.ToString()
            End If

            AcceptButton = btnAnalizeaza

        Catch ex As Exception
            ' Granita UI (constructor de formular): un throw ar impiedica deschiderea.
            GlobalErrorLog.Write("MigratorForm.New", ex)
            lblStare.Text = "Pornirea a întâmpinat o eroare: " & ex.Message
        End Try
    End Sub

    Private Async Sub MigratorForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Try
            Await ReciteseFisiereAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.MigratorForm_Shown", ex)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 1 — sursa
    ' =========================================================================

    Private Sub cboDc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboDc.SelectedIndexChanged
        Try
            Dim dc As AvacontDc = TryCast(cboDc.SelectedItem, AvacontDc)
            If dc Is Nothing Then Return

            lblUnitate.Text = If(String.IsNullOrWhiteSpace(dc.NumeUnitate), "—",
                                 dc.NumeUnitate & "   (CUI " & dc.CodFiscal & ")")

            ' Anii declarati in registru, pentru unitatea asta.
            Dim anCurent As String = cboAn.Text
            cboAn.Items.Clear()
            For Each an As String In dc.Ani
                cboAn.Items.Add(an)
            Next
            If cboAn.Items.Count > 0 Then
                Dim idx As Integer = cboAn.Items.IndexOf(anCurent)
                cboAn.SelectedIndex = If(idx >= 0, idx, cboAn.Items.Count - 1)
            ElseIf String.IsNullOrWhiteSpace(cboAn.Text) Then
                cboAn.Text = Date.Today.Year.ToString()
            End If

            ' Baza tinta cu acelasi nume, daca serverul o are.
            SelecteazaBaza(dc.Dc)
            SugereazaCalea(dc)
            ResetInventar("Unitatea s-a schimbat — citește din nou tabelele.")

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.cboDc_SelectedIndexChanged", ex)
            ShowError(ex)
        End Try
    End Sub

    Private Sub cboAn_TextChanged(sender As Object, e As EventArgs) Handles cboAn.TextChanged
        Try
            SugereazaCalea(TryCast(cboDc.SelectedItem, AvacontDc))
            ResetInventar("Anul s-a schimbat — citește din nou tabelele.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.cboAn_TextChanged", ex)
        End Try
    End Sub

    Private Sub cboBaza_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboBaza.SelectedIndexChanged
        Try
            ResetInventar("Baza țintă s-a schimbat — citește din nou tabelele.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.cboBaza_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub btnRasfoireFx_Click(sender As Object, e As EventArgs) Handles btnRasfoireFx.Click
        Try
            Dim ales As String = AlegeFisier(txtFx.Text)
            If ales IsNot Nothing Then txtFx.Text = ales
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnRasfoireFx_Click", ex)
            ShowError(ex)
        End Try
    End Sub

    Private Async Sub btnReciteste_Click(sender As Object, e As EventArgs) Handles btnReciteste.Click
        Try
            If _busy Then Return
            SetBusy(True, "Se recitesc bazele și fișierele de pe server…")

            Dim baze As List(Of BazaInfo) = Await _client.GetBazeAsync()
            Dim ales As BazaInfo = TryCast(cboBaza.SelectedItem, BazaInfo)
            cboBaza.Items.Clear()
            For Each b As BazaInfo In baze
                cboBaza.Items.Add(b)
            Next
            If ales IsNot Nothing Then SelecteazaBaza(ales.Nume)

            Await ReciteseFisiereAsync()
            lblStare.Text = "Serverul a fost recitit: " & baze.Count.ToString() & " baze."

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnReciteste_Click", ex)
            ShowError(ex)
        Finally
            SetBusy(False, Nothing)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 2 — impingerea fisierelor
    ' =========================================================================

    Private Async Sub btnImpinge_Click(sender As Object, e As EventArgs) Handles btnImpinge.Click
        Try
            If _busy Then Return

            Dim an As String = cboAn.Text.Trim()
            Dim dc As String = NumeBazaAleasa()
            If dc Is Nothing Then Return
            If Not ValideazaAn(an) Then Return

            Dim fx As String = txtFx.Text.Trim()
            If String.IsNullOrWhiteSpace(fx) OrElse Not File.Exists(fx) Then
                MessageBox.Show(Me, "Alege fișierul FOREXE al anului (FX_" & an & ".accdb).",
                                "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim confirm As DialogResult = MessageBox.Show(
                Me,
                "Se urcă pe server:" & Environment.NewLine &
                "  • " & Path.GetFileName(fx) & " → fx_" & an & "_" & dc.ToLowerInvariant() & ".accdb" &
                Environment.NewLine & Environment.NewLine &
                "Fișierul TREBUIE să fie fără parolă de bază de date; serverul nu poate " &
                "decripta. Continui?",
                "Migrare FX", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If confirm <> DialogResult.Yes Then Return

            SetBusy(True, "Se urcă fișierul…")
            prgPush.Value = 0

            Using cts As New CancellationTokenSource()
                AppendLog("Se urcă «" & fx & "».")
                Await _client.PushAsync(an, dc, fx, AddressOf OnPushProgress, cts.Token)
            End Using

            prgPush.Value = prgPush.Maximum
            Await ReciteseFisiereAsync()
            ResetAnaliza("Fișierul e pe server — se citesc tabelele lui.")
            SetBusy(False, Nothing)

            ' Inventarul urmeaza de la sine: fara el lista de tabele ar ramane
            ' goala, si operatorul n-ar avea ce bifa.
            Await InventariazaAsync()

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnImpinge_Click", ex)
            lblStare.Text = "Încărcarea s-a oprit."
            ShowError(ex)
        Finally
            SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub OnPushProgress(facute As Integer, total As Integer)
        Try
            If InvokeRequired Then
                BeginInvoke(New Action(Of Integer, Integer)(AddressOf OnPushProgress), facute, total)
                Return
            End If
            prgPush.Maximum = Math.Max(total, 1)
            prgPush.Value = Math.Min(facute, prgPush.Maximum)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.OnPushProgress", ex)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 3 — inventarul: ce tabele are fisierul, cu cate randuri
    ' =========================================================================

    Private Async Sub btnInventar_Click(sender As Object, e As EventArgs) Handles btnInventar.Click
        Await InventariazaAsync()
    End Sub

    ''' <summary>
    ''' Cere serverului numărul de rânduri al fiecărui tabel din fișierul deja
    ''' împins și umple lista cu bife: <b>bifate doar tabelele care CHIAR au
    ''' rânduri</b>. Câte dintre ele sunt ale unității alese se află abia la
    ''' analiză — pentru asta trebuie citit fiecare rând.
    ''' </summary>
    Private Async Function InventariazaAsync() As Task
        Try
            If _busy Then Return

            Dim baza As String = NumeBazaAleasa()
            If baza Is Nothing Then Return
            Dim an As String = cboAn.Text.Trim()
            If Not ValideazaAn(an) Then Return

            SetBusy(True, "Se citesc tabelele fișierului…")
            ResetAnaliza(Nothing)

            Dim jobId As String = Await _client.StartInventarAsync(baza, an, baza)
            Dim stare As StareLucrare = Await AsteaptaLucrareAsync(jobId)

            If stare.EsteEroare Then
                lblStare.Text = "Citirea tabelelor s-a oprit: " & stare.Eroare
                Return
            End If

            Dim inv As InventarFisier = MigrareApiClient.CitesteInventar(stare.Rezultat)
            UmpleTabele(inv)

            If inv Is Nothing Then
                lblStare.Text = "Serverul n-a întors inventarul fișierului."
            Else
                lblStare.Text = "Fișierul poartă unitățile " &
                                String.Join(", ", inv.ToateUnitatile) &
                                "; se scrie doar unitatea bazei «" & baza & "» (" &
                                String.Join(", ", inv.Unitati) & "). " &
                                "Bifează tabelele și rulează analiza."
            End If

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.InventariazaAsync", ex)
            lblStare.Text = "Citirea tabelelor s-a oprit."
            ShowError(ex)
        Finally
            SetBusy(False, Nothing)
        End Try
    End Function

    ' =========================================================================
    ' Regiunea 4 — analiza
    ' =========================================================================

    Private Async Sub btnAnalizeaza_Click(sender As Object, e As EventArgs) Handles btnAnalizeaza.Click
        Try
            If _busy Then Return

            Dim baza As String = NumeBazaAleasa()
            If baza Is Nothing Then Return
            Dim an As String = cboAn.Text.Trim()
            If Not ValideazaAn(an) Then Return

            Dim bifate As List(Of String) = TabeleBifate()
            If bifate Is Nothing Then Return

            SetBusy(True, "Analiză în curs…")
            txtJurnal.Clear()
            ResetAnaliza(Nothing)

            Dim jobId As String = Await _client.StartAnalizaAsync(baza, an, baza, bifate)
            Dim stare As StareLucrare = Await AsteaptaLucrareAsync(jobId)

            If stare.EsteEroare Then
                lblStare.Text = "Analiza s-a oprit: " & stare.Eroare
                Return
            End If

            _analizaId = jobId
            _raport = MigrareApiClient.CitesteRaport(stare.Rezultat)
            UmpleGrila(_raport)
            ActualizeazaTabeleDinRaport(_raport)
            ActualizeazaButoane()

            If _raport Is Nothing Then
                lblStare.Text = "Analiza s-a încheiat, dar serverul n-a întors un raport."
            ElseIf _raport.Curat Then
                lblStare.Text = "Analiză curată — «Rulează» poate porni."
            ElseIf _raport.AreBlocante Then
                lblStare.Text = "Analiza a găsit probleme BLOCANTE (tip / dimensiune / structură). " &
                                "Niciun buton nu pornește până nu sunt reparate la sursă."
            Else
                lblStare.Text = "Analiza a găsit doar probleme de integritate. «Forțează rularea» " &
                                "poate porni; rândurile vinovate vor fi sărite."
            End If

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnAnalizeaza_Click", ex)
            ResetAnaliza("Analiza s-a oprit.")
            ShowError(ex)
        Finally
            SetBusy(False, Nothing)
        End Try
    End Sub

    ' =========================================================================
    ' Regiunea 5 — rularea
    ' =========================================================================

    Private Async Sub btnRuleaza_Click(sender As Object, e As EventArgs) Handles btnRuleaza.Click
        Await RuleazaAsync(False)
    End Sub

    Private Async Sub btnForteaza_Click(sender As Object, e As EventArgs) Handles btnForteaza.Click
        Await RuleazaAsync(True)
    End Sub

    Private Async Function RuleazaAsync(fortat As Boolean) As Task
        Try
            If _busy Then Return
            If _raport Is Nothing OrElse String.IsNullOrEmpty(_analizaId) Then Return

            Dim an As String = cboAn.Text.Trim()
            Dim baza As String = _raport.Baza

            Dim bifate As List(Of String) = TabeleBifate()
            If bifate Is Nothing Then Return

            Dim mesaj As String =
                "Se scriu rândurile unității bazei «" & baza & "», din tabelele: " &
                String.Join(", ", bifate) & "." & Environment.NewLine &
                "Rândurile deja existente pe server se ADUC LA ZI din fișierul Access." &
                Environment.NewLine &
                "Rândurile altor unități din același fișier rămân neatinse."
            If fortat Then
                mesaj &= Environment.NewLine & Environment.NewLine &
                         "RULARE FORȚATĂ: rândurile cu probleme de integritate vor fi SĂRITE. " &
                         "Rămân în raport, dar nu ajung în baza de date."
            End If
            mesaj &= Environment.NewLine & Environment.NewLine & "Continui?"

            If MessageBox.Show(Me, mesaj, "Migrare FX",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            SetBusy(True, If(fortat, "Scriere forțată în curs…", "Scriere în curs…"))

            Dim jobId As String = Await _client.StartRulareAsync(_analizaId, an, baza, fortat, bifate)
            Dim stare As StareLucrare = Await AsteaptaLucrareAsync(jobId)

            If stare.EsteEroare Then
                lblStare.Text = "Scrierea s-a oprit: " & stare.Eroare
                Return
            End If

            lblStare.Text = "Scriere încheiată. Vezi jurnalul pentru numărătoare."
            ' Raportul de dinainte descrie o stare care s-a schimbat: nu-l lasam sa
            ' mai aprinda butoanele.
            ResetAnaliza(Nothing)

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.RuleazaAsync", ex)
            lblStare.Text = "Scrierea s-a oprit."
            ShowError(ex)
        Finally
            SetBusy(False, Nothing)
        End Try
    End Function

    ''' <summary>
    ''' Urmărește o lucrare până se încheie, aducând jurnalul pe măsură ce crește.
    ''' Interogare la o secundă: lucrările durează minute, nu milisecunde.
    ''' </summary>
    Private Async Function AsteaptaLucrareAsync(jobId As String) As Task(Of StareLucrare)
        Dim vazute As Integer = 0
        Dim stare As StareLucrare
        Do
            stare = Await _client.GetStareAsync(jobId, vazute)
            For Each line As String In stare.Jurnal
                AppendLog(line)
            Next
            vazute = stare.JurnalTotal

            If stare.EsteGata OrElse stare.EsteEroare Then Exit Do
            Await Task.Delay(1000)
        Loop
        Return stare
    End Function

    ' =========================================================================
    ' Ajutoare de ecran
    ' =========================================================================

    Private Async Function ReciteseFisiereAsync() As Task
        Dim fisiere As List(Of FisierInfo) = Await _client.GetFisiereAsync()
        If fisiere.Count = 0 Then
            lblFisiere.Text = "Pe server: niciun fișier."
            Return
        End If
        Dim total As Long = 0
        For Each f As FisierInfo In fisiere
            total += f.Octeti
        Next
        lblFisiere.Text = "Pe server: " & fisiere.Count.ToString() & " fișiere, " &
                          (total \ (1024L * 1024L)).ToString() & " MB."
    End Function

    ''' <summary>
    ''' Umple lista de tabele din inventar. Bifa se pune DOAR pe tabelele care
    ''' există în fișier și au măcar un rând — un tabel gol n-are ce actualiza.
    ''' </summary>
    Private Sub UmpleTabele(inv As InventarFisier)
        dgvTabele.Rows.Clear()
        If inv Is Nothing Then Return

        For Each t As TabelFisier In inv.Tabele
            Dim idx As Integer = dgvTabele.Rows.Add(
                t.Exista AndAlso t.Randuri > 0,
                t.Nume,
                If(t.Exista, t.Randuri.ToString(), "lipsește"),
                "")
            ' Un tabel care nu e in fisier nu se poate bifa deloc.
            dgvTabele.Rows(idx).Cells(0).ReadOnly = Not t.Exista
        Next
    End Sub

    ''' <summary>
    ''' După analiză se știe și câte dintre rânduri sunt ale unității alese.
    ''' Un tabel care n-are niciunul se DEBIFEAZĂ: are rânduri, dar nu ale noastre.
    ''' </summary>
    Private Sub ActualizeazaTabeleDinRaport(raport As RaportAnaliza)
        If raport Is Nothing Then Return

        For Each rand As DataGridViewRow In dgvTabele.Rows
            Dim nume As String = TryCast(rand.Cells(1).Value, String)
            If nume Is Nothing Then Continue For

            Dim numere As Integer() = Nothing
            If Not raport.PeTabel.TryGetValue(nume, numere) Then Continue For

            rand.Cells(3).Value = numere(1).ToString()
            If numere(1) = 0 Then rand.Cells(0).Value = False
        Next
    End Sub

    ''' <summary>
    ''' Tabelele bifate, sau <c>Nothing</c> (cu mesaj) dacă nu e bifat niciunul.
    ''' Lista goală nu se trimite ca «toate»: n-ar fi ce a cerut operatorul.
    ''' </summary>
    Private Function TabeleBifate() As List(Of String)
        Dim alese As New List(Of String)()
        For Each rand As DataGridViewRow In dgvTabele.Rows
            If CBool(rand.Cells(0).Value) Then
                Dim nume As String = TryCast(rand.Cells(1).Value, String)
                If Not String.IsNullOrEmpty(nume) Then alese.Add(nume)
            End If
        Next

        If alese.Count = 0 Then
            MessageBox.Show(Me,
                "Nu e bifat niciun tabel. Apasă «Citește tabelele», apoi bifează ce " &
                "vrei să se actualizeze.",
                "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        Return alese
    End Function

    ''' <summary>
    ''' Bifa se aplică la clic, nu la ieșirea din celulă: altfel operatorul apasă
    ''' «Analizează» și pleacă cu bifa veche.
    ''' </summary>
    Private Sub dgvTabele_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) _
            Handles dgvTabele.CurrentCellDirtyStateChanged
        Try
            If dgvTabele.IsCurrentCellDirty Then
                dgvTabele.CommitEdit(DataGridViewDataErrorContexts.Commit)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_CurrentCellDirtyStateChanged", ex)
        End Try
    End Sub

    Private Sub UmpleGrila(raport As RaportAnaliza)
        Dim dt As New DataTable()
        dt.Columns.Add("Clasă", GetType(String))
        dt.Columns.Add("Tabel", GetType(String))
        dt.Columns.Add("Coloană", GetType(String))
        dt.Columns.Add("Fel", GetType(String))
        dt.Columns.Add("Rânduri", GetType(Integer))
        dt.Columns.Add("Exemplu — cheie", GetType(String))
        dt.Columns.Add("Exemplu — ce nu e în regulă", GetType(String))
        dt.Columns.Add("Exemplu — valoare", GetType(String))

        If raport IsNot Nothing Then
            For Each c As Constatare In raport.Constatari
                Dim primul As ExempluConstatare = If(c.Exemple.Count > 0, c.Exemple(0), Nothing)
                dt.Rows.Add(c.Clasa, c.Tabel, c.Coloana, c.Fel, c.Numar,
                            If(primul Is Nothing, "", primul.Cheie),
                            If(primul Is Nothing, "", primul.Mesaj),
                            If(primul Is Nothing, "", primul.Valoare))
            Next
        End If

        dgvConstatari.DataSource = dt
    End Sub

    Private Sub dgvConstatari_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) _
            Handles dgvConstatari.CellDoubleClick
        Try
            If _raport Is Nothing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= _raport.Constatari.Count Then Return

            ' Rândul din grilă are un singur exemplu; restul se scriu în jurnal, unde
            ' încap și pot fi copiate.
            Dim c As Constatare = _raport.Constatari(e.RowIndex)
            AppendLog("— " & c.Tabel & "." & c.Coloana & " · " & c.Fel & " · " &
                      c.Numar.ToString() & " rânduri:")
            For Each x As ExempluConstatare In c.Exemple
                AppendLog("    cheia «" & x.Cheie & "»: " & x.Mesaj &
                          If(String.IsNullOrEmpty(x.Valoare), "", "  [" & x.Valoare & "]"))
            Next
            If c.Numar > c.Exemple.Count Then
                AppendLog("    … și încă " & (c.Numar - c.Exemple.Count).ToString() &
                          " rânduri de același fel.")
            End If

        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvConstatari_CellDoubleClick", ex)
        End Try
    End Sub

    Private Sub AppendLog(text As String)
        Try
            If txtJurnal.InvokeRequired Then
                txtJurnal.BeginInvoke(New Action(Of String)(AddressOf AppendLog), text)
                Return
            End If
            txtJurnal.AppendText(text & Environment.NewLine)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.AppendLog", ex)
        End Try
    End Sub

    Private Sub SelecteazaBaza(nume As String)
        For i As Integer = 0 To cboBaza.Items.Count - 1
            Dim b As BazaInfo = TryCast(cboBaza.Items(i), BazaInfo)
            If b IsNot Nothing AndAlso String.Equals(b.Nume, nume, StringComparison.OrdinalIgnoreCase) Then
                cboBaza.SelectedIndex = i
                Return
            End If
        Next
    End Sub

    Private Sub SugereazaCalea(dc As AvacontDc)
        If dc Is Nothing Then Return
        Dim an As String = cboAn.Text.Trim()
        If an.Length <> 4 Then Return

        ' Sugestii, nu adevaruri: caile reale pot diferi (fisiere per unitate).
        ' Nu le suprascriem daca operatorul a ales deja altceva care exista.
        Dim fx As String = AvacontRegistry.SuggestFxPath(dc, an)
        If Not String.IsNullOrEmpty(fx) AndAlso Not File.Exists(txtFx.Text.Trim()) Then
            txtFx.Text = fx
        End If
    End Sub

    Private Function AlegeFisier(curent As String) As String
        If Not String.IsNullOrWhiteSpace(curent) AndAlso File.Exists(curent) Then
            dlgFisier.InitialDirectory = Path.GetDirectoryName(curent)
            dlgFisier.FileName = Path.GetFileName(curent)
        End If
        If dlgFisier.ShowDialog(Me) = DialogResult.OK Then Return dlgFisier.FileName
        Return Nothing
    End Function

    ''' <summary>Baza bifată, sau Nothing (cu mesaj) dacă nu e aleasă niciuna.</summary>
    Private Function NumeBazaAleasa() As String
        Dim b As BazaInfo = TryCast(cboBaza.SelectedItem, BazaInfo)
        If b Is Nothing Then
            MessageBox.Show(Me, "Alege baza țintă de pe MariaDB.",
                            "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        If Not b.Complet Then
            Dim raspuns As DialogResult = MessageBox.Show(
                Me,
                "Baza «" & b.Nume & "» are doar " & b.TabeleFx.ToString() & " din cele 16 tabele FX_. " &
                "Migrarea NU creează tabele, deci se va opri la primul care lipsește." &
                Environment.NewLine & Environment.NewLine & "Continui oricum?",
                "Migrare FX", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If raspuns <> DialogResult.Yes Then Return Nothing
        End If
        Return b.Nume
    End Function

    Private Function ValideazaAn(an As String) As Boolean
        Dim n As Integer
        If an.Length = 4 AndAlso Integer.TryParse(an, n) Then Return True
        MessageBox.Show(Me, "Anul trebuie să aibă patru cifre.",
                        "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Return False
    End Function

    ''' <summary>
    ''' Inventarul descrie un fișier și o bază anume; când se schimbă vreuna, lista
    ''' de tabele nu mai are ce descrie și se golește. O bifă rămasă de la altă
    ''' bază ar fi exact bifa greșită.
    ''' </summary>
    Private Sub ResetInventar(mesaj As String)
        dgvTabele.Rows.Clear()
        ResetAnaliza(mesaj)
    End Sub

    Private Sub ResetAnaliza(mesaj As String)
        _analizaId = Nothing
        _raport = Nothing
        UmpleGrila(Nothing)
        ActualizeazaButoane()
        If mesaj IsNot Nothing Then lblStare.Text = mesaj
    End Sub

    ''' <summary>
    ''' Cele două butoane, exact după regula din analiză: «Rulează» doar pe un
    ''' raport curat, «Forțează» doar când nu există nicio constatare blocantă.
    ''' Serverul verifică din nou amândouă — interfața nu e singura pază.
    ''' </summary>
    Private Sub ActualizeazaButoane()
        Dim gata As Boolean = Not _busy AndAlso _raport IsNot Nothing
        btnRuleaza.Enabled = gata AndAlso _raport.PoateRula
        btnForteaza.Enabled = gata AndAlso _raport.PoateForta
    End Sub

    Private Sub SetBusy(busy As Boolean, mesaj As String)
        _busy = busy
        btnInventar.Enabled = Not busy
        btnAnalizeaza.Enabled = Not busy
        btnImpinge.Enabled = Not busy
        btnReciteste.Enabled = Not busy
        btnRasfoireFx.Enabled = Not busy
        dgvTabele.Enabled = Not busy
        cboDc.Enabled = Not busy
        cboAn.Enabled = Not busy
        cboBaza.Enabled = Not busy
        ActualizeazaButoane()
        If mesaj IsNot Nothing Then lblStare.Text = mesaj
        Cursor = If(busy, Cursors.WaitCursor, Cursors.Default)
    End Sub

    Private Sub ShowError(ex As Exception)
        MessageBox.Show(Me, ex.Message, "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Private Sub MigratorForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            _client.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.MigratorForm_FormClosed", ex)
        End Try
    End Sub

End Class
