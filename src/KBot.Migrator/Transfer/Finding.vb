''' <summary>How much a finding weighs.</summary>
''' <remarks>
''' The two classes are the mechanism behind the two buttons, exactly as in slice 0044:
''' anything BLOCANT stops both, and the run cannot start while one stands.
''' Unlike 0044 there is no FORTABIL class here - this transfer is a one-off (D1) into a
''' brand-new database (D7), so "run anyway and skip the guilty rows" would leave a
''' half-populated unit with no way to tell which rows are missing.
''' </remarks>
Public Enum FindingClass
    ''' <summary>Worth reading, changes nothing.</summary>
    Informativ = 0
    ''' <summary>Should be read before running, but does not stop the run.</summary>
    Atentie = 1
    ''' <summary>Stops the run.</summary>
    Blocant = 2
End Enum

''' <summary>
''' One thing the verifier found. The kind is an ASCII token, never a sentence: it is
''' matched on, grouped by and written into a report column.
''' </summary>
''' <remarks>
''' RULE 0 - the tokens are ASCII because they are protocol, not prose. Only
''' <see cref="Message"/> is operator-facing Romanian with real diacritics.
''' </remarks>
Public NotInheritable Class Finding

    ' ---- kinds ----------------------------------------------------------------
    Public Const TABEL_LIPSA As String = "TABEL_LIPSA"
    Public Const COLOANA_OBLIGATORIE As String = "COLOANA_OBLIGATORIE"
    Public Const COLOANA_PREA_INGUSTA As String = "COLOANA_PREA_INGUSTA"
    Public Const CLASIFICATIE_NEREZOLVATA As String = "CLASIFICATIE_NEREZOLVATA"
    ''' <summary>
    ''' A row carries an <c>IdUnitate</c> column, it is NULL, and no owner chain says
    ''' which unit the row belongs to.
    ''' </summary>
    ''' <remarks>
    ''' Its own kind rather than CLASIFICATIE_NEREZOLVATA, because the remedy is
    ''' different: the classification is not missing, the row's UNIT is unknown, and the
    ''' fix is either the Access data or a <see cref="TableMap.OwnedVia"/> declaration.
    ''' Reading NULL as "belongs to the unit being written" is what produced the mirrored
    ''' 141 / 97+374 findings of 23.08.
    ''' </remarks>
    Public Const UNITATE_NEDETERMINATA As String = "UNITATE_NEDETERMINATA"
    ''' <summary>
    ''' An Access column lands in the target's <c>IdClsf</c> on a plain name match.
    ''' </summary>
    ''' <remarks>
    ''' Rule 1 again, at the other end: the target's <c>IdClsf</c> is the id MariaDB
    ''' assigned, the Access one is local to the file. FX_DDF_REV_SA/SB and FX_ORD_TBL
    ''' resolve it through <see cref="ClasificatiiMap"/>; a name-matched table that
    ''' carries the column has nothing to resolve it and would write the local id.
    ''' </remarks>
    Public Const CLASIFICATIE_NECORELATA As String = "CLASIFICATIE_NECORELATA"
    ''' <summary>
    ''' An authority row names a unit that appears in NO <c>cai</c> row at all.
    ''' </summary>
    ''' <remarks>
    ''' Decision D7 case C, and distinct from every other unit finding: the unit is not
    ''' unknown to the RUN, it is unknown to the REGISTRY. A unit belonging to another DC
    ''' (case B) is the normal shape of a shared Forexe file and is skipped in silence -
    ''' one <c>FX_2026.accdb</c> legitimately holds several DCs. This kind means the file
    ''' and the registry disagree, and neither can be guessed past.
    ''' </remarks>
    Public Const UNITATE_NECUNOSCUTA As String = "UNITATE_NECUNOSCUTA"
    ''' <summary>
    ''' A DDF with no <c>FX_DDF_REV_SA</c> row at all.
    ''' </summary>
    ''' <remarks>
    ''' The operator says this is impossible (D6). It is BLOCANT precisely because it is
    ''' impossible: an impossible case that passes quietly is how bad data arrives. With
    ''' the authority gone the document has no unit and no way to get one - it is not the
    ''' NULL case, where the column exists and is empty.
    ''' </remarks>
    Public Const DDF_FARA_SECTIUNE_A As String = "DDF_FARA_SECTIUNE_A"
    ''' <summary>
    ''' The DC's CodFiscal is absent from the registry and the override box is blank.
    ''' </summary>
    ''' <remarks>
    ''' D15: BLOCANT rather than "no statements matched". Matching nothing would look
    ''' exactly like "this file has no extrase for us", which is the wrong answer arrived
    ''' at without a word.
    ''' </remarks>
    Public Const COD_FISCAL_LIPSA As String = "COD_FISCAL_LIPSA"
    ''' <summary>
    ''' A parent table in the write set cannot record the key its children are checked on.
    ''' </summary>
    ''' <remarks>
    ''' Decision D13, and it exists because of a defect that ran silently:
    ''' <c>PrimaryKeyColumn</c> returns Nothing for a multi-column primary key, so
    ''' <c>FX_DDF</c> (then <c>PRIMARY KEY (IDDF, CUAL)</c>) recorded nothing,
    ''' <c>WrittenKeys.Tracks("FX_DDF")</c> stayed False, and the first line of
    ''' <c>ParentsTravelled</c> dropped the FX_DDF link for EVERY child pointing at it.
    ''' The gate would have caught that before the first row was written. The silence was
    ''' the defect, not the Nothing.
    ''' </remarks>
    Public Const CHEIE_PARINTE_NEURMARITA As String = "CHEIE_PARINTE_NEURMARITA"
    Public Const PARTENER_NEREZOLVAT As String = "PARTENER_NEREZOLVAT"
    Public Const UNITATE_LIPSA As String = "UNITATE_LIPSA"
    Public Const BAZA_COMUNA_LIPSA As String = "BAZA_COMUNA_LIPSA"
    Public Const DICTIONAR_COMUN_LIPSA As String = "DICTIONAR_COMUN_LIPSA"
    ''' <summary>
    ''' The target database exists but holds no tables at all.
    ''' </summary>
    ''' <remarks>
    ''' Distinct from TABEL_LIPSA on purpose: one missing table is a schema that has drifted,
    ''' but ZERO tables is a database somebody created empty, and it has its own remedy -
    ''' schema_sync builds the whole structure in one go. The form offers exactly that when
    ''' it sees this kind, so the two must not be confused.
    ''' </remarks>
    Public Const BAZA_FARA_TABELE As String = "BAZA_FARA_TABELE"
    Public Const RANDURI_EXISTENTE As String = "RANDURI_EXISTENTE"
    Public Const ORDINE_TABELE As String = "ORDINE_TABELE"
    Public Const CICLU_TABELE As String = "CICLU_TABELE"
    Public Const FARA_CHEIE_UPSERT As String = "FARA_CHEIE_UPSERT"
    Public Const POTRIVIRE_DUPA_NUME As String = "POTRIVIRE_DUPA_NUME"
    Public Const FISIER_LIPSA As String = "FISIER_LIPSA"
    Public Const PARTENER_NULAT As String = "PARTENER_NULAT"
    Public Const COLOANA_NECORELATA As String = "COLOANA_NECORELATA"

    Public Sub New(kind As String, severity As FindingClass, table As String,
                   column As String, message As String, Optional rowCount As Integer = 0)
        Me.Kind = kind
        Me.Severity = severity
        Me.Table = If(table, String.Empty)
        Me.Column = If(column, String.Empty)
        Me.Message = message
        Me.RowCount = rowCount
    End Sub

    Public ReadOnly Property Kind As String
    Public ReadOnly Property Severity As FindingClass
    Public ReadOnly Property Table As String
    Public ReadOnly Property Column As String
    ''' <summary>Operator-facing Romanian, with real diacritics.</summary>
    Public ReadOnly Property Message As String
    Public ReadOnly Property RowCount As Integer

    Public ReadOnly Property IsBlocking As Boolean
        Get
            Return Severity = FindingClass.Blocant
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim where = Table
        If Column.Length > 0 Then where &= "." & Column
        If where.Length > 0 Then Return $"[{Kind}] {where}: {Message}"
        Return $"[{Kind}] {Message}"
    End Function

