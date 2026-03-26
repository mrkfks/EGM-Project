using System;
using System.IO;
using Microsoft.Data.Sqlite;

var dbPath = args.Length > 0 ? args[0] : @"C:\Users\mrkfk\Desktop\EGM-Project\EGM.API\egm.db";
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
