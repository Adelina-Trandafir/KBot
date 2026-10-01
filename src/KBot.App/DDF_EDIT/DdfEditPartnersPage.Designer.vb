Imports KBot.Controls

' «Parteneri» of the DDF editor (slice 0094-02): every partner associated with the document.
'
' FX_DDF has one pair of columns for the partner, so the header combo (cmbPartener on the form)
' still picks the document's MAIN partner -- the one written on every line -- and this page holds
' the whole set: the main partner plus any number of others. The list itself is the shared
' DdfPartnersView; the page only connects it to the draft.
'
' The nav entry is right-aligned (KBotNavAlign.Far) on the form's navSub: it is a different kind
' of page from the four that edit the revision, and the operator who never needs a second
' partner never needs to look at it.
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Coordinates are written at 144 dpi; AutoScaleDimensions goes with them (slice 0066-02).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfEditPartnersPage
    Inherits Global.KBot.Theming.KBotThemedUserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        tlyRoot = New KBotTableLayoutPanel()
        lblInfo = New Label()
        vwPartners = New DdfPartnersView()
        tlyRoot.SuspendLayout()
        SuspendLayout()
        '
        ' tlyRoot
        '
        tlyRoot.ColumnCount = 1
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRoot.Controls.Add(lblInfo, 0, 0)
        tlyRoot.Controls.Add(vwPartners, 0, 1)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 2
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.Size = New Size(1018, 609)
        tlyRoot.TabIndex = 0
        '
        ' lblInfo
        '
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Font = New Font("Calibri", 9F)
        lblInfo.Location = New Point(11, 0)
        lblInfo.Margin = New Padding(11, 0, 11, 0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(996, 64)
        lblInfo.TabIndex = 0
        lblInfo.Text = "Partenerii asociați documentului. Partenerul principal este cel ales în antet și se scrie pe toate rândurile din secțiunile A și B; ceilalți parteneri doar rămân asociați documentului."
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' vwPartners
        '
        vwPartners.Dock = DockStyle.Fill
        vwPartners.Location = New Point(0, 64)
        vwPartners.Margin = New Padding(0)
        vwPartners.Name = "vwPartners"
        vwPartners.Size = New Size(1018, 545)
        vwPartners.TabIndex = 1
        '
        ' DdfEditPartnersPage
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "DdfEditPartnersPage"
        Size = New Size(1018, 609)
        tlyRoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlyRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblInfo As Label
    Friend WithEvents vwPartners As DdfPartnersView
End Class
