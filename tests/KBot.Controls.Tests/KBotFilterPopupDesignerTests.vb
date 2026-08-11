Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls

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

    ' ── Butoane mai înalte (schema Modern) nu mănâncă din listă ─────────────────

    <Fact>
    Public Sub WhenACommandButtonGrows_TheWindowGrowsWithIt()
        ' Schema «Modern» cere aer în jurul textului, deci butoanele de comandă cresc
        ' (vezi ModernButtonHeightTests). Fiind andocate SUS, creșterea lor ar mușca din lista de
        ' dedesubt, care e Fill — meniul ar arăta pe urmă mai puține valori decât înainte.
        ' Testul crește butonul DIRECT, ca să nu mute schema globală a procesului (convenția casei
        ' din AdvancedTreeThemingTests): ce se probează aici e re-măsurarea ferestrei, nu tema.
        RunSta(Sub()
                   Using p = Meniu()
                       p.PerformLayout()
                       Dim inainte As Integer = p.lstValori.Height
                       Dim inaltimeFereastra As Integer = p.ClientSize.Height

                       p.btnSortAsc.Height += 20
                       p.DebugMeasure()

                       Assert.Equal(inainte, p.lstValori.Height)
                       Assert.Equal(inaltimeFereastra + 20, p.ClientSize.Height)
                   End Using
               End Sub)
    End Sub

    ' ── Ce depinde de TIPUL coloanei ────────────────────────────────────────────

    <Fact>
    Public Sub ABooleanColumn_HidesTheConditionsButton()
        ' Două căsuțe spun deja tot ce se poate spune despre o bifă: butonul se ASCUNDE (nu se
        ' stinge), ca să nu rămână un rând care nu duce nicăieri. Se verifică prin CONSECINȚA lui —
        ' rândul dispare, deci ce urmează urcă — fiindcă «Visible» pe un formular nearătat răspunde
        ' despre lanțul de părinți, nu despre butonul acesta.
        RunSta(Sub()
                   Using text = Meniu(tip:=KBotValueType.Text)
                       Using logic = Meniu(tip:=KBotValueType.Boolean)
                           text.PerformLayout()
                           logic.PerformLayout()
                           Assert.True(logic.txtCauta.Top < text.txtCauta.Top,
                                       "fără butonul de condiții, căutarea trebuie să urce cu un rând")
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
