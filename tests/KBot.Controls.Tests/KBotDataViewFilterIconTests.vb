Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Tests for the header filter icon (slice 0028-03): where it sits, that it never collides with
''' the per-column right icon, the width floor it imposes, the per-column opt-out — and the
''' designer-serialization pair without which Visual Studio would freeze the resolved defaults
''' into every host form (the trap documented in CLAUDE.md).
'''
''' Placement is asserted through <c>ComputeHeaderCellLayout</c>, the same pure function the
''' painter and the hit-test call, so a passing test means the icon is drawn where it is clicked.
''' </summary>
Public Class KBotDataViewFilterIconTests

    Private Const Pad As Integer = KBotDataColumn.HeaderIconPad     ' 8
    Private Const Gap As Integer = KBotDataColumn.HeaderIconGap     ' 4

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 300)
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Return dv
    End Function

    ' ── Așezare ──────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub FilterIcon_SitsAtTheRightEnd()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.ShowColumnFilter = True

            Dim l = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, 200, 30),
                                                         Pad, Gap, New Size(16, 16))
            Assert.Equal(200 - Pad - 16, l.FilterIcon.Left)
            Assert.Equal((30 - 16) \ 2, l.FilterIcon.Top)
        End Using
    End Sub

    <Fact>
    Public Sub FilterIcon_PushesTheColumnRightIcon_Leftwards_NeverOverlapping()
        ' Pictograma de filtru e o funcție a GRILEI: cade în același loc pe orice coloană, iar cea
        ' a coloanei se dă la o parte. Două butoane suprapuse ar însemna un click ambiguu.
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 240)
            col.HeaderRightIcon = New Bitmap(16, 16)
            col.ShowColumnFilter = True

            Dim l = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, 240, 30),
                                                         Pad, Gap, New Size(16, 16))
            Assert.False(l.FilterIcon.IsEmpty)
            Assert.False(l.RightIcon.IsEmpty)
            Assert.False(l.FilterIcon.IntersectsWith(l.RightIcon))
            Assert.Equal(l.FilterIcon.Left - Gap, l.RightIcon.Right)
            ' Titlul se oprește înaintea amândurora.
            Assert.True(l.Text.Right <= l.RightIcon.Left - Gap)
        End Using
    End Sub

    <Fact>
    Public Sub NoFilterSize_LeavesTheOldLayoutUntouched()
        ' Grilele care nu aprind filtrarea trebuie să arate exact ca înainte de slice 0028-03.
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Dim faraFiltru = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, 200, 30), Pad, Gap)
            Assert.True(faraFiltru.FilterIcon.IsEmpty)
            Assert.Equal(Pad, faraFiltru.Text.Left)
            Assert.Equal(200 - Pad, faraFiltru.Text.Right)
        End Using
    End Sub

    ' ── Podeaua de lățime ────────────────────────────────────────────────────────

    <Fact>
    Public Sub TurningTheFilterOn_RaisesTheColumnWidthFloor()
        ' MinWidth coborât dinadins: pe implicitul de 40 px podeaua dată de o singură pictogramă
        ' (2×8 + 16 = 32) e deja acoperită, deci n-ar dovedi nimic.
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.MinWidth = 10
            Assert.Equal(10, col.EffectiveMinWidth)

            col.ShowColumnFilter = True
            Assert.Equal(2 * Pad + col.ColumnFilterIconSize.Width, col.EffectiveMinWidth)

            ' O coloană strâmtată la zero nu coboară sub podea, deci piesele nu se suprapun.
            col.Width = 1
            Assert.Equal(col.EffectiveMinWidth, col.Width)
        End Using
    End Sub

    <Fact>
    Public Sub TheFloorCountsTheFilterIcon_OnTopOfTheColumnsOwnIcons()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 300)
            col.MinWidth = 10
            col.HeaderLeftIcon = New Bitmap(16, 16)
            col.HeaderRightIcon = New Bitmap(16, 16)
            Dim podeaFara As Integer = col.EffectiveMinWidth

            col.ShowColumnFilter = True
            ' Cele trei piese + două spații între ele, plus aerul de la capete.
            Assert.Equal(2 * Pad + 3 * 16 + 2 * Gap, col.EffectiveMinWidth)
            Assert.True(col.EffectiveMinWidth > podeaFara)
        End Using
    End Sub

    <Fact>
    Public Sub EachColumnDecidesForItself()
        ' Cerința: butonul se aprinde COLOANĂ CU COLOANĂ, din designer. O coloană aprinsă nu
        ' trage după ea vecinele.
        Using dv = Grid()
            Dim cuFiltru = dv.AddColumn("a", "Cu filtru", KBotColumnType.Text, 200)
            Dim faraFiltru = dv.AddColumn("b", "Fără filtru", KBotColumnType.Text, 200)
            cuFiltru.ShowColumnFilter = True

            Assert.False(dv.FilterIconSizeFor(cuFiltru).IsEmpty)
            Assert.True(dv.FilterIconSizeFor(faraFiltru).IsEmpty)
            Assert.False(dv.DebugFilterIconRect("a").IsEmpty)
            Assert.True(dv.DebugFilterIconRect("b").IsEmpty)
        End Using
    End Sub

    ' ── Tipurile pe care filtrarea nu are ce însemna ─────────────────────────────

    <Theory>
    <InlineData(KBotColumnType.Button)>
    <InlineData(KBotColumnType.ProgressBar)>
    Public Sub ForbiddenColumnTypes_RejectTheFilter_Loudly(tip As KBotColumnType)
        ' Un buton care nu apare acolo unde a fost cerut e chiar no-op-ul tăcut interzis de
        ' regula casei — deci se ARUNCĂ, nu se stinge în tăcere.
        Using dv = Grid()
            Dim col = dv.AddColumn("x", "Acțiune", tip, 200)
            Assert.Throws(Of ArgumentException)(Sub() col.ShowColumnFilter = True)
            Assert.False(col.ShowColumnFilter)
            Assert.True(dv.FilterIconSizeFor(col).IsEmpty)
        End Using
    End Sub

    <Fact>
    Public Sub ChangingTheTypeToAForbiddenOne_IsRejectedToo()
        ' Perechea se apără din AMÂNDOUĂ direcțiile, altfel regula s-ar ocoli aprinzând filtrul
        ' pe o coloană de text și mutând apoi tipul pe Button.
        Using dv = Grid()
            Dim col = dv.AddColumn("x", "Text", KBotColumnType.Text, 200)
            col.ShowColumnFilter = True
            Assert.Throws(Of ArgumentException)(Sub() col.ColumnType = KBotColumnType.Button)
            Assert.Equal(KBotColumnType.Text, col.ColumnType)
        End Using
    End Sub

    <Fact>
    Public Sub AForbiddenPair_SurvivesUntilEndInit_ThenThrows()
        ' În blocul designerului ordinea proprietăților e a LUI: «ShowColumnFilter» poate ajunge
        ' înaintea lui «ColumnType», iar o excepție acolo ar închide formularul cu totul. Perechea
        ' AȘEZATĂ se verifică la EndInit.
        Using dv As New KBotDataView()
            dv.BeginInit()
            Dim col As New KBotDataColumn() With {.Key = "x", .ShowColumnFilter = True}
            dv.Columns.Add(col)
            col.ColumnType = KBotColumnType.ProgressBar      ' trece: suntem în BeginInit
            Assert.Throws(Of ArgumentException)(Sub() dv.EndInit())
        End Using
    End Sub

    <Fact>
    Public Sub FilterOff_MeansNoIconAnywhere()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Assert.True(dv.FilterIconSizeFor(col).IsEmpty)
            Assert.True(dv.DebugFilterIconRect("a").IsEmpty)
        End Using
    End Sub

    <Fact>
    Public Sub TheIconIsOnScreen_WhereTheLayoutSaysItIs()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.ShowColumnFilter = True
            Dim r = dv.DebugFilterIconRect("a")
            Assert.False(r.IsEmpty)
            Assert.Equal(col.ColumnFilterIconSize.Width, r.Width)
        End Using
    End Sub

    ' ── Serializarea în designer (regula casei) ──────────────────────────────────

    <Fact>
    Public Sub AFreshColumn_SerialisesNoFilterProperty()
        ' Verificat pe calea pe care o ia CHIAR Visual Studio (TypeDescriptor), nu chemând
        ' ShouldSerializeX direct — acela n-ar dovedi nimic.
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Assert.False(ShouldSerialize(col, "ColumnFilterIconSize"))
            Assert.False(ShouldSerialize(col, "ColumnFilterIcon"))
            Assert.False(ShouldSerialize(col, "ColumnFilterHoverColor"))
        End Using
    End Sub

    <Fact>
    Public Sub AValueSetByTheOperator_IsSerialised_AndResetTakesItBack()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.ColumnFilterIconSize = New Size(20, 20)
            col.ColumnFilterHoverColor = Color.Firebrick
            Assert.True(ShouldSerialize(col, "ColumnFilterIconSize"))
            Assert.True(ShouldSerialize(col, "ColumnFilterHoverColor"))

            TypeDescriptor.GetProperties(col)("ColumnFilterIconSize").ResetValue(col)
            TypeDescriptor.GetProperties(col)("ColumnFilterHoverColor").ResetValue(col)
            Assert.False(ShouldSerialize(col, "ColumnFilterIconSize"))
            Assert.False(ShouldSerialize(col, "ColumnFilterHoverColor"))
        End Using
    End Sub

    ' ── Mărimea pusă pe GRILĂ ────────────────────────────────────────────────────

    <Fact>
    Public Sub TheGridSize_DrivesEveryColumn()
        ' Filtrul e o funcție a grilei: mărimea se pune o dată și se vede pe tot antetul.
        Using dv = Grid()
            Dim a = dv.AddColumn("a", "A", KBotColumnType.Text, 200)
            Dim b = dv.AddColumn("b", "B", KBotColumnType.Text, 200)
            a.ShowColumnFilter = True
            b.ShowColumnFilter = True

            dv.FilterIconSize = New Size(24, 24)
            Assert.Equal(New Size(24, 24), a.ColumnFilterIconSize)
            Assert.Equal(New Size(24, 24), b.ColumnFilterIconSize)
            Assert.Equal(24, dv.FilterIconSizeFor(a).Width)
            Assert.Equal(24, dv.DebugFilterIconRect("b").Width)
            ' Podeaua de lățime crește odată cu pictograma, altfel piesele s-ar suprapune.
            a.MinWidth = 10
            Assert.Equal(2 * Pad + 24, a.EffectiveMinWidth)
        End Using
    End Sub

    <Fact>
    Public Sub AColumnThatSaysOtherwise_WinsOverTheGrid()
        Using dv = Grid()
            Dim a = dv.AddColumn("a", "A", KBotColumnType.Text, 200)
            Dim b = dv.AddColumn("b", "B", KBotColumnType.Text, 200)
            a.ShowColumnFilter = True
            b.ShowColumnFilter = True

            dv.FilterIconSize = New Size(24, 24)
            b.ColumnFilterIconSize = New Size(12, 12)
            Assert.Equal(24, dv.DebugFilterIconRect("a").Width)
            Assert.Equal(12, dv.DebugFilterIconRect("b").Width)

            ' Mutarea grilei nu mai atinge coloana pe care a scris operatorul.
            dv.FilterIconSize = New Size(32, 32)
            Assert.Equal(32, dv.DebugFilterIconRect("a").Width)
            Assert.Equal(12, dv.DebugFilterIconRect("b").Width)
        End Using
    End Sub

    <Fact>
    Public Sub AnExplicitColumnSize_IsSerialised_EvenWhenItEqualsTheOldDefault()
        ' 16×16 scris DINADINS pe o coloană dintr-o grilă trecută pe 24 e o alegere, nu o
        ' întâmplare: trebuie să supraviețuiască salvării.
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "A", KBotColumnType.Text, 200)
            col.ShowColumnFilter = True
            dv.FilterIconSize = New Size(24, 24)

            col.ColumnFilterIconSize = New Size(16, 16)
            Assert.True(ShouldSerialize(col, "ColumnFilterIconSize"))
            Assert.Equal(16, dv.DebugFilterIconRect("a").Width)

            ' Reset = «înapoi la grilă», și nu lasă nimic în urmă în .Designer.vb.
            TypeDescriptor.GetProperties(col)("ColumnFilterIconSize").ResetValue(col)
            Assert.False(ShouldSerialize(col, "ColumnFilterIconSize"))
            Assert.Equal(24, dv.DebugFilterIconRect("a").Width)
        End Using
    End Sub

    <Fact>
    Public Sub AFreshGrid_SerialisesNoFilterIconSize()
        Using dv = Grid()
            Assert.Equal(New Size(16, 16), dv.FilterIconSize)
            Assert.False(ShouldSerialize(dv, "FilterIconSize"))
            dv.FilterIconSize = New Size(24, 24)
            Assert.True(ShouldSerialize(dv, "FilterIconSize"))
            TypeDescriptor.GetProperties(dv)("FilterIconSize").ResetValue(dv)
            Assert.False(ShouldSerialize(dv, "FilterIconSize"))
        End Using
    End Sub

    Private Shared Function ShouldSerialize(target As Object, propertyName As String) As Boolean
        Return TypeDescriptor.GetProperties(target)(propertyName).ShouldSerializeValue(target)
    End Function

End Class
