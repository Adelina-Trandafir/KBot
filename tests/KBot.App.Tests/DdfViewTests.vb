Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.App

' Headless behaviour + shaping tests for DdfView (slice 0020-02). They cover what no server
' test can reach: a null/blank/AreDDF-False context must NOT hit the network; a response must
' shape into the 2-level month/revision tree; the month root must carry the REAL sum of its
' leaves (Access sends the literal 0); a root must go red only on its OWN negative total (Access
' copies the last leaf's colour into the parent); and a STALE response must be discarded.
'
' Slice 0032 split the four page panels out into standalone UserControls, so the tests that used
' to reach through DdfView for the grid, the preview surfaces and the Adobe combos now drive the
' PAGES directly (DdfValoriPage / DdfDocumentPage / …) — which is exactly how the host drives
' them: build a DdfPageContext, push it, assert what rendered. The clsf-filter tests are gone
' with the feature itself.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so Async Sub continuations need Application.DoEvents()
' to pump. Same pattern as PlatiViewTests / ReceptiiViewTests.
Public Class DdfViewTests

    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly RequestedCods As New List(Of String)()
        Public ReadOnly GenerareCods As New List(Of String)()
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of DdfInfo))(StringComparer.Ordinal)

        Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                    Optional pentruGenerare As Boolean = False) _
            As Task(Of DdfInfo) Implements IApiClient.GetDdfAsync
            RequestedCods.Add(cod)
            If pentruGenerare Then GenerareCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of DdfInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As DdfInfo)
            Pending(cod).SetResult(data)
        End Sub

        ' --- restul contractului: nefolosit aici ---
        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo) _
            Implements IApiClient.GetPlatiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo) _
            Implements IApiClient.GetReceptiiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo) _
            Implements IApiClient.GetRezervariAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo) _
            Implements IApiClient.GetSumarAsync
            Throw New NotSupportedException()
        End Function

        Public Function UpsertAngajamenteAsync(dbName As String, rows As IReadOnlyList(Of Angajament),
                                               ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.UpsertAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                            ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) _
            Implements IApiClient.GetAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                                     ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo)) _
            Implements IApiClient.GetTreeAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetIstoricAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo) _
            Implements IApiClient.GetIstoricAsync
            Throw New NotSupportedException()
        End Function


        Public Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.ProcessExcelAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) _
            Implements IApiClient.GetAsync
            Throw New NotSupportedException()
        End Function

        Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest,
                                                          ct As CancellationToken) As Task(Of TResponse) _
            Implements IApiClient.PostAsync
            Throw New NotSupportedException()
        End Function
    End Class

    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod, .AreDDF = True}
    End Function

    Private Shared Function Rev(idrev As Integer, numar As Integer, d As Date, total As Double,
                                Optional incarcat As Boolean = False,
                                Optional preluat As Boolean = False,
                                Optional desc As String = "") As RevizieRow
        Return New RevizieRow() With {
            .Idrev = idrev, .Iddf = 1, .NumarRev = numar, .DataRev = d, .TotalRevizie = total,
            .Incarcat = incarcat, .Preluat = preluat,
            .DescScurta = If(desc = "", "Revizia " & numar.ToString(), desc)
        }
    End Function

    Private Shared Function Linie(idSecA As Integer, idrev As Integer, clsf As String,
                                  valCur As Double, Optional element As String = "Element") As LinieSaRow
        Return New LinieSaRow() With {
            .IdSecA = idSecA, .Idrev = idrev, .IdClsf = 141, .Clsf = clsf,
            .ElementFund = element, .ParametriiFund = "P",
            .ValPrec = 0.0, .ValCur = valCur, .ValTot = valCur
        }
    End Function

    ' Set standard: DOUĂ luni.
    '  Ian: R1 (18 Ian, 3 linii: 100+200+300 = 600, Incarcat) și
    '       R2 (30 Ian, 1 linie: 50, Preluat)                    -> rădăcina Ian = 650
    '  Feb: R3 (11 Feb, ZERO linii, total 0) și
    '       R4 (12 Feb, 1 linie: -900)                            -> rădăcina Feb = -900 (roșie)
    Private Shared Function StandardData() As DdfInfo
        Dim data As New DdfInfo() With {.Cod = "A100"}
        data.Antet.Add(New DdfAntet() With {
            .Iddf = 1, .CodAngajament = "A100", .Cual = 3,
            .PartAng = True, .NumePartener = "TERMO PLOIESTI"})

        data.Revizii.Add(Rev(11, 0, New Date(2026, 1, 18), 600.0, incarcat:=True))
        data.Revizii.Add(Rev(12, 1, New Date(2026, 1, 30), 50.0, preluat:=True))
        data.Revizii.Add(Rev(13, 2, New Date(2026, 2, 11), 0.0))
        data.Revizii.Add(Rev(14, 3, New Date(2026, 2, 12), -900.0))

        data.Linii.Add(Linie(101, 11, "65.02.04.02.20.01.03", 100.0, "Alfa"))
        data.Linii.Add(Linie(102, 11, "65.02.04.02.20.01.04", 200.0, "Beta"))
        data.Linii.Add(Linie(103, 11, "65.02.04.02.20.01.03", 300.0, "Gama"))
        data.Linii.Add(Linie(104, 12, "65.02.04.02.20.01.05", 50.0, "Delta"))
        ' R3 nu are nicio linie -> frunza trebuie SĂ RĂMÂNĂ, cu total 0.
        data.Linii.Add(Linie(105, 14, "65.02.04.02.20.01.03", -900.0, "Eps"))
        Return data
    End Function

    Private Shared Function FindControl(Of T As Class)(root As Control) As T
        For Each c As Control In root.Controls
            Dim hit As T = TryCast(c, T)
            If hit IsNot Nothing Then Return hit
            Dim nested As T = FindControl(Of T)(c)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    ' Felia 0032: grila de valori NU mai e în DdfView, ci în `DdfValoriPage` — o pagină PARCATĂ,
    ' care nu are intrare în navSub și deci nu se creează niciodată prin vedere. Testele grilei o
    ' construiesc direct și îi împing un DdfPageContext, exact cum face gazda.
    '
    ' O căutare pe tip prin DdfView ar fi acum ÎNȘELĂTOARE, nu goală: pagina «Vizualizare» (cea
    ' implicită) găzduiește un XfaXmlPreview, care are ȘI EL un KBotDataView — deci un
    ' FindControl(Of KBotDataView)(view) ar întoarce grila ALTUI control și ar trece/pica pe
    ' motive fără legătură. De aceea helperul cere pagina, nu vederea.
    Private Shared Function GridOf(page As Control) As KBotDataView
        Dim g = FindControl(Of KBotDataView)(page)
        If g Is Nothing Then Throw New InvalidOperationException("Pagina nu conține un KBotDataView.")
        Return g
    End Function

    ' Contextul pe care gazda îl compune pentru un nod. `linii` = rândurile nodului.
    Private Shared Function Ctx(linii As List(Of LinieSaRow), isRoot As Boolean,
                                Optional revizie As RevizieRow = Nothing) As DdfPageContext
        Dim data = StandardData()
        Return New DdfPageContext(data.Antet(0), linii, data.Revizii, isRoot, revizie,
                                  "A100", Nothing, False)
    End Function

    ' Liniile unei revizii din setul standard.
    Private Shared Function LiniiFor(ParamArray idrev As Integer()) As List(Of LinieSaRow)
        Dim wanted As New HashSet(Of Integer)(idrev)
        Return StandardData().Linii.FindAll(Function(l) wanted.Contains(l.Idrev))
    End Function

    Private Shared Function TreeOf(view As DdfView) As AdvancedTreeControl
        Dim t = FindControl(Of AdvancedTreeControl)(view)
        If t Is Nothing Then Throw New InvalidOperationException("DdfView nu conține un AdvancedTreeControl.")
        Return t
    End Function

    Private Shared Function FindByName(root As Control, name As String) As Control
        For Each c As Control In root.Controls
            If String.Equals(c.Name, name, StringComparison.Ordinal) Then Return c
            Dim nested As Control = FindByName(c, name)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    ' Cautare INSENSIBILA la litere mari/mici: Reflection e sensibila, VB nu, deci un handler
    ' redenumit `Tree_NodeMouseUp` -> `tree_NodeMouseUp` (sau invers) intorcea Nothing si
    ' testul cadea cu NullReference inainte sa verifice ceva.
    Private Shared Sub ClickNode(view As DdfView, node As AdvancedTreeControl.TreeItem)
        Dim m = view.GetType().GetMethod("tree_NodeMouseUp",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance Or
            Reflection.BindingFlags.IgnoreCase)
        If m Is Nothing Then Throw New InvalidOperationException("DdfView nu are tree_NodeMouseUp.")
        m.Invoke(view, New Object() {node, New MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)})
    End Sub

    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        ' «Throw failure» ar reseta urma de stivă la linia asta, adică fiecare eșec din firul STA
        ' s-ar raporta ca o excepție fără loc. Capture().Throw() o păstrează pe cea originală.
        If failure IsNot Nothing Then Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw()
    End Sub

    Private Shared Function Loaded(api As FakeApiClient, view As DdfView) As AdvancedTreeControl
        Dim t = TreeOf(view)
        view.SetContext(Context("A100"))
        api.Complete("A100", StandardData())
        Application.DoEvents()
        Return t
    End Function

    ' ── Fără context: nicio cerere de rețea ──────────────────────────────────

    <Fact>
    Public Sub SetContext_Nothing_MakesNoApiCall_AndClearsTree()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Equal(2, t.Items.Count)          ' două luni

                       view.SetContext(Nothing)
                       Assert.Single(api.RequestedCods)
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_BlankCod_MakesNoApiCall()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   ", .AreDDF = True})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_WithoutAreDdf_MakesNoApiCall()
        ' Intrarea de navigare e deja ascunsă de shell; vederea nu trebuie să ceară oricum.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "A100", .AreDDF = False})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    ' ── Arborele ─────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Tree_TwoLevels_MonthRootsAndRevisionLeaves()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)

                       Assert.Equal(2, t.Items.Count)
                       Dim ian = t.Items(0)
                       Dim feb = t.Items(1)
                       Assert.StartsWith("Ianuarie/2026", ian.Caption)
                       Assert.StartsWith("Februarie/2026", feb.Caption)
                       Assert.Equal(2, ian.Children.Count)
                       Assert.Equal(2, feb.Children.Count)
                       ' Rădăcinile sunt expandate (planul §5).
                       Assert.True(ian.Expanded)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_RootKeyAndLeafKey_FollowThePlan()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Equal("LA_2026_1", t.Items(0).Key)
                       Assert.Equal("LA_2026_2", t.Items(1).Key)
                       Assert.Equal("RC_11", t.Items(0).Children(0).Key)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub MonthRoot_ShowsRealSumOfItsLeaves_NotAccessLiteralZero()
        ' ABATERE DELIBERATĂ: Access trimite «…~~~0» în AddTree_Root.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Contains("650,00", t.Items(0).Caption)      ' 600 + 50
                       Assert.Contains("-900,00", t.Items(1).Caption)     ' 0 + (-900)
                       Assert.DoesNotContain("~~~0.00", t.Items(0).Caption)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Leaf_CaptionPadsRevisionNumberWithSpaces()
        ' §2.6: Format(NumarRev,"@@@") e format TEXT — trei caractere, umplut cu SPAȚII.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.StartsWith("  0 - 18.01.2026", t.Items(0).Children(0).Caption)
                       Assert.StartsWith("  1 - 30.01.2026", t.Items(0).Children(1).Caption)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Leaf_TooltipIsDescScurta()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Equal("Revizia 0", t.Items(0).Children(0).Tooltip)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub RevisionWithoutSectionA_StillAppears_WithZeroTotal()
        ' Un INNER JOIN pe secțiunea A (ca în Access) ar șterge revizia din arbore.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim r3 = t.Items(1).Children(0)
                       Assert.Equal("RC_13", r3.Key)
                       Assert.Contains("0,00", r3.Caption)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NegativeLeaf_IsRed_AndRootIsRedOnlyOnItsOwnTotal()
        ' ABATERE DELIBERATĂ: Access face `cRoot.foreColor = cNode.foreColor`, deci culoarea
        ' rădăcinii ajunge să depindă de ultima frunză procesată. Aici o rădăcină e roșie
        ' DOAR când propriul ei total e negativ.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim rosu As Color = KBot.Theming.ThemeManager.Current.Palette.ErrorColor

                       ' Februarie: frunza -900 e roșie ȘI rădăcina (total -900) e roșie.
                       Assert.Equal(rosu, t.Items(1).Children(1).NodeForeColor)
                       Assert.Equal(rosu, t.Items(1).NodeForeColor)

                       ' Ianuarie: nicio frunză negativă, total pozitiv -> rădăcina NU e roșie.
                       Assert.NotEqual(rosu, t.Items(0).NodeForeColor)
                   End Using
               End Sub)
    End Sub

    ' ── Grila paginii «Valori» (felia 0032: DdfValoriPage) ───────────────────
    '
    ' Testele filtrului pe clasificație (`ClsfCombo_*`) au fost ȘTERSE, nu mutate: felia 0032 a
    ' scos banda `pnlFilter` cu tot cu combo, fiindcă grila (KBotDataView) filtrează singură pe
    ' coloană. Nu mai există comportament de acoperit.
    '
    ' La fel, aserțiunile despre coloana «data» și despre `element.AutoHide` au dispărut odată cu
    ' ele: coloanele grilei sunt AUTORITE ÎN DESIGNER de la felia 0025-05 încoace, iar acel set nu
    ' conține o coloană «data» și nu marchează nimic AutoHide. (Cele trei teste care le cereau erau
    ' deja roșii în arborele de lucru, înainte de felia 0032 — vezi worklog-ul.)

    <Fact>
    Public Sub LeafContext_FillsTheGridWithThatRevisionsRows()
        RunSta(Sub()
                   Using page As New DdfValoriPage()
                       Dim g = GridOf(page)

                       page.SetContext(Ctx(LiniiFor(11), isRoot:=False, revizie:=StandardData().Revizii(0)))
                       Assert.Equal(3, g.RowCount)                 ' R1 -> 3 linii

                       page.SetContext(Ctx(LiniiFor(12), isRoot:=False, revizie:=StandardData().Revizii(1)))
                       Assert.Equal(1, g.RowCount)                 ' R2 -> 1 linie
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub RootContext_ShowsAllMonthRows_AsAFlatList()
        ' Decizia 3: grila rădăcinii e o listă PLATĂ — un rând per linie de secțiune A
        ' peste TOATE reviziile lunii.
        RunSta(Sub()
                   Using page As New DdfValoriPage()
                       Dim g = GridOf(page)
                       page.SetContext(Ctx(LiniiFor(11, 12), isRoot:=True))   ' Ianuarie -> 3 + 1
                       Assert.Equal(4, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NothingSelected_EmptiesTheGrid()
        RunSta(Sub()
                   Using page As New DdfValoriPage()
                       Dim g = GridOf(page)
                       page.SetContext(Ctx(LiniiFor(11), isRoot:=False, revizie:=StandardData().Revizii(0)))
                       Assert.Equal(3, g.RowCount)

                       page.SetContext(Nothing)
                       Assert.Equal(0, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TotalsRow_SumsOnlyValCur()
        ' Decizia 5: rând de totaluri activ, Sum DOAR pe «Valoare curentă».
        RunSta(Sub()
                   Using page As New DdfValoriPage()
                       Dim g = GridOf(page)
                       Assert.True(g.FooterVisible)
                       Assert.Equal(KBotAggregate.Sum, g.Column("valcur").Aggregate)
                       For Each key As String In New String() {"clsf", "element", "valprec", "valtot"}
                           Assert.Equal(KBotAggregate.None, g.Column(key).Aggregate)
                       Next
                   End Using
               End Sub)
    End Sub

    ' ── Stale-guard ──────────────────────────────────────────────────────────

    <Fact>
    Public Sub StaleResponse_IsDiscarded()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = TreeOf(view)

                       view.SetContext(Context("A100"))     ' cererea 1, lăsată în aer
                       view.SetContext(Context("B200"))     ' cererea 2 — cea curentă

                       api.Complete("A100", StandardData())         ' răspuns DEPĂȘIT
                       Application.DoEvents()
                       Assert.Empty(t.Items)                        ' ignorat

                       api.Complete("B200", StandardData())         ' răspunsul curent
                       Application.DoEvents()
                       Assert.Equal(2, t.Items.Count)
                   End Using
               End Sub)
    End Sub

    ' ── Felia 04: browser + previzualizare partajată ────────────────────────

    <Fact>
    Public Sub EachPage_HostsItsOwnSurface()
        ' Felia 0032: suprafețele nu mai sunt montate de vedere în patru panouri, ci fiecare stă
        ' în designerul paginii ei. Paginile se construiesc FĂRĂ dependențe — chiar asta le face
        ' deschizibile în designerul Visual Studio.
        RunSta(Sub()
                   Using p As New DdfVizualizarePage()
                       Assert.NotNull(FindControl(Of XfaXmlPreview)(p))     ' previzualizarea XFA
                   End Using
                   Using p As New DdfDocumentPage()
                       Assert.NotNull(FindControl(Of ReaderHostPreview)(p)) ' PDF-ul REAL
                   End Using
                   Using p As New DdfFisierePage()
                       Assert.NotNull(FindControl(Of DdfFileBrowser)(p))    ' browserul de fișiere
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Pages_AreCreatedLazily_OnlyTheActiveOneExists()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       ' Selecția inițială e «Vizualizare» -> doar ea e construită.
                       Assert.NotNull(FindControl(Of DdfVizualizarePage)(view))
                       Assert.Null(FindControl(Of DdfDocumentPage)(view))
                       Assert.Null(FindControl(Of DdfFisierePage)(view))
                       ' «Valori» e PARCATĂ: fără intrare în navSub, nu se creează niciodată.
                       Assert.Null(FindControl(Of DdfValoriPage)(view))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub FileActivated_RoutesToDocumentPage_NotVizualizare()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       ' Încărcăm date ca `split` să fie vizibil (altfel Visible propagă False
                       ' de la ancestorul ascuns și testul nu poate distinge paginile).
                       Loaded(api, view)

                       Dim viz = FindControl(Of DdfVizualizarePage)(view)
                       Assert.True(viz.Visible)               ' pagina implicită de la parcarea lui «valori»

                       ' OnFileActivated comută pe pagina «Document» (PDF-ul real), NU pe
                       ' «Vizualizare» (reconstrucția XFA). Fișierul lipsește -> ReaderHostPreview
                       ' arată starea „document lipsă", fără să pornească Adobe.
                       Dim onFile = view.GetType().GetMethod("OnFileActivated",
                           Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
                       onFile.Invoke(view, New Object() {Nothing, "C:\nu\exista\DDF_NR_1_REV_0_A.PDF"})

                       Dim doc = FindControl(Of DdfDocumentPage)(view)
                       Assert.NotNull(doc)                    ' creată leneș, abia acum
                       Assert.True(doc.Visible)
                       Assert.False(viz.Visible)              ' NU rămâne pe «Vizualizare»
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EmptyRevisions_ShowsEmptyState_NotACrash()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", New DdfInfo() With {.Cod = "A100"})
                       Application.DoEvents()
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

    ' ══ Slice 0024 — the Adobe host settings on the «Document» page ═════════════
    '
    ' The two combos are the operator's only way to change how Adobe is hosted, and the brief is
    ' explicit that the change must take effect WHILE THE APPLICATION RUNS. These tests pin the
    ' wiring: the controls exist on the right page, they are populated in the documented order with
    ' Romanian labels, they start on the stored value, and changing one PERSISTS both.
    '
    ' They save and restore the real settings around themselves — a test must not leave the
    ' operator's kbot_paths.json rewritten.

    ' Felia 0032: banda de setări s-a mutat, cu tot cu combo-uri, în `DdfDocumentPage`.
    Private Shared Function ComboByName(page As DdfDocumentPage, name As String) As ComboBox
        Dim c = TryCast(FindByName(page, name), ComboBox)
        If c Is Nothing Then Throw New InvalidOperationException($"DdfDocumentPage nu conține {name}.")
        Return c
    End Function

    Private Shared Sub WithSavedSettings(body As Action)
        Dim mode As String = KBotPaths.Current.AdobeViewerMode
        Dim inst As String = KBotPaths.Current.AdobeNewInstance
        Try
            body()
        Finally
            KBotPaths.Current.AdobeViewerMode = mode
            KBotPaths.Current.AdobeNewInstance = inst
        End Try
    End Sub

    <Fact>
    Public Sub AdobeSettings_LiveOnTheDocumentPage_NotOnValues()
        ' They belong where their effect is visible: changing the mode re-places the window that is
        ' hosted right there. Since slice 0032 that page IS `DdfDocumentPage`, so the band's parent
        ' is the page itself rather than the old `pnlPdf` panel.
        '
        ' The old Dock assertion is gone on purpose: `pnlAdobe` has never been docked — it is
        ' placed and sized outright, and hidden (`Visible = False`) in the designer. That assertion
        ' was already failing at HEAD; it pinned a layout the designer never had.
        RunSta(Sub()
                   Using page As New DdfDocumentPage()
                       Dim band = FindByName(page, "pnlAdobe")
                       Assert.NotNull(band)
                       Assert.Same(page, band.Parent)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AdobeModeCombo_OffersExactlyThreeRomanianChoices()
        RunSta(Sub()
                   Using page As New DdfDocumentPage()
                       Dim c = ComboByName(page, "cboAdobeMod")
                       Assert.Equal(3, c.Items.Count)
                       Assert.Equal("Automat", c.Items(0).ToString())
                       Assert.Equal("Modern", c.Items(1).ToString())
                       Assert.Equal("Clasic", c.Items(2).ToString())
                       ' DropDownList: the operator picks, never types a fourth value.
                       Assert.Equal(ComboBoxStyle.DropDownList, c.DropDownStyle)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AdobeNewInstanceCombo_OffersAutomatDaNu()
        RunSta(Sub()
                   Using page As New DdfDocumentPage()
                       Dim c = ComboByName(page, "cboAdobeInst")
                       Assert.Equal(3, c.Items.Count)
                       Assert.Equal("Automat", c.Items(0).ToString())
                       Assert.Equal("Da", c.Items(1).ToString())
                       Assert.Equal("Nu", c.Items(2).ToString())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AdobeCombos_StartOnTheStoredValue()
        WithSavedSettings(
            Sub()
                KBotPaths.Current.AdobeViewerMode = AdobeViewerSettings.ModeToText(AdobeViewerMode.Modern)
                KBotPaths.Current.AdobeNewInstance = AdobeViewerSettings.NewInstanceToText(AdobeNewInstanceMode.Nu)
                RunSta(Sub()
                           Using page As New DdfDocumentPage()
                               Assert.Equal("Modern", ComboByName(page, "cboAdobeMod").SelectedItem.ToString())
                               Assert.Equal("Nu", ComboByName(page, "cboAdobeInst").SelectedItem.ToString())
                           End Using
                       End Sub)
            End Sub)
    End Sub

    <Fact>
    Public Sub AnInvalidStoredValue_StartsOnAutomat_WithoutThrowing()
        ' A broken settings file must never stop the view from opening.
        WithSavedSettings(
            Sub()
                KBotPaths.Current.AdobeViewerMode = "turbo"
                KBotPaths.Current.AdobeNewInstance = "poate"
                RunSta(Sub()
                           Using page As New DdfDocumentPage()
                               Assert.Equal("Automat", ComboByName(page, "cboAdobeMod").SelectedItem.ToString())
                               Assert.Equal("Automat", ComboByName(page, "cboAdobeInst").SelectedItem.ToString())
                           End Using
                       End Sub)
            End Sub)
    End Sub

    <Fact>
    Public Sub ChangingTheModeCombo_PersistsBOTHSettings()
        ' Both, because they describe the same host: saving one and leaving the other behind is how
        ' a settings file ends up disagreeing with the panel the operator is looking at.
        WithSavedSettings(
            Sub()
                KBotPaths.Current.AdobeViewerMode = AdobeViewerSettings.ModeToText(AdobeViewerMode.Auto)
                KBotPaths.Current.AdobeNewInstance = AdobeViewerSettings.NewInstanceToText(AdobeNewInstanceMode.Auto)
                RunSta(Sub()
                           Using page As New DdfDocumentPage()
                               ComboByName(page, "cboAdobeInst").SelectedIndex = 1   ' «Da»
                               ComboByName(page, "cboAdobeMod").SelectedIndex = 2    ' «Clasic»
                               Assert.Equal(AdobeViewerMode.Classic, AdobeViewerSettings.CurrentMode().Value)
                               Assert.Equal(AdobeNewInstanceMode.Da, AdobeViewerSettings.CurrentNewInstance().Value)
                           End Using
                       End Sub)
            End Sub)
    End Sub

    <Fact>
    Public Sub BuildingTheCombos_DoesNotItselfWriteTheSettings()
        ' Populating a ComboBox raises SelectedIndexChanged. Without the guard, merely OPENING the
        ' view would overwrite the stored value with whatever landed in the list first.
        WithSavedSettings(
            Sub()
                KBotPaths.Current.AdobeViewerMode = AdobeViewerSettings.ModeToText(AdobeViewerMode.Modern)
                KBotPaths.Current.AdobeNewInstance = AdobeViewerSettings.NewInstanceToText(AdobeNewInstanceMode.Nu)
                RunSta(Sub()
                           Using page As New DdfDocumentPage()
                               Assert.Equal(AdobeViewerMode.Modern, AdobeViewerSettings.CurrentMode().Value)
                               Assert.Equal(AdobeNewInstanceMode.Nu, AdobeViewerSettings.CurrentNewInstance().Value)
                           End Using
                       End Sub)
            End Sub)
    End Sub

End Class
