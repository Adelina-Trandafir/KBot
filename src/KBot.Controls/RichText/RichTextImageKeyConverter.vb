Option Strict On
Imports System.ComponentModel

''' <summary>
''' Drop-down of the keys held by <see cref="KBotRichTextEditor.Images"/>, offered in the
''' property grid for every <c>*ImageKey</c> of the toolbar.
'''
''' <para>The twin of <c>TreeImageKeyConverter</c>, and deliberately the same contract, so the
''' operator does not learn two of them: alongside the image picker -- which drops the picture
''' itself into the host form's <c>.resx</c> -- the key picker points at a picture that already
''' lives in the shared <see cref="ImageList"/>. One place for the pictures, and swapping that
''' list re-skins the whole toolbar at once.</para>
'''
''' <para>NOT exclusive, for the same reason as the tree's: an editor whose list is assigned at
''' runtime must leave the operator free to type a key no designer <see cref="ImageList"/> holds
''' yet.</para>
''' </summary>
Public NotInheritable Class RichTextImageKeyConverter
    Inherits StringConverter

    Public Overrides Function GetStandardValuesSupported(context As ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValuesExclusive(context As ITypeDescriptorContext) As Boolean
        Return False
    End Function

    ''' <summary>The empty key ("from nowhere") first, then the keys of the bound image list.</summary>
    Public Overrides Function GetStandardValues(context As ITypeDescriptorContext) As StandardValuesCollection
        Try
            Dim keys As New List(Of String) From {String.Empty}
            Dim images As ImageList = ImageListOf(context)
            If images IsNot Nothing Then
                For Each key As String In images.Images.Keys
                    If String.IsNullOrEmpty(key) Then Continue For
                    If Not keys.Contains(key) Then keys.Add(key)
                Next
            End If
            Return New StandardValuesCollection(keys)
        Catch ex As Exception
            GlobalErrorLog.Write("RichTextImageKeyConverter.GetStandardValues", ex)
            Return New StandardValuesCollection(New String() {String.Empty})
        End Try
    End Function

    ''' <summary>
    ''' The list bound to the editor being edited. A multi-selection hands the grid an array of
    ''' components, so the first editor in it wins -- the keys are the same list anyway.
    ''' </summary>
    Private Shared Function ImageListOf(context As ITypeDescriptorContext) As ImageList
        Try
            If context Is Nothing Then Return Nothing
            Dim editor As KBotRichTextEditor = TryCast(context.Instance, KBotRichTextEditor)
            If editor Is Nothing Then
                Dim many As Object() = TryCast(context.Instance, Object())
                If many IsNot Nothing Then
                    For Each item As Object In many
                        editor = TryCast(item, KBotRichTextEditor)
                        If editor IsNot Nothing Then Exit For
                    Next
                End If
            End If
            If editor Is Nothing Then Return Nothing
            Return editor.Images
        Catch ex As Exception
            GlobalErrorLog.Write("RichTextImageKeyConverter.ImageListOf", ex)
            Return Nothing
        End Try
    End Function
End Class
