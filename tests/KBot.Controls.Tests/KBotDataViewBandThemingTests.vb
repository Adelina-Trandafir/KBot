Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028: benzile (antet + subsol) se tematizează CU ADEVĂRAT.
'''
''' Până acum fontul antetului era scris în control («Segoe UI Semibold»), deci schimbarea schemei
''' nu ajungea la el, iar subsolul împrumuta sloturile antetului, deci nu se putea distinge de el
''' nici dacă schema voia. Testele de aici fixează ambele: fontul benzilor urmează
''' <c>Style.BaseFontName</c>/<c>BaseFontSize</c> ale schemei active, subsolul are culori proprii,
''' iar o valoare pusă de operator în designer bate tema — fără ca vreuna din proprietățile astea
''' să ajungă serializată când n-a atins-o nimeni (regula casei despre valorile rezolvate pe care
''' Visual Studio le îngheață în <c>.Designer.vb</c>).
''' </summary>
Public Class KBotDataViewBandThemingTests

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(500, 300)
        dv.AddColumn("cod", "Cod", KBotColumnType.Text, 120)
        dv.FooterVisible = True
        Return dv
    End Function

    ' ── Fontul benzilor vine din schemă ──────────────────────────────────────────

    ''' <summary>
    ''' Fontul benzii vine din SCHEMĂ, nu dintr-o familie scrisă în control.
    '''
    ''' <para>Testul nu mai compară două scheme built-in (felia 0052). Le comparaa fiindcă atunci
    ''' Modern cerea «Segoe UI Variable Text» iar Classic nu cerea nimic — dar tocmai acea
    ''' nepotrivire era defectul: fontul schemei se măsura altfel decât cel cu care se proiectase
    ''' fereastra, deci fiecare fereastră se redimensiona la comutarea temei. Acum toate patru
    ''' poartă același font, deci două built-in-uri nu mai pot proba nimic — ceea ce nu înseamnă
    ''' că proprietatea a dispărut, ci că trebuie probată pe o schemă care CHIAR cere altceva.</para>
    '''
    ''' <para>E și o probă mai bună decât cea veche: nu depinde de ce fonturi sunt instalate pe
    ''' mașina de test, și nu se strică data viitoare când cineva aliniază încă o schemă.</para>
    ''' </summary>
    <Fact>
    Public Sub BandFont_FollowsTheActiveSchemesFont_NotAHardcodedFamily()
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Modern())
            Dim implicit_ As Font = dv.ResolvedHeaderFont()

            ' Schemele sunt mutabile prin design (vezi BuiltInSchemes) — cerem o mărime pe care
            ' n-o poate produce nimic altceva.
            Dim alta As ThemeScheme = BuiltInSchemes.Modern()
            alta.Style.BaseFontSize = implicit_.Size + 5.0F
            dv.ApplyTheme(alta)
            Dim ceruta As Font = dv.ResolvedHeaderFont()

            Assert.True(Math.Abs(ceruta.Size - implicit_.Size) > 0.01F,
                        $"fontul benzii nu urmează schema: {implicit_.Name}/{implicit_.Size} vs {ceruta.Name}/{ceruta.Size}")
        End Using
    End Sub

    <Fact>
    Public Sub BandFont_IsHeavierThanTheBodyFont()
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Classic())
            Dim bandaFont As Font = dv.ResolvedHeaderFont()
            ' Fie familia semibold, fie stilul bold — dar niciodată fontul obișnuit al corpului.
            Dim maiGros As Boolean = bandaFont.Bold OrElse
                                     bandaFont.Name.IndexOf("Semibold", StringComparison.OrdinalIgnoreCase) >= 0
            Assert.True(maiGros, $"fontul benzii nu e mai gros decât corpul: {bandaFont.Name}")
        End Using
    End Sub

    <Fact>
    Public Sub FooterFont_TracksTheThemeToo()
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(dv.ResolvedHeaderFont().Name, dv.ResolvedFooterFont().Name)
            Assert.Equal(dv.ResolvedHeaderFont().Size, dv.ResolvedFooterFont().Size)
        End Using
    End Sub

    ' ── Subsolul are culori proprii ──────────────────────────────────────────────

    <Fact>
    Public Sub FooterBand_HasItsOwnColour_NotTheHeaders()
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Classic())
            Assert.NotEqual(dv.HeaderBackResolved(), dv.FooterBackResolved())
        End Using
    End Sub

    <Fact>
    Public Sub SwitchingTheScheme_MovesBothBands()
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Classic())
            Dim antetClasic As Color = dv.HeaderBackResolved()
            Dim subsolClasic As Color = dv.FooterBackResolved()

            dv.ApplyTheme(BuiltInSchemes.Dark())
            Assert.NotEqual(antetClasic, dv.HeaderBackResolved())
            Assert.NotEqual(subsolClasic, dv.FooterBackResolved())
        End Using
    End Sub

    ' ── Operatorul are ultimul cuvânt ────────────────────────────────────────────

    <Fact>
    Public Sub AColourSetByTheOperator_SurvivesASwitchBetweenLIGHTSchemes()
        ' Slice 0028-03: regula «cine a pus explicit o culoare câștigă» a rămas întreagă pe
        ' schemele LUMINOASE. Sub întuneric e răsturnată dinadins — vezi testele de mai jos.
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.FooterBackColor = Color.Firebrick
            dv.ApplyTheme(BuiltInSchemes.Modern())
            Assert.Equal(Color.Firebrick, dv.FooterBackResolved())

            ' Golirea o dă înapoi temei — «gol = din temă» merge în ambele sensuri.
            dv.FooterBackColor = Color.Empty
            Assert.NotEqual(Color.Firebrick, dv.FooterBackResolved())
        End Using
    End Sub

    ' ── …dar întunericul are ultimul cuvânt (slice 0028-03) ──────────────────────

    <Fact>
    Public Sub DarkScheme_IgnoresTheColoursSetInTheDesigner()
        ' Paleta de designer se autorează pe fundal deschis: o bandă lăsată roșu-cărămiziu peste
        ' un corp devenit aproape negru nu e „alegerea operatorului respectată”, e o grilă
        ' imposibil de citit. Sub întuneric, contrastul bate preferința.
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.HeaderBackColor = Color.Firebrick
            dv.HeaderForeColor = Color.Yellow
            dv.FooterBackColor = Color.Firebrick
            dv.FooterForeColor = Color.Yellow

            dv.ApplyTheme(BuiltInSchemes.Dark())
            Assert.True(dv.DarkOverridesDesignerColors)
            Assert.NotEqual(Color.Firebrick, dv.HeaderBackResolved())
            Assert.NotEqual(Color.Yellow, dv.HeaderForeResolved())
            Assert.NotEqual(Color.Firebrick, dv.FooterBackResolved())
            Assert.NotEqual(Color.Yellow, dv.FooterForeResolved())
        End Using
    End Sub

    <Fact>
    Public Sub LeavingTheDarkScheme_GivesTheDesignerColoursBack()
        ' Culorile nu se PIERD sub întuneric, doar se ignoră — altfel comutarea temei ar șterge
        ' definitiv ce a autorat operatorul, iar asta n-ar mai fi o schimbare de temă.
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.HeaderBackColor = Color.Firebrick

            dv.ApplyTheme(BuiltInSchemes.Dark())
            Assert.NotEqual(Color.Firebrick, dv.HeaderBackResolved())

            dv.ApplyTheme(BuiltInSchemes.Classic())
            Assert.False(dv.DarkOverridesDesignerColors)
            Assert.Equal(Color.Firebrick, dv.HeaderBackResolved())
            ' Proprietatea însăși n-a fost atinsă niciodată.
            Assert.Equal(Color.Firebrick, dv.HeaderBackColor)
        End Using
    End Sub

    <Fact>
    Public Sub DarkScheme_LeavesAPinnedFontAlone()
        ' Suprascrierea e doar despre CULORI: un font nu devine ilizibil pe fundal închis.
        Using dv = Grid()
            Using propriu As New Font("Consolas", 11.0F)
                dv.HeaderFont = propriu
                dv.ApplyTheme(BuiltInSchemes.Dark())
                Assert.Same(propriu, dv.ResolvedHeaderFont())
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub AFontSetByTheOperator_WinsOverTheScheme()
        Using dv = Grid()
            dv.ApplyTheme(BuiltInSchemes.Classic())
            Using propriu As New Font("Consolas", 11.0F)
                dv.HeaderFont = propriu
                dv.ApplyTheme(BuiltInSchemes.Modern())
                Assert.Same(propriu, dv.ResolvedHeaderFont())

                dv.HeaderFont = Nothing
                Assert.NotSame(propriu, dv.ResolvedHeaderFont())
            End Using
        End Using
    End Sub

    ' ── Nimic nu ajunge în .Designer.vb cât timp nu l-a atins nimeni ─────────────

    ' Verificarea se face prin TypeDescriptor — drumul pe care merge chiar Visual Studio.
    ' Un ShouldSerializeX chemat direct nu dovedește nimic (vezi regula casei).
    <Theory>
    <InlineData("HeaderBackColor")>
    <InlineData("HeaderForeColor")>
    <InlineData("HeaderFont")>
    <InlineData("FooterBackColor")>
    <InlineData("FooterForeColor")>
    <InlineData("FooterFont")>
    <InlineData("FooterHeight")>
    Public Sub UntouchedBandProperties_AreNotSerialized(numeProprietate As String)
        Using dv As New KBotDataView()
            dv.ApplyTheme(BuiltInSchemes.Modern())      ' tema le-a REZOLVAT, dar nu le-a fixat
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(dv)(numeProprietate)
            Assert.NotNull(prop)
            Assert.False(prop.ShouldSerializeValue(dv),
                         $"«{numeProprietate}» s-ar scrie în .Designer.vb fără ca operatorul s-o fi atins")
        End Using
    End Sub

    <Theory>
    <InlineData("HeaderBackColor")>
    <InlineData("FooterBackColor")>
    Public Sub AColourTheOperatorPinned_IsSerialized(numeProprietate As String)
        Using dv As New KBotDataView()
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(dv)(numeProprietate)
            prop.SetValue(dv, Color.Firebrick)
            Assert.True(prop.ShouldSerializeValue(dv))
            prop.ResetValue(dv)
            Assert.False(prop.ShouldSerializeValue(dv))
        End Using
    End Sub

    <Fact>
    Public Sub CollapseImages_AreNotSerializedWhileUnset()
        Using dv As New KBotDataView()
            For Each nume In {"CollapseExpandedImage", "CollapseCollapsedImage"}
                Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(dv)(nume)
                Assert.False(prop.ShouldSerializeValue(dv), $"«{nume}» s-ar serializa nesetată")
            Next
        End Using
    End Sub

End Class
