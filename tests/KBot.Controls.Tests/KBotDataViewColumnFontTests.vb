Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Fonturile CERUTE PE COLOANĂ: <see cref="KBotDataColumn.HeaderFont"/> (titlul) și
''' <see cref="KBotDataColumn.ColumnFont"/> (celulele). Amândouă erau, fiecare pe jumătate, un
''' no-op — felul de proprietate pe care regula casei îl interzice:
'''
'''  • <c>HeaderFont</c> nu era citit de NIMENI. Pictorul rezolva un singur font pentru toată banda
'''    (<c>ResolvedHeaderFont</c>) și îl dădea fiecărei celule de antet; măsurarea la conținut și
'''    înălțimea benzii pe mai multe linii îl citeau pe același. «Calibri 9 bold» scris pe cinci
'''    coloane în RezervariView nu schimba nimic pe ecran.
'''  • <c>ColumnFont</c> era citit la PICTARE, dar nu și la MĂSURARE: coloana se scria cu un font și
'''    se măsura cu altul, deci una cu font propriu mai mare își tăia valorile cu elipsă taman în
'''    trecerea care exista ca să le facă loc.
'''
''' Amândouă răspund acum din același loc — <c>HeaderFontFor</c> / <c>CellFontFor</c> — și amândouă
''' au implicitul «gol = din grilă», cu perechea ShouldSerialize/Reset care ține designerul să nu
''' înghețe fontul rezolvat în formularul-gazdă.
''' </summary>
Public Class KBotDataViewColumnFontTests

    ' Destul de mare cât diferența să nu poată fi rotunjire de măsurare.
    Private Shared Function FontMare() As Font
        Return New Font("Segoe UI", 24.0F, FontStyle.Bold)
    End Function

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(900, 300)
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.ToContent
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Return dv
    End Function

    ' ── Antetul: fontul coloanei intră în măsurarea la conținut ──────────────────

    <Fact>
    Public Sub AColumnHeaderFont_WidensThatColumn_WhenSizingToContent()
        Using dv = Grid()
            Dim normala = dv.AddColumn("a", "Clasificația", KBotColumnType.Text, 40)
            Dim mare = dv.AddColumn("b", "Clasificația", KBotColumnType.Text, 40)
            Using f As Font = FontMare()
                mare.HeaderFont = f
                dv.AutoSizeColumns()

                ' Același titlu, aceleași celule (niciuna): singura diferență e fontul de antet.
                Assert.True(mare.Width > normala.Width,
                            $"coloana cu font propriu s-a măsurat la {mare.Width}px, la fel ca cea din bandă ({normala.Width}px)")
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub AColumnHeaderFont_RaisesTheMultilineBand_ByItself()
        Using dv As New KBotDataView()
            dv.Size = New Size(600, 300)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None      ' lățimea afirmată rămâne afirmată
            dv.ApplyTheme(BuiltInSchemes.Classic())

            Dim col = dv.AddColumn("a", "Rezervare Definitivă Totală", KBotColumnType.Text, 200)
            col.MultiLine = True
            Dim cuFontulBenzii As Integer = dv.EffectiveHeaderHeight()

            Using f As Font = FontMare()
                col.HeaderFont = f
                ' Fontul e și el o intrare în înălțimea benzii, nu doar lățimea coloanei: scris mai
                ' mare, titlul cere mai multe rânduri la aceeași lățime.
                Assert.True(dv.EffectiveHeaderHeight() > cuFontulBenzii,
                            $"banda a rămas la {dv.EffectiveHeaderHeight()}px, deși titlul se scrie cu un font mult mai mare")
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub TheColumnFont_WinsOverTheBandFont_AndOnlyForThatColumn()
        Using dv = Grid()
            Dim aLui = dv.AddColumn("a", "Antet", KBotColumnType.Text, 100)
            Dim aBenzii = dv.AddColumn("b", "Antet", KBotColumnType.Text, 100)
            Using f As Font = FontMare()
                aLui.HeaderFont = f

                Assert.Same(f, dv.HeaderFontFor(aLui))
                Assert.Same(dv.ResolvedHeaderFont(), dv.HeaderFontFor(aBenzii))

                ' Golită la loc, coloana se întoarce la bandă — «gol» chiar înseamnă «din grilă».
                aLui.HeaderFont = Nothing
                Assert.Same(dv.ResolvedHeaderFont(), dv.HeaderFontFor(aLui))
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Fontul fixat pe GRILĂ rămâne ce se folosește acolo unde coloana n-a cerut nimic — cele două
    ''' proprietăți sunt trepte, nu alternative.
    ''' </summary>
    <Fact>
    Public Sub TheGridHeaderFont_StillAnswersForAColumnThatAsksForNothing()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 100)
            Using banda As Font = New Font("Consolas", 11.0F, FontStyle.Bold)
                dv.HeaderFont = banda
                Assert.Same(banda, dv.HeaderFontFor(col))

                Using alLui As Font = FontMare()
                    col.HeaderFont = alLui
                    Assert.Same(alLui, dv.HeaderFontFor(col))
                End Using
            End Using
        End Using
    End Sub

    ' ── Celulele: același lucru, pe cealaltă față a coloanei ─────────────────────

    <Fact>
    Public Sub AColumnCellFont_WidensThatColumn_WhenSizingToContent()
        Using dv = Grid()
            Dim normala = dv.AddColumn("a", "", KBotColumnType.Text, 40)
            Dim mare = dv.AddColumn("b", "", KBotColumnType.Text, 40)
            Dim r = dv.AddRow()
            r.Item("a") = "1.234.567,89"
            r.Item("b") = "1.234.567,89"

            Using f As Font = FontMare()
                mare.ColumnFont = f
                dv.AutoSizeColumns()

                ' Aceeași valoare în amândouă: singura diferență e fontul cu care se scrie. Măsurată
                ' cu fontul grilei, coloana mare ieșea îngustă și își tăia cifrele cu elipsă.
                Assert.True(mare.Width > normala.Width,
                            $"coloana cu font propriu s-a măsurat la {mare.Width}px, la fel ca cea cu fontul grilei ({normala.Width}px)")
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub TheCellFont_FallsBackToTheGridFont()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 100)
            Assert.Same(dv.Font, dv.CellFontFor(col))

            Using f As Font = FontMare()
                col.ColumnFont = f
                Assert.Same(f, dv.CellFontFor(col))

                col.ColumnFont = Nothing
                Assert.Same(dv.Font, dv.CellFontFor(col))
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Eticheta de depășire se aprinde pentru ce NU încape, iar «cât încape» depinde de fontul cu
    ''' care se scrie celula: sonda ei trebuie să citească exact fontul pictorului.
    ''' </summary>
    <Fact>
    Public Sub TheOverflowTooltip_MeasuresWithTheColumnFont()
        Using dv As New KBotDataView()
            dv.Size = New Size(600, 300)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ApplyTheme(BuiltInSchemes.Classic())

            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 120)
            dv.AddRow().Item("a") = "Rezervare"
            Assert.Null(dv.CellTooltipTextFor("a", 0))          ' încape cu fontul grilei

            Using f As Font = FontMare()
                col.ColumnFont = f
                Assert.Equal("Rezervare", dv.CellTooltipTextFor("a", 0))
            End Using
        End Using
    End Sub

    ' ── Suprafața de designer ────────────────────────────────────────────────────

    ''' <summary>
    ''' O coloană proaspăt creată nu scrie NICIUN font în formularul-gazdă. <c>Font</c> nu poate
    ''' purta <c>DefaultValue</c>, deci fără perechea ShouldSerialize/Reset designerul îngheață
    ''' fontul rezolvat, iar linia aceea citește pe vecie ca alegerea operatorului și blochează tema.
    ''' Se întreabă prin <c>TypeDescriptor</c> — calea pe care merge chiar Visual Studio.
    ''' </summary>
    <Theory>
    <InlineData("HeaderFont")>
    <InlineData("ColumnFont")>
    Public Sub AFreshColumn_SerializesNoFont(nume As String)
        Dim col As New KBotDataColumn()
        Assert.False(TypeDescriptor.GetProperties(col)(nume).ShouldSerializeValue(col),
                     $"«{nume}» s-ar scrie în .Designer.vb pe o coloană pe care n-a atins-o nimeni")
    End Sub

    <Theory>
    <InlineData("HeaderFont")>
    <InlineData("ColumnFont")>
    Public Sub APinnedFont_IsSerialized_AndResetTakesItBack(nume As String)
        Dim col As New KBotDataColumn()
        Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(col)(nume)
        Using f As Font = FontMare()
            prop.SetValue(col, f)
            Assert.True(prop.ShouldSerializeValue(col))

            prop.ResetValue(col)
            Assert.Null(prop.GetValue(col))
            Assert.False(prop.ShouldSerializeValue(col))
        End Using
    End Sub

    ''' <summary>
    ''' Implicitul e GOL, nu un font construit în cod. Cât timp coloana își făcea singură un
    ''' «Calibri 9», ea bătea și banda, și tema, pe fiecare coloană, pentru totdeauna — iar
    ''' <c>HeaderFontFor</c>/<c>CellFontFor</c> n-ar fi avut niciodată pe ce cădea înapoi.
    ''' </summary>
    <Fact>
    Public Sub AFreshColumn_HasNoFontOfItsOwn()
        Dim col As New KBotDataColumn()
        Assert.Null(col.HeaderFont)
        Assert.Null(col.ColumnFont)
    End Sub

End Class