End Class

''' <summary>Everything the verifier found, plus the plan it verified.</summary>
Public NotInheritable Class VerificationReport

    Private ReadOnly _findings As New List(Of Finding)()

    ''' <summary>The write order the sort produced, target table names.</summary>
    Public Property WriteOrder As IReadOnlyList(Of String)

    ''' <summary>
    ''' Which rows this verification decided would travel.
    ''' </summary>
    ''' <remarks>
    ''' Carried out of the verification on purpose: the form hands this very object to
    ''' <see cref="TransferRunner"/>, so the selection the operator was shown is the
    ''' selection that gets written. Resolving it twice - once to measure, once to write -
    ''' is how a selection shifts between the two without anybody touching a line of code.
    ''' </remarks>
    Public Property Ownership As OwnershipPlan

    ''' <summary>Access row counts per target table, as measured.</summary>
    Public ReadOnly Property RowCounts As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>The columns that will be written per target table.</summary>
    Public ReadOnly Property WrittenColumns As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

    Public ReadOnly Property Findings As IReadOnlyList(Of Finding)
        Get
            Return _findings
        End Get
    End Property

    Public Sub Add(finding As Finding)
        _findings.Add(finding)
    End Sub

    Public Sub Add(kind As String, severity As FindingClass, table As String,
                   column As String, message As String, Optional rowCount As Integer = 0)
        _findings.Add(New Finding(kind, severity, table, column, message, rowCount))
    End Sub

    ''' <summary>True when nothing blocking was found, so the transfer may start.</summary>
    Public ReadOnly Property CanRun As Boolean
        Get
            Return Not _findings.Any(Function(f) f.IsBlocking)
        End Get
    End Property

    Public ReadOnly Property BlockingCount As Integer
        Get
            Return _findings.Where(Function(f) f.IsBlocking).Count
        End Get
    End Property

    ''' <summary>A short Romanian summary for the status line.</summary>
    Public Function Summary() As String
        Dim blocking = _findings.Where(Function(f) f.Severity = FindingClass.Blocant).Count
        Dim warnings = _findings.Where(Function(f) f.Severity = FindingClass.Atentie).Count
        Dim info = _findings.Where(Function(f) f.Severity = FindingClass.Informativ).Count

        If blocking > 0 Then
            Return $"Verificare: {blocking} constatări BLOCANTE, {warnings} atenționări, " &
                   $"{info} informative. Transferul nu poate porni."
        End If
        Return $"Verificare trecută: {warnings} atenționări, {info} informative. " &
               "Transferul poate porni."
    End Function

End Class
