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

namespace LoadFoxProDBToSQL
{
    public partial class Form1 : Form
    {
        string _path;
        SqlConnection _sqlConnection;
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
            lbMessages.Items.Add(@"Processing started at {DateTime.Now}");
            var masterConnString = $"Server = {sqlServerName.Text}; Database = master; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};";
            _masterConnString = masterConnString;
            _newConnString = $"Server ={sqlServerName.Text}; Database = {newSQLDBName.Text}; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};";
            bool sqlSuccess = ConnectAndCreateSQLDB(_masterConnString);

            var tableList = ConnectToDBFGetTableList();




        }

        private bool ConnectAndCreateSQLDB(string connString)
        {
            bool success = false; 
            //connect to SQL First
            SqlConnection sqlConn = new SqlConnection(connString);
            _sqlConnection = sqlConn;
            try
            {
                sqlConn.Open();

            }
            catch (Exception ex)
            {
                lbMessages.Items.Add($"Cannot open connection to {sqlServerName.Text}. Ex: {ex.Message}");
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
                lbMessages.Items.Add($"Error occurred creating new DB {newSQLDBName.Text}. Ex:{ex.Message}");

            }
            sqlConn.Close();
            _newConnString = $"Server ={sqlServerName.Text}; Database = {newSQLDBName.Text}; User Id ={sqlUserName.Text}; Password ={sqlPassword.Text};";
            sqlConn.ConnectionString = _newConnString;
            //make sure we can connect to the new db
            sqlConn.Open();
            sqlConn.Close();

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

                    CreateSqlServerTable(columns, _newConnString, tableName);
                    InsertSqlServerData(tableData, _newConnString);
                }
                catch(Exception ex)
                {
                    var tableName = t[2].ToString();
                    lbMessages.Items.Add($"Error occurred on table {tableName}. Exception: {ex.Message}");
                }
            }

            return tables;
        }

        private void CreateSqlServerTable(DataTable dataTable, string connString, string tableName)
        {
            lbMessages.Items.Add("Create table Started for Table: {dataTable.TableName}");
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connString;
            conn.Open();
            string createSql = "";
            try
            {
                using (conn)
                {
                    createSql = BuildCreateStatement(dataTable, tableName);
                    SqlCommand createCmd = conn.CreateCommand();
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
        private void InsertSqlServerData(DataTable dataTable, string connString)
        {
            lbMessages.Items.Add("Insert Data started for Table: " + dataTable.TableName);

            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            using (conn)
            {
                using (dataTable)
                {
                    string sqlSelect = String.Format("SELECT * FROM {0}", dataTable);

                    SqlBulkCopy bulkCopy = new SqlBulkCopy(conn);
                    using (bulkCopy)
                    {
                        try
                        {

                            if (conn.State == ConnectionState.Closed)
                                conn.Open();
                            bulkCopy.DestinationTableName = dataTable.TableName;
                            bulkCopy.BatchSize = 1000;
                            bulkCopy.BulkCopyTimeout = 360;
                            bulkCopy.WriteToServer(dataTable);

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
                        sqlColumnCreate = "CREATE TABLE " + tableName + " ( srcId INT PRIMARY KEY IDENTITY (1, 1), [" + columName.ToString().Trim() + "] " + dataType + ", ";
                }
                else if (i == dataTable.Rows.Count)
                    sqlColumnCreate = sqlColumnCreate + " [" + columName.ToString().Trim() + "] " + dataType + "); ";
                else
                    sqlColumnCreate = sqlColumnCreate + " [" + columName.ToString().Trim() + "] " + dataType + ", ";
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
                var dataType = GetDataType(dataTypeString, maxLength, precision);

                string primKey = "";
                if (i == 1)
                {
                    sqlSelect = $"Select  cast(null as integer) as srcId, Cast( {columName.ToString().Trim()} as {dataType.ToString()}) as {columName.ToString()} , ";
                }
                else if (i == dataTable.Rows.Count)
                    sqlSelect = sqlSelect + $" Cast({columName.ToString().Trim()} as {dataType}) as {columName.ToString()} FROM {tableName.ToString()}";
                else
                    sqlSelect = sqlSelect + $" Cast({columName.ToString().Trim()} as {dataType} ) as {columName.ToString()}, ";
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
                    dataType = $"Varchar(250)";
                    break;
                case "133":
                    //dataType = "Date";
                    dataType = $"Varchar(254)";
                    break;
                case "20":
                    dataType = "Integer";
                    break;
                case "128":
                    dataType = "Varbinary";
                    break;
                case "11":
                    dataType = "char(1)";
                    break;
                case "8":
                    dataType = $"nvarchar(254)";
                    break;
                case "6":
                    dataType = "decimal(19, 2)";
                    break;
                case "7":
                    dataType = "Datetime";
                    break;
                case "134":
                    dataType = "time";
                    break;
                case "135":
                    dataType = "DateTime";
                    break;
                case "14":
                    dataType = "Decimal(19, 4)";
                    break;
                case "5":
                    dataType = "Decimal(19,2)";
                    break;
                case "72":
                    dataType = $"varchar(254)";
                    break;
                default:
                    dataType = "varchar(254)";
                    break;
            };

            return dataType;

        }
    }
}
