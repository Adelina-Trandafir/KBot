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

' Headless behaviour + shaping tests for PlatiView (slice 0017-03). They cover what no server
' test can reach: a null/blank context must NOT hit the network; a response must shape into the
' 2-level month -> day tree (one leaf per day, holding ALL that day's payments); the «+» must
' land on EXACTLY the oldest un-ordonanțat day + its month and nowhere else; that month — and
' only it — starts EXPANDED; node icons come from the designer's «image_list»; selecting a node
' FILTERS the grid (not aggregate); selecting a grid row drives the bank-statement detail pane;
' INCASARE green sits on fully-INCASARE days and never on the month; STALE responses discarded.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so Async Sub continuations need Application.DoEvents()
' to pump. Same pattern as ReceptiiViewTests / RezervariViewTests.
Public Class PlatiViewTests

    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        ' Felia 0048-02: ingestia FOREXE. Nefolosita de vederile astea.
        Public Function TrimitePrelucrareAsync(rezultat As PrelucrareRezultat,
                                               alegeri As IReadOnlyList(Of AlegereUnitate),
                                               ct As CancellationToken) As Task(Of PrelucrareRaspuns) _
            Implements IApiClient.TrimitePrelucrareAsync
            Throw New NotSupportedException()
        End Function

        Public ReadOnly RequestedCods As New List(Of String)()
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of PlatiInfo))(StringComparer.Ordinal)

        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) _
            As Task(Of PlatiInfo) Implements IApiClient.GetPlatiAsync
            RequestedCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of PlatiInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As PlatiInfo)
            Pending(cod).SetResult(data)
        End Sub

        ' --- restul contractului: nefolosit aici ---
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

        Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                    Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo) _
            Implements IApiClient.GetDdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetIstoricAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo) _
            Implements IApiClient.GetIstoricAsync
            Throw New NotSupportedException()
        End Function

        ' Felia 0033: vederea ORD nu e exercitată de acest dublu — contractul cere metoda,
        ' deci o refuzăm zgomotos, ca pe celelalte neatinse.
        Public Function GetOrdAsync(cod As String, ct As CancellationToken) As Task(Of OrdInfo) _
            Implements IApiClient.GetOrdAsync
            Throw New NotSupportedException()
        End Function

        ' Felia 0041: rutele de PDF semnat nu sunt exercitate de acest dublu — contractul cere
        ' metodele, deci le refuzăm zgomotos, ca pe celelalte neatinse.
        Public Function DownloadDdfPdfAsync(idrev As Integer, cachedSha As String,
                                            ct As CancellationToken) As Task(Of PdfDownloadResult) _
            Implements IApiClient.DownloadDdfPdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function DownloadOrdPdfAsync(idordp As Integer, cachedSha As String,
                                            ct As CancellationToken) As Task(Of PdfDownloadResult) _
            Implements IApiClient.DownloadOrdPdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function UploadDdfPdfAsync(idrev As Integer, continut As Byte(), shaPrecedent As String,
                                          ct As CancellationToken) As Task(Of PutPdfResponse) _
            Implements IApiClient.UploadDdfPdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function UploadOrdPdfAsync(idordp As Integer, continut As Byte(), shaPrecedent As String,
                                          ct As CancellationToken) As Task(Of PutPdfResponse) _
            Implements IApiClient.UploadOrdPdfAsync
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

    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of PlatiInfo)), Task(Of PlatiInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod}
    End Function

    Private Shared Function Row(id As Integer, d As Date, suma As Double, tip As String,
                                incarcat As Boolean, preluat As Boolean, areOrd As Boolean,
                                Optional withExtras As Boolean = False, Optional platitor As String = "",
                                Optional nrOp As String = "") As PlataRow
        Dim r As New PlataRow() With {
            .IdPlataFX = id, .DataPlata = d, .Suma = suma, .Tip = tip,
            .Incarcat = incarcat, .Preluat = preluat, .AreOrd = areOrd,
            .NrOP = If(nrOp = "", "OP" & id.ToString(), nrOp),
            .ReferintaTrezor = "TZ" & id.ToString(),
            .Clsf = "65.02", .Denumire = "Cheltuieli", .ClsfPlata = "65.02"
        }
        If withExtras Then
            r.Extras = New ExtrasBancar() With {
                .Idfxe = id, .NrDoc = "DOC" & id.ToString(), .DataDoc = "31.01.2026",
                .Referinta = "TZ" & id.ToString(), .PlatitorNume = platitor,
                .PlatitorCui = "123", .PlatitorIban = "RO00", .SumaDebit = suma, .SumaCredit = 0.0,
                .Explicatii = "Explicație"
            }
        End If
        Return r
    End Function

    ' Set standard: DOUA luni.
    '  Ian: P1 (19 Ian, PLATA, Incarcat, NE-ordonantat, cu extras) -> cea mai veche zi
    '       ne-ordonantată => «+»; P2 (31 Ian, PLATA, Preluat, ordonantat, fără extras).
    '  Feb: P3 + P4 (4 Feb, ambele INCASARE, ordonantate) -> ziua toată INCASARE => verde.
    Private Shared Function StandardData() As PlatiInfo
        Dim data As New PlatiInfo() With {.Cod = "A100"}
        data.Plati.Add(Row(1, New Date(2026, 1, 19), 1331.0, "PLATA", True, False, False,
                           withExtras:=True, platitor:="FURNIZOR SRL"))
        data.Plati.Add(Row(2, New Date(2026, 1, 31), 700.0, "PLATA", False, True, True))
        data.Plati.Add(Row(3, New Date(2026, 2, 4), -23.0, "INCASARE", False, True, True))
        data.Plati.Add(Row(4, New Date(2026, 2, 4), -48.0, "INCASARE", True, False, True))
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

    Private Shared Function FindByName(root As Control, name As String) As Control
        For Each c As Control In root.Controls
            If String.Equals(c.Name, name, StringComparison.Ordinal) Then Return c
            Dim nested As Control = FindByName(c, name)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    Private Shared Function GridOf(view As PlatiView) As KBotDataView
        Dim g = FindControl(Of KBotDataView)(view)
        If g Is Nothing Then Throw New InvalidOperationException("PlatiView nu conține un KBotDataView.")
        Return g
    End Function

    Private Shared Function TreeOf(view As PlatiView) As AdvancedTreeControl
        Dim t = FindControl(Of AdvancedTreeControl)(view)
        If t Is Nothing Then Throw New InvalidOperationException("PlatiView nu conține un AdvancedTreeControl.")
        Return t
    End Function

    ' IgnoreCase: VB e insensibil la majuscule, dar Type.GetMethod NU — o redenumire a
    ' handler-ului (tree_NodeMouseUp -> Tree_NodeMouseUp) ar întoarce altfel Nothing.
    Private Shared Sub ClickNode(view As PlatiView, node As AdvancedTreeControl.TreeItem)
        Dim m = view.GetType().GetMethod("tree_NodeMouseUp",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance Or
            Reflection.BindingFlags.IgnoreCase)
        If m Is Nothing Then Throw New InvalidOperationException("PlatiView nu are handler-ul tree_NodeMouseUp.")
        m.Invoke(view, New Object() {node, New MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)})
    End Sub

    Private Shared Function GreenColor() As Color
        Return KBot.Theming.ThemeManager.Current.Palette.SuccessColor
    End Function

    ' Egalitate de imagine pe conținut. ImageList.Images(i) materializează un Bitmap nou la
    ' fiecare acces, deci identitatea de referință nu spune nimic despre proveniență.
    Private Shared Function SamePixels(a As Image, b As Image) As Boolean
        If a Is Nothing OrElse b Is Nothing Then Return False
        If a.Size <> b.Size Then Return False
        Using ba As New Bitmap(a), bb As New Bitmap(b)
            For y As Integer = 0 To ba.Height - 1
                For x As Integer = 0 To ba.Width - 1
                    If ba.GetPixel(x, y) <> bb.GetPixel(x, y) Then Return False
                Next
            Next
        End Using
        Return True
    End Function

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
        ' Capture: «Throw failure» ar reseta stiva la linia asta și ar ascunde locul real.
        If failure IsNot Nothing Then Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw()
    End Sub

    <Fact>
    Public Sub SetContext_Nothing_MakesNoApiCall_AndClearsTree()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()
                       Assert.Equal(2, t.Items.Count)          ' două luni (fără rădăcina ALL)

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
                   Using view As New PlatiView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   "})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_TwoLevels_MonthThenDay()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ' Două rădăcini de lună, cronologic — și NIMIC sub frunze (două niveluri).
                       Assert.Equal(2, t.Items.Count)
                       Dim ian = t.Items(0)
                       Dim feb = t.Items(1)
                       Assert.StartsWith("Ianuarie", ian.Caption)
                       Assert.Contains("2.031,00", ian.Caption)         ' 1331 + 700
                       Assert.StartsWith("Februarie", feb.Caption)

                       ' Ian: două zile distincte (19, 31), fiecare cu suma ei.
                       Assert.Equal(2, ian.Children.Count)
                       Assert.StartsWith("19.01.2026", ian.Children(0).Caption)
                       Assert.Contains("1.331,00", ian.Children(0).Caption)
                       Assert.StartsWith("31.01.2026", ian.Children(1).Caption)

                       ' Feb: P3 + P4 cad în ACEEAȘI zi -> o singură frunză, cu suma zilei.
                       Dim zi4 = Assert.Single(feb.Children)
                       Assert.StartsWith("04.02.2026", zi4.Caption)
                       Assert.Contains("-71,00", zi4.Caption)           ' -23 + -48

                       ' Nivelul de sub zi nu mai există.
                       For Each luna In t.Items
                           For Each zi In luna.Children
                               Assert.Empty(zi.Children)
                           Next
                       Next
                   End Using
               End Sub)
    End Sub

    ' Rădăcinile stau strânse; se deschide DOAR luna care poartă «+» (cerință operator).
    <Fact>
    Public Sub Months_Collapsed_ExceptTheOneCarryingPlus()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Assert.True(t.Items(0).Expanded)     ' Ianuarie — poartă «+»
                       Assert.False(t.Items(1).Expanded)    ' Februarie — nu
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Months_AllCollapsed_WhenNoPlusAnywhere()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)

                       Dim data As New PlatiInfo() With {.Cod = "A100"}
                       data.Plati.Add(Row(1, New Date(2026, 1, 19), 100.0, "PLATA", True, False, True))
                       data.Plati.Add(Row(2, New Date(2026, 2, 3), 200.0, "PLATA", True, False, True))

                       view.SetContext(Context("A100"))
                       api.Complete("A100", data)
                       Application.DoEvents()

                       For Each luna In t.Items
                           Assert.False(luna.Expanded)
                       Next
                   End Using
               End Sub)
    End Sub

    ' Iconițele de nod vin din «image_list» (designer): «month» pe lună, «up»/«down» pe plată.
    <Fact>
    Public Sub NodeIcons_ComeFromTheDesignerImageList()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Assert.NotNull(t.NodeImages)
                       Dim luna = t.NodeImage("month")
                       Dim sus = t.NodeImage("up")
                       Dim jos = t.NodeImage("down")
                       Assert.NotNull(luna)
                       Assert.NotNull(sus)
                       Assert.NotNull(jos)

                       ' Comparăm pe PIXELI, nu pe referință: ImageList.Images(i) întoarce un
                       ' Bitmap NOU la fiecare acces, deci Assert.Same n-ar trece niciodată.
                       ' Luna = «month», nu o stare merjată.
                       Assert.True(SamePixels(luna, t.Items(0).LeftIconClosed))
                       Assert.True(SamePixels(luna, t.Items(0).LeftIconOpen))
                       ' Ziua 19 are doar P1 (Incarcat) -> «up»; ziua 31 doar P2 (Preluat) -> «down».
                       Assert.True(SamePixels(sus, t.Items(0).Children(0).LeftIconClosed))
                       Assert.True(SamePixels(jos, t.Items(0).Children(1).LeftIconClosed))
                       ' Feb: ziua 4 merjează P3 (Preluat) cu P4 (Incarcat) -> ORICE sus câștigă.
                       Assert.True(SamePixels(sus, t.Items(1).Children(0).LeftIconClosed))
                       ' …și cele două stări chiar diferă (altfel testul ar trece degeaba).
                       Assert.False(SamePixels(sus, jos))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Plus_OnOldestUnordonantatDay_AndItsMonth()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim ian = t.Items(0)
                       Dim feb = t.Items(1)
                       Dim zi19 = ian.Children(0)    ' cea mai veche zi ne-ordonantată
                       Dim zi31 = ian.Children(1)

                       ' «+» pe EXACT: ziua 19.01 și luna care o conține.
                       Assert.NotNull(ian.RightIcon)
                       Assert.NotNull(zi19.RightIcon)
                       ' Și nicăieri altundeva.
                       Assert.Null(zi31.RightIcon)
                       Assert.Null(feb.RightIcon)
                       For Each zi In feb.Children
                           Assert.Null(zi.RightIcon)
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Plus_NoneWhenAllOrdonantat()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)

                       Dim data As New PlatiInfo() With {.Cod = "A100"}
                       data.Plati.Add(Row(1, New Date(2026, 1, 19), 100.0, "PLATA", True, False, True))
                       data.Plati.Add(Row(2, New Date(2026, 1, 31), 200.0, "PLATA", True, False, True))

                       view.SetContext(Context("A100"))
                       api.Complete("A100", data)
                       Application.DoEvents()

                       Dim ian = t.Items(0)
                       Assert.Null(ian.RightIcon)
                       For Each zi In ian.Children
                           Assert.Null(zi.RightIcon)
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NodeClick_FiltersGrid_NotAggregate()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ' Luna Ian -> 2 plăți.
                       ClickNode(view, t.Items(0))
                       Assert.Equal(2, g.RowCount)
                       ' Luna Feb -> 2 plăți.
                       ClickNode(view, t.Items(1))
                       Assert.Equal(2, g.RowCount)
                       ' Ziua 19.01 -> exact 1 rând.
                       ClickNode(view, t.Items(0).Children(0))
                       Assert.Equal(1, g.RowCount)
                       ' Ziua 04.02 strânge două plăți -> DOUĂ rânduri, nu unul agregat.
                       ClickNode(view, t.Items(1).Children(0))
                       Assert.Equal(2, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub GridColumns_ClsfPlatitorNrdocDataSuma()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ClickNode(view, t.Items(0).Children(0))   ' ziua 19.01 -> doar P1
                       Assert.Equal(1, g.RowCount)
                       Assert.Equal("65.02", CStr(g.Rows(0)("clsf")))
                       Assert.Equal("FURNIZOR SRL", CStr(g.Rows(0)("platitor")))   ' din extras
                       Assert.Equal("OP1", CStr(g.Rows(0)("nrdoc")))
                       Assert.Equal("19.01.2026", CStr(g.Rows(0)("data")))
                       Assert.Equal(1331.0, CDbl(g.Rows(0)("suma")), 2)
                       ' (Rândul de totaluri e verificat headless în KBotDataViewTotalsTests —
                       '  DebugFooterText e Friend în KBot.Controls, invizibil de aici.)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub DetailPane_ShowsExtras_ThenEmptyStates()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim table = CType(FindByName(view, "detailTable"), TableLayoutPanel)
                       Dim msg = CType(FindByName(view, "lblDetailMessage"), Label)
                       ' „val4", nu „valPlatitor": InitDetailPair rebotează fiecare valoare cu
                       ' «val»&rowIndex când o pune în tabel, iar Plătitor e rândul 4.
                       Dim valPlatitor = CType(FindByName(view, "val4"), Label)

                       ' Luna Ian -> P1 (are extras), P2 (fără extras).
                       ClickNode(view, t.Items(0))
                       ' Nimic selectat imediat după umplere.
                       Assert.False(table.Visible)
                       Assert.True(msg.Visible)
                       Assert.Equal("Selectați o plată.", msg.Text)

                       ' Selectăm rândul 0 (P1, cu extras) -> tabelul apare.
                       g.CurrentRowIndex = 0
                       Assert.True(table.Visible)
                       Assert.False(msg.Visible)
                       Assert.Equal("FURNIZOR SRL", valPlatitor.Text)

                       ' Selectăm rândul 1 (P2, fără extras) -> mesaj dedicat.
                       g.CurrentRowIndex = 1
                       Assert.False(table.Visible)
                       Assert.True(msg.Visible)
                       Assert.Equal("Fără extras bancar asociat.", msg.Text)
                   End Using
               End Sub)
    End Sub

    ' Verdele INCASARE stă pe ZI (și doar dacă TOATE plățile ei sunt încasări). Luna rămâne
    ' neutră chiar când toate zilele ei sunt verzi.
    <Fact>
    Public Sub IncasareColouring_OnFullyIncasareDays_NeverOnTheMonth()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim green = GreenColor()
                       Dim ian = t.Items(0)
                       Dim feb = t.Items(1)

                       ' Feb: ziua 4 e formată doar din INCASARE (P3 + P4) -> verde.
                       Assert.Equal(green, feb.Children(0).NodeForeColor)
                       ' …dar luna NU, deși toate zilele ei sunt verzi.
                       Assert.Equal(Color.Empty, feb.NodeForeColor)

                       ' Ian: zile de PLATA -> NU verzi (rămân Color.Empty).
                       Assert.Equal(Color.Empty, ian.NodeForeColor)
                       Assert.Equal(Color.Empty, ian.Children(0).NodeForeColor)
                       Assert.Equal(Color.Empty, ian.Children(1).NodeForeColor)
                   End Using
               End Sub)
    End Sub

    ' O zi MIXTĂ (o plată + o încasare) nu e verde — regula e „toate", nu „măcar una".
    <Fact>
    Public Sub IncasareColouring_MixedDay_IsNotGreen()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)

                       Dim data As New PlatiInfo() With {.Cod = "A100"}
                       data.Plati.Add(Row(1, New Date(2026, 1, 19), 100.0, "INCASARE", True, False, True))
                       data.Plati.Add(Row(2, New Date(2026, 1, 19), 200.0, "PLATA", True, False, True))

                       view.SetContext(Context("A100"))
                       api.Complete("A100", data)
                       Application.DoEvents()

                       Dim zi = Assert.Single(t.Items(0).Children)
                       Assert.Equal(Color.Empty, zi.NodeForeColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EmptyPlati_ShowsNoTree()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", New PlatiInfo() With {.Cod = "A100"})
                       Application.DoEvents()
                       Assert.Empty(t.Items)
                       Assert.Equal(0, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StaleResponse_ForSupersededCod_IsDiscarded()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       view.SetContext(Context("B200"))
                       Assert.Equal(New String() {"A100", "B200"}, api.RequestedCods.ToArray())

                       ' B200 (o singură lună) răspunde primul.
                       Dim b As New PlatiInfo() With {.Cod = "B200"}
                       b.Plati.Add(Row(9, New Date(2026, 3, 1), 7.0, "PLATA", True, False, True))
                       api.Complete("B200", b)
                       Application.DoEvents()
                       Assert.Single(t.Items)                  ' o singură lună

                       ' A100 (2 luni) răspunde după — trebuie ignorat.
                       api.Complete("A100", StandardData())
                       Application.DoEvents()
                       Assert.Single(t.Items)
                   End Using
               End Sub)
    End Sub

End Class
