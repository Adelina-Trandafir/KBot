Option Strict On
Imports System.Collections.ObjectModel
Imports KBot.Common

''' <summary>
''' Colecția ordonată din spatele lui <see cref="KBotChipBar.Chips"/> — geamăna lui
''' <c>KBotNavItemCollection</c>. Orice mutație invalidează așezarea barei, deci o modificare din
''' designer (adăugare / ștergere / reordonare) se repictează singură, fără să aștepte un resize.
'''
''' <b>Validarea cheilor NU stă aici</b>, dinadins: dialogul de colecție inserează un jeton gol în
''' clipa în care se apasă «Add», cu mult înainte să se fi tastat ceva în el. Contractul (cheie
''' nevidă și unică) se impune în <c>KBotChipBar.EndInit</c> și în metodele de rulare.
''' </summary>
Public NotInheritable Class KBotChipCollection
    Inherits Collection(Of KBotChip)

    ''' <summary>Bara care deține colecția (Nothing pentru o instanță liberă).</summary>
    Friend Property Owner As KBotChipBar

    ' Cele patru mutatoare își poartă propriul Try/Catch fiindcă sunt PUNCTE DE INTRARE — designer-ul
    ' le cheamă din InitializeComponent, codul le cheamă direct, deci nu există deasupra lor o
    ' frontieră deja împachetată la care să se logheze (acoperirea tranzitivă din regula casei nu se
    ' aplică). Clasificare «frontieră»: loghează și RE-ARUNCĂ, niciodată nu înghite — o bară care
    ' pierde tăcut un jeton e chiar felul de eșec pe care regula îl interzice.
    Protected Overrides Sub InsertItem(index As Integer, item As KBotChip)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.InsertItem(index, item)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChipCollection.InsertItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub SetItem(index As Integer, item As KBotChip)
        Try
            If item Is Nothing Then Throw New ArgumentNullException(NameOf(item))
            MyBase.SetItem(index, item)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChipCollection.SetItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub RemoveItem(index As Integer)
        Try
            MyBase.RemoveItem(index)
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChipCollection.RemoveItem", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub ClearItems()
        Try
            MyBase.ClearItems()
            Owner?.InvalidateLayout()
        Catch ex As Exception
            LogUnlessDesignTime("KBotChipCollection.ClearItems", ex)
            Throw
        End Try
    End Sub

    ' Un fișier de erori scris din interiorul devenv.exe e zgomot, nu diagnostic (vezi KBotDesignTime).
    Private Sub LogUnlessDesignTime(source As String, ex As Exception)
        If KBotDesignTime.IsDesignTime(Owner) Then Return
        GlobalErrorLog.Write(source, ex)
    End Sub

End Class
