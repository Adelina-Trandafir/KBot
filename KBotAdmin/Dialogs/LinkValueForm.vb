Option Strict On
Imports System.Globalization
Imports System.Text.Json

''' <summary>
''' The value picker of a linked-table jump on a composite key (page «Tabele», ME01-08): one column of the key whose value the operator
''' must give. A combo they TYPE in (<c>Editable</c>), <c>LimitToList</c> (a text that is not one of the values does not stay in the field and
''' cannot be accepted, so the answer is never empty or invented). The values are not all brought: after the first character typed the
''' <c>fetch</c> given by the page asks the server for those that CONTAIN the text (the ones that START with it first), up to
''' <see cref="PickLimit"/>; typing more narrows the list already there (the combo's own find-as-you-type), and the server is asked again only
''' when the list was cut at the limit or the text no longer extends what was asked.
''' </summary>
Public Class LinkValueForm

    ''' <summary>The most values one answer of the server holds.</summary>
    Public Const PickLimit As Integer = 200

    ''' <summary>One row of the combo: what the operator reads and the JSON the server compares with.</summary>
    Private NotInheritable Class PickItem
        Public ReadOnly Text As String
        Public ReadOnly Json As String
        Public Sub New(k_text As String, k_json As String)
            Text = k_text
            Json = k_json
        End Sub
        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Private ReadOnly _fetch As Func(Of String, Task(Of DbDistinctResult))
    Private _loadedQuery As String = String.Empty
    Private _loadedMore As Boolean
    Private _request As Integer

    ''' <param name="k_title">The window title.</param>
    ''' <param name="k_prompt">What is asked (the column and what is already chosen).</param>
    ''' <param name="k_fetch">Brings the values that contain a text (Nothing = the call failed and was already shown to the operator).</param>
    Public Sub New(k_title As String, k_prompt As String, k_fetch As Func(Of String, Task(Of DbDistinctResult)))
        InitializeComponent()
        Icon = AdminIcon.Value
        Text = k_title
        lblPrompt.Text = k_prompt
        _fetch = k_fetch
        ShowHint(StartHint)
    End Sub

    Private Const StartHint As String = "Scrieți primul caracter al valorii: se aduc din tabel valorile care îl conțin."

    ''' <summary>The JSON of the value chosen (what the server compares with). Read after the window closed with OK.</summary>
    Public ReadOnly Property ChosenJson As String
        Get
            Dim k_item As PickItem = TryCast(cmbValue.SelectedItem, PickItem)
            Return If(k_item Is Nothing, String.Empty, k_item.Json)
        End Get
    End Property

    ''' <summary>The value chosen as the operator reads it.</summary>
    Public ReadOnly Property ChosenText As String
        Get
            Dim k_item As PickItem = TryCast(cmbValue.SelectedItem, PickItem)
            Return If(k_item Is Nothing, String.Empty, k_item.Text)
        End Get
    End Property

    ''' <summary>A value of a pick list as the operator reads it: a text without quotes, a number as it is.</summary>
    Public Shared Function ValueText(k_value As JsonElement) As String
        Return If(k_value.ValueKind = JsonValueKind.String, k_value.GetString(), k_value.GetRawText())
    End Function

    Private Sub ShowHint(k_text As String)
        lblHint.Text = k_text
    End Sub

    Private Sub LinkValueForm_Shown(k_sender As Object, e As EventArgs) Handles Me.Shown
        Try
            cmbValue.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("LinkValueForm.LinkValueForm_Shown", ex)
        End Try
    End Sub

    ''' <summary>Every change of the text in the box: the first character brings the values, the next ones narrow them (see the class summary).</summary>
    Private Async Sub CmbValue_TextChanged(k_sender As Object, e As EventArgs) Handles cmbValue.TextChanged
        Try
            Dim k_typed As String = cmbValue.Text
            If String.IsNullOrWhiteSpace(k_typed) Then
                TableNavLog.Write("VALUE", "typed text empty: list emptied")
                _request += 1
                _loadedQuery = String.Empty
                _loadedMore = False
                cmbValue.Items.Clear()
                ShowHint(StartHint)
                Return
            End If
            ' A value the combo just took from its list (it writes its text): nothing to ask.
            Dim k_picked As PickItem = TryCast(cmbValue.SelectedItem, PickItem)
            If k_picked IsNot Nothing AndAlso String.Equals(k_picked.Text, k_typed, StringComparison.CurrentCultureIgnoreCase) Then
                TableNavLog.Write("VALUE", "text «" & k_typed & "» is the value taken from the list: nothing to ask")
                Return
            End If
            ' What is in the combo already holds every value that contains the text (it was asked with a shorter start of it and was not cut).
            If _loadedQuery <> String.Empty AndAlso Not _loadedMore AndAlso k_typed.StartsWith(_loadedQuery, StringComparison.CurrentCultureIgnoreCase) Then
                TableNavLog.Write("VALUE", "typed «" & k_typed & "»: narrows what was brought for «" & _loadedQuery & "» (" & cmbValue.Items.Count.ToString(CultureInfo.InvariantCulture) & " values in the combo), no server call")
                If cmbValue.Items.Count = 0 Then ShowHint(NoneHint(k_typed))
                Return
            End If
            _request += 1
            Dim k_id As Integer = _request
            TableNavLog.Write("VALUE", "typed «" & k_typed & "»: asks the server (" & If(_loadedQuery = String.Empty, "first characters", If(_loadedMore, "the list brought for «" & _loadedQuery & "» was cut", "the text no longer extends «" & _loadedQuery & "»")) & ")")
            ShowHint("Se caută valorile care conțin «" & k_typed & "»...")
            Dim k_result As DbDistinctResult = Await _fetch(k_typed)
            If IsDisposed OrElse k_id <> _request OrElse k_result Is Nothing Then
                TableNavLog.Write("VALUE", "answer for «" & k_typed & "» NOT used: " & If(k_result Is Nothing, "the call failed", "the text changed meanwhile or the window closed"))
                Return
            End If
            _loadedQuery = k_typed
            _loadedMore = k_result.HasMore
            cmbValue.BeginUpdate()
            Try
                cmbValue.Items.Clear()
                cmbValue.Items.AddRange(k_result.Values.Select(Function(k_v) New PickItem(ValueText(k_v), k_v.GetRawText())).ToArray())
            Finally
                cmbValue.EndUpdate()
            End Try
            If k_result.Values.Count = 0 Then
                ShowHint(NoneHint(k_typed))
                Return
            End If
            ShowHint(k_result.Values.Count.ToString("N0", CultureInfo.CurrentCulture) & " valori" &
                     If(k_result.HasMore, " (primele; scrieți mai mult ca să le restrângeți)", String.Empty) & ". Alegeți una din listă.")
            cmbValue.DroppedDown = True
        Catch ex As Exception
            ' UI boundary (async void).
            GlobalErrorLog.Write("LinkValueForm.CmbValue_TextChanged", ex)
        End Try
    End Sub

    Private Shared Function NoneHint(k_typed As String) As String
        Return "Nu există valori care conțin «" & k_typed & "»."
    End Function

    ''' <summary>A row taken from the list closes the window at once.</summary>
    Private Sub CmbValue_ItemPicked(k_sender As Object, e As EventArgs) Handles cmbValue.ItemPicked
        Try
            If cmbValue.SelectedItem Is Nothing Then Return
            TableNavLog.Write("VALUE", "row taken from the list: " & ChosenText & " (json " & ChosenJson & ")")
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("LinkValueForm.CmbValue_ItemPicked", ex)
        End Try
    End Sub

    ''' <summary>«Alege»: the text in the box must be a value of the list (the combo's LimitToList decides, here), else nothing is accepted.</summary>
    Private Sub BtnOk_Click(k_sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            cmbValue.CommitText()
            TableNavLog.Write("VALUE", "«Alege» pressed with text «" & cmbValue.Text & "»: " & If(cmbValue.SelectedItem Is Nothing, "NOT a value of the list, refused", "accepted as " & ChosenText))
            If cmbValue.SelectedItem Is Nothing Then
                KBotMessage.Show(Me, "Alegeți o valoare din listă: scrieți primul caracter și alegeți din valorile aduse.", "Tabele legate", MessageBoxButtons.OK, MessageBoxIcon.Information)
                cmbValue.Focus()
                Return
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("LinkValueForm.BtnOk_Click", ex)
        End Try
    End Sub

End Class
