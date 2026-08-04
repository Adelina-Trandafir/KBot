Option Strict On
Imports System.Globalization

' One HKCU preference ROW in the panel, parsed (slice 0023, pass 5).
'
' WHY THIS TYPE EXISTS: the panel used to carry one CHECKBOX per preference, and a checkbox has
' exactly two states — "write the one value the caption names" and "leave alone". The scenario
' schema, meanwhile, distinguishes FOUR: absent (leave alone), a number, a string, and null
' (delete). So «bEnableAv2 = 0» ticked on a machine already holding 0 logged `0 -> 0`, which is
' true, useless, and impossible to turn into a 1 without editing a file. The row now carries the
' value itself, and "nu atinge" is a first-class state that is NOT the same as writing 0.
Public NotInheritable Class PrefRowSelection

    Private Sub New()
    End Sub

    ' Sentinels shown in the combo lists. Neither can collide with a real registry payload here:
    ' the DWORD rows accept only integers, and the string row holds Adobe's own view-mode names.
    Public Const Untouched As String = "nu atinge"
    Public Const DeleteText As String = "șterge"

    ''' <summary>
    ''' Parses a REG_DWORD row. Blank or «nu atinge» → no intent at all; «șterge» → delete;
    ''' any integer → write it LITERALLY (not just 0/1 — the combo is editable on purpose).
    ''' Anything else is reported rather than guessed at.
    ''' </summary>
    Public Shared Function ParseDword(name As String, text As String) As PrefRowParse
        Dim t As String = If(text, "").Trim()
        If t.Length = 0 OrElse String.Equals(t, Untouched, StringComparison.OrdinalIgnoreCase) Then
            Return PrefRowParse.Untouched()
        End If
        If String.Equals(t, DeleteText, StringComparison.OrdinalIgnoreCase) Then
            Return PrefRowParse.From(New UserPrefIntent(name, UserPrefAction.Delete, Nothing))
        End If
        Dim n As Integer
        If Integer.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then
            Return PrefRowParse.From(New UserPrefIntent(name, UserPrefAction.WriteDword, n))
        End If
        Return PrefRowParse.Bad($"{name}: «{t}» nu este un număr întreg — rândul nu poate fi aplicat.")
    End Function

    ''' <summary>
    ''' Parses a REG_SZ row. Blank or «nu atinge» → no intent; «șterge» → delete; anything else is
    ''' written literally, so the operator can try a view-mode name the panel never heard of.
    ''' </summary>
    Public Shared Function ParseString(name As String, text As String) As PrefRowParse
        Dim t As String = If(text, "").Trim()
        If t.Length = 0 OrElse String.Equals(t, Untouched, StringComparison.OrdinalIgnoreCase) Then
            Return PrefRowParse.Untouched()
        End If
        If String.Equals(t, DeleteText, StringComparison.OrdinalIgnoreCase) Then
            Return PrefRowParse.From(New UserPrefIntent(name, UserPrefAction.Delete, Nothing))
        End If
        Return PrefRowParse.From(New UserPrefIntent(name, UserPrefAction.WriteString, t))
    End Function

    ''' <summary>
    ''' The text a row must show for an intent that came from a scenario file, so the panel states
    ''' what will actually be written instead of the nearest thing it can express.
    ''' </summary>
    Public Shared Function TextFor(intent As UserPrefIntent) As String
        If intent Is Nothing Then Return Untouched
        If intent.Action = UserPrefAction.Delete Then Return DeleteText
        Return Convert.ToString(intent.Value, CultureInfo.InvariantCulture)
    End Function

End Class

' Result of parsing one row: an intent, "leave alone", or an operator-facing complaint.
Public NotInheritable Class PrefRowParse

    Public ReadOnly Property Intent As UserPrefIntent
    Public ReadOnly Property Invalid As Boolean
    Public ReadOnly Property Message As String

    Private Sub New(intent As UserPrefIntent, invalid As Boolean, message As String)
        Me.Intent = intent
        Me.Invalid = invalid
        Me.Message = message
    End Sub

    Public Shared Function Untouched() As PrefRowParse
        Return New PrefRowParse(Nothing, False, Nothing)
    End Function

    Public Shared Function From(intent As UserPrefIntent) As PrefRowParse
        Return New PrefRowParse(intent, False, Nothing)
    End Function

    Public Shared Function Bad(message As String) As PrefRowParse
        Return New PrefRowParse(Nothing, True, message)
    End Function

    ' True only for the "leave this value exactly as it is" state — never for an invalid row,
    ' which must stop the run rather than quietly behave like "nu atinge".
    Public ReadOnly Property IsUntouched As Boolean
        Get
            Return Intent Is Nothing AndAlso Not Invalid
        End Get
    End Property

End Class
