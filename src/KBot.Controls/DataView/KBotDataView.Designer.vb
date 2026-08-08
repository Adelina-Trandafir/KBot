Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Partea „designer” a <see cref="KBotDataView"/>. Conform regulii casei, TOATE controalele
''' copil se declară aici (nu se construiesc în cod la nevoie): cei doi editori flotanți și
''' cele două bare de derulare. Pozițiile se setează la runtime (pictarea le poziționează),
''' dar câmpurile trăiesc în Designer. Doar UN editor e vizibil odată, și doar cât o celulă
''' e în editare.
''' </summary>
Partial Class KBotDataView

    ''' <summary>Containerul de componente (contract designer standard).</summary>
    Private components As System.ComponentModel.IContainer

    ''' <summary>Editorul de text flotant (ascuns implicit).</summary>
    Friend WithEvents editText As TextBox

    ''' <summary>Editorul combo flotant (ascuns implicit). DropDownStyle se comută per coloană.</summary>
    Friend WithEvents editCombo As ComboBox

    ''' <summary>Bara de derulare verticală.</summary>
    Friend WithEvents vScroll As VScrollBar

    ''' <summary>Bara de derulare orizontală.</summary>
    Friend WithEvents hScroll As HScrollBar

    Private Sub InitializeComponent()
        Me.editText = New TextBox()
        Me.editCombo = New ComboBox()
        Me.vScroll = New VScrollBar()
        Me.hScroll = New HScrollBar()
        Me.SuspendLayout()
        '
        ' editText — editor de text flotant
        '
        Me.editText.BorderStyle = BorderStyle.FixedSingle
        Me.editText.Visible = False
        '
        ' editCombo — editor combo flotant
        '
        Me.editCombo.Visible = False
        '
        ' vScroll — bară verticală (poziționată/afișată la virtualizare, slice 0010-02)
        '
        Me.vScroll.Visible = False
        '
        ' hScroll — bară orizontală (poziționată/afișată la virtualizare, slice 0010-02)
        '
        Me.hScroll.Visible = False
        '
        ' KBotDataView
        '
        Me.Controls.Add(Me.editText)
        Me.Controls.Add(Me.editCombo)
        Me.Controls.Add(Me.vScroll)
        Me.Controls.Add(Me.hScroll)
        Me.ResumeLayout(False)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                components?.Dispose()
                DisposeThemeResources()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
