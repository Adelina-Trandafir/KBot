Option Strict On
Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' The four grids of the bank statements whose columns the operator chooses in
''' «Setări → Extrase» (slice 0080-02, operator 24.09.2026): which columns are shown, and in
''' what order. Separate for the Extrase view and the «Extrase de cont» window; inside one
''' window the operations layout serves both the day grid and the lower grid.
''' </summary>
Public Enum ExtraseGrid
    ''' <summary>Extrase view, the FX_Extrase_H grid (Toate / month selected).</summary>
    ViewHeaders = 0
    ''' <summary>Extrase view, the FX_Extrase grid (day selected, and the lower grid).</summary>
    ViewOperations = 1
    ''' <summary>«Extrase de cont» window, the FX_Extrase_H grid.</summary>
    WindowHeaders = 2
    ''' <summary>«Extrase de cont» window, the FX_Extrase grid.</summary>
    WindowOperations = 3
End Enum

''' <summary>One column the operator may choose: its grid key and the caption shown for it. POCO.</summary>
Public NotInheritable Class ExtraseColumnInfo
    Public ReadOnly Property Key As String
    Public ReadOnly Property Caption As String

    Public Sub New(key As String, caption As String)
        Me.Key = key
        Me.Caption = caption
    End Sub
End Class

''' <summary>
''' The column catalogue of the statement grids, and the defaults (what the operator asked for
''' on 24.09.2026). The keys are the <c>KBotDataColumn.Key</c> of the columns authored in the
''' designer of <c>ExtrasePanel</c> -- one list, used by the panel, the settings page and the
''' store, so a key typed twice cannot drift.
''' </summary>
Public NotInheritable Class ExtraseColumns

    Private Sub New()
    End Sub

    ' ── header grid (FX_Extrase_H) ───────────────────────────────────────
    Public Const HData As String = "h_data"
    Public Const HNumar As String = "h_numar"
    Public Const HClsf As String = "h_clsf"
    Public Const HDenumire As String = "h_denumire"
    Public Const HCont As String = "h_cont"
    Public Const HIban As String = "h_iban"
    Public Const HSid As String = "h_sid"
    Public Const HSic As String = "h_sic"
    Public Const HRpd As String = "h_rpd"
    Public Const HRpc As String = "h_rpc"
    Public Const HTsd As String = "h_tsd"
    Public Const HTsc As String = "h_tsc"
    Public Const HSfd As String = "h_sfd"
    Public Const HSfc As String = "h_sfc"

    ' ── operations grid (FX_Extrase) ─────────────────────────────────────
    Public Const ODataBanca As String = "o_data_banca"
    Public Const ODataDoc As String = "o_data_doc"
    Public Const OClsf As String = "o_clsf"
    Public Const ONrDoc As String = "o_nr_doc"
    Public Const OReferinta As String = "o_referinta"
    Public Const OReferintaDest As String = "o_referinta_dest"
    Public Const OPlatitor As String = "o_platitor"
    Public Const OCui As String = "o_cui"
    Public Const OIban As String = "o_iban"
    Public Const ODebit As String = "o_debit"
    Public Const OCredit As String = "o_credit"
    Public Const OCodAngajament As String = "o_cod_angajament"
    Public Const OIndicator As String = "o_indicator"
    Public Const OCodProgram As String = "o_cod_program"
    Public Const OCodAi As String = "o_cod_ai"
    Public Const OExplicatii As String = "o_explicatii"

    Private Shared ReadOnly _headers As IReadOnlyList(Of ExtraseColumnInfo) = New List(Of ExtraseColumnInfo) From {
        New ExtraseColumnInfo(HData, "Data"),
        New ExtraseColumnInfo(HNumar, "Nr. extras"),
        New ExtraseColumnInfo(HClsf, "Clasificație"),
        New ExtraseColumnInfo(HDenumire, "Denumire clasificație"),
        New ExtraseColumnInfo(HCont, "Cont"),
        New ExtraseColumnInfo(HIban, "IBAN"),
        New ExtraseColumnInfo(HSid, "SID"),
        New ExtraseColumnInfo(HSic, "SIC"),
        New ExtraseColumnInfo(HRpd, "RPD"),
        New ExtraseColumnInfo(HRpc, "RPC"),
        New ExtraseColumnInfo(HTsd, "TSD"),
        New ExtraseColumnInfo(HTsc, "TSC"),
        New ExtraseColumnInfo(HSfd, "SFD"),
        New ExtraseColumnInfo(HSfc, "SFC")}

    Private Shared ReadOnly _operations As IReadOnlyList(Of ExtraseColumnInfo) = New List(Of ExtraseColumnInfo) From {
        New ExtraseColumnInfo(ODataBanca, "Data bancă"),
        New ExtraseColumnInfo(ODataDoc, "Data document"),
        New ExtraseColumnInfo(OClsf, "Clasificație"),
        New ExtraseColumnInfo(ONrDoc, "Nr. document"),
        New ExtraseColumnInfo(OReferinta, "Referință"),
        New ExtraseColumnInfo(OReferintaDest, "Referință destinatar"),
        New ExtraseColumnInfo(OPlatitor, "Plătitor"),
        New ExtraseColumnInfo(OCui, "CUI"),
        New ExtraseColumnInfo(OIban, "IBAN"),
        New ExtraseColumnInfo(ODebit, "Debit"),
        New ExtraseColumnInfo(OCredit, "Credit"),
        New ExtraseColumnInfo(OCodAngajament, "Cod angajament"),
        New ExtraseColumnInfo(OIndicator, "Indicator"),
        New ExtraseColumnInfo(OCodProgram, "Cod program"),
        New ExtraseColumnInfo(OCodAi, "CodAI"),
        New ExtraseColumnInfo(OExplicatii, "Explicații")}

    ''' <summary>The columns a grid can show, in catalogue order.</summary>
    Public Shared Function Catalog(grid As ExtraseGrid) As IReadOnlyList(Of ExtraseColumnInfo)
        Select Case grid
            Case ExtraseGrid.ViewHeaders, ExtraseGrid.WindowHeaders : Return _headers
            Case ExtraseGrid.ViewOperations, ExtraseGrid.WindowOperations : Return _operations
            Case Else
                Throw New ArgumentException($"Grilă de extrase necunoscută: '{grid}'.", NameOf(grid))
        End Select
    End Function

    ''' <summary>
    ''' The default columns, in order (operator, 24.09.2026). Headers: the date, Clsf and the
    ''' six balances. Operations: DataBanca first (it can differ from DataDoc), the document,
    ''' the payer and the two amounts; the window adds Clsf, CodAngajament / Indicator and,
    ''' last, Explicații.
    ''' </summary>
    Public Shared Function Defaults(grid As ExtraseGrid) As List(Of String)
        Select Case grid
            Case ExtraseGrid.ViewHeaders, ExtraseGrid.WindowHeaders
                Return New List(Of String) From {HData, HClsf, HSid, HSic, HTsd, HTsc, HSfd, HSfc}
            Case ExtraseGrid.ViewOperations
                Return New List(Of String) From {ODataBanca, ONrDoc, OPlatitor, OCui, OIban, ODebit, OCredit}
            Case ExtraseGrid.WindowOperations
                Return New List(Of String) From {ODataBanca, OClsf, ONrDoc, OPlatitor, OCui, OIban,
                                                 ODebit, OCredit, OCodAngajament, OIndicator, OExplicatii}
            Case Else
                Throw New ArgumentException($"Grilă de extrase necunoscută: '{grid}'.", NameOf(grid))
        End Select
    End Function

    ''' <summary>
    ''' A stored list made safe to apply: keys the catalogue does not know are dropped (a file
    ''' from another build), duplicates keep their first place. An empty result means the
    ''' defaults -- a grid with no column at all cannot be what anyone chose.
    ''' </summary>
    Public Shared Function Normalize(grid As ExtraseGrid, keys As IEnumerable(Of String)) As List(Of String)
        Dim known As New HashSet(Of String)(Catalog(grid).Select(Function(c) c.Key), StringComparer.Ordinal)
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim result As New List(Of String)()
        If keys IsNot Nothing Then
            For Each k As String In keys
                If k IsNot Nothing AndAlso known.Contains(k) AndAlso seen.Add(k) Then result.Add(k)
            Next
        End If
        If result.Count = 0 Then Return Defaults(grid)
        Return result
    End Function

End Class
