Option Strict On
Imports KBot.Domain

''' <summary>
''' The contract of one page of the DDF EDITOR (slice 0051) -- the sister of
''' <see cref="IDdfPage"/>, but for writing.
'''
''' <para>Access spread this document over a form and four subforms
''' (<c>frmFX_DDF</c> + <c>frmFX_DDF_REV</c> + <c>_SECT_A</c> + <c>_SECT_B</c> + <c>_ATT</c>).
''' Here the two HEADERS collapse into the form's own header band -- every field of
''' <c>frmFX_DDF</c> and every revision field of <c>frmFX_DDF_REV</c> sit in <c>tlyAntet</c>,
''' because they are one document's identity and splitting them across a tab would hide half
''' of it -- and what remains becomes four PAGES behind a horizontal <c>KBotNavList</c>:
''' «Sectiunea A», «Sectiunea B», «Descriere», «Fisiere».</para>
'''
''' <para>As in <see cref="IDdfPage"/>, a page has NO api client and makes NO network
''' request -- but unlike there, a page MODIFIES what it is given. The draft is held by the
''' form and handed to the pages BY REFERENCE: all four write into the same graph, so a
''' change made on one page is visible on the others with no synchronisation. That is what
''' makes the section-B page correct without asking anyone: section A rewrites the shared
''' object, and section B renders it.</para>
'''
''' <para>Deliberate side effect: with no dependency injection, every page has a
''' PARAMETERLESS constructor and therefore instantiates in the Visual Studio designer.</para>
''' </summary>
Public Interface IDdfEditPage

    ''' <summary>
    ''' The page's key: «sectiunea-a», «sectiunea-b», «descriere» or «fisiere». It must be
    ''' IDENTICAL to the key of the matching <c>navSub</c> entry, which the designer writes as
    ''' a literal.
    ''' </summary>
    ReadOnly Property PageKey As String

    ''' <summary>
    ''' Hands the page the graph it edits. <c>Nothing</c> = no document open, so the page
    ''' shows its empty state. Called on EVERY activation, so a page created late sees
    ''' everything that changed before it existed.
    ''' </summary>
    Sub SetDraft(draft As DdfDraft)

    ''' <summary>
    ''' Something in the graph changed on this page. The form refreshes the header band (the
    ''' total) and marks the document unsaved.
    ''' </summary>
    Event DraftModificat As EventHandler

End Interface
