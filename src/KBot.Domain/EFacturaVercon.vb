Option Strict On
Imports System.Collections.Generic

' Slice 00EF-45 / 00EF-47 (EV01, EV03) -- what the routes of the SENDING of a received invoice to VERCON answer
' (PYTHON/routes/efactura/primite_vercon_routes.py). Plain data, no logic. Plan: docs/EV_PLAN.md.

''' <summary>The states of a sending (the <c>Stare</c> of the routes). The server already applies the rule «a taken-over sending whose NIRs are all cancelled is waiting again».</summary>
Public NotInheritable Class EFacturaVerconStare

    Private Sub New()
    End Sub

    ''' <summary>Waiting for the Administrator to take it over.</summary>
    Public Const Trimisa As String = "TRIMISA"

    ''' <summary>The Administrator made a NIR from it.</summary>
    Public Const Preluata As String = "PRELUATA"

    ''' <summary>The Administrator sent it back, with a reason.</summary>
    Public Const Returnata As String = "RETURNATA"

    ''' <summary>The accountant took it back before it was taken over.</summary>
    Public Const Retrasa As String = "RETRASA"

End Class

''' <summary>An account of VERCON (<c>VC_Masini</c>) the accountant can propose for a line.</summary>
Public NotInheritable Class EFacturaVerconCont
    Public Property IdMasina As Integer
    Public Property Masina As String = String.Empty
End Class

''' <summary>A gestiune of VERCON with its active accounts: the tree the accountant picks from (subunit / gestiune, then the accounts).</summary>
Public NotInheritable Class EFacturaVerconGestiune
    Public Property SubunitId As Integer
    Public Property GestiuneId As Integer
    Public Property Subunitate As String = String.Empty
    Public Property Gestiune As String = String.Empty

    ''' <summary>MATERIALE, ALIMENTE, OI or MF.</summary>
    Public Property Tip As String = String.Empty

    Public ReadOnly Property Conturi As New List(Of EFacturaVerconCont)()
End Class

''' <summary>A line of the invoice with its value WITH VAT and the account proposed for it (<see cref="IdMasina"/> Nothing = the Administrator chooses).</summary>
Public NotInheritable Class EFacturaVerconLinie
    Public Property IdLinie As Integer
    Public Property NrLinie As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Cpv As String = String.Empty
    Public Property Unit As String = String.Empty
    Public Property Cant As Decimal
    Public Property Pret As Decimal
    Public Property Valoare As Decimal

    ''' <summary>The value WITH VAT, with the sign of the invoice.</summary>
    Public Property ValoareCuTva As Decimal

    Public Property IdMasina As Integer?
    Public Property Cont As String = String.Empty

    ''' <summary>«Subunit / Gestiune» of the proposed account; empty without one.</summary>
    Public Property Gestiune As String = String.Empty
End Class

''' <summary>A NIR made from a sending.</summary>
Public NotInheritable Class EFacturaVerconNir
    Public Property SubunitId As Integer
    Public Property GestiuneId As Integer
    Public Property Gestiune As String = String.Empty
    Public Property ExercitiuId As Integer
    Public Property IdIntrare As Integer
    Public Property Numar As Integer?
    Public Property NumarSub As Integer?
    Public Property Data As Date?

    ''' <summary>Cancelled in VERCON.</summary>
    Public Property Anulat As Boolean

    ''' <summary>It exists and is not cancelled.</summary>
    Public Property Valid As Boolean
End Class

''' <summary>One sending of the invoice (the active one, or one of the history).</summary>
Public NotInheritable Class EFacturaVerconTrimitere
    Public Property IdTrimitere As Integer

    ''' <summary>The EFFECTIVE state (see <see cref="EFacturaVerconStare"/>).</summary>
    Public Property Stare As String = EFacturaVerconStare.Trimisa

    ''' <summary>The state as stored.</summary>
    Public Property StareSalvata As String = EFacturaVerconStare.Trimisa

    ''' <summary>The sending had been taken over, every NIR was cancelled, and it waits again.</summary>
    Public Property Revenita As Boolean

    Public Property Observatie As String = String.Empty
    Public Property UnTrimis As String = String.Empty
    Public Property DataTrimis As Date?
    Public Property MotivReturnare As String = String.Empty
    Public Property UnReturnat As String = String.Empty
    Public Property DataReturnat As Date?
    Public Property UnPreluat As String = String.Empty
    Public Property DataPreluat As Date?
    Public ReadOnly Property Nir As New List(Of EFacturaVerconNir)()
End Class

''' <summary>The invoice, as the window shows it.</summary>
Public NotInheritable Class EFacturaVerconFactura
    Public Property IdPrimita As Integer
    Public Property NrFact As String = String.Empty
    Public Property DataFact As Date?
    Public Property Furnizor As String = String.Empty
    Public Property CuiNormalizat As String = String.Empty
    Public Property Total As Decimal
End Class

''' <summary>A NIR already recorded for the same supplier, invoice number and date (a warning, not a refusal).</summary>
Public NotInheritable Class EFacturaVerconDuplicat
    Public Property Gestiune As String = String.Empty
    Public Property Numar As Integer?
    Public Property NumarSub As Integer?
    Public Property IdIntrare As Integer
    Public Property Data As Date?
End Class

''' <summary>Everything the window «Trimite la Vercon» needs for one invoice.</summary>
Public NotInheritable Class EFacturaVerconStareFactura
    ''' <summary>The session's role is «Contabil»: only then can it send, change or take back.</summary>
    Public Property PoateScrie As Boolean

    ''' <summary>The invoice can be sent now; otherwise <see cref="Motiv"/> says why not.</summary>
    Public Property PoateTrimite As Boolean

    Public Property Motiv As String = String.Empty
    Public Property Factura As New EFacturaVerconFactura()

    ''' <summary>The supplier's row in the unit's partners; Nothing when there is none (then it cannot be sent).</summary>
    Public Property PartenerId As Integer?

    Public Property PartenerDenumire As String = String.Empty
    Public ReadOnly Property Duplicate As New List(Of EFacturaVerconDuplicat)()

    ''' <summary>The active sending (waiting or taken over); Nothing when none.</summary>
    Public Property Trimitere As EFacturaVerconTrimitere

    Public ReadOnly Property Istoric As New List(Of EFacturaVerconTrimitere)()
    Public ReadOnly Property Linii As New List(Of EFacturaVerconLinie)()
End Class

''' <summary>The short line of an invoice that was ever sent: for the list of received invoices.</summary>
Public NotInheritable Class EFacturaVerconSumar
    Public Property IdPrimita As Integer
    Public Property IdTrimitere As Integer
    Public Property Stare As String = EFacturaVerconStare.Trimisa
    Public Property Revenita As Boolean
    Public Property MotivReturnare As String = String.Empty

    ''' <summary>The NIRs that exist and are not cancelled, as «Gestiune: nr».</summary>
    Public ReadOnly Property Nir As New List(Of String)()
End Class

''' <summary>The account proposed for a line, as sent to the server (<see cref="IdMasina"/> Nothing = none).</summary>
Public NotInheritable Class EFacturaVerconContLinie
    Public Property IdLinie As Integer
    Public Property IdMasina As Integer?
End Class
