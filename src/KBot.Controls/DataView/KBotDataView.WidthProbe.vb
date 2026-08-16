Option Strict On
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' SONDA DE LĂȚIMI — unealtă de dezvoltare, existentă DOAR în compilarea Debug.
'''
''' <para>Click DREAPTA pe un antet de coloană, în timpul rulării: se deschide un meniu care spune
''' cât are coloana aceea. Rostul e drumul invers față de designer — operatorul trage marginile până
''' arată bine PE ECRAN, apoi citește numărul de aici și îl scrie în <c>.Designer.vb</c>. Fără el,
''' lățimile se ghicesc din ochi și se corectează prin recompilări.</para>
'''
''' <para>Se arată DOUĂ numere, fiindcă ele diferă de la 125% în sus: <b>lățimea logică</b> (px la
''' 96 dpi) e cea care se pune în designer, iar cea de <b>ecran</b> e doar ce a măsurat tragerea.
''' Confundarea lor e chiar capcana din felia 0035-01 — o valoare de ecran pusă în designer se
''' scalează încă o dată la următoarea încărcare.</para>
'''
''' <para>Meniul copiază în clipboard fie linia coloanei, fie toate lățimile vizibile, gata de lipit
''' în <c>.Designer.vb</c>. Nu modifică nimic din grilă.</para>
''' </summary>
Partial Public Class KBotDataView

#If DEBUG Then

    ''' <summary>
    ''' Click dreapta în banda de antet: deschide sonda. True = apăsarea a fost consumată.
    ''' Metodă de dezvoltare — orice eșec se loghează și se înghite, nu are voie să strice grila.
    ''' </summary>
    Private Function HandleHeaderWidthProbe(location As Point) As Boolean
        Try
            If Not _showHeader Then Return False
            If KBotDesignTime.IsDesignTime(Me) Then Return False
            If location.Y < 0 OrElse location.Y >= HeaderBandHeight() Then Return False

            Dim col As KBotDataColumn = ColumnAtX(location.X)
            If col Is Nothing Then Return False

            Dim titlu As String = If(String.IsNullOrWhiteSpace(col.HeaderText), col.Key, col.HeaderText)
            Dim items As New List(Of CustomPopupItem) From {
                New CustomPopupItem("titlu", $"Coloana «{titlu}»  [{col.Key}]") With {.Enabled = False},
                CustomPopupItem.Separator(),
                New CustomPopupItem("logic", $"Lățime (designer): {col.Width} px") With {.Enabled = False},
                New CustomPopupItem("ecran", $"Lățime pe ecran: {col.WidthPx} px  ({DebugScaleText()})") With {.Enabled = False},
                New CustomPopupItem("min", $"MinWidth: {col.MinWidth} px · AutoSize: {col.AutoSizeMode}") With {.Enabled = False},
                CustomPopupItem.Separator(),
                New CustomPopupItem("copyOne", "&Copiază linia acestei coloane"),
                New CustomPopupItem("copyAll", "Copiază &toate lățimile vizibile")
            }

            Dim meniu As New CustomPopup(items)
            AddHandler meniu.ItemClicked,
                Sub(s As Object, e As CustomPopupItemEventArgs)
                    Try
                        Select Case e.Item.Key
                            Case "copyOne" : Clipboard.SetText(DebugWidthLine(col))
                            Case "copyAll" : Clipboard.SetText(DebugAllWidthLines())
                        End Select
                    Catch ex As Exception
                        GlobalErrorLog.Write("KBotDataView.HandleHeaderWidthProbe.ItemClicked", ex)
                    End Try
                End Sub
            meniu.ShowAtCursor(Me)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.HandleHeaderWidthProbe", ex)
            Return False
        End Try
    End Function

    ' Scara la care desenează grila acum — ca să se vadă de ce cele două numere diferă.
    Private Function DebugScaleText() As String
        Return $"{CInt(Math.Round(DeviceDpi * 100.0R / 96.0R))}%"
    End Function

    ' O linie gata de lipit în .Designer.vb, în lățimi LOGICE.
    Private Function DebugWidthLine(col As KBotDataColumn) As String
        Return $"{col.Key}.Width = {col.Width}"
    End Function

    Private Function DebugAllWidthLines() As String
        Dim sb As New StringBuilder()
        For Each c In VisibleColumns()
            sb.AppendLine(DebugWidthLine(c))
        Next
        Return sb.ToString()
    End Function

#End If

End Class
