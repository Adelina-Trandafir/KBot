Option Strict On

''' <summary>The three parts of the help and of the manual (slice 0000).</summary>
Public Enum HelpPart
    ''' <summary>Part 1: the accountant's everyday workflows.</summary>
    Contabil = 1
    ''' <summary>Part 2: the password-gated advanced options.</summary>
    Avansat = 2
    ''' <summary>Part 3: the director's window.</summary>
    Director = 3
End Enum

''' <summary>
''' One help topic: a Markdown file under <c>Help\</c> with a header block (slice 0000-01).
''' The header keys and values other than <see cref="Title"/> are ASCII: the operator never sees
''' them. Format and rules: <c>HelpContent\README.md</c>.
''' </summary>
Public NotInheritable Class HelpTopic

    ''' <summary>Unique, dotted, ASCII (<c>contabil.ddf.trimitere</c>). Links use it.</summary>
    Public Property Id As String = String.Empty

    ''' <summary>Romanian title, shown in the contents tree and as the page heading.</summary>
    Public Property Title As String = String.Empty

    Public Property Part As HelpPart = HelpPart.Contabil

    ''' <summary>Position among its siblings (lower first).</summary>
    Public Property Order As Integer

    ''' <summary>Id of the parent topic; empty = directly under the part.</summary>
    Public Property Parent As String = String.Empty

    ''' <summary>
    ''' Screens this topic explains, as <c>TypeName</c> (a whole form or view) or
    ''' <c>TypeName.controlName</c> (one control inside it). F1 and «?» look here.
    ''' </summary>
    Public Property Screens As New List(Of String)()

    ''' <summary>Search words that are not in the title or body (synonyms, old Access names).</summary>
    Public Property Keywords As New List(Of String)()

    ''' <summary>
    ''' Slice 0000-19: the screen this topic explains, as a capture's <c>goto</c>
    ''' (<c>view:ddf</c>, <c>menu:clasificatii</c>, <c>setari:tema</c>); empty = none. A search
    ''' hit on the topic offers «Deschide ...» for it.
    ''' </summary>
    Public Property Open As String = String.Empty

    ''' <summary>The Markdown body, header removed.</summary>
    Public Property Body As String = String.Empty

    ''' <summary>The file it came from (for error messages).</summary>
    Public Property SourcePath As String = String.Empty

    ' Slice 0000-18: the search index, built once when the file is read (HelpSearch.Index).
    Friend ReadOnly Property Sections As New List(Of HelpSection)()
    Friend Property TitleTerms As HelpTermSet
    Friend Property KeywordTerms As HelpTermSet

    ''' <summary>Romanian name of a part, as the operator reads it.</summary>
    Public Shared Function PartTitle(part As HelpPart) As String
        Select Case part
            Case HelpPart.Contabil : Return "Partea 1 — Contabil"
            Case HelpPart.Avansat : Return "Partea 2 — Opțiuni avansate"
            Case HelpPart.Director : Return "Partea 3 — Director"
            Case Else : Throw New ArgumentException("Unknown help part: " & part.ToString(), NameOf(part))
        End Select
    End Function

End Class
