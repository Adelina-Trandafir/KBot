Imports System.Collections.Generic
Imports System.Text.Json.Serialization

' Security plan S1: the server's REAL password policy and the structured refusals built on it.
' POCOs plus one pure evaluator. Field names are the wire contract (ASCII, both sides).

' Answer of GET /api/auth/password/policy (read from MariaDB on the server, never a client copy).
Public NotInheritable Class PasswordPolicyInfo
    ' False = the server has no active validation plugin; only an application floor applies.
    <JsonPropertyName("plugin_active")> Public Property PluginActive As Boolean
    <JsonPropertyName("strict")> Public Property Strict As Boolean
    <JsonPropertyName("minimal_length")> Public Property MinimalLength As Integer
    <JsonPropertyName("digits")> Public Property Digits As Integer
    <JsonPropertyName("letters_same_case")> Public Property LettersSameCase As Integer
    <JsonPropertyName("other_characters")> Public Property OtherCharacters As Integer
End Class

Public NotInheritable Class PasswordPolicyResponse
    <JsonPropertyName("policy")> Public Property Policy As PasswordPolicyInfo
End Class

' One unmet requirement, as the server reports it in a PASSWORD_POLICY_FAILED refusal.
Public NotInheritable Class PasswordViolation
    <JsonPropertyName("code")> Public Property Code As String
    <JsonPropertyName("have")> Public Property Have As Integer
    <JsonPropertyName("need")> Public Property Need As Integer
    <JsonPropertyName("message")> Public Property Message As String
End Class

' What a refused password carries: the CURRENT policy plus every unmet requirement.
Public NotInheritable Class PasswordPolicyFailure
    Public Sub New(policy As PasswordPolicyInfo, violations As IReadOnlyList(Of PasswordViolation))
        Me.Policy = policy
        Me.Violations = If(violations, Array.Empty(Of PasswordViolation)())
    End Sub

    Public ReadOnly Property Policy As PasswordPolicyInfo
    Public ReadOnly Property Violations As IReadOnlyList(Of PasswordViolation)
End Class

' One line of the live requirement list: the sentence and whether the typed password meets it.
Public NotInheritable Class PasswordRequirement
    Public Sub New(text As String, met As Boolean)
        Me.Text = text
        Me.Met = met
    End Sub

    Public ReadOnly Property Text As String
    Public ReadOnly Property Met As Boolean
End Class

' Mirrors the server's evaluation (routes/auth/password_policy.py) so the operator sees what is
' missing while typing. The server stays the authority: it judges again before SET PASSWORD.
Public NotInheritable Class PasswordPolicyEvaluator
    Private Sub New()
    End Sub

    Public Shared Function Evaluate(k_password As String, k_policy As PasswordPolicyInfo) As IReadOnlyList(Of PasswordRequirement)
        If k_policy Is Nothing Then Throw New ArgumentNullException(NameOf(k_policy))
        Dim k_pw As String = If(k_password, String.Empty)
        Dim k_digits As Integer = 0, k_upper As Integer = 0, k_lower As Integer = 0, k_other As Integer = 0
        For Each k_ch As Char In k_pw
            If Char.IsDigit(k_ch) Then
                k_digits += 1
            ElseIf Char.IsLetter(k_ch) Then
                If Char.IsUpper(k_ch) Then k_upper += 1 Else k_lower += 1
            Else
                k_other += 1
            End If
        Next

        Dim k_list As New List(Of PasswordRequirement)()
        k_list.Add(New PasswordRequirement(
            $"Lungime: {k_pw.Length} din minimum {k_policy.MinimalLength}", k_pw.Length >= k_policy.MinimalLength))
        If k_policy.Digits > 0 Then
            k_list.Add(Line(k_digits, k_policy.Digits, "Cifre", "Lipsește o cifră", "Lipsesc {0} cifre"))
        End If
        If k_policy.LettersSameCase > 0 Then
            k_list.Add(Line(k_upper, k_policy.LettersSameCase, "Majuscule", "Lipsește o majusculă", "Lipsesc {0} majuscule"))
            k_list.Add(Line(k_lower, k_policy.LettersSameCase, "Minuscule", "Lipsește o minusculă", "Lipsesc {0} minuscule"))
        End If
        If k_policy.OtherCharacters > 0 Then
            k_list.Add(Line(k_other, k_policy.OtherCharacters, "Caractere speciale", "Lipsește un caracter special",
                            "Lipsesc {0} caractere speciale"))
        End If
        Return k_list
    End Function

    Private Shared Function Line(k_have As Integer, k_need As Integer, k_label As String,
                                 k_one As String, k_many As String) As PasswordRequirement
        If k_have >= k_need Then Return New PasswordRequirement($"{k_label}: {k_have} din minimum {k_need}", True)
        Dim k_missing As Integer = k_need - k_have
        Return New PasswordRequirement(If(k_missing = 1, k_one, String.Format(k_many, k_missing)), False)
    End Function
End Class
