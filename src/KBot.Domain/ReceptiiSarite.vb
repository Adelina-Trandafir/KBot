Option Strict On
Imports System.Collections.Generic

''' <summary>
''' The receptions the operator left UNTICKED in «Ce receptii reimprospatez?» (slice 0060),
''' in the two shapes the download needs them (sliceless, 30.09.2026). POCO.
''' </summary>
''' <remarks>
''' <para><see cref="Zile"/> = days where NO reception is ticked. They go to the robot
''' (<c>RECEPTII_SARITE</c>), which skips opening their detail -- that is where the minutes are.
''' The workflow files match on the date only, and stay that way.</para>
''' <para><see cref="Receptii"/> = single receptions of a day where another reception IS ticked.
''' The robot cannot tell them apart by date, so it reads the whole day; K-BOT then drops the
''' unticked one by date + rank within the day (<c>RangZi</c>, stamped on every row right after
''' the robot, see <c>WorkflowResultStore.CuPozitiaReceptiilor</c>).</para>
''' </remarks>
Public NotInheritable Class ReceptiiSarite

    ''' <summary>Days with no reception ticked: the robot skips their detail.</summary>
    Public ReadOnly Property Zile As New List(Of Date)()

    ''' <summary>Unticked receptions of a partly ticked day: read by the robot, dropped by K-BOT.</summary>
    Public ReadOnly Property Receptii As New List(Of ReceptieSarita)()

    ''' <summary>Nothing to skip.</summary>
    Public ReadOnly Property EsteGol As Boolean
        Get
            Return Zile.Count = 0 AndAlso Receptii.Count = 0
        End Get
    End Property
End Class

''' <summary>One reception named by its date and its rank among that date's receptions. POCO.</summary>
Public NotInheritable Class ReceptieSarita
    Public Property Data As Date
    Public Property RangZi As Integer
End Class
