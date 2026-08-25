''' <summary>How an Access row names the unit it belongs to.</summary>
''' <remarks>
''' The states are NOT interchangeable, and collapsing them is the bug this type exists to
''' prevent. One Forexe file holds the rows of every unit in the DC (verified 23.08 on the
''' live registry, still true 24.08: all eleven <c>cai</c> rows with a <c>CaleForexe</c>
''' point at the SAME <c>C:\AVACONT\Forexe\FX_2026.accdb</c>), so "the file is open for
''' unit 75" says nothing at all about who a row belongs to.
''' </remarks>
Public Enum UnitScope
    ''' <summary>The row carries its own <c>IdUnitate</c>, or the file it came from does.</summary>
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
    ''' <summary>
    ''' The row genuinely serves SEVERAL units: a DDF header, a revision, an ORD and its
    ''' descriptive tables.
    ''' </summary>
    ''' <remarks>
    ''' Added by slice 0046 with decision D4. One <c>IDDF</c> can serve many units, so its
    ''' unit is a SET and not a value - which is why <c>FX_DDF.IdUnitate</c> was a relic
    ''' long before anyone said so out loud. A row in this state travels, but nothing that
    ''' needs one unit (a classification, a partner) can be resolved on it; no such table
    ''' asks for one, and <see cref="TransferRunner"/> stops rather than picking a unit if
    ''' one ever does.
    ''' </remarks>
    SharedByMany = 3
End Enum

''' <summary>
''' Reads a row's own <c>IdUnitate</c>, keeping "absent" and "NULL" apart.
''' </summary>
''' <remarks>
''' <para>
''' <b>What this no longer does.</b> Until slice 0046 this type also walked a PARENT chain
''' declared by <c>TableMap.OwnedVia</c>: an <c>FX_DDF_REV_SA</c> row with a NULL
''' <c>IdUnitate</c> asked <c>FX_DDF</c> whose it was. Decision D1 killed that reading -
''' <c>FX_DDF.IdUnitate</c> is a relic, never read, and one <c>IDDF</c> can serve many
''' units, so it could not have answered honestly even when it was filled in. The arrow
''' now points the other way: the parent asks the children, once, before the run starts,
''' and <see cref="OwnershipPlan"/> holds the answer.
''' </para>
''' <para>
''' <see cref="AccessTableReader.ValueOrMissing"/> keeps "absent" and "NULL" apart.
''' Everything above this line used <see cref="Verifier.AsInteger"/>, which returns Nothing
''' for both, and threw the distinction away one line after it was made.
''' </para>
''' </remarks>
Public NotInheritable Class UnitOwnership

    ''' <summary>The column every FX_* table spells the same way.</summary>
    Public Const UnitColumn As String = "IdUnitate"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Which unit the current row names for itself, and how we know.
    ''' </summary>
    ''' <remarks>
    ''' The one function both the verifier and the runner ask, through
    ''' <see cref="OwnershipPlan.Decide"/>. They asked separately before, and they
    ''' disagreed - the verifier reported a miss where the runner would have written a
    ''' wrong id.
    ''' </remarks>
    Public Shared Function Resolve(reader As AccessTableReader, ByRef idUnitate As Integer) As UnitScope
        If reader Is Nothing Then Return UnitScope.Unattributable
        If Not reader.HasColumn(UnitColumn) Then Return UnitScope.ParentScoped

        Dim own = Verifier.AsInteger(reader.Value(UnitColumn))
        If own.HasValue Then
            idUnitate = own.Value
            Return UnitScope.Named
        End If

        Return UnitScope.Unattributable
    End Function

End Class
