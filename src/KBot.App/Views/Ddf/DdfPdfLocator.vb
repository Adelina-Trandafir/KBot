Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports KBot.Domain

''' <summary>
''' Logica pură de căutare/nume a PDF-urilor DDF (felia 0020-04). Fără WinForms: se poate testa.
''' Convenția de cale (mdl_FX_DDF_PDF, planul §2.5):
'''   &lt;root&gt;\&lt;partener | GENERAL&gt;\DDF_NR_{CUAL}_REV_{NumarRev}_{CodAngajament}.PDF
''' Folderul partenerului se folosește doar când FX_DDF.PartAng e True; numele lui e
''' NumePartener normalizat (fiecare grup \W+ -&gt; «_»). Un sibling `.xml` stă lângă PDF, dar
''' browserul NU îl listează.
''' </summary>
Public NotInheritable Class DdfPdfLocator

    Private Sub New()
    End Sub

    ' DDF_NR_{CUAL}_REV_{NumarRev}_{Cod}.PDF — CUAL și NumarRev sunt numere; Cod e restul.
    ' NumarRev poate fi scris cu spații (Format "@@@"), deci acceptăm spații în grup.
    Private Shared ReadOnly _nameRx As New Regex(
        "^DDF_NR_(?<cual>\d+)_REV_(?<rev>[ \d]+)_(?<cod>.+)\.PDF$",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled)

    ''' <summary>
    ''' Calea AȘTEPTATĂ a PDF-ului unei revizii (planul §2.5), sub rădăcina dată. Folderul e
    ''' numele partenerului când <paramref name="antet"/> e legat de partener, altfel «GENERAL».
    ''' </summary>
    Public Shared Function ExpectedPath(root As String, antet As DdfAntet, numarRev As Integer) As String
        If antet Is Nothing Then Return Nothing
        Dim folder As String = antet.FolderPdf                 ' partener normalizat sau «GENERAL»
        Dim fileName As String = $"DDF_NR_{antet.Cual}_REV_{numarRev}_{antet.CodAngajament}.PDF"
        Return Path.Combine(NormalizeRoot(root), folder, fileName)
    End Function

    ''' <summary>
    ''' Enumerează recursiv toate PDF-urile de sub <paramref name="root"/> care aparțin
    ''' angajamentului dat (numele se termină cu «_{CodAngajament}.PDF», case-insensitive).
    ''' Aceasta prinde și «GENERAL\» și fiecare folder de partener, fără a hardcoda vreunul.
    ''' Siblingii `.xml` sunt excluși prin construcție (căutăm doar *.pdf). O rădăcină
    ''' inexistentă întoarce o listă goală (NU aruncă, NU creează folderul).
    ''' </summary>
    Public Shared Function Enumerate(root As String, codAngajament As String) As List(Of DdfPdfFile)
        Dim rezultat As New List(Of DdfPdfFile)()
        If String.IsNullOrWhiteSpace(root) OrElse String.IsNullOrWhiteSpace(codAngajament) Then Return rezultat
        If Not Directory.Exists(root) Then Return rezultat

        Dim suffix As String = "_" & codAngajament & ".PDF"
        For Each full As String In Directory.EnumerateFiles(root, "*.pdf", SearchOption.AllDirectories)
            Dim name As String = Path.GetFileName(full)
            If Not name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then Continue For

            Dim fi As New FileInfo(full)
            Dim cual As Integer = 0
            Dim rev As Integer = 0
            ParseName(name, cual, rev)

            rezultat.Add(New DdfPdfFile() With {
                .FullPath = full,
                .FileName = name,
                .Folder = FolderNameOf(full, root),
                .Cual = cual,
                .NumarRev = rev,
                .Size = fi.Length,
                .Modified = fi.LastWriteTime
            })
        Next

        ' Cele mai recente întâi (planul §7).
        Return rezultat.OrderByDescending(Function(f) f.Modified).ToList()
    End Function

    ''' <summary>Extrage CUAL și NumarRev din numele fișierului. False dacă nu se potrivește.</summary>
    Public Shared Function ParseName(fileName As String, ByRef cual As Integer, ByRef numarRev As Integer) As Boolean
        cual = 0
        numarRev = 0
        If String.IsNullOrEmpty(fileName) Then Return False
        Dim m As Match = _nameRx.Match(fileName)
        If Not m.Success Then Return False
        Integer.TryParse(m.Groups("cual").Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, cual)
        Integer.TryParse(m.Groups("rev").Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, numarRev)
        Return True
    End Function

    ' Numele folderului direct sub rădăcină (partener sau «GENERAL»). Dacă fișierul stă chiar
    ' în rădăcină, întoarce «GENERAL».
    Private Shared Function FolderNameOf(fullPath As String, root As String) As String
        Dim dir As String = Path.GetDirectoryName(fullPath)
        Dim rootFull As String = Path.GetFullPath(NormalizeRoot(root)).TrimEnd(Path.DirectorySeparatorChar)
        Dim dirFull As String = Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar)
        If String.Equals(dirFull, rootFull, StringComparison.OrdinalIgnoreCase) Then Return "GENERAL"
        Return Path.GetFileName(dirFull)
    End Function

    Private Shared Function NormalizeRoot(root As String) As String
        Return If(String.IsNullOrEmpty(root), String.Empty, root)
    End Function

End Class

''' <summary>Un PDF DDF găsit pe disc. POCO -> fără Try/Catch.</summary>
Public NotInheritable Class DdfPdfFile
    Public Property FullPath As String = String.Empty
    Public Property FileName As String = String.Empty
    ''' <summary>Numele folderului (partener sau «GENERAL»).</summary>
    Public Property Folder As String = String.Empty
    Public Property Cual As Integer
    Public Property NumarRev As Integer
    Public Property Size As Long
    Public Property Modified As Date
End Class
