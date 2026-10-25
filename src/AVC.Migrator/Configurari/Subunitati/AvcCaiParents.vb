Imports System.Globalization
Imports KBot.Common
Imports KBot.Migrator

''' <summary>
''' The parent links of <c>cale.accdb</c> ▸ <c>cai</c>: <c>IdUnitateParinte</c> names the unit whose classifications and budget a
''' unit reads (every other datum is the unit's own). KBot.Migrator's <c>CaiUnit</c> does not carry it and stays untouched, so
''' this reads the column on its own.
''' </summary>
Friend NotInheritable Class AvcCaiParents

    Private Sub New()
    End Sub

    ''' <summary>
    ''' IdUnitate ▸ IdUnitate of its parent, only for the units that have one (NULL and 0 mean "no parent"). A registry without the
    ''' column gives an empty map. Access I/O: logs and rethrows.
    ''' </summary>
    Public Shared Function Read(k_calePath As String, k_password As String) As Dictionary(Of Integer, Integer)
        Try
            Dim k_result As New Dictionary(Of Integer, Integer)()
            Using k_cn = AccessProvider.Open(k_calePath, k_password)
                Dim k_table = AccessSchema.ResolveTableName(k_cn, "cai")
                If k_table Is Nothing Then Return k_result
                Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
                    While k_reader.Read()
                        Dim k_id = ToInt(k_reader.ValueOrMissing("IdUnitate"))
                        Dim k_parent = ToInt(k_reader.ValueOrMissing("IdUnitateParinte"))
                        If k_id > 0 AndAlso k_parent > 0 AndAlso k_parent <> k_id Then k_result(k_id) = k_parent
                    End While
                End Using
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcCaiParents.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Function ToInt(k_value As Object) As Integer
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return 0
        Dim k_number As Integer
        Return If(Integer.TryParse(Convert.ToString(k_value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, k_number), k_number, 0)
    End Function

End Class
