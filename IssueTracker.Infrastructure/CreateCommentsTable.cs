using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Server=.\\SQLEXPRESS;Database=IssueTrackerDb;Integrated Security=True;TrustServerCertificate=True";
        using var conn = new SqlConnection(connStr);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments')
            BEGIN
                CREATE TABLE Comments (
                    CommentId INT IDENTITY(1,1) PRIMARY KEY,
                    IssueId INT NOT NULL FOREIGN KEY REFERENCES Issues(IssueId) ON DELETE CASCADE,
                    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
                    Content NVARCHAR(MAX) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                );
            END";
        cmd.ExecuteNonQuery();
        Console.WriteLine("Comments table created successfully!");
    }
}
