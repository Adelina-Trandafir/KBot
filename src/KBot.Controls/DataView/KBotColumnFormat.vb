Option Strict On
Imports System.Globalization

''' <summary>
''' TRADUCEREA formatelor numite Access (<see cref="KBotFormat"/>) în text — singurul loc unde
''' scrie ce înseamnă „Standard” sau „Short Date” (slice 0028-02). O citesc doi consumatori, ca
''' regula să nu ajungă să existe în două variante: pictarea celulelor
''' (<c>KBotDataView.FormatValue</c>) și agregatele din subsol (<c>KBotDataView.Footer</c>) —
''' altfel un total s-ar scrie altfel decât coloana de deasupra lui.
'''
''' <para><b>Coerciția.</b> Formatele numite CITESC valoarea în tipul cerut, nu doar o formatează:
''' o coloană de tip <see cref="KBotValueType.Text"/> care poartă numere („1234.5” ca șir) se
''' formatează „Standard” corect. Asta e diferența față de <see cref="KBotDataColumn.FormatString"/>,
''' care merge doar pe valori <see cref="IFormattable"/> — un format .NET dat de apelant descrie
''' un tip pe care apelantul îl cunoaște, pe când formatul numit descrie o INTENȚIE.</para>
'''
''' <para><b>Câte zecimale.</b> Formatele cu zecimale (Currency/Euro/Fixed/Standard/Percent/
''' Scientific) iau numărul din <see cref="KBotDataColumn.DecimalPlaces"/> când e fixat, altfel
''' 2 — implicitul Access. Așa cele două proprietăți nu se contrazic: una spune CÂTE zecimale,
''' cealaltă CUM arată numărul.</para>
''' </summary>
Public NotInheritable Class KBotColumnFormat

    Private Sub New()
    End Sub

    ''' <summary>Câte zecimale scrie un format Access care nu primește altă indicație.</summary>
    Public Const ZecimaleImplicite As Integer = 2

    ''' <summary>
    ''' Textul valorii după formatul NUMIT. Întoarce False când formatul nu se aplică — fie
    ''' pentru că nu e niciunul (<see cref="KBotFormat.None"/> / <see cref="KBotFormat.GeneralNumber"/>,
    ''' care înseamnă „așa cum e”), fie pentru că valoarea nu se poate citi în tipul cerut (un
    ''' text ne-numeric sub «Standard»). În ambele cazuri apelantul cade pe calea lui obișnuită,
    ''' NU pe un șir gol: o valoare care nu se potrivește cu formatul tot trebuie să se vadă.
    ''' </summary>
    Friend Shared Function TryFormat(value As Object, format As KBotFormat, decimale As Integer,
                                     ByRef text As String) As Boolean
        text = Nothing
        If value Is Nothing OrElse format = KBotFormat.None OrElse format = KBotFormat.GeneralNumber Then Return False

        Select Case format
            Case KBotFormat.YesNo, KBotFormat.TrueFalse, KBotFormat.OnOff
                text = TextLogic(KBotDataView.ToBool(value), format)
                Return True

            Case KBotFormat.GeneralDate
                ' Access scrie ora doar dacă valoarea chiar are una: pentru o dată curată,
                ' „00:00” e zgomot care arată ca o oră adevărată.
                Dim d As Date
                If Not KBotDataView.TryDate(value, d) Then Return False
                text = If(d.TimeOfDay = TimeSpan.Zero,
                          d.ToString("d", CultureInfo.CurrentCulture),
                          d.ToString("g", CultureInfo.CurrentCulture))
                Return True

            Case KBotFormat.LongDate, KBotFormat.MediumDate, KBotFormat.ShortDate,
                 KBotFormat.LongTime, KBotFormat.MediumTime, KBotFormat.ShortTime
                Dim d As Date
                If Not KBotDataView.TryDate(value, d) Then Return False
                text = d.ToString(NetFormat(format, decimale), CultureInfo.CurrentCulture)
                Return True

            Case Else
                Dim n As Double
                If Not KBotDataView.TryNumeric(value, n) Then Return False
                text = n.ToString(NetFormat(format, decimale), CultureInfo.CurrentCulture)
                Return True
        End Select
    End Function

    ''' <summary>
    ''' Formatul .NET echivalent unui format numit. <c>Nothing</c> pentru cele care nu se pot
    ''' exprima ca format (None/GeneralNumber/GeneralDate și cele logice) — acelea trec prin
    ''' <see cref="TryFormat"/>. Public: e și documentația vie a echivalențelor.
    ''' </summary>
    Public Shared Function NetFormat(format As KBotFormat, decimale As Integer) As String
        Dim z As Integer = If(decimale >= 0, decimale, ZecimaleImplicite)
        Dim zStr As String = z.ToString(CultureInfo.InvariantCulture)

        Select Case format
            Case KBotFormat.Currency
                Return "C" & zStr
            Case KBotFormat.Euro
                ' Access impune simbolul €, indiferent de cultura mașinii; „.” și „,” rămân
                ' substituenți, deci separatorii tot ai culturii curente sunt.
                Return "€#,##0" & ZecimaleCustom(z)
            Case KBotFormat.Fixed
                Return "F" & zStr                       ' fără separatori de mii
            Case KBotFormat.Standard
                Return "N" & zStr                       ' cu separatori de mii
            Case KBotFormat.Percent
                Return "P" & zStr
            Case KBotFormat.Scientific
                Return "0" & ZecimaleCustom(z) & "E+00"
            Case KBotFormat.LongDate
                Return "D"
            Case KBotFormat.MediumDate
                Return "dd-MMM-yy"
            Case KBotFormat.ShortDate
                Return "d"
            Case KBotFormat.LongTime
                Return "T"
            Case KBotFormat.MediumTime
                Return "hh:mm tt"
            Case KBotFormat.ShortTime
                Return "HH:mm"
            Case Else
                Return Nothing
        End Select
    End Function

    ' Coada de zecimale a unui format personalizat: „.00” pentru 2, nimic pentru 0.
    Private Shared Function ZecimaleCustom(z As Integer) As String
        If z <= 0 Then Return String.Empty
        Return "." & New String("0"c, z)
    End Function

    ' Perechea de cuvinte a formatelor logice. Text de interfață => românesc.
    Private Shared Function TextLogic(value As Boolean, format As KBotFormat) As String
        Select Case format
            Case KBotFormat.TrueFalse
                Return If(value, "Adevărat", "Fals")
            Case KBotFormat.OnOff
                Return If(value, "Pornit", "Oprit")
            Case Else
                Return If(value, "Da", "Nu")
        End Select
    End Function

End Class
