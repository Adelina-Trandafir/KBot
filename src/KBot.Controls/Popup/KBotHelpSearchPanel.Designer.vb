<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotHelpSearchPanel
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
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        tmrCauta = New System.Windows.Forms.Timer(components)
        lstRezultate = New KBotHelpList()
        stele = New KBotHelpStars()
        txtCauta = New KBotTextField()
        SuspendLayout()
        '
        ' tmrCauta
        '
        tmrCauta.Interval = 150
        '
        ' lstRezultate
        '
        lstRezultate.Dock = DockStyle.Fill
        lstRezultate.Location = New Point(8, 44)
        lstRezultate.Margin = New Padding(0)
        lstRezultate.Name = "lstRezultate"
        lstRezultate.Size = New Size(344, 216)
        lstRezultate.TabIndex = 1
        '
        ' stele
        '
        stele.Dock = DockStyle.Bottom
        stele.Location = New Point(8, 260)
        stele.Margin = New Padding(0)
        stele.Name = "stele"
        stele.Size = New Size(344, 32)
        stele.TabIndex = 2
        stele.Visible = False
        tips.SetToolTipHeader(stele, "A fost util răspunsul?")
        tips.SetToolTipText(stele, "Dă o notă de la 1 la 5 stele. Un clic pe aceeași stea o retrage. Notele ne arată ce trebuie scris mai bine în ajutor.")
        '
        ' txtCauta
        '
        txtCauta.Dock = DockStyle.Top
        txtCauta.Location = New Point(8, 8)
        txtCauta.Margin = New Padding(0, 0, 0, 6)
        txtCauta.MaxLength = 300
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "Scrie o întrebare (fără nume sau date personale)..."
        txtCauta.Size = New Size(344, 30)
        txtCauta.TabIndex = 0
        tips.SetToolTipHeader(txtCauta, "Caută în ajutor")
        tips.SetToolTipText(txtCauta, "Scrie ce vrei să faci, cu cuvintele tale («cum trimit un DDF»). Rezultatele apar pe măsură ce scrii; săgețile sus / jos aleg unul, Enter îl deschide. Întrebările se trimit fără numele tău, ca să îmbunătățim ajutorul: nu scrie nume, coduri fiscale sau alte date personale.")
        '
        ' KBotHelpSearchPanel
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(lstRezultate)
        Controls.Add(stele)
        Controls.Add(txtCauta)
        Name = "KBotHelpSearchPanel"
        Padding = New Padding(8)
        Size = New Size(360, 300)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tmrCauta As System.Windows.Forms.Timer
    Friend WithEvents lstRezultate As KBotHelpList
    Friend WithEvents stele As KBotHelpStars
    Friend WithEvents txtCauta As KBotTextField
End Class
