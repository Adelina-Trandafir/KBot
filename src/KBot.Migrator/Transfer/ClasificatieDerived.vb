''' <summary>
''' Recomputes the values MariaDB's GENERATED columns on <c>Clasificatii</c> will hold,
''' from the four columns the migrator actually writes.
''' </summary>
''' <remarks>
''' <para>
''' This exists for one reason: <c>Clasificatii</c> carries SIX constraints and FIVE of
''' them point into a DIFFERENT database, <c>AVACONT_COMUN</c>:
''' </para>
''' <code>
''' ClsfE   -> AVACONT_COMUN.DefaClsfE          (generated)
''' ClsfF   -> AVACONT_COMUN.DefaClsfF          (generated)
''' Titlu   -> AVACONT_COMUN.DefaTitlu          (generated)
''' SS      -> AVACONT_COMUN.DefaSursaSector    (generated)
''' Articol -> AVACONT_COMUN.DefaArticol        (written)
''' </code>
''' <para>
''' Four of the five are values the migrator never writes and cannot see before the
''' INSERT - they fall out of concat/left/replace over what it does write. So a row is
''' rejected with <c>1452</c> naming a column nobody wrote. Recomputing them here lets
''' Verifică say which classification will be refused, and why, before anything is
''' written.
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

    Public Sub New(capitol As String, subcapitol As String, articol As String, alineat As String)
        Me.Capitol = Safe(capitol)
        Me.Subcapitol = Safe(subcapitol)
        Me.Articol = Safe(articol)
        Me.Alineat = Safe(alineat)
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
    ''' <c>case right(Capitol,2) when '02' then '02' when '01' then '01'
    ''' when '10' then '02' when '00' then '01' else '' end</c>
    ''' </summary>
    Public ReadOnly Property Sector As String
        Get
            Select Case Right2(Capitol)
                Case "02" : Return "02"
                Case "01" : Return "01"
                Case "10" : Return "02"
                Case "00" : Return "01"
                Case Else : Return String.Empty
            End Select
        End Get
    End Property

    ''' <summary>
    ''' <c>case right(Capitol,2) when '02' then 'A' when '01' then 'A'
    ''' when '10' then 'E' when '00' then 'A' else '' end</c>
    ''' </summary>
    Public ReadOnly Property Sursa As String
        Get
            Select Case Right2(Capitol)
                Case "02" : Return "A"
                Case "01" : Return "A"
                Case "10" : Return "E"
                Case "00" : Return "A"
                Case Else : Return String.Empty
            End Select
        End Get
    End Property

    ''' <summary>
    ''' <c>concat(Sector, Sursa)</c>.
    ''' </summary>
    ''' <remarks>
    ''' Note the failure mode: a Capitol outside 00/01/02/10 computes an EMPTY SS, which
    ''' will not be in DefaSursaSector. The row is then refused for a blank, not for a
    ''' wrong value - which is far harder to read in a raw 1452.
    ''' </remarks>
    Public ReadOnly Property SS As String
        Get
            Return Sector & Sursa
        End Get
    End Property

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
