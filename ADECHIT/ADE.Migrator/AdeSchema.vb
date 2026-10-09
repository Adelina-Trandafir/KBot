''' <summary>The kind of value a column of an AD_ table holds; drives the Access -> MariaDB conversion.</summary>
Public Enum AdeKind
    Int
    Dbl
    Flag
    Moment
    Str
End Enum

''' <summary>One column of an AD_ table: name, kind and (for text) the longest value the column takes.</summary>
Public NotInheritable Class AdeColumn

    Public Sub New(k_name As String, k_kind As AdeKind, Optional k_maxLen As Integer = 0)
        Name = k_name
        Kind = k_kind
        MaxLen = k_maxLen
    End Sub

    Public ReadOnly Property Name As String
    Public ReadOnly Property Kind As AdeKind
    Public ReadOnly Property MaxLen As Integer

End Class

''' <summary>An AD_ table as the migrator writes it: its Access name, its key and its columns.</summary>
Public NotInheritable Class AdeTable

    Public Sub New(k_name As String, k_key As String, ParamArray k_columns As AdeColumn())
        Name = k_name
        Key = k_key
        Columns = k_columns
    End Sub

    ''' <summary>Name without prefix (the Access table); the MariaDB table is <c>AD_</c> + this.</summary>
    Public ReadOnly Property Name As String
    Public ReadOnly Property Key As String
    Public ReadOnly Property Columns As AdeColumn()

    Public ReadOnly Property Target As String
        Get
            Return "AD_" & Name
        End Get
    End Property

End Class

