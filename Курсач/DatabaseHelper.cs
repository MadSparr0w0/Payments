using System;
using System.Configuration;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace UtilityPaymentsManager
{
    public static class DatabaseHelper
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["UtilityPaymentsConnection"].ConnectionString;

        public static DataTable GetTableData(string tableName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    string query = $"SELECT * FROM {tableName} ORDER BY 1";
                    var adapter = new NpgsqlDataAdapter(query, connection);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                return null;
            }
        }

        public static bool SaveChanges(DataTable dataTable, string tableName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    string pkQuery = $@"
                        SELECT a.attname
                        FROM pg_index i
                        JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
                        WHERE i.indrelid = '{tableName}'::regclass AND i.indisprimary";

                    string pkColumn = null;
                    using (var cmd = new NpgsqlCommand(pkQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pkColumn = reader.GetString(0);
                        }
                    }

                    var adapter = new NpgsqlDataAdapter($"SELECT * FROM {tableName} WHERE 1=0", connection);
                    var builder = new NpgsqlCommandBuilder(adapter);

                    adapter.InsertCommand = builder.GetInsertCommand(true);
                    adapter.UpdateCommand = builder.GetUpdateCommand(true);
                    adapter.DeleteCommand = builder.GetDeleteCommand();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                        {
                            foreach (DataColumn column in dataTable.Columns)
                            {
                                if (row[column] == DBNull.Value || row[column].ToString() == "")
                                {
                                    if (column.ColumnName.EndsWith("_id") && column.ColumnName != pkColumn)
                                    {
                                        row[column] = DBNull.Value;
                                    }
                                }
                            }
                        }
                    }

                    adapter.Update(dataTable);
                    dataTable.AcceptChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n\n{ex.StackTrace}");
                return false;
            }
        }

        public static DataTable GetForeignKeyData(string foreignTable, string idColumn, string displayExpression)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    string query = $"SELECT {idColumn}, ({displayExpression}) AS display_value FROM {foreignTable} ORDER BY display_value";
                    var adapter = new NpgsqlDataAdapter(query, connection);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных внешнего ключа: {ex.Message}");
                return null;
            }
        }
    }
}