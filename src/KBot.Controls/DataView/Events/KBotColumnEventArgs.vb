Option Strict On
Imports System.Drawing

''' <summary>
''' Identifică o COLOANĂ într-un eveniment de antet (slice 0028-02: apăsarea pictogramei din
''' dreapta). Ca și <see cref="KBotCellEventArgs"/>, NU se refolosește — evenimentul e o acțiune
''' a operatorului, deci o alocare per apăsare e irelevantă.
'''
''' <para><see cref="IconBounds"/> vine odată cu cheia fiindcă acesta e chiar cazul de folosire:
''' pictograma deschide un meniu (filtru, sortare), iar meniul trebuie așezat SUB ea. Fără
''' dreptunghi, gazda ar trebui să recalculeze o geometrie care e a grilei.</para>
''' </summary>
Public Class KBotColumnEventArgs
    Inherits EventArgs

    ''' <summary>Cheia coloanei.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>Dreptunghiul pictogramei apăsate, în coordonatele client ale grilei.</summary>
    Public ReadOnly Property IconBounds As Rectangle

    Public Sub New(columnKey As String, iconBounds As Rectangle)
        _ColumnKey = columnKey
        _IconBounds = iconBounds
    End Sub

End Class
