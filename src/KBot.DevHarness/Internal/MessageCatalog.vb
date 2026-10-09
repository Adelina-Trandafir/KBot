Option Strict On
Imports System.IO
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' One message box of the application, as the debug catalog (<c>Config\mesaje_catalog.json</c>)
''' stores it. The file is produced by <c>tools\MessageCatalog\scan.js</c> from the source and
''' edited by <see cref="MessageCatalogForm"/>. <c>Type</c> / <c>Buttons</c> are the enum NAMES of
''' <see cref="KBotMsgKind"/> / <see cref="KBotMsgButtons"/> (readable in the file).
''' </summary>
Public NotInheritable Class MessageEntry

    <JsonPropertyName("id")> Public Property Id As String = String.Empty
    <JsonPropertyName("file")> Public Property File As String = String.Empty
    <JsonPropertyName("line")> Public Property Line As Integer
    <JsonPropertyName("function")> Public Property FunctionName As String = String.Empty
    <JsonPropertyName("api")> Public Property Api As String = String.Empty
    <JsonPropertyName("type")> Public Property Type As String = "Info"
    <JsonPropertyName("buttons")> Public Property Buttons As String = "OK"
    <JsonPropertyName("defaultButton")> Public Property DefaultButton As Integer = 1
    <JsonPropertyName("extraButton")> Public Property ExtraButton As String = String.Empty
    <JsonPropertyName("caption")> Public Property Caption As String = String.Empty
    <JsonPropertyName("text")> Public Property Text As String = String.Empty

    ''' <summary>A heading above the text; the code has none, so any value here is an edit.</summary>
    <JsonPropertyName("header")> Public Property Header As String = String.Empty

    ''' <summary>When the X of the title bar shows: Auto (only if the message has a way out), Show or Hide.</summary>
    <JsonPropertyName("closeButton")> Public Property CloseButton As String = "Auto"

    ''' <summary>The text holds {expressions} the program fills in at run time.</summary>
    <JsonPropertyName("dynamic")> Public Property Dynamic As Boolean

    ''' <summary>The text was followed back from a variable assigned earlier in the function.</summary>
    <JsonPropertyName("fromVariable")> Public Property FromVariable As Boolean

    ''' <summary>What the CODE has (type, buttons, caption, text); rewritten by every scan. The edited
    ''' fields above differ from these exactly when the operator changed the message.</summary>
    <JsonPropertyName("origType")> Public Property OrigType As String = "Info"
    <JsonPropertyName("origButtons")> Public Property OrigButtons As String = "OK"
    <JsonPropertyName("origCaption")> Public Property OrigCaption As String = String.Empty
    <JsonPropertyName("origText")> Public Property OrigText As String = String.Empty

    ''' <summary>Found by a re-scan, not seen by the operator yet.</summary>
    <JsonPropertyName("isNew")> Public Property IsNew As Boolean

    ''' <summary>True when anything the operator can edit differs from the code (then it applies at run time).</summary>
    Public Function IsEdited() As Boolean
        Return ExtraButton.Trim().Length > 0 OrElse Header.Trim().Length > 0 OrElse
            Not String.Equals(CloseButton, "Auto", StringComparison.OrdinalIgnoreCase) OrElse
            Not String.Equals(Text, OrigText, StringComparison.Ordinal) OrElse
            Not String.Equals(Caption, OrigCaption, StringComparison.Ordinal) OrElse
            Not String.Equals(Type, OrigType, StringComparison.OrdinalIgnoreCase) OrElse
            Not String.Equals(Buttons, OrigButtons, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>Puts every edited field back to what the code has.</summary>
    Public Sub ResetToCode()
        Type = OrigType
        Buttons = OrigButtons
        Caption = OrigCaption
        Text = OrigText
        ExtraButton = String.Empty
        Header = String.Empty
        CloseButton = "Auto"
    End Sub

    Public Function Kind() As KBotMsgKind
        Dim k_value As KBotMsgKind
        If Not [Enum].TryParse(Type, True, k_value) Then Return KBotMsgKind.None
        Return k_value
    End Function

    Public Function CloseKind() As KBotMsgClose
        Dim k_value As KBotMsgClose
        If Not [Enum].TryParse(CloseButton, True, k_value) Then Return KBotMsgClose.Auto
        Return k_value
    End Function

    Public Function ButtonSet() As KBotMsgButtons
        Dim k_value As KBotMsgButtons
        If Not [Enum].TryParse(Buttons, True, k_value) Then Return KBotMsgButtons.OK
        Return k_value
    End Function

    Public Function ToSpec() As KBotMessageSpec
        Return New KBotMessageSpec() With {
            .Kind = Kind(),
            .Buttons = ButtonSet(),
            .Caption = Caption,
            .Text = Text,
            .ExtraButton = ExtraButton,
            .Header = Header,
            .CloseButton = CloseKind(),
            .DefaultButton = DefaultButton
        }
    End Function

End Class

''' <summary>The catalog file: every message box call found in the source.</summary>
Public NotInheritable Class MessageCatalog

    <JsonPropertyName("version")> Public Property Version As Integer = 1
    <JsonPropertyName("generated")> Public Property Generated As String = String.Empty
    <JsonPropertyName("messages")> Public Property Messages As New List(Of MessageEntry)()

    Public Const FileName As String = "mesaje_catalog.json"

    Private Shared ReadOnly Options As New JsonSerializerOptions() With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,   ' diacritics stay literal in the file
        .PropertyNameCaseInsensitive = True
    }

    ''' <summary>
    ''' The file to edit: the one in the source tree (<c>src\KBot.App\Config</c>, found by
    ''' walking up from the executable) so a save lands where git sees it; failing that, the copy
    ''' next to the executable.
    ''' </summary>
    Public Shared Function ResolvePath() As String
        Try
            Dim k_dir As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
            While k_dir IsNot Nothing
                Dim k_candidate As String = Path.Combine(k_dir.FullName, "src", "KBot.App", "Config", FileName)
                If File.Exists(k_candidate) Then Return k_candidate
                k_dir = k_dir.Parent
            End While
            Return Path.Combine(AppContext.BaseDirectory, "Config", FileName)
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalog.ResolvePath", ex)
            Throw
        End Try
    End Function

    Public Shared Function Load(k_path As String) As MessageCatalog
        Try
            Dim k_json As String = File.ReadAllText(k_path, System.Text.Encoding.UTF8)
            Dim k_catalog As MessageCatalog = JsonSerializer.Deserialize(Of MessageCatalog)(k_json, Options)
            Return If(k_catalog, New MessageCatalog())
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalog.Load", ex)
            Throw
        End Try
    End Function

    ''' <summary>The repository root (the folder that holds <c>toolsMessageCatalogscan.js</c>), or Nothing.</summary>
    Public Shared Function FindRepositoryRoot() As String
        Try
            Dim k_dir As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
            While k_dir IsNot Nothing
                If File.Exists(Path.Combine(k_dir.FullName, "tools", "MessageCatalog", "scan.js")) Then Return k_dir.FullName
                k_dir = k_dir.Parent
            End While
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalog.FindRepositoryRoot", ex)
            Throw
        End Try
    End Function

    Public Sub Save(k_path As String)
        Try
            Dim k_json As String = JsonSerializer.Serialize(Me, Options)
            ' Written to a side file first, so a failure half-way never leaves a cut catalog.
            Dim k_temp As String = k_path & ".tmp"
            File.WriteAllText(k_temp, k_json & vbLf, New System.Text.UTF8Encoding(False))
            File.Move(k_temp, k_path, True)
        Catch ex As Exception
            GlobalErrorLog.Write("MessageCatalog.Save", ex)
            Throw
        End Try
    End Sub

End Class
