Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming
Imports KBot.Xfa

''' <summary>
''' The bench for the whole signing chain of a DDF, with the SERVER between the steps: can A, B and the
''' director's signature follow each other when every step starts from what the server holds and
''' leaves nothing behind on this PC? Sibling of <see cref="DdfSectiuneaBHarnessForm"/>, which keeps
''' everything local; this one stores every signed step in the bench table (KBOT_BANC_PDF, 000_DEMO only,
''' one row per upload) and reads the next step's document back from it.
'''
''' <para><b>Steps.</b>
''' (1) The DDF of the revision (real section A ONLY, nothing of section B) is generated and shown; the operator signs A. When the signature is
''' written and saved, the signed file goes to the server, Adobe is closed and the local copies are deleted.
''' (2) A separate button reads that signed A back from the server, writes section B into it (incremental
''' update, A's bytes untouched) and shows it; the operator signs B; same upload, close, delete.
''' (3) A third button reads the signed A+B back; the director signs (only «Ordonator» is allowed); same
''' upload, close, delete.</para>
'''
''' <para><b>Which server row.</b> The newest row of the bench table with this IDREV and the step's
''' label in <c>pas</c> («FLUX 1», «FLUX 2», «FLUX 3»). The real DDF of the revision is never written.</para>
''' </summary>
Public NotInheritable Class DdfFluxServerHarnessForm

    Private Const DocType As String = "DDF"
    Private Const PasA As String = "FLUX 1 - A semnat"
    Private Const PasAB As String = "FLUX 2 - A+B semnat"
    Private Const PasDirector As String = "FLUX 3 - director semnat"

    ' The step whose signature is being waited for.
    Private Enum Etapa
        Nimic = 0
        SemnareA = 1
        SemnareB = 2
        SemnareDirector = 3
    End Enum

    Private ReadOnly _api As IApiClient
    Private ReadOnly _session As SessionContext
    Private ReadOnly _loginFactory As Func(Of LoginForm)
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _temporare As New List(Of String)()
    Private _path As String
    Private _etapa As Etapa
    Private _data As DdfInfo
    Private _dataCod As String
    Private _logHooked As Boolean
    Private _signing As PdfSigningSession
    ' Sample captures picked by the operator (base64); empty = step 2 uses the revision's own PrtScr attachments from the server.
    Private _capturiProba As New List(Of String)()

    Public Sub New(api As IApiClient, session As SessionContext, loginFactory As Func(Of LoginForm),
                   log As Action(Of String))
        InitializeComponent()
        _api = api
        _session = session
        _loginFactory = loginFactory
        _log = log
        ' The harness signs in with its own session (no Program.RunShellWithLogin), so the template download reads that one.
        KBot.Xfa.TemplateDownloader.TokenProvider = Function() _session.Token
        ' The bench works on the template the server holds right now, never on a copy left in the cache folder.
        KBot.Xfa.TemplateDownloader.UseLocalCache = False
        AddHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
        _logHooked = True
        AddHandler preview.DocumentSaved, AddressOf OnDocumentSaved
        AddHandler preview.SaveCancelled, AddressOf OnSaveCancelled
        UpdateSessionLabel()
        ShowEtapa()
        Write("Banc pornit. Fiecare pas semnat se încarcă în tabela de probă de pe server (KBOT_BANC_PDF, doar în 000_DEMO); " &
              "DDF-ul real al reviziei nu se scrie. Fișierele locale sunt în «" & TempPdfStore.Root & "» și se șterg după fiecare pas.")
        Write("1) Autentificare, cod angajament + IDREV, «1. Generează A…»: semnați A în Adobe; încărcarea, închiderea Adobe și ștergerea merg singure.")
        Write("2) «2. A de pe server…»: ia A semnat de pe server, inserează Secțiunea B, semnați B; aceleași trei acțiuni.")
        Write("3) «3. A+B de pe server…»: ia documentul semnat A+B, directorul semnează (doar «Ordonator»); aceleași trei acțiuni.")
    End Sub

    ' ── Buttons (UI boundaries: log and swallow) ────────────────────────────────
    Private Sub btnAutentificare_Click(sender As Object, e As EventArgs) Handles btnAutentificare.Click
        Try
            Using login As LoginForm = _loginFactory.Invoke()
                If login.ShowDialog(Me) = DialogResult.OK Then Write("Autentificat.")
            End Using
            UpdateSessionLabel()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.btnAutentificare_Click", ex)
            Write("Autentificarea a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Sample captures for step 2: image files picked here are written into section B's Table4 instead of the revision's own
    ''' PrtScr attachments. Cancelling the dialog with files already chosen clears them (back to the server's).
    ''' </summary>
    Private Sub btnCapturi_Click(sender As Object, e As EventArgs) Handles btnCapturi.Click
        Try
            If dlgCapturi.ShowDialog(Me) <> DialogResult.OK Then
                If _capturiProba.Count > 0 Then Write("Capturile de probă șterse: pasul 2 folosește capturile reviziȳ de pe server.")
                _capturiProba.Clear()
                btnCapturi.Text = "Capturi de probă (server)"
                Return
            End If
            _capturiProba.Clear()
            Dim k_total As Long = 0
            For Each k_file As String In dlgCapturi.FileNames
                Dim k_bytes As Byte() = File.ReadAllBytes(k_file)
                k_total += k_bytes.LongLength
                _capturiProba.Add(Convert.ToBase64String(k_bytes))
            Next
            btnCapturi.Text = $"Capturi de probă: {_capturiProba.Count}"
            Write($"Capturi de probă alese: {_capturiProba.Count} fișier(e), {k_total:N0} octeți. Pasul 2 le scrie în Secțiunea B.")
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.btnCapturi_Click", ex)
            Write("Capturile nu s-au putut citi: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Step 1: the interim document of the revision (real A, placeholder B), shown for the signature on A.</summary>
    Private Async Sub btnPas1_Click(sender As Object, e As EventArgs) Handles btnPas1.Click
        Try
            Dim k_cod As String = Nothing
            Dim k_idrev As Integer
            If Not TryReadTarget(k_cod, k_idrev) OrElse Not RequireLogin() Then Return
            SetBusy(True)
            Try
                Dim k_data As DdfInfo = Await LoadDataAsync(k_cod).ConfigureAwait(True)
                Dim k_revizie As RevizieRow = k_data.Revizii.FirstOrDefault(Function(k_r) k_r.Idrev = k_idrev)
                If k_revizie Is Nothing Then
                    Write($"Revizia {k_idrev} nu există la angajamentul «{k_cod}» ({k_data.Revizii.Count} revizii citite).")
                    Return
                End If
                Write($"Citit: «{k_cod}», revizia {k_revizie.NumarRev} (IDREV {k_idrev}), stare {k_revizie.Stare}, " &
                      $"{k_data.Linii.Where(Function(k_l) k_l.Idrev = k_idrev).Count()} rânduri în Secțiunea A.")

                Dim k_generat As DdfPdfGenerator.Rezultat = Await DdfPdfGenerator.GenereazaAsync(
                    Function() Task.FromResult(k_data), Nothing, _session, k_idrev, DdfPdfMode.OnlyA).ConfigureAwait(True)
                If k_generat Is Nothing Then Return   ' the template could not be downloaded: the operator was told

                ReleaseDocument()
                Dim k_target As String = StampedPath(TempPdfStore.EnsureRoot(), $"BANC_FLUX_{k_idrev}_A")
                File.Copy(k_generat.PdfPath, k_target, overwrite:=True)
                Track(k_target)
                Track(k_generat.PdfPath)
                Track(Path.ChangeExtension(k_generat.PdfPath, ".xml"))
                Write($"Generat DOAR cu Secțiunea A, fără nimic din B: «{k_target}». Validați și semnați Secțiunea A.")
                OpenForSigning(k_target, k_idrev, Etapa.SemnareA)
            Finally
                SetBusy(False)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.btnPas1_Click", ex)
            Write("Pasul 1 a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Step 2: the signed A read back from the server, section B written into it, shown for the signature on B.</summary>
    Private Async Sub btnPas2_Click(sender As Object, e As EventArgs) Handles btnPas2.Click
        Try
            Dim k_cod As String = Nothing
            Dim k_idrev As Integer
            If Not TryReadTarget(k_cod, k_idrev) OrElse Not RequireLogin() Then Return
            SetBusy(True)
            Try
                Dim k_downloaded As String = Await DownloadStepAsync(k_idrev, PasA, "A semnat", "A").ConfigureAwait(True)
                If k_downloaded Is Nothing Then Return

                Dim k_before As Byte() = SignedPdfFiles.ReadShared(k_downloaded)
                Dim k_dataXml As String = Await SectiuneaBXmlAsync(k_before).ConfigureAwait(True)
                If k_dataXml Is Nothing Then Return

                Dim k_target As String = SectionBPath(k_downloaded)
                Dim k_inPath As String = Path.ChangeExtension(k_target, ".inainte.pdf")
                Dim k_xmlPath As String = Path.ChangeExtension(k_target, ".xml")
                Track(k_target)
                Track(k_inPath)
                Track(k_xmlPath)
                File.WriteAllBytes(k_inPath, k_before)
                File.WriteAllText(k_xmlPath, k_dataXml, New Text.UTF8Encoding(False))
                Dim k_note As String = Await Task.Run(Function() XfaSignedDocument.FillIncremental(k_inPath, k_target, k_xmlPath)).ConfigureAwait(True)

                Dim k_after As Byte() = File.ReadAllBytes(k_target)
                Dim k_prefixKept As Boolean = k_after.Length > k_before.Length AndAlso
                                              k_after.AsSpan(0, k_before.Length).SequenceEqual(k_before)
                Write($"Scris «{k_target}»: {k_before.Length:N0} ▸ {k_after.Length:N0} octeți; octeții semnați " &
                      If(k_prefixKept, "PĂSTRAȚI neatinși (actualizare incrementală).", "MODIFICAȚI — semnătura A e pierdută!"))
                Write(k_note)
                LogSignatures("După Secțiunea B", k_after)
                Write("Documentul cu Secțiunea B se deschide. Semnați Secțiunea B.")
                OpenForSigning(k_target, k_idrev, Etapa.SemnareB)
            Finally
                SetBusy(False)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.btnPas2_Click", ex)
            Write("Pasul 2 a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Step 3: the signed A+B read back from the server, shown for the director (only «Ordonator» may be signed).</summary>
    Private Async Sub btnPas3_Click(sender As Object, e As EventArgs) Handles btnPas3.Click
        Try
            Dim k_cod As String = Nothing
            Dim k_idrev As Integer
            If Not TryReadTarget(k_cod, k_idrev) OrElse Not RequireLogin() Then Return
            SetBusy(True)
            Try
                Dim k_downloaded As String = Await DownloadStepAsync(k_idrev, PasAB, "A+B semnat", "A", "B").ConfigureAwait(True)
                If k_downloaded Is Nothing Then Return
                Write("Documentul A+B se deschide. Directorul semnează în câmpul «Ordonator».")
                OpenForSigning(k_downloaded, k_idrev, Etapa.SemnareDirector)
            Finally
                SetBusy(False)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.btnPas3_Click", ex)
            Write("Pasul 3 a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Empties the viewer and closes EVERY Acrobat / Acrobat Reader process of this machine, asking first:
    ''' an Adobe the operator opened goes too. The log is kept.
    ''' </summary>
    Private Sub btnInchideAdobe_Click(sender As Object, e As EventArgs) Handles btnInchideAdobe.Click
        Try
            _etapa = Etapa.Nimic
            ShowEtapa()
            ReleaseDocument()
            Write("Vizualizatorul golit.")
            CloseAllAdobe()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.btnInchideAdobe_Click", ex)
            Write("Închiderea Adobe a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub btnJurnal_Click(sender As Object, e As EventArgs) Handles btnJurnal.Click
        txtJurnal.Clear()
    End Sub

    ' ── The signature arrived: upload, close Adobe, delete ──────────────────────
    ' The recording client got an upload from the session (UI thread): the signed bytes of the step.
    Private Sub OnUpload(k_docType As String, k_id As Integer, k_bytes As Byte(), k_semnatura As String,
                         k_records As IReadOnlyList(Of PdfSignatureRecord))
        Try
            Dim k_fields As String = If(k_records Is Nothing OrElse k_records.Count = 0, "niciuna",
                                        String.Join(", ", k_records.Select(Function(k_r) $"{k_r.camp} ({k_r.rol}, {k_r.semnatar})")))
            Write($"Semnătură primită {k_docType} {k_id}: {k_bytes.Length:N0} octeți, roluri «{k_semnatura}», semnături noi: {k_fields}.")
            LogSignatures("  Semnat", k_bytes)
            Dim k_etapa As Etapa = _etapa
            If k_etapa = Etapa.Nimic Then
                Write("Nicio etapă în lucru — documentul nu se încarcă.")
                Return
            End If
            If Not RolesDone(k_etapa, k_semnatura) Then
                Write($"Rolurile «{k_semnatura}» nu sunt cele așteptate la această etapă ({RolesExpected(k_etapa)}) — " &
                      "documentul rămâne deschis, nu se încarcă.")
                Return
            End If
            _etapa = Etapa.Nimic   ' this step is taken: a second upload of the same step is ignored
            FinishEtapaAsync(k_etapa, k_id, k_bytes, k_semnatura, k_records, _path)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.OnUpload", ex)
            Write("Prelucrarea semnăturii a eșuat: " & ex.Message)
        End Try
    End Sub

    ' Upload to the bench table, then (only when the server has exactly these bytes) close Adobe and delete the local
    ' copies. UI-thread async boundary: log and swallow.
    Private Async Sub FinishEtapaAsync(k_etapa As Etapa, k_id As Integer, k_bytes As Byte(), k_semnatura As String,
                                       k_records As IReadOnlyList(Of PdfSignatureRecord), k_localPath As String)
        Try
            Dim k_client As ApiClient = TryCast(_api, ApiClient)
            If k_client Is Nothing Then
                Write("Nu se poate încărca: lipsește clientul API. Documentul rămâne deschis și local.")
                Return
            End If
            Dim k_pas As String = PasOf(k_etapa)
            Dim k_resp As PutPdfResponse = Await k_client.UploadBancPdfAsync(
                "ddf", k_id, k_bytes, Nothing, k_semnatura, k_records,
                If(k_localPath Is Nothing, "", Path.GetFileName(k_localPath)), k_pas, CancellationToken.None).ConfigureAwait(True)
            Dim k_same As Boolean = PdfHash.AreEqual(k_resp.sha256, PdfHash.Compute(k_bytes))
            Write($"ÎNCĂRCAT pe server «{k_pas}»: {k_resp.dimensiune:N0} octeți, sha {PdfSignatureReport.ShortSha(k_resp.sha256)} " &
                  If(k_same, "— IDENTIC cu ce s-a trimis.", "— DIFERIT de ce s-a trimis!"))
            If Not k_same Then
                Write("Serverul nu are exact octeții semnați: Adobe și fișierele locale rămân, nu se șterge nimic.")
                Return
            End If

            Write("Închid Adobe și șterg copiile locale…")
            ReleaseDocument()
            Await Task.Run(Sub() AdobeProcessRegistry.Shutdown()).ConfigureAwait(True)
            Await DeleteLocalAsync().ConfigureAwait(True)
            ShowEtapa()
            Write($"Etapa «{k_pas}» încheiată. Pasul următor: butonul {NextButtonText(k_etapa)}.")
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.FinishEtapaAsync", ex)
            Write("Încărcarea / curățarea a eșuat: " & ex.Message & " Documentul și fișierele locale rămân.")
        End Try
    End Sub

    ''' <summary>
    ''' Deletes every tracked local file. Adobe may still hold a file for a moment, so it is retried; what is
    ''' still held after that is offered a close of every Adobe process, then retried once more.
    ''' </summary>
    Private Async Function DeleteLocalAsync() As Task
        Dim k_left As List(Of String) = Await TryDeleteTrackedAsync(10).ConfigureAwait(True)
        If k_left.Count > 0 Then
            Write($"Adobe mai ține {k_left.Count} fișier(e) deschis(e): {String.Join(", ", k_left.Select(Function(k_p) Path.GetFileName(k_p)))}.")
            If CloseAllAdobe() Then k_left = Await TryDeleteTrackedAsync(10).ConfigureAwait(True)
        End If
        If k_left.Count = 0 Then
            Write("Copiile locale au fost șterse.")
        Else
            Write($"RĂMASE pe disc ({k_left.Count}): {String.Join(", ", k_left)}.")
        End If
    End Function

    Private Async Function TryDeleteTrackedAsync(k_attempts As Integer) As Task(Of List(Of String))
        Dim k_left As List(Of String) = _temporare.Where(Function(k_p) File.Exists(k_p)).ToList()
        Dim k_last As Exception = Nothing
        For k_try As Integer = 1 To k_attempts
            Dim k_still As New List(Of String)()
            For Each k_file As String In k_left
                Try
                    File.Delete(k_file)
                Catch ex As IOException
                    k_last = ex
                    k_still.Add(k_file)
                Catch ex As UnauthorizedAccessException
                    k_last = ex
                    k_still.Add(k_file)
                End Try
            Next
            k_left = k_still
            If k_left.Count = 0 Then Exit For
            Await Task.Delay(500).ConfigureAwait(True)
        Next
        If k_left.Count > 0 AndAlso k_last IsNot Nothing Then GlobalErrorLog.Write("DdfFluxServerHarnessForm.TryDeleteTrackedAsync", k_last)
        _temporare.RemoveAll(Function(k_p) Not File.Exists(k_p))
        Return k_left
    End Function

    ''' <summary>Asks, then closes every Adobe process of the machine. False = none running or the operator said no.</summary>
    Private Function CloseAllAdobe() As Boolean
        Dim k_pids As List(Of Integer) = AdobeWindowHosting.AdobeProcessIds()
        If k_pids.Count = 0 Then
            Write("Niciun proces Adobe pornit.")
            Return False
        End If
        If KBotMessage.Show(Me, $"Se închid TOATE procesele Adobe de pe calculator ({k_pids.Count}), " &
                                "inclusiv documentele deschise în afara K-BOT, fără salvare." &
                                Environment.NewLine & Environment.NewLine & "Continuați?",
                            "Închide Adobe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Write("Procesele Adobe lăsate pornite.")
            Return False
        End If
        For Each k_pid As Integer In k_pids
            AdobeWindowHosting.KillPid(k_pid)
        Next
        Dim k_left As Integer = AdobeWindowHosting.AdobeProcessIds().Count
        Write($"Închise {k_pids.Count} procese Adobe [{String.Join(", ", k_pids)}]" & If(k_left > 0, $", {k_left} încă pornite.", "."))
        Return True
    End Function

    ' ── Server ──────────────────────────────────────────────────────────────────
    ''' <summary>
    ''' The newest bench-table document of this revision whose step label is <paramref name="k_pas"/>, written to a new
    ''' local file; Nothing (with the reason written) when the server has none, it cannot be read or it lacks the
    ''' roles the step needs.
    ''' </summary>
    Private Async Function DownloadStepAsync(k_idrev As Integer, k_pas As String, k_label As String,
                                             ParamArray k_roles As String()) As Task(Of String)
        Dim k_client As ApiClient = TryCast(_api, ApiClient)
        If k_client Is Nothing Then
            Write("Lipsește clientul API.")
            Return Nothing
        End If
        Dim k_list As BancPdfList = Await k_client.ListBancPdfAsync(CancellationToken.None).ConfigureAwait(True)
        Dim k_row As BancPdfRow = If(k_list Is Nothing OrElse k_list.fisiere Is Nothing, Nothing,
            k_list.fisiere.Where(Function(k_r) String.Equals(k_r.tip, "ddf", StringComparison.OrdinalIgnoreCase) AndAlso
                                               k_r.id_doc = k_idrev AndAlso
                                               k_r.pas.StartsWith(k_pas.Substring(0, 6), StringComparison.Ordinal)).
                                  OrderByDescending(Function(k_r) k_r.id).FirstOrDefault())
        If k_row Is Nothing Then
            Write($"Pe server nu e niciun «{k_label}» pentru IDREV {k_idrev} (pas «{k_pas.Substring(0, 6)}»). Faceți mai întâi pasul dinainte.")
            Return Nothing
        End If
        Dim k_result As PdfDownloadResult = Await k_client.DownloadBancPdfAsync(k_row.id, CancellationToken.None).ConfigureAwait(True)
        If k_result.Status <> PdfDownloadStatus.Content Then
            Write($"Rândul {k_row.id} nu mai există pe server.")
            Return Nothing
        End If
        Write($"De pe server: rândul {k_row.id} ({k_row.primit}, «{k_row.pas}»), {k_result.Bytes.Length:N0} octeți, " &
              $"sha {PdfSignatureReport.ShortSha(k_result.Sha256)}.")
        LogSignatures("  De pe server", k_result.Bytes)
        Dim k_found As String = PdfSignatures.Read(k_result.Bytes, DocType).ToSemnatura()
        For Each k_role As String In k_roles
            If Not DdfRevisionStates.HasRole(k_found, k_role) Then
                Write($"Documentul de pe server nu are semnătura «{k_role}» (roluri găsite: «{k_found}») — nu merg mai departe.")
                Return Nothing
            End If
        Next
        ReleaseDocument()
        _etapa = Etapa.Nimic
        Dim k_target As String = StampedPath(TempPdfStore.EnsureRoot(), $"BANC_FLUX_{k_idrev}_srv{k_row.id}")
        File.WriteAllBytes(k_target, k_result.Bytes)
        Track(k_target)
        Return k_target
    End Function

    ' ── Section B ───────────────────────────────────────────────────────────────
    ''' <summary>
    ''' The fill XML for section B, or Nothing (with the reason written) when there is nothing to build it from. The
    ''' server's section B rows of the revision win; without them, section A's rows of the document itself plus the
    ''' typed code and indicator.
    ''' </summary>
    Private Async Function SectiuneaBXmlAsync(k_pdfBytes As Byte()) As Task(Of String)
        Dim k_count As Integer = 0
        Dim k_idrev As Integer
        Dim k_formXml As String = Await Task.Run(Function() XfaSignedDocument.ReadFormData(k_pdfBytes)).ConfigureAwait(True)
        If txtCod.Text.Trim().Length > 0 AndAlso Integer.TryParse(txtIdrev.Text.Trim(), k_idrev) AndAlso
           _session IsNot Nothing AndAlso _session.IsAuthenticated Then
            Dim k_cod As String = txtCod.Text.Trim()
            Dim k_data As DdfInfo = Await LoadDataAsync(k_cod).ConfigureAwait(True)
            Dim k_revizie As RevizieRow = k_data.Revizii.FirstOrDefault(Function(k_r) k_r.Idrev = k_idrev)
            Dim k_sb As List(Of SectiuneBRow) = k_data.SectiuneB.Where(Function(k_s) k_s.Idrev = k_idrev).ToList()
            If k_revizie IsNot Nothing AndAlso k_sb.Count > 0 Then
                ' The program of section B = the one section A of the SIGNED document carries.
                Dim k_program As String = DdfSectionBInsert.SectionAProgram(k_formXml)
                If k_program.Length = 0 Then k_program = If(_session.CodProgram, String.Empty)
                Dim k_capturi As List(Of String) = Await CapturiAsync(k_data, k_idrev).ConfigureAwait(True)
                Dim k_xml As String = DdfSectionBInsert.BuildFillXml(DdfXmlBuilder.Context.FromSession(_session),
                                                                      k_data.AntetDeLucru(k_revizie.Iddf), k_revizie, k_sb, k_capturi, k_program)
                Write($"Secțiunea B: {k_sb.Count} rând(uri) de pe server (IDREV {k_idrev}), program «{k_program}», bifa opțiunii 1 pusă.")
                Return k_xml
            End If
            Write($"Serverul nu are Secțiunea B pentru IDREV {k_idrev} — se construiește din Secțiunea A a documentului.")
        End If

        Dim k_codB As String = txtCodB.Text.Trim()
        Dim k_indicator As String = txtIndicator.Text.Trim()
        If k_codB.Length = 0 OrElse k_indicator.Length = 0 Then
            Write("Scrieți codul angajamentului și indicatorul pentru Secțiunea B (le dă FOREXE după trimitere; " &
                  "pentru probă, orice valoare).")
            Return Nothing
        End If
        Dim k_capturiB As List(Of String) = _capturiProba.ToList()
        If k_capturiB.Count = 0 AndAlso _data IsNot Nothing AndAlso Integer.TryParse(txtIdrev.Text.Trim(), k_idrev) Then
            k_capturiB = Await CapturiAsync(_data, k_idrev).ConfigureAwait(True)
        End If
        Dim k_built As String = FromSectionA(k_formXml, k_codB, k_indicator, k_count, k_capturiB)
        If k_count = 0 Then
            Write("Secțiunea A nu are rânduri în datele formularului (Table1) — nu am din ce construi Secțiunea B.")
            Return Nothing
        End If
        Write($"Secțiunea B: {k_count} rând(uri) din Secțiunea A, cod «{k_codB}», indicator «{k_indicator}», bifa opțiunii 1 pusă.")
        Return k_built
    End Function

    ''' <summary>
    ''' The captures written into section B: the sample files when some were picked, else the revision's PrtScr attachments
    ''' read from the server (the base64 the row carries, or the stored file). Empty = section B without captures.
    ''' </summary>
    Private Async Function CapturiAsync(k_data As DdfInfo, k_idrev As Integer) As Task(Of List(Of String))
        If _capturiProba.Count > 0 Then
            Write($"Capturi: {_capturiProba.Count} de probă (alese aici).")
            Return _capturiProba.ToList()
        End If
        Dim k_att As List(Of AtasamentRow) = k_data.Atasamente.Where(Function(k_a) k_a.Idrev = k_idrev AndAlso k_a.PrtScr).ToList()
        If k_att.Count = 0 Then
            Write("Capturi: niciuna (revizia nu are atașamente PrtScr pe server și nu ai ales capturi de probă).")
            Return New List(Of String)()
        End If
        Dim k_list As List(Of String) = Await DdfPdfGenerator.CapturileAsync(k_att, AddressOf ReadAttachmentAsync).ConfigureAwait(True)
        Write($"Capturi: {k_list.Count} de pe server (atașamentele PrtScr ale reviziei).")
        Return k_list
    End Function

    Private Async Function ReadAttachmentAsync(k_idRevAtt As Integer) As Task(Of Byte())
        Dim k_r As PdfDownloadResult = Await _api.GetDdfFisierAsync(k_idRevAtt, String.Empty, CancellationToken.None).ConfigureAwait(True)
        Return If(k_r IsNot Nothing AndAlso k_r.Status = PdfDownloadStatus.Content, k_r.Bytes, Nothing)
    End Function

    ''' <summary>
    ''' Section B from section A's rows in the form data (<c>Table1</c>: Cell2 = program, Cell3 = SSI, Cell6 = current
    ''' value) -- a new angajament with no rezervare. Same row shape as <c>DdfXmlBuilder.BuildFormXml</c>.
    ''' ASSUMPTION (bench only): section A's Cell3 is the SSI code section B wants in Cell4.
    ''' </summary>
    Private Shared Function FromSectionA(k_formXml As String, k_cod As String, k_indicator As String,
                                         ByRef k_rows As Integer, k_capturi As IEnumerable(Of String)) As String
        k_rows = 0
        If String.IsNullOrWhiteSpace(k_formXml) Then Return String.Empty
        Dim k_doc As XDocument = XDocument.Parse(k_formXml)
        Dim k_table1 As XElement = k_doc.Descendants().FirstOrDefault(Function(k_x) k_x.Name.LocalName = "Table1")
        Dim k_table3 As New XElement("Table3", New XElement("Row1", New XElement("Cell1")))
        If k_table1 IsNot Nothing Then
            For Each k_row As XElement In k_table1.Elements().Where(Function(k_x) k_x.Name.LocalName = "Row1")
                Dim k_program As String = CellText(k_row, "Cell2")
                Dim k_ssi As String = CellText(k_row, "Cell3")
                Dim k_valoare As String = CellText(k_row, "Cell6")
                If k_program.Length = 0 AndAlso k_ssi.Length = 0 AndAlso k_valoare.Length = 0 Then Continue For   ' the template row
                k_table3.Add(New XElement("Row1",
                    New XElement("Cell1", k_cod), New XElement("Cell2", k_indicator),
                    New XElement("Cell3", k_program), New XElement("Cell4", k_ssi),
                    New XElement("Cell5", "0"), New XElement("Cell6", k_valoare),
                    New XElement("Cell8", "0"), New XElement("Cell9", k_valoare)))
                k_rows += 1
            Next
        End If
        Dim k_sectB As New XElement("SubformSectiuneaB", New XElement("CheckBox9", "1"), k_table3)
        Dim k_table4 As XElement = DdfXmlBuilder.Table4Of(k_capturi)
        If k_table4 IsNot Nothing Then k_sectB.Add(New XElement("Subform51", k_table4))
        Return "<?xml version=""1.0"" encoding=""UTF-8""?>" & New XElement("form1", k_sectB).ToString()
    End Function

    Private Shared Function CellText(k_row As XElement, k_name As String) As String
        Dim k_cell As XElement = k_row.Elements().FirstOrDefault(Function(k_x) k_x.Name.LocalName = k_name)
        Return If(k_cell Is Nothing, String.Empty, k_cell.Value.Trim())
    End Function

    ' «…_srv12_HHmmss.pdf» -> «…_B_<now>.pdf»: a NEW name every time (Acrobat keeps an earlier file open and serves
    ' a reused name from memory). Never the source itself.
    Private Shared Function SectionBPath(k_source As String) As String
        Return StampedPath(Path.GetDirectoryName(k_source), StampBase(k_source) & "_B")
    End Function

    ' The file name without extension and without the time stamps this bench appended («_HHmmss», possibly repeated,
    ' plus a «_2» collision suffix).
    Private Shared Function StampBase(k_path As String) As String
        Dim k_name As String = Path.GetFileNameWithoutExtension(k_path)
        Return System.Text.RegularExpressions.Regex.Replace(k_name, "(_\d{6}(_\d+)?)+$", "")
    End Function

    ' «<folder>\<base>_HHmmss.pdf»; «_2», «_3»… when that name is already taken in the same second.
    Private Shared Function StampedPath(k_folder As String, k_baseName As String) As String
        Dim k_stamp As String = DateTime.Now.ToString("HHmmss")
        Dim k_candidate As String = Path.Combine(k_folder, $"{k_baseName}_{k_stamp}.pdf")
        Dim k_n As Integer = 2
        While File.Exists(k_candidate)
            k_candidate = Path.Combine(k_folder, $"{k_baseName}_{k_stamp}_{k_n}.pdf")
            k_n += 1
        End While
        Return k_candidate
    End Function

    ' ── Document on screen ──────────────────────────────────────────────────────
    ''' <summary>
    ''' Shows <paramref name="k_path"/> with the Save As trap on and a REAL signing session whose upload is caught by
    ''' <see cref="OnUpload"/> (the recording client: the session's own upload goes nowhere, the bench uploads the
    ''' bytes it was given).
    ''' </summary>
    Private Sub OpenForSigning(k_path As String, k_idrev As Integer, k_etapa As Etapa)
        _path = k_path
        _etapa = k_etapa
        lblFisier.Text = Path.GetFileName(k_path)
        ShowEtapa()
        LogSignatures("Deschis", SignedPdfFiles.ReadShared(k_path))
        _signing = New PdfSigningSession(PdfDocKind.Ddf, k_idrev, k_path, k_path, String.Empty,
                                         RecordingPdfApi.ForUploads(AddressOf OnUpload))
        ' The director signs only as «Ordonator»; a signature in another field is refused and not uploaded.
        If k_etapa = Etapa.SemnareDirector Then _signing.AllowedNewRoles = DirectorMode.SigningRoles
        AddHandler _signing.Completed, AddressOf OnSigningCompleted
        _signing.Begin()
        preview.Signing = _signing
        preview.ShowDocument(k_path, exists:=True)
    End Sub

    ''' <summary>Adobe holds the file open: the preview lets go before it is read, replaced or deleted.</summary>
    Private Sub ReleaseDocument()
        preview.Signing = Nothing
        EndSigning()
        preview.Clear()
        _path = Nothing
        lblFisier.Text = "Niciun document"
    End Sub

    Private Sub EndSigning()
        Dim k_session As PdfSigningSession = _signing
        _signing = Nothing
        If k_session Is Nothing Then Return
        RemoveHandler k_session.Completed, AddressOf OnSigningCompleted
        k_session.Dispose()
    End Sub

    Private Sub OnSigningCompleted(k_session As PdfSigningSession, k_outcome As PdfSigningOutcome)
        Try
            If k_outcome Is Nothing Then Return
            Write($"Sesiune «{Path.GetFileName(k_session.DisplayedPath)}»: {k_outcome.Status} — {k_outcome.Message}")
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.OnSigningCompleted", ex)
        End Try
    End Sub

    ' A trapped save finished / was cancelled: logged only, the signing session reads the file itself.
    Private Sub OnDocumentSaved(k_path As String)
        Write("SALVAT de capcană: " & k_path)
    End Sub

    Private Sub OnSaveCancelled(k_reason As String)
        Write("SALVARE ANULATĂ de capcană: " & k_reason)
    End Sub

    ' Raised on the writer's thread. Never throws: AdobeHostLog swallows a failing listener anyway.
    Private Sub OnAdobeLogLine(k_line As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of String)(AddressOf AppendLine), "[Adobe] " & k_line)
        Else
            AppendLine("[Adobe] " & k_line)
        End If
    End Sub

    ' ── Steps ───────────────────────────────────────────────────────────────────
    Private Shared Function RolesDone(k_etapa As Etapa, k_semnatura As String) As Boolean
        Select Case k_etapa
            Case Etapa.SemnareA
                Return DdfRevisionStates.HasRole(k_semnatura, DdfRevisionStates.RoleA)
            Case Etapa.SemnareB
                Return DdfRevisionStates.HasRole(k_semnatura, DdfRevisionStates.RoleA) AndAlso
                       DdfRevisionStates.HasRole(k_semnatura, DdfRevisionStates.RoleB)
            Case Etapa.SemnareDirector
                Return DdfRevisionStates.HasRole(k_semnatura, DdfRevisionStates.RoleA) AndAlso
                       DdfRevisionStates.HasRole(k_semnatura, DdfRevisionStates.RoleB) AndAlso
                       DdfRevisionStates.HasRole(k_semnatura, DdfRevisionStates.RoleDirector)
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function RolesExpected(k_etapa As Etapa) As String
        Select Case k_etapa
            Case Etapa.SemnareA : Return "A"
            Case Etapa.SemnareB : Return "A, B"
            Case Etapa.SemnareDirector : Return "A, B, Ordonator"
            Case Else : Return "-"
        End Select
    End Function

    Private Shared Function PasOf(k_etapa As Etapa) As String
        Select Case k_etapa
            Case Etapa.SemnareA : Return PasA
            Case Etapa.SemnareB : Return PasAB
            Case Etapa.SemnareDirector : Return PasDirector
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(k_etapa))
        End Select
    End Function

    Private Shared Function NextButtonText(k_etapa As Etapa) As String
        Select Case k_etapa
            Case Etapa.SemnareA : Return "«2. A de pe server…»"
            Case Etapa.SemnareB : Return "«3. A+B de pe server…»"
            Case Else : Return "— lanțul s-a încheiat"
        End Select
    End Function

    Private Sub ShowEtapa()
        Select Case _etapa
            Case Etapa.SemnareA : lblEtapa.Text = "Etapa: se așteaptă semnătura A"
            Case Etapa.SemnareB : lblEtapa.Text = "Etapa: se așteaptă semnătura B"
            Case Etapa.SemnareDirector : lblEtapa.Text = "Etapa: se așteaptă semnătura directorului"
            Case Else : lblEtapa.Text = "Etapa: —"
        End Select
    End Sub

    Private Sub SetBusy(k_busy As Boolean)
        btnPas1.Enabled = Not k_busy
        btnPas2.Enabled = Not k_busy
        btnPas3.Enabled = Not k_busy
    End Sub

    Private Sub Track(k_path As String)
        If Not _temporare.Contains(k_path, StringComparer.OrdinalIgnoreCase) Then _temporare.Add(k_path)
    End Sub

    ' ── Helpers ─────────────────────────────────────────────────────────────────
    ''' <summary>The DDF of <paramref name="k_cod"/> with section B and attachments; read once per code.</summary>
    Private Async Function LoadDataAsync(k_cod As String) As Task(Of DdfInfo)
        If _data IsNot Nothing AndAlso String.Equals(_dataCod, k_cod, StringComparison.OrdinalIgnoreCase) Then Return _data
        Dim k_info As DdfInfo = Await _api.GetDdfAsync(k_cod, CancellationToken.None, pentruGenerare:=True).ConfigureAwait(True)
        If k_info Is Nothing Then Throw New InvalidOperationException("The DDF read returned nothing.")
        _data = k_info
        _dataCod = k_cod
        Return k_info
    End Function

    Private Sub LogSignatures(k_label As String, k_bytes As Byte())
        For Each k_line As String In PdfSignatureReport.Lines(k_label, k_bytes, DocType)
            Write(k_line)
        Next
    End Sub

    Private Function TryReadTarget(ByRef k_cod As String, ByRef k_idrev As Integer) As Boolean
        k_cod = txtCod.Text.Trim()
        If k_cod.Length > 0 AndAlso Integer.TryParse(txtIdrev.Text.Trim(), k_idrev) AndAlso k_idrev > 0 Then Return True
        Write("Scrieți codul angajamentului (cel din FX_DDF) și IDREV-ul reviziei.")
        Return False
    End Function

    Private Function RequireLogin() As Boolean
        If _session IsNot Nothing AndAlso _session.IsAuthenticated Then Return True
        Write("Nu sunteți autentificat — apăsați «Autentificare…».")
        Return False
    End Function

    Private Sub UpdateSessionLabel()
        If _session IsNot Nothing AndAlso _session.IsAuthenticated Then
            lblSesiune.Text = $"Autentificat: {_session.OperatorName} @ {_session.DbName}"
        Else
            lblSesiune.Text = "Neautentificat"
        End If
    End Sub

    Private Sub Write(k_line As String)
        AppendLine(k_line)
        _log?.Invoke(k_line)
    End Sub

    Private Sub AppendLine(k_line As String)
        If IsDisposed Then Return
        txtJurnal.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") & "  " & k_line & Environment.NewLine)
    End Sub

    ' Called from Dispose (see Designer). Must not throw there.
    Private Sub Inchide()
        Try
            If _logHooked Then
                RemoveHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
                _logHooked = False
            End If
            preview.Signing = Nothing
            EndSigning()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessForm.Inchide", ex)
        End Try
    End Sub

End Class
