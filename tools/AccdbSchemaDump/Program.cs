// Throwaway discovery spike for PLAN_MigratorDirect.md section 12.
// NOT part of KBot.sln. Dumps the schema and a row sample of every .accdb
// passed on the command line, so the nomenclator mapping can be written from
// evidence instead of guesswork.
//
// Usage: AccdbSchemaDump <outputDir> <path.accdb> [password] ...
// Arguments after the output directory are read in pairs: path, then password
// ("-" for no password).

using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Text;

const int SampleRows = 20;
const int TextTruncate = 20;

if (args.Length < 3)
{
    Console.Error.WriteLine("Usage: AccdbSchemaDump <outputDir> <path.accdb> <password|-> ...");
    return 2;
}

var outputDir = args[0];
Directory.CreateDirectory(outputDir);

var exit = 0;
for (var i = 1; i + 1 < args.Length; i += 2)
{
    var path = args[i];
    var password = args[i + 1] == "-" ? "" : args[i + 1];

    try
    {
        DumpFile(path, password, outputDir);
    }
    catch (Exception ex)
    {
        exit = 1;
        Console.Error.WriteLine($"FAILED {path}: {ex.GetType().Name}: {ex.Message}");
    }
}

return exit;

static void DumpFile(string path, string password, string outputDir)
{
    var name = Path.GetFileNameWithoutExtension(path);
    var sb = new StringBuilder();
    sb.AppendLine($"# {Path.GetFileName(path)}");
    sb.AppendLine();
    sb.AppendLine($"- Path: `{path}`");
    sb.AppendLine($"- Size: {new FileInfo(path).Length:N0} bytes");
    sb.AppendLine($"- Password used: {(password.Length == 0 ? "no" : "yes")}");

    using var cn = Open(path, password, out var provider);
    sb.AppendLine($"- Provider: `{provider}`");
    sb.AppendLine();

    // --- tables -----------------------------------------------------------
    var allTables = cn.GetSchema("Tables");
    var tables = allTables
        .AsEnumerable()
        .Where(r => string.Equals(r.Field<string>("TABLE_TYPE"), "TABLE", StringComparison.OrdinalIgnoreCase))
        .Select(r => r.Field<string>("TABLE_NAME")!)
        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var others = allTables
        .AsEnumerable()
        .Where(r => !string.Equals(r.Field<string>("TABLE_TYPE"), "TABLE", StringComparison.OrdinalIgnoreCase))
        .Select(r => $"{r.Field<string>("TABLE_NAME")} ({r.Field<string>("TABLE_TYPE")})")
        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
        .ToList();

    sb.AppendLine("## Tables");
    sb.AppendLine();
    sb.AppendLine("| Table | Rows |");
    sb.AppendLine("|---|---:|");
    var counts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
    foreach (var t in tables)
    {
        long n;
        try
        {
            using var cmd = new OleDbCommand($"SELECT COUNT(*) FROM [{t}]", cn);
            n = Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  count failed on {t}: {ex.Message}");
            n = -1;
        }
        counts[t] = n;
        sb.AppendLine($"| {t} | {(n < 0 ? "?" : n.ToString("N0", CultureInfo.InvariantCulture))} |");
    }
    sb.AppendLine();

    if (others.Count > 0)
    {
        sb.AppendLine("## Non-table objects (queries / links / system)");
        sb.AppendLine();
        foreach (var v in others) sb.AppendLine($"- {v}");
        sb.AppendLine();
    }

    // --- columns ----------------------------------------------------------
    var columns = cn.GetSchema("Columns");
    sb.AppendLine("## Columns");
    sb.AppendLine();
    foreach (var t in tables)
    {
        sb.AppendLine($"### {t}");
        sb.AppendLine();
        sb.AppendLine("| # | Column | Type | Size | Nullable | Default |");
        sb.AppendLine("|---:|---|---|---:|---|---|");
        var rows = columns.AsEnumerable()
            .Where(r => string.Equals(r.Field<string>("TABLE_NAME"), t, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => Convert.ToInt64(r["ORDINAL_POSITION"], CultureInfo.InvariantCulture));
        foreach (var r in rows)
        {
            var ord = r["ORDINAL_POSITION"];
            var col = r.Field<string>("COLUMN_NAME");
            var dt = (OleDbType)Convert.ToInt32(r["DATA_TYPE"], CultureInfo.InvariantCulture);
            var len = r["CHARACTER_MAXIMUM_LENGTH"] is DBNull
                ? ""
                : Convert.ToInt64(r["CHARACTER_MAXIMUM_LENGTH"], CultureInfo.InvariantCulture).ToString("N0", CultureInfo.InvariantCulture);
            var nul = r["IS_NULLABLE"] is DBNull
                ? "?"
                : (Convert.ToBoolean(r["IS_NULLABLE"], CultureInfo.InvariantCulture) ? "YES" : "NO");
            var def = r["COLUMN_HASDEFAULT"] is not DBNull && Convert.ToBoolean(r["COLUMN_HASDEFAULT"], CultureInfo.InvariantCulture)
                ? Convert.ToString(r["COLUMN_DEFAULT"], CultureInfo.InvariantCulture) ?? ""
                : "";
            sb.AppendLine($"| {ord} | {col} | {dt} | {len} | {nul} | {Escape(def)} |");
        }
        sb.AppendLine();
    }

    // --- indexes ----------------------------------------------------------
    sb.AppendLine("## Indexes");
    sb.AppendLine();
    try
    {
        var ix = cn.GetSchema("Indexes");
        sb.AppendLine("| Table | Index | Column | Ord | Unique | PK |");
        sb.AppendLine("|---|---|---|---:|---|---|");
        foreach (var r in ix.AsEnumerable()
                     .OrderBy(r => r.Field<string>("TABLE_NAME"), StringComparer.OrdinalIgnoreCase)
                     .ThenBy(r => r.Field<string>("INDEX_NAME"), StringComparer.OrdinalIgnoreCase)
                     .ThenBy(r => Convert.ToInt64(r["ORDINAL_POSITION"], CultureInfo.InvariantCulture)))
        {
            sb.AppendLine($"| {r.Field<string>("TABLE_NAME")} | {r.Field<string>("INDEX_NAME")} | {r.Field<string>("COLUMN_NAME")} | {r["ORDINAL_POSITION"]} | {Flag(r, "UNIQUE")} | {Flag(r, "PRIMARY_KEY")} |");
        }
    }
    catch (Exception ex)
    {
        sb.AppendLine($"_Indexes schema unavailable: {ex.Message}_");
    }
    sb.AppendLine();

    // --- foreign keys -----------------------------------------------------
    sb.AppendLine("## Foreign keys");
    sb.AppendLine();
    try
    {
        var fk = cn.GetSchema("ForeignKeys");
        if (fk.Rows.Count == 0)
        {
            sb.AppendLine("_None._");
        }
        else
        {
            sb.AppendLine("| Child table | Child column | Parent table | Parent column | Name |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var r in fk.AsEnumerable())
            {
                sb.AppendLine($"| {r.Field<string>("FK_TABLE_NAME")} | {r.Field<string>("FK_COLUMN_NAME")} | {r.Field<string>("PK_TABLE_NAME")} | {r.Field<string>("PK_COLUMN_NAME")} | {r.Field<string>("FK_NAME")} |");
            }
        }
    }
    catch (Exception ex)
    {
        sb.AppendLine($"_ForeignKeys schema unavailable: {ex.Message}_");
    }
    sb.AppendLine();

    // --- sample rows ------------------------------------------------------
    sb.AppendLine($"## Sample rows (first {SampleRows}, text truncated to {TextTruncate} chars)");
    sb.AppendLine();
    foreach (var t in tables)
    {
        sb.AppendLine($"### {t}");
        sb.AppendLine();
        if (counts.TryGetValue(t, out var c) && c == 0)
        {
            sb.AppendLine("_Empty._");
            sb.AppendLine();
            continue;
        }
        try
        {
            using var cmd = new OleDbCommand($"SELECT TOP {SampleRows} * FROM [{t}]", cn);
            using var rd = cmd.ExecuteReader();
            var head = new List<string>();
            for (var i = 0; i < rd.FieldCount; i++) head.Add(rd.GetName(i));
            sb.AppendLine("| " + string.Join(" | ", head) + " |");
            sb.AppendLine("|" + string.Concat(head.Select(_ => "---|")));
            while (rd.Read())
            {
                var cells = new List<string>();
                for (var i = 0; i < rd.FieldCount; i++) cells.Add(Cell(rd, i));
                sb.AppendLine("| " + string.Join(" | ", cells) + " |");
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"_Read failed: {ex.Message}_");
        }
        sb.AppendLine();
    }

    var outPath = Path.Combine(outputDir, name + ".md");
    File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
    Console.WriteLine($"wrote {outPath} ({tables.Count} tables)");
}

static string Flag(DataRow r, string col)
{
    if (!r.Table.Columns.Contains(col) || r[col] is DBNull) return "";
    return Convert.ToBoolean(r[col], CultureInfo.InvariantCulture) ? "yes" : "";
}

static string Cell(OleDbDataReader rd, int i)
{
    if (rd.IsDBNull(i)) return "_NULL_";
    var v = rd.GetValue(i);
    var s = v switch
    {
        byte[] b => $"0x[{b.Length} bytes]",
        DateTime d => d.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => v.ToString() ?? ""
    };
    if (s.Length > TextTruncate) s = s[..TextTruncate] + "\u2026";
    return Escape(s);
}

static string Escape(string s) =>
    s.Replace("\\", "\\\\").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

static OleDbConnection Open(string path, string password, out string provider)
{
    var errors = new List<string>();
    foreach (var p in new[] { "Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0" })
    {
        var b = new OleDbConnectionStringBuilder { Provider = p, DataSource = path };
        if (password.Length > 0) b["Jet OLEDB:Database Password"] = password;
        var cn = new OleDbConnection(b.ConnectionString);
        try
        {
            cn.Open();
            provider = p;
            return cn;
        }
        catch (Exception ex)
        {
            errors.Add($"{p}: {ex.Message}");
            cn.Dispose();
        }
    }
    throw new InvalidOperationException(string.Join(" || ", errors));
}
