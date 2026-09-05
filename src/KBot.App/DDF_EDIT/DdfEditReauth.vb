Option Strict On
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Domain

''' <summary>
''' The shell's 401 net, specialised once per response shape the DDF editor needs.
'''
''' <para><b>Why a parameter object and not seven constructor parameters.</b>
''' <c>MainForm.WithReauth</c> is private AND generic, so a form that talks to the server has
''' to be handed one closure per response type -- <c>OrdEditForm</c> takes four that way, and
''' <c>AsociereForm</c> two. The DDF editor needs SEVEN: the save, the file upload, the file
''' download, the number lock, and three combo sources. Seven positional delegates of nearly
''' identical shape in one constructor call is a line nobody can read and an argument order
''' nobody can get right twice; naming them fixes both without changing the pattern -- the
''' re-login policy still lives in exactly one place, in <c>MainForm</c>.</para>
'''
''' <para>Every member is required. A missing one is a defect, not a degraded mode, so the
''' constructor refuses rather than leaving a call site to fail later with a null reference
''' in the middle of a save.</para>
''' </summary>
Public NotInheritable Class DdfEditReauth

    Public ReadOnly Property Salvare As Func(Of Func(Of Task(Of DdfSaveRezultat)), Task(Of DdfSaveRezultat))
    Public ReadOnly Property Incarcare As Func(Of Func(Of Task(Of PutDdfFisierResponse)), Task(Of PutDdfFisierResponse))
    Public ReadOnly Property Descarcare As Func(Of Func(Of Task(Of PdfDownloadResult)), Task(Of PdfDownloadResult))
    Public ReadOnly Property Numar As Func(Of Func(Of Task(Of DdfNumarLock)), Task(Of DdfNumarLock))
    Public ReadOnly Property Compartimente As Func(Of Func(Of Task(Of List(Of String))), Task(Of List(Of String)))
    Public ReadOnly Property Parteneri As Func(Of Func(Of Task(Of List(Of DdfPartener))), Task(Of List(Of DdfPartener)))
    Public ReadOnly Property Clasificatii As Func(Of Func(Of Task(Of List(Of DdfClasificatie))), Task(Of List(Of DdfClasificatie)))

    Public Sub New(salvare As Func(Of Func(Of Task(Of DdfSaveRezultat)), Task(Of DdfSaveRezultat)),
                   incarcare As Func(Of Func(Of Task(Of PutDdfFisierResponse)), Task(Of PutDdfFisierResponse)),
                   descarcare As Func(Of Func(Of Task(Of PdfDownloadResult)), Task(Of PdfDownloadResult)),
                   numar As Func(Of Func(Of Task(Of DdfNumarLock)), Task(Of DdfNumarLock)),
                   compartimente As Func(Of Func(Of Task(Of List(Of String))), Task(Of List(Of String))),
                   parteneri As Func(Of Func(Of Task(Of List(Of DdfPartener))), Task(Of List(Of DdfPartener))),
                   clasificatii As Func(Of Func(Of Task(Of List(Of DdfClasificatie))), Task(Of List(Of DdfClasificatie))))
        ArgumentNullException.ThrowIfNull(salvare)
        ArgumentNullException.ThrowIfNull(incarcare)
        ArgumentNullException.ThrowIfNull(descarcare)
        ArgumentNullException.ThrowIfNull(numar)
        ArgumentNullException.ThrowIfNull(compartimente)
        ArgumentNullException.ThrowIfNull(parteneri)
        ArgumentNullException.ThrowIfNull(clasificatii)

        _Salvare = salvare
        _Incarcare = incarcare
        _Descarcare = descarcare
        _Numar = numar
        _Compartimente = compartimente
        _Parteneri = parteneri
        _Clasificatii = clasificatii
    End Sub
End Class
