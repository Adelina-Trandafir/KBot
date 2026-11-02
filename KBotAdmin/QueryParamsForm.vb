Option Strict On
Imports System.Globalization

''' <summary>
''' The window that opens when the operator clicks a query of the tree group «_Interogari» (page «Tabele»): what the query does, the database it
''' runs on, and one field per parameter with its explanation next to it. «Rulează» checks every value against the type the query declared
''' (<see cref="QueryParamDef.Parse"/>); the values are then read from <see cref="Values"/> and the page runs the query. A query without parameters
''' still opens this window, so the operator reads what it does before it runs.
''' </summary>
Public Class QueryParamsForm

    ''' <summary>The parameter rows the Designer holds (the label, the field and the explanation of each).</summary>
    Public Const Slots As Integer = 6

    Private Const RowHeight As Single = 56.0F

    Private ReadOnly _query As QueryDef
    Private ReadOnly _labels As Label()
    Private ReadOnly _fields As KBotTextBox()
    Private ReadOnly _explanations As Label()
    Private ReadOnly _values As New Dictionary(Of String, Object)(StringComparer.Ordinal)

    ''' <param name="k_query">The query to ask the values of.</param>
    ''' <param name="k_db">The database it will run on (only shown here).</param>
    ''' <param name="k_previous">The values typed the last time this query ran (name -> text), or Nothing.</param>
    Public Sub New(k_query As QueryDef, k_db As String, k_previous As IDictionary(Of String, String))
        InitializeComponent()
        Icon = AdminIcon.Value
        If k_query.Params.Count > Slots Then
            Throw New ArgumentException("Interogarea «" & k_query.Id & "» are " & k_query.Params.Count.ToString(CultureInfo.InvariantCulture) &
                                        " parametri; fereastra are loc pentru " & Slots.ToString(CultureInfo.InvariantCulture) & ".", NameOf(k_query))
        End If
        _query = k_query
        _labels = {lblP1, lblP2, lblP3, lblP4, lblP5, lblP6}
        _fields = {txtP1, txtP2, txtP3, txtP4, txtP5, txtP6}
        _explanations = {lblE1, lblE2, lblE3, lblE4, lblE5, lblE6}
        Text = "Interogare: " & k_query.Title
        lblDescription.Text = k_query.Description
        lblDatabase.Text = "Rulează pe baza " & k_db & " (doar citește, nu schimbă nimic)."
        For k_i As Integer = 0 To Slots - 1
            If k_i < k_query.Params.Count Then
                Dim k_param As QueryParamDef = k_query.Params(k_i)
                _labels(k_i).Text = k_param.Label & If(k_param.Required, String.Empty, " (opțional)")
                _explanations(k_i).Text = k_param.Explanation
                tips.SetToolTipHeader(_fields(k_i), k_param.Label)
                tips.SetToolTipText(_fields(k_i), k_param.Explanation)
                Dim k_text As String = Nothing
                If k_previous IsNot Nothing AndAlso k_previous.TryGetValue(k_param.Name, k_text) Then _fields(k_i).Text = k_text
            Else
                tlyParams.SetRowCollapsed(k_i, True)
                _labels(k_i).Visible = False
                _fields(k_i).Visible = False
                _explanations(k_i).Visible = False
            End If
        Next
        ' The form is authored with room for every slot: the rows not used give their height back.
        Dim k_unused As Integer = Slots - k_query.Params.Count
        ClientSize = New Size(ClientSize.Width, ClientSize.Height - CInt(Math.Round(k_unused * RowHeight * AppScaling.FactorFor(Me))))
    End Sub

    ''' <summary>The values checked and typed (parameter name -> value; Nothing = an optional field left empty). Read after the window closed with OK.</summary>
    Public ReadOnly Property Values As IDictionary(Of String, Object)
        Get
            Return _values
        End Get
    End Property

    ''' <summary>What the operator typed (parameter name -> text), to offer again the next time the same query opens.</summary>
    Public Function TypedTexts() As Dictionary(Of String, String)
        Dim k_texts As New Dictionary(Of String, String)(StringComparer.Ordinal)
        For k_i As Integer = 0 To _query.Params.Count - 1
            k_texts(_query.Params(k_i).Name) = _fields(k_i).Text
        Next
        Return k_texts
    End Function

    Private Sub QueryParamsForm_Shown(k_sender As Object, e As EventArgs) Handles Me.Shown
        Try
            If _query.Params.Count > 0 Then
                _fields(0).Focus()
            Else
                btnOk.Focus()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("QueryParamsForm.QueryParamsForm_Shown", ex)
        End Try
    End Sub

    Private Sub BtnOk_Click(k_sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            _values.Clear()
            For k_i As Integer = 0 To _query.Params.Count - 1
                Dim k_problem As String = String.Empty
                Dim k_value As Object = _query.Params(k_i).Parse(_fields(k_i).Text, k_problem)
                If k_problem <> String.Empty Then
                    KBotMessage.Show(Me, k_problem, "Interogare", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    _fields(k_i).Focus()
                    Return
                End If
                _values(_query.Params(k_i).Name) = k_value
            Next
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("QueryParamsForm.BtnOk_Click", ex)
        End Try
    End Sub

End Class
