''' <summary>Definitia unei coloane in modul TreeListView. Populata din &lt;Columns&gt; XML.</summary>
Friend Structure ColumnDef
    Dim Name As String
    Dim Header As String
    Dim Width As Integer
    Dim ColType As En_ColType      ' era String, acum enum
    Dim Align As En_ColAlign     ' era HorizontalAlignment, acum enum
    Dim Format As String
    ' ── header styling ───────────────────────────────────────────────────────
    Dim HeaderBackColor As Color
    Dim HeaderForeColor As Color
    Dim HeaderBold As Boolean
    Dim HeaderItalic As Boolean
    Dim HeaderUnderline As Boolean
    Dim HeaderAlign As En_ColAlign   ' ColAlign_Inherit = mosteneste Align
End Structure

''' <summary>Tipul de date al unei coloane TreeListView.</summary>
Friend Enum En_ColType
    ColType_Text = 0
    ColType_Number = 1
    ColType_Date = 2
    ColType_Boolean = 3
End Enum

''' <summary>
''' Alinierea textului intr-o coloana sau header TreeListView.
''' ColAlign_Inherit (-1) este valid doar pe ColumnDef.HeaderAlign:
''' inseamna "mosteneste Align (celule)".
''' </summary>
Friend Enum En_ColAlign
    ColAlign_Inherit = -1   ' sentinel: mosteneste Align celule (doar HeaderAlign)
    ColAlign_Left = 0
    ColAlign_Center = 1
    ColAlign_Right = 2
End Enum
