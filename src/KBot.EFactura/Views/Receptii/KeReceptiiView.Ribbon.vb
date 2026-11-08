Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' SLICE-KE21-02: KeReceptiiView.Ribbon - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    ''' <summary>The operator's ribbon button of the embedded variant: the text it shows now (follows the mode: «Leagă», «Creează recepția», «Modifică recepția»).</summary>
    Public ReadOnly Property ActionText As String
        Get
            Return btnOk.Text
        End Get
    End Property

    ''' <summary>The ribbon button of the embedded variant can be pressed now.</summary>
    Public ReadOnly Property ActionEnabled As Boolean
        Get
            Return btnOk.Enabled
        End Get
    End Property

    ''' <summary>
    ''' The ribbon button of the work is offered at all: not while an invoice already covered by a reception is only looked at. Slice 02EF-04: the page «Recepții <> EFactura» has no
    ''' button of its own at the bottom any more either; its work is the ribbon button, as in the view «Recepții» of an invoice.
    ''' </summary>
    Public ReadOnly Property ActionAvailable As Boolean
        Get
            Return _viewing Is Nothing AndAlso Not _assocTab
        End Get
    End Property

    ''' <summary>What <see cref="ActionText"/>, <see cref="ActionEnabled"/> or <see cref="ActionAvailable"/> answer has changed (the ribbon asks again).</summary>
    Public Event ActionStateChanged As EventHandler

    ''' <summary>The ribbon button was pressed: the same work as the button of the page (when it can be pressed). UI boundary: logs and swallows.</summary>
    Public Sub RunAction()
        Try
            If Not btnOk.Enabled Then Return
            BtnOk_Click(btnOk, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.RunAction", ex)
        End Try
    End Sub

    ''' <summary>Slice 02EF-11: the angajament of the reception saved last (see <see cref="OrdDisponibil"/>).</summary>
    Public ReadOnly Property OrdCod As String
        Get
            Return _ordCod
        End Get
    End Property

    ''' <summary>Slice 02EF-11: the snapshot (IDRH) of the reception saved last: the ordonantare is built on it, not on payments.</summary>
    Public ReadOnly Property OrdIdrh As Integer
        Get
            Return _ordIdrh
        End Get
    End Property

    ''' <summary>Slice 02EF-11: the ribbon button «Genereaza ordonantare» is offered: this page saved a reception and nothing was ticked since.</summary>
    Public ReadOnly Property OrdDisponibil As Boolean
        Get
            Return Not _embedded AndAlso _ordIdrh > 0 AndAlso _ordCod.Length > 0
        End Get
    End Property

    ''' <summary>Slice 02EF-12: the ribbon button «Creeaza rezervare» is offered: a new or a modified reception on a chosen angajament (never while an invoice is only looked at).</summary>
    Public ReadOnly Property RezervareAvailable As Boolean
        Get
            Return _viewing Is Nothing AndAlso Not _assocTab AndAlso _mode <> EFacturaKeTip.Existenta AndAlso _data IsNot Nothing
        End Get
    End Property

    ''' <summary>Slice 02EF-12: the ribbon button «Creeaza rezervare» can be pressed: the ticked invoices put an amount on at least one classification.</summary>
    Public ReadOnly Property RezervareEnabled As Boolean
        Get
            Return RezervareAvailable AndAlso Not _busy AndAlso ReceptionTotals().Values.Any(Function(k_v) k_v > Tolerance)
        End Get
    End Property

    ''' <summary>
    ''' Slice 02EF-12 (operator, 07.11.2026): the ribbon button «Creeaza rezervare» was pressed. The shell opens the window «Adauga rezervare» with the value of each
    ''' indicator taken from the reception; saving there is refused while a value cannot make the reception fit. UI boundary (async Sub): logs and tells the operator.
    ''' </summary>
    Public Async Sub RunRezervare()
        Try
            If Not RezervareEnabled Then Return
            If KeRobot.Current Is Nothing Then
                Notify("Fereastra de rezervare nu este disponibilă în acest context.", MessageBoxIcon.Information)
                Return
            End If
            Dim k_cerere As New KeRezervareDeCreat() With {.CodAngajament = _angCod}
            For Each k_pair As KeyValuePair(Of Integer, Decimal) In ReceptionTotals()
                If k_pair.Value <= Tolerance Then Continue For
                Dim k_ind As EFacturaKeIndicator = IndicatorOf(k_pair.Key)
                If k_ind Is Nothing Then
                    Notify("O clasificație a recepției nu este indicator al angajamentului: este nevoie întâi de o revizie nouă de rezervare cu acel indicator (documentul de fundamentare).",
                           MessageBoxIcon.Warning)
                    Return
                End If
                Dim k_dispo As EFacturaKeDisponibil = DispoOf(k_pair.Key)
                k_cerere.Randuri.Add(New KeRezervareRand() With {
                    .CodIndicator = k_ind.CodIndicator, .Clsf = k_ind.Clsf, .SumaReceptie = k_pair.Value,
                    .Rezervat = If(k_dispo IsNot Nothing AndAlso k_dispo.RezervariAng.HasValue, k_dispo.RezervariAng.Value, 0D),
                    .Receptionat = If(k_dispo IsNot Nothing AndAlso k_dispo.ReceptiiAng.HasValue, k_dispo.ReceptiiAng.Value, 0D)})
            Next
            Dim k_result As KeRezervareCreata = Await KeRobot.Current.CreeazaRezervareAsync(k_cerere).ConfigureAwait(True)
            If IsDisposed Then Return
            If k_result Is Nothing Then Return
            If k_result.Salvata Then
                ShowNotice("Rezervarea este salvată în K-BOT. Trimite-o în FOREXE din vederea DDF a angajamentului; după trimitere, cifrele din pagină se actualizează.", NoticeKind.Success)
            ElseIf k_result.Mesaj.Length > 0 Then
                Notify(k_result.Mesaj, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.RunRezervare", ex)
            ShowNotice("Fereastra de rezervare nu a putut fi deschisă. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' Slice 02EF-11: the operator went on to other invoices: the reception saved before is no longer the work in hand, so the ribbon button of its ordonantare goes.
    Friend Sub ForgetOrdSource()
        If _ordIdrh = 0 AndAlso _ordCod.Length = 0 Then Return
        _ordIdrh = 0
        _ordCod = String.Empty
        RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
    End Sub

    ' Slice 02EF-12: the reservations or receptions of an angajament changed somewhere in the app: the figures of this page are asked again, and the message about too few
    ' reservations is judged again with them. UI boundary (static event): logs and swallows.
    Private Sub KeRobot_DateAngajamentSchimbate(sender As Object, k_cod As String)
        Try
            If IsDisposed OrElse _api Is Nothing Then Return
            If k_cod.Length > 0 AndAlso Not String.Equals(k_cod, _angCod, StringComparison.OrdinalIgnoreCase) Then Return
            _dispoCache.Clear()
            _viewDispo.Clear()
            _dispoKey = String.Empty
            If _data IsNot Nothing Then BeginRefreshDispo()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.KeRobot_DateAngajamentSchimbate", ex)
        End Try
    End Sub

    ' UI boundary: the button the operator sees in the ribbon follows the one of the page.
    Private Sub BtnOk_StateChanged(sender As Object, e As EventArgs) Handles btnOk.EnabledChanged, btnOk.TextChanged
        Try
            RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.BtnOk_StateChanged", ex)
        End Try
    End Sub

End Class
