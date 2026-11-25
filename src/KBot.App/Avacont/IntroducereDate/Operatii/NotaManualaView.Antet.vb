Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' SLICE-AV08-07 — documentul nou și operația adăugată unui document existent folosesc aceleași câmpuri.
Partial Public Class NotaManualaView
    Private _alegereDocument As Integer
    Private _idNotaDocumente As Integer?
    Private _documente As New List(Of NotaManualaDocument)()
    Private _incarcaDocumente As Boolean
    Private _populeazaDocumente As Boolean
    Private _adaugaLaDocument As Boolean
    Private _confirmareInCurs As Boolean
    Private _confirmareDocument As (IdNota As Integer, Numar As String)?

    Private Sub IncarcaAntet()
        cmbNota.Text = String.Empty
        _adaugaLaDocument = False
        _confirmareDocument = Nothing
        Interlocked.Increment(_alegereDocument)
        _incarcaDocumente = False
        _idNotaDocumente = Nothing
        _documente.Clear()
        cmbDocument.Items.Clear()
        cmbDocument.Text = String.Empty
        cmbTipOperatie.Items.Clear()
        cmbTipOperatie.Items.AddRange(_context.TipuriOperatie.OrderBy(Function(t) t.Denumire, Alfabetic).Cast(Of Object)())
        AlegeTipOperatie(Nothing)
        IncarcaIstoric(cmbFelDocument, _context.FeluriDocument)
        IncarcaIstoric(cmbCompartiment, _context.Compartimente)
        IncarcaIstoric(cmbExplicatie, _context.Explicatii)
    End Sub

    Private Shared Sub IncarcaIstoric(camp As KBotComboBox, valori As IEnumerable(Of String))
        camp.Items.Clear()
        camp.Items.AddRange(valori.Distinct(Alfabetic).OrderBy(Function(valoare) valoare, Alfabetic).Cast(Of Object)())
        camp.Text = String.Empty
    End Sub

    Private Sub AlegeTipOperatie(cod As String)
        Dim cautat As String = If(String.Equals(cod, "V", StringComparison.OrdinalIgnoreCase), String.Empty, If(cod, String.Empty))
        cmbTipOperatie.SelectedItem = _context.TipuriOperatie.FirstOrDefault(Function(t) Alfabetic.Equals(t.Cod, cautat))
        If cmbTipOperatie.SelectedItem Is Nothing Then cmbTipOperatie.Text = cautat
    End Sub

    Private Function NotaAleasa() As NotaManualaNota
        Dim numar As Integer
        If Not Integer.TryParse(cmbNota.Text.Trim(), NumberStyles.None, Ro, numar) Then Return Nothing
        Return _context.Note.FirstOrDefault(Function(n) n.NumarNC = numar)
    End Function

    Private Async Sub Nota_Changed(sender As Object, e As EventArgs) Handles cmbNota.SelectedIndexChanged, cmbNota.Leave
        If _loading OrElse _saving OrElse _populeazaDocumente Then Return
        Await IncarcaDocumenteleNoteiAsync()
    End Sub

    Private Async Function IncarcaDocumenteleNoteiAsync() As Task
        Dim idNota As Integer? = NotaAleasa()?.Id
        If Nullable.Equals(idNota, _idNotaDocumente) Then Return
        Dim cerere As Integer = Interlocked.Increment(_alegereDocument)
        _idNotaDocumente = idNota
        _confirmareDocument = Nothing
        _documente.Clear()
        PopuleazaDocumente()
        _incarcaDocumente = idNota.HasValue
        ActualizeazaCampuri()
        If Not idNota.HasValue Then Return
        Try
            Dim documente = Await _gate.RunAsync(
                Function() _api.GetNotaManualaDocumenteAsync(_sid, _an, idNota.Value, CancellationToken.None))
            If IsDisposed OrElse cerere <> _alegereDocument Then Return
            _documente = documente
            PopuleazaDocumente()
        Catch ex As Exception
            If Not IsDisposed AndAlso cerere = _alegereDocument Then
                _idNotaDocumente = Nothing
                ArataEroare(ex)
            End If
        Finally
            If Not IsDisposed AndAlso cerere = _alegereDocument Then
                _incarcaDocumente = False
                ActualizeazaCampuri()
            End If
        End Try
    End Function

    Private Sub PopuleazaDocumente()
        Dim numar As String = If(_adaugaLaDocument, String.Empty, cmbDocument.Text)
        _populeazaDocumente = True
        cmbDocument.BeginUpdate()
        Try
            cmbDocument.Items.Clear()
            If _adaugaLaDocument Then
                Dim documente = _documente.Where(Function(doc) doc.PoatePrimiOperatie).
                    OrderBy(Function(doc) CheieNumericaDocument(doc.NumarDocument)).
                    ThenBy(Function(doc) doc.NumarDocument, Alfabetic).ThenByDescending(Function(doc) doc.DataDocument).
                    ThenBy(Function(doc) doc.FelDocument, Alfabetic).
                    Select(Function(doc) New KBotComboGroupedItem(doc.NumarDocument, doc.NumarDocument,
                        $"{doc.DataDocument.ToString("dd.MM.yyyy", Ro)} · {doc.FelDocument}", doc))
                cmbDocument.Items.AddRange(documente.Cast(Of Object)())
            Else
                Dim numere = _documente.Select(Function(doc) doc.NumarDocument.Trim()).Distinct(Alfabetic).
                    OrderBy(Function(value) CheieNumericaDocument(value)).ThenBy(Function(value) value, Alfabetic)
                cmbDocument.Items.AddRange(numere.Cast(Of Object)())
            End If
            cmbDocument.Text = numar
        Finally
            cmbDocument.EndUpdate()
            _populeazaDocumente = False
        End Try
    End Sub

    Private Shared Function CheieNumericaDocument(numar As String) As Decimal?
        Dim valoare As Decimal
        Const Format As NumberStyles = NumberStyles.AllowLeadingSign Or NumberStyles.AllowDecimalPoint
        If Decimal.TryParse(numar, Format, Ro, valoare) OrElse Decimal.TryParse(numar, Format, CultureInfo.InvariantCulture, valoare) Then Return valoare
        Return Nothing
    End Function

    Private Function DocumentAles() As NotaManualaDocument
        If Not _adaugaLaDocument Then Return Nothing
        Return TryCast(TryCast(cmbDocument.SelectedItem, KBotComboGroupedItem)?.Tag, NotaManualaDocument)
    End Function

    Private Sub Document_Changed(sender As Object, e As EventArgs) Handles cmbDocument.SelectedIndexChanged
        If _loading OrElse _saving OrElse _populeazaDocumente Then Return
        If _adaugaLaDocument Then
            Dim doc = DocumentAles()
            If doc IsNot Nothing Then CompleteazaDocument(doc, actualizeazaNumar:=False)
        ElseIf cmbDocument.SelectedIndex >= 0 Then
            ConfirmaNumarulDocumentului()
        End If
    End Sub

    Private Sub Document_Leave(sender As Object, e As EventArgs) Handles cmbDocument.Leave
        If Not _loading AndAlso Not _saving AndAlso Not _adaugaLaDocument AndAlso cmbDocument.Enabled Then ConfirmaNumarulDocumentului()
    End Sub

    Private Function NumarDocumentConfirmat(nota As NotaManualaNota, numar As String) As Boolean
        Return nota IsNot Nothing AndAlso _confirmareDocument.HasValue AndAlso _confirmareDocument.Value.IdNota = nota.Id AndAlso
            Alfabetic.Equals(_confirmareDocument.Value.Numar, numar)
    End Function

    Private Function ConfirmaNumarulDocumentului() As Boolean
        If _confirmareInCurs Then Return False
        Dim nota = NotaAleasa()
        Dim numar = cmbDocument.Text.Trim()
        Dim docInitial = _context.Detalii?.Document
        If nota Is Nothing OrElse numar.Length = 0 OrElse NumarDocumentConfirmat(nota, numar) OrElse
            (docInitial IsNot Nothing AndAlso nota.Id = docInitial.IdNota AndAlso Alfabetic.Equals(numar, docInitial.NumarDocument.Trim())) Then Return True
        If nota.Id <> _idNotaDocumente.GetValueOrDefault() OrElse
            Not _documente.Any(Function(doc) doc.IdDocument <> If(docInitial?.IdDocument, 0) AndAlso Alfabetic.Equals(doc.NumarDocument.Trim(), numar)) Then Return True
        _confirmareInCurs = True
        Try
            If KBotMessage.Show(FindForm(), $"Există deja un document cu numărul {numar} în nota contabilă {nota.NumarNC}. Este corect?",
                                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                cmbDocument.Text = String.Empty
                Return False
            End If
            _confirmareDocument = (nota.Id, numar)
            Return True
        Finally
            _confirmareInCurs = False
        End Try
    End Function

    Private Sub CompleteazaDocument(doc As NotaManualaDocument, actualizeazaNumar As Boolean)
        If actualizeazaNumar Then
            _populeazaDocumente = True
            Try
                cmbNota.Text = doc.NumarNC.ToString(Ro)
                cmbDocument.Text = doc.NumarDocument
            Finally
                _populeazaDocumente = False
            End Try
        End If
        cmbFelDocument.Text = If(doc.FelDocument, String.Empty).ToUpper(Ro)
        AlegeTipOperatie(doc.Fel)
        dtpDocument.Value = doc.DataDocument.ToDateTime(TimeOnly.MinValue)
        If _adaugaLaDocument Then dtpOperatie.Value = dtpDocument.Value
    End Sub

    Private Async Sub ModificareDocument_Changed(sender As Object, e As EventArgs) Handles chkDocument.CheckedChanged
        If _loading Then Return
        If Not chkDocument.Checked Then CompleteazaDocument(_context.Detalii.Document, actualizeazaNumar:=True)
        ActualizeazaCampuri()
        If chkDocument.Checked Then Await IncarcaDocumenteleNoteiAsync()
    End Sub

    Private Async Function SchimbaModDocumentAsync(adaugaLaDocument As Boolean) As Task
        _adaugaLaDocument = adaugaLaDocument
        _confirmareDocument = Nothing
        cmbDocument.Text = String.Empty
        cmbFelDocument.Text = String.Empty
        AlegeTipOperatie(Nothing)
        PopuleazaDocumente()
        ActualizeazaCampuri()
        Await IncarcaDocumenteleNoteiAsync()
        If IsDisposed OrElse Not Visible Then Return
        If cmbDocument.Enabled Then
            cmbDocument.Focus()
        Else
            cmbNota.Focus()
        End If
    End Function

    Private Sub TextCuMajuscule_Leave(sender As Object, e As EventArgs) Handles cmbFelDocument.Leave, cmbCompartiment.Leave, cmbExplicatie.Leave
        Dim combo = DirectCast(sender, KBotComboBox)
        combo.Text = combo.Text.Trim().ToUpper(Ro)
    End Sub

    Private Shared Function TextIstoric(combo As KBotComboBox, original As String) As String
        Dim text As String = combo.Text.Trim().ToUpper(Ro)
        If original IsNot Nothing AndAlso String.Equals(text, original.Trim().ToUpper(Ro), StringComparison.Ordinal) Then Return original
        Return text
    End Function
End Class
