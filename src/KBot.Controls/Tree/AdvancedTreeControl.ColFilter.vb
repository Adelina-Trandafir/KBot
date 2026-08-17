Imports System.Linq

''' <summary>
''' Popup flotant pentru filtrarea pe o coloană specifică din TreeListView.
''' Conține un TextBox (Enter = filtrare) și un ListBox cu valori distincte.
''' Se închide automat la OnDeactivate.
''' </summary>
Partial Public Class AdvancedTreeControl
    Private NotInheritable Class ColFilterPopup
        Inherits Form

        Private ReadOnly _owner   As AdvancedTreeControl
        Private ReadOnly _colName As String
        Private _lblTitle  As Label
        Private _textBox   As TextBox
        Private _separator As Panel
        Private _listBox   As ListBox

        ' Măsurile ferestrei, LOGICE (px @96dpi). Se scalează cu SP() de mai jos — fereastra e
        ' construită în cod, deci nimic n-o scalează în locul nostru: până în felia 0040 rămânea
        ' lată de 230 px cu un font cu 50% mai mare, iar lista și caseta se înghesuiau în ea.
        Private Const LATIME_LOGICA As Integer = 230
        Private Const INALTIME_TITLU As Integer = 24
        Private Const MARGINE_CASETA As Integer = 6
        Private Const TOP_CASETA As Integer = 28
        Private Const AER_SUB_CASETA As Integer = 6
        Private Const AER_JOS As Integer = 3
        Private Const MAX_LINII_LISTA As Integer = 8   ' linii, nu pixeli — nu se scalează

        ' Scara ferestrei — aceeași sursă unică (AppScaling) ca a arborelui care o deschide.
        Private Function SP(logical As Integer) As Integer
            Return CInt(Math.Round(logical * AppScaling.FactorFor(Me)))
        End Function

        ' ────────────────────────────────────────────────────────────────
        Friend Sub New(owner As AdvancedTreeControl, colName As String, screenPos As Point)
            _owner   = owner
            _colName = colName

            Dim latime As Integer = SP(LATIME_LOGICA)

            ' ── Form ────────────────────────────────────────────────────
            Me.FormBorderStyle = FormBorderStyle.None
            Me.ShowInTaskbar   = False
            Me.TopMost         = True
            Me.StartPosition   = FormStartPosition.Manual
            Me.BackColor       = Color.FromArgb(250, 250, 252)
            Me.Width           = latime

            ' ── Title ───────────────────────────────────────────────────
            _lblTitle = New Label() With {
                .Text      = "  " & colName,
                .Font      = New Font(owner.Font, FontStyle.Bold),
                .BackColor = Color.FromArgb(228, 228, 244),
                .ForeColor = Color.FromArgb(40, 40, 80),
                .Height    = SP(INALTIME_TITLU),
                .Width     = latime,
                .Location  = New Point(0, 0),
                .TextAlign = ContentAlignment.MiddleLeft
            }

            ' ── TextBox ─────────────────────────────────────────────────
            _textBox = New TextBox() With {
                .BorderStyle = BorderStyle.FixedSingle,
                .Font        = owner.Font,
                .Width       = latime - SP(MARGINE_CASETA) * 2,
                .Location    = New Point(SP(MARGINE_CASETA), SP(TOP_CASETA))
            }
            ' Pre-populează cu filtrul activ (dacă există)
            If owner._activeColFilters.ContainsKey(colName) Then
                _textBox.Text = owner._activeColFilters(colName)
            End If

            ' ── Separator ───────────────────────────────────────────────
            Dim tbBottom As Integer = _textBox.Top + _textBox.PreferredHeight + SP(AER_SUB_CASETA)
            _separator = New Panel() With {
                .BackColor = Color.FromArgb(200, 200, 215),
                .Height    = 1,
                .Width     = latime,
                .Location  = New Point(0, tbBottom)
            }

            ' ── ListBox ─────────────────────────────────────────────────
            _listBox = New ListBox() With {
                .BorderStyle    = BorderStyle.None,
                .Font           = owner.Font,
                .IntegralHeight = True,
                .Location       = New Point(0, tbBottom + 1),
                .Width          = latime
            }
            _listBox.Items.Add("(Toate)")
            For Each v In owner.GetDistinctColumnValues(colName)
                _listBox.Items.Add(v)
            Next
            ' Pre-selectează valoarea curentă (dacă există în listă)
            If owner._activeColFilters.ContainsKey(colName) Then
                Dim cur As String = owner._activeColFilters(colName)
                Dim idx As Integer = _listBox.Items.IndexOf(cur)
                If idx >= 0 Then _listBox.SelectedIndex = idx
            End If
            ' Înălțime: max 8 linii vizibile (ItemHeight urmează fontul, deci scara)
            Dim visItems As Integer = Math.Min(_listBox.Items.Count, MAX_LINII_LISTA)
            _listBox.Height = _listBox.ItemHeight * visItems + 2

            ' ── Form height ─────────────────────────────────────────────
            Me.Height = _listBox.Top + _listBox.Height + SP(AER_JOS)

            ' ── Adaugă controalele ──────────────────────────────────────
            Me.Controls.AddRange(New Control() {_lblTitle, _textBox, _separator, _listBox})

            ' ── Poziționare — ajustare dacă iese din ecran ──────────────
            Me.Location = screenPos
            Dim scr As Rectangle = Screen.FromPoint(screenPos).WorkingArea
            If Me.Right  > scr.Right  Then Me.Left = scr.Right  - Me.Width
            If Me.Bottom > scr.Bottom Then Me.Top  = screenPos.Y - Me.Height

            ' ── Events ──────────────────────────────────────────────────
            AddHandler _textBox.KeyDown, AddressOf OnTextBoxKeyDown
            AddHandler _listBox.Click,   AddressOf OnListBoxClick
        End Sub

        ' ────────────────────────────────────────────────────────────────
        Protected Overrides Sub OnShown(e As EventArgs)
            Try
                MyBase.OnShown(e)
                _textBox.Focus()
                _textBox.SelectAll()
            Catch ex As Exception
                GlobalErrorLog.Write("ColFilterPopup.OnShown", ex)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Try
                MyBase.OnPaint(e)
                Using pen As New Pen(Color.FromArgb(160, 160, 200), 1)
                    e.Graphics.DrawRectangle(pen, 0, 0, Me.Width - 1, Me.Height - 1)
                End Using
            Catch ex As Exception
                GlobalErrorLog.Write("ColFilterPopup.OnPaint", ex)
            End Try
        End Sub

        Protected Overrides Sub OnDeactivate(e As EventArgs)
            Try
                MyBase.OnDeactivate(e)
                Me.Close()
            Catch ex As Exception
                GlobalErrorLog.Write("ColFilterPopup.OnDeactivate", ex)
            End Try
        End Sub

        Protected Overrides Sub OnClosed(e As EventArgs)
            Try
                MyBase.OnClosed(e)
                ReturnFocusToOwner()
            Catch ex As Exception
                GlobalErrorLog.Write("ColFilterPopup.OnClosed", ex)
            End Try
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If _owner._activeColFilterPopup Is Me Then
                    _owner._activeColFilterPopup = Nothing
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        ' ────────────────────────────────────────────────────────────────
        Private Sub OnTextBoxKeyDown(sender As Object, e As KeyEventArgs)
            Try
                Select Case e.KeyCode
                    Case Keys.Return
                        e.SuppressKeyPress = True
                        ApplyFilter(_textBox.Text.Trim())
                        Me.Close()
                    Case Keys.Escape
                        Me.Close()
                End Select
            Catch ex As Exception
                GlobalErrorLog.Write("ColFilterPopup.OnTextBoxKeyDown", ex)
            End Try
        End Sub

        Private Sub OnListBoxClick(sender As Object, e As EventArgs)
            Try
                If _listBox.SelectedIndex < 0 Then Return
                Dim selected As String = _listBox.SelectedItem.ToString()
                ApplyFilter(If(selected = "(Toate)", "", selected))
                Me.Close()
            Catch ex As Exception
                GlobalErrorLog.Write("ColFilterPopup.OnListBoxClick", ex)
            End Try
        End Sub

        Private Sub ApplyFilter(text As String)
            If String.IsNullOrEmpty(text) Then
                _owner._activeColFilters.Remove(_colName)
            Else
                _owner._activeColFilters(_colName) = text
            End If
            _owner.ApplyColumnFilters()
        End Sub

        Private Sub ReturnFocusToOwner()
            Try
                If _owner Is Nothing OrElse _owner.IsDisposed OrElse Not _owner.IsHandleCreated Then Return
                _owner.BeginInvoke(New Action(
                    Sub()
                        Try
                            If Not _owner.IsDisposed AndAlso _owner._activeColFilterPopup Is Nothing Then
                                _owner.Focus()
                            End If
                        Catch
                        End Try
                    End Sub))
            Catch
            End Try
        End Sub

    End Class  ' ColFilterPopup
End Class  ' AdvancedTreeControl (partial)
