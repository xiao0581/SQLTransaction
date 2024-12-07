using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

const string ConnectionString = "Data Source=XIAO-PC\\XIAODATA;Integrated Security=True;Database=TdataTest;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

Console.WriteLine("Testing SQL Server Default Transaction Level...");

// create a task to update the balance without committing
Task updateTask = Task.Run(() => UpdateWithoutCommit());

// create a task to query the balance
Task queryTask = Task.Run(() => QueryBalance());

await Task.WhenAll(updateTask, queryTask);

Console.WriteLine("Test Completed.");

void UpdateWithoutCommit()
{
    using (SqlConnection conn = new SqlConnection(ConnectionString))
    {
        conn.Open();

        using (SqlTransaction transaction = conn.BeginTransaction())
        {
            try
            {
                Console.WriteLine("Starting Update Transaction...");

                // update record but not commit
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Accounts SET Balance = Balance - 100 WHERE AccountId = 1", conn, transaction);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Update done, but not committed. Waiting...");
                Task.Delay(5000).Wait(); 

                Console.WriteLine("Rolling back transaction.");
                transaction.Rollback(); //rollback the transaction
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex.Message}");
            }
        }
    }
}

void QueryBalance()
{
    using (SqlConnection conn = new SqlConnection(ConnectionString))
    {
        conn.Open();

        Console.WriteLine("Querying balance...");
        SqlCommand cmd = new SqlCommand(
            "SELECT AccountId, Balance FROM Accounts", conn);
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine($"AccountId: {reader["AccountId"]}, Balance: {reader["Balance"]}");
            }
        }
    }
}