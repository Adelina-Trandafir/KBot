Option Strict On
Imports System.Drawing

''' <summary>
''' Argumentele evenimentului <c>CellFormatting</c> — ridicat pentru FIECARE celulă pictată.
''' Câmpurile sunt PRE-UMPLUTE cu valorile implicite (culorile rândului din temă, textul deja
''' formatat prin <c>Column.FormatString</c>, alinierea coloanei); handler-ul suprascrie doar
''' ce-l interesează (ex. valoare negativă => text roșu).
'''
''' ATENȚIE: instanța e REFOLOSITĂ de control pentru fiecare celulă pictată (mii de celule =>
''' zero presiune pe GC). Handler-ul NU are voie să o rețină după ieșirea din event.
''' </summary>
Public NotInheritable Class KBotCellFormattingEventArgs
    Inherits EventArgs

    ''' <summary>Cheia coloanei pictate.</summary>
    Public Property ColumnKey As String

    ''' <summary>Indexul rândului pictat.</summary>
    Public Property RowIndex As Integer

    ''' <summary>Coloana pictată (tip, format, sursă combo…).</summary>
    Public Property Column As KBotDataColumn

    ''' <summary>Rândul pictat (pentru reguli care se uită la ALTE celule ale rândului).</summary>
    Public Property Row As KBotDataRow

    ''' <summary>Valoarea brută din celulă (înainte de formatare).</summary>
    Public Property Value As Object

    ''' <summary>Fundalul celulei (implicit: fundalul rândului).</summary>
    Public Property BackColor As Color

    ''' <summary>Culoarea textului celulei.</summary>
    Public Property ForeColor As Color

    ''' <summary>Fontul celulei (implicit: fontul ambient al controlului).</summary>
    Public Property Font As Font

    ''' <summary>Textul afișat (implicit: valoarea formatată). Suprascrierea schimbă doar afișarea.</summary>
    Public Property Text As String

    ''' <summary>Alinierea conținutului (implicit: cea a coloanei).</summary>
    Public Property Alignment As ContentAlignment

    ''' <summary>
    ''' Celulă activă. Implicit = <c>Column.Enabled AndAlso Row.Enabled</c>; handler-ul poate
    ''' coborî pe False ca să dezactiveze O SINGURĂ celulă (nu se poate ridica peste rând/coloană).
    ''' </summary>
    Public Property Enabled As Boolean

    ''' <summary>Re-inițializează instanța refolosită înaintea unei noi ridicări.</summary>
    Friend Sub Reset(column As KBotDataColumn, row As KBotDataRow, rowIndex As Integer,
                     value As Object, text As String, back As Color, fore As Color,
                     font As Font, alignment As ContentAlignment, enabled As Boolean)
        Me.Column = column
        Me.ColumnKey = column.Key
        Me.Row = row
        Me.RowIndex = rowIndex
        Me.Value = value
        Me.Text = text
        Me.BackColor = back
        Me.ForeColor = fore
        Me.Font = font
        Me.Alignment = alignment
        Me.Enabled = enabled
    End Sub

End Class
