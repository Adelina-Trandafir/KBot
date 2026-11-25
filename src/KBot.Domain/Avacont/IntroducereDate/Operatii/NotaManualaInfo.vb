Option Strict On
Imports System.Collections.Generic

Public NotInheritable Class NotaManualaAlegere
    Public Property Id As Integer
    Public Property Cod As String
    Public Property Denumire As String
    ' SLICE-AV08-01 («Protectie operatii»): base account / analytic of an account; functional / economic classification of an account or of a classification.
    Public Property Cont As Integer?
    Public Property Analitic As String
    Public Property ClsfF As String
    Public Property ClsfE As String
    Public Property Articol As String
    Public Property Activ As Integer?

    Public Overrides Function ToString() As String
        If String.IsNullOrWhiteSpace(Cod) Then Return If(Denumire, String.Empty)
        If String.IsNullOrWhiteSpace(Denumire) Then Return Cod
        Return $"{Cod} — {Denumire}"
    End Function
End Class

Public NotInheritable Class NotaManualaNota
    Public Property Id As Integer
    Public Property NumarNC As Integer
End Class

Public NotInheritable Class NotaManualaDocument
    Public Property IdDocument As Integer
    Public Property IdNota As Integer
    Public Property NumarDocument As String
    Public Property DataDocument As DateOnly
    Public Property FelDocument As String
    Public Property Fel As String
    Public Property NumarNC As Integer
    Public Property PoatePrimiOperatie As Boolean
End Class

Public NotInheritable Class NotaManualaTipOperatie
    Public Property Cod As String
    Public Property Denumire As String

    Public Overrides Function ToString() As String
        Return Denumire
    End Function
End Class

Public NotInheritable Class NotaManualaLinie
    Public Property IdLinie As Integer?
    Public Property IdCont As Integer
    Public Property IdPartener As Integer?
    Public Property Suma As Decimal
End Class

Public NotInheritable Class NotaManualaOperatie
    Public Property IdOper As Integer
    Public Property IdJurnal As Integer
    Public Property IdClsf As Integer?
    Public Property DataOperatie As DateOnly
    Public Property Explicatie As String
    Public Property Compartiment As String
    Public Property Cfpp As Integer
    Public Property Chelt As Integer
End Class

Public NotInheritable Class NotaManualaDetalii
    Public Property Operatie As NotaManualaOperatie
    Public Property Document As NotaManualaDocument
    Public Property Debit As New List(Of NotaManualaLinie)()
    Public Property Credit As New List(Of NotaManualaLinie)()
    Public Property EditareCompleta As Boolean
    Public Property EditareDocument As Boolean
    Public Property SchimbareTip As Boolean
    Public Property Evidentiere As Boolean
    ' SLICE-AV08-02: an operation of the last closing document (no «Protectie operatii» on it).
    Public Property Inchidere As Boolean
    Public Property Refuz As String
    ' SLICE-AV08-02: an earlier, closed year only warns (Access message 138).
    Public Property Avertisment As String
    Public Property Versiune As String
    Public Property NrOperatiiDocument As Integer
End Class

Public NotInheritable Class NotaManualaContext
    Public Property Subunitate As String
    Public Property Note As New List(Of NotaManualaNota)()
    Public Property Jurnale As New List(Of NotaManualaAlegere)()
    Public Property Clasificatii As New List(Of NotaManualaAlegere)()
    Public Property Conturi As New List(Of NotaManualaAlegere)()
    Public Property Parteneri As New List(Of NotaManualaAlegere)()
    Public Property TipuriOperatie As New List(Of NotaManualaTipOperatie)()
    Public Property FeluriDocument As New List(Of String)()
    Public Property Compartimente As New List(Of String)()
    Public Property Explicatii As New List(Of String)()
    Public Property Detalii As NotaManualaDetalii
End Class

Public NotInheritable Class NotaManualaCerere
    Public Property IdSubunitate As Integer
    Public Property An As Integer
    Public Property IdOper As Integer?
    Public Property IdDocument As Integer?
    Public Property IdNota As Integer?
    Public Property NotaNoua As Boolean?
    Public Property NumarNC As Integer?
    Public Property NumarDocument As String
    Public Property FelDocument As String
    Public Property TipOperatie As String
    Public Property DataDocument As DateOnly
    Public Property DataOperatie As DateOnly
    Public Property IdJurnal As Integer?
    Public Property IdClsf As Integer?
    Public Property Explicatie As String
    Public Property Compartiment As String
    Public Property Cfpp As Boolean
    Public Property Chelt As Boolean
    Public Property ModificaDocument As Boolean
    Public Property Versiune As String
    Public Property Debit As List(Of NotaManualaLinie)
    Public Property Credit As List(Of NotaManualaLinie)
    ' SLICE-AV08-01: the tick «Protectie operatii» of the user.
    Public Property Protectie As Boolean
    ' SLICE-AV08-05: the warnings the operator accepted (LUNA_NOUA, DOCUMENT_DUBLAT).
    Public Property Confirmari As New List(Of String)()
End Class

' SLICE-AV08-05: what the server really saved.
Public NotInheritable Class NotaManualaRezultat
    Public Property IdOper As Integer
    Public Property IdDocument As Integer
    Public Property IdNota As Integer
    Public Property NumarNC As Integer
    Public Property NumarDocument As String
    Public Property DataDocument As DateOnly
    Public Property DataOperatie As DateOnly
    Public Property OperatiiDocument As New List(Of Integer)()
End Class

' SLICE-AV08-05: a refusal of the save, with the box or the line to correct and the warning the operator may accept.
Public NotInheritable Class NotaManualaRefuz
    Public Property Mesaj As String
    Public Property StatusCode As Integer
    Public Property Reason As String
    Public Property Camp As String
    Public Property Parte As String
    Public Property Linie As Integer?
    Public Property Confirmare As String
End Class
