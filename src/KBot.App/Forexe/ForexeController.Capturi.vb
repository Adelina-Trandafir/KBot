Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Forexe

''' <summary>
''' The pictures of the FOREXE page (operator, 28.09.2026) — the coordinator's half.
'''
''' <para>The ALOP guide wants captures of the system inside both documents the operator
''' signs: the reservations in the document of fundamentare (GUIDE p.11-14, example p.41) and
''' two reception pictures in the ordonanțare (p.21-22 — the Recepții tab and «Informații
''' complete contract»). Until now the operator took them with PrtScr and pasted them in by
''' hand. K-BOT takes them at the moments the page script names:</para>
'''
''' <list type="bullet">
''' <item>a reservation session: one BEFORE the first eye is pressed (the page holds that
''' click and asks for it — <see cref="IaCapturaCerutaAsync"/>), one AFTER the last save,
''' when the operator says they are done and the page is back on the Buget tab;</item>
''' <item>a reception: the Recepții tab as it stands after the save, then «Informații
''' complete contract» scrolled to its right end.</item>
''' </list>
'''
''' <para><b>Nothing here is allowed to cost the operator their work.</b> Every path answers
''' with Nothing / False and says why; the click the page is holding is released either way.
''' The pictures wait on disk (<see cref="CapturaStore"/>) until the download and the ingest
''' have written the records they hang off.</para>
''' </summary>
Partial Public NotInheritable Class ForexeController

    Private ReadOnly _marcaje As MarcajeRecente

    ''' <summary>The markers last handed to the page; read by the shell at upload time.</summary>
    Public ReadOnly Property Marcaje As MarcajeRecente
        Get
            Return _marcaje
        End Get
    End Property

    ''' <summary>«Pagina originală» lifts the operator's own CSS rules for the picture.</summary>
    Private Shared ReadOnly Property PaginaOriginala As Boolean
        Get
            Return AppSettings.Current.ForexeCapturaPaginaOriginala
        End Get
    End Property

    ''' <summary>
    ''' The page is holding a click and wants a picture of what is on screen (the eye of the
    ''' first reservation row). True when it was taken AND written to disk — the page is told
    ''' either way and its click goes through.
    ''' </summary>
    Private Async Function IaCapturaCerutaAsync(tip As String, ct As CancellationToken) As Task(Of Boolean)
        Try
            Dim cod As String = Await CodulDinPaginaAsync()
            Dim captura As CapturaForexe =
                Await IaCapturaAsync(cod, CapturaStore.FelRezervare, CapturaStore.MomentInainte)
            Return captura IsNot Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.IaCapturaCerutaAsync", ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' One picture of the page as it stands, written to disk. Nothing when there is no
    ''' session, the page would not be photographed, or the file could not be written — said
    ''' on the console, never thrown at the operator.
    ''' </summary>
    ''' <param name="unaSingura">
    ''' True = do nothing when a picture of this kind and moment is already waiting for this
    ''' angajament. A session whose download failed is tried again, and the second «DA» must
    ''' not put a second «after» picture into the same document.
    ''' </param>
    Public Async Function IaCapturaAsync(cod As String, fel As String, moment As String,
                                         Optional unaSingura As Boolean = False) As Task(Of CapturaForexe)
        Try
            If Not IsConnected Then Return Nothing
            If unaSingura Then
                Dim asteapta As CapturaForexe =
                    CapturaStore.AleSale(cod, fel).
                        FirstOrDefault(Function(c) String.Equals(c.Moment, moment, StringComparison.OrdinalIgnoreCase))
                If asteapta IsNot Nothing Then Return asteapta
            End If
            Dim octeti As Byte() = Await _runner.CapturePaginaAsync(PaginaOriginala)
            Return PastreazaCaptura(cod, fel, moment, octeti)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.IaCapturaAsync", ex)
            SpuneStare("Captura paginii FOREXE nu a reușit: " & ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' The guide's second reception picture: «Afișează informații complete», the table
    ''' pushed to its right end (Recepții and Plăți live there), the picture, «Înapoi».
    ''' Nothing when the page does not offer the button.
    ''' </summary>
    Public Async Function IaCapturaInfoCompleteAsync(cod As String) As Task(Of CapturaForexe)
        Try
            If Not IsConnected Then Return Nothing
            Dim octeti As Byte() = Await _runner.CaptureInfoCompleteAsync(PaginaOriginala)
            Return PastreazaCaptura(cod, CapturaStore.FelReceptie, CapturaStore.MomentInfoComplete, octeti)
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.IaCapturaInfoCompleteAsync", ex)
            SpuneStare("Captura «Informații complete contract» nu a reușit: " & ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' The bytes on disk, with the number of the marker the page was given, when there is
    ''' one. Nothing (and a word on the console) when there is nothing to keep.
    ''' </summary>
    Private Function PastreazaCaptura(cod As String, fel As String, moment As String,
                                      octeti As Byte()) As CapturaForexe
        If octeti Is Nothing OrElse octeti.Length = 0 Then
            SpuneStare("Pagina FOREXE nu a putut fi fotografiată; documentul se va face fără captura asta.")
            Return Nothing
        End If
        Dim marcaj As Integer = MarcajulDe(fel, cod)
        Dim captura As CapturaForexe =
            CapturaStore.Salveaza(cod, fel, moment, octeti, marcaj)
        SpuneStare($"Captura «{moment}» a paginii FOREXE e păstrată ({octeti.Length \ 1024} KB).")
        Return captura
    End Function

    ''' <summary>
    ''' The id a picture of this kind hangs off: the coming DDF revision for a reservation,
    ''' the reception snapshot for a reception. Zero while the page has no marker yet — the
    ''' «before» picture is taken before the first save, so its number is filled in later.
    ''' </summary>
    Private Function MarcajulDe(fel As String, cod As String) As Integer
        If String.Equals(fel, CapturaStore.FelReceptie, StringComparison.OrdinalIgnoreCase) Then
            Return _marcaje.Numar(MarcajeRecente.TipReceptie, cod, MarcajeRecente.CheieIdrh)
        End If
        Return _marcaje.Numar(MarcajeRecente.TipRezervare, cod, MarcajeRecente.CheieIdrev)
    End Function

    ''' <summary>
    ''' Puts the number the page later received on the pictures that were taken before it
    ''' existed, and hands back everything of that kind waiting for this angajament.
    ''' </summary>
    Public Function CapturileDe(cod As String, fel As String) As List(Of CapturaForexe)
        Try
            Dim lista As List(Of CapturaForexe) = CapturaStore.AleSale(cod, fel)
            Dim marcaj As Integer = MarcajulDe(fel, cod)
            If marcaj > 0 Then
                For Each captura As CapturaForexe In lista
                    If captura.Marcaj <= 0 Then CapturaStore.PuneMarcaj(captura, marcaj)
                Next
            End If
            Return lista
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.CapturileDe", ex)
            Return New List(Of CapturaForexe)()
        End Try
    End Function

    ''' <summary>
    ''' The reservation session is filed: the next one photographs its list again, and the
    ''' marker of this one means nothing any more.
    ''' </summary>
    Public Async Function IncheieSesiuneaDeCapturiAsync(cod As String) As Task
        Try
            _marcaje.Uita(MarcajeRecente.TipRezervare, cod)
            If IsConnected Then Await _runner.ResetShotSessionAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.IncheieSesiuneaDeCapturiAsync", ex)
        End Try
    End Function

    ''' <summary>The angajament the page shows, or an empty text; never throws.</summary>
    Private Async Function CodulDinPaginaAsync() As Task(Of String)
        Try
            Return Await _runner.ReadPageAngajamentAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.CodulDinPaginaAsync", ex)
            Return String.Empty
        End Try
    End Function

End Class
