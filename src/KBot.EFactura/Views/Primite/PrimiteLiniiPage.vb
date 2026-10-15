Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net
Imports System.Text
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-18 / 00EF-29 -- the first view of a received invoice: ABOVE (40% of the height) the supplier as it wrote itself in the XML
''' (name, tax code, address, due date, the accounts it asks to be paid into) and the notes of the XML; UNDER them (60%) the lines of
''' the invoice. The VAT per rate is no longer shown (operator, 15.10.2026). The page only OWNS its controls (declared in its designer
''' file); what they show is the work of <see cref="PrimiteView"/>, which holds the invoice and the server.
''' </summary>
Public Class PrimiteLiniiPage
    Implements IThemedContainer

    Public Sub New()
        InitializeComponent()
        rtbNote.Editabil = False
    End Sub

    ''' <summary>Puts the supplier block and the notes of <paramref name="k_detail"/> on screen.</summary>
    Public Sub ShowSupplier(k_detail As EFacturaPrimitaDetaliu)
        ArgumentNullException.ThrowIfNull(k_detail)
        Dim k_f As EFacturaPrimita = k_detail.Factura
        Dim k_html As New StringBuilder()
        k_html.Append("<b>").Append(WebUtility.HtmlEncode(If(String.IsNullOrWhiteSpace(k_f.Furnizor), "Furnizor necunoscut", k_f.Furnizor.Trim()))).Append("</b>")
        If Not String.IsNullOrWhiteSpace(k_f.Cui) Then
            k_html.Append("<br>Cod fiscal: ").Append(WebUtility.HtmlEncode(k_f.Cui.Trim()))
        End If
        If Not String.IsNullOrWhiteSpace(k_f.Adresa) Then
            k_html.Append("<br>Adresa: ").Append(WebUtility.HtmlEncode(k_f.Adresa.Trim()))
        End If
        If k_f.DataScad.HasValue Then
            k_html.Append("<br>Scadentă: ").Append(k_f.DataScad.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture))
        End If
        If Not String.IsNullOrWhiteSpace(k_f.Ref) Then
            k_html.Append("<br>Referință: ").Append(WebUtility.HtmlEncode(k_f.Ref.Trim()))
        End If
        Dim k_accounts As List(Of String) = k_detail.Plati.Where(Function(k_x) Not String.IsNullOrWhiteSpace(k_x.Cont)).
            Select(Function(k_x) k_x.Cont.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        If k_accounts.Count > 0 Then
            k_html.Append("<br>Cont: ").Append(WebUtility.HtmlEncode(String.Join(", ", k_accounts)))
        End If
        lblFurnizor.Html = k_html.ToString()
        ' No empty line anywhere: inside a note or between two notes (operator, 15.10.2026).
        rtbNote.Rtf = String.Join(vbCrLf, PrimiteObservatiiPage.CleanNotes(k_detail.Note))
    End Sub

    ''' <summary>Empties the supplier block and the notes (no invoice is shown).</summary>
    Public Sub ClearSupplier()
        lblFurnizor.Html = String.Empty
        rtbNote.Rtf = String.Empty
    End Sub

    ' Slice 00EF-27: a line of an invoice read with the CPV column and without a CPV code is painted in the theme's error
    ' colour (dark red); a line stored before the column existed (Cpv = Nothing) is painted as usual.
    Private Sub GridLinii_RowFormatting(sender As Object, e As KBotRowFormattingEventArgs) Handles gridLinii.RowFormatting
        Try
            Dim k_line As EFacturaPrimitaLinie = TryCast(e.Row?.Tag, EFacturaPrimitaLinie)
            If k_line IsNot Nothing AndAlso k_line.CpvLipsa Then e.ForeColor = ThemeManager.Current.Palette.ErrorColor
        Catch ex As Exception
            ' Paint boundary: the row is painted as it is.
            GlobalErrorLog.Write("PrimiteLiniiPage.GridLinii_RowFormatting", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteLiniiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
