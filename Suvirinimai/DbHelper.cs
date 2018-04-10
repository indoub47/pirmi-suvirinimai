using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class DbHelper
    {
        public static DataTable fillDataTable (string sqlSelectStatement)
        {
            DataTable dataTable = new DataTable();
            try
            {
                DbDataAdapter dataAdapter = Program.dpf.CreateDataAdapter();
                DbConnection connection = Program.dpf.CreateConnection();
                connection.ConnectionString = Program.connectionString;
                dataAdapter.SelectCommand = Program.dpf.CreateCommand();
                dataAdapter.SelectCommand.Connection = connection;
                dataAdapter.SelectCommand.CommandText = sqlSelectStatement;

                DbCommandBuilder commandBuilder = Program.dpf.CreateCommandBuilder();
                commandBuilder.DataAdapter = dataAdapter;
                // dataTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
                dataAdapter.Fill(dataTable);
                return dataTable;
            }
            catch (DbException dbe)
            {
                MessageBox.Show("DbHelper.fillDataTable: duomenų bazės klaida arba SQL klaida");
                return null;
            }
        }

        public static object fetchSingleValue(string sqlSelectStatement)
        {
            using (DbConnection connection = Program.dpf.CreateConnection())
            {
                try
                {   
                    connection.ConnectionString = Program.connectionString;
                    DbCommand command = Program.dpf.CreateCommand();
                    command.CommandText = sqlSelectStatement;
                    command.Connection = connection;

                    object returnObject;
                    if (connection.State == ConnectionState.Closed) connection.Open();
                    returnObject = command.ExecuteScalar();
                    connection.Close();
                    return returnObject;
                }
                catch (DbException dbe)
                {
                    MessageBox.Show("DbHelper.fetchSingleValue: duomenų bazės klaida arba SQL klaida");
                    connection.Close();
                    return null;
                }
            }
        }

        public static int executeNonQuery(string sqlNonQueryStatement)
        {
            using (DbConnection connection = Program.dpf.CreateConnection())
            {
                try
                {
                    connection.ConnectionString = Program.connectionString;
                    DbCommand command = Program.dpf.CreateCommand();
                    command.CommandText = sqlNonQueryStatement;
                    command.Connection = connection;

                    int returnValue;
                    if (connection.State == ConnectionState.Closed) connection.Open();
                    returnValue = command.ExecuteNonQuery();
                    connection.Close();
                    return returnValue;
                }
                catch (DbException dbe)
                {
                    MessageBox.Show("DbHelper.executeNonQuery: duomenų bazės klaida arba SQL klaida");
                    connection.Close();
                    return 0;
                }
            }
        }       

        public static string formatDateValue(Object dateObject)
        {
            string dateOrNull;
            try
            {
                dateOrNull = string.Format("#{0}#", ((DateTime)dateObject).ToString("yyyy-MM-dd"));
            }
            catch
            {
                dateOrNull = "NULL";
            }
            return dateOrNull;
        }

        public static string escape(string text)
        {
            return text.Replace('"', '\"').Replace("'", "\"");
        }
    }
}
