Imports KBot.Common

''' <summary>
''' How many rows each catalogue table holds in the Access files of the ticked units - the
''' «Rânduri Access» column of the table grid.
''' </summary>
''' <remarks>
''' <para>
''' A RAW count: every row of the table in every distinct file the selection reads, before
''' any unit or subtree filter. The shared FOREXE file holds the rows of every unit of the
''' DC (and sometimes of other DCs), so its count is larger than what the transfer writes;
''' the transfer's own result says how many travelled.
''' </para>
''' <para>
''' Files are the same ones the transfer opens (<c>TransferRunner.FilePasses</c>): the
''' nomenclator file of each ticked unit, and each distinct FOREXE file once.
''' </para>
''' </remarks>
Public NotInheritable Class AccessRowCounter

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Target table ▸ row count. A table absent from the result had no file to read;
    ''' <c>Unitati</c> (built from the registry) counts the ticked units.
    ''' </summary>
    Public Shared Function Count(request As TransferRequest, maps As IEnumerable(Of TableMap),
                                 cancel As Threading.CancellationToken) As Dictionary(Of String, Long)
        Try
            Dim counts As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
            Dim files = FilesBySource(request)

            For Each map In maps
                cancel.ThrowIfCancellationRequested()
                If map.Source = SourceFile.Derived Then
                    counts(map.TargetTable) = request.Units.Count
                    Continue For
                End If

                Dim paths As List(Of String) = Nothing
                If Not files.TryGetValue(map.Source, paths) OrElse paths.Count = 0 Then Continue For
                Dim password = If(map.Source = SourceFile.UnitFile,
                                  request.UnitFilePassword, request.ForexeFilePassword)

                Dim total As Long = 0
                For Each path In paths
                    Using cn = AccessProvider.Open(path, password)
                        Dim realName = AccessSchema.ResolveTableName(cn, map.AccessTable)
                        If realName IsNot Nothing Then total += AccessSchema.CountRows(cn, realName)
                    End Using
                Next
                counts(map.TargetTable) = total
            Next

            Return counts

        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AccessRowCounter.Count", ex)
            Throw
        End Try
    End Function

    ''' <summary>The distinct existing files per source, in the ticked units' order.</summary>
    Private Shared Function FilesBySource(request As TransferRequest) As Dictionary(Of SourceFile, List(Of String))
        Dim result As New Dictionary(Of SourceFile, List(Of String)) From {
            {SourceFile.UnitFile, New List(Of String)()},
            {SourceFile.ForexeFile, New List(Of String)()}
        }
        For Each unit In request.Units
            AddIfPresent(result(SourceFile.UnitFile), unit.UnitFilePath)
            AddIfPresent(result(SourceFile.ForexeFile), unit.ForexeFilePath)
        Next
        Return result
    End Function

    Private Shared Sub AddIfPresent(list As List(Of String), path As String)
        If String.IsNullOrEmpty(path) OrElse Not IO.File.Exists(path) Then Return
        If list.Contains(path, StringComparer.OrdinalIgnoreCase) Then Return
        list.Add(path)
    End Sub

End Class
