Imports System.Globalization
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>A sector-source of the DC with the unit that stands for it: the lowest IdUnitate of that SS (the K-BOT window does the same for a new partner).</summary>
Public NotInheritable Class VrcSectorSource

    Public Property SS As String = String.Empty
    Public Property IdUnitate As Long

End Class

''' <summary>A supplier to be added to Parteneri: tax code and data from ANAF, and the sector-sources the operator ticked for it.</summary>
Public NotInheritable Class VrcNewPartner

    Public Property TaxCode As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Address As String = String.Empty
    Public Property SectorSources As New List(Of String)()

End Class

''' <summary>
''' Adds the missing suppliers to Parteneri of the DC, on the transaction of the transfer (never on its own). A partner is one row per tax code in the DC (the
''' K-BOT rule), so ticking several SS does not create several partners: the unit is the lowest IdUnitate among the ticked SS,
''' and CodPartener is the next free numeric code (three digits at least) among the partners of that SS, as ParteneriForm does.
''' </summary>
Public NotInheritable Class VrcPartnerStore

    Private Sub New()
    End Sub

    ''' <summary>The SS of the DC with their lowest IdUnitate, by SS.</summary>
    Public Shared Function SectorSources(k_server As TargetServer, k_dc As String) As List(Of VrcSectorSource)
        Try
            Using k_cn = k_server.Open(k_dc)
                Return ReadSectorSources(k_cn, Nothing)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPartnerStore.SectorSources", ex)
            Throw
        End Try
    End Function

    Private Shared Function ReadSectorSources(k_cn As MySqlConnection, k_tx As MySqlTransaction) As List(Of VrcSectorSource)
        Dim k_list As New List(Of VrcSectorSource)()
        Using k_cmd As New MySqlCommand("SELECT SursaSector, MIN(IdUnitate) FROM Unitati GROUP BY SursaSector ORDER BY SursaSector", k_cn, k_tx)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_list.Add(New VrcSectorSource() With {.SS = k_reader.GetString(0), .IdUnitate = k_reader.GetInt64(1)})
                End While
            End Using
        End Using
        Return k_list
    End Function

    ''' <summary>
    ''' One line per supplier: added (with its unit and code) or why it was left out. Runs on the CALLER's connection and transaction and
    ''' never commits: the transfer commits or rolls back the partners together with everything else.
    ''' </summary>
    Public Shared Function AddInTransaction(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_dc As String, k_items As List(Of VrcNewPartner)) As List(Of String)
        Try
            Dim k_report As New List(Of String)()
            Dim k_units = ReadSectorSources(k_cn, k_tx)
            Dim k_existing As New HashSet(Of String)(StringComparer.Ordinal)
            Dim k_highest As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Using k_cmd As New MySqlCommand("SELECT P.CodFiscal, P.CodPartener, U.SursaSector FROM Parteneri P LEFT JOIN Unitati U ON U.IdUnitate = P.IdUnitate", k_cn, k_tx)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        If Not k_reader.IsDBNull(0) Then k_existing.Add(VrcConvert.NormalizeTaxCode(k_reader.GetString(0)))
                        If k_reader.IsDBNull(1) OrElse k_reader.IsDBNull(2) Then Continue While
                        Dim k_number As Integer
                        If Integer.TryParse(k_reader.GetString(1).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, k_number) Then
                            Dim k_ss As String = k_reader.GetString(2)
                            Dim k_old As Integer = 0
                            k_highest.TryGetValue(k_ss, k_old)
                            k_highest(k_ss) = Math.Max(k_old, k_number)
                        End If
                    End While
                End Using
            End Using

            For Each k_item In k_items
                If k_existing.Contains(k_item.TaxCode) Then
                    k_report.Add($"{k_item.TaxCode}: există deja un partener cu acest cod fiscal; nu s-a adăugat.")
                    Continue For
                End If
                Dim k_chosen = k_units.Where(Function(k_u) k_item.SectorSources.Contains(k_u.SS)).OrderBy(Function(k_u) k_u.IdUnitate).FirstOrDefault()
                If k_chosen Is Nothing Then
                    k_report.Add($"{k_item.TaxCode}: nicio sursă-sector bifată (sau fără unitate în baza {k_dc}); nu s-a adăugat.")
                    Continue For
                End If
                Dim k_last As Integer = 0
                k_highest.TryGetValue(k_chosen.SS, k_last)
                Dim k_code As String = (k_last + 1).ToString("000", CultureInfo.InvariantCulture)
                k_highest(k_chosen.SS) = k_last + 1
                Using k_cmd As New MySqlCommand("INSERT INTO Parteneri (IdUnitate, CodPartener, DenumirePartener, CodFiscal, Adresa, Tip, Ascuns, Provenienta) VALUES (@u, @c, @n, @f, @a, '1', 0, 'MIGRARE_VERCON')", k_cn, k_tx)
                    k_cmd.Parameters.AddWithValue("@u", k_chosen.IdUnitate)
                    k_cmd.Parameters.AddWithValue("@c", k_code)
                    k_cmd.Parameters.AddWithValue("@n", Cut(k_item.Name, 255))
                    k_cmd.Parameters.AddWithValue("@f", k_item.TaxCode)
                    k_cmd.Parameters.AddWithValue("@a", Cut(k_item.Address, 255))
                    k_cmd.ExecuteNonQuery()
                End Using
                k_existing.Add(k_item.TaxCode)
                k_report.Add($"{k_item.TaxCode}: adăugat ca {k_code} în unitatea {k_chosen.IdUnitate} ({k_chosen.SS}) — {k_item.Name}.")
            Next
            Return k_report
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPartnerStore.AddInTransaction", ex)
            Throw
        End Try
    End Function

    Private Shared Function Cut(k_text As String, k_length As Integer) As String
        If k_text Is Nothing Then Return String.Empty
        Dim k_trimmed As String = k_text.Trim()
        Return If(k_trimmed.Length <= k_length, k_trimmed, k_trimmed.Substring(0, k_length))
    End Function

End Class
