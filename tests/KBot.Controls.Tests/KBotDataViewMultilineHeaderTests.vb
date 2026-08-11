Option Strict On
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Titlul de coloană scris pe MAI MULTE LINII (<see cref="KBotDataColumn.MultiLine"/>) și ce trage
''' el după el: banda de antet nu mai are o înălțime fixă, ci una MĂSURATĂ — urcă până încape
''' textul rupt și coboară la loc când coloana se lărgește.
'''
''' Trei lucruri se verifică aici, și al treilea e cel care a lipsit prima dată:
'''
'''  • banda crește (rupere între cuvinte ȘI rupturile scrise cu Enter);
'''  • banda SCADE când coloana se lărgește — altfel proprietatea ar fi o cricătoare cu clichet;
'''  • înălțimea măsurată e una de GEOMETRIE, nu de desen: rândurile încep sub ea, iar pictogramele
'''    de antet se apasă acolo unde sunt desenate. O primă variantă mărea doar dreptunghiul
'''    pictat, iar rândurile rămâneau să înceapă sub vechiul <c>HeaderHeight</c>, adică sub antet.
'''
''' Auto-dimensionarea e stinsă oriunde se afirmă o lățime, altfel trecerea «la conținut» ar
''' re-măsura coloana de sub afirmație.
''' </summary>
Public Class KBotDataViewMultilineHeaderTests

    ' Un titlu destul de lung cât să se rupă în mai multe rânduri la 90px.
    Private Const Titlu As String = "Rezervare Definitivă Totală Cumulată"

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 300)
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Return dv
    End Function

    ' ── Banda urcă ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub WithoutMultiLine_TheBandKeepsTheAuthoredHeight()
        Using dv = Grid()
            dv.AddColumn("a", Titlu, KBotColumnType.Text, 90)
            Assert.Equal(dv.HeaderHeight, dv.EffectiveHeaderHeight())
        End Using
    End Sub

    <Fact>
    Public Sub MultiLine_RaisesTheBandAboveTheAuthoredHeight()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", Titlu, KBotColumnType.Text, 90)
            col.MultiLine = True

            Assert.True(dv.EffectiveHeaderHeight() > dv.HeaderHeight,
                        $"banda a rămas la {dv.EffectiveHeaderHeight()}px, deși titlul se rupe în mai multe rânduri")
            ' HeaderHeight rămâne ce a cerut operatorul — banda nu se scrie niciodată pe ea însăși.
            Assert.Equal(30, dv.HeaderHeight)
        End Using
    End Sub

    <Fact>
    Public Sub AHardLineBreak_CountsEvenWhenTheTextWouldFitOnOneLine()
        Using dv = Grid()
            Dim fara = dv.AddColumn("a", "Credit Bugetar", KBotColumnType.Text, 400)
            fara.MultiLine = True
            Dim peUnRand As Integer = dv.EffectiveHeaderHeight()

            Dim cu = dv.AddColumn("b", "Credit" & Environment.NewLine & "Bugetar", KBotColumnType.Text, 400)
            cu.MultiLine = True

            ' Late de 400px, amândouă titlurile ar încăpea de trei ori pe un rând: singurul motiv
            ' pentru care banda urcă e ruptura scrisă cu Enter.
            Assert.True(dv.EffectiveHeaderHeight() > peUnRand,
                        "ruptura scrisă cu Enter n-a fost respectată")
        End Using
    End Sub

    ''' <summary>
    ''' Ruptura scrisă cu Enter se respectă și FĂRĂ <c>MultiLine</c> — asta e regula, nu o
    ''' îngăduință. <c>DrawText</c> o desenează oricum; cât timp banda nu creștea decât pentru
    ''' <c>MultiLine</c>, al doilea rând se picta sub linia de bază, adică dispărea cu totul.
    ''' </summary>
    <Fact>
    Public Sub AHardBreak_RaisesTheBand_EvenWithMultiLineOff()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Credit" & Environment.NewLine & "Bugetar", KBotColumnType.Text, 400)
            Assert.False(col.MultiLine)
            Assert.True(dv.EffectiveHeaderHeight() > dv.HeaderHeight,
                        "banda n-a făcut loc rândului al doilea, deci el se desena sub linia de bază")
        End Using
    End Sub

    ''' <summary>
    ''' … dar atât: fără <c>MultiLine</c>, un titlu lung NU se rupe singur între cuvinte, oricât
    ''' de îngustă ar fi coloana. Cele două lucruri sunt separate.
    ''' </summary>
    <Fact>
    Public Sub WithoutMultiLine_ALongCaptionStillDoesNotWrapOnItsOwn()
        Using dv = Grid()
            dv.AddColumn("a", Titlu, KBotColumnType.Text, 60)
            Assert.Equal(dv.HeaderHeight, dv.EffectiveHeaderHeight())
        End Using
    End Sub

    ' ── … și coboară la loc ──────────────────────────────────────────────────────

    <Fact>
    Public Sub WideningTheColumn_LowersTheBandBackDown()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", Titlu, KBotColumnType.Text, 90)
            col.MultiLine = True
            Dim ingusta As Integer = dv.EffectiveHeaderHeight()

            col.Width = 200
            Dim medie As Integer = dv.EffectiveHeaderHeight()
            Assert.True(medie < ingusta, $"banda n-a coborât la lărgire: {ingusta}px -> {medie}px")

            ' Lărgită de tot, tot titlul încape pe un rând și banda se întoarce la minimul cerut.
            col.Width = 500
            Assert.Equal(dv.HeaderHeight, dv.EffectiveHeaderHeight())
        End Using
    End Sub

    <Fact>
    Public Sub RetypingTheCaption_ReMeasuresTheBand()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", Titlu, KBotColumnType.Text, 90)
            col.MultiLine = True
            Assert.True(dv.EffectiveHeaderHeight() > dv.HeaderHeight)

            col.HeaderText = "Cod"
            Assert.Equal(dv.HeaderHeight, dv.EffectiveHeaderHeight())
        End Using
    End Sub

    ' ── Plafonul ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub MaxHeaderHeight_CapsTheBand()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", Titlu, KBotColumnType.Text, 90)
            col.MultiLine = True
            Assert.True(dv.EffectiveHeaderHeight() > 40)

            dv.MaxHeaderHeight = 40
            Assert.Equal(40, dv.EffectiveHeaderHeight())
        End Using
    End Sub

    <Fact>
    Public Sub MaxHeaderHeight_NeverCutsBelowTheAuthoredMinimum()
        Using dv = Grid()
            dv.HeaderHeight = 30
            Dim col = dv.AddColumn("a", Titlu, KBotColumnType.Text, 90)
            col.MultiLine = True

            ' Un plafon sub minim e o cerere imposibilă: minimul câștigă (podeaua bate plafonul,
            ' aceeași regulă ca la lățimea coloanei).
            dv.MaxHeaderHeight = 10
            Assert.Equal(30, dv.EffectiveHeaderHeight())
        End Using
    End Sub

    ' ── Înălțimea e de GEOMETRIE, nu de desen ────────────────────────────────────

    <Fact>
    Public Sub TheTallerBand_PushesTheRowsDown()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", Titlu, KBotColumnType.Text, 90)
            col.MultiLine = True
            dv.AddRow().Item("a") = "x"

            Dim banda As Integer = dv.EffectiveHeaderHeight()
            Assert.True(banda > dv.HeaderHeight)
            Assert.Equal(banda, dv.CellRectangle("a", 0).Top)
        End Using
    End Sub

    <Fact>
    Public Sub TheTallerBand_MovesTheHeaderIconWithIt()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", Titlu, KBotColumnType.Text, 200)
            col.HeaderRightIcon = New Bitmap(16, 16)
            col.MultiLine = True

            Dim banda As Integer = dv.EffectiveHeaderHeight()
            Assert.True(banda > dv.HeaderHeight)

            ' Pictograma stă centrată în banda EFECTIVĂ — desenul și hit-testul citesc amândouă
            ' aceeași înălțime, deci un test pe dreptunghi le acoperă pe amândouă.
            Dim r As Rectangle = dv.DebugHeaderRightIconRect("a")
            Assert.Equal((banda - 16) \ 2, r.Top)
        End Using
    End Sub

    ' ── Măsurarea la conținut ────────────────────────────────────────────────────

    <Fact>
    Public Sub ToContent_SizesAMultilineColumnToItsLongestWord_NotToTheWholeCaption()
        Using dv As New KBotDataView()
            dv.Size = New Size(900, 300)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.ToContent
            dv.ApplyTheme(BuiltInSchemes.Classic())

            Dim intreg = dv.AddColumn("a", Titlu, KBotColumnType.Text, 40)
            Dim rupt = dv.AddColumn("b", Titlu, KBotColumnType.Text, 40)
            rupt.MultiLine = True
            dv.AutoSizeColumns()

            ' Aceleași titluri, aceleași celule (niciuna): singura diferență e ruperea. Dacă
            ' trecerea ar măsura tot titlul și pe coloana ruptă, ea ar fi lărgită exact cât să nu
            ' mai fie nevoie de rupere — adică proprietatea n-ar face nimic.
            Assert.True(rupt.Width < intreg.Width,
                        $"coloana ruptă a fost măsurată la {rupt.Width}px, la fel de lată ca cea întreagă ({intreg.Width}px)")
        End Using
    End Sub

    ' ── Retragerea titlului față de cea a pictogramelor ──────────────────────────

    ''' <summary>
    ''' Titlul stă mai aproape de margine decât pictogramele. Erau o singură retragere (8px de
    ''' fiecare parte), iar antetul ieșea vizibil mai retras decât celulele din corp (6px) — și,
    ''' mai important, pierdea 16px din lățimea la care se rupe un titlu pe mai multe linii.
    ''' </summary>
    <Fact>
    Public Sub TheCaptionSitsCloserToTheEdgeThanAnIconWould()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            Dim cell As New Rectangle(0, 0, 200, 30)

            Dim l = KBotDataView.ComputeHeaderCellLayout(col, cell,
                                                         KBotDataColumn.HeaderIconPad,
                                                         KBotDataColumn.HeaderIconGap,
                                                         Nothing,
                                                         KBotDataColumn.HeaderTextPad)

            Assert.True(KBotDataColumn.HeaderTextPad < KBotDataColumn.HeaderIconPad)
            Assert.Equal(KBotDataColumn.HeaderTextPad, l.Text.Left)
            Assert.Equal(200 - 2 * KBotDataColumn.HeaderTextPad, l.Text.Width)
        End Using
    End Sub

    <Fact>
    Public Sub WhereAnIconSits_TheCaptionStopsAtTheIcon_NotAtItsOwnPadding()
        Using dv = Grid()
            Dim col = dv.AddColumn("a", "Antet", KBotColumnType.Text, 200)
            col.HeaderLeftIcon = New Bitmap(16, 16)

            Dim l = KBotDataView.ComputeHeaderCellLayout(col, New Rectangle(0, 0, 200, 30),
                                                         KBotDataColumn.HeaderIconPad,
                                                         KBotDataColumn.HeaderIconGap,
                                                         Nothing,
                                                         KBotDataColumn.HeaderTextPad)

            ' Pictograma nu s-a mișcat: retragerea mică e doar a titlului, și doar pe latura liberă.
            Assert.Equal(KBotDataColumn.HeaderIconPad, l.LeftIcon.Left)
            Assert.Equal(l.LeftIcon.Right + KBotDataColumn.HeaderIconGap, l.Text.Left)
            Assert.Equal(200 - KBotDataColumn.HeaderTextPad, l.Text.Right)
        End Using
    End Sub

    ' ── Suprafața de designer ────────────────────────────────────────────────────

    <Fact>
    Public Sub HeaderText_OffersTheMultilineEditorToThePropertyGrid()
        ' Se întreabă prin TypeDescriptor — calea pe care merge chiar Visual Studio. Fără editorul
        ' ăsta, grila de proprietăți dă un singur rând și Enter închide editarea, deci o ruptură
        ' scrisă de mână n-ar avea pe unde să intre în formular.
        Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(GetType(KBotDataColumn))(NameOf(KBotDataColumn.HeaderText))
        Assert.IsType(Of MultilineStringEditor)(prop.GetEditor(GetType(UITypeEditor)))
    End Sub

End Class
