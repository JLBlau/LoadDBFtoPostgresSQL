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
        string _dbfConnString;
        static bool _isFoxPro;
        static bool _isDbase;
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
            if (!dbaseButton1.Checked && !foxProButton1.Checked)
                MessageBox.Show("DBF Type is Required.");
            else
            {
                _isDbase = dbaseButton1.Checked;
                _isFoxPro = foxProButton1.Checked;
                LoadDBF();
            }
        }

        private void LoadDBF()
        {

            var stopwatch = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch.Start();
            lbMessages.Items.Add($"Processing started at {DateTime.Now}");
            var masterConnString = $"Server={serverName.Text};Database=postgres;Port=5432;Username={sqlUserName.Text};Password={sqlPassword.Text};SslMode=Require;";
            _masterConnString = masterConnString;
            _newConnString = $"Server={serverName.Text};Database={newSQLDBName.Text};Port=5432;Username={sqlUserName.Text};Password={sqlPassword.Text};SslMode=Require;ClientEncoding=\"sql-ASCIIEncoding\";";

            if (dbaseButton1.Checked)
                //_dbfConnString = $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbfPath.Text};Extended Properties=dBASE IV;User ID=Admin";
                _dbfConnString =  $"Provider = Microsoft.Jet.OLEDB.4.0; Data Source ={dbfPath.Text}; Extended Properties = dBase 5.0";
            else
                _dbfConnString = $"Provider=vfpoledb;Data Source={dbfPath.Text};CollatingSequence=general;";


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
                lbMessages.Items.Add($"Cannot open connection to {serverName.Text}. Ex: {ex.Message}");
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
            _newConnString = $"Server ={serverName.Text}; Database = {newSQLDBName.Text}; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};Ssl Mode=Require;";
            connection.ConnectionString = _newConnString;
            //make sure we can connect to the new db
            connection.Open();
            connection.Close();

            return success;
        }

        private DataTable ConnectToDBFGetTableList()
        {
            OleDbConnection tableConn = new OleDbConnection(_dbfConnString);
            
            tableConn.Open();
            DataTable tables = new DataTable();
            tables = tableConn.GetSchema("Tables");
            tableConn.Close();
            tableConn.Dispose();


            foreach (var t in tables.AsEnumerable())
            {

                try
                {
                    var conn1 = new OleDbConnection(_dbfConnString);
                    conn1.Open();

                    var tableName = t[2].ToString();
                        DataTable columns = new DataTable();
                        columns = conn1.GetSchema("Columns", new[] { null, null, tableName });

                        using (OleDbCommand comm = conn1.CreateCommand())
                        {
                            comm.CommandText = BuildSelectStatement(columns, tableName);
                            comm.CommandType = CommandType.Text;
                            var dataReader = comm.ExecuteReader();
                            CreateDestinationTable(columns, _newConnString, tableName);
                            InsertPostgresData(dataReader, columns,_newConnString, tableName);
                        }
                        conn1.Close();
                        conn1.Dispose();
                    
                }
                catch (OleDbException ex)
                {
                    var tableName = t[2].ToString();
                    lbMessages.Items.Add($"Error occurred on table {tableName}. Exception: {ex.Message}");
                    continue;

                }
                catch (Exception ex)
                {
                    var tableName = t[2].ToString();
                    lbMessages.Items.Add($"Error occurred on table {tableName}. Exception: {ex.Message}");
                    continue;
                }
            }

                return tables;
            
        }

        private void CreateDestinationTable(DataTable columnsDataTable, string connString, string tableName)
        {
            lbMessages.Items.Add($"Create table Started for Table: {tableName}");
            NpgsqlConnection conn = new NpgsqlConnection();
            conn.ConnectionString = connString;
            using (conn)
            {
                if(conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    conn.Dispose();
                    conn = new NpgsqlConnection();
                    conn.ConnectionString = connString;
                }
                conn.Open();
                string createSql = "";
                try
                {
                    using (conn)
                    {
                        createSql = BuildCreateStatement(columnsDataTable, tableName);
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
            }
            lbMessages.Items.Add($"Table {tableName} created successfully.");
        }
        //private void InsertPostgresData(DataTable dataTable, string connString)
        //{
        //    lbMessages.Items.Add("Insert Data started for Table: " + dataTable.TableName);

        //    NpgsqlConnection conn = new NpgsqlConnection(connString);
        //    StringBuilder sb = new StringBuilder();
        //    conn.Open();
        //    using (conn)
        //    {
        //        using (dataTable)
        //        {

        //            if (conn.State != ConnectionState.Open)
        //            {
        //                conn.Close();
        //                conn.Dispose();
        //                conn = new NpgsqlConnection(connString);
        //                conn.Open();
        //            }
        //            var setEncoding = "set client_encoding = 'WIN1252'";
        //            var command = conn.CreateCommand();
        //            command.CommandText = setEncoding;
        //            command.ExecuteNonQuery();
                    
        //            using (var bulk = new BulkOperation(conn))
        //            {
        //                try
        //                {
        //                    bulk.Provider = ProviderType.PostgreSql;
        //                    bulk.DestinationSchemaName = "public";
        //                    bulk.DestinationTableName = dataTable.TableName.ToLower();
        //                    bulk.ColumnMappings = BuildColumnMapping(dataTable);
        //                    bulk.AutoMapOutputIdentity = true;
        //                    bulk.AutoTruncate = true;
                            
        //                    bulk.Log = s => sb.AppendLine(s);
                         
        //                    bulk.BulkInsert(dataTable);

        //                    lbMessages.Items.Add("Insert Data Completed Successfully for Table: " + dataTable.TableName);
        //                }
        //                catch (SqlException ex)
        //                {
        //                    lbMessages.Items.Add("SQL Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace);
        //                }
        //                catch (Exception ex2)
                        
        //                {
        //                    lbMessages.Items.Add("Exception:" + ex2.Message + ", Stack Trace:" + ex2.StackTrace);
        //                }
        //                conn.Close();
        //                conn.Dispose();
                        
        //            }
        //        }
        //        lbMessages.Items.Add($"StringBuilderLog for DataTable:{dataTable.TableName} {sb.ToString()}");
        //        dataTable.Clear();
        //        dataTable.Dispose();
        //    }
        //}

        private void InsertPostgresData(OleDbDataReader dataReader, DataTable columns, string connString, string tableName)
        {
            lbMessages.Items.Add("Insert Data started for Table: " + tableName);

            NpgsqlConnection conn = new NpgsqlConnection(connString);
            StringBuilder sb = new StringBuilder();
            conn.Open();
            using (conn)
            {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Close();
                        conn.Dispose();
                        conn = new NpgsqlConnection(connString);
                        conn.Open();
                    }
                    var setEncoding = "set client_encoding = 'utf8'";
                    var command = conn.CreateCommand();
                    command.CommandText = setEncoding;
                    command.ExecuteNonQuery();

                    using (var bulk = new BulkOperation(conn))
                    {
                        try
                        {
                            bulk.Connection = conn;
                            
                            bulk.BatchSize = 10000;
                            bulk.BatchTimeout = 360;
                            bulk.Provider = ProviderType.PostgreSql;
                            bulk.DestinationSchemaName = "public";
                            bulk.DestinationTableName = tableName.ToLower();

                            bulk.ColumnMappings = BuildColumnMapping(columns);
                            bulk.AutoTruncate = true;
                            bulk.DataSource = dataReader;
                            bulk.UseLogDump = true;
                            bulk.LogDump = sb;

                            bulk.BulkInsert();


                            lbMessages.Items.Add("Insert Data Completed Successfully for Table: " + tableName);
                        }
                        catch (SqlException ex)
                        {
                            lbMessages.Items.Add($"LogDump:{sb.ToString()}");
                            lbMessages.Items.Add("SQL Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace);
                        }
                        catch (Exception ex2)

                        {
                            lbMessages.Items.Add($"LogDump:{sb.ToString()}");
                            lbMessages.Items.Add("Exception:" + ex2.Message + ", Stack Trace:" + ex2.StackTrace);
                        }
                        conn.Close();
                        conn.Dispose();

                    }
                }
                lbMessages.Items.Add($"StringBuilderLog for DataTable:{tableName} {sb.ToString()}");
                //dataTable.Clear();
                //dataTable.Dispose();
            
        }

        private static List<ColumnMapping> BuildColumnMapping(DataTable dt)
        {
            List<ColumnMapping> mappings = new List<ColumnMapping>();

            foreach(var row in dt.AsEnumerable().OrderBy(x => x.ItemArray[6]))
            {
                ColumnMapping colMap;
                var columName = row[3].ToString();
                Type dataType = GetDotNetDataType(row[11].ToString());
                var ordinal = int.Parse(row[6].ToString());


                colMap = new ColumnMapping
                { 
                    SourceName = columName.ToString(),
                    DestinationName = columName.ToString().ToLower()
                };
                mappings.Add(colMap);
            }
            return mappings;
        }

        private static string BuildCreateStatement(DataTable columnsDataTable, string tableName)
        {

            int i = 1;
            string sqlColumnCreate = "";
            foreach (var row in columnsDataTable.AsEnumerable().OrderBy(x => x.ItemArray[6]))
            {
                
                var columName = row[3].ToString();
                var dataTypeString = row[11].ToString();
                var maxLength = row[13].ToString();
                var precision = row[15].ToString();
                var scaleString = row[16].ToString();
                int.TryParse(scaleString, out int scale);
                var dataType = GetDataType(dataTypeString,maxLength, precision, scale);

                string primKey = "";
                if (i == 1 && columnsDataTable.Rows.Count > 1)
                {
                        primKey = " PRIMARY KEY";
                        sqlColumnCreate = $"CREATE TABLE  \"{tableName.ToLower()}\" ( \"{columName.ToLower().ToString().Trim()}\"   { dataType}, ";
                }
                else if (i == 1 && columnsDataTable.Rows.Count == 1)
                {
                    sqlColumnCreate = $"CREATE TABLE \"{tableName.ToLower()}\" ( \"{columName.ToLower().ToString().Trim()}\"   { dataType}); ";

                }
                else if (i == columnsDataTable.Rows.Count)
                    sqlColumnCreate = sqlColumnCreate + $" \"{columName.ToLower().ToString().Trim()}\" {dataType}); ";
                else
                    sqlColumnCreate = sqlColumnCreate + $" \"{columName.ToLower().ToString().Trim()}\" {dataType}, ";
                i++;
            }
            return sqlColumnCreate;
        }

          private static string BuildSelectStatement(DataTable dataTable, string tableName)
        {

            int i = 1;
            string sqlSelect = "";
            foreach (var row in dataTable.AsEnumerable().OrderBy(x=> x.ItemArray[6]))
            {
                var columName = row[3].ToString();
                var dataTypeString = row[11].ToString();
                var maxLength = row[13].ToString();
                var precision = row[15].ToString();
                var scaleString = row[16].ToString();
                int.TryParse(scaleString, out int scale);
                var dataType = _isDbase ? GetDBaseDataType(dataTypeString, maxLength, precision, scale) :
                    GetFoxProDataType(dataTypeString, maxLength, precision, scale);

                string primKey = "";
                if (i == 1 && dataTable.Rows.Count > 1)
                {
                    sqlSelect = $"Select {columName.ToString().Trim()} , ";
                }
                else if (i == 1 && dataTable.Rows.Count == 1)
                {
                    sqlSelect = $"Select {columName.ToString().Trim()} FROM [{tableName.ToString()}] ";
                }
                else if (i == dataTable.Rows.Count)
                    sqlSelect = sqlSelect + $" {columName.ToString().Trim()}  FROM [{tableName.ToString()}] ";
                else
                    sqlSelect = sqlSelect + $" {columName.ToString().Trim()}, ";
                i++;
            }
            return sqlSelect;
        }
        private static string GetDataType(string dataType, string maxLength, string precision = "0", int scale = 0)
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
                    dataType = "Boolean";
                    break;
                case "200":
                case "8":
                    maxLength = maxLength == String.Empty ? "65535" : maxLength;
                    dataType = $"Character Varying({maxLength})";
                    break;
                case "130":
                    dataType = "Text";
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
                case "131":
                case "5":
                case "14":
                    precision = precision == String.Empty ? "19" : precision;
                    //scale = scale == 0 ? 2 : scale;
                    dataType = $"NUMERIC({precision}, {scale})";
                    break;
                case "72":
                    dataType = $"UUID";
                    break;
                case "3":
                    dataType = "Integer";
                    break;
                default:
                    dataType = $"Character Varying(65535)";
                    break;
            };

            return dataType;

        }

        private static string GetFoxProDataType(string dataType, string maxLength, string precision = "0", int scale = 0)
        {
            //254 is the max length of a column on FoxPro

            switch (dataType)
            {
                case "129":
                    dataType = $"C";
                    break;
                case "20":
                case "131":
                    if(scale == 0)
                    dataType = $"N({precision})";
                    else
                    dataType = $"N({precision}, { scale})";
                    break;
                case "133":
                    dataType = "D";
                   // dataType = $"VARCHAR(254)";
                    break;
                case "134":
                    dataType = "C";
                    break;
                case "7":
                case "135":
                    dataType = "DateTime";
                    break;

                case "204":
                case "128":
                    dataType = "Varbinary";
                    break;
                case "11":
                    dataType = $"Boolean";
                    break;
                case "8":
                    dataType = $"VARCHAR(254)";
                    break;
                case "6":
                    dataType = "Numeric";
                    break;
                case "14":
                    dataType = "Numeric";
                    break;
                case "5":
                    dataType = "Numeric";
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

        private static string GetDBaseDataType(string dataType, string maxLength, string precision = "0", int scale = 0)
        {
            //254 is the max length of a column on FoxPro

            switch (dataType)
            {
                case "8":
                case "11":
                case "72":
                case "129":
                case "130":
                case "134":
                    dataType = $"Char({maxLength})";
                    break;
                case "7":
                case "133":
                case "135":
                    dataType = "DATE";
                    // dataType = $"VARCHAR(254)";
                    break;
                case "20":
                    dataType = $"DECIMAL({precision}, {scale})";
                    break;
                case "204":
                case "128":
                    dataType = "BINARY";
                    break;
                case "6":
                    dataType = $"DECIMAL({precision}, {scale})";
                    break;
                case "14":
                    dataType = $"DECIMAL({precision}, {scale})";
                    break;
                case "5":
                    //double
                    dataType = $"DECIMAL({precision}, {scale})";
                    break;

                default:
                    dataType = $"Text";
                    break;
            };

            return dataType;

        }

        private static System.Type GetDotNetDataType(string dataType)
        {
           switch (dataType)
            {
                case "0": return typeof(Nullable);
                case "2": return typeof(Int16);
                case "3": return typeof(Int32);
                case "4": return typeof(Single);
                case "5": return typeof(Double);
                case "6": return typeof(Decimal);
                case "7": return typeof(DateTime);
                case "8": return typeof(String);
                case "9": return typeof(Object);
                case "10": return typeof(Exception);
                case "11": return typeof(Boolean);
                case "12": return typeof(Object);
                case "13": return typeof(Object);
                case "14": return typeof(Decimal);
                case "16": return typeof(SByte);
                case "17": return typeof(Byte);
                case "18": return typeof(UInt16);
                case "19": return typeof(UInt32);
                case "20": return typeof(Int64);
                case "21": return typeof(UInt64);
                case "64": return typeof(DateTime);
                case "72": return typeof(Guid);
                case "128": return typeof(Byte[]);
                case "129": return typeof(String);
                case "130": return typeof(String);
                case "131": return typeof(Decimal);
                case "133": return typeof(DateTime);
                case "134": return typeof(TimeSpan);
                case "135": return typeof(DateTime);
                case "138": return typeof(Object);
                case "139": return typeof(Decimal);
                case "200": return typeof(String);
                case "201": return typeof(String);
                case "202": return typeof(String);
                case "203": return typeof(String);
                case "204": return typeof(Byte[]);
                case "205": return typeof(Byte[]);
                default: return typeof(String);
            };
        }

        private void dbfPath_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
