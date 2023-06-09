using Microsoft.Data.SqlClient;

namespace QFileServer.Configuration
{
    public static class SqlServerTransientErrorDetector
    {
        private static readonly int[] TransientErrorNumbers =
        {
        1205, // Deadlock
        49920, // Cannot process request. Too many operations in progress for subscription "%ld".
        49919, // Cannot process create or update request. Too many create or update operations in progress for subscription "%ld".
        49918, // Cannot process request. Not enough resources to process request.
        41839, // Transaction exceeded the maximum number of commit dependencies and the last statement was aborted. Retry the statement
        41325, // The current transaction failed to commit due to a serializable validation failure.
        41305, // The current transaction failed to commit due to a repeatable read validation failure.
        41302, // The current transaction attempted to update a record that has been updated since this transaction started. The transaction was aborted.
        41301, // Dependency failure: a dependency was not found.
        1204, // The instance of the SQL Server Database Engine cannot obtain a LOCK resource at this time. Rerun your statement when there are fewer active users. Ask the database administrator to check the lock and memory configuration for this instance, or to check for long-running transactions.
        233, // A transport-level error has occurred when sending the request to the server. (provider: Shared Memory Provider, error: 0 - No process is on the other end of the pipe.)
        121, // The semaphore timeout period has expired
        64, // A connection was successfully established with the server, but then an error occurred during the login process. (provider: TCP Provider, error: 0 - The specified network name is no longer available.) 
        20, // The instance of SQL Server you attempted to connect to does not support encryption.
    };

        public static bool IsTransient(SqlException ex)
        {
            if (ex != null)
            {
                if (ex.Message.Contains("Execution Timeout Expired"))
                {
                    return true;
                }

                if (ex.Message.Contains("A network-related or instance-specific error occurred while establishing a connection to SQL Server"))
                {
                    return true;
                }

                foreach (SqlError err in ex.Errors)
                {
                    if (TransientErrorNumbers.Contains(err.Number))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
