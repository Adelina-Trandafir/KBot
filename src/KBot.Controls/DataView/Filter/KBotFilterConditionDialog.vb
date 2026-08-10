Option Strict On
Imports System.ComponentModel
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Dialogul de CONDIȚIE al filtrului de coloană (slice 0028-03) — echivalentul ferestrei «Custom
''' Filter» din Access: se deschide după ce operatorul a ales o condiție din submeniul «Filtre
''' text / numerice / de dată» și îi cere operandul (doi, pentru «Între…»).
'''
''' <para>E un dialog MODAL obișnuit, nu un popup: cere o valoare tastată și un răspuns
''' da/nu, adică exact treaba unui dialog. Se tematizează prin <see cref="KBotThemedForm"/>,
''' deci nu are nicio culoare scrisă în el.</para>
'''
''' <para><b>Nu validează operandul.</b> Un text care nu se citește în tipul coloanei face condiția
''' INERTĂ (vezi <see cref="KBotFilterEngine.MatchesCondition"/>), nu goală: grila arată tot, în loc
''' să arate nimic. A respinge aici, cu un mesaj, ar fi a doua regulă despre același lucru — și cele
''' două ar ajunge să se contrazică la prima cultură cu altă virgulă zecimală.</para>
''' </summary>
Friend NotInheritable Class KBotFilterConditionDialog

    Private ReadOnly _condition As KBotFilterOperator
    Private ReadOnly _valueType As KBotValueType

    ''' <summary>
    ''' Dialogul pentru o condiție, pe o coloană anume. <paramref name="columnCaption"/> e titlul
    ''' coloanei, ca întrebarea să sune ca o propoziție, nu ca o casetă goală.
    ''' </summary>
    Friend Sub New(condition As KBotFilterOperator, valueType As KBotValueType,
                   columnCaption As String, operand1 As String, operand2 As String)
        InitializeComponent()
        _condition = condition
        _valueType = valueType

        Dim numeCol As String = If(String.IsNullOrWhiteSpace(columnCaption), "coloana", columnCaption)
        lblPrompt.Text = $"Arată rândurile în care «{numeCol}»" & Environment.NewLine &
                         KBotFilterEngine.OperatorCaption(condition, valueType).TrimEnd("…"c)

        ' A doua casetă are sens numai la «Între…»; altfel nu se ascunde doar, ci se și strânge
        ' fereastra peste ea — un gol de 50 de pixeli sub o casetă arată ca un control lipsă.
        Dim doiOperanzi As Boolean = (KBotFilterEngine.OperandCount(condition) = 2)
        lblOperand2.Visible = doiOperanzi
        txtOperand2.Visible = doiOperanzi
        If Not doiOperanzi Then
            Dim scade As Integer = txtOperand2.Bottom - txtOperand1.Bottom
            btnOk.Top -= scade
            btnCancel.Top -= scade
            ClientSize = New Drawing.Size(ClientSize.Width, ClientSize.Height - scade)
        End If

        lblOperand1.Text = If(doiOperanzi, "De la:", "Valoare:")
        txtOperand1.Text = If(operand1, String.Empty)
        txtOperand2.Text = If(operand2, String.Empty)
    End Sub

    ''' <summary>Primul operand, așa cum l-a tastat operatorul.</summary>
    Friend ReadOnly Property Operand1 As String
        Get
            Return txtOperand1.Text
        End Get
    End Property

    ''' <summary>Al doilea operand (gol dacă nu e o condiție cu două capete).</summary>
    Friend ReadOnly Property Operand2 As String
        Get
            Return If(txtOperand2.Visible, txtOperand2.Text, String.Empty)
        End Get
    End Property

    Protected Overrides Sub OnShown(e As EventArgs)
        Try
            MyBase.OnShown(e)
            txtOperand1.Focus()
            txtOperand1.SelectAll()
        Catch ex As Exception
            ' Boundary UI: focusul nu are voie să arunce în bucla de mesaje.
            GlobalErrorLog.Write("KBotFilterConditionDialog.OnShown", ex)
        End Try
    End Sub

End Class
