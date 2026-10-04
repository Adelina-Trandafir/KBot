Option Strict On
Imports KBot.Common

''' <summary>
''' The bar shown while the recorder watches (slice 000T-04): how many steps were taken so far and
''' «Opreste». Bottom right of the screen the main window is on, top-most, never takes the focus (the
''' application under it must keep it); a click on its button still works.
''' </summary>
Friend NotInheritable Class TutorialRecorderBar

    Private Const WS_EX_NOACTIVATE As Integer = &H8000000

    ''' <summary>The operator pressed «Opreste».</summary>
    Public Event StopRequested()

    Public Sub New()
        InitializeComponent()
        Try
            Dim area As Rectangle = Screen.PrimaryScreen.WorkingArea
            Location = New Point(area.Right - Width - 24, area.Bottom - Height - 24)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRecorderBar.New", ex)
            Throw
        End Try
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE
            Return cp
        End Get
    End Property

    ''' <summary>Shows how many steps were recorded.</summary>
    Public Sub SetCount(k_count As Integer)
        lblInfo.Text = "Înregistrez: " & k_count & If(k_count = 1, " pas", " pași") & ". Lucrează în K-BOT ca de obicei."
    End Sub

    Private Sub BtnOpreste_Click(sender As Object, e As EventArgs) Handles btnOpreste.Click
        RaiseEvent StopRequested()
    End Sub

End Class
