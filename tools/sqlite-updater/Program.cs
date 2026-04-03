using System;
using System.IO;
using Microsoft.Data.Sqlite;

var dbPath = args.Length > 0 ? args[0] : @"C:\Users\mrkfk\Desktop\EGM-Project\EGM.API\egm.db";

// Mode: "sql" for raw SQL execution, default is role update
if (args.Length > 1 && args[1] == "sql")
{
    // args[2..] are SQL statements to execute
    Console.WriteLine($"DB: {dbPath}");
    if (!File.Exists(dbPath)) { Console.WriteLine("ERROR: DB file not found."); return 1; }
    var connString2 = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    using var conn2 = new SqliteConnection(connString2);
    conn2.Open();
    using var tran2 = conn2.BeginTransaction();
    for (int i = 2; i < args.Length; i++)
    {
        Console.WriteLine($"Running: {args[i]}");
        using var cmd2 = conn2.CreateCommand();
        cmd2.CommandText = args[i];
        var trimmed = args[i].TrimStart().ToUpperInvariant();
        if (trimmed.StartsWith("SELECT") || trimmed.StartsWith("PRAGMA"))
        {
            using var rdr2 = cmd2.ExecuteReader();
            while (rdr2.Read())
            {
                var cols = new string[rdr2.FieldCount];
                for (int c = 0; c < rdr2.FieldCount; c++) cols[c] = $"{rdr2.GetName(c)}={rdr2.GetValue(c)}";
                Console.WriteLine("  " + string.Join(", ", cols));
            }
        }
        else
        {
            var rows = cmd2.ExecuteNonQuery();
            Console.WriteLine($"  -> affected rows: {rows}");
        }
    }
    tran2.Commit();
    conn2.Close();
    Console.WriteLine("Done.");
    return 0;
}

var sicil = args.Length > 1 ? int.Parse(args[1]) : 425394;
var newRole = args.Length > 2 ? args[2] : "BaskanlikYoneticisi";

Console.WriteLine($"DB: {dbPath}");
if (!File.Exists(dbPath)) { Console.WriteLine("ERROR: DB file not found."); return 1; }
var bak = dbPath + ".autobak";
File.Copy(dbPath, bak, true);
Console.WriteLine($"Backup created: {bak}");

var connString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
using var conn = new SqliteConnection(connString);
conn.Open();
using var tran = conn.BeginTransaction();
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "UPDATE Users SET Role = $role WHERE Sicil = $sicil;";
    cmd.Parameters.AddWithValue("$role", newRole);
    cmd.Parameters.AddWithValue("$sicil", sicil);
    var changed = cmd.ExecuteNonQuery();
    Console.WriteLine($"Rows updated: {changed}");
}
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT Sicil, FullName, Role FROM Users WHERE Sicil = $sicil";
    cmd.Parameters.AddWithValue("$sicil", sicil);
    using var rdr = cmd.ExecuteReader();
    while (rdr.Read())
    {
        Console.WriteLine($"Sicil: {rdr.GetInt32(0)}, FullName: {rdr.GetString(1)}, Role: {rdr.GetString(2)}");
    }
}
tran.Commit();
conn.Close();
return 0;
