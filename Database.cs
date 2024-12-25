using System;
using System.Data.SQLite;
using System.IO;

public class Database
{
    private SQLiteConnection connection;

    public Database()
    {
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        string dbFile = "game_results.db";
        if (!File.Exists(dbFile))
        {
            SQLiteConnection.CreateFile(dbFile);
            connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();

            string createTableQuery = "CREATE TABLE IF NOT EXISTS Results (Id INTEGER PRIMARY KEY, Result TEXT, Date DATETIME)";
            SQLiteCommand command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }
        else
        {
            connection = new SQLiteConnection($"Data Source={dbFile};Version=3;");
            connection.Open();
        }
    }

    public void SaveResult(string result)
    {
        string insertQuery = "INSERT INTO Results (Result, Date) VALUES (@result, @date)";
        SQLiteCommand command = new SQLiteCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@result", result);
        command.Parameters.AddWithValue("@date", DateTime.Now);
        command.ExecuteNonQuery();
    }

    public string GetStatistics()
    {
        string message = "Статистика матчей:\n";
        string selectQuery = "SELECT * FROM Results ORDER BY Date DESC";
        SQLiteCommand command = new SQLiteCommand(selectQuery, connection);
        SQLiteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            message += $"Дата: {reader["Date"]}, Результат: {reader["Result"]}\n";
        }
        return message;
    }
}