''' <summary>
''' The shape of the tables written from Access, in insertion order. It mirrors sql/AD_03_schema_finala.sql; the form
''' compares it with the real columns of the unit database before writing.
''' </summary>
Public NotInheritable Class AdeSchema

    Private Sub New()
    End Sub

    Private Shared Function I(k_name As String) As AdeColumn
        Return New AdeColumn(k_name, AdeKind.Int)
    End Function

    Private Shared Function D(k_name As String) As AdeColumn
        Return New AdeColumn(k_name, AdeKind.Dbl)
    End Function

    Private Shared Function B(k_name As String) As AdeColumn
        Return New AdeColumn(k_name, AdeKind.Flag)
    End Function

    Private Shared Function T(k_name As String) As AdeColumn
        Return New AdeColumn(k_name, AdeKind.Moment)
    End Function

    Private Shared Function S(k_name As String, Optional k_maxLen As Integer = 255) As AdeColumn
        Return New AdeColumn(k_name, AdeKind.Str, k_maxLen)
    End Function

    ''' <summary>Insertion order: parents before children.</summary>
    Public Shared ReadOnly Tables As AdeTable() = {
        New AdeTable("Grupe", "IDG", I("IDG"), S("Grupa", 50), S("Tip", 10), I("InchisaDinAn")),
        New AdeTable("ValoriTaxe", "IDV", I("IDV"), D("TaxaZilnica"), B("Activ"), S("Expl")),
        New AdeTable("Platitori", "IDP", I("IDP"), I("IDG"), S("Nume"), S("CNP"), B("Plecat"), I("SI"), T("DataIntrare"), T("DataIesire")),
        New AdeTable("Platitori_sub", "IDS", I("IDS"), I("IDP"), S("Nume", 50), S("Adresa"), S("CUI"), S("Cont"), S("Banca"),
                     S("EMail"), S("Telefon", 50), B("TrimiteMail"), B("Activ"), S("CNP_Platitor")),
        New AdeTable("LunaD", "IDL", I("IDL"), I("IDV"), I("Luna"), I("Anul"), S("LunaT"), S("LA"), B("Inchisa"), I("ZileLuna")),
        New AdeTable("Prezenta", "IDZ", I("IDZ"), I("IDP"), I("IDL"), I("IDV"), I("IDG"), I("ZilePrezenta"), D("ValoareContract"), I("ValoareTotala")),
        New AdeTable("Plati", "IDPL", I("IDPL"), I("IDP"), I("IDZ"), I("IDS"), I("IDL"), T("Data"), I("Plata"), I("TIP"), B("Anulata"),
                     B("Valid"), S("Motivul"), I("OriginMonth"), I("OriginYear")),
        New AdeTable("Chitante", "IDC", I("IDC"), I("IDPL"), T("Data"), S("Serie", 50), I("Numar"), S("Explicatie"), B("Anulata"), I("IDL")),
        New AdeTable("AlteDoc", "IDA", I("IDA"), I("IDPL"), S("NrDoc"), S("FelDoc"), T("DataDoc"), B("Anulata"), I("IDL"), S("Explicatie")),
        New AdeTable("Retur", "IDR", I("IDR"), I("IDP"), I("IDZ"), I("IDL"), I("IDS"), T("Data"), S("Explicatie"), B("Anulat"), S("NrDoc"),
                     D("Suma"), I("OriginMonth"), I("OriginYear")),
        New AdeTable("SS_Buget", "ID", I("ID"), I("IDG"), I("IDP"), I("IDL"), I("IDZ"), S("Luna"), I("Anul"), S("Nume"), S("CNP"),
                     I("ZilePrezenta"), I("SID"), I("SIC"), D("ValoareContract"), I("ValoareTotala"), D("Plata"), D("Restanta"),
                     D("Compensare"), D("Anticipat"), I("Plati"), I("Retur"), I("SFD"), I("SFC"), S("Detalii", 0), B("Plecat"),
                     S("Educator"), S("Grupa"))
    }

    ''' <summary>The two tables the migrator builds itself (no Access counterpart).</summary>
    Public Shared ReadOnly GrupeEducator As New AdeTable("Grupe_Educator", "IDGE",
        I("IDGE"), I("IDG"), S("Educator", 50), T("DeLa"), T("PanaLa"))

    Public Shared ReadOnly PlatitoriIstoric As New AdeTable("Platitori_Istoric", "IDI",
        I("IDI"), I("IDP"), T("Data"), S("Tip", 20), I("IDG_Vechi"), I("IDG_Nou"), B("Dedus"), S("Utilizator", 80), S("Nota"))

    ''' <summary>Every foreign key of the written tables: child table, column, parent table.</summary>
    Public Shared ReadOnly Relations As (Child As String, Col As String, Parent As String)() = {
        ("Platitori", "IDG", "Grupe"),
        ("Platitori_sub", "IDP", "Platitori"),
        ("LunaD", "IDV", "ValoriTaxe"),
        ("Prezenta", "IDP", "Platitori"), ("Prezenta", "IDL", "LunaD"), ("Prezenta", "IDV", "ValoriTaxe"), ("Prezenta", "IDG", "Grupe"),
        ("Plati", "IDP", "Platitori"), ("Plati", "IDZ", "Prezenta"), ("Plati", "IDS", "Platitori_sub"), ("Plati", "IDL", "LunaD"),
        ("Chitante", "IDPL", "Plati"), ("Chitante", "IDL", "LunaD"),
        ("AlteDoc", "IDPL", "Plati"), ("AlteDoc", "IDL", "LunaD"),
        ("Retur", "IDP", "Platitori"), ("Retur", "IDZ", "Prezenta"), ("Retur", "IDS", "Platitori_sub"), ("Retur", "IDL", "LunaD"),
        ("SS_Buget", "IDG", "Grupe"), ("SS_Buget", "IDP", "Platitori"), ("SS_Buget", "IDL", "LunaD"), ("SS_Buget", "IDZ", "Prezenta")
    }

    ''' <summary>
    ''' Links that old data may leave pointing nowhere (a month or an attendance row that was deleted): the value is
    ''' stored as NULL and counted, not blocked. Every other dangling link blocks the migration.
    ''' </summary>
    Public Shared ReadOnly DanglingAllowed As String() = {
        "Plati.IDZ", "Plati.IDL", "Retur.IDZ", "Retur.IDL", "SS_Buget.IDL", "SS_Buget.IDZ", "SS_Buget.IDG",
        "Chitante.IDL", "AlteDoc.IDL"
    }

End Class
