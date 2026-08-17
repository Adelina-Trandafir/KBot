Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Contractul lui <see cref="KBotScrollBar"/>: semantica de interval (aceeași ca la bara nativă),
''' geometria cursorului, pașii cu evenimentul lor, culorile din temă vs. cele pinuite în designer
''' și curățenia la serializare.
'''
''' Ce NU pot dovedi testele astea: cum ARATĂ bara pe ecran. Culorile se verifică prin proprietăți,
''' nu prin pixeli — pictura rămâne o verificare vizuală, nerulată.
''' </summary>
Public Class KBotScrollBarTests

    ' Controlul e WinForms: se creează pe un fir STA, ca suitele surori.
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

    Private Shared Function BaraVerticala() As KBotScrollBar
        Dim b As New KBotScrollBar()
        b.Size = New Size(12, 200)
        b.ShowArrows = False   ' fără săgeți, șina = tot controlul: geometria e ușor de citit
        Return b
    End Function

    ' ── Interval ─────────────────────────────────────────────────────────────

    <Fact>
    Public Sub MaxValue_scade_cu_fereastra_vizibila()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.SetRange(0, 99, 10, 0)
                       ' Ca la ScrollBar-ul nativ: Maximum - LargeChange + 1.
                       Assert.Equal(90, b.MaxValue)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Value_se_prinde_in_interval_nu_arunca()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.SetRange(0, 99, 10, 0)
                       b.Value = 1000
                       Assert.Equal(90, b.Value)
                       b.Value = -5
                       Assert.Equal(0, b.Value)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Fereastra_cat_tot_continutul_inseamna_nimic_de_derulat()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.SetRange(0, 9, 10, 0)
                       Assert.False(b.IsScrollable)
                       Assert.Equal(Rectangle.Empty, b.ThumbBounds)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Micsorarea_ferestrei_trage_valoarea_inapoi_in_interval()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.SetRange(0, 99, 10, 90)
                       Assert.Equal(90, b.Value)
                       ' O fereastră mai mare înseamnă un capăt mai mic: valoarea trebuie să coboare.
                       b.LargeChange = 50
                       Assert.Equal(50, b.Value)
                   End Using
               End Sub)
    End Sub

    ' ── Geometrie ────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Cursorul_ocupa_fractia_ferestrei_din_sina()
        RunSta(Sub()
                   Using b As KBotScrollBar = BaraVerticala()
                       b.SetRange(0, 99, 50, 0)   ' jumătate din conținut e vizibilă
                       Assert.Equal(100, b.ThumbBounds.Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Cursorul_nu_scade_sub_lungimea_minima()
        RunSta(Sub()
                   Using b As KBotScrollBar = BaraVerticala()
                       b.MinimumThumbLength = 30
                       b.SetRange(0, 9999, 1, 0)  ' fracție infimă
                       Assert.Equal(30, b.ThumbBounds.Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub La_capat_cursorul_atinge_capatul_sinei()
        RunSta(Sub()
                   Using b As KBotScrollBar = BaraVerticala()
                       b.SetRange(0, 99, 10, 0)
                       Assert.Equal(b.TrackBounds.Y, b.ThumbBounds.Y)
                       b.Value = b.MaxValue
                       Assert.Equal(b.TrackBounds.Bottom, b.ThumbBounds.Bottom)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Sagetile_dispar_cand_banda_e_prea_scurta()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.ShowArrows = True
                       b.Size = New Size(12, 200)
                       Assert.Equal(12, b.TrackBounds.Y)          ' o săgeată sus, una jos
                       ' Sub trei grosimi nu mai încap: șina ia tot controlul.
                       b.Size = New Size(12, 20)
                       Assert.Equal(0, b.TrackBounds.Y)
                       Assert.Equal(20, b.TrackBounds.Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Bara_orizontala_isi_intinde_cursorul_pe_latime()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.Orientation = Orientation.Horizontal
                       b.ShowArrows = False
                       b.Size = New Size(200, 12)
                       b.SetRange(0, 99, 50, 0)
                       Assert.Equal(100, b.ThumbBounds.Width)
                       Assert.Equal(b.TrackBounds.X, b.ThumbBounds.X)
                   End Using
               End Sub)
    End Sub

    ' ── Pași și evenimente ───────────────────────────────────────────────────

    <Fact>
    Public Sub Pasul_mic_si_cel_mare_muta_si_anunta()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.SetRange(0, 99, 10, 0)
                       b.SmallChange = 2
                       Dim tipuri As New List(Of ScrollEventType)()
                       AddHandler b.Scroll, Sub(s, e) tipuri.Add(e.Type)

                       b.Pas(ScrollEventType.SmallIncrement)
                       Assert.Equal(2, b.Value)
                       b.Pas(ScrollEventType.LargeIncrement)
                       Assert.Equal(12, b.Value)
                       b.Pas(ScrollEventType.LargeDecrement)
                       Assert.Equal(2, b.Value)

                       Assert.Equal(3, tipuri.Count)
                       Assert.Equal(ScrollEventType.SmallIncrement, tipuri(0))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Pasul_care_nu_misca_nimic_nu_ridica_evenimentul()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.SetRange(0, 99, 10, 0)
                       Dim numar As Integer = 0
                       AddHandler b.Scroll, Sub(s, e) numar += 1
                       b.Pas(ScrollEventType.SmallDecrement)   ' e deja la 0
                       Assert.Equal(0, numar)
                       Assert.Equal(0, b.Value)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tragerea_cursorului_traduce_pozitia_in_valoare()
        RunSta(Sub()
                   Using b As KBotScrollBar = BaraVerticala()
                       b.SetRange(0, 99, 50, 0)   ' cursor de 100px pe o șină de 200 => cursă 100
                       ' Fără apucare (decalaj 0), capătul de sus al cursorului merge la 50 =>
                       ' jumătate din cursă => jumătate din interval.
                       b.MutaCursorLa(50)
                       Assert.Equal(25, b.Value)
                       b.MutaCursorLa(1000)
                       Assert.Equal(b.MaxValue, b.Value)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ValueChanged_vine_si_la_scrierea_programatica()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       b.SetRange(0, 99, 10, 0)
                       Dim numar As Integer = 0
                       AddHandler b.ValueChanged, Sub(s, e) numar += 1
                       b.Value = 5
                       b.Value = 5   ' aceeași valoare nu mai anunță nimic
                       Assert.Equal(1, numar)
                   End Using
               End Sub)
    End Sub

    ' ── Temă și designer ─────────────────────────────────────────────────────

    <Fact>
    Public Sub Tema_da_culorile_iar_cea_pusa_de_operator_castiga()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       Dim intunecata As ThemeScheme = BuiltInSchemes.Dark()
                       b.ApplyTheme(intunecata)
                       Assert.Equal(intunecata.Palette.AccentColor, b.ThumbHoverColor)

                       b.ThumbColor = Color.Red
                       b.ApplyTheme(intunecata)
                       Assert.Equal(Color.Red, b.ThumbColor)   ' tema nu suprascrie alegerea
                       b.ResetThumbColor()
                       Assert.NotEqual(Color.Red, b.ThumbColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub O_bara_proaspata_nu_are_nimic_de_serializat()
        RunSta(Sub()
                   Using b As New KBotScrollBar()
                       ' Drumul pe care îl ia Visual Studio, nu ShouldSerializeX chemat direct.
                       For Each nume As String In New String() {"TrackColor", "ThumbColor", "ThumbHoverColor",
                                                                "ArrowColor", "Font", "Size", "BackColor", "ForeColor"}
                           Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(b)(nume)
                           Assert.False(pd.ShouldSerializeValue(b), $"«{nume}» ar fi scris în .Designer.vb")
                       Next
                   End Using
               End Sub)
    End Sub

End Class
