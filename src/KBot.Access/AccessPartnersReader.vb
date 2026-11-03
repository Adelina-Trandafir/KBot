Option Strict On
Imports System.Collections.Generic
Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Slice 0093-01: reads the table <c>Parteneri</c> of a unit's Access file for «Preia date din Access» of the «Parteneri» window.
''' Read only. The table has no <c>IdUnitate</c> (the unit is the file's); of its three id-shaped columns only <c>CodPartener</c> is taken
''' (docs/MAPARE_NOMENCLATOARE.md section 6). A column missing from a file is read as empty, so an older file still reads.
''' </summary>
Public NotInheritable Class AccessPartnersReader

    Private Sub New()
    End Sub

    ''' <summary>Every row of <c>Parteneri</c>. Throws <see cref="InvalidOperationException"/> (Romanian message) when the table cannot be read.</summary>
    Public Shared Function Read(unitFile As String) As List(Of AccessPartner)
        Try
            Dim k_result As New List(Of AccessPartner)()
            Using k_cn As OleDbConnection = AccessBudgetSender.OpenAccess(unitFile)
                Using k_cmd As New OleDbCommand("SELECT * FROM [Parteneri]", k_cn)
                    Using k_r As OleDbDataReader = k_cmd.ExecuteReader()
                        Dim k_at As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
                        For k_i As Integer = 0 To k_r.FieldCount - 1
                            k_at(k_r.GetName(k_i)) = k_i
                        Next
                        If Not k_at.ContainsKey("CodPartener") Then
                            Throw New InvalidOperationException($"Tabela «Parteneri» din «{unitFile}» nu are coloana «CodPartener».")
                        End If
                        Do While k_r.Read()
                            k_result.Add(New AccessPartner() With {
                                .CodPartener = Text(k_r, k_at, "CodPartener"),
                                .Denumire = Text(k_r, k_at, "DenumirePartener"),
                                .CodFiscal = Text(k_r, k_at, "CodFiscal"),
                                .ContIban = Text(k_r, k_at, "ContIBAN"),
                                .Banca = Text(k_r, k_at, "Banca"),
                                .Adresa = Text(k_r, k_at, "Adresa"),
                                .Tip = Text(k_r, k_at, "Tip"),
                                .Ascuns = Flag(k_r, k_at, "Ascuns")})
                        Loop
                    End Using
                End Using
            End Using
            Return k_result
        Catch ex As OleDbException
            ' A missing table or a locked file: the operator reads the Romanian sentence, the detail goes to the log.
            GlobalErrorLog.Write("AccessPartnersReader.Read", ex)
            Throw New InvalidOperationException($"Tabela «Parteneri» din «{unitFile}» nu a putut fi citită: {ex.Message}", ex)
        Catch ex As Exception
            GlobalErrorLog.Write("AccessPartnersReader.Read", ex)
            Throw
        End Try
    End Function

    ' ---- helpers, reached only through the wrapped method above ----------------------------------------

    Private Shared Function Text(k_r As OleDbDataReader, k_at As Dictionary(Of String, Integer), k_column As String) As String
        Dim k_i As Integer
        If Not k_at.TryGetValue(k_column, k_i) OrElse k_r.IsDBNull(k_i) Then Return String.Empty
        Return Convert.ToString(k_r.GetValue(k_i), CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function Flag(k_r As OleDbDataReader, k_at As Dictionary(Of String, Integer), k_column As String) As Boolean
        Dim k_i As Integer
        If Not k_at.TryGetValue(k_column, k_i) OrElse k_r.IsDBNull(k_i) Then Return False
        Return Convert.ToBoolean(k_r.GetValue(k_i), CultureInfo.InvariantCulture)
    End Function

End Class
