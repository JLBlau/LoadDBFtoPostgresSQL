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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
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
        //NpgsqlConnection _pgsqlConnection;
        //SqlConnection _sqlConnection;
        DbConnection _masterdbConnection;
        DbConnection _dbConnection;
        OleDbConnection _sourceConnection;
        string _masterConnString;
        string _newConnString;
        string _sourceConnString;
        static bool _isFoxPro;
        static bool _isDbase;
        static bool _isAccess;
        static bool _isSqlServer;
        static bool _isPostgres;
        StringBuilder _messageBuilder;
        public Form1()
        {
            InitializeComponent();
        }

        private void selectFolder_Click(object sender, EventArgs e)
        {
            int size = -1;
            string path = null;
            if (!_isAccess)
            {
                FolderBrowserDialog pathFileDialog = new FolderBrowserDialog();
                DialogResult result = pathFileDialog.ShowDialog(); // Show the dialog.
                if (result == DialogResult.OK) // Test result.
                {
                    path = pathFileDialog.SelectedPath.ToString();
                    _path = path;
                    dbPath.Text = path;
                }
            }
            else
            {
                FileDialog fileDialog = new OpenFileDialog();
                DialogResult result = fileDialog.ShowDialog();
                if(result == DialogResult.OK)
                {
                    _path = fileDialog.FileName.ToString();
                    dbPath.Text = _path;
                }

            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (!dbaseButton1.Checked && !foxProButton1.Checked && !accessButton.Checked)
                MessageBox.Show("Source DB Type is Required.");
            if (!serverButton1.Checked && !serverButton2.Checked)
                MessageBox.Show("Server Type is Required");
            else
            {
                _isDbase = dbaseButton1.Checked;
                _isFoxPro = foxProButton1.Checked;
                _isAccess = accessButton.Checked;
                _isSqlServer = serverButton1.Checked;
                _isPostgres = serverButton2.Checked;
                LoadSource();
            }

        }

        private void LoadSource()
        {

            var stopwatch = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch.Start();
            _messageBuilder = new StringBuilder();
            _messageBuilder.Append($"Processing started at {DateTime.Now}" + Environment.NewLine);
            messageBox.Text = _messageBuilder.ToString();
            if (serverButton1.Checked)
            {
                var masterConnString = $"Server = {serverName.Text}; Database = master; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};";
                _masterConnString = masterConnString;
                _newConnString = $"Server ={serverName.Text}; Database = {newSQLDBName.Text}; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};";

            }
            else if(serverButton2.Checked)
            {
                var masterConnString = $"Server={serverName.Text};Database=postgres;Port=5432;Username={sqlUserName.Text};Password={sqlPassword.Text};SslMode=Require;";
                _masterConnString = masterConnString;
                _newConnString = $"Server={serverName.Text};Database={newSQLDBName.Text};Port=5432;Username={sqlUserName.Text};Password={sqlPassword.Text};SslMode=Require;";

            }

            if (_isDbase)
                //_dbfConnString = $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbfPath.Text};Extended Properties=dBASE IV;User ID=Admin";
                _sourceConnString = $"Provider = Microsoft.Jet.OLEDB.4.0; Data Source ={dbPath.Text}; Extended Properties = dBase 5.0";
            else if (_isFoxPro)
                _sourceConnString = $"Provider=vfpoledb;Data Source={dbPath.Text};CollatingSequence=general;";
            else if (_isAccess)
                _sourceConnString = $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={dbPath.Text};User ID=Admin";
            else
                throw new NotImplementedException();

            bool sqlSuccess;
            if (serverButton1.Checked)
                sqlSuccess = ConnectAndCreateSQLDB(_masterConnString);
            else
                sqlSuccess = ConnectAndCreatePostgresDB(_masterConnString);

            var tableList = GetSourceDBTableList();

            foreach (var t in tableList.AsEnumerable())
            {

                try
                {
                    var conn1 = new OleDbConnection(_sourceConnString);
                    conn1.Open();
                    _sourceConnection = conn1;

                    var tableName = t[2].ToString();
                    DataTable columns = new DataTable();
                    columns = conn1.GetSchema("Columns", new[] { null, null, tableName });
                    OleDbDataReader dataReader;

                    using (OleDbCommand comm = conn1.CreateCommand())
                    {
                        comm.CommandText = BuildSelectStatement(columns, tableName);
                        comm.CommandType = CommandType.Text;
                        dataReader = comm.ExecuteReader();


                        if (_isPostgres)
                        {
                            CreatePostgresDestinationTable(columns, _newConnString, tableName);
                            InsertData(dataReader, columns, _newConnString, tableName);
                        }
                        else if (_isSqlServer)
                        {
                            CreateSqlServerDestinationTable(columns, _newConnString, tableName);
                            InsertData(dataReader, columns, _newConnString, tableName);
                        }
                    }
                }
                catch (OleDbException ex)
                {
                    var tableName = t[2].ToString();
                    _messageBuilder.Append($"Error occurred on table {tableName}. Exception: {ex.Message}" + Environment.NewLine);
                    messageBox.Text = _messageBuilder.ToString();
                    continue;

                }
                catch (Exception ex)
                {
                    var tableName = t[2].ToString();
                    _messageBuilder.Append($"Error occurred on table {tableName}. Exception: {ex.Message}" + Environment.NewLine);
                    messageBox.Text = _messageBuilder.ToString();
                    continue;
                }
            }



        }

        private DbConnection GetDbConnection(string connString)
        {
            if (_isPostgres)
                return new NpgsqlConnection(connString);
            else
                return new SqlConnection(connString);
        }

        private bool ConnectAndCreateSQLDB(string connString)
        {
            bool success = false;
            //connect to SQL First
            SqlConnection sqlConn = (SqlConnection)GetDbConnection(connString);
            _masterdbConnection = sqlConn;
            try
            {
                sqlConn.Open();

            }
            catch (Exception ex)
            {
                _messageBuilder.Append($"Cannot open connection to {serverName.Text}. Ex: {ex.Message}" + Environment.NewLine);
                messageBox.Text = _messageBuilder.ToString();
                
                
            }
            try
            {
                //Create the DB to house the DB data
                SqlCommand sqlCreateTableCmd = new SqlCommand($"CREATE DATABASE {newSQLDBName.Text.Trim()}", sqlConn);
                sqlCreateTableCmd.ExecuteNonQuery();
                success = true;
            }
            catch (Exception ex)
            {
                //lbMessages.Items.Add($"ERROR! occurred creating new DB {newSQLDBName.Text}. Ex:{ex.Message}");
                _messageBuilder.Append($"ERROR! occurred creating new DB {newSQLDBName.Text}. Ex:{ex.Message}" + Environment.NewLine);
                messageBox.Text = _messageBuilder.ToString();
            }
            sqlConn.Close();
            _newConnString = $"Server ={serverName.Text}; Database = {newSQLDBName.Text}; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};";
            sqlConn.ConnectionString = _newConnString;
            //make sure we can connect to the new db
            sqlConn.Open();
            _dbConnection = sqlConn;
            sqlConn.Close();

            return success;
        }

        private bool ConnectAndCreatePostgresDB(string connString)
        {
            bool success = false;
            //connect to SQL First

            NpgsqlConnection connection = (NpgsqlConnection)GetDbConnection(connString);
                //new Npgsql.NpgsqlConnection(connString);
            _masterdbConnection = connection;
            try
            {
                connection.Open();

            }
            catch (Exception ex)
            {
                _messageBuilder.Append($"Cannot open connection to {serverName.Text}. Ex: {ex.Message}" + Environment.NewLine);
                messageBox.Text = _messageBuilder.ToString();
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
                _messageBuilder.Append($"Error occurred creating new DB {newSQLDBName.Text}. Ex:{ex.Message}" + Environment.NewLine);
                messageBox.Text = _messageBuilder.ToString();
            }
            connection.Close();
            _newConnString = $"Server ={serverName.Text}; Database = {newSQLDBName.Text}; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};Ssl Mode=Require;";
            connection.ConnectionString = _newConnString;
            //make sure we can connect to the new db
            connection.Open();
            _dbConnection = connection;
            connection.Close();

            return success;
        }

        private DataTable GetSourceDBTableList()
        {
            OleDbConnection tableConn = new OleDbConnection(_sourceConnString);
            
            tableConn.Open();
            DataTable tables = new DataTable();
            tables = tableConn.GetSchema("Tables");
            tableConn.Close();
            tableConn.Dispose();

            return tables;
            
        }

        private void CreatePostgresDestinationTable(DataTable columnsDataTable, string connString, string tableName)
        {
            
            _messageBuilder.Append($"Create table Started for Table: {tableName}" + Environment.NewLine);
            messageBox.Text = _messageBuilder.ToString();
            NpgsqlConnection conn = new NpgsqlConnection();
            conn.ConnectionString = connString;
            using (conn)
            {
                if(conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    conn = new NpgsqlConnection();
                    conn.ConnectionString = connString;
                }
                conn.Open();
                string createSql = "";
                try
                {
                    using (conn)
                    {
                        createSql = BuildPostgresCreateStatement(columnsDataTable, tableName);
                        NpgsqlCommand createCmd = conn.CreateCommand();
                        createCmd.CommandText = createSql;
                        createCmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    //lbMessages.Items.Add("Sql Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace + " for tablename: " + tableName + " sql statement " + createSql);
                    _messageBuilder.Append("Sql Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace + " for tablename: " + tableName + " sql statement " + createSql + Environment.NewLine);
                    messageBox.Text = _messageBuilder.ToString();
                }
                catch (Exception ex2)
                {
                    _messageBuilder.Append("Create Table Error:" + ex2.Message + ", Stack Trace:" + ex2.StackTrace + " for tablename: " + tableName + " sql statement " + createSql + Environment.NewLine);
                    messageBox.Text = _messageBuilder.ToString();
                }
                conn.Close();
            }
            _messageBuilder.Append($"Table {tableName} created successfully." + Environment.NewLine);
            messageBox.Text = _messageBuilder.ToString();
        }

        private void CreateSqlServerDestinationTable(DataTable dataTable, string connString, string tableName)
        {
            var conn = _dbConnection;
            //lbMessages.Items.Add("Create table Started for Table: {dataTable.TableName}");
            _messageBuilder.Append($"Create table Started for Table: {tableName}" + Environment.NewLine);
            messageBox.Text = _messageBuilder.ToString();
            if (conn.State != ConnectionState.Open)
            {
                conn.ConnectionString =_newConnString;
                conn.Open();
            }
            string createSql = "";
            try
            {
                using (_dbConnection)
                {
                    createSql = BuildSqlServerCreateStatement(dataTable, tableName);
                    SqlCommand createCmd = (SqlCommand)_dbConnection.CreateCommand();
                    createCmd.CommandText = createSql;
                    createCmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                _messageBuilder.Append("Sql Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace + " for tablename: " + tableName + " sql statement " + createSql + Environment.NewLine);
                messageBox.Text = _messageBuilder.ToString();
            }
            catch (Exception ex2)
            {
                _messageBuilder.Append("Create Table Error:" + ex2.Message + ", Stack Trace:" + ex2.StackTrace + " for tablename: " + tableName + " sql statement " + createSql + Environment.NewLine);
                messageBox.Text = _messageBuilder.ToString();
            }
            conn.Close();

            _messageBuilder.Append($"Table {tableName} created successfully." + Environment.NewLine);
            messageBox.Text = _messageBuilder.ToString();
        }

        private static string BuildSqlServerCreateStatement(DataTable dataTable, string tableName)
        {

            int i = 1;
            string sqlColumnCreate = "";
            foreach (var row in dataTable.AsEnumerable().OrderBy(x => x.ItemArray[6]))
            {
                var columnName = row[3].ToString();
                var dataTypeString = row[11].ToString();
                var maxLength = row[13].ToString();
                var precision = row[15].ToString();
                var dataType = GetSqlServerDataType(dataTypeString, maxLength, precision);

                if (_isAccess )
                {
                    columnName = columnName.Replace(" ", "_");
                    tableName = tableName.Replace(" ", "_");
                }

                string primKey = "";
                if (i == 1)
                {
                    sqlColumnCreate = "CREATE TABLE [" + tableName + "] ([" + columnName.ToString().Trim() + "] " + dataType + ", ";
                }
                else if (i == dataTable.Rows.Count)
                    sqlColumnCreate = sqlColumnCreate + " [" + columnName.ToString().Trim() + "] " + dataType + "); ";
                else
                    sqlColumnCreate = sqlColumnCreate + " [" + columnName.ToString().Trim() + "] " + dataType + ", ";
                i++;
            }
            return sqlColumnCreate;
        }
        private static string GetSqlServerDataType(string dataType, string maxLength, string precision = "0", int scale = 0)
        {
            //254 is the max length of a column on FoxPro

            switch (dataType)
            {
                case "129":
                    dataType = $"Varchar({maxLength})";
                    break;
                case "130":
                    dataType = $"NVarchar(max)";
                    break;
                case "7":
                case "133":
                case "135":
                    dataType = $"DateTime2";
                    break;

                case "20":
                    dataType = "Bigint";
                    break;
                case "128":
                    dataType = "Varbinary";
                    break;
                case "11":
                    dataType = "Bit";
                    break;
                case "8":
                    maxLength = maxLength == String.Empty ? "65535" : maxLength;

                    dataType = $"nvarchar({maxLength})";
                    break;

                    dataType = "decimal(19, 2)";
                    break;
                case "134":
                    dataType = "Timestamp";
                    break;
                case "5":
                case "6":
                case "14":
                case "131":
                    precision = precision == String.Empty ? "19" : precision;
                    dataType = $"Decimal({precision}, {scale})";
                    break;
                case "72":
                    dataType = $"UniqueIdentifier";
                    break;
                default:
                    dataType = "varchar(max)";
                    break;
            };

            return dataType;

        }

        private void InsertData(OleDbDataReader dataReader, DataTable columns, string connString, string tableName)
        {

            _messageBuilder.Append("Insert Data started for Table: " + tableName + Environment.NewLine);
            messageBox.Text = _messageBuilder.ToString();

            DbConnection conn = GetDbConnection(connString);

            StringBuilder sb = new StringBuilder();

            using (conn)
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.ConnectionString = _newConnString;
                    conn.Open();
                }
                if (_isPostgres)
                {
                    var setEncoding = "set client_encoding = 'UTF8'";
                    var command = conn.CreateCommand();
                    command.CommandText = setEncoding;
                    command.ExecuteNonQuery();
                }

                    using (var bulk = new BulkOperation(conn))
                    {
                        try
                        {
                            bulk.Connection = conn;
                            bulk.AutoMap = AutoMapType.ByName;
                            bulk.BatchSize = 10000;
                            bulk.BatchTimeout = 360;
                            bulk.Provider = _isSqlServer ? ProviderType.SqlServer : ProviderType.PostgreSql;
                            //ProviderType.PostgreSql;
                            bulk.DestinationSchemaName = _isPostgres? "public" : "dbo";
                            bulk.DestinationTableName = tableName.Replace(" ", "_");
                            bulk.CaseSensitive = CaseSensitiveType.DestinationInsensitive;
                            bulk.ColumnMappings = BuildColumnMapping(columns);
                            bulk.AutoTruncate = true;
                            bulk.DataSource = dataReader;
                            bulk.UseLogDump = true;
                            bulk.LogDump = sb;
                            bulk.UseRowsAffected = true;

                            bulk.BulkInsert();
                        _messageBuilder.Append("Rows Inserted:" + bulk.ResultInfo.RowsAffectedInserted + Environment.NewLine);
                        messageBox.Text = _messageBuilder.ToString();


                        _messageBuilder.Append("Insert Data Completed Successfully for Table: " + tableName + Environment.NewLine);
                        messageBox.Text = _messageBuilder.ToString();
                    }
                        catch (SqlException ex)
                        {
                        _messageBuilder.Append($"LogDump:{ sb.ToString()}" + Environment.NewLine);
                        _messageBuilder.Append("SQL Exception:" + ex.Message + ", Stack Trace:" + ex.StackTrace + Environment.NewLine);
                        messageBox.Text = _messageBuilder.ToString();

                    }
                        catch (Exception ex2)
                        {
                        _messageBuilder.Append($"LogDump:{ sb.ToString()}" + Environment.NewLine);
                        _messageBuilder.Append("SQL Exception:" + ex2.Message + ", Stack Trace:" + ex2.StackTrace + Environment.NewLine);
                        messageBox.Text = _messageBuilder.ToString();
                    }
                        conn.Close();
                    }
                }

                //_messageBuilder.Append($"StringBuilderLog for DataTable:{tableName} {sb.ToString()}");
                messageBox.Text = _messageBuilder.ToString();
            //dataTable.Clear();
            //dataTable.Dispose();

        }

        private static List<ColumnMapping> BuildColumnMapping(DataTable dt)
        {
            List<ColumnMapping> mappings = new List<ColumnMapping>();

            foreach(var row in dt.AsEnumerable().OrderBy(x => x.ItemArray[6]))
            {
                ColumnMapping colMap;
                var columnName = row[3].ToString();
                var newColumnName = row[3].ToString();

                if (newColumnName.Contains(" "))
                    newColumnName = newColumnName.Replace(" ", "_");

                Type dataType = GetDotNetDataType(row[11].ToString());
                var ordinal = int.Parse(row[6].ToString());
                
                if(dataType == typeof(System.String))
                {
                    colMap = new ColumnMapping();
                    colMap.SourceName = columnName.ToString();
                    colMap.DestinationName = newColumnName.ToString();
                    //colMap.FormulaInsert = $"CASE WHEN LTRIM(RTRIM(REPLACE(REPLACE(StagingTable.{columnName},  char(0), ''), 0x00, ''))) = '' THEN NULL ELSE LTRIM(RTRIM(REPLACE(REPLACE(StagingTable.{columnName},  char(0), ''), 0x00, '')) END";
                    colMap.SourceValueFactory = x => {
                        var r = (IDataReader)x;
                        var dtable = dataType;
                        var stringValue = r.GetString(r.GetOrdinal($"{columnName}"));
                        var value = stringValue.Replace(Convert.ToChar(0x00).ToString(), string.Empty).Replace('\u0000'.ToString(), "");
                        //var bytes = Encoding.UTF8.GetBytes(stringValue).Where(b => b != 0).ToArray();
                        //var value = Encoding.ASCII.GetString(bytes).TrimEnd();
                        return value;
                    };

                }
                else if(dataType == typeof(System.Decimal))
                {
                    colMap = new ColumnMapping();
                    colMap.SourceName = columnName.ToString();
                    colMap.DestinationName = newColumnName.ToString();
                    colMap.DefaultValue = 0;
                }
                else
                {

                    colMap = new ColumnMapping();
                    colMap.SourceName = columnName.ToString();
                    colMap.DestinationName = newColumnName.ToString();
                    colMap.DefaultValueResolution = DefaultValueResolutionType.Null;

                }
                mappings.Add(colMap);
            }
            return mappings;
        }

        private static string BuildPostgresCreateStatement(DataTable columnsDataTable, string tableName)
        {

            int i = 1;
            string sqlColumnCreate = "";
            foreach (var row in columnsDataTable.AsEnumerable().OrderBy(x => x.ItemArray[6]))
            {
                
                var columnName = row[3].ToString();
                var dataTypeString = row[11].ToString();
                var maxLength = row[13].ToString();
                var precision = row[15].ToString();
                var scaleString = row[16].ToString();
                int.TryParse(scaleString, out int scale);
                var dataType = GetDataType(dataTypeString,maxLength, precision, scale);

                if (columnName.Contains(" "))
                    columnName = columnName.Replace(" ", "_");
                if(_isAccess)
                {
                    columnName = columnName.Replace(" ", "_").ToLower();
                    tableName = tableName.Replace(" ", "_").ToLower();
                }

                string primKey = "";
                if (i == 1 && columnsDataTable.Rows.Count > 1)
                {
                        primKey = " PRIMARY KEY";
                        sqlColumnCreate = $"CREATE TABLE  \"{tableName.ToLower()}\" ( \"{columnName.ToLower().ToString().Trim()}\"   { dataType}, ";
                }
                else if (i == 1 && columnsDataTable.Rows.Count == 1)
                {
                    sqlColumnCreate = $"CREATE TABLE \"{tableName.ToLower()}\" ( \"{columnName.ToLower().ToString().Trim()}\"   { dataType}); ";

                }
                else if (i == columnsDataTable.Rows.Count)
                    sqlColumnCreate = sqlColumnCreate + $" \"{columnName.ToLower().ToString().Trim()}\" {dataType}); ";
                else
                    sqlColumnCreate = sqlColumnCreate + $" \"{columnName.ToLower().ToString().Trim()}\" {dataType}, ";
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
                var columnName = row[3].ToString();
                if (!_isFoxPro)
                    columnName = '[' + columnName.ToString() + ']'; 
                var dataTypeString = row[11].ToString();
                var maxLength = row[13].ToString();
                var precision = row[15].ToString();
                var scaleString = row[16].ToString();
                int.TryParse(scaleString, out int scale);
                //var dataType = _isDbase ? GetDBaseDataType(dataTypeString, maxLength, precision, scale) :
                //    GetFoxProDataType(dataTypeString, maxLength, precision, scale);

                if (i == 1 && dataTable.Rows.Count > 1)
                {
                    sqlSelect = $"Select {columnName.ToString().Trim()} , ";
                }
                else if (i == 1 && dataTable.Rows.Count == 1)
                {
                    sqlSelect = $"Select {columnName.ToString().Trim()} FROM {tableName.ToString()} ";
                }
                else if (i == dataTable.Rows.Count)
                    sqlSelect = sqlSelect + $" {columnName.ToString().Trim()}  FROM {tableName.ToString()} ";
                else
                    sqlSelect = sqlSelect + $" {columnName.ToString().Trim()}, ";
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
                case "134":
                    dataType = $"Char({maxLength})";
                    break;
                case "130":
                    dataType = "MEMO";
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

        private void dbaseButton1_CheckedChanged(object sender, EventArgs e)
        {
            _isAccess = accessButton.Checked;
            _isDbase = dbaseButton1.Checked;
            _isFoxPro = foxProButton1.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void accessButton_CheckedChanged(object sender, EventArgs e)
        {
            _isAccess = accessButton.Checked;
            _isDbase = dbaseButton1.Checked;
            _isFoxPro = foxProButton1.Checked;
        }

        private void foxProButton1_CheckedChanged(object sender, EventArgs e)
        {
            _isAccess = accessButton.Checked;
            _isDbase = dbaseButton1.Checked;
            _isFoxPro = foxProButton1.Checked;
        }
    }
}
