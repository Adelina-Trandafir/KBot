Option Strict On
Imports System.Drawing

''' <summary>
''' Argumentele evenimentului <c>RowFormatting</c> — ridicat O DATĂ pe rând, ÎNAINTEA
''' celulelor lui. Câmpurile sunt PRE-UMPLUTE cu valorile implicite din temă; handler-ul
''' suprascrie doar ce-l interesează (ex. Stare = «Anulat» => tot rândul gri).
'''
''' ATENȚIE: instanța e REFOLOSITĂ de control pentru fiecare rând pictat (mii de rânduri
''' => zero presiune pe GC). Handler-ul NU are voie să o rețină după ieșirea din event.
''' </summary>
Public NotInheritable Class KBotRowFormattingEventArgs
    Inherits EventArgs

    ''' <summary>Indexul rândului pictat.</summary>
    Public Property RowIndex As Integer

    ''' <summary>Rândul pictat (pentru citirea valorilor / Tag-ului).</summary>
    Public Property Row As KBotDataRow

    ''' <summary>Fundalul rândului (implicit: normal / alternant / selectat).</summary>
    Public Property BackColor As Color

    ''' <summary>Culoarea textului pe rând.</summary>
    Public Property ForeColor As Color

    ''' <summary>Rând activ. False => tot rândul se pictează șters și devine inert.</summary>
    Public Property Enabled As Boolean

    ''' <summary>Re-inițializează instanța refolosită înaintea unei noi ridicări.</summary>
    Friend Sub Reset(rowIndex As Integer, row As KBotDataRow, back As Color, fore As Color, enabled As Boolean)
        Me.RowIndex = rowIndex
        Me.Row = row
        Me.BackColor = back
        Me.ForeColor = fore
        Me.Enabled = enabled
    End Sub

End Class
