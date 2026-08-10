Option Strict On
Imports System.Globalization

''' <summary>
''' MOTORUL de sortare și filtrare al <see cref="KBotDataView"/> (slice 0028-03) — pur, static și
''' fără nicio referință la pictare sau la vreun control. Aici scrie ce înseamnă „mai mic”, ce
''' înseamnă „conține” și ce condiții are voie să ofere o coloană, o singură dată: meniul, dialogul
''' de condiție și potrivirea propriu-zisă citesc toate din aceleași funcții, deci oferta nu poate
''' ajunge să difere de ce se acceptă.
'''
''' <para><b>Două feluri de valoare, dinadins.</b> Condițiile de TEXT (Conține, Începe cu…) se
''' potrivesc pe textul AFIȘAT, iar cele de mărime (Mai mic, Între…) pe valoarea BRUTĂ, citită în
''' tipul coloanei. Așa filtrul spune ce vede operatorul: o coloană de sume afișată cu două
''' zecimale se caută cu «1.234,50», dar se compară numeric — nu alfabetic, unde «9» ar fi mai mare
''' decât «10».</para>
'''
''' <para><b>Ordinea valorilor care nu se citesc în tip.</b> O coloană numerică poate purta și un
''' text ne-numeric (o notă, o liniuță). Regula: cele care se citesc în tip se compară între ele
''' în tip, cele care nu se citesc se compară între ele ca text, iar o valoare citibilă e mereu
''' ÎNAINTEA uneia necitibile. Alternativa — să le declarăm egale — ar face sortarea instabilă
''' tocmai pe rândurile ciudate, adică exact pe cele pe care le caută cineva.</para>
'''
''' <para><b>Goalele întâi.</b> <c>Nothing</c> și textul vid sunt același lucru (o celulă
''' necompletată) și stau primele la sortarea crescătoare, ca în Access.</para>
''' </summary>
Public NotInheritable Class KBotFilterEngine

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Cheia cu care celulele goale intră în lista de valori bifabile. E textul vid, nu o
    ''' etichetă: eticheta («(Necompletate)») e treaba interfeței, iar dacă ea ar ajunge în model,
    ''' o coloană care chiar conține textul «(Necompletate)» s-ar filtra singură împreună cu goalele.
    ''' </summary>
    Public Const CheieGol As String = ""

    ' ══════════════════════════════════════════════════════════════════════════
    ' OFERTA DE CONDIȚII (ce vede operatorul în submeniu, pe fiecare tip)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Condițiile pe care le poate purta o coloană de tipul dat, în ordinea în care Access le
    ''' înșiră în submeniu. Lista e și oferta meniului, și regula de validare.
    ''' </summary>
    Public Shared Function AllowedOperators(valueType As KBotValueType) As KBotFilterOperator()
        Select Case valueType
            Case KBotValueType.Number
                Return New KBotFilterOperator() {
                    KBotFilterOperator.Equals, KBotFilterOperator.NotEquals,
                    KBotFilterOperator.LessThan, KBotFilterOperator.GreaterThan,
                    KBotFilterOperator.Between,
                    KBotFilterOperator.IsEmpty, KBotFilterOperator.IsNotEmpty}

            Case KBotValueType.DateTime
                Return New KBotFilterOperator() {
                    KBotFilterOperator.Equals, KBotFilterOperator.NotEquals,
                    KBotFilterOperator.LessThan, KBotFilterOperator.GreaterThan,
                    KBotFilterOperator.Between,
                    KBotFilterOperator.IsEmpty, KBotFilterOperator.IsNotEmpty}

            Case KBotValueType.Boolean
                ' O bifă are două valori: lista de bifat le acoperă pe amândouă, deci o condiție
                ' în plus n-ar spune nimic ce nu se poate spune deja cu două căsuțe.
                Return Array.Empty(Of KBotFilterOperator)()

            Case Else
                Return New KBotFilterOperator() {
                    KBotFilterOperator.Equals, KBotFilterOperator.NotEquals,
                    KBotFilterOperator.BeginsWith, KBotFilterOperator.NotBeginsWith,
                    KBotFilterOperator.EndsWith, KBotFilterOperator.NotEndsWith,
                    KBotFilterOperator.Contains, KBotFilterOperator.NotContains,
                    KBotFilterOperator.IsEmpty, KBotFilterOperator.IsNotEmpty}
        End Select
    End Function

    ''' <summary>Condiția e permisă pe tipul dat? (<see cref="KBotFilterOperator.None"/> mereu da.)</summary>
    Public Shared Function IsAllowed(valueType As KBotValueType, op As KBotFilterOperator) As Boolean
        If op = KBotFilterOperator.None Then Return True
        Return Array.IndexOf(AllowedOperators(valueType), op) >= 0
    End Function

    ''' <summary>
    ''' Câți operanzi cere condiția: 0 (goală / necompletată), 1 (cele obișnuite) sau 2
    ''' (<see cref="KBotFilterOperator.Between"/>). Îl citește dialogul de condiție, ca să știe
    ''' câte casete să arate.
    ''' </summary>
    Public Shared Function OperandCount(op As KBotFilterOperator) As Integer
        Select Case op
            Case KBotFilterOperator.None, KBotFilterOperator.IsEmpty, KBotFilterOperator.IsNotEmpty
                Return 0
            Case KBotFilterOperator.Between
                Return 2
            Case Else
                Return 1
        End Select
    End Function

    ''' <summary>
    ''' Numele condiției în meniu. Depinde de tip acolo unde Access îl schimbă: pe date «Mai mic
    ''' decât» se citește «Înainte de», fiindcă despre o dată nimeni nu spune că e mai mică.
    ''' </summary>
    Public Shared Function OperatorCaption(op As KBotFilterOperator, valueType As KBotValueType) As String
        Select Case op
            Case KBotFilterOperator.Equals
                Return "Egal cu…"
            Case KBotFilterOperator.NotEquals
                Return "Diferit de…"
            Case KBotFilterOperator.Contains
                Return "Conține…"
            Case KBotFilterOperator.NotContains
                Return "Nu conține…"
            Case KBotFilterOperator.BeginsWith
                Return "Începe cu…"
            Case KBotFilterOperator.NotBeginsWith
                Return "Nu începe cu…"
            Case KBotFilterOperator.EndsWith
                Return "Se termină cu…"
            Case KBotFilterOperator.NotEndsWith
                Return "Nu se termină cu…"
            Case KBotFilterOperator.LessThan
                Return If(valueType = KBotValueType.DateTime, "Înainte de…", "Mai mic decât…")
            Case KBotFilterOperator.GreaterThan
                Return If(valueType = KBotValueType.DateTime, "După…", "Mai mare decât…")
            Case KBotFilterOperator.Between
                Return "Între…"
            Case KBotFilterOperator.IsEmpty
                Return "Este necompletat"
            Case KBotFilterOperator.IsNotEmpty
                Return "Este completat"
            Case Else
                Return String.Empty
        End Select
    End Function

    ''' <summary>Titlul submeniului de condiții, în vocabularul tipului («Filtre text» / «numerice» / «de dată»).</summary>
    Public Shared Function ConditionMenuCaption(valueType As KBotValueType) As String
        Select Case valueType
            Case KBotValueType.Number
                Return "Filtre numerice"
            Case KBotValueType.DateTime
                Return "Filtre de dată"
            Case Else
                Return "Filtre text"
        End Select
    End Function

    ''' <summary>
    ''' Numele unei sortări în meniu, în vocabularul tipului: Access scrie «A → Z» pe text, «cel mai
    ''' mic → cel mai mare» pe numere și «cel mai vechi → cel mai nou» pe date.
    ''' </summary>
    Public Shared Function SortCaption(valueType As KBotValueType, direction As KBotSortDirection) As String
        Dim crescator As Boolean = (direction <> KBotSortDirection.Descending)
        Select Case valueType
            Case KBotValueType.Number
                Return If(crescator, "Sortare de la mic la mare", "Sortare de la mare la mic")
            Case KBotValueType.DateTime
                Return If(crescator, "Sortare de la vechi la nou", "Sortare de la nou la vechi")
            Case KBotValueType.Boolean
                Return If(crescator, "Sortare nebifate → bifate", "Sortare bifate → nebifate")
            Case Else
                Return If(crescator, "Sortare de la A la Z", "Sortare de la Z la A")
        End Select
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' COMPARAȚIE (sortarea)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Compară două valori de celulă în tipul coloanei: &lt;0 dacă <paramref name="a"/> e înainte,
    ''' 0 la egalitate, &gt;0 dacă e după. Goalele stau primele; o valoare care se citește în tip e
    ''' înaintea uneia care nu se citește (vezi comentariul clasei).
    ''' </summary>
    Public Shared Function Compare(a As Object, b As Object, valueType As KBotValueType) As Integer
        Dim golA As Boolean = IsBlank(a)
        Dim golB As Boolean = IsBlank(b)
        If golA AndAlso golB Then Return 0
        If golA Then Return -1
        If golB Then Return 1

        Select Case valueType
            Case KBotValueType.Number
                Dim na, nb As Double
                Dim okA As Boolean = KBotDataView.TryNumeric(a, na)
                Dim okB As Boolean = KBotDataView.TryNumeric(b, nb)
                If okA AndAlso okB Then Return na.CompareTo(nb)
                If okA <> okB Then Return If(okA, -1, 1)

            Case KBotValueType.DateTime
                Dim da, db As Date
                Dim okA As Boolean = KBotDataView.TryDate(a, da)
                Dim okB As Boolean = KBotDataView.TryDate(b, db)
                If okA AndAlso okB Then Return da.CompareTo(db)
                If okA <> okB Then Return If(okA, -1, 1)

            Case KBotValueType.Boolean
                ' Nebifat înaintea bifatului, ca în Access (False = 0, True = -1 la ei, dar
                ' ordinea afișată e tot nebifate întâi).
                Dim ba As Boolean = KBotDataView.ToBool(a)
                Dim bb As Boolean = KBotDataView.ToBool(b)
                If ba = bb Then Return 0
                Return If(ba, 1, -1)
        End Select

        ' Text — și plasa de siguranță pentru perechile care nu s-au citit în tipul cerut.
        Return String.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCultureIgnoreCase)
    End Function

    ''' <summary>Celulă necompletată: <c>Nothing</c> sau text format doar din spații.</summary>
    Public Shared Function IsBlank(value As Object) As Boolean
        If value Is Nothing Then Return True
        Return String.IsNullOrWhiteSpace(value.ToString())
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' POTRIVIRE (condiția)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Valoarea trece de condiție? <paramref name="displayText"/> e textul AFIȘAT al celulei (pe
    ''' el lucrează condițiile de text, ca operatorul să caute ce vede), iar
    ''' <paramref name="rawValue"/> e valoarea brută (pe ea lucrează cele de mărime).
    '''
    ''' <para>Un operand care nu se poate citi în tipul coloanei nu ELIMINĂ rândurile: condiția
    ''' devine inertă și întoarce True. Alternativa — să nu treacă nimic — ar arăta ca o grilă
    ''' golită de un bug, în loc de un filtru care n-a înțeles ce i s-a scris.</para>
    ''' </summary>
    Public Shared Function MatchesCondition(rawValue As Object, displayText As String,
                                            valueType As KBotValueType, op As KBotFilterOperator,
                                            operand1 As String, operand2 As String) As Boolean
        Select Case op
            Case KBotFilterOperator.None
                Return True
            Case KBotFilterOperator.IsEmpty
                Return IsBlank(rawValue)
            Case KBotFilterOperator.IsNotEmpty
                Return Not IsBlank(rawValue)
        End Select

        Dim text As String = If(displayText, String.Empty)
        Dim tinta As String = If(operand1, String.Empty)

        Select Case op
            Case KBotFilterOperator.Contains
                Return text.IndexOf(tinta, StringComparison.CurrentCultureIgnoreCase) >= 0
            Case KBotFilterOperator.NotContains
                Return text.IndexOf(tinta, StringComparison.CurrentCultureIgnoreCase) < 0
            Case KBotFilterOperator.BeginsWith
                Return text.StartsWith(tinta, StringComparison.CurrentCultureIgnoreCase)
            Case KBotFilterOperator.NotBeginsWith
                Return Not text.StartsWith(tinta, StringComparison.CurrentCultureIgnoreCase)
            Case KBotFilterOperator.EndsWith
                Return text.EndsWith(tinta, StringComparison.CurrentCultureIgnoreCase)
            Case KBotFilterOperator.NotEndsWith
                Return Not text.EndsWith(tinta, StringComparison.CurrentCultureIgnoreCase)
        End Select

        ' Condițiile de MĂRIME: pe numere și date se compară în tip, pe text alfabetic.
        Select Case op
            Case KBotFilterOperator.Equals
                If valueType = KBotValueType.Text Then
                    Return String.Equals(text, tinta, StringComparison.CurrentCultureIgnoreCase)
                End If
                Return CompareToOperand(rawValue, tinta, valueType, 0, True)

            Case KBotFilterOperator.NotEquals
                If valueType = KBotValueType.Text Then
                    Return Not String.Equals(text, tinta, StringComparison.CurrentCultureIgnoreCase)
                End If
                Return CompareToOperand(rawValue, tinta, valueType, 0, False)

            Case KBotFilterOperator.LessThan
                Return CompareToOperandSign(rawValue, tinta, valueType, -1)

            Case KBotFilterOperator.GreaterThan
                Return CompareToOperandSign(rawValue, tinta, valueType, 1)

            Case KBotFilterOperator.Between
                Dim jos As Object = CoerceOperand(operand1, valueType)
                Dim sus As Object = CoerceOperand(operand2, valueType)
                If jos Is Nothing OrElse sus Is Nothing Then Return True      ' operand ilizibil => inert
                ' Capetele date invers tot un interval descriu — nu e o greșeală de corectat, e
                ' una de citit în ordinea în care are sens.
                If Compare(jos, sus, valueType) > 0 Then
                    Dim tmp As Object = jos
                    jos = sus
                    sus = tmp
                End If
                If IsBlank(rawValue) Then Return False
                Return Compare(rawValue, jos, valueType) >= 0 AndAlso Compare(rawValue, sus, valueType) <= 0
        End Select

        Return True
    End Function

    ' Egalitate/neegalitate față de un operand citit în tip. «asteptat» = ce trebuie să dea
    ' comparația, «potrivireCandDa» = ce întoarcem când chiar dă asta.
    Private Shared Function CompareToOperand(rawValue As Object, operand As String,
                                             valueType As KBotValueType, asteptat As Integer,
                                             potrivireCandDa As Boolean) As Boolean
        Dim tinta As Object = CoerceOperand(operand, valueType)
        If tinta Is Nothing Then Return True                                  ' operand ilizibil => inert
        If IsBlank(rawValue) Then Return Not potrivireCandDa
        Dim semn As Integer = Compare(rawValue, tinta, valueType)
        Return If(semn = asteptat, potrivireCandDa, Not potrivireCandDa)
    End Function

    ' Strict mai mic (semn = -1) sau strict mai mare (semn = 1) decât operandul.
    Private Shared Function CompareToOperandSign(rawValue As Object, operand As String,
                                                 valueType As KBotValueType, semnCerut As Integer) As Boolean
        Dim tinta As Object = CoerceOperand(operand, valueType)
        If tinta Is Nothing Then Return True                                  ' operand ilizibil => inert
        If IsBlank(rawValue) Then Return False                                ' goalele nu sunt nici sub, nici peste
        Dim semn As Integer = Compare(rawValue, tinta, valueType)
        Return If(semnCerut < 0, semn < 0, semn > 0)
    End Function

    ''' <summary>
    ''' Operandul tastat, citit în tipul coloanei. <c>Nothing</c> = nu s-a putut citi (și atunci
    ''' condiția devine inertă). Pe coloanele de text operandul e chiar textul, deci se întoarce
    ''' ca atare.
    ''' </summary>
    Public Shared Function CoerceOperand(operand As String, valueType As KBotValueType) As Object
        If operand Is Nothing Then Return Nothing
        Dim s As String = operand.Trim()
        If s.Length = 0 Then Return Nothing

        Select Case valueType
            Case KBotValueType.Number
                Dim n As Double
                If Double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, n) Then Return n
                Return Nothing

            Case KBotValueType.DateTime
                Dim d As Date
                If Date.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, d) Then Return d
                Return Nothing

            Case KBotValueType.Boolean
                Return KBotDataView.ToBool(s)

            Case Else
                Return s
        End Select
    End Function

End Class
