Option Strict On

''' <summary>
''' Slice 0078-15 (operator, 06.10.2026): a view (or one of its pages) that shows a PDF and can let it go. The shell calls it
''' on the view it just put away (DDF -> ORD ...): the document is closed and its local signed copy is deleted; when the
''' view comes back the shell pushes the context again and the copy is fetched anew (sum check).
''' </summary>
Public Interface IReleasesDocument

    ''' <summary>Closes the document on screen and forgets it, so the next context push shows it again.</summary>
    Sub ReleaseDocument()

End Interface
