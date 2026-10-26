Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports KBot.Common

''' <summary>A source file kept from one run to the next: its path and whether it was ticked.</summary>
Public NotInheritable Class VrcSavedFile

    Public Property Path As String = String.Empty
    Public Property Selected As Boolean

End Class

''' <summary>
''' A row of the plan grid as the operator left it. Identified by the file and the type the tool proposed for it
''' (a file can feed two rows: Alimente and Materiale), so the same file is recognised after the next reading.
''' </summary>
Public NotInheritable Class VrcSavedPlanRow

    Public Property Path As String = String.Empty
    ''' <summary>The type proposed by the tool for this row (the key together with the path).</summary>
    Public Property ProposedType As String = String.Empty
    Public Property Year As String = String.Empty
    Public Property Gestiune As String = String.Empty
    Public Property GestiuneType As String = String.Empty
    Public Property Subunit As String = String.Empty

End Class

''' <summary>
''' What the tool remembers from one run to the next, next to the executable: the server (host, port, user), the files and which are
''' ticked, DC and the plan rows (year, subunit, gestiune, type). Never a password. Same server defaults as ADE.Migrator.
''' </summary>
Public NotInheritable Class VrcSettings

    Private Const FileName As String = "vrc-migrator-settings.json"

    Public Property Host As String = "89.33.25.34"
    Public Property Port As Integer = 3306
    Public Property User As String = "Admin"
    Public Property Dc As String = String.Empty
    ''' <summary>The cale.accdb of VERCON last loaded (it proposes the subunit of each file); loaded again at start when it still exists.</summary>
    Public Property CalePath As String = String.Empty
    ''' <summary>Folder of the last «Alege folder…» / «+ Adaugă Baza Date…» pick, where the next dialog opens.</summary>
    Public Property LastFolder As String = String.Empty
    Public Property Files As New List(Of VrcSavedFile)()
    Public Property PlanRows As New List(Of VrcSavedPlanRow)()
    ''' <summary>Partners prepared from ANAF (with the sector-sources ticked for each), by DC (see <see cref="DcKey"/>): they survive a new list of files and a new run.</summary>
    Public Property PreparedPartners As New Dictionary(Of String, List(Of VrcNewPartner))()

    ''' <summary>The key of a DC in <see cref="PreparedPartners"/>: trimmed, upper case.</summary>
    Public Shared Function DcKey(k_dc As String) As String
        Return If(k_dc, String.Empty).Trim().ToUpperInvariant()
    End Function

    Private Shared Function FilePath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileName)
    End Function

    Public Shared Function Load() As VrcSettings
        Try
            If Not File.Exists(FilePath()) Then Return New VrcSettings()
            Return If(JsonSerializer.Deserialize(Of VrcSettings)(File.ReadAllText(FilePath(), Encoding.UTF8)), New VrcSettings())
        Catch ex As Exception
            GlobalErrorLog.Write("VrcSettings.Load", ex)   ' a damaged file must not stop the tool: start from the defaults
            Return New VrcSettings()
        End Try
    End Function

    Public Sub Save()
        Try
            File.WriteAllText(FilePath(), JsonSerializer.Serialize(Me, New JsonSerializerOptions() With {.WriteIndented = True}), Encoding.UTF8)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcSettings.Save", ex)   ' remembering is a convenience; the run goes on
        End Try
    End Sub

End Class
