Imports System.Collections.Generic
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Ecranul utilitarului de migrare, cu cei cinci pasi in ordinea in care ii face
''' operatorul:
''' <list type="number">
''' <item><b>Sursa</b> — unitatea (din registrul AVACONT), anul, baza tinta de pe
''' MariaDB si fisierul FOREXE al anului, de pe statie.</item>
''' <item><b>Impingerea</b> — fisierul urca pe server, in bucati, cu amprenta.</item>
''' <item><b>Tabelele</b> — serverul numara randurile fiecarui tabel din fisier;
''' cele fara randuri raman NEBIFATE, ca operatorul sa nu porneasca scrierea
''' pentru nimic. Ordinea din lista e ORDINEA DE SCRIERE (sageti sau tragere cu
''' mouse-ul), iar pentru tabelul ales se vad coloanele lui: doar cele bifate
''' calatoresc — cheile primare mereu, coloanele absente din tinta pornesc
''' nebifate. Fila <b>Corelatii coloane</b> spune, tot pentru tabelul ales, in
''' CE coloana de pe MariaDB ajunge fiecare coloana din Access.</item>
''' <item><b>Analiza</b> — serverul citeste tabelele bifate si le masoara. Nu
''' scrie nimic.</item>
''' <item><b>Rularea</b> — «Ruleaza» porneste doar daca analiza n-a gasit nimic;
''' «Forteaza rularea» porneste cand singurele probleme sunt de integritate, si
''' atunci sare peste randurile vinovate. Problemele de tip sau de dimensiune
''' opresc amandoua butoanele.</item>
''' </list>
'''
''' Un fisier FOREXE poate purta MAI MULTE unitati; se scriu doar randurile
''' unitatii bazei alese. Cine e unitatea aia se afla din fisierul insusi
''' (FX_Angajamente poarta si <c>IdUnitate</c>, si <c>DC</c>; FX_Indicatori
''' poarta <c>IdUnitate</c>), deci nu mai exista niciun fisier de rutare pe langa.
'''
''' Migratorul NU deschide niciun fisier Access si nicio conexiune MariaDB: din
''' .NET nu se refera niciun driver Access — nici OleDb, nici ACE, nici COM.
''' </summary>
Public Class MigratorForm

    Private ReadOnly _client As MigrareApiClient
    Private _dcs As List(Of AvacontDc)
    Private _analizaId As String
    Private _raport As RaportAnaliza
    Private _busy As Boolean

    ''' <summary>Coloanele fiecarui tabel din inventar, cu bifele operatorului.</summary>
    Private ReadOnly _coloane As New Dictionary(Of String, List(Of ColoanaFisier))(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Coloanele pe care le are fiecare tabel PE MARIADB (tinta corelatiei).</summary>
    Private ReadOnly _coloaneTinta As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

    ''' <summary>
    ''' Lista de tabele, in ordinea de scriere — MODELUL ei, nu grila.
    ''' <see cref="KBotDataView"/> nu-si muta randurile singura (modelul e al
    ''' gazdei), deci rearanjarea se face aici, iar grila se reumple din lista.
    ''' </summary>
    Private ReadOnly _tabele As New List(Of RandTabel)()

    ' Tragerea unui rand din lista de tabele: de unde a pornit si pragul de la
    ' care un clic devine tragere (altfel bifarea s-ar transforma in drag).
    Private _dragIndex As Integer = -1
    Private _dragStart As Rectangle = Rectangle.Empty

    ''' <summary>Ce se alege in «Se scrie in» pentru o coloana fara pereche pe MariaDB.</summary>
    Private Const FaraTinta As String = "(nu se scrie)"

    ''' <summary>
    ''' Un rand al listei de tabele: ce a spus inventarul, plus bifa operatorului
    ''' si numaratoarea adusa de analiza. POCO.
    ''' </summary>
    Private NotInheritable Class RandTabel
        Public Property Nume As String
        Public Property Exista As Boolean
        Public Property Randuri As Integer
        Public Property Bifat As Boolean
        Public Property AleUnitatii As String
    End Class

    ''' <summary>
    ''' Clientul vine gata conectat din <see cref="ConnectForm"/>. Formularul il
    ''' si elibereaza la inchidere — e ultimul care il foloseste.
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
    ''' Cere serverului numarul de randuri al fiecarui tabel din fisierul deja
    ''' impins si umple lista cu bife: <b>bifate doar tabelele care CHIAR au
    ''' randuri</b>. Cate dintre ele sunt ale unitatii alese se afla abia la
    ''' analiza — pentru asta trebuie citit fiecare rand.
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

            Dim jobId As String = Await _client.StartAnalizaAsync(baza, an, baza, bifate,
                                                                 ColoaneAlese(bifate),
                                                                 CorelatiiAlese(bifate))
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
            Dim inlocuieste As Boolean = chkInlocuieste.Checked

            Dim mesaj As String =
                "Se scriu rândurile unității bazei «" & baza & "», din tabelele (în ordinea din listă): " &
                String.Join(", ", bifate) & "." & Environment.NewLine &
                If(inlocuieste,
                   "ÎNLOCUIEȘTE TOT: datele existente din tabelele bifate se ȘTERG întâi de pe " &
                   "server, apoi se scriu cele din fișier. Totul într-o singură tranzacție — " &
                   "la orice eroare, baza rămâne exact cum era.",
                   "Rândurile deja existente pe server se ADUC LA ZI din fișierul Access.") &
                Environment.NewLine &
                "Rândurile altor unități din același fișier rămân neatinse."
            If fortat Then
                mesaj &= Environment.NewLine & Environment.NewLine &
                         "RULARE FORȚATĂ: rândurile cu probleme de integritate vor fi SĂRITE. " &
                         "Rămân în raport, dar nu ajung în baza de date."
            End If
            mesaj &= Environment.NewLine & Environment.NewLine & "Continui?"

            Dim pictograma As MessageBoxIcon =
                If(inlocuieste, MessageBoxIcon.Warning, MessageBoxIcon.Question)
            If MessageBox.Show(Me, mesaj, "Migrare FX",
                               MessageBoxButtons.YesNo, pictograma) <> DialogResult.Yes Then
                Return
            End If

            SetBusy(True, If(fortat, "Scriere forțată în curs…", "Scriere în curs…"))

            Dim jobId As String = Await _client.StartRulareAsync(_analizaId, an, baza, fortat,
                                                                 bifate, inlocuieste)
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
    ''' Urmareste o lucrare pana se incheie, aducand jurnalul pe masura ce creste.
    ''' Interogare la o secunda: lucrarile dureaza minute, nu milisecunde.
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
    ''' exista in fisier si au macar un rand — un tabel gol n-are ce actualiza.
    ''' Coloanele fiecarui tabel se pastreaza pe formular, cu bifele lor de
    ''' pornire (cheile mereu; restul doar daca exista si pe MariaDB).
    ''' </summary>
    Private Sub UmpleTabele(inv As InventarFisier)
        _tabele.Clear()
        _coloane.Clear()
        _coloaneTinta.Clear()
        dgvColoane.ClearRows()
        dgvCorelatii.ClearRows()
        If inv IsNot Nothing Then
            For Each t As TabelFisier In inv.Tabele
                _tabele.Add(New RandTabel() With {
                    .Nume = t.Nume,
                    .Exista = t.Exista,
                    .Randuri = t.Randuri,
                    .Bifat = t.Exista AndAlso t.Randuri > 0,
                    .AleUnitatii = ""
                })
                _coloane(t.Nume) = New List(Of ColoanaFisier)(t.Coloane)
                _coloaneTinta(t.Nume) = New List(Of String)(t.ColoaneTinta)
            Next
        End If

        ReumpleTabele(0)
    End Sub

    ''' <summary>
    ''' Scrie <see cref="_tabele"/> in grila si pune selectia pe randul cerut.
    ''' Reumplerea e drumul prin care lista isi schimba ORDINEA: grila nu muta
    ''' randuri, modelul da.
    ''' </summary>
    Private Sub ReumpleTabele(selectat As Integer)
        dgvTabele.BeginUpdate()
        Try
            dgvTabele.ClearRows()
            For Each t As RandTabel In _tabele
                Dim rand As KBotDataRow = dgvTabele.AddRow()
                rand("bifa") = t.Bifat
                rand("tabel") = t.Nume
                rand("randuri") = If(t.Exista, t.Randuri.ToString(), "lipsește")
                rand("ale_unitatii") = t.AleUnitatii
                rand.Tag = t
            Next
        Finally
            dgvTabele.EndUpdate()
        End Try

        If _tabele.Count = 0 Then
            UmpleColoane(Nothing)
            Return
        End If
        dgvTabele.CurrentRowIndex = Math.Max(0, Math.Min(selectat, _tabele.Count - 1))
        UmpleColoane(TabelCurent())
    End Sub

    ''' <summary>Numele tabelului de pe randul selectat, sau Nothing.</summary>
    Private Function TabelCurent() As String
        Dim idx As Integer = dgvTabele.CurrentRowIndex
        If idx < 0 OrElse idx >= _tabele.Count Then Return Nothing
        Return _tabele(idx).Nume
    End Function

    ''' <summary>
    ''' Un tabel care nu e in fisier nu se poate bifa deloc: bifa lui se stinge
    ''' aici, la pictare, fiindca activarea unei CELULE nu e o proprietate de
    ''' coloana — vine din <c>CellFormatting</c>, exact ca la grila de coloane.
    ''' </summary>
    Private Sub dgvTabele_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) _
            Handles dgvTabele.CellFormatting
        Try
            If Not String.Equals(e.ColumnKey, "bifa", StringComparison.Ordinal) Then Return
            Dim t As RandTabel = TryCast(e.Row.Tag, RandTabel)
            If t IsNot Nothing AndAlso Not t.Exista Then e.Enabled = False
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_CellFormatting", ex)
        End Try
    End Sub

    ''' <summary>Bifa din grila se scrie inapoi in modelul listei de tabele.</summary>
    Private Sub dgvTabele_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
            Handles dgvTabele.CellValueChanged
        Try
            If Not String.Equals(e.ColumnKey, "bifa", StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= _tabele.Count Then Return
            _tabele(e.RowIndex).Bifat = CBool(e.NewValue)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Arata coloanele tabelului ales: bifa spune ce calatoreste. Cheia primara
    ''' e mereu bifata si nu se poate atinge; o coloana absenta din baza tinta e
    ''' scrisa apasat, ca sa se vada de ce porneste nebifata.
    ''' </summary>
    Private Sub UmpleColoane(tabel As String)
        Try
            dgvColoane.BeginUpdate()
            Try
                dgvColoane.ClearRows()
                If tabel Is Nothing OrElse Not _coloane.ContainsKey(tabel) Then
                    lblColoane.Text = "Coloane:"
                    Return
                End If

                lblColoane.Text = "Coloane — " & tabel & ":"
                For Each c As ColoanaFisier In _coloane(tabel)
                    Dim rand As KBotDataRow = dgvColoane.AddRow()
                    rand("bifa") = c.Aleasa
                    rand("nume") = c.Nume
                    rand("stare") = If(c.Cheie, "cheie", If(c.InBaza, "da", "LIPSEȘTE"))
                    rand.Tag = c
                Next
            Finally
                dgvColoane.EndUpdate()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.UmpleColoane", ex)
        Finally
            ' Cele doua file descriu ACELASI tabel: se schimba impreuna.
            UmpleCorelatii(tabel)
        End Try
    End Sub

    Private Sub dgvTabele_SelectionChanged(sender As Object, e As EventArgs) _
            Handles dgvTabele.SelectionChanged
        Try
            UmpleColoane(TabelCurent())
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Cheia primara calatoreste intotdeauna: serverul o adauga oricum, deci bifa
    ''' ei nu e o alegere si nu se poate schimba.
    ''' </summary>
    Private Sub dgvColoane_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) _
            Handles dgvColoane.CellFormatting
        Try
            If Not String.Equals(e.ColumnKey, "bifa", StringComparison.Ordinal) Then Return
            Dim c As ColoanaFisier = TryCast(e.Row.Tag, ColoanaFisier)
            If c IsNot Nothing AndAlso c.Cheie Then e.Enabled = False
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvColoane_CellFormatting", ex)
        End Try
    End Sub

    ''' <summary>Bifa din grila se scrie inapoi in modelul coloanelor.</summary>
    Private Sub dgvColoane_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
            Handles dgvColoane.CellValueChanged
        Try
            If Not String.Equals(e.ColumnKey, "bifa", StringComparison.Ordinal) Then Return
            Dim c As ColoanaFisier = TryCast(dgvColoane.Rows(e.RowIndex).Tag, ColoanaFisier)
            If c Is Nothing Then Return
            c.Aleasa = c.Cheie OrElse CBool(e.NewValue)
            ' O coloana debifata nu mai calatoreste, deci corelatia ei nu mai are
            ' ce spune — se vede in coloana «Stare» din fila de dincolo.
            For i As Integer = 0 To dgvCorelatii.RowCount - 1
                If ReferenceEquals(dgvCorelatii.Rows(i).Tag, c) Then
                    dgvCorelatii("stare", i) = StareCorelatie(c)
                    Exit For
                End If
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvColoane_CellValueChanged", ex)
        End Try
    End Sub

    ' =========================================================================
    ' Fila «Corelatii coloane» — in ce coloana de pe MariaDB ajunge fiecare
    ' coloana din Access
    ' =========================================================================

    ''' <summary>
    ''' Arata corelatiile tabelului ales. Serverul le propune (unu-la-unu dupa
    ''' nume, cu exceptia perechii clasificatiilor: Access <c>IdClsf</c> ▸ MariaDB
    ''' <c>IdClsfAcc</c>, Access <c>IdClsfPY</c> ▸ MariaDB <c>IdClsf</c>), iar
    ''' operatorul le poate schimba rand cu rand. Lista din care alege sunt
    ''' COLOANELE TINTEI, plus «(nu se scrie)».
    ''' </summary>
    Private Sub UmpleCorelatii(tabel As String)
        Try
            dgvCorelatii.BeginUpdate()
            Try
                dgvCorelatii.ClearRows()
                If tabel Is Nothing OrElse Not _coloane.ContainsKey(tabel) Then
                    lblCorelatii.Text = "Corelații:"
                    Return
                End If

                lblCorelatii.Text = "Corelații — " & tabel & " ▸ MariaDB:"

                Dim optiuni As New List(Of Object)() From {FaraTinta}
                Dim tinte As List(Of String) = Nothing
                If _coloaneTinta.TryGetValue(tabel, tinte) Then
                    For Each t As String In tinte
                        optiuni.Add(t)
                    Next
                End If
                dgvCorelatii.Column("tinta").ComboItems = optiuni

                For Each c As ColoanaFisier In _coloane(tabel)
                    Dim rand As KBotDataRow = dgvCorelatii.AddRow()
                    rand("access") = c.Nume
                    rand("tinta") = If(String.IsNullOrEmpty(c.Tinta), FaraTinta, c.Tinta)
                    rand("implicit") = If(String.IsNullOrEmpty(c.TintaImplicita), FaraTinta, c.TintaImplicita)
                    rand("stare") = StareCorelatie(c)
                    rand.Tag = c
                Next
            Finally
                dgvCorelatii.EndUpdate()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.UmpleCorelatii", ex)
        End Try
    End Sub

    ''' <summary>Ce se scrie in coloana «Stare» a unei corelatii.</summary>
    Private Shared Function StareCorelatie(c As ColoanaFisier) As String
        If c.Cheie Then Return "cheie primară"
        If Not c.Aleasa Then Return "coloană debifată"
        If String.IsNullOrEmpty(c.Tinta) Then Return "fără pereche"
        If Not String.Equals(c.Tinta, c.TintaImplicita, StringComparison.OrdinalIgnoreCase) Then
            Return "schimbată de tine"
        End If
        If Not String.Equals(c.Tinta, c.Nume, StringComparison.OrdinalIgnoreCase) Then
            Return "corelare încrucișată"
        End If
        Return ""
    End Function

    ''' <summary>
    ''' Doua coloane din Access nu pot merge in ACEEASI coloana de pe MariaDB —
    ''' una dintre valori s-ar pierde, si nu se poate spune care. Serverul refuza
    ''' si el, dar aici operatorul afla pe loc, nu dupa ce porneste analiza.
    ''' </summary>
    Private Sub dgvCorelatii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) _
            Handles dgvCorelatii.CellValidating
        Try
            If Not String.Equals(e.ColumnKey, "tinta", StringComparison.Ordinal) Then Return
            Dim tinta As String = If(TryCast(e.ProposedValue, String), String.Empty).Trim()
            If tinta.Length = 0 OrElse String.Equals(tinta, FaraTinta, StringComparison.Ordinal) Then
                e.ProposedValue = FaraTinta
                Return
            End If

            ' Editorul combo se poate si TASTA, nu doar alege: un nume care nu e al
            ' unei coloane de pe MariaDB se refuza aici. Serverul ar ignora-o tacut,
            ' iar operatorul ar ramane cu o corelatie care nu face nimic.
            Dim tabel As String = TabelCurent()
            Dim tinte As List(Of String) = Nothing
            If tabel Is Nothing OrElse Not _coloaneTinta.TryGetValue(tabel, tinte) Then Return
            Dim exact As String = Nothing
            For Each t As String In tinte
                If String.Equals(t, tinta, StringComparison.OrdinalIgnoreCase) Then
                    exact = t
                    Exit For
                End If
            Next
            If exact Is Nothing Then
                e.Cancel = True
                MessageBox.Show(Me,
                    "Baza «" & tabel & "» n-are nicio coloană «" & tinta & "». " &
                    "Alege una din listă, sau «" & FaraTinta & "».",
                    "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            ' Pe MariaDB se scrie cu ortografia EXACTA a tintei.
            tinta = exact
            e.ProposedValue = exact

            For i As Integer = 0 To dgvCorelatii.RowCount - 1
                If i = e.RowIndex Then Continue For
                Dim alta As String = TryCast(dgvCorelatii("tinta", i), String)
                If Not String.Equals(alta, tinta, StringComparison.OrdinalIgnoreCase) Then Continue For

                e.Cancel = True
                MessageBox.Show(Me,
                    "Coloana «" & tinta & "» de pe MariaDB e deja corelată cu «" &
                    CStr(dgvCorelatii("access", i)) & "» din Access. O coloană a " &
                    "țintei poate primi o singură coloană din Access.",
                    "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvCorelatii_CellValidating", ex)
        End Try
    End Sub

    ''' <summary>Corelatia aleasa se scrie inapoi in modelul coloanelor.</summary>
    Private Sub dgvCorelatii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
            Handles dgvCorelatii.CellValueChanged
        Try
            If Not String.Equals(e.ColumnKey, "tinta", StringComparison.Ordinal) Then Return
            Dim c As ColoanaFisier = TryCast(dgvCorelatii.Rows(e.RowIndex).Tag, ColoanaFisier)
            If c Is Nothing Then Return

            Dim tinta As String = If(TryCast(e.NewValue, String), String.Empty)
            c.Tinta = If(String.Equals(tinta, FaraTinta, StringComparison.Ordinal), String.Empty, tinta)
            dgvCorelatii("stare", e.RowIndex) = StareCorelatie(c)
            ResetAnaliza("Corelațiile s-au schimbat — analizează din nou.")
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvCorelatii_CellValueChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Corelatiile pentru analiza, pe tabel. Se trimit INTREGI pentru fiecare
    ''' tabel bifat — si cele nemodificate: serverul le-a propus, dar analiza
    ''' trebuie sa masoare exact ce vede operatorul pe ecran, nu ce ar recalcula
    ''' el singur.
    ''' </summary>
    Private Function CorelatiiAlese(bifate As IEnumerable(Of String)) As Dictionary(Of String, Dictionary(Of String, String))
        Dim alese As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.OrdinalIgnoreCase)
        For Each tabel As String In bifate
            Dim lista As List(Of ColoanaFisier) = Nothing
            If Not _coloane.TryGetValue(tabel, lista) OrElse lista.Count = 0 Then Continue For

            Dim harta As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each c As ColoanaFisier In lista
                harta(c.Nume) = If(c.Tinta, String.Empty)
            Next
            alese(tabel) = harta
        Next
        Return alese
    End Function

    ''' <summary>
    ''' Coloanele alese, pe tabel, pentru analiza. Un tabel cu TOATE coloanele
    ''' bifate nu se trimite deloc — «toate» e si intelesul lipsei — deci
    ''' dictionarul poarta doar tabelele unde operatorul chiar a debifat ceva.
    ''' </summary>
    Private Function ColoaneAlese(bifate As IEnumerable(Of String)) As Dictionary(Of String, List(Of String))
        Dim alese As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
        For Each tabel As String In bifate
            Dim lista As List(Of ColoanaFisier) = Nothing
            If Not _coloane.TryGetValue(tabel, lista) OrElse lista.Count = 0 Then Continue For

            Dim toate As Boolean = True
            Dim nume As New List(Of String)()
            For Each c As ColoanaFisier In lista
                If c.Aleasa OrElse c.Cheie Then
                    nume.Add(c.Nume)
                Else
                    toate = False
                End If
            Next
            If Not toate Then alese(tabel) = nume
        Next
        Return alese
    End Function

    ' =========================================================================
    ' Ordinea tabelelor: sageti + tragere cu mouse-ul
    ' =========================================================================

    Private Sub btnSus_Click(sender As Object, e As EventArgs) Handles btnSus.Click
        Try
            MutaTabelCurent(-1)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnSus_Click", ex)
        End Try
    End Sub

    Private Sub btnJos_Click(sender As Object, e As EventArgs) Handles btnJos.Click
        Try
            MutaTabelCurent(1)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.btnJos_Click", ex)
        End Try
    End Sub

    Private Sub MutaTabelCurent(pas As Integer)
        Dim idx As Integer = dgvTabele.CurrentRowIndex
        If idx < 0 Then Return
        MutaRand(idx, idx + pas)
    End Sub

    ''' <summary>
    ''' Muta un rand al listei de tabele pe alta pozitie. Ordinea din lista e
    ''' ORDINEA DE SCRIERE — parintii trebuie sa ramana inaintea copiilor, iar
    ''' asta e pe mana operatorului, exact cum a cerut. Se muta MODELUL, apoi
    ''' grila se reumple din el.
    ''' </summary>
    Private Sub MutaRand(deLa As Integer, la As Integer)
        If deLa < 0 OrElse deLa >= _tabele.Count Then Return
        If la < 0 OrElse la >= _tabele.Count OrElse la = deLa Then Return

        Dim rand As RandTabel = _tabele(deLa)
        _tabele.RemoveAt(deLa)
        _tabele.Insert(la, rand)
        ReumpleTabele(la)
    End Sub

    Private Sub dgvTabele_MouseDown(sender As Object, e As MouseEventArgs) _
            Handles dgvTabele.MouseDown
        Try
            Dim rowIndex As Integer = dgvTabele.RowIndexAt(e.Location)
            ' Din celula cu bifa nu se porneste tragerea: acolo clicul E bifa.
            ' Latimea coloanei e LOGICA (px la 96 dpi), iar `e.X` e in pixeli de
            ' ecran — de aceea se scaleaza, din aceeasi sursa ca grila.
            Dim latimeBifa As Integer =
                dgvTabele.Column("bifa").Width * dgvTabele.DeviceDpi \ 96
            Dim peBifa As Boolean = rowIndex >= 0 AndAlso e.X < latimeBifa
            If rowIndex >= 0 AndAlso Not peBifa Then
                _dragIndex = rowIndex
                Dim prag As Size = SystemInformation.DragSize
                _dragStart = New Rectangle(New Point(e.X - prag.Width \ 2,
                                                     e.Y - prag.Height \ 2), prag)
            Else
                _dragIndex = -1
                _dragStart = Rectangle.Empty
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_MouseDown", ex)
        End Try
    End Sub

    Private Sub dgvTabele_MouseMove(sender As Object, e As MouseEventArgs) _
            Handles dgvTabele.MouseMove
        Try
            If e.Button <> MouseButtons.Left OrElse _dragIndex < 0 Then Return
            If _dragStart <> Rectangle.Empty AndAlso _dragStart.Contains(e.X, e.Y) Then Return
            dgvTabele.DoDragDrop(_dragIndex, DragDropEffects.Move)
            _dragIndex = -1
            _dragStart = Rectangle.Empty
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_MouseMove", ex)
        End Try
    End Sub

    Private Sub dgvTabele_DragOver(sender As Object, e As DragEventArgs) _
            Handles dgvTabele.DragOver
        Try
            e.Effect = If(e.Data.GetDataPresent(GetType(Integer)),
                          DragDropEffects.Move, DragDropEffects.None)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_DragOver", ex)
        End Try
    End Sub

    Private Sub dgvTabele_DragDrop(sender As Object, e As DragEventArgs) _
            Handles dgvTabele.DragDrop
        Try
            If Not e.Data.GetDataPresent(GetType(Integer)) Then Return
            Dim deLa As Integer = CInt(e.Data.GetData(GetType(Integer)))
            Dim punct As Point = dgvTabele.PointToClient(New Point(e.X, e.Y))
            Dim rowIndex As Integer = dgvTabele.RowIndexAt(punct)
            Dim la As Integer = If(rowIndex >= 0, rowIndex, _tabele.Count - 1)
            MutaRand(deLa, la)
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorForm.dgvTabele_DragDrop", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Dupa analiza se stie si cate dintre randuri sunt ale unitatii alese.
    ''' Un tabel care n-are niciunul se DEBIFEAZA: are randuri, dar nu ale noastre.
    ''' </summary>
    Private Sub ActualizeazaTabeleDinRaport(raport As RaportAnaliza)
        If raport Is Nothing Then Return

        For i As Integer = 0 To _tabele.Count - 1
            Dim t As RandTabel = _tabele(i)
            Dim numere As Integer() = Nothing
            If Not raport.PeTabel.TryGetValue(t.Nume, numere) Then Continue For

            t.AleUnitatii = numere(1).ToString()
            If numere(1) = 0 Then t.Bifat = False
            dgvTabele("ale_unitatii", i) = t.AleUnitatii
            dgvTabele("bifa", i) = t.Bifat
        Next
    End Sub

    ''' <summary>
    ''' Tabelele bifate, sau <c>Nothing</c> (cu mesaj) daca nu e bifat niciunul.
    ''' Lista goala nu se trimite ca «toate»: n-ar fi ce a cerut operatorul.
    ''' </summary>
    Private Function TabeleBifate() As List(Of String)
        Dim alese As New List(Of String)()
        For Each t As RandTabel In _tabele
            If t.Bifat AndAlso Not String.IsNullOrEmpty(t.Nume) Then alese.Add(t.Nume)
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

    Private Sub UmpleGrila(raport As RaportAnaliza)
        dgvConstatari.BeginUpdate()
        Try
            dgvConstatari.ClearRows()
            If raport Is Nothing Then Return

            For Each c As Constatare In raport.Constatari
                Dim primul As ExempluConstatare = If(c.Exemple.Count > 0, c.Exemple(0), Nothing)
                Dim rand As KBotDataRow = dgvConstatari.AddRow()
                rand("clasa") = c.Clasa
                rand("tabel") = c.Tabel
                rand("coloana") = c.Coloana
                rand("fel") = c.Fel
                rand("randuri") = c.Numar.ToString()
                rand("cheie") = If(primul Is Nothing, "", primul.Cheie)
                rand("mesaj") = If(primul Is Nothing, "", primul.Mesaj)
                rand("valoare") = If(primul Is Nothing, "", primul.Valoare)
            Next
        Finally
            dgvConstatari.EndUpdate()
        End Try
    End Sub

    Private Sub dgvConstatari_CellDoubleClick(sender As Object, e As KBotCellEventArgs) _
            Handles dgvConstatari.CellDoubleClick
        Try
            If _raport Is Nothing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= _raport.Constatari.Count Then Return

            ' Randul din grila are un singur exemplu; restul se scriu in jurnal, unde
            ' incap si pot fi copiate.
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

    ''' <summary>Baza bifata, sau Nothing (cu mesaj) daca nu e aleasa niciuna.</summary>
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
                "Baza «" & b.Nume & "» are doar " & b.TabeleFx.ToString() & " dintre tabelele FX_ migrate. " &
                "Migrarea NU creează tabele: un tabel lipsă se sare dacă nimic bifat nu depinde " &
                "de el, altfel oprește scrierea." &
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
    ''' Inventarul descrie un fisier si o baza anume; cand se schimba vreuna, lista
    ''' de tabele nu mai are ce descrie si se goleste. O bifa ramasa de la alta
    ''' baza ar fi exact bifa gresita.
    ''' </summary>
    Private Sub ResetInventar(mesaj As String)
        _tabele.Clear()
        _coloane.Clear()
        _coloaneTinta.Clear()
        dgvTabele.ClearRows()
        dgvColoane.ClearRows()
        dgvCorelatii.ClearRows()
        lblColoane.Text = "Coloane:"
        lblCorelatii.Text = "Corelații:"
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
    ''' Cele doua butoane, exact dupa regula din analiza: «Ruleaza» doar pe un
    ''' raport curat, «Forteaza» doar cand nu exista nicio constatare blocanta.
    ''' Serverul verifica din nou amandoua — interfata nu e singura paza.
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
        dgvColoane.Enabled = Not busy
        dgvCorelatii.Enabled = Not busy
        btnSus.Enabled = Not busy
        btnJos.Enabled = Not busy
        chkInlocuieste.Enabled = Not busy
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
