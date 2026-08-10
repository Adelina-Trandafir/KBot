Option Strict On

''' <summary>
''' Filtrul așezat pe O coloană (slice 0028-03). Are DOUĂ jumătăți, exact ca meniul din foaia de
''' date Access, iar ele se aplică împreună (ȘI):
'''
''' <list type="bullet">
''' <item><description>lista de valori BIFATE (<see cref="SelectedValues"/>) — jumătatea de jos a
''' meniului. <c>Nothing</c> înseamnă «toate», adică nicio restricție; NU e același lucru cu o
''' mulțime goală, care înseamnă «niciuna» și golește coloana.</description></item>
''' <item><description>CONDIȚIA (<see cref="Condition"/> + operanzii) — submeniul «Filtre text /
''' numerice / de dată».</description></item>
''' </list>
'''
''' <para>Se numește <c>Condition</c>, nu <c>Operator</c>, fiindcă acela din urmă e cuvânt
''' rezervat în VB (supraîncărcarea de operatori) și ar trebui scris în paranteze drepte la
''' fiecare folosire.</para>
'''
''' <para>Un filtru care nu restrânge nimic (<see cref="IsActive"/> = False) NU se ține în grilă:
''' altfel coloana ar purta pictograma de «filtrat» fără să filtreze ceva.</para>
''' </summary>
Public NotInheritable Class KBotColumnFilter

    Private ReadOnly _columnKey As String

    ''' <summary>Filtru gol (inactiv) pentru coloana dată.</summary>
    Public Sub New(columnKey As String)
        If String.IsNullOrWhiteSpace(columnKey) Then Throw New ArgumentException("Cheie vidă.", NameOf(columnKey))
        _columnKey = columnKey
    End Sub

    ''' <summary>Cheia coloanei filtrate.</summary>
    Public ReadOnly Property ColumnKey As String
        Get
            Return _columnKey
        End Get
    End Property

    ''' <summary>
    ''' Textele AFIȘATE bifate în listă. <c>Nothing</c> = toate valorile trec (nicio restricție).
    ''' Celulele goale intră cu <see cref="KBotFilterEngine.CheieGol"/>.
    ''' </summary>
    Public Property SelectedValues As HashSet(Of String)

    ''' <summary>Condiția din submeniu. <see cref="KBotFilterOperator.None"/> = fără condiție.</summary>
    Public Property Condition As KBotFilterOperator = KBotFilterOperator.None

    ''' <summary>Primul operand al condiției (text, citit în tipul coloanei la potrivire).</summary>
    Public Property Operand1 As String

    ''' <summary>Al doilea operand — doar pentru <see cref="KBotFilterOperator.Between"/>.</summary>
    Public Property Operand2 As String

    ''' <summary>
    ''' Filtrul restrânge ceva? False => grila îl uită cu totul (vezi comentariul clasei).
    ''' </summary>
    Public ReadOnly Property IsActive As Boolean
        Get
            If Condition <> KBotFilterOperator.None Then Return True
            Return SelectedValues IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Rândul trece de AMBELE jumătăți? <paramref name="displayText"/> e textul afișat al celulei
    ''' (pe el se face bifarea și condițiile de text), <paramref name="rawValue"/> valoarea brută.
    ''' </summary>
    Public Function Matches(rawValue As Object, displayText As String, valueType As KBotValueType) As Boolean
        Dim text As String = If(displayText, String.Empty)

        If SelectedValues IsNot Nothing Then
            ' O celulă goală e bifată sub cheia ei proprie, nu sub textul ei afișat: o coloană
            ' formatată poate scrie ceva și pentru Nothing (un «0,00»), iar atunci golul n-ar mai
            ' avea cum să fie deosebit de un zero adevărat.
            Dim cheie As String = If(KBotFilterEngine.IsBlank(rawValue), KBotFilterEngine.CheieGol, text)
            If Not SelectedValues.Contains(cheie) Then Return False
        End If

        Return KBotFilterEngine.MatchesCondition(rawValue, text, valueType, Condition, Operand1, Operand2)
    End Function

    ''' <summary>Copie independentă — dialogul lucrează pe ea și o predă abia la «OK».</summary>
    Public Function Clone() As KBotColumnFilter
        Dim c As New KBotColumnFilter(_columnKey) With {
            .Condition = Condition,
            .Operand1 = Operand1,
            .Operand2 = Operand2}
        If SelectedValues IsNot Nothing Then
            c.SelectedValues = New HashSet(Of String)(SelectedValues, StringComparer.CurrentCultureIgnoreCase)
        End If
        Return c
    End Function

End Class
