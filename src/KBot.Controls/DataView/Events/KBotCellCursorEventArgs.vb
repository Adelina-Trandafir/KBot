Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Argumentele evenimentului <c>CellCursor</c> (ME01-08): cursorul mouse-ului peste o celulă de date. Câmpul <see cref="Cursor"/> vine
''' PRE-UMPLUT cu cursorul pe care grila l-ar pune oricum (săgeata, sau bara de text peste o celulă editabilă); handler-ul îl schimbă
''' doar pe celula lui (de ex. mâna peste o celulă care se comportă ca un link). Se ridică numai peste celulele de date, nu peste antet,
''' benzile de grup, subsol sau marginile de redimensionare.
''' </summary>
Public NotInheritable Class KBotCellCursorEventArgs
    Inherits KBotCellEventArgs

    Public Sub New(k_columnKey As String, k_rowIndex As Integer, k_cursor As Cursor)
        MyBase.New(k_columnKey, k_rowIndex)
        Cursor = k_cursor
    End Sub

    ''' <summary>Cursorul de pus peste celulă (implicit: cel pe care l-ar pune grila).</summary>
    Public Property Cursor As Cursor

End Class
