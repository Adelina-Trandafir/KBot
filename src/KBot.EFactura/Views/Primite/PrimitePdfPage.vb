Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The view «Factură PDF» of a received invoice: a horizontal bar over one document area. «Factură clasică» = the invoice K-BOT draws
''' from what it stores, «Factură ANAF» = the one ANAF's service draws from the XML, «Atașamente» = a popup with the NAMES of the files
''' embedded in the invoice; the one chosen opens in the same area (a PDF in the embedded viewer, an image in a picture box).
'''
''' <para>The page does not decide WHAT to fetch: it raises <see cref="ModeChanged"/> / <see cref="AttachmentChosen"/> and
''' <see cref="PrimiteView"/> prepares the file and calls <see cref="ShowDocument"/> / <see cref="ShowImage"/> / <see cref="ShowNotice"/>.</para>
''' </summary>
Public Class PrimitePdfPage
    Implements IThemedContainer

    ''' <summary>The keys of the bar (written in the designer).</summary>
    Public Const ModeClassic As String = "clasica"
    Public Const ModeAnaf As String = "anaf"
    Public Const ModeAttachments As String = "atasamente"

    Private Const AttachmentsCaption As String = "Atașamente"
    Private Const MaxCaptionName As Integer = 28

    ''' <summary>The bar now shows «Factură clasică» or «Factură ANAF» (<paramref name="k_mode"/>).</summary>
    Public Event ModeChanged(k_mode As String)

    ''' <summary>The operator chose, in the popup, the attachment with this <see cref="EFacturaPrimitaAtasament.Index"/>.</summary>
    Public Event AttachmentChosen(k_index As Integer)

    Private _attachments As New List(Of EFacturaPrimitaAtasament)()
    Private _previousKey As String = ModeClassic
    Private _picked As Boolean

    Public Sub New()
        InitializeComponent()
        navDoc.SelectedKey = ModeClassic
    End Sub

    ''' <summary>Makes the viewer (set by the host before the page is first shown); forwarded to the document area.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ViewerFactory As Func(Of IFacturaPdfViewer)
        Get
            Return pgViewer.ViewerFactory
        End Get
        Set(k_value As Func(Of IFacturaPdfViewer))
            pgViewer.ViewerFactory = k_value
        End Set
    End Property

    ''' <summary>What is on screen now ("invoice|what"): the same again is not opened twice.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Stamp As String

    ''' <summary>The bar's choice: <see cref="ModeClassic"/>, <see cref="ModeAnaf"/> or <see cref="ModeAttachments"/>.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Mode As String
        Get
            Return If(navDoc.SelectedKey, ModeClassic)
        End Get
    End Property

    ''' <summary>Gives the page the files embedded in the shown invoice; the «Atașamente» button shows only when there are some.</summary>
    Public Sub SetAttachments(k_files As IEnumerable(Of EFacturaPrimitaAtasament))
        _attachments = If(k_files Is Nothing, New List(Of EFacturaPrimitaAtasament)(), New List(Of EFacturaPrimitaAtasament)(k_files))
        navDoc.SetItemText(ModeAttachments, AttachmentsCaption)
        navDoc.SetItemVisible(ModeAttachments, _attachments.Count > 0)
        ' A new invoice: an attachment of the previous one is no longer on screen, so the bar goes back to the classic invoice.
        If String.Equals(navDoc.SelectedKey, ModeAttachments, StringComparison.Ordinal) Then
            _previousKey = ModeClassic
            navDoc.SelectedKey = ModeClassic
        End If
    End Sub

    ''' <summary>Shows the PDF at <paramref name="k_path"/> (an existing file).</summary>
    Public Sub ShowDocument(k_path As String)
        ShowViewer()
        pgViewer.ShowDocument(k_path)
    End Sub

    ''' <summary>A plain line instead of a document («Se aduce…», or why there is none).</summary>
    Public Sub ShowNotice(k_message As String)
        Stamp = Nothing
        ShowViewer()
        pgViewer.ShowNotice(k_message)
    End Sub

    ''' <summary>Draws an image attachment. The bytes are copied into a bitmap of the page's own (the stream is not kept).</summary>
    ''' <exception cref="ArgumentException">The bytes are not an image GDI+ knows.</exception>
    Public Sub ShowImage(k_bytes As Byte())
        Try
            ArgumentNullException.ThrowIfNull(k_bytes)
            Dim k_image As Bitmap
            Using k_stream As New MemoryStream(k_bytes)
                Using k_original As Image = Image.FromStream(k_stream)
                    k_image = New Bitmap(k_original)
                End Using
            End Using
            Dim k_old As Image = picAtas.Image
            picAtas.Image = k_image
            k_old?.Dispose()
            pgViewer.ReleaseDocument()
            pgViewer.Visible = False
            picAtas.Visible = True
        Catch ex As Exception
            GlobalErrorLog.Write("PrimitePdfPage.ShowImage", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Lets go of what is on screen (the PDF and the picture).</summary>
    Public Sub ReleaseDocument()
        Stamp = Nothing
        pgViewer.ReleaseDocument()
        Dim k_old As Image = picAtas.Image
        picAtas.Image = Nothing
        k_old?.Dispose()
    End Sub

    ' The document area shows the viewer (and not the picture).
    Private Sub ShowViewer()
        picAtas.Visible = False
        pgViewer.Visible = True
    End Sub

    ' ── The bar ─────────────────────────────────────────────────────────────────

    ''' <summary>UI boundary: logs and swallows.</summary>
    Private Sub NavDoc_SelectionChanged(k_key As String) Handles navDoc.SelectionChanged
        Try
            If String.IsNullOrEmpty(k_key) Then Return
            If String.Equals(k_key, ModeAttachments, StringComparison.Ordinal) Then
                ShowPicker()
                Return
            End If
            _previousKey = k_key
            navDoc.SetItemText(ModeAttachments, AttachmentsCaption)
            RaiseEvent ModeChanged(k_key)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimitePdfPage.NavDoc_SelectionChanged", ex)
        End Try
    End Sub

    ' The popup with the names of the attachments. Dismissed without a choice: the bar goes back to what it showed.
    Private Sub ShowPicker()
        Dim k_items As New List(Of CustomPopupItem)()
        For Each k_file As EFacturaPrimitaAtasament In _attachments
            k_items.Add(New CustomPopupItem(k_file.Index.ToString(CultureInfo.InvariantCulture), k_file.Nume))
        Next
        If k_items.Count = 0 Then
            navDoc.SelectedKey = _previousKey
            Return
        End If
        _picked = False
        Dim k_menu As New CustomPopup(k_items)
        AddHandler k_menu.ItemClicked, AddressOf Picker_ItemClicked
        AddHandler k_menu.FormClosed, AddressOf Picker_Closed
        k_menu.ShowAt(navDoc, Control.MousePosition)
    End Sub

    ' UI boundary: logs and swallows.
    Private Sub Picker_ItemClicked(k_sender As Object, k_args As CustomPopupItemEventArgs)
        Try
            _picked = True
            Dim k_index As Integer = Integer.Parse(k_args.Item.Key, CultureInfo.InvariantCulture)
            Dim k_name As String = k_args.Item.Text
            If k_name.Length > MaxCaptionName Then k_name = k_name.Substring(0, MaxCaptionName - 1) & "…"
            navDoc.SetItemText(ModeAttachments, $"{AttachmentsCaption}: {k_name}")
            RaiseEvent AttachmentChosen(k_index)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimitePdfPage.Picker_ItemClicked", ex)
        End Try
    End Sub

    ' UI boundary: logs and swallows.
    Private Sub Picker_Closed(k_sender As Object, k_args As FormClosedEventArgs)
        Try
            If _picked OrElse IsDisposed Then Return
            navDoc.SelectedKey = _previousKey
        Catch ex As Exception
            GlobalErrorLog.Write("PrimitePdfPage.Picker_Closed", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            picAtas.BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimitePdfPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
