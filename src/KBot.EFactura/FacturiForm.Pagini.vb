Option Strict On
Imports System.Windows.Forms
Imports KBot.Controls

' Slice 00EF-09 -- the controls of the view pages (Vanzare*Page, each declared in ITS OWN designer file) as the window's code sees them.
' The window keeps the logic of the invoice (what is shown, what is saved, what the buttons do), so it needs the page controls by name:
' BindPages (called right after InitializeComponent) points each name at the control of its page, and the handlers written with
' "Handles <name>.<event>" hook up at that moment. The pages hold no logic of their own; nothing here creates or places a control.
Partial Public Class FacturiForm

    ' VanzareGeneralePage (pgGenerale): 9 of 18 controls are used by the window's code
    Private WithEvents lblNumar As System.Windows.Forms.Label
    Private WithEvents dtpData As KBotDatePicker
    Private WithEvents lblTip As System.Windows.Forms.Label
    Private WithEvents lblStareFactura As System.Windows.Forms.Label
    Private WithEvents txtComentarii As KBotTextBox
    Private WithEvents txtRef As KBotTextField
    Private WithEvents lblTotal As System.Windows.Forms.Label
    Private WithEvents lblInfoFactura As System.Windows.Forms.Label
    Private WithEvents cmbContPlata As KBotComboBox

    ' VanzareCumparatorPage (pgCumparator): 14 of 27 controls are used by the window's code
    Private WithEvents cmbClient As KBotComboBox
    Private WithEvents chkCnp As System.Windows.Forms.CheckBox
    Private WithEvents txtClientInd As KBotTextField
    Private WithEvents txtClientCf As KBotTextField
    Private WithEvents txtClientDen As KBotTextField
    Private WithEvents cmbClientJud As KBotComboBox
    Private WithEvents txtClientOras As KBotTextField
    Private WithEvents cmbClientSector As KBotComboBox
    Private WithEvents txtClientAdresa As KBotTextField
    Private WithEvents txtClientCont As KBotTextField
    Private WithEvents txtClientBanca As KBotTextField
    Private WithEvents btnClientNou As System.Windows.Forms.Button
    Private WithEvents btnClientSalveaza As System.Windows.Forms.Button
    Private WithEvents btnClientSterge As System.Windows.Forms.Button

    ' VanzareAtasamentePage (pgAtasamente): 1 of 3 controls are used by the window's code
    Private WithEvents chkAtasament As System.Windows.Forms.CheckBox

    ' VanzareContinutPage (pgContinut): 2 of 4 controls are used by the window's code
    Private WithEvents btnLinieNoua As System.Windows.Forms.Button
    Private WithEvents gridLinii As KBotDataView

    ''' <summary>Points the names above at the controls of the pages; called once, right after InitializeComponent.</summary>
    Private Sub BindPages()
        lblNumar = pgGenerale.lblNumar
        dtpData = pgGenerale.dtpData
        lblTip = pgGenerale.lblTip
        lblStareFactura = pgGenerale.lblStareFactura
        txtComentarii = pgGenerale.txtComentarii
        txtRef = pgGenerale.txtRef
        lblTotal = pgGenerale.lblTotal
        lblInfoFactura = pgGenerale.lblInfoFactura
        cmbContPlata = pgGenerale.cmbContPlata
        cmbClient = pgCumparator.cmbClient
        chkCnp = pgCumparator.chkCnp
        txtClientInd = pgCumparator.txtClientInd
        txtClientCf = pgCumparator.txtClientCf
        txtClientDen = pgCumparator.txtClientDen
        cmbClientJud = pgCumparator.cmbClientJud
        txtClientOras = pgCumparator.txtClientOras
        cmbClientSector = pgCumparator.cmbClientSector
        txtClientAdresa = pgCumparator.txtClientAdresa
        txtClientCont = pgCumparator.txtClientCont
        txtClientBanca = pgCumparator.txtClientBanca
        btnClientNou = pgCumparator.btnClientNou
        btnClientSalveaza = pgCumparator.btnClientSalveaza
        btnClientSterge = pgCumparator.btnClientSterge
        chkAtasament = pgAtasamente.chkAtasament
        btnLinieNoua = pgContinut.btnLinieNoua
        gridLinii = pgContinut.gridLinii
    End Sub

End Class
