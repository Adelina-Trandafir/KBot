Option Strict On
Imports System.Globalization
Imports KBot.Common

''' <summary>
''' CALCULUL benzii de SUBSOL a <see cref="KBotDataView"/> (fostul „rând de totaluri”, slice
''' 0017-01; rebotezat și lărgit în 0028, ca să poarte același vocabular ca subsolul
''' arborelui: <c>FooterVisible</c> / <c>Footer*</c>).
'''
''' Agregatele se calculează peste rândurile care TREC DE FILTRE (<c>ViewRows</c>), nu peste tot
''' modelul — de la slice 0028-03, când grila a căpătat filtrare. Un subsol care ar aduna și
''' rândurile ascunse ar arăta un total ce nu se potrivește cu pagina de deasupra lui, adică, pentru
''' cine o citește, o greșeală de calcul. Rândurile doar DERULATE în afara ecranului se adună
''' normal: ele fac parte din pagină, doar că nu se văd acum.
'''
''' Textul formatat se ține în cache pe coloană și se recalculează doar când se schimbă modelul
''' (AddRow / ClearRows / EndUpdate / un commit de editare / o coloană adăugată / banda aprinsă /
''' un agregat schimbat / un filtru sau o sortare), deci pictarea nu re-agregă niciodată și o grilă
''' mare nu plătește per repictare. Geometria stă în <c>.Layout</c>, pictarea în <c>.Painting</c>.
'''
''' <para><b>Ce agregate are voie o coloană</b> ține de <see cref="KBotDataColumn.ValueType"/> și
''' e scris O SINGURĂ dată, în <see cref="KBotAggregateRules"/>. Aici se calculează, nu se
''' decide.</para>
''' </summary>
Partial Class KBotDataView

    ''' <summary>
    ''' Recalculează tot ce DERIVĂ din model: harta de vedere (sortare + filtrare) și textele
    ''' agregate din subsol. O cheamă fiecare schimbare de model — rânduri adăugate sau golite, o
    ''' valoare scrisă, un format schimbat.
    '''
    ''' <para>Harta se marchează murdară ÎNAINTEA gărzii de <c>BeginUpdate</c>, nu după: agregatele
    ''' se pot amâna până la <c>EndUpdate</c>, dar harta nu — o pictare căzută la mijlocul unei
    ''' încărcări în masă o citește, iar o hartă rămasă în urma rândurilor indexează în gol.
    ''' Reconstrucția propriu-zisă rămâne leneșă (<see cref="EnsureView"/>), deci marcajul e ieftin.</para>
    ''' </summary>
    Private Sub RecomputeDerived()
        Try
            InvalidateView()
            If _updateDepth <> 0 Then Return
            _footerText.Clear()
            If Not _showFooter Then Return
            For Each col In _columns
                If String.IsNullOrEmpty(col.Key) Then Continue For
                _footerText(col.Key) = ComputeAggregateText(col)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.RecomputeDerived", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Chemată de <see cref="KBotDataColumn.Aggregate"/> după o schimbare: banda trebuie să arate
    ''' imediat noul agregat, iar liniile verticale (care urmăresc «e agregată?») se mută și ele.
    ''' </summary>
    Friend Sub OnColumnAggregateChanged()
        Try
            If _initializing Then Return
            RecomputeDerived()
            LayoutChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnColumnAggregateChanged", ex)
        End Try
    End Sub

    ' Textul agregat cache-uit al unei coloane, calculat la cerere dacă nu e încă în cache
    ' (ex. o coloană citită înainte de primul recalcul). Folosit de pictor și de auto-size.
    Private Function FooterTextFor(col As KBotDataColumn) As String
        Dim t As String = Nothing
        If _footerText.TryGetValue(If(col.Key, String.Empty), t) Then Return t
        Return ComputeAggregateText(col)
    End Function

    ' Calculează + formatează agregatul unei coloane peste TOATE rândurile.
    Private Function ComputeAggregateText(col As KBotDataColumn) As String
        Select Case col.Aggregate
            Case KBotAggregate.Sum
                Dim s As Double = 0
                Dim d As Double
                For Each r In ViewRows()
                    ' Se adună valorile ROTUNJITE, adică exact cele afișate în coloană: un total
                    ' care nu iese la adunare pe ecran e, pentru cine citește pagina, o greșeală
                    ' de calcul — degeaba e „mai exact”.
                    If TryNumericRounded(r(col.Key), col, d) Then s += d
                Next
                Return FormatAggregate(col, s)

            Case KBotAggregate.Average
                Dim s As Double = 0
                Dim cnt As Integer = 0
                Dim d As Double
                For Each r In ViewRows()
                    If TryNumericRounded(r(col.Key), col, d) Then
                        s += d
                        cnt += 1
                    End If
                Next
                ' Nicio celulă numărabilă -> vid, niciodată 0 și niciodată NaN.
                If cnt = 0 Then Return String.Empty
                Return FormatAggregate(col, s / cnt)

            Case KBotAggregate.Min, KBotAggregate.Max
                Return ComputeExtremeText(col, col.Aggregate = KBotAggregate.Min)

            Case KBotAggregate.Count
                ' Numărul RÂNDURILOR care AU o valoare stocată pentru coloană (stare prezentă),
                ' NU numărul celulelor numerice ne-vide — un rând cu Nothing stocat tot se numără.
                Dim n As Integer = 0
                For Each r In ViewRows()
                    If r.HasValue(col.Key) Then n += 1
                Next
                Return FormatCount(n)

            Case KBotAggregate.CountDistinct
                ' Distincte pe TEXTUL AFIȘAT: aceeași cultură și același FormatString ca în corp,
                ' deci ce se vede la fel se și numără la fel (două DateTime cu ore diferite,
                ' afișate „dd.MM.yyyy”, sunt o singură zi — exact ce întreabă operatorul).
                Dim vazute As New HashSet(Of String)(StringComparer.CurrentCulture)
                For Each r In ViewRows()
                    Dim v As Object = r(col.Key)
                    If EsteGol(v) Then Continue For
                    vazute.Add(FormatValue(v, col))
                Next
                Return FormatCount(vazute.Count)

            Case KBotAggregate.CountEmpty
                Dim n As Integer = 0
                For Each r In ViewRows()
                    If EsteGol(r(col.Key)) Then n += 1
                Next
                Return FormatCount(n)

            Case KBotAggregate.CountTrue, KBotAggregate.CountFalse
                Dim caut As Boolean = (col.Aggregate = KBotAggregate.CountTrue)
                Dim n As Integer = 0
                For Each r In ViewRows()
                    ' Doar rândurile care AU valoare stocată: o celulă niciodată scrisă nu e nici
                    ' bifată, nici debifată — e absentă (o numără CountEmpty).
                    If Not r.HasValue(col.Key) Then Continue For
                    If ToBool(r(col.Key)) = caut Then n += 1
                Next
                Return FormatCount(n)

            Case KBotAggregate.First, KBotAggregate.Last
                Dim primul As Boolean = (col.Aggregate = KBotAggregate.First)
                If primul Then
                    For Each r In ViewRows()
                        Dim v As Object = r(col.Key)
                        If Not EsteGol(v) Then Return FormatValue(v, col)
                    Next
                Else
                    ' „Ultimul” înseamnă ultimul de pe ECRAN, deci se numără în ordinea de
                    ' afișare: sub o sortare, ultimul rând încărcat nu mai e cel de jos.
                    For i As Integer = ViewCount() - 1 To 0 Step -1
                        Dim v As Object = ViewRowAt(i)(col.Key)
                        If Not EsteGol(v) Then Return FormatValue(v, col)
                    Next
                End If
                Return String.Empty

            Case Else
                Return String.Empty
        End Select
    End Function

    ' Min/Max — comparate în tipul coloanei: numeric pentru Number, calendaristic pentru DateTime.
    ' Celulele care nu se pot citi în tipul cerut se sar (ca la Sum), iar zero candidați => vid.
    Private Function ComputeExtremeText(col As KBotDataColumn, cautMinim As Boolean) As String
        If col.ValueType = KBotValueType.DateTime Then
            Dim best As Date = Nothing
            Dim gasit As Boolean = False
            Dim d As Date
            For Each r In ViewRows()
                If Not TryDate(r(col.Key), d) Then Continue For
                If Not gasit OrElse If(cautMinim, d < best, d > best) Then
                    best = d
                    gasit = True
                End If
            Next
            If Not gasit Then Return String.Empty
            Return FormatAggregateDate(col, best)
        End If

        Dim bestN As Double = 0
        Dim gasitN As Boolean = False
        Dim n As Double
        For Each r In ViewRows()
            If Not TryNumericRounded(r(col.Key), col, n) Then Continue For
            If Not gasitN OrElse If(cautMinim, n < bestN, n > bestN) Then
                bestN = n
                gasitN = True
            End If
        Next
        If Not gasitN Then Return String.Empty
        Return FormatAggregate(col, bestN)
    End Function

    ' Numărătorile randează întotdeauna un întreg simplu (ignoră orice format string).
    Private Shared Function FormatCount(value As Integer) As String
        Return value.ToString(CultureInfo.CurrentCulture)
    End Function

    ' Formatează o valoare agregată numerică: AggregateFormatString dacă e setat, altfel
    ' FormatString-ul coloanei, altfel un ToString simplu — mereu în CurrentCulture, la fel ca
    ' FormatValue din corp.
    Private Shared Function FormatAggregate(col As KBotDataColumn, value As Double) As String
        ' Și rezultatul trece prin rotunjire: media a trei valori cu 2 zecimale are, ea însăși,
        ' o coadă de zecimale care n-are ce căuta sub o coloană de 2.
        Dim v As Double = value
        If col.HasDecimalPlaces Then
            v = Math.Round(v, col.DecimalPlaces, MidpointRounding.AwayFromZero)
        End If

        ' Formatul explicit al agregatului bate orice; altfel totalul se scrie exact ca valorile
        ' de deasupra lui — inclusiv prin formatul NUMIT al coloanei (slice 0028-02).
        If Not String.IsNullOrEmpty(col.AggregateFormatString) Then
            Return v.ToString(col.AggregateFormatString, CultureInfo.CurrentCulture)
        End If
        Dim numit As String = Nothing
        If KBotColumnFormat.TryFormat(v, col.Format, col.DecimalPlaces, numit) Then Return numit

        Dim fmt As String = FormatulAgregatului(col)
        If Not String.IsNullOrEmpty(fmt) Then Return v.ToString(fmt, CultureInfo.CurrentCulture)
        If col.HasDecimalPlaces Then
            Return v.ToString("F" & col.DecimalPlaces.ToString(CultureInfo.InvariantCulture),
                              CultureInfo.CurrentCulture)
        End If
        Return v.ToString(CultureInfo.CurrentCulture)
    End Function

    ' TryNumeric + rotunjirea de afișare a coloanei: agregatele lucrează pe valorile VĂZUTE.
    Private Shared Function TryNumericRounded(value As Object, col As KBotDataColumn,
                                              ByRef result As Double) As Boolean
        If Not TryNumeric(value, result) Then Return False
        If col.HasDecimalPlaces Then
            result = Math.Round(result, col.DecimalPlaces, MidpointRounding.AwayFromZero)
        End If
        Return True
    End Function

    Private Shared Function FormatAggregateDate(col As KBotDataColumn, value As Date) As String
        If Not String.IsNullOrEmpty(col.AggregateFormatString) Then
            Return value.ToString(col.AggregateFormatString, CultureInfo.CurrentCulture)
        End If
        Dim numit As String = Nothing
        If KBotColumnFormat.TryFormat(value, col.Format, col.DecimalPlaces, numit) Then Return numit

        Dim fmt As String = FormatulAgregatului(col)
        If String.IsNullOrEmpty(fmt) Then Return value.ToString(CultureInfo.CurrentCulture)
        Return value.ToString(fmt, CultureInfo.CurrentCulture)
    End Function

    Private Shared Function FormatulAgregatului(col As KBotDataColumn) As String
        Return If(Not String.IsNullOrEmpty(col.AggregateFormatString),
                  col.AggregateFormatString, col.FormatString)
    End Function

    ' „Gol” pentru numărători: fără valoare stocată, Nothing, sau text numai din spații.
    Private Shared Function EsteGol(value As Object) As Boolean
        If value Is Nothing Then Return True
        Dim s As String = TryCast(value, String)
        Return s IsNot Nothing AndAlso String.IsNullOrWhiteSpace(s)
    End Function

    ' Coerciție numerică pentru Sum/Average/Min/Max: primitivele numerice contează; un ȘIR
    ' numeric se parsează; orice altceva (Nothing, Boolean, un șir ne-numeric, un obiect) se sare.
    ' Întoarce dacă valoarea a CONTRIBUIT, ca «sărită» și «a contribuit cu 0» să rămână distincte.
    ''' <summary>Friend: o reia și <see cref="KBotColumnFormat"/> — formatele numite CITESC
    ''' valoarea, iar „ce se poate citi ca număr” trebuie să fie o singură regulă.</summary>
    Friend Shared Function TryNumeric(value As Object, ByRef result As Double) As Boolean
        result = 0
        If value Is Nothing Then Return False
        Select Case True
            Case TypeOf value Is Double, TypeOf value Is Single, TypeOf value Is Decimal,
                 TypeOf value Is Integer, TypeOf value Is Long, TypeOf value Is Short,
                 TypeOf value Is Byte
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                Return True
        End Select
        Dim s As String = TryCast(value, String)
        If s IsNot Nothing Then
            Return Double.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, result)
        End If
        Return False
    End Function

    ' Perechea calendaristică a lui TryNumeric (Min/Max pe coloane DateTime). Friend din același
    ' motiv ca ea: formatele numite calendaristice o refolosesc.
    Friend Shared Function TryDate(value As Object, ByRef result As Date) As Boolean
        result = Nothing
        If value Is Nothing Then Return False
        If TypeOf value Is Date Then
            result = CDate(value)
            Return True
        End If
        Dim s As String = TryCast(value, String)
        If s Is Nothing Then Return False
        Return Date.TryParse(s.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, result)
    End Function

    ''' <summary>
    ''' Poartă de verificare Friend pentru teste: textul agregat cache-uit al unei coloane
    ''' (headless — fără pictare). Șir vid când subsolul e stins sau coloana n-are agregat.
    ''' </summary>
    Friend Function DebugFooterText(colKey As String) As String
        Dim t As String = Nothing
        If _footerText.TryGetValue(colKey, t) Then Return If(t, String.Empty)
        Return String.Empty
    End Function

End Class
