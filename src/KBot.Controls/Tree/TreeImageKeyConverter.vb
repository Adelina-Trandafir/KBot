Option Strict On
Imports System.ComponentModel

''' <summary>
''' Drop-down of the keys held by the tree's <see cref="AdvancedTreeControl.NodeImages"/>, offered
''' in the property grid for every <c>*IconKey</c> / <c>*ImageKey</c> of a NON-node icon (header,
''' footer, collapse button, search clear button).
'''
''' Alongside the image picker, which drops the picture itself into the host form's .resx, the key
''' picker points at a picture that already lives in the shared <see cref="ImageList"/> — one place
''' for the pictures, and a switch of that list re-skins every band at once.
'''
''' NOT exclusive on purpose: a tree filled at runtime resolves its keys against the icon cache
''' (see <c>ResolveHeaderIcons</c>), so the operator must stay free to type a key that no designer
''' <see cref="ImageList"/> holds yet.
''' </summary>
Public NotInheritable Class TreeImageKeyConverter
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
            GlobalErrorLog.Write("TreeImageKeyConverter.GetStandardValues", ex)
            Return New StandardValuesCollection(New String() {String.Empty})
        End Try
    End Function

    ''' <summary>
    ''' The list bound to the tree being edited. A multi-selection hands the grid an array of
    ''' components, so the first tree in it wins — the keys are the same list anyway.
    ''' </summary>
    Friend Shared Function ImageListOf(context As ITypeDescriptorContext) As ImageList
        Try
            If context Is Nothing Then Return Nothing
            Dim tree As AdvancedTreeControl = TryCast(context.Instance, AdvancedTreeControl)
            If tree Is Nothing Then
                Dim many As Object() = TryCast(context.Instance, Object())
                If many IsNot Nothing Then
                    For Each item As Object In many
                        tree = TryCast(item, AdvancedTreeControl)
                        If tree IsNot Nothing Then Exit For
                    Next
                End If
            End If
            If tree Is Nothing Then Return Nothing
            Return tree.NodeImages
        Catch ex As Exception
            GlobalErrorLog.Write("TreeImageKeyConverter.ImageListOf", ex)
            Return Nothing
        End Try
    End Function

End Class
