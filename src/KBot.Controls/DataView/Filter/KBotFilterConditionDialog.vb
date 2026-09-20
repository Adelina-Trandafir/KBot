Option Strict On
Imports System.ComponentModel
Imports System.Globalization
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
''' <para><b>Pe o coloană de dată operandul se alege din calendar</b> (slice 0072-02): în locul
''' casetei de text stă un <see cref="KBotDatePicker"/> cu formatul coloanei — cu oră, secunde sau
''' milisecunde exact cât arată celula (vezi <see cref="KBotColumnFormat.DateOperandFormat"/>).
''' Câmpul poate rămâne gol (condiția fără operand e inertă, ca înainte), iar ce scrie el se
''' citește înapoi de <see cref="KBotFilterEngine.CoerceOperand"/> în aceeași cultură.</para>
'''
''' <para><b>Nu validează operandul.</b> Un text care nu se citește în tipul coloanei face condiția
''' INERTĂ (vezi <see cref="KBotFilterEngine.MatchesCondition"/>), nu goală: grila arată tot, în loc
''' să arate nimic. A respinge aici, cu un mesaj, ar fi a doua regulă despre același lucru — și cele
''' două ar ajunge să se contrazică la prima cultură cu altă virgulă zecimală.</para>
''' </summary>
Friend NotInheritable Class KBotFilterConditionDialog

    Private ReadOnly _condition As KBotFilterOperator
    Private ReadOnly _valueType As KBotValueType
    Private ReadOnly _twoOperands As Boolean
    Private ReadOnly _dateOperands As Boolean

    ''' <summary>
    ''' Dialogul pentru o condiție, pe o coloană anume. <paramref name="columnCaption"/> e titlul
    ''' coloanei, ca întrebarea să sune ca o propoziție, nu ca o casetă goală.
    ''' <paramref name="dateOperandFormat"/> e formatul câmpului de dată (folosit doar pe o coloană
    ''' <see cref="KBotValueType.DateTime"/>); gol = data scurtă a culturii curente.
    ''' </summary>
    Friend Sub New(condition As KBotFilterOperator, valueType As KBotValueType,
                   columnCaption As String, operand1 As String, operand2 As String,
                   Optional dateOperandFormat As String = Nothing)
        InitializeComponent()
        Try
            _condition = condition
            _valueType = valueType
            _twoOperands = (KBotFilterEngine.OperandCount(condition) = 2)
            _dateOperands = (valueType = KBotValueType.DateTime)

            Dim numeCol As String = If(String.IsNullOrWhiteSpace(columnCaption), "coloana", columnCaption)
            lblPrompt.Text = $"Arată rândurile în care «{numeCol}»" & Environment.NewLine &
                             KBotFilterEngine.OperatorCaption(condition, valueType).TrimEnd("…"c)

            ' Câmpurile de dată vorbesc cultura curentă, aceeași în care motorul de filtrare
            ' citește operandul înapoi: un câmp în ro-RO pe o mașină en-US ar scrie «20.09.2026»
            ' și motorul n-ar înțelege nimic din el.
            If _dateOperands Then
                Dim format As String = If(String.IsNullOrWhiteSpace(dateOperandFormat),
                                          CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
                                          dateOperandFormat.Trim())
                Dim cultura As String = CultureInfo.CurrentCulture.Name     ' gol = invariantă
                For Each dtp As KBotDatePicker In {dtpOperand1, dtpOperand2}
                    If cultura.Length > 0 Then dtp.CultureName = cultura
                    dtp.Format = format
                Next
            End If

            ' Un singur fel de câmp rămâne pe operand: pe o coloană de dată se strâng casetele de
            ' text, pe oricare alta câmpurile de dată. Ascunderea singură nu ajunge: rândurile din
            ' tlyMAIN au înălțime ABSOLUTĂ, deci ar rămâne un gol sub fiecare control ascuns — un
            ' control lipsă, nu o fereastră mai scurtă. Rândurile se STRÂNG la zero, iar fereastra
            ' se scurtează cu exact cât s-a strâns; restul așezării rămâne treaba tabelului.
            Dim strans As Integer = 0
            txtOperand1.Visible = Not _dateOperands
            dtpOperand1.Visible = _dateOperands
            strans += StrangeRandul(If(_dateOperands, CType(txtOperand1, Control), dtpOperand1))

            ' A doua pereche are sens numai la «Între…».
            lblOperand2.Visible = _twoOperands
            txtOperand2.Visible = _twoOperands AndAlso Not _dateOperands
            dtpOperand2.Visible = _twoOperands AndAlso _dateOperands
            If _twoOperands Then
                strans += StrangeRandul(If(_dateOperands, CType(txtOperand2, Control), dtpOperand2))
            Else
                strans += StrangeRandul(lblOperand2) + StrangeRandul(txtOperand2) + StrangeRandul(dtpOperand2)
            End If
            If strans > 0 Then
                ClientSize = New Drawing.Size(ClientSize.Width, ClientSize.Height - strans)
            End If

            lblOperand1.Text = If(_twoOperands, "De la:", "Valoare:")
            If _dateOperands Then
                ScrieData(dtpOperand1, operand1)
                ScrieData(dtpOperand2, operand2)
            Else
                txtOperand1.Text = If(operand1, String.Empty)
                txtOperand2.Text = If(operand2, String.Empty)
            End If
        Catch ex As Exception
            ' Punct de intrare (construcția dialogului): loghează și RE-ARUNCĂ — un dialog pe
            ' jumătate așezat e mai rău decât unul care nu s-a deschis.
            GlobalErrorLog.Write("KBotFilterConditionDialog.New", ex)
            Throw
        End Try
    End Sub

    ' Pune un operand memorat în câmpul de dată: ce se citește ca dată intră ca valoare, ce nu se
    ' citește (sau lipsește) lasă câmpul gol. Un operand tastat pe vremea casetei de text și
    ' rămas de neînțeles nu are ce căuta într-un calendar.
    Private Shared Sub ScrieData(dtp As KBotDatePicker, operand As String)
        Dim d As Date
        If Not String.IsNullOrWhiteSpace(operand) AndAlso
           KBotDatePicker.TryParseDate(operand, dtp.Format, CultureInfo.CurrentCulture, d) Then
            dtp.Value = d
        Else
            dtp.ClearValue()
        End If
    End Sub

    ''' <summary>
    ''' Strânge la zero rândul din <c>tlyMAIN</c> pe care stă controlul dat și întoarce înălțimea
    ''' eliberată. Rândul se CITEȘTE din tabel, nu se scrie ca indice aici: reordonarea lui în
    ''' designer (rostul pentru care totul a intrat în tlyMAIN) n-are voie să strice socoteala.
    ''' </summary>
    Private Function StrangeRandul(ctrl As Control) As Integer
        Dim rand As Integer = tlyMAIN.GetRow(ctrl)
        If rand < 0 OrElse rand >= tlyMAIN.RowStyles.Count Then Return 0

        ' Through the table's own API (slice 0066), so the collapse survives every scale and
        ' theme pass: a fixed row is collapsed, any other is rewritten as a fixed row of 0.
        Dim stil As RowStyle = tlyMAIN.RowStyles(rand)
        Dim inainte As Integer = If(stil.SizeType = SizeType.Absolute, CInt(stil.Height), ctrl.Height)
        If stil.SizeType = SizeType.Absolute Then
            tlyMAIN.SetRowCollapsed(rand, True)
        Else
            tlyMAIN.SetRowHeight(rand, 0F)
        End If
        Return inainte
    End Function

    ''' <summary>
    ''' Primul operand, așa cum l-a tastat operatorul — sau, pe o coloană de dată, data aleasă
    ''' scrisă în formatul câmpului (gol când câmpul e gol).
    ''' </summary>
    Friend ReadOnly Property Operand1 As String
        Get
            Return If(_dateOperands, TextData(dtpOperand1), txtOperand1.Text)
        End Get
    End Property

    ''' <summary>Al doilea operand (gol dacă nu e o condiție cu două capete).</summary>
    Friend ReadOnly Property Operand2 As String
        Get
            If Not _twoOperands Then Return String.Empty
            Return If(_dateOperands, TextData(dtpOperand2), txtOperand2.Text)
        End Get
    End Property

    ''' <summary>Dialogul cere operanzii din calendar (coloană de dată)? Poartă de verificare.</summary>
    Friend ReadOnly Property UsesDateFields As Boolean
        Get
            Return _dateOperands
        End Get
    End Property

    ''' <summary>Formatul câmpurilor de dată. Poartă de verificare.</summary>
    Friend ReadOnly Property DateFieldFormat As String
        Get
            Return dtpOperand1.Format
        End Get
    End Property

    ' Textul unui câmp de dată, în formatul lui: exact ce ar fi tastat operatorul. Textul care e
    ' încă în casetă (netrimis cu Enter sau la pierderea focusului) se citește întâi.
    Private Shared Function TextData(dtp As KBotDatePicker) As String
        dtp.CommitText()
        If Not dtp.HasValue Then Return String.Empty
        Return dtp.Value.ToString(dtp.Format, CultureInfo.CurrentCulture)
    End Function

    Protected Overrides Sub OnShown(e As EventArgs)
        Try
            MyBase.OnShown(e)
            If _dateOperands Then
                dtpOperand1.InnerTextBox.Focus()
                dtpOperand1.InnerTextBox.SelectAll()
            Else
                txtOperand1.Focus()
                txtOperand1.SelectAll()
            End If
        Catch ex As Exception
            ' Boundary UI: focusul nu are voie să arunce în bucla de mesaje.
            GlobalErrorLog.Write("KBotFilterConditionDialog.OnShown", ex)
        End Try
    End Sub

End Class
