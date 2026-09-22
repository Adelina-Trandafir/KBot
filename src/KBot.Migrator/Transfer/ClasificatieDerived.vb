''' <summary>
''' Recomputes the values MariaDB's GENERATED columns on <c>Clasificatii</c> will hold,
''' from the columns the migrator actually writes.
''' </summary>
''' <remarks>
''' <para>
''' This exists for one reason: <c>Clasificatii</c> carries constraints that point into a
''' DIFFERENT database, <c>AVACONT_COMUN</c>:
''' </para>
''' <code>
''' ClsfF   -> AVACONT_COMUN.DefaClsfF          (generated)
''' Titlu   -> AVACONT_COMUN.DefaTitlu          (generated)
''' SS      -> AVACONT_COMUN.DefaSursaSector    (generated from Sector + the WRITTEN Sursa)
''' Articol -> AVACONT_COMUN.DefaArticol        (written)
''' ClsfE   -> AVACONT_COMUN.DefaClsfE          (generated; NO foreign key on the live
'''                                              server - checked anyway, see Verifier)
''' </code>
''' <para>
''' Most of them are values the migrator never writes and cannot see before the
''' INSERT - they fall out of concat/left/replace over what it does write. So a row is
''' rejected with <c>1452</c> naming a column nobody wrote. Recomputing them here lets
''' Verifică say which classification will be refused, and why, before anything is
''' written.
''' </para>
''' <para>
''' Slice 0075-00 moved <c>Sector</c>, <c>Sursa</c> and <c>SS</c> out of the generated set
''' entirely: all three are plain written columns now. The source letter was never readable
''' from the capitol (01A, 01D, 01F and 01G all end in <c>01</c>), and keeping <c>SS</c>
''' generated as <c>concat(&lt;sector&gt;, Sursa)</c> is not possible either - MariaDB 10.11
''' refuses a stored generated column built out of another column with error 1901, proven on
''' the live server in a fresh table. So for these three this class is no longer a
''' REPLICATION of the DDL: it is the SOURCE of the values, and what it returns is what gets
''' written. The remaining six (Clsf, Titlu, ClsfSal, ClsfF, ClsfE, ClsfX) are still
''' generated and still only predicted here.
''' </para>
''' <para>
''' <b>This is a REPLICATION of the DDL, not a reading of it.</b> The expressions below
''' are transcribed from <c>000_DEMO.sql</c> of 22.08. If a generated column is redefined
''' on the server, this check goes stale - but it fails SAFE: the check is an early
''' warning, and the foreign key on the server is still the thing that actually refuses
''' the row. <see cref="Verifier"/> also reports the live GENERATION_EXPRESSION next to
''' these, so drift is visible rather than silent.
''' </para>
''' </remarks>
Public NotInheritable Class ClasificatieDerived

    ''' <param name="sursa">
    ''' The Access <c>Sursa</c> value. Since slice 0075-00 the target column is WRITTEN, so
    ''' this is the value that decides <see cref="SS"/> - it is no longer computed from the
    ''' capitol. Left out (or empty) it falls back to <c>A</c>, which is what the old
    ''' generated expression produced for every capitol but <c>xx10</c>.
    ''' </param>
    Public Sub New(capitol As String, subcapitol As String, articol As String, alineat As String,
                   Optional sursa As String = Nothing)
        Me.Capitol = Safe(capitol)
        Me.Subcapitol = Safe(subcapitol)
        Me.Articol = Safe(articol)
        Me.Alineat = Safe(alineat)
        Me.Sursa = NormalizeSursa(sursa, Me.Capitol)
    End Sub

    Public ReadOnly Property Capitol As String
    Public ReadOnly Property Subcapitol As String
    Public ReadOnly Property Articol As String
    Public ReadOnly Property Alineat As String

    ''' <summary><c>concat_ws('.', Capitol, Subcapitol, Articol, Alineat)</c></summary>
    Public ReadOnly Property Clsf As String
        Get
            Return String.Join(".", {Capitol, Subcapitol, Articol, Alineat})
        End Get
    End Property

    ''' <summary><c>left(coalesce(Articol,''), 2)</c></summary>
    Public ReadOnly Property Titlu As String
        Get
            Return Left2(Articol)
        End Get
    End Property

    ''' <summary>
    ''' <c>concat(left(Capitol,2), replace(Subcapitol,'.',''))</c>
    ''' </summary>
    Public ReadOnly Property ClsfF As String
        Get
            Return Left2(Capitol) & Subcapitol.Replace(".", String.Empty)
        End Get
    End Property

    ''' <summary>
    ''' <c>concat(replace(Articol,'.',''), Alineat)</c>
    ''' </summary>
    Public ReadOnly Property ClsfE As String
        Get
            Return Articol.Replace(".", String.Empty) & Alineat
        End Get
    End Property

    ''' <summary>
    ''' The WRITTEN sector (slice 0075-00): the last two characters of the capitol, mapped.
    ''' </summary>
    ''' <remarks>
    ''' It was <c>case right(Capitol,2) when '02' then '02' when '01' then '01' when '10'
    ''' then '02' when '00' then '01' else '' end</c> on the server. 03/04/05/08 were added
    ''' here, being the sectors DefaSursaSector knows; the four historical endings keep
    ''' exactly the mapping they had. An unknown ending yields an empty sector, so SS is a
    ''' bare letter and the foreign key refuses it - a near-blank, never a plausible value.
    ''' </remarks>
    Public ReadOnly Property Sector As String
        Get
            Select Case Right2(Capitol)
                Case "00" : Return "01"
                Case "01" : Return "01"
                Case "02" : Return "02"
                Case "10" : Return "02"
                Case "03" : Return "03"
                Case "04" : Return "04"
                Case "05" : Return "05"
                Case "08" : Return "08"
                Case Else : Return String.Empty
            End Select
        End Get
    End Property

    ''' <summary>
    ''' The WRITTEN source letter (slice 0075-00), normalised by
    ''' <see cref="NormalizeSursa"/> - no longer computed from the capitol.
    ''' </summary>
    Public ReadOnly Property Sursa As String

    ''' <summary>
    ''' The WRITTEN <c>SS</c> (slice 0075-00): sector + source.
    ''' </summary>
    ''' <remarks>
    ''' NOT NULL on the target, with the foreign key into <c>DefaSursaSector</c> and NO
    ''' default - an INSERT that leaves it out fails with 1364 rather than storing a wrong
    ''' sector-source quietly. A Capitol outside the eight endings computes an EMPTY sector,
    ''' so SS is one letter, which the foreign key refuses: a near-blank, not a plausible
    ''' wrong value.
    ''' </remarks>
    Public ReadOnly Property SS As String
        Get
            Return Sector & Sursa
        End Get
    End Property

    ''' <summary>
    ''' The value written into <c>Clasificatii.Sursa</c> for one Access row.
    ''' </summary>
    ''' <remarks>
    ''' Three rules, in this order:
    ''' <list type="number">
    ''' <item>A <c>xx10</c> capitol is <c>E</c> whatever the file says - that is what the old
    ''' generated column produced, and the capitol is what the rest of the row is built on.</item>
    ''' <item>Otherwise the Access value, trimmed and uppercased (its first character: the
    ''' target is <c>char(1)</c>).</item>
    ''' <item>Nothing usable - the default <c>A</c>, again what the old expression gave for
    ''' every capitol but <c>xx10</c>.</item>
    ''' </list>
    ''' </remarks>
    Public Shared Function NormalizeSursa(sursa As String, capitol As String) As String
        If Right2(Safe(capitol)) = "10" Then Return "E"
        Dim text = Safe(sursa).Trim().ToUpperInvariant()
        If text.Length = 0 Then Return "A"
        Return text.Substring(0, 1)
    End Function

    Private Shared Function Safe(value As String) As String
        Return If(value, String.Empty)
    End Function

    Private Shared Function Left2(value As String) As String
        If value.Length <= 2 Then Return value
        Return value.Substring(0, 2)
    End Function

    Private Shared Function Right2(value As String) As String
        If value.Length <= 2 Then Return value
        Return value.Substring(value.Length - 2)
    End Function

End Class
