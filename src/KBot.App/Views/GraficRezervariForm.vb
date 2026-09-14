Option Strict On
Imports System.Globalization
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' GRAFICUL REZERVĂRILOR (felia 0061-02) — fereastra pe care o deschide iconița din dreapta
''' antetului arborelui din <see cref="RezervariView"/>, la fel cum iconița din antetul recepțiilor
''' deschide editorul de legături.
'''
''' <para><b>Ce arată.</b> Aceleași cifre pe care le scrie arborele, desfăcute pe axa timpului:
''' fila «Evoluția» e suma alergătoare a operațiilor (cât era rezervat la fiecare zi cu mișcare),
''' fila «Pe luni» sunt chiar totalurile de pe folderele de lună. Nicio a treia cifră, care să nu
''' se poată regăsi în arbore — două suprafețe care spun sume diferite despre același angajament ar
''' pune operatorul să aleagă în ce să creadă.</para>
'''
''' <para><b>Datele intră o dată, prin constructor.</b> Rezervările s-au citit deja de vedere, iar
''' o a doua cerere de rețea de aici ar putea răspunde ALTCEVA decât scrie în arborele de sub
''' fereastră. Fereastra e de PRIVIT: nu scrie nimic, deci nu are ce împăca la închidere.</para>
'''
''' <para><b>Graficul e al ei</b>, nu împrumutat ca la <see cref="AsociereForm"/>: acolo suprafața
''' exista deja în formularul de lucru, cu tratatorii ei cu tot, aici nu există niciun grafic al
''' rezervărilor de împrumutat.</para>
''' </summary>
Public Class GraficRezervariForm

    ' Cheile filelor — aceleași două șiruri pe care le scrie designerul în `grafic.Tabs`.
    Private Const TAB_EVOLUTIE As String = "evolutie"
    Private Const TAB_LUNI As String = "luni"

    Private Const SERIA_EVOLUTIE As String = "cumulat"
    Private Const SERIA_LUNI As String = "luni"

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00). Același ca în
    ' `RezervariView`, ca sumele din grafic să se citească la fel cu cele din arbore.
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    ' Rândurile angajamentului, așa cum le-a încărcat vederea. Niciodată `Nothing`: o listă goală
    ' e un răspuns legitim (un angajament fără rezervări) și se vede ca atare, prin `EmptyText`.
    Private ReadOnly _rows As List(Of RezervareRow)

    Public Sub New(cod As String, rows As List(Of RezervareRow))
        InitializeComponent()
        _rows = If(rows, New List(Of RezervareRow)())
        If Not String.IsNullOrWhiteSpace(cod) Then
            capBar.Text = "K-BOT — Graficul rezervărilor · " & cod
            Text = capBar.Text
        End If
    End Sub

    Private Sub GraficRezervariForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            Reconstruieste()
        Catch ex As Exception
            ' Graniță de UI: se loghează și rămâne o fereastră goală, în loc să cadă deschiderea.
            GlobalErrorLog.Write("GraficRezervariForm.GraficRezervariForm_Load", ex)
        End Try
    End Sub

    Private Sub Grafic_TabSelected(tabKey As String) Handles grafic.TabSelected
        Try
            Reconstruieste()
        Catch ex As Exception
            GlobalErrorLog.Write("GraficRezervariForm.Grafic_TabSelected", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Culorile punctelor sunt VALORI copiate din paletă, nu legături: la schimbarea temei nimic
    ''' nu se întoarce să le corecteze, deci graficul se face din nou.
    ''' </summary>
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Reconstruieste()
        Catch ex As Exception
            GlobalErrorLog.Write("GraficRezervariForm.OnThemeChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Graficul se reface ÎNTREG, nu se peticește: cele două file desenează lucruri diferite din
    ''' aceleași rânduri, iar o serie rămasă de la fila de dinainte ar fi singurul lucru de pe
    ''' ecran care mai susține că e adevărat.
    ''' </summary>
    Private Sub Reconstruieste()
        grafic.BeginUpdate()
        Try
            grafic.ClearSeries()
            If _rows.Count = 0 Then
                grafic.EmptyText = "Angajamentul nu are rezervări."
                Return
            End If
            If String.Equals(grafic.SelectedTabKey, TAB_LUNI, StringComparison.Ordinal) Then
                ConstruiesteLuni()
            Else
                ConstruiesteEvolutia()
            End If
        Finally
            grafic.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Cât era rezervat, zi de zi: o treaptă pentru fiecare zi cu operații, la înălțimea sumei
    ''' tuturor operațiilor de până atunci.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>În trepte, nu în pantă.</b> O rezervare nu crește pe nesimțite între două zile cu
    ''' operații — ține cât ține și sare când vine operația următoare. Desenată în pantă, linia ar
    ''' spune despre fiecare zi dintre ele o sumă care n-a existat niciodată.</para>
    ''' <para><b>Se adună R_Valoare</b>, aceeași coloană din care se scriu totalurile lunilor pe
    ''' folderele arborelui — deci ultima treaptă e chiar suma acelor totaluri. Micșorările intră
    ''' cu semnul lor, deci linia poate și să coboare.</para>
    ''' </remarks>
    Private Sub ConstruiesteEvolutia()
        Dim zile = _rows.GroupBy(Function(r) r.DataRezervare.Date).OrderBy(Function(gp) gp.Key)

        Dim serie As KBotChartSeries = grafic.AddSeries(SERIA_EVOLUTIE, "Rezervat, cumulat")
        serie.Emphasis = True
        serie.FillArea = True
        serie.LineMode = KBotChartLineMode.Step

        Dim cumulat As Double = 0
        For Each zi In zile
            Dim aZilei As Double = zi.Sum(Function(r) r.RValoare)
            cumulat += aZilei
            Dim punct As KBotChartPoint = serie.AddPoint(zi.Key, cumulat)
            punct.TooltipHeader = zi.Key.ToString("dd.MM.yyyy", _roCulture)
            punct.TooltipText = $"Rezervat până aici: {Bani(cumulat)}" & vbCrLf &
                                $"În ziua asta: {Bani(aZilei)} din {zi.Count()} operații"
        Next
    End Sub

    ''' <summary>
    ''' Totalul fiecărei luni, luna cu luna — exact sumele scrise pe folderele de lună din arbore.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Nu cumulate.</b> Cealaltă filă e cea cumulată; asta răspunde la altă întrebare —
    ''' «în ce lună s-a rezervat mult» — iar două file care spun același lucru înseamnă că una
    ''' dintre ele n-avea de ce să existe.</para>
    ''' <para>Punctul unei luni stă în ZIUA ÎNTÂI a ei, nu la o zi cu operații: valoarea e a lunii
    ''' întregi, nu a vreunei zile din ea, iar așezarea pe prima zi ține punctele la distanțe
    ''' egale, cum sunt și lunile.</para>
    ''' <para><b>Lunile negative NU se înroșesc</b>, deși rândul lor din arbore se înroșește.
    ''' Încercat și scos: <c>PointColor</c> vopsește și SEGMENTUL CARE PLEACĂ din punct, deci o lună
    ''' negativă făcea roșie urcarea către luna următoare — adică spunea roșu exact despre partea
    ''' care creștea. Graficul are oricum axa: sub linia lui zero se vede fără nicio culoare.</para>
    ''' </remarks>
    Private Sub ConstruiesteLuni()
        Dim luni = _rows.GroupBy(Function(r) New With {Key .Y = r.DataRezervare.Year, Key .M = r.DataRezervare.Month}).
                         OrderBy(Function(gp) gp.Key.Y).ThenBy(Function(gp) gp.Key.M)

        Dim serie As KBotChartSeries = grafic.AddSeries(SERIA_LUNI, "Total pe lună")
        serie.LineMode = KBotChartLineMode.Straight

        For Each luna In luni
            Dim total As Double = luna.Sum(Function(r) r.RValoare)
            Dim moment As New Date(luna.Key.Y, luna.Key.M, 1)
            Dim punct As KBotChartPoint = serie.AddPoint(moment, total)
            punct.TooltipHeader = $"{NumeleLunii(luna.Key.M)} {luna.Key.Y}"
            punct.TooltipText = $"Total: {Bani(total)} din {luna.Count()} operații"
        Next
    End Sub

    Private Shared Function Bani(value As Double) As String
        Return value.ToString("N2", _roCulture)
    End Function

    ' Numele lunii în română (Ianuarie, Februarie…), cu prima literă mare.
    Private Shared Function NumeleLunii(month As Integer) As String
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return month.ToString(CultureInfo.InvariantCulture)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    Private Sub btnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub
End Class
