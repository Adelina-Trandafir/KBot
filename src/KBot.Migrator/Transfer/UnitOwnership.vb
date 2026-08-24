Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common

''' <summary>How an Access row names the unit it belongs to.</summary>
''' <remarks>
''' The three states are NOT interchangeable, and collapsing the last two is the bug this
''' type exists to prevent. One Forexe file holds the rows of every unit in the DC
''' (verified 23.08 on the live registry: all eleven <c>cai</c> rows with a
''' <c>CaleForexe</c> point at the SAME <c>C:\AVACONT\Forexe\FX_2026.accdb</c>), so
''' "the file is open for unit 75" says nothing at all about who a row belongs to.
''' </remarks>
Public Enum UnitScope
    ''' <summary>The row carries its own <c>IdUnitate</c>, or a parent does.</summary>
    Named = 0
    ''' <summary>
    ''' The table has no <c>IdUnitate</c> column at all, so the row reaches its unit
    ''' through its parents.
    ''' </summary>
    ParentScoped = 1
    ''' <summary>
    ''' The column is there and NULL, and nothing resolves it. The row belongs to ONE
    ''' unit and we cannot say which - never to all of them.
    ''' </summary>
    Unattributable = 2
End Enum

''' <summary>
''' Answers "which unit does this Access row belong to?" for one table.
''' </summary>
''' <remarks>
''' <para>
''' The row's own <c>IdUnitate</c> answers it when it is filled in. When it is NULL, the
''' answer comes from the parent table declared by <see cref="TableMap.OwnedVia"/> -
''' <c>FX_DDF_REV_SA.IDDF</c> ▸ <c>FX_DDF.IdUnitate</c>, for instance.
''' </para>
''' <para>
''' What made this necessary: four <c>FX_DDF_REV_SA</c> rows (and the four matching
''' <c>FX_DDF_REV_SB</c> rows) carry <c>IdUnitate = NULL</c>, and every
''' <c>FX_Extrase</c> row does. Read as "belongs to whichever unit is being processed"
''' they were checked - and would have been written - against EVERY selected unit, each
''' time resolving <c>IdClsf</c> against the wrong nomenclator. That is what produced
''' the two mirrored findings on 23.08: unit 75 reported IdClsf 141 missing (it is unit
''' 76's row, through IDDF 73) and unit 76 reported 97 and 374 missing (unit 75's rows,
''' through IDDF 77, 79 and 80).
''' </para>
''' <para>
''' <see cref="AccessTableReader.ValueOrMissing"/> already keeps "absent" and "NULL"
''' apart. Everything above this line used <see cref="Verifier.AsInteger"/>, which
''' returns Nothing for both, and threw the distinction away one line after it was made.
''' </para>
''' </remarks>
Public NotInheritable Class UnitOwnership

    ''' <summary>The column every FX_* table spells the same way.</summary>
    Public Const UnitColumn As String = "IdUnitate"

    Private ReadOnly _childColumn As String
    Private ReadOnly _owners As Dictionary(Of String, Integer)

    Private Sub New(childColumn As String, owners As Dictionary(Of String, Integer))
        _childColumn = childColumn
        _owners = owners
    End Sub

    ''' <summary>Parent rows indexed, for the log.</summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _owners.Count
        End Get
    End Property

    ''' <summary>
    ''' Reads the parent table named by <paramref name="map"/> and indexes its key ▸ unit.
    ''' </summary>
    ''' <returns>Nothing when the map declares no owner chain, or the parent is missing.</returns>
    Public Shared Function Build(cn As OleDbConnection, map As TableMap) As UnitOwnership
        If map Is Nothing OrElse Not map.HasUnitOwner Then Return Nothing

        Try
            Dim realName = AccessSchema.ResolveTableName(cn, map.UnitOwnerTable)
            If realName Is Nothing Then Return Nothing

            Dim owners As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Using reader = AccessSchema.OpenReader(cn, realName)
                If Not reader.HasColumn(map.UnitOwnerParentColumn) Then Return Nothing
                If Not reader.HasColumn(UnitColumn) Then Return Nothing

                While reader.Read()
                    Dim owner = Verifier.AsInteger(reader.Value(UnitColumn))
                    If Not owner.HasValue Then Continue While
                    Dim key = Normalise(reader.Value(map.UnitOwnerParentColumn))
                    If key.Length = 0 Then Continue While
                    owners(key) = owner.Value
                End While
            End Using

            Return New UnitOwnership(map.UnitOwnerChildColumn, owners)

        Catch ex As Exception
            ' A parent that cannot be read leaves the rows unattributable, which the
            ' verifier reports and the runner refuses. It must not crash the gate.
            GlobalErrorLog.Write("UnitOwnership.Build", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>The unit of the row's parent, if the chain reaches one.</summary>
    Public Function TryOwner(reader As AccessTableReader, ByRef idUnitate As Integer) As Boolean
        If reader Is Nothing OrElse Not reader.HasColumn(_childColumn) Then Return False
        Dim key = Normalise(reader.Value(_childColumn))
        If key.Length = 0 Then Return False
        Return _owners.TryGetValue(key, idUnitate)
    End Function

    ''' <summary>
    ''' Which unit the current row belongs to, and how we know.
    ''' </summary>
    ''' <remarks>
    ''' The one function both the verifier and the runner ask. They asked it separately
    ''' before, and they disagreed - the verifier reported a miss where the runner would
    ''' have written a wrong id.
    ''' </remarks>
    Public Shared Function Resolve(reader As AccessTableReader, ownership As UnitOwnership,
                                   ByRef idUnitate As Integer) As UnitScope
        If reader Is Nothing Then Return UnitScope.Unattributable
        If Not reader.HasColumn(UnitColumn) Then Return UnitScope.ParentScoped

        Dim own = Verifier.AsInteger(reader.Value(UnitColumn))
        If own.HasValue Then
            idUnitate = own.Value
            Return UnitScope.Named
        End If

        If ownership IsNot Nothing AndAlso ownership.TryOwner(reader, idUnitate) Then
            Return UnitScope.Named
        End If

        Return UnitScope.Unattributable
    End Function

    ''' <summary>One canonical text form per key, so Integer 1 and Long 1 are one key.</summary>
    Private Shared Function Normalise(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Dim formattable = TryCast(value, IFormattable)
        If formattable IsNot Nothing Then Return formattable.ToString(Nothing, CultureInfo.InvariantCulture)
        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

End Class
