Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 0028-06: meniul de filtrare a trecut de la o fereastră desenată integral de noi la un
''' formular AUTORAT ÎN DESIGNER. Testele de aici trec prin CONTROALELE adevărate (bifa listei,
''' «(Selectează tot)», butoanele), nu prin porțile <c>Debug*</c> — altfel s-ar proba modelul, iar
''' partea nouă, tocmai legătura control ↔ model, ar rămâne neprobată.
'''
''' <c>KBotFilterPopupTests</c> rămâne cum era: el fixează DECIZIILE (ce filtru iese la OK), care
''' n-au voie să se schimbe fiindcă s-a schimbat felul în care e desenat meniul.
''' </summary>
Public Class KBotFilterPopupDesignerTests

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    ' Un clic adevărat pe un control: CheckBox n-are «PerformClick» public (îl are doar Button), iar
    ' ce ne trebuie e chiar drumul pe care-l face mouse-ul — OnClick ridică evenimentul Click, deci
    ' handler-ul din popup rulează exact ca la operator.
    Private Shared Sub ClicPe(c As Control)
        Dim m = c.GetType().GetMethod("OnClick",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        m.Invoke(c, New Object() {EventArgs.Empty})
    End Sub

    Private Shared ReadOnly Valori As New List(Of String) From {"", "Ana", "Barbu", "Cezar"}

    Private Shared Function Meniu(Optional filtru As KBotColumnFilter = Nothing,
                                  Optional tip As KBotValueType = KBotValueType.Text) As KBotFilterPopup
        Return New KBotFilterPopup("nume", "Nume", tip, Valori, filtru, KBotSortDirection.None)
    End Function

    ' ── Lista: control din designer, conținut de la rulare ───────────────────────

    <Fact>
    Public Sub TheListIsDesignerDeclared_AndFilledAtRuntime()
        RunSta(Sub()
                   Using p = Meniu()
                       ' Controlul există fără să-l fi construit cineva în cod…
                       Assert.NotNull(p.lstValori)
                       Assert.Equal(DockStyle.Fill, p.lstValori.Dock)
                       ' …iar valorile sunt puse la rulare, în ordinea primită.
                       Assert.Equal(Valori.Count, p.lstValori.Items.Count)
                       Assert.Equal("(Necompletate)", CStr(p.lstValori.Items(0)))
                       Assert.Equal("Ana", CStr(p.lstValori.Items(1)))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Searching_RefillsTheListControl()
        RunSta(Sub()
                   Using p = Meniu()
                       p.DebugSearch("ana")
                       Assert.Equal(1, p.lstValori.Items.Count)
                       Assert.Equal("Ana", CStr(p.lstValori.Items(0)))
                       p.DebugSearch("")
                       Assert.Equal(Valori.Count, p.lstValori.Items.Count)
                   End Using
               End Sub)
    End Sub

    ' ── Bifa unui rând = drumul operatorului, prin evenimentul controlului ───────

    <Fact>
    Public Sub UncheckingInTheListControl_ReachesTheFilter()
        RunSta(Sub()
                   Using p = Meniu()
                       Assert.False(p.BuildFilter().IsActive)          ' tot bifat = fără filtru

                       p.lstValori.SetItemChecked(2, False)            ' „Barbu”

                       Dim f = p.BuildFilter()
                       Assert.True(f.IsActive)
                       Assert.DoesNotContain("Barbu", f.SelectedValues)
                       Assert.Equal(Valori.Count - 1, p.DebugCheckedCount())
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub UncheckAllThenTickOne_FiltersToThatOneValue()
        ' Drumul obișnuit al operatorului, și cel care s-a rupt: «(Selectează tot)» stinge tot, apoi
        ' se bifează la loc una singură. Dacă bifele individuale nu ajung în model (clauza «Handles»
        ' a lui lstValori.ItemCheck), setul predat la OK rămâne GOL — adică o grilă goală, nu una
        ' filtrată pe o valoare. Testul trece prin controlul adevărat, nu prin porțile Debug*.
        RunSta(Sub()
                   Using p = Meniu()
                       ClicPe(p.chkSelecteazaTot)                 ' stinge tot
                       Assert.Equal(0, p.DebugCheckedCount())

                       p.lstValori.SetItemChecked(1, True)        ' „Ana”

                       Dim f = p.BuildFilter()
                       Assert.True(f.IsActive)
                       Assert.Single(f.SelectedValues)
                       Assert.Contains("Ana", f.SelectedValues)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheBodyFollowsTheWindow()
        ' Corpul e andocat Fill: o fereastră care se re-măsoară pe înălțime, cu un corp rămas la
        ' mărimea scrisă în designer, lasă o bandă goală sub controale — exact ce se vedea.
        RunSta(Sub()
                   Using p = Meniu()
                       p.DebugMeasure()
                       Assert.Equal(p.ClientSize.Height - 2, p.pnlCorp.Height)   ' minus rama de 1px
                       Assert.Equal(p.ClientSize.Width - 2, p.pnlCorp.Width)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub AnExistingFilter_ArrivesInTheListControlsCheckMarks()
        RunSta(Sub()
                   Dim filtru As New KBotColumnFilter("nume") With {
                       .SelectedValues = New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase) From {"Ana"}}
                   Using p = Meniu(filtru)
                       Assert.True(p.lstValori.GetItemChecked(1))      ' Ana
                       Assert.False(p.lstValori.GetItemChecked(0))
                       Assert.False(p.lstValori.GetItemChecked(2))
                       Assert.True(p.btnStergeFiltru.Enabled)          ' există filtru => se poate șterge
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub WithoutAFilter_ClearButtonIsDisabled()
        RunSta(Sub()
                   Using p = Meniu()
                       Assert.False(p.btnStergeFiltru.Enabled)
                   End Using
               End Sub)
    End Sub

    ' ── «(Selectează tot)» — trei stări, peste rândurile ARĂTATE ─────────────────

    <Fact>
    Public Sub SelectAll_ShowsMixedState_WhenOnlySomeAreChecked()
        RunSta(Sub()
                   Using p = Meniu()
                       Assert.Equal(CheckState.Checked, p.chkSelecteazaTot.CheckState)
                       p.lstValori.SetItemChecked(1, False)
                       Assert.Equal(CheckState.Indeterminate, p.chkSelecteazaTot.CheckState)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SelectAll_TouchesOnlyTheRowsOnScreen()
        RunSta(Sub()
                   Using p = Meniu()
                       p.DebugSearch("ar")                             ' Barbu + Cezar (nu Ana, nu golul)
                       Dim aratate As Integer = p.DebugShownCount()
                       Assert.True(aratate > 0 AndAlso aratate < Valori.Count)

                       ClicPe(p.chkSelecteazaTot)               ' toate arătate erau bifate => se sting
                       Assert.Equal(Valori.Count - aratate, p.DebugCheckedCount())
                       Assert.Equal(CheckState.Unchecked, p.chkSelecteazaTot.CheckState)

                       ClicPe(p.chkSelecteazaTot)               ' …și se aprind la loc
                       Assert.Equal(Valori.Count, p.DebugCheckedCount())
                   End Using
               End Sub)
    End Sub

    ' ── Un rând mai înalt (schema Modern) nu mănâncă din listă ──────────────────

    <Fact>
    Public Sub WhenAPaddedButtonNoLongerFits_TheRowAndTheWindowGrowWithIt()
        ' Schema «Modern» cere aer în jurul textului (ControlPadding = 12,8,12,8) — și designerul a
        ' autorat rândurile pe Classic, adică pe umplutură zero. Fără ThemeTableFit, butonul rămâne
        ' tăiat la măsura celulei; cu el, rândul crește cu exact cât nu încape, iar fereastra crește
        ' cu rândul, fără să mușce din lista de valori (care e Percent).
        ' Testul pune umplutura DIRECT pe buton, ca să nu mute schema globală a procesului
        ' (convenția casei din AdvancedTreeThemingTests): ce se probează aici e re-măsurarea, nu tema.
        RunSta(Sub()
                   Using p = Meniu()
                       Const randConditii As Integer = 4
                       p.DebugMeasure()
                       Dim listaInainte As Integer = p.lstValori.Height
                       Dim fereastraInainte As Integer = p.ClientSize.Height
                       Dim randInainte As Integer = p.tlyFiltrare.GetRowHeights()(randConditii)

                       p.btnConditii.Padding = New Padding(12, 20, 12, 20)
                       p.DebugMeasure()

                       Dim crescut As Integer = p.tlyFiltrare.GetRowHeights()(randConditii) - randInainte
                       Assert.True(crescut > 0, "rândul trebuie să crească cu cât nu încape umplutura")
                       Assert.Equal(listaInainte, p.lstValori.Height)
                       Assert.Equal(fereastraInainte + crescut, p.ClientSize.Height)

                       ' …și se întorc amândouă la loc când umplutura pleacă: măsura de referință e
                       ' cea AUTORATĂ, nu cea de acum — altfel a doua comutare de schemă ar crește
                       ' peste rezultatul primeia, la nesfârșit.
                       p.btnConditii.Padding = New Padding(0)
                       p.DebugMeasure()
                       Assert.Equal(randInainte, p.tlyFiltrare.GetRowHeights()(randConditii))
                       Assert.Equal(fereastraInainte, p.ClientSize.Height)
                   End Using
               End Sub)
    End Sub

    ' ── Ce depinde de TIPUL coloanei ────────────────────────────────────────────

    <Fact>
    Public Sub ABooleanColumn_HidesTheConditionsButton()
        ' Două căsuțe spun deja tot ce se poate spune despre o bifă: butonul se ASCUNDE, ca să nu
        ' rămână un rând care nu duce nicăieri. Se verifică prin CONSECINȚA lui — rândul de tabel
        ' se strânge la zero — fiindcă «Visible» pe un formular nearătat răspunde despre lanțul de
        ' părinți, nu despre butonul acesta.
        RunSta(Sub()
                   Using text = Meniu(tip:=KBotValueType.Text)
                       Using logic = Meniu(tip:=KBotValueType.Boolean)
                           text.PerformLayout()
                           logic.PerformLayout()
                           Const randConditii As Integer = 4
                           Assert.True(text.tlyFiltrare.GetRowHeights()(randConditii) > 0)
                           Assert.Equal(0, logic.tlyFiltrare.GetRowHeights()(randConditii))
                       End Using
                   End Using
               End Sub)
    End Sub

    ' ── Cele trei file ──────────────────────────────────────────────────────────

    <Fact>
    Public Sub TheMenuOpensOnTheFilterTab()
        ' Pâlnia din antet se apasă ca să se filtreze; sortarea și gruparea sunt celelalte două
        ' drumuri, nu cel implicit. Se citește FilaCurenta, nu «pnlFiltrare.Visible»: pe un
        ' formular nearătat, getter-ul acela răspunde despre lanțul de părinți.
        RunSta(Sub()
                   Using p = Meniu()
                       Assert.Equal("filtrare", p.FilaCurenta)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SwitchingToTheSortTab_ResizesTheWindowToThatTab()
        ' Fiecare filă își cere propria înălțime: sortarea are patru rânduri fixe, filtrarea are o
        ' listă. O fereastră care ar rămâne la înălțimea celeilalte file ar avea o gaură sub ea.
        RunSta(Sub()
                   Using p = Meniu()
                       Dim filtrare As Integer = p.DebugMeasure().Height
                       p.DebugSelectTab("sortare")
                       Dim sortare As Integer = p.DebugMeasure().Height
                       Assert.NotEqual(filtrare, sortare)
                       p.DebugSelectTab("filtrare")
                       Assert.Equal(filtrare, p.DebugMeasure().Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub WithoutAGrid_TheGroupingTabIsNotOffered()
        ' Fila de grupare vine de la grilă (EnableGrouping). Un meniu fără gazdă n-are pe ce grupa,
        ' deci elementul din bară e ASCUNS — iar KBotNavList refuză o cheie neselectabilă, ceea ce
        ' e chiar dovada că fila nu se poate deschide.
        RunSta(Sub()
                   Using p = Meniu()
                       Assert.Throws(Of ArgumentException)(Sub() p.DebugSelectTab("grupare"))
                       Assert.Equal("filtrare", p.FilaCurenta)
                   End Using
               End Sub)
    End Sub

    ' ── Fila de grupare, cu o grilă în spate ────────────────────────────────────

    ' O grilă mică, gata de grupat: două coloane și patru rânduri, două luni.
    Private Shared Function Grila() As KBotDataView
        Dim g As New KBotDataView()
        g.Size = New Size(600, 400)
        g.AutoSizeColumnsMode = KBotAutoSizeMode.None
        g.ApplyTheme(BuiltInSchemes.Classic())
        g.EnableGrouping = True
        g.AddColumn("luna", "Luna", KBotColumnType.Text, 90)
        g.AddColumn("nume", "Nume", KBotColumnType.Text, 120)
        Umple(g, "ian", "Ana")
        Umple(g, "ian", "Barbu")
        Umple(g, "feb", "Cezar")
        Umple(g, "feb", "Dan")
        Return g
    End Function

    Private Shared Sub Umple(g As KBotDataView, luna As String, nume As String)
        Dim r = g.AddRow()
        r("luna") = luna
        r("nume") = nume
    End Sub

    <Fact>
    Public Sub TheGroupingTab_ReadsTheGridsRealLevels()
        ' «Dinamic după ce e în grilă» înseamnă exact asta: nimic din ce se vede în filă nu e scris
        ' în designer — nici titlul, nici sensul, nici pe al câtelea nivel stă coloana.
        RunSta(Sub()
                   Using g = Grila()
                       g.GroupBy("luna", KBotSortDirection.Descending)
                       Using p = New KBotFilterPopup("luna", "Luna", KBotValueType.Text,
                                                     Valori, Nothing, KBotSortDirection.None, g)
                           Assert.True(p.chkGrupeaza.Checked)
                           Assert.True(p.rbGrupDesc.Checked)
                           Dim linii = p.DebugLevelLines()
                           Assert.Single(linii)
                           Assert.Contains("Luna", linii(0))
                           Assert.Contains("descrescător", linii(0))
                           Assert.Contains("coloana aceasta", linii(0))
                       End Using
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TickingTheGroupingBox_GroupsTheGridOnTheSpot()
        ' Gruparea e o COMANDĂ, ca sortarea: se aplică imediat. Spre deosebire de sortare, meniul
        ' rămâne deschis — fila are șapte opțiuni, nu una.
        RunSta(Sub()
                   Using g = Grila()
                       Using p = New KBotFilterPopup("luna", "Luna", KBotValueType.Text,
                                                     Valori, Nothing, KBotSortDirection.None, g)
                           AddHandler p.GroupingRequested,
                               Sub(s As Object, e As KBotGroupingRequestedEventArgs) g.SetColumnGroupLevel(e.ColumnKey, e.Level)

                           Assert.False(g.IsGrouped)
                           ClicPe(p.chkGrupeaza)                  ' drumul mouse-ului
                           Assert.True(g.IsGrouped)
                           Assert.Equal(2, g.GroupCount())        ' «ian» și «feb»
                           Assert.False(p.IsDisposed)             ' meniul NU s-a închis

                           ClicPe(p.chkGrupeaza)                  ' …și înapoi
                           Assert.False(g.IsGrouped)
                       End Using
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TheGroupingTab_KeepsTheLevelsOwnAppearance()
        ' Nivelul EXISTENT se refolosește, nu se înlocuiește: pe el pot sta culori și fonturi puse
        ' din designer, iar o bifă din meniu n-are voie să le șteargă.
        RunSta(Sub()
                   Using g = Grila()
                       Dim nivel = g.GroupBy("luna")
                       nivel.HeaderBackColor = Color.Goldenrod
                       Using p = New KBotFilterPopup("luna", "Luna", KBotValueType.Text,
                                                     Valori, Nothing, KBotSortDirection.None, g)
                           ClicPe(p.chkGrupSubsol)                ' stinge banda de subsol
                           Dim iesit = p.BuildGroupLevel()
                           Assert.False(iesit.ShowFooter)
                           Assert.Equal(Color.Goldenrod, iesit.HeaderBackColor)
                       End Using
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SortCaptions_ComeFromTheValueType()
        RunSta(Sub()
                   Using text = Meniu(tip:=KBotValueType.Text)
                       Using numar = Meniu(tip:=KBotValueType.Number)
                           Assert.NotEqual(text.btnSortAsc.Text, numar.btnSortAsc.Text)
                           Assert.False(String.IsNullOrWhiteSpace(text.btnSortAsc.Text))
                           Assert.False(String.IsNullOrWhiteSpace(numar.btnSortDesc.Text))
                       End Using
                   End Using
               End Sub)
    End Sub

End Class
