Option Strict On
Imports System.Globalization
Imports KBot.Domain

''' <summary>
''' Motorul de filtrare al vederii Istoric (felia 0022) — portul lui
''' <c>mdl_FX_Popups.ApplyColumnFilter</c>. Clasă PURĂ, fără nicio referință de UI, complet
''' testabilă headless.
'''
''' Semantică (identică cu Access):
'''   * trei segmente INDEPENDENTE — <c>Clsf</c>, <c>TipRand</c>, <c>DataFx</c>;
'''   * setarea unui segment îl ÎNLOCUIEȘTE pe acela (nu-l acumulează);
'''   * «TOATE» golește DOAR acel segment, lăsându-le pe celelalte;
'''   * filtrul efectiv = ȘI-ul (AND) segmentelor ne-goale;
'''   * <see cref="Apply"/> întoarce lista filtrată.
'''
''' Feluri de segment:
'''   * <c>Clsf</c>    — o mulțime de valori <c>IdClsf</c> (selecția din meniu, expandată prin
'''                      ierarhie);
'''   * <c>TipRand</c> — fie o valoare EXACTĂ, fie un grup de prefix (<c>Rez_</c> / <c>Plata_</c>),
'''                      potrivit CASE-INSENSITIVE (ABATERE §9.2: Access lua asta gratis din
'''                      Option Compare Database);
'''   * <c>DataFx</c>  — fie lună+an, fie o zi exactă.
''' POCO cu logică pură — fără I/O, deci fără Try/Catch (regula casei).
''' </summary>
Public NotInheritable Class IstoricFilter

    ' Fiecare segment: un predicat (Nothing = inactiv) + o etichetă scurtă pentru rezumat.
    Private _clsfIds As HashSet(Of Integer)
    Private _clsfLabel As String

    Private _tipPredicate As Func(Of String, Boolean)
    Private _tipLabel As String

    Private _dataPredicate As Func(Of Date?, Boolean)
    Private _dataLabel As String

    ' ── Segment Clsf ─────────────────────────────────────────────────────────
    ''' <summary>Fixează segmentul de clasificație pe mulțimea de <c>IdClsf</c> dată.
    ''' O mulțime goală filtrează la zero rânduri (selecție validă, dar fără potriviri).</summary>
    Public Sub SetClsf(ids As IEnumerable(Of Integer), label As String)
        _clsfIds = New HashSet(Of Integer)(If(ids, Array.Empty(Of Integer)()))
        _clsfLabel = label
    End Sub

    ''' <summary>«TOATE» pe clasificație — golește DOAR acest segment.</summary>
    Public Sub ClearClsf()
        _clsfIds = Nothing
        _clsfLabel = Nothing
    End Sub

    ' ── Segment TipRand ──────────────────────────────────────────────────────
    ''' <summary>Fixează segmentul TipRand pe o valoare EXACTĂ (case-insensitive).</summary>
    Public Sub SetTipRandExact(value As String)
        Dim wanted As String = If(value, String.Empty)
        _tipPredicate = Function(t) String.Equals(If(t, String.Empty), wanted, StringComparison.OrdinalIgnoreCase)
        _tipLabel = wanted
    End Sub

    ''' <summary>Fixează segmentul TipRand pe un GRUP de prefix (ex. «Rez_» / «Plata_»),
    ''' potrivit case-insensitive. Eticheta e furnizată de apelant («REZERVĂRI» / «PLĂȚI»).</summary>
    Public Sub SetTipRandPrefix(prefix As String, label As String)
        Dim p As String = If(prefix, String.Empty)
        _tipPredicate = Function(t) If(t, String.Empty).StartsWith(p, StringComparison.OrdinalIgnoreCase)
        _tipLabel = label
    End Sub

    ''' <summary>«TOATE» pe TipRand — golește DOAR acest segment.</summary>
    Public Sub ClearTipRand()
        _tipPredicate = Nothing
        _tipLabel = Nothing
    End Sub

    ' ── Segment DataFx ───────────────────────────────────────────────────────
    ''' <summary>Fixează segmentul DataFx pe o lună+an (toate zilele acelei luni).</summary>
    Public Sub SetDataFxMonth(year As Integer, month As Integer, label As String)
        _dataPredicate = Function(d) d.HasValue AndAlso d.Value.Year = year AndAlso d.Value.Month = month
        _dataLabel = label
    End Sub

    ''' <summary>Fixează segmentul DataFx pe o zi exactă (ignorând ora).</summary>
    Public Sub SetDataFxDay(zi As Date, label As String)
        Dim day As Date = zi.Date
        _dataPredicate = Function(d) d.HasValue AndAlso d.Value.Date = day
        _dataLabel = label
    End Sub

    ''' <summary>«TOATE» pe DataFx — golește DOAR acest segment.</summary>
    Public Sub ClearDataFx()
        _dataPredicate = Nothing
        _dataLabel = Nothing
    End Sub

    ' ── Reset total ──────────────────────────────────────────────────────────
    ''' <summary>Golește toate cele trei segmente (butonul «Reset» / context nou).</summary>
    Public Sub ClearAll()
        ClearClsf()
        ClearTipRand()
        ClearDataFx()
    End Sub

    ' ── Stare (pentru rezumatul din bară) ────────────────────────────────────
    Public ReadOnly Property ClsfActive As Boolean
        Get
            Return _clsfIds IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property TipRandActive As Boolean
        Get
            Return _tipPredicate IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property DataFxActive As Boolean
        Get
            Return _dataPredicate IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property AnySegmentActive As Boolean
        Get
            Return ClsfActive OrElse TipRandActive OrElse DataFxActive
        End Get
    End Property

    ''' <summary>Rezumat scurt al segmentelor active, pentru <c>lblFiltruActiv</c>. Gol când
    ''' nu e nimic activ.</summary>
    Public ReadOnly Property Summary As String
        Get
            Dim parts As New List(Of String)()
            If Not String.IsNullOrEmpty(_clsfLabel) Then parts.Add("Clsf: " & _clsfLabel)
            If Not String.IsNullOrEmpty(_tipLabel) Then parts.Add("Tip: " & _tipLabel)
            If Not String.IsNullOrEmpty(_dataLabel) Then parts.Add("Data: " & _dataLabel)
            Return String.Join(" · ", parts)
        End Get
    End Property

    ' ── Aplicare ─────────────────────────────────────────────────────────────
    ''' <summary>
    ''' Întoarce rândurile care trec de ȘI-ul segmentelor active. Un segment inactiv (Nothing)
    ''' nu filtrează nimic. Ordinea de intrare se păstrează.
    ''' </summary>
    Public Function Apply(randuri As IEnumerable(Of IstoricRand)) As List(Of IstoricRand)
        Dim result As New List(Of IstoricRand)()
        If randuri Is Nothing Then Return result
        For Each r As IstoricRand In randuri
            If r Is Nothing Then Continue For
            If _clsfIds IsNot Nothing AndAlso Not _clsfIds.Contains(r.IdClsf) Then Continue For
            If _tipPredicate IsNot Nothing AndAlso Not _tipPredicate(r.TipRand) Then Continue For
            If _dataPredicate IsNot Nothing AndAlso Not _dataPredicate(r.DataFx) Then Continue For
            result.Add(r)
        Next
        Return result
    End Function

    ''' <summary>Numele lunii în română, cu prima literă mare (ex. «Ianuarie») — utilitar de
    ''' etichetă partajat cu vederea.</summary>
    Public Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return month.ToString(CultureInfo.InvariantCulture)
        Dim ro As New CultureInfo("ro-RO")
        Dim name As String = ro.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return month.ToString(CultureInfo.InvariantCulture)
        Return Char.ToUpper(name(0), ro) & name.Substring(1)
    End Function

End Class
