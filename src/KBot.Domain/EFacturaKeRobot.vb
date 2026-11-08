Option Strict On
Imports System.Collections.Generic

' Slices KE05..KE07 -- what the shell's FOREXE robot is asked to save as a reception, and what it answers.
' Plain data, no logic (the rules are on the server, the steps in the workflows «Receptie Noua» / «Receptie Modificata»).

''' <summary>One row of the reception's form on the site: where it is, and the value (WITH VAT) its field must hold.</summary>
Public NotInheritable Class KeReceptieRand

    ''' <summary>The classification as the site's table matches it: letters and digits only (the row's «Sector - Sursa - Indicator»).</summary>
    Public Property ClsfSal As String = String.Empty

    ''' <summary>The classification as the operator reads it (for messages).</summary>
    Public Property Clsf As String = String.Empty

    Public Property CodIndicator As String = String.Empty

    ''' <summary>The value the field holds AFTER the save: the whole value of that indicator, not the part this invoice adds.</summary>
    Public Property Valoare As Decimal
End Class

''' <summary>A reception K-BOT must create or change in FOREXE.</summary>
Public NotInheritable Class KeReceptieDeSalvat
    Public Property CodAngajament As String = String.Empty

    ''' <summary>True = an existing reception is changed (opened with the eye of its row); False = a new one is added.</summary>
    Public Property Modifica As Boolean

    ''' <summary>«Avans», «Partial» or «Final»: the text of the site's list (the page selects by text).</summary>
    Public Property TipReceptie As String = "Final"

    ''' <summary>The date written in the form (a new reception).</summary>
    Public Property Data As Date

    ''' <summary>The description written in the form; for a new reception it already ends with the K-BOT marker.</summary>
    Public Property Descriere As String = String.Empty

    ''' <summary>The marker text «(IDRH: n; IDR: m)» the server reserved; for a changed reception it is appended to the description on the site.</summary>
    Public Property Marcaj As String = String.Empty

    ''' <summary>The date of the reception to change, as the site's list shows it (a changed reception).</summary>
    Public Property DataTinta As Date?

    ''' <summary>The total of the reception to change, as the site's list shows it (with the date it finds the row).</summary>
    Public Property SumaTinta As Decimal?

    ''' <summary>
    ''' A pattern (no colon: the workflow's <c>IfVar</c> splits on it) that matches the description of the reception to change because it holds a
    ''' K-BOT marker K-BOT wrote itself; empty = no marker is known, the date and the total decide.
    ''' </summary>
    Public Property MarcajRegex As String = String.Empty

    ''' <summary>A short label for the log lines and the messages («factura 123 / 10.2026»).</summary>
    Public Property Eticheta As String = String.Empty

    Public ReadOnly Property Randuri As New List(Of KeReceptieRand)()
End Class

''' <summary>What came of a save.</summary>
Public NotInheritable Class KeReceptieSalvata

    ''' <summary>The reception is saved in FOREXE: the site said so, or the re-read found it.</summary>
    Public Property Salvata As Boolean

    ''' <summary>The site showed its «saved» message.</summary>
    Public Property MesajVazut As Boolean

    ''' <summary>The reception was read back and taken into K-BOT.</summary>
    Public Property Preluata As Boolean

    ''' <summary>What the operator reads: the reason for a failure, or a line that says what was done.</summary>
    Public Property Mesaj As String = String.Empty
End Class

' Slice 02EF-12 -- what the page «Recepții <> EFactura» hands the shell when the operator asks for a reservation that covers a reception: the window
' «Adaugă rezervare» (the DDF editor) opens with the values filled in from the reception.

''' <summary>One indicator of the reservation to create: what the reception takes from it and what the angajament holds on it now.</summary>
Public NotInheritable Class KeRezervareRand

    ''' <summary>The indicator of the angajament (the key of the line in section A).</summary>
    Public Property CodIndicator As String = String.Empty

    ''' <summary>The classification as the operator reads it (for messages).</summary>
    Public Property Clsf As String = String.Empty

    ''' <summary>What the reception takes on this indicator (WITH VAT): the amount the reservation must cover.</summary>
    Public Property SumaReceptie As Decimal

    ''' <summary>Reserved on the indicator now (SUM of the reservations of the angajament).</summary>
    Public Property Rezervat As Decimal

    ''' <summary>Received on the indicator before the day of the reception (SUM of the receptions of the angajament).</summary>
    Public Property Receptionat As Decimal

    ''' <summary>What the new reservation must add so that the reception fits: the reception minus what is still free (never below 0).</summary>
    Public ReadOnly Property Necesar As Decimal
        Get
            Return Math.Max(0D, SumaReceptie - (Rezervat - Receptionat))
        End Get
    End Property
End Class

''' <summary>A reservation to create for a reception: the angajament and the indicators the reception touches.</summary>
Public NotInheritable Class KeRezervareDeCreat
    Public Property CodAngajament As String = String.Empty
    Public ReadOnly Property Randuri As New List(Of KeRezervareRand)()
End Class

''' <summary>What came of the window: the reservation was saved, or the operator closed it (nothing was written).</summary>
Public NotInheritable Class KeRezervareCreata
    Public Property Salvata As Boolean
    Public Property Mesaj As String = String.Empty
End Class
