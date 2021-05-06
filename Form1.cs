using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using NpgsqlTypes;
using Z.BulkOperations;

namespace LoadFoxProDBToSQL
{
    public partial class Form1 : Form
    {
        string _path;
        NpgsqlConnection _sqlConnection;
        string _masterConnString;
        string _newConnString;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void selectFolder_Click(object sender, EventArgs e)
        {
            int size = -1;
            string path = null;
            FolderBrowserDialog pathFileDialog = new FolderBrowserDialog();
            DialogResult result = pathFileDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                path = pathFileDialog.SelectedPath.ToString();
                _path = path;
                dbfPath.Text = path;
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            LoadDBF();
        }

        private void LoadDBF()
        {

            Stopwatch stopwatch = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch.Start();
            lbMessages.Items.Add($"Processing started at {DateTime.Now}");
            var masterConnString = $"Server={sqlServerName.Text};Database=postgres;Port=5432;Username={sqlUserName.Text};Password={sqlPassword.Text};SslMode=Require;";
            _masterConnString = masterConnString;
            _newConnString = $"Server={sqlServerName.Text};Database={newSQLDBName.Text};Port=5432;Username={sqlUserName.Text};Password={sqlPassword.Text};SslMode=Require;";
            bool sqlSuccess = ConnectAndCreateSQLDB(_masterConnString);

            var tableList = ConnectToDBFGetTableList();




        }

        private bool ConnectAndCreateSQLDB(string connString)
        {
            bool success = false; 
            //connect to SQL First

            Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(connString);
            _sqlConnection = connection;
            try
            {
                connection.Open();

            }
            catch (Exception ex)
            {
                lbMessages.Items.Add($"Cannot open connection to {sqlServerName.Text}. Ex: {ex.Message}");
            }
            try
            {
                //Create the DB to house the DB data
                NpgsqlCommand sqlCreateTableCmd = new NpgsqlCommand($"CREATE DATABASE \"{newSQLDBName.Text.Trim()}\";", connection);
                sqlCreateTableCmd.ExecuteNonQuery();
                success = true;
            }
            catch (Exception ex)
            {
                lbMessages.Items.Add($"Error occurred creating new DB {newSQLDBName.Text}. Ex:{ex.Message}");

            }
            connection.Close();
            _newConnString = $"Server ={sqlServerName.Text}; Database = {newSQLDBName.Text}; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};Ssl Mode=Require;";
            connection.ConnectionString = _newConnString;
            //make sure we can connect to the new db
            connection.Open();
            connection.Close();

            return success;
        }

        private DataTable ConnectToDBFGetTableList()
        {
            var dbConnString = $"Provider = vfpoledb; Data Source = {dbfPath.Text}; Collating Sequence = general;";
            OleDbConnection ole = new OleDbConnection(dbConnString);

            ole.Open();
            DataTable tables = ole.GetSchema("Tables");
            //DataTable columns = ole.GetSchema("Columns");

            foreach (var t in tables.AsEnumerable())
            {
                try
                {
                    var tableName = t[2].ToString();
                    DataTable columns = ole.GetSchema("Columns", new[] { null, null, tableName });

                    OleDbCommand comm = ole.CreateCommand();
                    comm.CommandText = BuildSelectStatement(columns, tableName);
                    comm.CommandType = CommandType.Text;
                    var dataReader = comm.ExecuteReader();
                    DataTable tableData = new DataTable();
                    tableData.Load(dataReader);
                    tableData.TableName = tableName;

                    CreateDestinationTable(columns, _newConnString, tableName);
                    InsertPostgresData(tableData, _newConnString);
                }
                catch(Exception ex)
                {
                    var tableName = t[2].ToString();
                    lbMessages.Items.Add($"Error occurred on table {tableName}. Exception: {ex.Message}");
                }
            }

            return tables;
        }

