Imports System.Globalization
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

' Slice ME01-36 (migration): the partners that the templates (operations and salaries) name by a fixed code and that «Parteneri» does not have.

''' <summary>A partner that a template names and the target lacks: who it is (Access, when Access has it), where it is used, and its new id once it is created.</summary>
Friend NotInheritable Class AvcEnsuredPartner
    Public Property IdUnitate As Integer
    Public Property Code As String = String.Empty
    ''' <summary>The partner of the Access unit file; Nothing when the code is not in the Access <c>Parteneri</c> either.</summary>
    Public Property Access As AvcPartnerRow
    Public Property Denumire As String = String.Empty
    Public Property IdPartener As Long?
    Public ReadOnly Property Uses As New List(Of String)()
    ''' <summary>What asked for the partner (a template of the step); the step says which of them are written.</summary>
    Public ReadOnly Property Owners As New List(Of Object)()
End Class

''' <summary>
''' The partners that the templates need and that do not exist in «Parteneri»: they are CREATED (operator's rule), marked as NOT suppliers.
''' </summary>
''' <remarks>
''' <para>
''' «Not a supplier» = <c>Tip</c> «0» (the supplier lists of K-BOT show only <c>Tip = '1'</c>, see <c>routes/forexe/parteneri_edit.py</c>, slice 0093) and
''' <c>Provenienta</c> «AVC_NEFURNIZOR», so the mention stays on the row. Name, tax code, IBAN, bank and address come from the Access <c>Parteneri</c> of the unit when it has the
''' code; otherwise the row is only the code with the name «Partener &lt;cod&gt;».
''' </para>
''' <para>
''' Two phases, so a partner is never created for a template that is left out for another reason: the step resolves ONCE with this object collecting the missing partners
''' (nothing is written), then <see cref="CreateNeeded"/> creates the ones a written template needs, then the step resolves again and finds them.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcPartnerEnsure

    Public Const NotSupplierTip As String = "0"
    Public Const NotSupplierSource As String = "AVC_NEFURNIZOR"

    Private ReadOnly _access As Dictionary(Of String, AvcPartnerRow)

    Public Sub New(k_access As Dictionary(Of String, AvcPartnerRow))
        _access = If(k_access, New Dictionary(Of String, AvcPartnerRow)())
    End Sub

    ''' <summary>"IdUnitate|CodPartener" ▸ the partner.</summary>
    Public ReadOnly Property Items As New SortedDictionary(Of String, AvcEnsuredPartner)(StringComparer.Ordinal)

    ''' <summary>Registers (or finds) the missing partner; true always: a missing partner is not a problem, it is created. <paramref name="k_id"/> is its id once created.</summary>
    Public Function Ensure(k_unit As Integer, k_code As String, k_use As String, k_owner As Object, ByRef k_id As Long?) As Boolean
        Dim k_key As String = k_unit.ToString(CultureInfo.InvariantCulture) & "|" & k_code
        Dim k_item As AvcEnsuredPartner = Nothing
        If Not Items.TryGetValue(k_key, k_item) Then
            Dim k_row As AvcPartnerRow = Nothing
            _access.TryGetValue(k_key, k_row)
            k_item = New AvcEnsuredPartner() With {
                .IdUnitate = k_unit, .Code = k_code, .Access = k_row,
                .Denumire = If(k_row IsNot Nothing AndAlso k_row.Denumire.Length > 0, k_row.Denumire, "Partener " & k_code)}
            Items(k_key) = k_item
        End If
        k_item.Uses.Add(k_use)
        k_item.Owners.Add(k_owner)
        k_id = k_item.IdPartener
        Return True
    End Function

    ''' <summary>Creates, in the transaction, the partners that at least one written template needs. Server I/O: logs and rethrows.</summary>
    Public Sub CreateNeeded(k_cn As MySqlConnection, k_transaction As MySqlTransaction, k_isWritten As Func(Of Object, Boolean), k_cancel As CancellationToken)
        Try
            For Each k_item In Items.Values
                k_cancel.ThrowIfCancellationRequested()
                If k_item.IdPartener.HasValue OrElse Not k_item.Owners.Any(k_isWritten) Then Continue For
                Dim k_row As AvcPartnerRow = k_item.Access
                Using k_cmd As New MySqlCommand("INSERT INTO `Parteneri` (`IdUnitate`, `CodPartener`, `DenumirePartener`, `CodFiscal`, `ContIBAN`, `Banca`, `Adresa`, `Tip`, `Ascuns`, `Provenienta`) " &
                                                "VALUES (@unit, @cod, @den, @cf, @iban, @banca, @adresa, @tip, @ascuns, @prov)", k_cn, k_transaction)
                    k_cmd.Parameters.AddWithValue("@unit", k_item.IdUnitate)
                    k_cmd.Parameters.AddWithValue("@cod", k_item.Code)
                    k_cmd.Parameters.AddWithValue("@den", k_item.Denumire)
                    k_cmd.Parameters.AddWithValue("@cf", NullIfEmpty(If(k_row Is Nothing, String.Empty, k_row.CodFiscal)))
                    k_cmd.Parameters.AddWithValue("@iban", NullIfEmpty(If(k_row Is Nothing, String.Empty, k_row.ContIBAN)))
                    k_cmd.Parameters.AddWithValue("@banca", NullIfEmpty(If(k_row Is Nothing, String.Empty, k_row.Banca)))
                    k_cmd.Parameters.AddWithValue("@adresa", NullIfEmpty(If(k_row Is Nothing, String.Empty, k_row.Adresa)))
                    k_cmd.Parameters.AddWithValue("@tip", NotSupplierTip)
                    k_cmd.Parameters.AddWithValue("@ascuns", 0)
                    k_cmd.Parameters.AddWithValue("@prov", NotSupplierSource)
                    k_cmd.ExecuteNonQuery()
                    k_item.IdPartener = k_cmd.LastInsertedId
                End Using
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPartnerEnsure.CreateNeeded", ex)
            Throw
        End Try
    End Sub

    Private Shared Function NullIfEmpty(k_value As String) As Object
        Return If(String.IsNullOrWhiteSpace(k_value), CType(DBNull.Value, Object), k_value)
    End Function

    Private Shared Function Describe(k_item As AvcEnsuredPartner, k_context As AvcContext, k_created As Boolean) As String
        Dim k_who As String = $"cod {k_item.Code} «{k_item.Denumire}»" &
                              If(k_item.Access IsNot Nothing AndAlso k_item.Access.CodFiscal.Length > 0, $", cod fiscal «{k_item.Access.CodFiscal}»", String.Empty)
        Dim k_source As String = If(k_item.Access Is Nothing, "nu există în tabelul Parteneri din Access: se creează doar cu codul și denumirea «Partener " & k_item.Code & "»", "date din Access")
        Return $"Partener {If(k_created, "creat", "nou, de creat")} în Parteneri (unitatea {k_context.UnitLabel(k_item.IdUnitate)}), NU este furnizor (Tip {NotSupplierTip}, nu apare în listele de furnizori): " &
               $"{k_who} ({k_source}); folosit în {k_item.Uses.Count} locuri din șabloane" &
               If(k_created AndAlso k_item.IdPartener.HasValue, $"; IdPartener {k_item.IdPartener.Value.ToString(CultureInfo.InvariantCulture)}.", ".")
    End Function

    ''' <summary>Verification: one warning for every partner that a written template needs and that the write will create.</summary>
    Public Sub AddFindings(k_findings As List(Of Finding), k_context As AvcContext, k_area As String, k_isWritten As Func(Of Object, Boolean))
        For Each k_item In Items.Values
            If k_item.Owners.Any(k_isWritten) Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Atentie, k_area, String.Empty, Describe(k_item, k_context, False)))
            End If
        Next
    End Sub

    ''' <summary>Write: tells the progress and the SQL dump which partners were created.</summary>
    Public Sub ReportCreated(k_context As AvcContext, k_dump As SqlDumpWriter, k_progress As IProgress(Of String))
        For Each k_item In Items.Values.Where(Function(k_i) k_i.IdPartener.HasValue)
            Dim k_text As String = Describe(k_item, k_context, True)
            k_progress.Report(k_text)
            k_dump.WriteComment("Parteneri", k_text)
        Next
    End Sub

    Public Function CreatedCount() As Integer
        Return Items.Values.Where(Function(k_i) k_i.IdPartener.HasValue).Count()
    End Function

    ''' <summary>The rows written by a step plus the partners created for it.</summary>
    Public Function AppendWritten(k_written As List(Of AvcWritten)) As List(Of AvcWritten)
        Dim k_count As Integer = CreatedCount()
        If k_count > 0 Then k_written.Add(New AvcWritten("Parteneri", k_count))
        Return k_written
    End Function

End Class
