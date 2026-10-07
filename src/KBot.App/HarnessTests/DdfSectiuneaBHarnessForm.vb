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
''' The bench for slice 0078-04: can a DDF signed on section A get section B afterwards and be
''' signed again, INSIDE K-BOT? The operator has no such DDF, so the bench makes one.
'''
''' <para><b>Steps.</b> (1) Cod angajament + IDREV -> the revision's INTERIM document (real section A,
''' placeholder section B -- exactly what «Genereaza» makes for a revision not sent yet) in
''' <c>Temp\PDF</c>, shown in
''' the real Adobe preview. The operator signs A and saves -- the Save As trap writes it back to
''' the same file. (2) Any saved PDF can be reopened later, in place. (3) «Insereaza Sectiunea B»
''' writes section B INTO the file on screen as an incremental update (the A-signed bytes stay
''' untouched), into a NEW file next to it, and shows that one; the operator checks A in Adobe's
''' signature panel, signs B and saves.</para>
'''
''' <para><b>Only local.</b> Nothing reaches the server; the server is only READ, for the revision's
''' data. Since slice 0078-05 a REAL signing session runs on every document shown, on
''' <see cref="RecordingPdfApi"/>: its uploads are only written to the log («ÎNCĂRCARE SIMULATĂ»),
''' with the signatures of the bytes that would have been sent. «După semnătură»: K-BOT presses
''' Ctrl+S itself once the signature is written and uploads after that save (what production does
''' since 0078-05), or nothing (the old behaviour, for comparison). The other variant tried --
''' uploading the save Adobe asks for on close -- left the document unsaved and was dropped.
''' «Trimite și în tabela de probă» also sends every such upload, exactly as the session built it,
''' to PUT /api/forexe/banc/pdf (table KBOT_BANC_PDF, only in 000_DEMO): one row per upload, so the
''' bytes the server would have received can be examined there.</para>
'''
''' <para><b>A new file name every time.</b> Generating, opening a saved PDF and inserting section B
''' each make a NEW file whose name ends in the time (<c>…_HHmmss.pdf</c>); opening copies the
''' chosen file first. Acrobat locks, and serves from memory, a name it already has open.</para>
'''
''' <para><b>Section B's rows.</b> The revision's own section B rows when the server has them
''' (after the send); otherwise -- a new angajament not sent yet, no rezervare -- built from
''' section A's rows with the typed code and indicator.</para>
''' </summary>
Public NotInheritable Class DdfSectiuneaBHarnessForm

    Private Const DocType As String = "DDF"

    Private ReadOnly _api As IApiClient
    Private ReadOnly _session As SessionContext
    Private ReadOnly _loginFactory As Func(Of LoginForm)
    Private ReadOnly _log As Action(Of String)
    Private _path As String
    Private _data As DdfInfo
    Private _dataCod As String
    Private _logHooked As Boolean
    ' Slice 0078-05: the (simulated) signing session of the document on screen.
    Private _signing As PdfSigningSession

    Public Sub New(api As IApiClient, session As SessionContext, loginFactory As Func(Of LoginForm),
                   log As Action(Of String))
        InitializeComponent()
        _api = api
        _session = session
        _loginFactory = loginFactory
        _log = log
        AddHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
        _logHooked = True
        AddHandler preview.DocumentSaved, AddressOf OnDocumentSaved
        AddHandler preview.SaveCancelled, AddressOf OnSaveCancelled
        cmbDupa.SelectedIndex = 0
        lblMotor.Text = "Motor: " & AdobeViewerSettings.EngineLabel(preview.Engine) & " (din «Setări»)"
        UpdateSessionLabel()
        Write("Banc pornit. Totul rămâne LOCAL, în «" & TempPdfStore.Root & "»: nimic nu se încarcă pe server.")
        Write("1) Autentificare, cod angajament + IDREV, «Generează DDF intermediar»: Secțiunea A reală + Secțiunea B provizorie " &
              "(cod «___________» — 11, indicator «___», program «0000000000»); validați, semnați A în Adobe și salvați.")
        Write("2) Oricând după: «Deschide un PDF salvat…» îl redeschide pe loc.")
        Write("3) «Inserează Secțiunea B»: valorile reale înlocuiesc B provizorie (doar datele formularului, macheta neatinsă), " &
              "într-un fișier nou (…_B.pdf) care se deschide; verificați panoul de semnături Adobe și atașamentul NOTAFD.xml, " &
              "apoi semnați B și salvați. Varianta de mână: completați B direct în Adobe peste valorile provizorii.")
    End Sub

    ' ── Buttons (UI boundaries: log and swallow) ────────────────────────────────
    Private Sub btnAutentificare_Click(sender As Object, e As EventArgs) Handles btnAutentificare.Click
        Try
            Using login As LoginForm = _loginFactory.Invoke()
                If login.ShowDialog(Me) = DialogResult.OK Then Write("Autentificat.")
            End Using
            UpdateSessionLabel()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.btnAutentificare_Click", ex)
            Write("Autentificarea a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Step 1: the revision's interim document (real A, placeholder B), shown on screen.</summary>
    Private Async Sub btnGenereaza_Click(sender As Object, e As EventArgs) Handles btnGenereaza.Click
        Try
            Dim cod As String = Nothing
            Dim idrev As Integer
            If Not TryReadTarget(cod, idrev) OrElse Not RequireLogin() Then Return
            btnGenereaza.Enabled = False
            Try
                Dim data As DdfInfo = Await LoadDataAsync(cod).ConfigureAwait(True)
                Dim revizie As RevizieRow = data.Revizii.FirstOrDefault(Function(r) r.Idrev = idrev)
                If revizie Is Nothing Then
                    Write($"Revizia {idrev} nu există la angajamentul «{cod}» ({data.Revizii.Count} revizii citite).")
                    Return
                End If
                Write($"Citit: «{cod}», revizia {revizie.NumarRev} (IDREV {idrev}), stare {revizie.Stare}, " &
                      $"{data.Linii.Where(Function(l) l.Idrev = idrev).Count()} rânduri în Secțiunea A, " &
                      $"{data.SectiuneB.Where(Function(s) s.Idrev = idrev).Count()} în Secțiunea B pe server.")

                ' The real interim generation (DdfPdfGenerator, as the DDF view does), from the data just read.
                Dim generat As DdfPdfGenerator.Rezultat = Await DdfPdfGenerator.GenereazaAsync(
                    Function() Task.FromResult(data), Nothing, _session, idrev, DdfPdfMode.Interim).ConfigureAwait(True)

                Dim target As String = StampedPath(TempPdfStore.EnsureRoot(), $"BANC_DDF_{idrev}_A")
                ReleaseDocument()
                File.Copy(generat.PdfPath, target, overwrite:=True)
                Write($"Generat intermediar (A reală, B provizorie): «{target}». Validați, semnați Secțiunea A și salvați.")
                Open(target)
            Finally
                btnGenereaza.Enabled = True
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.btnGenereaza_Click", ex)
            Write("Generarea a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Step 2: a saved PDF, reopened as a COPY under a new, time-stamped name (slice 0078-05: a name
    ''' Acrobat may still hold is never reused). The saves go to the copy.
    ''' </summary>
    Private Sub btnDeschide_Click(sender As Object, e As EventArgs) Handles btnDeschide.Click
        Try
            dlgDeschide.InitialDirectory = TempPdfStore.EnsureRoot()
            If dlgDeschide.ShowDialog(Me) <> DialogResult.OK Then Return
            ReleaseDocument()
            Dim source As String = dlgDeschide.FileName
            Dim target As String = StampedPath(Path.GetDirectoryName(source), StampBase(source))
            File.WriteAllBytes(target, SignedPdfFiles.ReadShared(source))
            Write($"Copiat «{Path.GetFileName(source)}» ▸ «{Path.GetFileName(target)}».")
            Open(target)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.btnDeschide_Click", ex)
            Write("Nu s-a putut deschide: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0078-05: one of the uploads stored in the bench table on the server (KBOT_BANC_PDF,
    ''' 000_DEMO), picked from the list, downloaded (sum checked against the server's) into a new
    ''' time-stamped file and shown -- exactly what the server received.
    ''' </summary>
    Private Async Sub btnDinServer_Click(sender As Object, e As EventArgs) Handles btnDinServer.Click
        Try
            Dim client As ApiClient = TryCast(_api, ApiClient)
            If client Is Nothing OrElse Not RequireLogin() Then Return
            btnDinServer.Enabled = False
            Try
                Dim list As BancPdfList = Await client.ListBancPdfAsync(CancellationToken.None).ConfigureAwait(True)
                If list Is Nothing OrElse list.fisiere Is Nothing OrElse list.fisiere.Count = 0 Then
                    Write("Tabela de probă (KBOT_BANC_PDF) e goală.")
                    Return
                End If
                Dim row As BancPdfRow
                Using picker As New BancPdfPickerForm(list.fisiere)
                    If picker.ShowDialog(Me) <> DialogResult.OK Then Return
                    row = picker.Selected
                End Using
                If row Is Nothing Then Return

                Dim result As PdfDownloadResult = Await client.DownloadBancPdfAsync(row.id, CancellationToken.None).ConfigureAwait(True)
                If result.Status <> PdfDownloadStatus.Content Then
                    Write($"Rândul {row.id} nu mai există pe server.")
                    Return
                End If
                Dim baseName As String = If(String.IsNullOrWhiteSpace(row.nume_fisier), $"BANC_{row.tip}_{row.id_doc}", StampBase(row.nume_fisier))
                ReleaseDocument()
                Dim target As String = StampedPath(TempPdfStore.EnsureRoot(), $"{baseName}_srv{row.id}")
                File.WriteAllBytes(target, result.Bytes)
                Write($"De pe server: rândul {row.id} ({row.primit}, «{row.nume_fisier}», «{row.pas}»), " &
                      $"{result.Bytes.Length:N0} octeți, sha {PdfSignatureReport.ShortSha(result.Sha256)} ▸ «{Path.GetFileName(target)}».")
                Open(target)
            Finally
                btnDinServer.Enabled = True
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.btnDinServer_Click", ex)
            Write("Încărcarea de pe server a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Step 3: section B written INTO the document on screen as an incremental update, into a new
    ''' file next to it, which is then shown.
    ''' </summary>
    Private Async Sub btnSectiuneaB_Click(sender As Object, e As EventArgs) Handles btnSectiuneaB.Click
        Try
            If String.IsNullOrEmpty(_path) OrElse Not File.Exists(_path) Then
                Write("Nu e niciun document deschis. Generați sau deschideți DDF-ul semnat pe Secțiunea A.")
                Return
            End If
            Dim source As String = _path
            Dim before As Byte() = SignedPdfFiles.ReadShared(source)
            LogSignatures("Înainte de Secțiunea B", before)
            If Not PdfSignatures.Read(before, DocType).IsSigned Then
                Write("ATENȚIE: documentul nu e semnat — proba merge mai departe, dar nu spune nimic despre semnătura A.")
            End If

            Dim dataXml As String = Await SectiuneaBXmlAsync(before).ConfigureAwait(True)
            If dataXml Is Nothing Then Return

            ReleaseDocument()   ' Adobe lets go of the file before it is read again
            Dim target As String = SectionBPath(source)
            Dim inPath As String = Path.ChangeExtension(target, ".inainte.pdf")
            Dim xmlPath As String = Path.ChangeExtension(target, ".xml")
            File.WriteAllBytes(inPath, before)
            File.WriteAllText(xmlPath, dataXml, New Text.UTF8Encoding(False))
            Dim notafdNote As String = Await Task.Run(Function() XfaSignedDocument.FillIncremental(inPath, target, xmlPath)).ConfigureAwait(True)

            Dim after As Byte() = File.ReadAllBytes(target)
            Dim prefixKept As Boolean = after.Length > before.Length AndAlso
                                        after.AsSpan(0, before.Length).SequenceEqual(before)
            Write($"Scris «{target}»: {before.Length:N0} ▸ {after.Length:N0} octeți; octeții semnați " &
                  If(prefixKept, "PĂSTRAȚI neatinși (actualizare incrementală).", "MODIFICAȚI — semnătura A e pierdută!"))
            ' Slice 0078-05: the embedded NOTAFD.xml follows section B in the same revision.
            Write(notafdNote)
            LogSignatures("După Secțiunea B", after)
            Write("Documentul cu Secțiunea B se deschide. În Adobe: panoul de semnături pentru A, apoi semnați B și salvați.")
            Open(target)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.btnSectiuneaB_Click", ex)
            Write("Inserarea Secțiunii B a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub btnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        Try
            If String.IsNullOrEmpty(_path) OrElse Not File.Exists(_path) Then
                Write("Nu e niciun document deschis.")
                Return
            End If
            LogSignatures("Acum", SignedPdfFiles.ReadShared(_path))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.btnVerifica_Click", ex)
            Write("Verificarea a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0078-05: empties the viewer (the document is let go, its session ended) and closes
    ''' EVERY Acrobat / Acrobat Reader process of this machine -- the ones Adobe handed the documents
    ''' to are not K-BOT's by PID, so the viewer never closes them and they pile up, each asking
    ''' «save changes?» and holding its files. Asks first: an Adobe the operator opened goes too.
    ''' The log is kept.
    ''' </summary>
    Private Sub btnInchideAdobe_Click(sender As Object, e As EventArgs) Handles btnInchideAdobe.Click
        Try
            ReleaseDocument()
            Dim pids As List(Of Integer) = AdobeWindowHosting.AdobeProcessIds()
            If pids.Count = 0 Then
                Write("Vizualizatorul golit; niciun proces Adobe pornit.")
                Return
            End If
            If KBotMessage.Show(Me, $"Se închid TOATE procesele Adobe de pe calculator ({pids.Count}), " &
                                    "inclusiv documentele deschise în afara K-BOT, fără salvare." &
                                    Environment.NewLine & Environment.NewLine & "Continuați?",
                                "Închide Adobe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
                Write("Vizualizatorul golit; procesele Adobe lăsate pornite.")
                Return
            End If
            For Each pid As Integer In pids
                AdobeWindowHosting.KillPid(pid)
            Next
            Dim left As Integer = AdobeWindowHosting.AdobeProcessIds().Count
            Write($"Vizualizatorul golit; închise {pids.Count} procese Adobe [{String.Join(", ", pids)}]" &
                  If(left > 0, $", {left} încă pornite.", "."))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.btnInchideAdobe_Click", ex)
            Write("Închiderea Adobe a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub btnJurnal_Click(sender As Object, e As EventArgs) Handles btnJurnal.Click
        txtJurnal.Clear()
    End Sub

    ' ── Section B ───────────────────────────────────────────────────────────────
    ''' <summary>
    ''' The fill XML for section B, or Nothing (with the reason written) when there is nothing to
    ''' build it from. The server's section B rows of the revision win; without them, section A's
    ''' rows of the document itself plus the typed code and indicator.
    ''' </summary>
    Private Async Function SectiuneaBXmlAsync(pdfBytes As Byte()) As Task(Of String)
        Dim count As Integer = 0
        Dim cod As String = Nothing
        Dim idrev As Integer
        Dim formXml As String = Await Task.Run(Function() XfaSignedDocument.ReadFormData(pdfBytes)).ConfigureAwait(True)
        If txtCod.Text.Trim().Length > 0 AndAlso Integer.TryParse(txtIdrev.Text.Trim(), idrev) AndAlso
           _session IsNot Nothing AndAlso _session.IsAuthenticated Then
            cod = txtCod.Text.Trim()
            Dim data As DdfInfo = Await LoadDataAsync(cod).ConfigureAwait(True)
            Dim revizie As RevizieRow = data.Revizii.FirstOrDefault(Function(r) r.Idrev = idrev)
            Dim sb As List(Of SectiuneBRow) = data.SectiuneB.Where(Function(s) s.Idrev = idrev).ToList()
            If revizie IsNot Nothing AndAlso sb.Count > 0 Then
                ' The program of section B = the one section A of the SIGNED document carries (the
                ' session's CodProgram was empty on the bench on 28.09.2026 -> an empty Cell3).
                Dim program As String = DdfSectionBInsert.SectionAProgram(formXml)
                If program.Length = 0 Then program = If(_session.CodProgram, String.Empty)
                Dim xml As String = FromServerRows(data, revizie, sb, program)
                Write($"Secțiunea B: {sb.Count} rând(uri) de pe server (IDREV {idrev}), program «{program}», bifa opțiunii 1 pusă.")
                Return xml
            End If
            Write($"Serverul nu are Secțiunea B pentru IDREV {idrev} — se construiește din Secțiunea A a documentului.")
        End If

        Dim codB As String = txtCodB.Text.Trim()
        Dim indicator As String = txtIndicator.Text.Trim()
        If codB.Length = 0 OrElse indicator.Length = 0 Then
            Write("Scrieți codul angajamentului și indicatorul pentru Secțiunea B (le dă FOREXE după trimitere; " &
                  "pentru probă, orice valoare).")
            Return Nothing
        End If
        Dim built As String = FromSectionA(formXml, codB, indicator, count)
        If count = 0 Then
            Write("Secțiunea A nu are rânduri în datele formularului (Table1) — nu am din ce construi Secțiunea B.")
            Return Nothing
        End If
        Write($"Secțiunea B: {count} rând(uri) din Secțiunea A, cod «{codB}», indicator «{indicator}», bifa opțiunii 1 pusă.")
        Return built
    End Function

    ' Section B exactly as the final generation writes it, cut down to that subform so nothing else
    ' of the signed form is rewritten -- the builder the DDF view uses since slice 0078-06 (the bench
    ' sends no captures).
    Private Function FromServerRows(data As DdfInfo, revizie As RevizieRow, sb As List(Of SectiuneBRow),
                                    program As String) As String
        Return DdfSectionBInsert.BuildFillXml(DdfXmlBuilder.Context.FromSession(_session),
                                              data.AntetDeLucru(revizie.Iddf), revizie, sb, Nothing, program)
    End Function

    ''' <summary>
    ''' Section B from section A's rows in the form data (<c>Table1</c>: Cell2 = program, Cell3 =
    ''' SSI, Cell6 = current value) -- a new angajament with no rezervare: nothing before, the
    ''' current values as the influence. Same row shape as <c>DdfXmlBuilder.BuildFormXml</c>, the
    ''' template's empty Row1 first. ASSUMPTION (bench only): section A's Cell3 is the SSI code
    ''' section B wants in Cell4.
    ''' </summary>
    Private Shared Function FromSectionA(formXml As String, cod As String, indicator As String,
                                         ByRef rows As Integer) As String
        rows = 0
        If String.IsNullOrWhiteSpace(formXml) Then Return String.Empty
        Dim doc As XDocument = XDocument.Parse(formXml)
        Dim table1 As XElement = doc.Descendants().FirstOrDefault(Function(x) x.Name.LocalName = "Table1")
        Dim table3 As New XElement("Table3", New XElement("Row1", New XElement("Cell1")))
        If table1 IsNot Nothing Then
            For Each r As XElement In table1.Elements().Where(Function(x) x.Name.LocalName = "Row1")
                Dim program As String = CellText(r, "Cell2")
                Dim ssi As String = CellText(r, "Cell3")
                Dim valoare As String = CellText(r, "Cell6")
                If program.Length = 0 AndAlso ssi.Length = 0 AndAlso valoare.Length = 0 Then Continue For   ' the template row
                table3.Add(New XElement("Row1",
                    New XElement("Cell1", cod), New XElement("Cell2", indicator),
                    New XElement("Cell3", program), New XElement("Cell4", ssi),
                    New XElement("Cell5", "0"), New XElement("Cell6", valoare),
                    New XElement("Cell8", "0"), New XElement("Cell9", valoare)))
                rows += 1
            Next
        End If
        Return WrapForm1(New XElement("SubformSectiuneaB", New XElement("CheckBox9", "1"), table3))
    End Function

    Private Shared Function WrapForm1(sectB As XElement) As String
        Return "<?xml version=""1.0"" encoding=""UTF-8""?>" & New XElement("form1", sectB).ToString()
    End Function

    Private Shared Function CellText(row As XElement, name As String) As String
        Dim cell As XElement = row.Elements().FirstOrDefault(Function(x) x.Name.LocalName = name)
        Return If(cell Is Nothing, String.Empty, cell.Value.Trim())
    End Function

    ' «…_A_HHmmss.pdf» / «…_B_HHmmss.pdf» -> «…_B_<now>.pdf», a NEW name every time: Acrobat keeps
    ' an earlier file open (it is then locked) and hands a reused name to the document it already
    ' has, so a fresh insert could show the OLD content (28.09.2026). Never the source itself.
    Private Shared Function SectionBPath(source As String) As String
        Dim name As String = StampBase(source)
        If name.EndsWith("_A", StringComparison.OrdinalIgnoreCase) OrElse
           name.EndsWith("_B", StringComparison.OrdinalIgnoreCase) Then name = name.Substring(0, name.Length - 2)
        Return StampedPath(Path.GetDirectoryName(source), name & "_B")
    End Function

    ' The file name without extension and without the time stamps this bench appended («_HHmmss»,
    ' possibly repeated, plus a «_2» collision suffix).
    Private Shared Function StampBase(path As String) As String
        Dim name As String = IO.Path.GetFileNameWithoutExtension(path)
        Return System.Text.RegularExpressions.Regex.Replace(name, "(_\d{6}(_\d+)?)+$", "")
    End Function

    ' «<folder>\<base>_HHmmss.pdf»; «_2», «_3»… when that name is already taken in the same second.
    Private Shared Function StampedPath(folder As String, baseName As String) As String
        Dim stamp As String = DateTime.Now.ToString("HHmmss")
        Dim candidate As String = IO.Path.Combine(folder, $"{baseName}_{stamp}.pdf")
        Dim n As Integer = 2
        While File.Exists(candidate)
            candidate = IO.Path.Combine(folder, $"{baseName}_{stamp}_{n}.pdf")
            n += 1
        End While
        Return candidate
    End Function

    ' ── Document on screen ──────────────────────────────────────────────────────
    ''' <summary>
    ''' Shows <paramref name="path"/> with the trap on and a SIMULATED signing session (slice 0078-05;
    ''' nothing is uploaded), configured by «După semnătură».
    ''' </summary>
    Private Sub Open(path As String)
        _path = path
        lblFisier.Text = System.IO.Path.GetFileName(path)
        LogSignatures("Deschis", SignedPdfFiles.ReadShared(path))
        StartSigning(path)
        preview.Signing = _signing
        ' The viewer turns the save after signing on; «Nimic» turns it off again for comparison.
        If cmbDupa.SelectedIndex = 1 Then _signing.SaveAfterSignature = Nothing
        preview.ShowDocument(path, exists:=True)
    End Sub

    Private Sub StartSigning(path As String)
        Dim id As Integer
        If Not Integer.TryParse(txtIdrev.Text.Trim(), id) OrElse id <= 0 Then id = 1
        _signing = New PdfSigningSession(PdfDocKind.Ddf, id, path, path, String.Empty,
                                         RecordingPdfApi.ForUploads(AddressOf OnSimulatedUpload))
        AddHandler _signing.Completed, AddressOf OnSigningCompleted
        _signing.Begin()
        Write($"Sesiune de semnare SIMULATĂ (DDF {id}), după semnătură: «{cmbDupa.Text}».")
    End Sub

    ''' <summary>Adobe holds the file open: the preview lets go before it is read or replaced.</summary>
    Private Sub ReleaseDocument()
        preview.Signing = Nothing
        EndSigning()
        preview.Clear()
        _path = Nothing
        lblFisier.Text = "Niciun document"
    End Sub

    Private Sub EndSigning()
        Dim session As PdfSigningSession = _signing
        _signing = Nothing
        If session IsNot Nothing Then DisposeSession(session)
    End Sub

    Private Sub DisposeSession(session As PdfSigningSession)
        RemoveHandler session.Completed, AddressOf OnSigningCompleted
        session.Dispose()
    End Sub

    ' The recording client got an upload from a session (UI thread): what would have been sent.
    Private Sub OnSimulatedUpload(docType As String, id As Integer, bytes As Byte(), semnatura As String,
                                  records As IReadOnlyList(Of PdfSignatureRecord))
        Try
            Dim fields As String = If(records Is Nothing OrElse records.Count = 0, "niciuna",
                                      String.Join(", ", records.Select(Function(r) $"{r.camp} ({r.rol}, {r.semnatar})")))
            Write($"ÎNCĂRCARE SIMULATĂ {docType} {id}: {bytes.Length:N0} octeți, roluri «{semnatura}», semnături noi: {fields}.")
            LogSignatures("  Trimis", bytes)
            If chkServerBanc.Checked Then SendToBancAsync(docType, id, bytes, semnatura, records, _path)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.OnSimulatedUpload", ex)
        End Try
    End Sub

    ' The same upload, to the bench table on the server. UI-thread async boundary: log and swallow.
    Private Async Sub SendToBancAsync(docType As String, id As Integer, bytes As Byte(), semnatura As String,
                                      records As IReadOnlyList(Of PdfSignatureRecord), localPath As String)
        Try
            Dim client As ApiClient = TryCast(_api, ApiClient)
            If client Is Nothing OrElse Not RequireLogin() Then
                Write("Tabela de probă: nu se trimite (lipsește clientul API sau autentificarea).")
                Return
            End If
            Dim pas As String = $"{cmbDupa.Text}; roluri {semnatura}"
            Dim resp As PutPdfResponse = Await client.UploadBancPdfAsync(
                docType, id, bytes, Nothing, semnatura, records,
                If(localPath Is Nothing, "", IO.Path.GetFileName(localPath)), pas, CancellationToken.None).ConfigureAwait(True)
            Dim same As Boolean = PdfHash.AreEqual(resp.sha256, PdfHash.Compute(bytes))
            Write($"Tabela de probă (KBOT_BANC_PDF): {resp.dimensiune:N0} octeți primiți, sha {PdfSignatureReport.ShortSha(resp.sha256)} " &
                  If(same, "— IDENTIC cu ce s-a trimis.", "— DIFERIT de ce s-a trimis!"))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.SendToBancAsync", ex)
            Write("Tabela de probă: trimiterea a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub OnSigningCompleted(session As PdfSigningSession, outcome As PdfSigningOutcome)
        Try
            If outcome Is Nothing Then Return
            Write($"Sesiune «{IO.Path.GetFileName(session.DisplayedPath)}»: {outcome.Status} — {outcome.Message}")
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.OnSigningCompleted", ex)
        End Try
    End Sub

    ' ── Events ──────────────────────────────────────────────────────────────────
    ' A trapped save finished: the bench reads the file itself.
    Private Async Sub OnDocumentSaved(path As String)
        Try
            Write("SALVAT de capcană: " & path)
            Dim bytes As Byte() = Await SignedPdfFiles.ReadWhenSettledAsync(path).ConfigureAwait(True)
            If bytes Is Nothing Then
                Write("Fișierul salvat nu s-a stabilizat în 10 s — nu a putut fi citit.")
                Return
            End If
            ' Slice 0078-05: right after the «Save As» Adobe has written only an EMPTY placeholder for
            ' the new signature (the bytes come after the token's PIN, in place). Read again until it
            ' is finished, like the signing session does, and say how long it took.
            Dim closed As DateTime = DateTime.Now
            Dim unfinished As List(Of String) = PdfSigningSession.UnfinishedSignatures(bytes)
            If unfinished.Count > 0 Then
                Write($"Semnătura încă se scrie ({String.Join(", ", unfinished)}) — aștept (max. 180 s).")
                While unfinished.Count > 0 AndAlso (DateTime.Now - closed).TotalSeconds < 180 AndAlso Not IsDisposed
                    Await Task.Delay(1000).ConfigureAwait(True)
                    Dim again As Byte() = Await SignedPdfFiles.ReadWhenSettledAsync(path).ConfigureAwait(True)
                    If again IsNot Nothing Then
                        bytes = again
                        unfinished = PdfSigningSession.UnfinishedSignatures(bytes)
                    End If
                End While
                Write(If(unfinished.Count = 0,
                         $"Semnătura terminată în fișier după {(DateTime.Now - closed).TotalSeconds:N1} s.",
                         "Semnătura NU s-a terminat în 180 s."))
            End If
            LogSignatures("După salvare", bytes)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.OnDocumentSaved", ex)
            Write("Citirea după salvare a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub OnSaveCancelled(reason As String)
        Write("SALVARE ANULATĂ de capcană: " & reason)
    End Sub

    ' Raised on the writer's thread. Never throws: AdobeHostLog swallows a failing listener anyway.
    Private Sub OnAdobeLogLine(line As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of String)(AddressOf AppendLine), "[Adobe] " & line)
        Else
            AppendLine("[Adobe] " & line)
        End If
    End Sub

    ' ── Helpers ─────────────────────────────────────────────────────────────────
    ''' <summary>The DDF of <paramref name="cod"/> with section B and attachments; read once per code.</summary>
    Private Async Function LoadDataAsync(cod As String) As Task(Of DdfInfo)
        If _data IsNot Nothing AndAlso String.Equals(_dataCod, cod, StringComparison.OrdinalIgnoreCase) Then Return _data
        Dim data As DdfInfo = Await _api.GetDdfAsync(cod, CancellationToken.None, pentruGenerare:=True).ConfigureAwait(True)
        If data Is Nothing Then Throw New InvalidOperationException("The DDF read returned nothing.")
        _data = data
        _dataCod = cod
        Return data
    End Function

    Private Sub LogSignatures(label As String, bytes As Byte())
        For Each line As String In PdfSignatureReport.Lines(label, bytes, DocType)
            Write(line)
        Next
    End Sub

    Private Function TryReadTarget(ByRef cod As String, ByRef idrev As Integer) As Boolean
        cod = txtCod.Text.Trim()
        If cod.Length > 0 AndAlso Integer.TryParse(txtIdrev.Text.Trim(), idrev) AndAlso idrev > 0 Then Return True
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

    Private Sub Write(line As String)
        AppendLine(line)
        _log?.Invoke(line)
    End Sub

    Private Sub AppendLine(line As String)
        If IsDisposed Then Return
        txtJurnal.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") & "  " & line & Environment.NewLine)
    End Sub

    ' Called from Dispose (see Designer). Must not throw there.
    Private Sub Inchide()
        Try
            If _logHooked Then
                RemoveHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
                _logHooked = False
            End If
            preview.Signing = Nothing
            If _signing IsNot Nothing Then DisposeSession(_signing)
            _signing = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessForm.Inchide", ex)
        End Try
    End Sub

End Class
