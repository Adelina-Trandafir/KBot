' POCO for POST /api/forexe/receptii/refacere (slice 0062): what the server found missing
' in FX_Receptii_H / FX_Receptii for one angajament, judged against FX_Istoric by IDH, and
' what it wrote. The same shape comes back from the dry run (Applied = False, every
' *Written count 0) and from the real run, so the confirmation the operator reads is the
' same walk that later writes.
' Pure model (no I/O) -> no Try/Catch (house rule: simple POCOs).

''' <summary>
''' Result of rebuilding the missing receptie snapshots (H) and lines from the history.
''' </summary>
Public NotInheritable Class ReceptiiRebuildResult
    Public Property Cod As String = String.Empty

    ''' <summary>False = dry run: nothing was written, the counts say what WOULD be.</summary>
    Public Property Applied As Boolean

    ''' <summary>Snapshots (FX_Receptii_H) whose history row has no H yet.</summary>
    Public Property HeadersMissing As Integer
    ''' <summary>Lines (FX_Receptii) whose history row has no line yet.</summary>
    Public Property LinesMissing As Integer
    ''' <summary>Lines that exist but hang on no snapshot (IDRH NULL); they get re-attached.</summary>
    Public Property LinesOrphaned As Integer

    Public Property HeadersWritten As Integer
    Public Property LinesWritten As Integer
    Public Property LinesRelinked As Integer

    ''' <summary>Lines at the end of the history that have no header yet (left alone).</summary>
    Public Property LinesWithoutHeader As Integer
    ''' <summary>Lines skipped because their indicator is not in FX_Indicatori.</summary>
    Public Property LinesSkipped As Integer

    ''' <summary>IDRR of every receptie whose DIF chain was recomputed after the writes.</summary>
    Public Property ReceptiiRecalculated As New List(Of Integer)()

    ''' <summary>Warnings for the operator (Romanian, literal diacritics).</summary>
    Public Property Warnings As New List(Of String)()

    ''' <summary>True when the walk found nothing to add or re-attach.</summary>
    Public ReadOnly Property NothingToDo As Boolean
        Get
            Return HeadersMissing = 0 AndAlso LinesMissing = 0 AndAlso LinesOrphaned = 0
        End Get
    End Property
End Class