        private void CreateDestinationTable(DataTable dataTable, string connString, string tableName)
        {
            lbMessages.Items.Add($"Create table Started for Table: {dataTable.TableName}");
            NpgsqlConnection conn = new NpgsqlConnection();
            conn.ConnectionString = connString;
            conn.Open();
            string createSql = "";
            try
            {
                using (conn)
                {
                    createSql = BuildCreateStatement(dataTable, tableName);
                    NpgsqlCommand createCmd = conn.CreateCommand();
                    createCmd.CommandText = createSql;
                    createCmd.ExecuteNonQuery();
                    createCmd.Dispose();
                }
            }
            catch (SqlException ex)
            {
                lbMessages.Items.Add("Sql Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace + " for tablename: " + tableName + " sql statement " + createSql);
            }
            catch (Exception ex2)
            {
                lbMessages.Items.Add("Create Table Error:" + ex2.Message + ", Stack Trace:" + ex2.StackTrace + " for tablename: " + tableName + " sql statement " + createSql);
            }
            conn.Close();
            conn.Dispose();
            lbMessages.Items.Add($"Table {tableName} created successfully.");
        }
        private void InsertPostgresData(DataTable dataTable, string connString)
        {
            lbMessages.Items.Add("Insert Data started for Table: " + dataTable.TableName);

            NpgsqlConnection conn = new NpgsqlConnection(connString);
            conn.Open();
            using (conn)
            {
                using (dataTable)
                {

                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    using (var bulk = new BulkOperation(conn))
                    {
                        try
                        {
                            bulk.Provider = ProviderType.PostgreSql;
                            bulk.DestinationSchemaName = "public";
                            bulk.DestinationTableName = dataTable.TableName;
                            bulk.ColumnMappings = BuildColumnMapping(dataTable);
                            bulk.AutoTruncate = true;
                         
                            bulk.BulkInsert(dataTable);

                            lbMessages.Items.Add("Insert Data Completed Successfully for Table: " + dataTable.TableName);
                        }
                        catch (SqlException ex)
                        {
                            lbMessages.Items.Add("SQL Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace);
                        }
                        catch (Exception ex2)
                        {
                            lbMessages.Items.Add("Exception:" + ex2.Message + ", Stack Trace:" + ex2.StackTrace);
                        }
                    }
                }
            }
        }

        private static List<ColumnMapping> BuildColumnMapping(DataTable dt)
        {
            List<ColumnMapping> mappings = new List<ColumnMapping>();

            foreach(var col in dt.Columns)
            {
                var dataTypeName = col.GetType().Name;
                //NpgsqlDbType dbType;

                //switch (dataTypeName)
                //{
                //    case "System.Boolean":
                //        dbType = NpgsqlDbType.Boolean;
                //        break;
                //    case "System.Byte":
                //        dbType = NpgsqlDbType.Smallint;
                //        break;
                //    case "System.Decimal":
                //        dbType = NpgsqlDbType.Numeric;
                //        break;
                //    case "System.Double":
                //        dbType = NpgsqlDbType.Double;
                //        break;
                //    case "System.Int32":
                //    case "System.Integer":
                //        dbType = NpgsqlDbType.Integer;
                //        break;
                //    case "System.Int16":
                //        dbType = NpgsqlDbType.Smallint;
                //        break;
                //    case "System.String":
                //        dbType = NpgsqlDbType.Text;
                //        break;
                //    case "System.Date":
                //        dbType = NpgsqlDbType.Date;
                //        break;
                //    case "System.DateTime":
                //        dbType = NpgsqlDbType.Timestamp;
                //        break;
                //    default:
                //        dbType = NpgsqlDbType.Text;
                //        break;
                //}

                ColumnMapping colMap = new ColumnMapping(col.ToString() );
                mappings.Add(colMap);
            }
            return mappings;
        }

        private static string BuildCreateStatement(DataTable dataTable, string tableName)
        {

            int i = 1;
            string sqlColumnCreate = "";
            foreach (var row in dataTable.AsEnumerable())
            {
                var columName = row[3].ToString();
                var dataTypeString = row[11].ToString();
                var maxLength = row[13].ToString();
                var precision = row[15].ToString();
                var dataType = GetDataType(dataTypeString,maxLength, precision);

                string primKey = "";
                if (i == 1)
                {
                        primKey = " PRIMARY KEY";
                        sqlColumnCreate = $"CREATE TABLE { tableName} ( \"{columName.ToString().Trim()}\"   { dataType}, ";
                }
                else if (i == dataTable.Rows.Count)
                    sqlColumnCreate = sqlColumnCreate + $" \"{columName.ToString().Trim()}\" {dataType}); ";
                else
                    sqlColumnCreate = sqlColumnCreate + $" \"{columName.ToString().Trim()}\" {dataType}, ";
                i++;
            }
            return sqlColumnCreate;
        }

        private static string BuildSelectStatement(DataTable dataTable, string tableName)
        {

            int i = 1;
            string sqlSelect = "";
            foreach (var row in dataTable.AsEnumerable())
            {
                var columName = row[3].ToString();
                var dataTypeString = row[11].ToString();
                var maxLength = row[13].ToString();
                var precision = row[15].ToString();
                var dataType = GetFoxProDataType(dataTypeString, maxLength, precision);


                string primKey = "";
                if (i == 1 && dataTable.Rows.Count > 1)
                {
                    sqlSelect = $"Select CAST({columName.ToString().Trim()} as {dataType}) as  {columName.ToString()} , ";
                }
                else if (i == 1 && dataTable.Rows.Count == 1)
                {
                    sqlSelect = $"Select CAST({columName.ToString().Trim()} AS {dataType}) as  {columName.ToString()} FROM {tableName.ToString()} ";
                }
                else if (i == dataTable.Rows.Count)
                    sqlSelect = sqlSelect + $" CAST({columName.ToString().Trim()}  as {dataType}) as {columName.ToString()} FROM {tableName.ToString()}";
                else
                    sqlSelect = sqlSelect + $" CAST({columName.ToString().Trim()} as {dataType})  as {columName.ToString()}, ";
                i++;
            }
            return sqlSelect;
        }
        private static string GetDataType(string dataType, string maxLength, string precision = "0")
        {
            //254 is the max length of a column on FoxPro
            
            switch(dataType)
            {
                case "129":
                    dataType = $"Text";
                    break;
                case "7":
                case "133":
                    dataType = "Date";
                    //dataType = $"Character Varying(MAX)";
                    break;
                case "20":
                    dataType = "bigint";
                    break;
                case "128":
                    dataType = "bytea";
                    break;
                case "11":
                    dataType = "Character(2)";
                    break;
                case "8":
                    dataType = $"Character Varying(500)";
                    break;
                case "6":
                    dataType = "money";
                    break;
                case "134":
                    dataType = "Timestamp";
                    break;
                case "135":
                    dataType = "Timestamp";
                    break;
                case "14":
                    dataType = "NUMERIC(19, 4)";
                    break;
                case "5":
                    dataType = "NUMERIC(19,2)";
                    break;
                case "72":
                    dataType = $"UUID";
                    break;
                default:
                    dataType = "Character Varying(500)";
                    break;
            };

            return dataType;

        }

        private static string GetFoxProDataType(string dataType, string maxLength, string precision = "0")
        {
            //254 is the max length of a column on FoxPro

            switch (dataType)
            {
                case "129":
                    dataType = $"VARCHAR(254)";
                    break;
                case "133":
                    //dataType = "Date";
                    dataType = $"VARCHAR(254)";
                    break;
                case "20":
                    dataType = "VARCHAR(254)";
                    break;
                case "204":
                case "128":
                    dataType = "Varbinary";
                    break;
                case "11":
                    dataType = "VARCHAR(1)";
                    break;
                case "8":
                    dataType = $"VARCHAR(254)";
                    break;
                case "6":
                    dataType = "VARCHAR(254)";
                    break;
                case "7":
                    dataType = "VARCHAR(254)";
                    break;
                case "134":
                    dataType = "VARCHAR(254)";
                    break;
                case "135":
                    dataType = "VARCHAR(254)";
                    break;
                case "14":
                    dataType = "VARCHAR(254)";
                    break;
                case "5":
                    dataType = "VARCHAR(254)";
                    break;
                case "72":
                    dataType = $"VARCHAR(254)";
                    break;
                default:
                    dataType = "VARCHAR(254)";
                    break;
            };

            return dataType;

        }

        private void dbfPath_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
