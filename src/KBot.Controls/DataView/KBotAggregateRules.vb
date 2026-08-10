Option Strict On
Imports System.Collections
Imports System.ComponentModel

''' <summary>
''' TABELA care leagă <see cref="KBotValueType"/> de agregatele permise — singurul loc unde
''' scrie ce se poate aduna și ce se poate doar număra. O citesc trei consumatori, ca regula să
''' nu ajungă să existe în trei variante:
''' <list type="number">
''' <item>setterul <see cref="KBotDataColumn.Aggregate"/> (o pereche greșită ARUNCĂ, nu se
''' transformă tăcut într-o celulă goală — regula casei);</item>
''' <item><see cref="KBotAggregateConverter"/>, ca grila de proprietăți din Visual Studio să
''' ofere DOAR agregatele valabile pentru tipul coloanei;</item>
''' <item>calculul din <c>KBotDataView.Footer</c>.</item>
''' </list>
'''
''' <para><b>De ce Min/Max nu apar la Text.</b> Alfabetic „cel mai mic” e o întrebare cu răspuns,
''' dar nu cea pe care o pune cineva care se uită la un subsol; <see cref="KBotAggregate.First"/>
''' și <see cref="KBotAggregate.Last"/> (ordinea rândurilor) sunt ce vrea de fapt operatorul, iar
''' două perechi care seamănă între ele s-ar alege greșit.</para>
''' </summary>
Public NotInheritable Class KBotAggregateRules

    Private Sub New()
    End Sub

    ' Agregatele valabile ORICE ar fi în coloană: se numără rânduri, nu se interpretează valori.
    Private Shared ReadOnly _oricare As KBotAggregate() = {
        KBotAggregate.None, KBotAggregate.Count, KBotAggregate.CountDistinct,
        KBotAggregate.CountEmpty, KBotAggregate.First, KBotAggregate.Last
    }

    Private Shared ReadOnly _numeric As KBotAggregate() = {
        KBotAggregate.None, KBotAggregate.Sum, KBotAggregate.Average,
        KBotAggregate.Min, KBotAggregate.Max, KBotAggregate.Count,
        KBotAggregate.CountDistinct, KBotAggregate.CountEmpty,
        KBotAggregate.First, KBotAggregate.Last
    }

    Private Shared ReadOnly _calendaristic As KBotAggregate() = {
        KBotAggregate.None, KBotAggregate.Min, KBotAggregate.Max, KBotAggregate.Count,
        KBotAggregate.CountDistinct, KBotAggregate.CountEmpty,
        KBotAggregate.First, KBotAggregate.Last
    }

    Private Shared ReadOnly _logic As KBotAggregate() = {
        KBotAggregate.None, KBotAggregate.Count, KBotAggregate.CountTrue,
        KBotAggregate.CountFalse, KBotAggregate.CountEmpty
    }

    ''' <summary>
    ''' Agregatele permise pentru un tip de valoare, în ordinea în care merită oferite.
    ''' Lista întoarsă e cea PARTAJATĂ (doar-citire) — nu se modifică de către apelant.
    ''' </summary>
    Public Shared Function Allowed(valueType As KBotValueType) As IReadOnlyList(Of KBotAggregate)
        Select Case valueType
            Case KBotValueType.Number
                Return _numeric
            Case KBotValueType.DateTime
                Return _calendaristic
            Case KBotValueType.Boolean
                Return _logic
            Case Else
                Return _oricare
        End Select
    End Function

    ''' <summary>Agregatul e permis pentru tipul de valoare dat?</summary>
    Public Shared Function IsAllowed(valueType As KBotValueType, aggregate As KBotAggregate) As Boolean
        For Each a In Allowed(valueType)
            If a = aggregate Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Mesajul unei perechi nepermise — același text oriunde se ridică excepția (setterul
    ''' agregatului, setterul tipului, validarea de la <c>EndInit</c>), ca operatorul să nu
    ''' primească trei formulări pentru aceeași greșeală.
    ''' </summary>
    Friend Shared Function MesajNepermis(colKey As String, valueType As KBotValueType,
                                         aggregate As KBotAggregate) As String
        Dim permise As New List(Of String)()
        For Each a In Allowed(valueType)
            permise.Add(a.ToString())
        Next
        Return $"Agregatul «{aggregate}» nu se poate aplica unei coloane «{If(colKey, String.Empty)}» " &
               $"de tip «{valueType}». Permise: {String.Join(", ", permise)}."
    End Function

End Class

''' <summary>
''' Convertorul care face ca grila de proprietăți din Visual Studio să ofere, în lista
''' <c>Aggregate</c>, DOAR agregatele valabile pentru <see cref="KBotDataColumn.ValueType"/>-ul
''' coloanei editate. Fără el, designerul ar arăta toate valorile enumerării, iar alegerea
''' greșită s-ar solda cu o excepție abia la rulare — adică exact traseul lung.
'''
''' Când contextul nu spune pe ce coloană suntem (colecție multi-selecție, folosire în afara
''' designerului), se întoarce lista completă: mai bine tot, decât un filtru ghicit greșit.
''' </summary>
Public NotInheritable Class KBotAggregateConverter
    Inherits EnumConverter

    Public Sub New(type As Type)
        MyBase.New(type)
    End Sub

    Public Overrides Function GetStandardValuesSupported(context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    ''' <summary>Lista rămâne închisă (doar valorile oferite), ca la orice enumerare.</summary>
    Public Overrides Function GetStandardValuesExclusive(context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues(context As ITypeDescriptorContext) As StandardValuesCollection
        Dim col As KBotDataColumn = If(context Is Nothing, Nothing, TryCast(context.Instance, KBotDataColumn))
        If col Is Nothing Then Return MyBase.GetStandardValues(context)
        Return New StandardValuesCollection(CType(KBotAggregateRules.Allowed(col.ValueType), ICollection))
    End Function

End Class
