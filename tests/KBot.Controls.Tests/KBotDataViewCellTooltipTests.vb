Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028: eticheta plutitoare pentru celulele al căror text NU încape.
'''
''' Decizia («cine primește etichetă și cu ce text») e o funcție pură — <c>CellTooltipTextFor</c> —
''' tocmai ca să se poată verifica fără ecran: testele n-au cum să plimbe un mouse, dar pot pune
''' exact întrebarea pe care o pune și <c>OnMouseMove</c>. Fereastra însăși e doar randare, ca la
''' <c>TreeNodeFlyout</c>, și se vede în DevHarness.
''' </summary>
Public Class KBotDataViewCellTooltipTests

    Private Const TextLung As String =
        "Denumire foarte lungă de indicator, care nu are cum să încapă într-o coloană îngustă"

    Private Shared Function Grid(latimeColoana As Integer) As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(500, 300)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        ' Auto-size ar lăți coloana până când textul încape, adică exact cazul pe care îl testăm
        ' n-ar mai exista niciodată.
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        Dim c = dv.AddColumn("den", "Denumire", KBotColumnType.Text, latimeColoana)
        c.MaxWidth = latimeColoana
        Return dv
    End Function

    <Fact>
    Public Sub TextThatFits_GetsNoTooltip()
        Using dv = Grid(300)
            dv.AddRow()("den") = "scurt"
            Assert.Null(dv.CellTooltipTextFor("den", 0))
        End Using
    End Sub

    <Fact>
    Public Sub TextThatDoesNotFit_GetsTheFullTextAsTooltip()
        Using dv = Grid(80)
            dv.AddRow()("den") = TextLung
            Assert.Equal(TextLung, dv.CellTooltipTextFor("den", 0))
        End Using
    End Sub

    <Fact>
    Public Sub EmptyCell_GetsNoTooltip()
        Using dv = Grid(80)
            dv.AddRow()
            Assert.Null(dv.CellTooltipTextFor("den", 0))
        End Using
    End Sub

    <Fact>
    Public Sub WideningTheColumn_TakesTheTooltipAway()
        Using dv = Grid(80)
            dv.AddRow()("den") = TextLung
            Assert.NotNull(dv.CellTooltipTextFor("den", 0))

            dv.Column("den").MaxWidth = 4000
            dv.Column("den").Width = 4000
            Assert.Null(dv.CellTooltipTextFor("den", 0))
        End Using
    End Sub

    <Fact>
    Public Sub TheTooltipShowsWhatTheCellWOULDShow_IncludingAFormattingHandlersText()
        ' Eticheta trece prin aceeași formatare ca pictarea: dacă un handler a înlocuit textul,
        ' eticheta arată textul ACELA, nu valoarea brută — altfel ar contrazice ce e pe ecran.
        Using dv = Grid(80)
            dv.AddRow()("den") = "brut"
            AddHandler dv.CellFormatting,
                Sub(s As Object, e As KBotCellFormattingEventArgs)
                    e.Text = TextLung
                End Sub
            Assert.Equal(TextLung, dv.CellTooltipTextFor("den", 0))
        End Using
    End Sub

    <Fact>
    Public Sub ColumnsWithoutTextOfTheirOwn_NeverGetATooltip()
        Using dv As New KBotDataView()
            dv.Size = New Size(500, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.AddColumn("bif", "Bifat", KBotColumnType.CheckBox, 40)
            dv.AddColumn("prg", "Progres", KBotColumnType.ProgressBar, 40)
            dv.AddRow()
            dv("bif", 0) = True
            dv("prg", 0) = 50.0
            Assert.Null(dv.CellTooltipTextFor("bif", 0))
            Assert.Null(dv.CellTooltipTextFor("prg", 0))
        End Using
    End Sub

    <Fact>
    Public Sub ComboCell_AccountsForTheChevron()
        ' La un combo, chevronul mănâncă din lățimea utilă: un text care „încăpea” într-o coloană
        ' de text trebuie să primească etichetă aici, altfel taman coada ascunsă sub săgeată ar
        ' rămâne necitibilă.
        Using dv As New KBotDataView()
            dv.Size = New Size(500, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            Dim latime As Integer = 0

            ' Se caută o lățime la care textul încape la Text, dar nu și la Combo.
            Dim textProba As String = "Valoare medie lunară"
            For w As Integer = 60 To 400 Step 2
                Dim t = dv.AddColumn("t" & w.ToString(), "T", KBotColumnType.Text, w)
                t.MaxWidth = w
                Dim c = dv.AddColumn("c" & w.ToString(), "C", KBotColumnType.Combo, w)
                c.MaxWidth = w
                If dv.RowCount = 0 Then dv.AddRow()
                dv("t" & w.ToString(), 0) = textProba
                dv("c" & w.ToString(), 0) = textProba
                If dv.CellTooltipTextFor("t" & w.ToString(), 0) Is Nothing AndAlso
                   dv.CellTooltipTextFor("c" & w.ToString(), 0) IsNot Nothing Then
                    latime = w
                    Exit For
                End If
            Next

            Assert.True(latime > 0, "nu s-a găsit nicio lățime la care chevronul să conteze")
        End Using
    End Sub

    <Fact>
    Public Sub TheTooltipObjectIsExposed_WithThemeDefaults()
        Using dv = Grid(80)
            Assert.NotNull(dv.CellTooltip)
            Assert.True(dv.CellTooltip.Enabled)
            ' Gol / nesetat = «din temă», convenția K-BOT peste tot.
            Assert.Equal(Color.Empty, dv.CellTooltip.BackColor)
            Assert.Equal(Color.Empty, dv.CellTooltip.ForeColor)
            Assert.Equal(Color.Empty, dv.CellTooltip.BorderColor)
            Assert.Null(dv.CellTooltip.Font)
        End Using
    End Sub

    <Fact>
    Public Sub UnknownColumnOrRow_IsAskedSafely_NotThrown()
        ' Interogarea vine dintr-un OnMouseMove, deci trebuie să suporte o țintă dispărută între
        ' timp (rând șters, coloană redenumită) fără să arunce în bucla de mesaje.
        Using dv = Grid(80)
            dv.AddRow()("den") = TextLung
            Assert.Null(dv.CellTooltipTextFor("inexistenta", 0))
            Assert.Null(dv.CellTooltipTextFor("den", 99))
            Assert.Null(dv.CellTooltipTextFor(Nothing, 0))
        End Using
    End Sub

End Class
