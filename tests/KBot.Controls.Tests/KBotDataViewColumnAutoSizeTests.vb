Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028-04: the PER-COLUMN <c>AutoSizeMode</c> and its precedence over the grid-wide
''' <c>AutoSizeColumnsMode</c>. Same headless setup as <see cref="KBotDataViewAutoSizeTests"/>
''' (no handle; ClientSize follows Size; the pass runs on EndUpdate). The assertions are about
''' WHICH columns the measuring pass touches, so they compare against the caller's own width
''' rather than against pixel counts, which are font/DPI dependent.
''' </summary>
Public Class KBotDataViewColumnAutoSizeTests

    Private Const LongText As String = "a fairly long cell value that easily beats the header"

    Private Shared Function NewGrid(w As Integer, h As Integer) As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(w, h)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Return dv
    End Function

    ' Două coloane cu ACELAȘI conținut lung și aceeași lățime de start — ce le desparte pe urmă
    ' e doar modul lor de auto-dimensionare.
    Private Shared Function TwoWideContentColumns(dv As KBotDataView) As KBotDataView
        dv.BeginUpdate()
        dv.AddColumn("a", "A", KBotColumnType.Text, 60)
        dv.AddColumn("b", "B", KBotColumnType.Text, 60)
        Dim r = dv.AddRow()
        r("a") = LongText
        r("b") = LongText
        dv.EndUpdate()
        Return dv
    End Function

    ' ── Implicit ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Default_ColumnInheritsFromGrid()
        Using dv = NewGrid(900, 400)
            TwoWideContentColumns(dv)

            Assert.Equal(KBotAutoSizeMode.Inherit, dv.Column("a").AutoSizeMode)
            ' Grila e pe ToContent implicit, deci o coloană «fără opinie» tot se măsoară.
            Assert.True(dv.Column("a").Width > 60, "coloana Inherit trebuie măsurată ca până acum")
        End Using
    End Sub

    ' ── Precedența coloanei ──────────────────────────────────────────────────────

    <Fact>
    Public Sub ColumnNone_BeatsGridToContent()
        Using dv = NewGrid(900, 400)
            TwoWideContentColumns(dv)
            Assert.Equal(KBotAutoSizeMode.ToContent, dv.AutoSizeColumnsMode)

            dv.Column("a").AutoSizeMode = KBotAutoSizeMode.None
            dv.Column("a").Width = 60                       ' lățimea pe care o vrea caller-ul
            dv.AutoSizeColumns()

            Assert.Equal(60, dv.Column("a").Width)          ' fixată de coloană
            Assert.True(dv.Column("b").Width > 60, "vecina fără opinie trebuie să rămână măsurată")
        End Using
    End Sub

    <Fact>
    Public Sub ColumnToContent_BeatsGridNone()
        Using dv = NewGrid(900, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            TwoWideContentColumns(dv)

            ' Grila e manuală: nimic nu s-a măsurat.
            Assert.Equal(60, dv.Column("a").Width)
            Assert.Equal(60, dv.Column("b").Width)

            dv.Column("a").AutoSizeMode = KBotAutoSizeMode.ToContent
            dv.AutoSizeColumns()

            Assert.True(dv.Column("a").Width > 60, "coloana cere singură măsurarea, chiar dacă grila nu")
            Assert.Equal(60, dv.Column("b").Width)          ' restul rămâne manual
        End Using
    End Sub

    <Fact>
    Public Sub ColumnToContent_RunsPassEvenWhenGridIsFullyManual()
        ' Poarta din PerformAutoSize (grilă None + fără umplere + fără auto-hide) nu are voie să
        ' oprească pasul când o coloană a cerut măsurarea — altfel butonul ar fi un no-op tăcut.
        Using dv = NewGrid(900, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.None
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 60)
            dv.Column("a").AutoSizeMode = KBotAutoSizeMode.ToContent
            Dim r = dv.AddRow()
            r("a") = LongText
            dv.EndUpdate()

            Assert.True(dv.Column("a").Width > 60)
        End Using
    End Sub

    <Fact>
    Public Sub SettingColumnMode_RelayoutsImmediately()
        ' Setter-ul cere singur re-layout (OnColumnAutoSizeModeChanged) — fără AutoSizeColumns().
        Using dv = NewGrid(900, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            TwoWideContentColumns(dv)

            dv.Column("a").AutoSizeMode = KBotAutoSizeMode.ToContent

            Assert.True(dv.Column("a").Width > 60)
        End Using
    End Sub

    <Fact>
    Public Sub UserSizedColumn_StaysUntouchedEvenWithColumnToContent()
        ' Precedența e față de GRILĂ, nu față de operator: o coloană trasă cu mouse-ul rămâne
        ' a operatorului până la ResetColumnSizing.
        Using dv = NewGrid(900, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            TwoWideContentColumns(dv)

            dv.Column("a").AutoSizeMode = KBotAutoSizeMode.ToContent
            dv.Column("a").Width = 70
            dv.Column("a").UserSized = True
            dv.AutoSizeColumns()
            Assert.Equal(70, dv.Column("a").Width)

            dv.ResetColumnSizing()
            Assert.True(dv.Column("a").Width > 70, "după reset, coloana se măsoară din nou")
        End Using
    End Sub

    ' ── Precedența acoperă DOAR măsurarea ────────────────────────────────────────

    <Fact>
    Public Sub ColumnNone_StillParticipatesInFill()
        ' Umplerea e alt buton: o coloană «None» nu se măsoară, dar spațiul rămas tot se cheltuie
        ' pe ea dacă e ținta umplerii — exact ca la o coloană trasă de operator.
        Using dv = NewGrid(500, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.LastColumn
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.AddColumn("b", "B", KBotColumnType.Text, 100)
            dv.Column("b").AutoSizeMode = KBotAutoSizeMode.None
            dv.AddRow()
            dv.EndUpdate()

            Assert.Equal(dv.ClientSize.Width, dv.Column("a").Width + dv.Column("b").Width)
            Assert.True(dv.Column("b").Width > 100, "ținta umplerii crește chiar și pe None")
        End Using
    End Sub

    ' ── Valori refuzate ──────────────────────────────────────────────────────────

    <Fact>
    Public Sub GridMode_RejectsInherit()
        Using dv As New KBotDataView()
            Assert.Throws(Of ArgumentException)(
                Sub() dv.AutoSizeColumnsMode = KBotAutoSizeMode.Inherit)
            Assert.Equal(KBotAutoSizeMode.ToContent, dv.AutoSizeColumnsMode)
        End Using
    End Sub

    <Fact>
    Public Sub ColumnMode_RejectsUndefinedValue()
        Dim col As New KBotDataColumn("k", "K", KBotColumnType.Text, 100)
        Assert.Throws(Of ArgumentException)(
            Sub() col.AutoSizeMode = CType(42, KBotAutoSizeMode))
    End Sub

End Class
