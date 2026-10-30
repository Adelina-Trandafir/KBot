Option Strict On
Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO

''' <summary>One active fixed asset of the old Access register (mf.mdb), as the server import takes it.</summary>
Public Class MfAsset
    Public Property Code As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property UnitId As Integer
    Public Property EntryDate As Date
    Public Property PurchaseDate As Date
    Public Property Value As Decimal
    Public Property Dnf As Integer
    Public Property Df As Integer
    Public Property Cc As String = String.Empty
End Class

''' <summary>
''' Reads the fixed assets of the old Access register «Mijloace Fixe» that are not marked deleted (STERS = False).
''' The file is copied to a temporary name first, so the original is never opened and no lock file appears beside it.
''' Nothing else of the old file is read: no depreciation history, no exits, no classifications table.
''' </summary>
Public NotInheritable Class MfAccess

    Private Shared ReadOnly Providers As String() = {"Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0"}

    Private Sub New()
    End Sub

    Public Shared Function ReadActive(k_path As String) As List(Of MfAsset)
        Try
            If String.IsNullOrWhiteSpace(k_path) OrElse Not File.Exists(k_path) Then
                Throw New AdminException("Fișierul Access nu există: " & k_path)
            End If
            Dim k_copy As String = Path.Combine(Path.GetTempPath(), "kbotadmin_mf_" & Guid.NewGuid().ToString("N") & ".mdb")
            File.Copy(k_path, k_copy)
            Try
                Return ReadFrom(k_copy)
            Finally
                DeleteQuietly(k_copy)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("MfAccess.ReadActive", ex)
            Throw
        End Try
    End Function

    Private Shared Function ReadFrom(k_file As String) As List(Of MfAsset)
        Dim k_lastError As String = String.Empty
        For Each k_provider As String In Providers
            Try
                Using k_cn As New OleDbConnection($"Provider={k_provider};Data Source={k_file};Mode=Read")
                    k_cn.Open()
                    Return Query(k_cn)
                End Using
            Catch ex As InvalidOperationException
                ' This provider is not installed; try the next one.
                k_lastError = ex.Message
            End Try
        Next
        Throw New AdminException("Fișierul Access nu poate fi citit: lipsește furnizorul ACE (Access Database Engine pe 64 de biți). " & k_lastError)
    End Function

    Private Shared Function Query(k_cn As OleDbConnection) As List(Of MfAsset)
        Dim k_list As New List(Of MfAsset)()
        Using k_cmd As New OleDbCommand("SELECT CodProdus, DenumireProdus, IdUnitate, DataIntrare, DataAchizitie, DNF, DF, ValoareIntrare, Categorie " &
                                        "FROM [Mijloace Fixe] WHERE STERS = False ORDER BY CodProdus", k_cn)
            Using k_reader As OleDbDataReader = k_cmd.ExecuteReader()
                Do While k_reader.Read()
                    Dim k_asset As New MfAsset With {
                        .Code = Text(k_reader, 0),
                        .Name = Text(k_reader, 1),
                        .UnitId = CInt(Number(k_reader, 2)),
                        .EntryDate = DateOf(k_reader, 3),
                        .PurchaseDate = DateOf(k_reader, 4),
                        .Dnf = CInt(Number(k_reader, 5)),
                        .Df = CInt(Number(k_reader, 6)),
                        .Value = Math.Round(Number(k_reader, 7), 4),
                        .Cc = Text(k_reader, 8)
                    }
                    k_list.Add(k_asset)
                Loop
            End Using
        End Using
        Return k_list
    End Function

    Private Shared Function Text(k_reader As OleDbDataReader, k_index As Integer) As String
        Return If(k_reader.IsDBNull(k_index), String.Empty, Convert.ToString(k_reader.GetValue(k_index), CultureInfo.InvariantCulture).Trim())
    End Function

    Private Shared Function Number(k_reader As OleDbDataReader, k_index As Integer) As Decimal
        Return If(k_reader.IsDBNull(k_index), 0D, Convert.ToDecimal(k_reader.GetValue(k_index), CultureInfo.InvariantCulture))
    End Function

    Private Shared Function DateOf(k_reader As OleDbDataReader, k_index As Integer) As Date
        Return If(k_reader.IsDBNull(k_index), Date.MinValue, Convert.ToDateTime(k_reader.GetValue(k_index), CultureInfo.InvariantCulture).Date)
    End Function

    Private Shared Sub DeleteQuietly(k_file As String)
        Try
            File.Delete(k_file)
        Catch ex As IOException
            GlobalErrorLog.Write("MfAccess.DeleteQuietly", ex)
        End Try
    End Sub

End Class
