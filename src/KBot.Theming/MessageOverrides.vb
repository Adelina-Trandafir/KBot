Option Strict On
Imports System.IO
Imports System.Text.Json
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The edited messages of <c>Config\mesaje_catalog.json</c>, looked up by the call that shows them.
'''
''' <para>Only entries the operator CHANGED matter (type, buttons, caption, text or an extra button
''' differ from what the code has, kept next to them as <c>orig*</c>); everything else is left to
''' the call. A call is found by its source file and member (both free from the compiler,
''' <c>KBotMessage</c>'s caller-info parameters) and confirmed by laying the code's template over the
''' text it really produced (<see cref="MessageTemplate"/>) -- so a catalog that went out of step with
''' the code (a line moved, a message reworded) simply stops matching and the call keeps its own text.
''' The line number only breaks ties between two calls in one member.</para>
''' </summary>
Friend NotInheritable Class MessageOverrides

    Private Sub New()
    End Sub

    Private NotInheritable Class Entry
        Public Key As String = String.Empty
        Public Line As Integer
        Public Type As String = String.Empty
        Public Buttons As String = String.Empty
        Public Caption As String = String.Empty
        Public Text As String = String.Empty
        Public Extra As String = String.Empty
        Public Header As String = String.Empty
        Public CloseButton As String = String.Empty
        Public OrigType As String = String.Empty
        Public OrigButtons As String = String.Empty
        Public OrigCaption As String = String.Empty
        Public OrigText As String = String.Empty
    End Class

    Private Shared ReadOnly Gate As New Object()
    Private Shared _index As Dictionary(Of String, List(Of Entry))
    Private Shared _path As String

    ''' <summary>The catalog file; Nothing = <c>&lt;AppDir&gt;\Config\mesaje_catalog.json</c>.</summary>
    Friend Shared Property CatalogPath As String
        Get
            Return If(_path, Path.Combine(AppContext.BaseDirectory, "Config", "mesaje_catalog.json"))
        End Get
        Set(k_value As String)
            _path = k_value
            Reload()
        End Set
    End Property

    ''' <summary>Forgets what was read; the next message reads the file again.</summary>
    Friend Shared Sub Reload()
        SyncLock Gate
            _index = Nothing
        End SyncLock
    End Sub

    ''' <summary>
    ''' Applies the catalog to one call. True when an edited entry matched; the by-ref values then hold
    ''' what to show.
    ''' </summary>
    Friend Shared Function Apply(k_file As String, k_member As String, k_line As Integer,
                                 ByRef k_caption As String, ByRef k_text As String,
                                 ByRef k_buttons As MessageBoxButtons, ByRef k_icon As MessageBoxIcon,
                                 k_extras As MessageExtras) As Boolean
        Dim k_idx As Dictionary(Of String, List(Of Entry)) = Index()
        If k_idx.Count = 0 Then Return False
        Dim k_name As String = If(String.IsNullOrEmpty(k_file), String.Empty, Path.GetFileNameWithoutExtension(k_file))
        Dim k_list As List(Of Entry) = Nothing
        If Not k_idx.TryGetValue(k_name & "." & If(k_member, String.Empty), k_list) Then Return False

        Dim k_best As Entry = Nothing
        Dim k_bestValues As Dictionary(Of String, String) = Nothing
        Dim k_bestGap As Integer = Integer.MaxValue
        For Each k_e As Entry In k_list
            Dim k_values As New Dictionary(Of String, String)(StringComparer.Ordinal)
            If Not MessageTemplate.Match(k_e.OrigText, k_text, k_values) Then Continue For
            If Not MessageTemplate.Match(k_e.OrigCaption, k_caption, k_values) Then Continue For
            Dim k_gap As Integer = If(k_line > 0, Math.Abs(k_e.Line - k_line), 0)
            If k_gap < k_bestGap Then
                k_best = k_e
                k_bestValues = k_values
                k_bestGap = k_gap
            End If
        Next
        If k_best Is Nothing Then Return False

        If Not String.Equals(k_best.Text, k_best.OrigText, StringComparison.Ordinal) Then
            k_text = MessageTemplate.Fill(k_best.Text, k_bestValues)
        End If
        If Not String.Equals(k_best.Caption, k_best.OrigCaption, StringComparison.Ordinal) Then
            k_caption = MessageTemplate.Fill(k_best.Caption, k_bestValues)
        End If
        If Not String.Equals(k_best.Type, k_best.OrigType, StringComparison.OrdinalIgnoreCase) Then
            k_icon = IconOf(k_best.Type)
        End If
        If Not String.Equals(k_best.Buttons, k_best.OrigButtons, StringComparison.OrdinalIgnoreCase) Then
            k_buttons = ButtonsOf(k_best.Buttons, k_buttons)
        End If
        k_extras.ExtraButton = k_best.Extra
        k_extras.Header = MessageTemplate.Fill(k_best.Header, k_bestValues)
        k_extras.CloseButton = If(k_best.CloseButton.Length = 0, "Auto", k_best.CloseButton)
        Return True
    End Function

    Private Shared Function IconOf(k_name As String) As MessageBoxIcon
        Select Case If(k_name, String.Empty).ToLowerInvariant()
            Case "info" : Return MessageBoxIcon.Information
            Case "warning" : Return MessageBoxIcon.Warning
            Case "error" : Return MessageBoxIcon.Error
            Case "question" : Return MessageBoxIcon.Question
            Case Else : Return MessageBoxIcon.None
        End Select
    End Function

    Private Shared Function ButtonsOf(k_name As String, k_keep As MessageBoxButtons) As MessageBoxButtons
        Select Case If(k_name, String.Empty).ToLowerInvariant()
            Case "ok" : Return MessageBoxButtons.OK
            Case "okcancel" : Return MessageBoxButtons.OKCancel
            Case "yesno" : Return MessageBoxButtons.YesNo
            Case "yesnocancel" : Return MessageBoxButtons.YesNoCancel
            Case "retrycancel" : Return MessageBoxButtons.RetryCancel
            Case "abortretryignore" : Return MessageBoxButtons.AbortRetryIgnore
            Case Else : Return k_keep
        End Select
    End Function

    ' ---------------- reading the file ----------------

    Private Shared Function Index() As Dictionary(Of String, List(Of Entry))
        SyncLock Gate
            If _index IsNot Nothing Then Return _index
            _index = New Dictionary(Of String, List(Of Entry))(StringComparer.Ordinal)
            Try
                Dim k_file As String = CatalogPath
                If Not File.Exists(k_file) Then Return _index
                Using k_doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(k_file, System.Text.Encoding.UTF8))
                    Dim k_messages As JsonElement
                    If Not k_doc.RootElement.TryGetProperty("messages", k_messages) Then Return _index
                    For Each k_item As JsonElement In k_messages.EnumerateArray()
                        Dim k_e As Entry = ReadEntry(k_item)
                        If k_e Is Nothing OrElse Not IsEdited(k_e) Then Continue For
                        Dim k_list As List(Of Entry) = Nothing
                        If Not _index.TryGetValue(k_e.Key, k_list) Then
                            k_list = New List(Of Entry)()
                            _index(k_e.Key) = k_list
                        End If
                        k_list.Add(k_e)
                    Next
                End Using
            Catch ex As Exception
                ' A broken catalog must never stop a dialog: log it and run on the code's own texts.
                GlobalErrorLog.Write("MessageOverrides.Index", ex)
                _index = New Dictionary(Of String, List(Of Entry))(StringComparer.Ordinal)
            End Try
            Return _index
        End SyncLock
    End Function

    Private Shared Function ReadEntry(k_item As JsonElement) As Entry
        Dim k_probe As JsonElement
        If Not k_item.TryGetProperty("origText", k_probe) Then Return Nothing   ' a catalog from before 'orig*' held no edits
        Dim k_e As New Entry() With {
            .Key = Str(k_item, "function"),
            .Line = Num(k_item, "line"),
            .Type = Str(k_item, "type"),
            .Buttons = Str(k_item, "buttons"),
            .Caption = Str(k_item, "caption"),
            .Text = Str(k_item, "text"),
            .Extra = Str(k_item, "extraButton").Trim(),
            .Header = Str(k_item, "header"),
            .CloseButton = Str(k_item, "closeButton"),
            .OrigType = Str(k_item, "origType"),
            .OrigButtons = Str(k_item, "origButtons"),
            .OrigCaption = Str(k_item, "origCaption"),
            .OrigText = Str(k_item, "origText")
        }
        ' The id is "File.Member#n": the key of the entry is the part before '#'.
        If k_e.Key.Length = 0 Then Return Nothing
        Return k_e
    End Function

    Private Shared Function IsEdited(k_e As Entry) As Boolean
        Return k_e.Extra.Length > 0 OrElse k_e.Header.Length > 0 OrElse
            (k_e.CloseButton.Length > 0 AndAlso Not String.Equals(k_e.CloseButton, "Auto", StringComparison.OrdinalIgnoreCase)) OrElse
            Not String.Equals(k_e.Text, k_e.OrigText, StringComparison.Ordinal) OrElse
            Not String.Equals(k_e.Caption, k_e.OrigCaption, StringComparison.Ordinal) OrElse
            Not String.Equals(k_e.Type, k_e.OrigType, StringComparison.OrdinalIgnoreCase) OrElse
            Not String.Equals(k_e.Buttons, k_e.OrigButtons, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function Str(k_item As JsonElement, k_name As String) As String
        Dim k_v As JsonElement
        If k_item.TryGetProperty(k_name, k_v) AndAlso k_v.ValueKind = JsonValueKind.String Then Return If(k_v.GetString(), String.Empty)
        Return String.Empty
    End Function

    Private Shared Function Num(k_item As JsonElement, k_name As String) As Integer
        Dim k_v As JsonElement
        If k_item.TryGetProperty(k_name, k_v) AndAlso k_v.ValueKind = JsonValueKind.Number Then Return k_v.GetInt32()
        Return 0
    End Function

End Class
