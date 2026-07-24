Option Strict On
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Vederea DDF (felia 0020) — echivalentul Access frmFX_MAIN_DDF: un arbore de revizii pe
''' DOUĂ niveluri la stânga (lună/an -> revizie) și, la dreapta, o sub-navigare ORIZONTALĂ
''' (decizia 8) cu trei pagini: «Valori» (grila liniilor de secțiune A + filtru de
''' clasificație), «Vizualizare» (felia 03) și «Fișiere» (felia 04). Read-only.
'''
''' Datele vin dintr-un SINGUR apel GET /api/forexe/ddf pentru tot CodAngajament-ul, prin
''' plasa de re-autentificare a shell-ului (401 -> re-login -> reia o dată). Un click în
''' arbore FILTREAZĂ datele deja încărcate — nu declanșează nicio cerere de rețea
''' (decizia 7).
'''
''' ABATERI DELIBERATE de la Access (motivate în worklog-ul feliei):
'''   * valoarea unei frunze este SUM(ValCur) peste revizie, calculat pe server —
'''     Access aliază `SA.ValCur AS TotalRevizie` și afișează o linie ARBITRARĂ;
'''   * valoarea unei rădăcini de lună este suma reală a frunzelor ei — Access trimite
'''     literalul `0` în AddTree_Root;
'''   * o rădăcină e roșie doar când PROPRIUL ei total e negativ — Access copiază culoarea
'''     ultimei frunze procesate în părinte (`cRoot.foreColor = cNode.foreColor`), ceea ce
'''     face culoarea rădăcinii să depindă de ordinea de parcurgere. Accidental, nu intenționat.
''' </summary>
Public Class DdfView
    Implements IAngajamentView, IThemedControl

    ' Cheile paginilor sub-navigării — o singură definiție, folosită la creare și la comutare.
    Private Const PAGE_VALORI As String = "valori"
    Private Const PAGE_PREVIEW As String = "previzualizare"
    ' «Document» = PDF-ul REAL (ReaderHostPreview), distinct de «Vizualizare» (reconstrucția XFA).
    Private Const PAGE_PDF As String = "document"
    Private Const PAGE_FISIERE As String = "fisiere"

    ' Cheile coloanelor grilei — o singură definiție, folosită la creare și la umplere.
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_ELEMENT As String = "element"
    Private Const COL_DATA As String = "data"
    Private Const COL_VALPREC As String = "valprec"
    Private Const COL_VALCUR As String = "valcur"
    Private Const COL_VALTOT As String = "valtot"

    ''' <summary>Prima intrare a combo-ului de clasificații — „fără filtru".</summary>
    Private Const CLSF_TOATE As String = "< Arată toate clasificațiile >"

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe DdfInfo.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo))
    ' Sesiunea (globalii unității pentru constructorul de XML, felia 05). Poate fi Nothing în
    ' teste — atunci contextul de generare e gol.
    Private ReadOnly _session As SessionContext

    ' Codul angajamentului CERUT ultima dată — stale-guard (identic cu Plăți/Recepții/Rezervări).
    Private _requestedCod As String
    ' IDDF preferat, din nodul de arbore al shell-ului — alege antetul când sunt mai multe.
    Private _preferredIddf As Integer

    ' Ultimele date încărcate — păstrate ca ApplyTheme să reconstruiască arborele
    ' (re-tintarea iconițelor) fără o nouă cerere de rețea.
    Private _revizii As List(Of RevizieRow)
    Private _liniiByRev As Dictionary(Of Integer, List(Of LinieSaRow))
    ' Antetul de lucru — poartă CUAL / PartAng / NumePartener pentru calea PDF (feliile 03-04).
    Private _antet As DdfAntet

    ' Rândurile nodului selectat acum — sursa grilei ȘI a combo-ului de clasificații.
    Private _nodeRows As List(Of LinieSaRow)
    ' Nodul selectat e o rădăcină de lună? Doar atunci coloana «Data reviziei» are sens.
    Private _nodeIsRoot As Boolean
    ' Revizia frunzei previzualizate acum — ținta generării (felia 05).
    Private _selectedRevizie As RevizieRow
    ' O generare e în curs? Blochează re-invocarea butonului.
    Private _generating As Boolean
    ' Se reconstruiește combo-ul chiar acum? Blochează re-filtrarea din SelectedIndexChanged.
    Private _suppressComboEvent As Boolean

    ' Suprafața de previzualizare (felia 03), aleasă la compilare de DdfPreviewFactory. O
    ' singură instanță, montată în pnlPreview, refolosită și de pagina «Fișiere» (felia 04).
    Private ReadOnly _preview As IDdfPreview
    ' Browserul de fișiere PDF (felia 04), montat în pnlFisiere.
    Private ReadOnly _browser As DdfFileBrowser

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo)),
                   Optional session As SessionContext = Nothing)
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If withReauth Is Nothing Then Throw New ArgumentNullException(NameOf(withReauth))
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _session = session
        ConfigureTree()
        BuildNav()
        BuildColumns()
        _preview = DdfPreviewFactory.Create()
        MountPreview()
        _browser = New DdfFileBrowser()
        MountBrowser()
        ResetClsfCombo(Nothing)
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    ' Montează suprafața de previzualizare aleasă la compilare în pagina «Vizualizare» și se
    ' abonează la butonul «Generează documentul» (felia 05 tratează generarea).
    Private Sub MountPreview()
        Try
            Dim surface As Control = _preview.Surface
            surface.Dock = DockStyle.Fill
            ' Suprafața acoperă panoul; eticheta goală rămâne dedesubt ca fallback.
            pnlPreview.Controls.Add(surface)
            surface.BringToFront()
            AddHandler _preview.GenerateRequested, AddressOf OnGenerateRequested
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.MountPreview", ex)
            Throw
        End Try
    End Sub

    ' Montează browserul de fișiere în pagina «Fișiere» și se abonează la selecția unui rând.
    Private Sub MountBrowser()
        Try
            lblFisiereGol.Visible = False
            _browser.Dock = DockStyle.Fill
            pnlFisiere.Controls.Add(_browser)
            _browser.BringToFront()
            AddHandler _browser.FileActivated, AddressOf OnFileActivated
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.MountBrowser", ex)
            Throw
        End Try
    End Sub

    ' Un fișier ales din browser -> aceeași suprafață de previzualizare ca pagina «Vizualizare»
    ' (planul §7: o singură suprafață, două puncte de intrare), apoi comutăm pe acea pagină.
    Private Sub OnFileActivated(pdfPath As String)
        Try
            If String.IsNullOrWhiteSpace(pdfPath) Then Return
            _preview.ShowDocument(pdfPath, IO.File.Exists(pdfPath))
            navSub.SelectedKey = PAGE_PREVIEW      ' ridică SelectionChanged -> ShowPage
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.OnFileActivated", ex)
        End Try
    End Sub

    ' «Generează documentul» de pe suprafața „document lipsă" (felia 05). Boundary UI async:
    ' loghează și înghite (NU rearuncă — nu există await care să prindă).
    Private Async Sub OnGenerateRequested(sender As Object, e As EventArgs)
        Try
            If _generating Then Return
            Dim cod As String = _requestedCod
            Dim revizie As RevizieRow = _selectedRevizie
            If String.IsNullOrWhiteSpace(cod) OrElse revizie Is Nothing OrElse _antet Is Nothing Then Return

            _generating = True
            Try
                ' 1. Datele de generare (secțiunea B + atașamentele) — un apel opt-in.
                Dim data As DdfInfo = Await _withReauth(
                    Function() _apiClient.GetDdfAsync(cod, CancellationToken.None, pentruGenerare:=True)).ConfigureAwait(True)
                If data Is Nothing Then Return
                ' Ținta s-a schimbat între timp? Renunțăm.
                If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

                ' 2. Doar rândurile reviziei-țintă (generarea e per revizie, ca tmpFX_* din Access).
                Dim liniiRev = data.Linii.Where(Function(l) l.Idrev = revizie.Idrev).ToList()
                Dim sbRev = data.SectiuneB.Where(Function(s) s.Idrev = revizie.Idrev).ToList()
                Dim attRev = data.Atasamente.Where(Function(a) a.Idrev = revizie.Idrev).ToList()

                ' 3. XML-ul complet (form1 + NOTAFD + atașamente).
                Dim ctx As DdfXmlBuilder.Context = DdfXmlBuilder.Context.FromSession(_session)
                Dim xml As String = DdfXmlBuilder.BuildComplete(ctx, _antet, revizie, liniiRev, sbRev, attRev)

                ' 4. Calea PDF (§2.5) + siblingul .xml, sub folderul partener/GENERAL (creat la nevoie).
                Dim pdfPath As String = DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, _antet, revizie.NumarRev)
                If String.IsNullOrEmpty(pdfPath) Then Return
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(pdfPath))
                Dim xmlPath As String = IO.Path.ChangeExtension(pdfPath, ".xml")
                IO.File.WriteAllText(xmlPath, xml, New Text.UTF8Encoding(False))

                ' 5. Generarea PROPRIU-ZISĂ pe thread de fundal (descarcă macheta, completează XFA,
                ' embedează atașamentele, scrie PDF-ul). XfaWriter loghează + rearuncă la graniță;
                ' NU adăugăm un al doilea strat de catch în jur — îl lăsăm să urce în catch-ul de aici.
                Await Task.Run(Sub() KBot.Xfa.XfaWriter.Genereaza(xmlPath, pdfPath, "DDF", deschidePdf:=False)).ConfigureAwait(True)

                ' 6. Fără scriere înapoi în bază (§2.4 — cele patru coloane nu există). Existența
                ' se decide prin scanare de disc: reîmprospătăm browserul și previzualizarea.
                _browser.SetContext(KBotPaths.Current.DdfPdfRoot, cod)
                _preview.ShowDocument(pdfPath, IO.File.Exists(pdfPath))
            Finally
                _generating = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.OnGenerateRequested", ex)
        End Try
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "ddf"
        End Get
    End Property

    ' Font + comportament neacoperite de Designer (setter-ele reconstruiesc fontul intern).
    Private Sub ConfigureTree()
        Try
            tree.FontName = "Segoe UI"
            tree.FontSize = 9.0F
            tree.RootExpander = True
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.ConfigureTree", ex)
            Throw
        End Try
    End Sub

    ' Sub-navigarea orizontală: trei pagini, «Valori» selectată implicit.
    Private Sub BuildNav()
        Try
            navSub.AddItem(PAGE_VALORI, "Valori")
            navSub.AddItem(PAGE_PREVIEW, "Vizualizare")
            navSub.AddItem(PAGE_FISIERE, "Fișiere")
            navSub.SelectedKey = PAGE_VALORI
            ShowPage(PAGE_VALORI)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.BuildNav", ex)
            Throw
        End Try
    End Sub

    ' Coloanele grilei = liniile de secțiune A (decizia 4: FĂRĂ Clasificatii.Denumire).
    ' «Element de fundamentare» e SINGURA coloană cu auto-ascundere — prima care dispare când
    ' spațiul e strâmt. Rând de totaluri cu Sum DOAR pe «Valoare curentă» (decizia 5).
    ' Coloanele de bani sunt Text cu FormatString N2 + aliniere dreapta, iar valorile se pun
    ' ca Double, ca agregatul să poată însuma (același tipar ca PlatiView).
    Private Sub BuildColumns()
        Try
            grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 170)

            Dim colElement As KBotDataColumn =
                grid.AddColumn(COL_ELEMENT, "Element de fundamentare", KBotColumnType.Text, 220)
            ' Auto-ascunderea scanează de la dreapta la stânga dar SARE coloanele fără AutoHide,
            ' deci funcționează și pe o coloană care nu e ultima. Ținta de umplere (ultima
            ' coloană, ColumnFillMode.LastColumn) e protejată — aici e «Valoare totală», nu asta.
            colElement.AutoHide = True

            ' Vizibilă DOAR pe rădăcina de lună: acolo grila e o listă plată peste mai multe
            ' revizii, iar ordonarea „Clsf, DataRev" ar fi ilizibilă fără ea.
            grid.AddColumn(COL_DATA, "Data reviziei", KBotColumnType.Text, 110)

            Dim colPrec As KBotDataColumn =
                grid.AddColumn(COL_VALPREC, "Valoare precedentă", KBotColumnType.Text, 130)
            colPrec.FormatString = "N2"
            colPrec.TextAlign = ContentAlignment.MiddleRight

            Dim colCur As KBotDataColumn =
                grid.AddColumn(COL_VALCUR, "Valoare curentă", KBotColumnType.Text, 130)
            colCur.FormatString = "N2"
            colCur.TextAlign = ContentAlignment.MiddleRight
            colCur.Aggregate = KBotAggregate.Sum        ' singurul agregat (decizia 5)

            Dim colTot As KBotDataColumn =
                grid.AddColumn(COL_VALTOT, "Valoare totală", KBotColumnType.Text, 130)
            colTot.FormatString = "N2"
            colTot.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.BuildColumns", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare) sau
    ''' fără DDF (<c>AreDDF = False</c>) NU se face niciun apel de rețea — doar se golește vederea.
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = If(info Is Nothing, Nothing, info.CodAngajament)
            If String.IsNullOrWhiteSpace(cod) Then
                ClearAll()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If
            If Not info.AreDDF Then
                ' Intrarea de navigare e deja ascunsă de shell; aici doar nu cerem nimic.
                ClearAll()
                ShowEmpty("Angajamentul nu are document de fundamentare.")
                Return
            End If

            _requestedCod = cod
            _preferredIddf = If(info.IDDF.HasValue, CInt(info.IDDF.Value), 0)
            ShowEmpty("Se încarcă documentul de fundamentare…")
            ' Fire-and-forget deliberat (handler sincron al shell-ului): metoda își tratează
            ' singură TOATE erorile — vezi comentariul din PlatiView/ReceptiiView.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai departe (apelul e pornit fără
    ' await din SetContext, deci nu există cine să o prindă).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As DdfInfo = Await _withReauth(
                Function() _apiClient.GetDdfAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament. Îl aruncăm.
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            Dim revizii As List(Of RevizieRow) =
                If(data Is Nothing, New List(Of RevizieRow)(), data.Revizii)
            If revizii Is Nothing OrElse revizii.Count = 0 Then
                ClearAll()
                ShowEmpty("Angajamentul nu are revizii.")
                Return
            End If

            ' Antetul de lucru: cel cu IDDF-ul nodului de arbore, altfel primul. Nimic tacit —
            ' când sunt mai multe antete o spunem în jurnal (schema le permite, §2.7).
            _antet = data.AntetDeLucru(_preferredIddf)
            If data.Antet IsNot Nothing AndAlso data.Antet.Count > 1 Then
                GlobalErrorLog.Write("DdfView.LoadAsync",
                    New InvalidOperationException(
                        $"Angajamentul {cod} are {data.Antet.Count} antete FX_DDF; " &
                        $"s-a ales IDDF={If(_antet Is Nothing, 0, _antet.Iddf)}."))
            End If

            _revizii = revizii
            _liniiByRev = GroupLinii(data.Linii)
            BuildTree(revizii)
            ' Nimic selectat -> grila e goală; se umple la click pe orice nod al arborelui.
            _nodeRows = Nothing
            _nodeIsRoot = False
            grid.ClearRows()
            ResetClsfCombo(Nothing)
            ' Browserul de fișiere: PDF-urile angajamentului sub rădăcina configurată.
            _browser.SetContext(KBotPaths.Current.DdfPdfRoot, cod)
            _preview.Clear()
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("DdfView.LoadAsync", ex)
            ClearAll()
            ShowEmpty(ex.Message)   ' mesaj românesc din câmpul «error» al serverului
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("DdfView.LoadAsync", ex)
            ClearAll()
            ShowEmpty("Documentul de fundamentare nu a putut fi încărcat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' Liniile de secțiune A, grupate pe revizie. O revizie fără linii pur și simplu lipsește
    ' din dicționar -> nodul ei primește o listă goală (rămâne vizibil, cu total 0).
    Private Shared Function GroupLinii(linii As List(Of LinieSaRow)) As Dictionary(Of Integer, List(Of LinieSaRow))
        Dim map As New Dictionary(Of Integer, List(Of LinieSaRow))()
        If linii Is Nothing Then Return map
        For Each l As LinieSaRow In linii
            Dim bucket As List(Of LinieSaRow) = Nothing
            If Not map.TryGetValue(l.Idrev, bucket) Then
                bucket = New List(Of LinieSaRow)()
                map(l.Idrev) = bucket
            End If
            bucket.Add(l)
        Next
        Return map
    End Function

    Private Function LiniiFor(idrev As Integer) As List(Of LinieSaRow)
        Dim bucket As List(Of LinieSaRow) = Nothing
        If _liniiByRev IsNot Nothing AndAlso _liniiByRev.TryGetValue(idrev, bucket) Then Return bucket
        Return New List(Of LinieSaRow)()
    End Function

    ' ── Arborele ─────────────────────────────────────────────────────────────
    ' DOUĂ niveluri: rădăcină de lună (cheia «LA_{yyyy}_{M}», valoarea = suma frunzelor ei)
    ' -> frunză de revizie (cheia «RC_{IDREV}», valoarea = TotalRevizie). Fiecare nod poartă
    ' în Tag liniile pe care le acoperă, ca un click să filtreze grila fără cerere de rețea.
    ' Rădăcinile sunt EXPANDATE (planul §5).
    Private Sub BuildTree(revizii As List(Of RevizieRow))
        Try
            tree.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            Dim monthGroups = revizii.GroupBy(Function(r) MonthKeyOf(r.DataRev)).
                                      OrderBy(Function(g) g.Key)

            For Each mg In monthGroups
                Dim monthRevs As List(Of RevizieRow) = mg.ToList()
                ' Valoarea rădăcinii = suma TOTALURILOR frunzelor ei (Access trimite literalul 0).
                Dim monthSum As Double = monthRevs.Sum(Function(r) r.TotalRevizie)
                Dim monthLines As New List(Of LinieSaRow)()
                For Each r As RevizieRow In monthRevs
                    monthLines.AddRange(LiniiFor(r.Idrev))
                Next

                Dim monthIcon As Image = If(palette Is Nothing, Nothing,
                                            DdfIcons.LunaIcon(palette.TextDimColor, tree.LeftIconSize.Width))
                Dim monthItem As AdvancedTreeControl.TreeItem =
                    tree.AddItem(MonthKeyText(mg.Key), $"{MonthYearLabel(mg.Key)}~~~{Money(monthSum)}",
                                 pLeftIconClosed:=monthIcon, pLeftIconOpen:=monthIcon,
                                 pExpanded:=True)
                monthItem.Tag = New DdfNodeRows(monthLines, isRoot:=True)
                ' Roșu doar când PROPRIUL total e negativ (Access copiază culoarea ultimei frunze).
                If monthSum < 0 AndAlso palette IsNot Nothing Then
                    monthItem.NodeForeColor = palette.ErrorColor
                End If

                ' Frunze de revizie, în ordinea serverului (DataRev, NumarRev).
                For Each r As RevizieRow In monthRevs
                    Dim leafIcon As Image = IconFor(StareOf(r), palette)
                    Dim leafItem As AdvancedTreeControl.TreeItem =
                        tree.AddItem($"RC_{r.Idrev}", $"{r.EtichetaRevizie}~~~{Money(r.TotalRevizie)}",
                                     monthItem, pLeftIconClosed:=leafIcon, pLeftIconOpen:=leafIcon)
                    leafItem.Tag = New DdfNodeRows(LiniiFor(r.Idrev), isRoot:=False, revizie:=r)
                    leafItem.Tooltip = r.DescScurta
                    If r.TotalRevizie < 0 AndAlso palette IsNot Nothing Then
                        leafItem.NodeForeColor = palette.ErrorColor
                    End If
                Next
            Next

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' Click pe orice nod -> resetează combo-ul din rândurile nodului, apoi umple grila
    ' NEFILTRATĂ. Fără apel de rețea (decizia 7).
    Private Sub tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim payload As DdfNodeRows = TryCast(pNode.Tag, DdfNodeRows)
            If payload Is Nothing Then Return

            _nodeRows = payload.Linii
            _nodeIsRoot = payload.IsRoot
            ' «Data reviziei» are sens doar pe rădăcină (listă plată peste mai multe revizii).
            grid.Column(COL_DATA).Visible = _nodeIsRoot

            ' Resetul e NECONDIȚIONAT (decizia 6): nu încearcă să păstreze selecția anterioară.
            ResetClsfCombo(_nodeRows)
            FillGrid(_nodeRows)

            ' Previzualizarea (planul §7): o rădăcină de lună NU are un singur document -> se
            ' golește; o frunză -> se calculează calea așteptată (§2.5) din antet (CUAL/PartAng/
            ' NumePartener) + NumarRev-ul reviziei și se dă previzualizării cu flag-ul de existență.
            If _nodeIsRoot Then
                _preview.Clear()
            Else
                LinkPreviewLaFrunza(payload.Revizie)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' Calea așteptată a PDF-ului reviziei (planul §2.5), din antetul de lucru + NumarRev, apoi
    ' o dă previzualizării cu flag-ul de existență de pe disc. Fără antet -> golește.
    Private Sub LinkPreviewLaFrunza(revizie As RevizieRow)
        _selectedRevizie = revizie      ' ținta unei eventuale generări (felia 05)
        If revizie Is Nothing OrElse _antet Is Nothing Then
            _preview.Clear()
            Return
        End If
        Dim path As String = DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, _antet, revizie.NumarRev)
        If String.IsNullOrEmpty(path) Then
            _preview.Clear()
            Return
        End If
        _preview.ShowDocument(path, IO.File.Exists(path))
    End Sub

    ' ── Combo-ul de clasificații ─────────────────────────────────────────────
    ' Valorile DISTINCTE ale rândurilor încărcate acum, sortate, cu «toate» pe prima poziție
    ' și selectată. Nu emite cereri; re-filtrează doar ce e deja în memorie.
    Private Sub ResetClsfCombo(rows As List(Of LinieSaRow))
        _suppressComboEvent = True
        Try
            cboClsf.BeginUpdate()
            cboClsf.Items.Clear()
            cboClsf.Items.Add(CLSF_TOATE)
            If rows IsNot Nothing Then
                Dim distincte = rows.Select(Function(r) If(r.Clsf, String.Empty)).
                                     Where(Function(s) Not String.IsNullOrWhiteSpace(s)).
                                     Distinct(StringComparer.Ordinal).
                                     OrderBy(Function(s) s, StringComparer.Ordinal)
                For Each c As String In distincte
                    cboClsf.Items.Add(c)
                Next
            End If
            cboClsf.SelectedIndex = 0
        Finally
            cboClsf.EndUpdate()
            _suppressComboEvent = False
        End Try
    End Sub

    Private Sub cboClsf_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboClsf.SelectedIndexChanged
        Try
            If _suppressComboEvent Then Return
            FillGrid(FilteredRows())
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.cboClsf_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' Rândurile nodului, filtrate pe clasificația aleasă. Prima intrare = fără filtru.
    Private Function FilteredRows() As List(Of LinieSaRow)
        If _nodeRows Is Nothing Then Return New List(Of LinieSaRow)()
        If cboClsf.SelectedIndex <= 0 Then Return _nodeRows
        Dim wanted As String = TryCast(cboClsf.SelectedItem, String)
        If String.IsNullOrEmpty(wanted) Then Return _nodeRows
        Return _nodeRows.Where(Function(r) String.Equals(r.Clsf, wanted, StringComparison.Ordinal)).ToList()
    End Function

    ' ── Grila ────────────────────────────────────────────────────────────────
    ' Ordonare: pe rădăcină «Clsf, DataRev» (listă plată peste revizii), pe frunză doar «Clsf».
    Private Sub FillGrid(rows As List(Of LinieSaRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows IsNot Nothing Then
                For Each r As LinieSaRow In SortRows(rows)
                    Dim rev As RevizieRow = RevizieOf(r.Idrev)
                    Dim row As KBotDataRow = grid.AddRow()
                    row.Tag = r
                    row(COL_CLSF) = r.Clsf
                    row(COL_ELEMENT) = r.ElementFund
                    row(COL_DATA) = If(rev Is Nothing, String.Empty, ShortDate(rev.DataRev))
                    row(COL_VALPREC) = r.ValPrec
                    row(COL_VALCUR) = r.ValCur
                    row(COL_VALTOT) = r.ValTot
                Next
            End If
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Private Function SortRows(rows As List(Of LinieSaRow)) As List(Of LinieSaRow)
        If _nodeIsRoot Then
            Return rows.OrderBy(Function(r) If(r.Clsf, String.Empty), StringComparer.Ordinal).
                        ThenBy(Function(r) DataRevOf(r.Idrev)).ToList()
        End If
        Return rows.OrderBy(Function(r) If(r.Clsf, String.Empty), StringComparer.Ordinal).ToList()
    End Function

    Private Function RevizieOf(idrev As Integer) As RevizieRow
        If _revizii Is Nothing Then Return Nothing
        For Each r As RevizieRow In _revizii
            If r.Idrev = idrev Then Return r
        Next
        Return Nothing
    End Function

    Private Function DataRevOf(idrev As Integer) As Date
        Dim r As RevizieRow = RevizieOf(idrev)
        If r Is Nothing OrElse Not r.DataRev.HasValue Then Return Date.MaxValue
        Return r.DataRev.Value
    End Function

    ' ── Sub-navigarea ────────────────────────────────────────────────────────
    Private Sub navSub_SelectionChanged(key As String) Handles navSub.SelectionChanged
        Try
            ShowPage(key)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.navSub_SelectionChanged", ex)
        End Try
    End Sub

    ' O singură pagină vizibilă odată — același tipar lazy ca gazda de vederi din MainForm,
    ' NU un TabControl.
    Private Sub ShowPage(key As String)
        pnlValori.Visible = String.Equals(key, PAGE_VALORI, StringComparison.Ordinal)
        pnlPreview.Visible = String.Equals(key, PAGE_PREVIEW, StringComparison.Ordinal)
        pnlFisiere.Visible = String.Equals(key, PAGE_FISIERE, StringComparison.Ordinal)
    End Sub

    ' ── Stare goală / conținut ───────────────────────────────────────────────
    Private Sub ClearAll()
        _requestedCod = Nothing
        _revizii = Nothing
        _liniiByRev = Nothing
        _antet = Nothing
        _nodeRows = Nothing
        _nodeIsRoot = False
        _selectedRevizie = Nothing
        tree.Clear()
        grid.ClearRows()
        ResetClsfCombo(Nothing)
        _preview?.Clear()
        _browser?.SetContext(Nothing, Nothing)
    End Sub

    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        split.Visible = False
    End Sub

    Private Sub ShowContent()
        lblEmpty.Visible = False
        split.Visible = True
    End Sub

    ' ── Formatare / iconițe ──────────────────────────────────────────────────
    Private Shared Function Money(value As Double) As String
        Return value.ToString("N2", _roCulture)
    End Function

    Private Shared Function ShortDate(value As Date?) As String
        If Not value.HasValue Then Return String.Empty
        Return value.Value.ToString("dd.MM.yyyy", _roCulture)
    End Function

    ' Cheia de lună = an*100 + lună din DataRev (0 dacă lipsește). Ordonabilă cronologic.
    Private Shared Function MonthKeyOf(value As Date?) As Integer
        If Not value.HasValue Then Return 0
        Return value.Value.Year * 100 + value.Value.Month
    End Function

    ' Cheia de nod a rădăcinii: «LA_{yyyy}_{M}» (planul §5). Fără dată -> «LA_0_0».
    Private Shared Function MonthKeyText(monthKey As Integer) As String
        If monthKey <= 0 Then Return "LA_0_0"
        Return $"LA_{monthKey \ 100}_{monthKey Mod 100}"
    End Function

    Private Shared Function MonthYearLabel(monthKey As Integer) As String
        If monthKey <= 0 Then Return "(fără dată)"
        Dim y As Integer = monthKey \ 100
        Dim m As Integer = monthKey Mod 100
        Return $"{MonthLabel(m)}/{y}"
    End Function

    ' Numele lunii în română (Ianuarie…), cu prima literă mare (ca în Plăți/Recepții/Rezervări).
    Private Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return CStr(month)
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return CStr(month)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    ' Starea vizuală a unei revizii: Incarcat->sus (REV_SUS), altfel Preluat->jos (REV_JOS),
    ' altfel neutru (REV_NOT) — regula din frmFX_MAIN_DDF.Show_Revizii.
    Private Shared Function StareOf(r As RevizieRow) As DdfIcons.Stare
        If r.Incarcat Then Return DdfIcons.Stare.Sus
        If r.Preluat Then Return DdfIcons.Stare.Jos
        Return DdfIcons.Stare.Neutru
    End Function

    ' Iconița stării, cu culoarea din paletă după stare (sus=succes, jos=accent, neutru=estompat).
    Private Function IconFor(stare As DdfIcons.Stare, palette As ThemePalette) As Image
        If palette Is Nothing Then Return Nothing
        Dim color As Color
        Select Case stare
            Case DdfIcons.Stare.Sus : color = palette.SuccessColor
            Case DdfIcons.Stare.Jos : color = palette.AccentColor
            Case Else : color = palette.TextDimColor
        End Select
        Return DdfIcons.StatusIcon(stare, color, tree.LeftIconSize.Width)
    End Function

    Private Shared Function TryGetPalette() As ThemePalette
        ' Headless (teste) sau înainte de inițializarea temei: ThemeManager.Current poate fi
        ' Nothing. Atunci arborele se construiește fără iconițe/culori (structura e aceeași),
        ' iar ApplyTheme reconstruiește când tema devine disponibilă.
        Dim current As ThemeScheme = ThemeManager.Current
        Return If(current Is Nothing, Nothing, current.Palette)
    End Function

    ''' <summary>
    ''' Reaplică culorile schemei pe arbore, banda de titlu, filtrul de clasificație și
    ''' paginile goale (grila și sub-navigarea se auto-temează: implementează ele însele
    ''' IThemedControl). Reconstruiește arborele dacă are date, ca iconițele să se re-tinteze.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor

            pnlTreeHead.BackColor = p.SurfaceAltColor
            lblTreeTitle.ForeColor = p.TextColor
            lblTreeTitle.BackColor = Color.Transparent

            tree.BackColor = p.SurfaceAltColor
            tree.ForeColor = p.TextColor
            tree.HoverBackColor = p.ButtonHoverColor
            tree.SelectedBackColor = p.ButtonPressedColor
            tree.SelectedBorderColor = p.AccentColor
            tree.LineColor = p.BorderColor
            tree.HeaderBackColor = p.SurfaceAltColor
            tree.HeaderForeColor = p.TextColor

            pnlPages.BackColor = p.SurfaceAltColor
            pnlValori.BackColor = p.SurfaceAltColor
            pnlFilter.BackColor = p.SurfaceAltColor
            lblClsf.ForeColor = p.TextDimColor
            lblClsf.BackColor = Color.Transparent
            cboClsf.BackColor = p.InputBackColor
            cboClsf.ForeColor = p.InputTextColor

            pnlPreview.BackColor = p.SurfaceAltColor
            lblPreviewGol.ForeColor = p.TextDimColor
            lblPreviewGol.BackColor = p.SurfaceAltColor
            pnlFisiere.BackColor = p.SurfaceAltColor
            lblFisiereGol.ForeColor = p.TextDimColor
            lblFisiereGol.BackColor = p.SurfaceAltColor

            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor

            ' Cascada temei spre suprafața de previzualizare + browserul de fișiere.
            Dim themedPreview As IThemedControl = TryCast(_preview, IThemedControl)
            themedPreview?.ApplyTheme(scheme)
            _browser?.ApplyTheme(scheme)

            ' Re-tintarea iconițelor pe noua paletă. Arborele se reconstruiește, deci selecția
            ' se pierde — grila rămâne cum e până la următorul click pe un nod.
            If _revizii IsNot Nothing AndAlso _revizii.Count > 0 Then
                BuildTree(_revizii)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("DdfView.ApplyTheme", ex)
        End Try
    End Sub

End Class

''' <summary>
''' Ce acoperă un nod din arborele DDF: liniile de secțiune A pe care le arată grila, dacă
''' nodul e o rădăcină de lună (atunci coloana «Data reviziei» devine vizibilă) și — pentru
''' frunze — revizia însăși, de care feliile 03/04 au nevoie ca să compună calea PDF-ului.
''' POCO -> fără Try/Catch.
''' </summary>
Friend NotInheritable Class DdfNodeRows
    Public ReadOnly Property Linii As List(Of LinieSaRow)
    Public ReadOnly Property IsRoot As Boolean
    ''' <summary>Revizia frunzei; Nothing pe o rădăcină de lună.</summary>
    Public ReadOnly Property Revizie As RevizieRow

    Public Sub New(linii As List(Of LinieSaRow), isRoot As Boolean, Optional revizie As RevizieRow = Nothing)
        Me.Linii = If(linii, New List(Of LinieSaRow)())
        Me.IsRoot = isRoot
        Me.Revizie = revizie
    End Sub
End Class
