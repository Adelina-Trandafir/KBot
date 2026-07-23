Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Teste pentru slice 0016: coloane care se pot ascunde automat (`Column.AutoHide`) când n-ar
''' încăpea fără scrollbar orizontal, plus interacțiunea cu „coloana care se întinde”
''' (`ColumnFillMode` First/Last) — care nu dispare niciodată și umple golul.
'''
''' Grila e fixată pe dimensionare MANUALĂ (AutoSize = None) ca lățimile — deci și pragul de
''' încăpere — să fie deterministe: 5 coloane × 100px într-o fereastră de 250px lată (fără
''' vscroll, deci lățimea utilă e exact 250).
''' </summary>
Public Class KBotDataViewAutoHideTests

    Private Shared Function Grid5(Optional width As Integer = 250) As KBotDataView
        Dim dv As New KBotDataView()
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None    ' păstrează lățimile exact
        dv.ColumnFillMode = KBotFillMode.None
        dv.Size = New Size(width, 150)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        For i As Integer = 0 To 4
            dv.AddColumn("c" & i.ToString(), "C" & i.ToString(), KBotColumnType.Text, 100)
        Next
        dv.AddRow()
        dv.AddRow()
        Return dv
    End Function

    Private Shared Sub SetAutoHideAll(dv As KBotDataView, value As Boolean)
        For Each c In dv.Columns
            c.AutoHide = value
        Next
    End Sub

    ' ── Ascundere simplă (fără fill) ────────────────────────────────────────────

    <Fact>
    Public Sub AllAutoHide_HidesFromRightUntilTheRestFit()
        Using dv = Grid5(250)                              ' 5 × 100 = 500 nu încape în 250
            SetAutoHideAll(dv, True)
            dv.AutoSizeColumns()

            ' 500 → ascunde c4/c3/c2 (dreapta întâi) => rămân c0,c1 (200 <= 250).
            Assert.True(dv.Column("c0").IsEffectivelyVisible)
            Assert.True(dv.Column("c1").IsEffectivelyVisible)
            Assert.True(dv.Column("c2").AutoHidden)
            Assert.True(dv.Column("c3").AutoHidden)
            Assert.True(dv.Column("c4").AutoHidden)
            ' Fără fill: supraviețuitoarele NU se lățesc, iar golul rămâne gol (fără scrollbar).
            Assert.Equal(100, dv.Column("c0").Width)
            Assert.Equal(100, dv.Column("c1").Width)
            Assert.False(dv.hScroll.Visible)
        End Using
    End Sub

    <Fact>
    Public Sub Visible_Untouched_AutoHideUsesASeparateFlag()
        Using dv = Grid5(250)
            SetAutoHideAll(dv, True)
            dv.AutoSizeColumns()
            ' Coloanele ascunse automat au Visible tot True — doar AutoHidden e setat.
            Assert.True(dv.Column("c4").Visible)
            Assert.True(dv.Column("c4").AutoHidden)
            Assert.False(dv.Column("c4").IsEffectivelyVisible)
        End Using
    End Sub

    ' ── Fără mai multe hidable => scrollbar normal ──────────────────────────────

    <Fact>
    Public Sub WhenNoMoreHideable_ScrollbarAppearsNormally()
        Using dv = Grid5(250)
            dv.Column("c2").AutoHide = True               ' singura care poate dispărea
            dv.AutoSizeColumns()

            Assert.True(dv.Column("c2").AutoHidden)        ' a dispărut…
            ' …dar 400 tot > 250 și nu mai e ce ascunde => scrollbarul apare.
            Assert.True(dv.hScroll.Visible)
            Assert.True(dv.Column("c0").IsEffectivelyVisible)
            Assert.True(dv.Column("c4").IsEffectivelyVisible)
        End Using
    End Sub

    ' ── Fill: coloana care se întinde umple golul și nu dispare ─────────────────

    <Fact>
    Public Sub FillLastColumn_ExpanderSurvivesAndAbsorbsTheGap()
        Using dv = Grid5(250)
            SetAutoHideAll(dv, True)                       ' inclusiv expanderul
            dv.ColumnFillMode = KBotFillMode.LastColumn    ' c4 se întinde
            dv.AutoSizeColumns()

            ' c4 e AND AutoHide AND expander => întinderea are prioritate: NU dispare.
            Assert.True(dv.Column("c4").IsEffectivelyVisible)
            Assert.False(dv.Column("c4").AutoHidden)
            Assert.True(dv.Column("c0").IsEffectivelyVisible)
            Assert.True(dv.Column("c1").AutoHidden)
            Assert.True(dv.Column("c2").AutoHidden)
            Assert.True(dv.Column("c3").AutoHidden)

            ' Golul lăsat de c1..c3 e umplut de c4: c0(100) + c4 = 250, fără scrollbar.
            Assert.Equal(100, dv.Column("c0").Width)
            Assert.Equal(150, dv.Column("c4").Width)
            Assert.False(dv.hScroll.Visible)
        End Using
    End Sub

    <Fact>
    Public Sub FillFirstColumn_ExpanderIsTheFirstAndAbsorbsTheGap()
        Using dv = Grid5(250)
            SetAutoHideAll(dv, True)
            dv.ColumnFillMode = KBotFillMode.FirstColumn   ' c0 se întinde
            dv.AutoSizeColumns()

            Assert.True(dv.Column("c0").IsEffectivelyVisible)
            Assert.False(dv.Column("c0").AutoHidden)
            Assert.True(dv.Column("c1").IsEffectivelyVisible)   ' c0,c1 rămân
            Assert.True(dv.Column("c2").AutoHidden)
            Assert.True(dv.Column("c3").AutoHidden)
            Assert.True(dv.Column("c4").AutoHidden)

            Assert.Equal(150, dv.Column("c0").Width)       ' c0 umple golul: 150 + 100 = 250
            Assert.Equal(100, dv.Column("c1").Width)
            Assert.False(dv.hScroll.Visible)
        End Using
    End Sub

    <Fact>
    Public Sub ExpanderThatIsAlsoTheOnlyHideable_NeverDisappears()
        Using dv = Grid5(250)
            dv.Column("c4").AutoHide = True                ' doar expanderul e hidable
            dv.ColumnFillMode = KBotFillMode.LastColumn
            dv.AutoSizeColumns()

            ' Expanderul e protejat => nimic nu dispare => 500 > 250 => scrollbar.
            Assert.False(dv.Column("c4").AutoHidden)
            Assert.True(dv.hScroll.Visible)
        End Using
    End Sub

    ' ── Reapariție la lărgire ───────────────────────────────────────────────────

    <Fact>
    Public Sub WideningTheGrid_BringsAutoHiddenColumnsBack()
        Using dv = Grid5(250)
            SetAutoHideAll(dv, True)
            dv.AutoSizeColumns()
            Assert.True(dv.Column("c4").AutoHidden)        ' ascunsă la 250

            dv.Size = New Size(600, 150)                   ' acum 500 <= 600 => încap toate
            For Each c In dv.Columns
                Assert.True(c.IsEffectivelyVisible, $"'{c.Key}' trebuie să reapară la lărgire")
            Next
        End Using
    End Sub

    ' ── Navigație: o coloană ascunsă automat e sărită ───────────────────────────

    <Fact>
    Public Sub AutoHiddenColumn_IsSkippedByNavigation()
        Using dv = Grid5(600)                              ' larg: nimic ascuns inițial
            dv.Column("c2").AutoHide = True
            dv.Size = New Size(250, 150)                    ' îngustează => c2 dispare
            dv.AutoSizeColumns()
            Assert.True(dv.Column("c2").AutoHidden)

            ' De la c1, următoarea coloană activă sare peste c2 (ascunsă) la c3.
            Assert.Equal("c3", dv.NextEnabledColumn("c1", 1).Key)
        End Using
    End Sub

    ' ── Fără AutoHide: comportamentul slice 0013 rămâne neschimbat ──────────────

    <Fact>
    Public Sub NoAutoHideColumns_ShrinkToFitStillApplies()
        Using dv = Grid5(250)
            dv.ColumnFillMode = KBotFillMode.LastColumn    ' fără AutoHide => 0013 micșorează
            dv.AutoSizeColumns()

            ' Nicio coloană nu dispare; se micșorează ca să încapă (fără scrollbar).
            For Each c In dv.Columns
                Assert.False(c.AutoHidden)
            Next
            Assert.False(dv.hScroll.Visible)
            Assert.True(SumVisibleWidths(dv) <= 250)
        End Using
    End Sub

    Private Shared Function SumVisibleWidths(dv As KBotDataView) As Integer
        Dim total As Integer = 0
        For Each c In dv.Columns
            If c.IsEffectivelyVisible Then total += c.Width
        Next
        Return total
    End Function

End Class
