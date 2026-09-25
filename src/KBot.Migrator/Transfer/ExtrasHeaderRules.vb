Imports System.Globalization
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' The unit and classification of one <c>FX_Extrase_H</c> row, computed from its account -
''' the SAME rules the extrase download applies (<c>PYTHON/routes/forexe/extrase.py</c>,
''' <c>_scrie_antet</c> and <c>_Nomenclatoare</c>, port of Access <c>Extrase_H_Add</c>).
''' </summary>
''' <remarks>
''' <para>
''' Operator, 25.09.2026: migrated headers arrived with <c>IdUnitate = 0</c> and no
''' <c>IdClsf</c>, and "when i migrate data, i don't have to manually do anything". The
''' Access values are not read at all; the three columns are recomputed exactly as a fresh
''' download would write them:
''' </para>
''' <list type="number">
''' <item>SS = the first 3 characters of <c>Cont</c> ▸ <c>DefaSSS.SSS</c> ▸ <c>IDSS</c> ▸
''' <c>DefaSS.SS</c> (both in the common database). A <c>500</c> prefix is read on 4
''' characters instead: 5005 = 02A, 5006 = 01A (<see cref="Source500X"/>).</item>
''' <item><c>IdUnitate</c> = <c>Unitati.SursaSector</c> = SS, on the target.</item>
''' <item>Only when <c>CodIBAN</c> is present: ClsfSal = <c>Cont</c> from position 4, 12
''' characters when that character is <c>6</c>, 6 characters when it is <c>3</c>.</item>
''' <item><c>IdClsf</c> = <c>Clasificatii.IDClsf</c> of that unit with that ClsfSal (the
''' lowest one, with a warning, when there are several).</item>
''' <item>No such row ▸ <c>IdClsfV</c> = <c>Clasificatii_Venituri</c> on
''' Capitol + SubCapitol + Paragraf = ClsfSal (no unit filter: the table has none).</item>
''' </list>
''' <para>
''' Nothing here stops the run - the download does not either. A miss leaves the column
''' NULL and adds a Romanian warning to <see cref="Warnings"/>, which the runner logs.
''' </para>
''' <para>
''' The lookups run on the run's own connection and transaction, lazily, at the first
''' header - by then <c>Unitati</c>, <c>Clasificatii</c> and <c>Clasificatii_Venituri</c>
''' of this run are already written (the runner checks that order) and are visible to it.
''' </para>
''' </remarks>
Public NotInheritable Class ExtrasHeaderRules

    ''' <summary>The Access table these rules apply to.</summary>
    Public Const TableName As String = "FX_Extrase_H"

    Private Const ErrNoSuchTable As Integer = 1146

    Private ReadOnly _cn As MySqlConnection
    Private ReadOnly _transaction As MySqlTransaction
    Private ReadOnly _commonDatabase As String

    Private _loaded As Boolean
    ''' <summary>SSS ▸ SS, or Nothing when DefaSS / DefaSSS are missing.</summary>
    Private _ssBySss As Dictionary(Of String, String)
    ''' <summary>SursaSector ▸ IdUnitate.</summary>
    Private _unitBySs As Dictionary(Of String, Integer)
    ''' <summary>IdUnitate ▸ (ClsfSal ▸ IDClsf), loaded per unit.</summary>
    Private ReadOnly _clsfByUnit As New Dictionary(Of Integer, Dictionary(Of String, Integer))()
    ''' <summary>Capitol+SubCapitol+Paragraf ▸ IdClsfV, or Nothing when the table is missing.</summary>
    Private _clsfV As Dictionary(Of String, Integer)
    Private _clsfVLoaded As Boolean

    Private ReadOnly _warnings As New List(Of String)()
    Private ReadOnly _warned As New HashSet(Of String)(StringComparer.Ordinal)

    Public Sub New(cn As MySqlConnection, transaction As MySqlTransaction, commonDatabase As String)
        If cn Is Nothing Then Throw New ArgumentNullException(NameOf(cn))
        _cn = cn
        _transaction = transaction
        _commonDatabase = commonDatabase
    End Sub

    ''' <summary>Every distinct warning raised so far, in order.</summary>
    Public ReadOnly Property Warnings As IReadOnlyList(Of String)
        Get
            Return _warnings
        End Get
    End Property

    ''' <summary>Headers given a unit / an IdClsf / an IdClsfV / nothing at all.</summary>
    Public Property WithUnit As Integer
    Public Property WithClsf As Integer
    Public Property WithClsfV As Integer
    Public Property WithoutUnit As Integer

    ' ---- the pure part (tested) ----------------------------------------------------------

    ''' <summary>
    ''' The 4-character sources behind a <c>500</c> account prefix (operator, 25.09.2026):
    ''' 5005 = 02A, 5006 = 01A. They get a unit, never a classification. Mirrors
    ''' <c>SURSA_500X</c> in <c>extrase.py</c>.
    ''' </summary>
    Public Shared ReadOnly Source500X As IReadOnlyDictionary(Of String, String) =
        New Dictionary(Of String, String)(StringComparer.Ordinal) From {
            {"5005", "02A"},
            {"5006", "01A"}
        }

    Private Shared Function Left4(text As String) As String
        Return If(text.Length <= 4, text, text.Substring(0, 4))
    End Function

    ''' <summary>The first 3 characters of the account (fewer when it is shorter).</summary>
    Public Shared Function SourcePrefix(cont As String) As String
        Dim text = If(cont, String.Empty)
        Return If(text.Length <= 3, text, text.Substring(0, 3))
    End Function

    ''' <summary>
    ''' The ClsfSal cut from the account, or empty. Only with an IBAN - the Access condition.
    ''' </summary>
    Public Shared Function ClsfSalFrom(cont As String, codIban As String) As String
        If String.IsNullOrEmpty(codIban) Then Return String.Empty
        Dim text = If(cont, String.Empty)
        If text.Length < 4 Then Return String.Empty
        Select Case text(3)
            Case "6"c : Return Slice(text, 3, 12)
            Case "3"c : Return Slice(text, 3, 6)
            Case Else : Return String.Empty
        End Select
    End Function

    Private Shared Function Slice(text As String, start As Integer, length As Integer) As String
        Return text.Substring(start, Math.Min(length, text.Length - start))
    End Function

    ' ---- one header -----------------------------------------------------------------------

    ''' <summary>
    ''' The three values for one header. Each is an Integer or <see cref="DBNull.Value"/>.
    ''' </summary>
    Public Function Resolve(cont As String, codIban As String) As (IdUnitate As Object, IdClsf As Object, IdClsfV As Object)
        Try
            EnsureLoaded()
            cont = If(cont, String.Empty).Trim()
            codIban = If(codIban, String.Empty).Trim()

            Dim unit As Integer? = Nothing
            If _ssBySss IsNot Nothing AndAlso cont.Length > 0 Then
                Dim prefix = SourcePrefix(cont)
                Dim ss As String = Nothing
                Dim known = If(prefix = "500",
                               Source500X.TryGetValue(Left4(cont), ss),
                               _ssBySss.TryGetValue(prefix, ss))
                If known Then
                    Dim found As Integer
                    If _unitBySs.TryGetValue(ss, found) Then
                        unit = found
                    Else
                        Warn($"Sursa-sector «{ss}» (contul {cont}) nu are nicio unitate în «Unitati».")
                    End If
                Else
                    Warn($"Sursa «{prefix}» nu a fost găsită pentru contul {cont}.")
                End If
            End If

            Dim clsfSal = ClsfSalFrom(cont, codIban)
            Dim idClsf As Integer? = Nothing
            Dim idClsfV As Integer? = Nothing
            If unit.HasValue AndAlso clsfSal.Length > 0 Then
                idClsf = ClsfFor(unit.Value, clsfSal)
                If Not idClsf.HasValue Then idClsfV = ClsfVFor(clsfSal)
            End If

            If unit.HasValue Then WithUnit += 1 Else WithoutUnit += 1
            If idClsf.HasValue Then WithClsf += 1
            If idClsfV.HasValue Then WithClsfV += 1

            Return (Box(unit), Box(idClsf), Box(idClsfV))
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasHeaderRules.Resolve", ex)
            Throw
        End Try
    End Function

    Private Shared Function Box(value As Integer?) As Object
        Return If(value.HasValue, CObj(value.Value), DBNull.Value)
    End Function

    ' ---- lookups --------------------------------------------------------------------------

    Private Sub EnsureLoaded()
        If _loaded Then Return
        _loaded = True

        Dim common = TargetServer.Quote(_commonDatabase)
        Try
            _ssBySss = New Dictionary(Of String, String)(StringComparer.Ordinal)
            Using cmd = Command(
                $"SELECT S3.SSS, S.SS FROM {common}.`DefaSS` S " &
                $"INNER JOIN {common}.`DefaSSS` S3 ON S.IDSS = S3.IDSS")
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        _ssBySss(Verifier.AsText(reader.GetValue(0))) = Verifier.AsText(reader.GetValue(1))
                    End While
                End Using
            End Using
        Catch ex As MySqlException When ex.Number = ErrNoSuchTable
            _ssBySss = Nothing
            Warn($"Nomenclatorul «{_commonDatabase}.DefaSS / {_commonDatabase}.DefaSSS» nu există " &
                 "pe acest server, deci antetele extraselor rămân fără unitate și clasificație.")
        End Try

        ' First unit per SS by IdUnitate; a second one is named, never picked silently.
        _unitBySs = New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        Using cmd = Command("SELECT SursaSector, IdUnitate FROM `Unitati` ORDER BY IdUnitate")
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim ss = Verifier.AsText(reader.GetValue(0))
                    Dim unit = Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture)
                    If _unitBySs.ContainsKey(ss) Then
                        Warn($"Sursa-sector «{ss}» apare la mai multe unități în «Unitati»; " &
                             $"s-a folosit prima ({_unitBySs(ss)}).")
                        Continue While
                    End If
                    _unitBySs(ss) = unit
                End While
            End Using
        End Using
    End Sub

    Private Function ClsfFor(unit As Integer, clsfSal As String) As Integer?
        Dim map As Dictionary(Of String, Integer) = Nothing
        If Not _clsfByUnit.TryGetValue(unit, map) Then
            map = New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Using cmd = Command("SELECT ClsfSal, IDClsf FROM `Clasificatii` WHERE IdUnitate = @unit ORDER BY IDClsf")
                cmd.Parameters.AddWithValue("@unit", unit)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        If reader.IsDBNull(0) Then Continue While
                        Dim sal = Verifier.AsText(reader.GetValue(0))
                        Dim id = Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture)
                        If map.ContainsKey(sal) Then
                            Warn($"Clasificația «{sal}» apare de mai multe ori la unitatea {unit}; " &
                                 $"s-a folosit prima (id {map(sal)}).")
                            Continue While
                        End If
                        map(sal) = id
                    End While
                End Using
            End Using
            _clsfByUnit(unit) = map
        End If

        Dim found As Integer
        If map.TryGetValue(clsfSal, found) Then Return found
        Return Nothing
    End Function

    Private Function ClsfVFor(clsfSal As String) As Integer?
        If Not _clsfVLoaded Then
            _clsfVLoaded = True
            Try
                _clsfV = New Dictionary(Of String, Integer)(StringComparer.Ordinal)
                Using cmd = Command(
                    "SELECT CONCAT_WS('', Capitol, SubCapitol, Paragraf), IdClsfV FROM `Clasificatii_Venituri`")
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim key = Verifier.AsText(reader.GetValue(0))
                            If Not _clsfV.ContainsKey(key) Then
                                _clsfV(key) = Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture)
                            End If
                        End While
                    End Using
                End Using
            Catch ex As MySqlException When ex.Number = ErrNoSuchTable
                _clsfV = Nothing
                Warn("Tabela «Clasificatii_Venituri» nu există în baza țintă, deci clasificația " &
                     "de venituri (IdClsfV) a antetelor rămâne necompletată.")
            End Try
        End If

        If _clsfV Is Nothing Then Return Nothing
        Dim found As Integer
        If _clsfV.TryGetValue(clsfSal, found) Then Return found
        Return Nothing
    End Function

    Private Function Command(sql As String) As MySqlCommand
        Return New MySqlCommand(sql, _cn, _transaction)
    End Function

    Private Sub Warn(message As String)
        If _warned.Add(message) Then _warnings.Add(message)
    End Sub

End Class
