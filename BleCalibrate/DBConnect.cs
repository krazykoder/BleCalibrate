using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Data;
using System.Windows;

namespace BleCalibrate
{
    class DBConnect
    {
        private SqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            string connectionString;
            //<add name="WaferTrackConnectionString2" connectionString="Data Source=ca1blwssdev01;Initial Catalog=WINAppsWaferTrack;User Id=winapps;Password=winapps123;"/>

            connectionString = @"Data Source=CA1CORPSQL02\PCORP;Initial Catalog=WINAppsFoupTrack;User Id=winapps;Password=winapps123;";

            connection = new SqlConnection(connectionString);
            //OpenConnection();            
            //CloseConnection();
            //TestConnection();
        }

        public bool TestConnection()
        {
            //string query = "SELECT top 10 * from FoupTrack;";
            //GenericSelect(query);
            bool open = false; ;
            bool close = false;
            try
            {
                open = OpenConnection();
                close = CloseConnection();
            }
            catch (Exception e) { }

            if (open && close) return true; else return false;

        }

        public string getFoupInfo(string id)
        {
            string returnstring = "0";
            SqlDataReader dataReader;
            SqlCommand command;
            string query = "select top 1 FoupName from FoupDB where FoupID = @foupID";
            try
            {
                if (this.OpenConnection() == true)
                {
                    command = new SqlCommand(query, connection);
                    command.Parameters.Add("@foupID", id);
                    dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        //MessageBox.Show(dataReader.GetString(dataReader.GetOrdinal("FoupName")));
                        returnstring = dataReader.GetString(dataReader.GetOrdinal("FoupName"));
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Query=" + query + "\n  " + e.ToString());
                PostException("Query=" + query + "\n  " + e.ToString(), "", "");
            }

            CloseConnection();
            return returnstring;
        }


        public string getUserInfo(string id)
        {
            string returnstring = "0";
            SqlDataReader dataReader;
            SqlCommand command;
            string query = "select top 1 UserName from UserDB where UserID = @userID AND validated = 1";
            try
            {
                if (this.OpenConnection() == true)
                {
                    command = new SqlCommand(query, connection);
                    command.Parameters.Add("@userID", id);
                    dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        //MessageBox.Show(dataReader.GetString(dataReader.GetOrdinal("UserName")));
                        returnstring = dataReader.GetString(dataReader.GetOrdinal("UserName"));
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Query=" + query + "\n  " + ex.ToString());
                PostException("Query=" + query + "\n  " + ex.ToString(), "", "");
            }
            CloseConnection();
            return returnstring;
        }


        public string CheckLastLocation(string tablename, string queryColumn, string foupID)
        {
            string returnstring = "None";
            SqlDataReader dataReader;
            SqlCommand command;
            string query = "select top 1 " + queryColumn + " from " + tablename + " where foupID=" + foupID + " order by datetimestamp desc";
            try
            {
                if (this.OpenConnection() == true)
                {
                    command = new SqlCommand(query, connection);
                    dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        //MessageBox.Show(dataReader.GetString(dataReader.GetOrdinal("UserName")));
                        returnstring = dataReader.GetString(dataReader.GetOrdinal(queryColumn));
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Query=" + query + "\n  " + ex.ToString());
                PostException("Query=" + query + "\n  " + ex.ToString(), "", "");
            }
            CloseConnection();
            return returnstring;
        }

        public string getSingleStringDatafromDatabase(string tablename, string queryColumn, string searchColumn, string searchStringValue, bool isorderby, string orderby, string order)
        {
            string returnstring = "None";
            if (queryColumn == "" || tablename == "" || searchColumn == "" || searchStringValue == "") return returnstring;
            SqlDataReader dataReader;
            SqlCommand command;
            string query = "";
            if (!isorderby) query = "select top 1 " + queryColumn + " from " + tablename + " where " + searchColumn + " =" + searchStringValue;
            else query = query = "select top 1 " + queryColumn + " from " + tablename + " where " + searchColumn + " =" + searchStringValue + " order by " + orderby + " " + order;
            try
            {
                if (this.OpenConnection() == true)
                {
                    command = new SqlCommand(query, connection);
                    dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        //MessageBox.Show(dataReader.GetString(dataReader.GetOrdinal("UserName")));
                        returnstring = dataReader.GetString(dataReader.GetOrdinal(queryColumn));
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Query=" + query + "\n  " + ex.ToString());
                PostException("Query=" + query + "\n  " + ex.ToString(), "", "");
            }
            CloseConnection();
            return returnstring;
        }
        public string getGUID(string tablename, string queryColumn, string searchColumn, int searchStringValue, bool isorderby, string orderby, string order)
        {
            string returnstring = "None";
            SqlDataReader dataReader;
            SqlCommand command;
            string query = "";
            if (!isorderby) query = "select top 1 " + queryColumn + " from " + tablename + " where " + searchColumn + " =" + searchStringValue;
            else query = query = "select top 1 " + queryColumn + " from " + tablename + " where " + searchColumn + " =" + searchStringValue + " order by " + orderby + " " + order;
            try
            {
                if (this.OpenConnection() == true)
                {
                    command = new SqlCommand(query, connection);
                    dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        //MessageBox.Show(dataReader.GetString(dataReader.GetOrdinal("UserName")));
                        Guid n = dataReader.GetGuid(dataReader.GetOrdinal(queryColumn));
                        returnstring = n.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Query=" + query + "\n  " + ex.ToString());
                PostException("Query=" + query + "\n  " + ex.ToString(), "", "");
            }
            CloseConnection();
            return returnstring;
        }
        public string checkUser(string tablename, string queryColumn, string searchColumn, string searchStringValue)
        {
            string returnstring = "None";
            SqlDataReader dataReader;
            SqlCommand command;
            string query = "";
            query = query = "select top 1 " + queryColumn + " from " + tablename + " where " + searchColumn + " =" + searchStringValue;
            try
            {
                if (this.OpenConnection() == true)
                {
                    command = new SqlCommand(query, connection);
                    dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        //MessageBox.Show(dataReader.GetString(dataReader.GetOrdinal("UserName")));
                        returnstring = dataReader.GetString(dataReader.GetOrdinal(queryColumn));
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Query=" + query + "\n  " + ex.ToString());
                PostException("Query=" + query + "\n  " + ex.ToString(), "", "");
            }
            CloseConnection();
            return returnstring;
        }

        public int CheckLastStatus(string tablename, string queryColumn, string foupID)
        {
            int returnstring = 2;
            SqlDataReader dataReader;
            SqlCommand command;
            string query = "select top 1 " + queryColumn + " from " + tablename + " where foupID=" + foupID + " order by datetimestamp desc";
            try
            {
                if (this.OpenConnection() == true)
                {
                    command = new SqlCommand(query, connection);
                    dataReader = command.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        //MessageBox.Show(dataReader.GetString(dataReader.GetOrdinal("UserName")));
                        returnstring = dataReader.GetInt32(dataReader.GetOrdinal(queryColumn));
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Query=" + query + "\n  " + ex.ToString());
                PostException("Query=" + query + "\n  " + ex.ToString(), "", "");
            }
            CloseConnection();
            return returnstring;
        }
        public bool Checkin(string foupID, string foupname, string userID, string username, string cleanroom, int inStatus)
        {
            return Checkin(foupID, foupname, userID, username, cleanroom, inStatus, "");
        }

        public bool Checkin(string foupID, string foupname, string userID, string username, string cleanroom, int inStatus, string messages)
        {
            try
            {
                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor

                    // Update the History Table 
                    string query = "Insert into FoupHistory (Datetimestamp, foup, foupid, incleanroom, intime, inuser, inuserID, inStatus, messages ) values (CURRENT_TIMESTAMP, @foupname,  @foupID , @cleanroom , CURRENT_TIMESTAMP,  @username , @userID, @inStatus, @messages );";
                    SqlCommand cmd = new SqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@foupname", foupname);
                    cmd.Parameters.AddWithValue("@foupID", foupID);
                    cmd.Parameters.AddWithValue("@cleanroom", cleanroom);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@inStatus", inStatus);
                    cmd.Parameters.AddWithValue("@messages", messages);

                    cmd.ExecuteNonQuery();
                    /*
                                        // checkin into Status Table 
                                        query = "update CheckInTable set Datetimestamp=CURRENT_TIMESTAMP , incleanroom = @cleanroom, intime=CURRENT_TIMESTAMP, inuser=@username, inuserID=@userID, Status=@inStatus  where foupID=@foupID";
                                        cmd = new SqlCommand(query, connection);
                                        //@foupname,  @foupID , @cleanroom , CURRENT_TIMESTAMP, , @username , @userID );";
                    
                                        cmd.Parameters.AddWithValue("@foupID", foupID);
                                        cmd.Parameters.AddWithValue("@cleanroom", cleanroom);
                                        cmd.Parameters.AddWithValue("@username", username);
                                        cmd.Parameters.AddWithValue("@userID", userID);
                                        cmd.Parameters.AddWithValue("@inStatus", inStatus);
                    */
                    // checkin into Status Table 
                    query = "update CheckInTable set Datetimestamp=CURRENT_TIMESTAMP , incleanroom = @cleanroom, intime=CURRENT_TIMESTAMP, inuser=@username, inuserID=@userID, Status=@inStatus, outtime=@outtime, outuser=@outuser, outuserID=@outuserID, messages=@messages    where foupID=@foupID";
                    cmd = new SqlCommand(query, connection);
                    //@foupname,  @foupID , @cleanroom , CURRENT_TIMESTAMP, , @username , @userID );";

                    cmd.Parameters.AddWithValue("@foupID", foupID);
                    cmd.Parameters.AddWithValue("@cleanroom", cleanroom);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@inStatus", inStatus);

                    cmd.Parameters.AddWithValue("@outtime", DBNull.Value);
                    cmd.Parameters.AddWithValue("@outuser", DBNull.Value);
                    cmd.Parameters.AddWithValue("@outuserID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@messages", messages); // updates current checkin table with errors

                    cmd.ExecuteNonQuery();

                    CloseConnection();
                    return true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Checkin Exception: \n" + ex.ToString());
                PostException("Checkin Exception: \n" + ex.ToString(), "", "");
                CloseConnection();
                return false;
            }
            CloseConnection();
            return false;
        }


        public bool Checkout(string foupID, string foupname, string userID, string username, string cleanroom, int outStatus, int rowID, string messages)
        {
            try
            {
                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    string query = "";
                    if (messages == "OK")
                        query = "UPDATE FoupHistory SET datetimestamp=CURRENT_TIMESTAMP, outtime =CURRENT_TIMESTAMP, outuser =@username, outuserID=@userID, outStatus=@outStatus where ID= @rowID";
                    else query = "UPDATE FoupHistory SET datetimestamp=CURRENT_TIMESTAMP,outtime =CURRENT_TIMESTAMP, outStatus=@outStatus, messages=@messages where ID= @rowID";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    //@foupname,  @foupID , @cleanroom , CURRENT_TIMESTAMP, , @username , @userID );";
                    //cmd.Parameters.AddWithValue("@foupname", foupname);
                    cmd.Parameters.AddWithValue("@foupID", foupID);
                    //cmd.Parameters.AddWithValue("@cleanroom", cleanroom);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@outStatus", outStatus);
                    cmd.Parameters.AddWithValue("@rowID", rowID);
                    cmd.Parameters.AddWithValue("@messages", messages);
                    cmd.ExecuteNonQuery();


                    // checkout info into Status Table 
                    if (messages == "OK")
                        query = "update CheckInTable set Datetimestamp=CURRENT_TIMESTAMP , outtime=CURRENT_TIMESTAMP, outuser=@username, outuserID=@userID, Status=@inStatus  where foupID=@foupID";
                    else query = "update CheckInTable set Datetimestamp=CURRENT_TIMESTAMP , outtime=CURRENT_TIMESTAMP,  Status=@inStatus, messages=@messages  where foupID=@foupID";
                    cmd = new SqlCommand(query, connection);
                    //@foupname,  @foupID , @cleanroom , CURRENT_TIMESTAMP, , @username , @userID );";

                    cmd.Parameters.AddWithValue("@foupID", foupID);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@inStatus", 0); // Status table to show checkout when the foup is not checked into another location
                    cmd.Parameters.AddWithValue("@messages", messages); // Error logging

                    cmd.ExecuteNonQuery();
                    CloseConnection();
                    return true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Checkin Exception: \n" + ex.ToString());
                PostException("Checkin Exception: \n" + ex.ToString(), "", "");
                CloseConnection();
                return false;
            }
            CloseConnection();
            return false;
        }

        public bool UpdateErrorTable(string foup, string foupID, string userID1, string userID2, string messages, int errorCode, int RowID)
        {
            try
            {
                string username1 = getSingleStringDatafromDatabase("UserDB", "username", "userID", userID1, false, "", "");
                string username2 = getSingleStringDatafromDatabase("UserDB", "username", "userID", userID2, false, "", "");
                //MessageBox.Show(username1 + "|" + username2);
                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    string query = "Insert into ErrorTable (Datetimestamp, foup, foupid, lastuserID1, lastusername1, lastuserID2, lastusername2, ErrorMessage, errorCode, RowID ) output INSERTED.ID  values (CURRENT_TIMESTAMP, @foup, @foupID ,  @userID1, @userName1,  @userID2, @userName2, @messages, @errorCode, @RowID )";

                    SqlCommand cmd = new SqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@foup", foup);
                    cmd.Parameters.AddWithValue("@foupID", foupID);
                    cmd.Parameters.AddWithValue("@userID1", userID1);
                    cmd.Parameters.AddWithValue("@userID2", userID2);
                    cmd.Parameters.AddWithValue("@messages", messages);
                    cmd.Parameters.AddWithValue("@errorCode", errorCode);
                    cmd.Parameters.AddWithValue("@RowID", RowID);
                    cmd.Parameters.AddWithValue("@userName1", username1);
                    cmd.Parameters.AddWithValue("@userName2", username2);

                    Int32 id = (Int32)cmd.ExecuteScalar();
                    CloseConnection();


                    string newlyinsertedID = getGUID("ErrorTable", "UID", "ID", id, false, "", "");// = cmd.Parameters["@guidid"].Value;
                    //MessageBox.Show("GUID " + newlyinsertedID);

                    OpenConnection();
                    query = "Insert into ErrorTableHistory (UID, Datetimestamp, foup, foupid, lastuserID1, lastusername1, lastuserID2, lastusername2, ErrorMessage, errorCode, RowID ) values (@lastID, CURRENT_TIMESTAMP, @foup, @foupID,  @userID1, @userName1,  @userID2, @userName2, @messages, @errorCode, @RowID );";

                    cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@lastID", newlyinsertedID);
                    cmd.Parameters.AddWithValue("@foup", foup);
                    cmd.Parameters.AddWithValue("@foupID", foupID);
                    cmd.Parameters.AddWithValue("@userID1", userID1);
                    cmd.Parameters.AddWithValue("@userID2", userID2);
                    cmd.Parameters.AddWithValue("@messages", messages);
                    cmd.Parameters.AddWithValue("@errorCode", errorCode);
                    cmd.Parameters.AddWithValue("@RowID", RowID);
                    cmd.Parameters.AddWithValue("@userName1", username1);
                    cmd.Parameters.AddWithValue("@userName2", username2);

                    cmd.ExecuteNonQuery();
                    CloseConnection();

                    checkErrorDatabase();
                    return true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Checkin Exception: \n" + ex.ToString());
                PostException("Checkin Exception: \n" + ex.ToString(), "", "");
                CloseConnection();
                return false;
            }
            CloseConnection();
            return false;
        }

        public bool registerUser(string userID, string userName, string userEmail)
        {
            try
            {
                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor

                    string query = "Insert into userDB (userID, userName, userEmail ) values (@userID,  @username, @userEmail );";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    //@foupname,  @foupID , @cleanroom , CURRENT_TIMESTAMP, , @username , @userID );";

                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@userName", userName);
                    cmd.Parameters.AddWithValue("@userEmail", userEmail);

                    cmd.ExecuteNonQuery();

                    CloseConnection();
                    return true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("UserAdd Exception: \n" + ex.ToString());
                PostException("UserAdd Exception: \n" + ex.ToString(), "", "");
                CloseConnection();
                return false;
            }
            CloseConnection();
            return false;
        }







        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                //Console.WriteLine("Connection Successfull.");
                return true;
            }
            catch (SqlException ex)
            {
                //When handling errors, you can your application's Console based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                //MessageBox.Show("Error Code: " + ex.Number + "\n" + ex.ToString());
                PostException("Exception: \n" + ex.ToString(), "", "");
                //Console.WriteLine(ex.ToString());
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
                //Console.WriteLine("Connection Close Successfull.");
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        //Insert statement
        public bool Insert(string query)
        {
            //string query = "INSERT INTO tableinfo (name, age) VALUES('John Smith', '33')";
            try
            {
                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    SqlCommand cmd = new SqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery();
                    //close connection
                    this.CloseConnection();
                    return true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Query=" + query + "\n  " + ex.ToString());
                PostException("Query=" + query + "\n  " + ex.ToString(), "", "");
                CloseConnection();
                return false;
            }
            CloseConnection();
            return false;
        }

        //Update statement
        public void Update(string query)
        {
            //string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                SqlCommand cmd = new SqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete(string query)
        {
            //string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /* Select statement        
         * NEEDS WORK - MAKE IT GENERIC
        */
        public List<string>[] Select(string query)
        {
            //string query = "SELECT * FROM tableinfo";

            //Create a list to store the result
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            //Open connection
            if (this.OpenConnection() == true)
            {

                //Create Command
                SqlCommand cmd = new SqlCommand(query, connection);
                //Create a data reader and Execute the command
                SqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    list[0].Add(dataReader["id"] + "");
                    list[1].Add(dataReader["name"] + "");
                    list[2].Add(dataReader["age"] + "");
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }

        }


        public void GenericSelect(string query)
        {
            /* Display Data in the table for the query*/
            SqlDataReader dataReader;
            SqlCommand command;

            if (this.OpenConnection() == true)
            {
                //read data from the table to our data reader

                command = new SqlCommand(query, connection);
                dataReader = command.ExecuteReader();
                //loop through each row we have read
                Object[] values = new Object[dataReader.FieldCount];
                dataReader.Read();
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    Console.Write(dataReader.GetName(i).ToString().Trim() + ", "); ;
                }

                while (dataReader.Read())
                {
                    Console.Write("\n");

                    int fieldCount = dataReader.GetValues(values);
                    //Console.WriteLine("reader.GetValues retrieved {0} columns.", fieldCount);
                    for (int i = 0; i < fieldCount; i++)
                    {
                        Console.Write(values[i].ToString().Trim() + ", ");

                    }
                    dataReader.Close();


                }
            }
            connection.Close();
        }

        //Count statement
        public int Count(string query)
        {
            //string query = "SELECT Count(*) FROM tableinfo";
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                SqlCommand cmd = new SqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }

        }

        //Backup
        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                //Save file to C:\ with the current date as a filename
                string path;
                path = "C:\\MySqlBackup" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output;
                output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to backup!");
            }
        }

        //Restore
        public void Restore()
        {
            try
            {
                //Read file from C:\
                string path;
                path = "C:\\MySqlBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;


                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to Restore!");
            }
        }

        public void checkErrorDatabase()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://winappsweb/fouptrack/checkError.aspx");
            httpWebRequest.ContentType = "text";
            httpWebRequest.Method = "GET";


            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                PostException(e.ToString(), "", "");

            }
        }

        
        public void PostException(string exception, string origin, string source)
        {
            DBConnect exdb = new DBConnect();
            try
            {
                //open connection
                if (exdb.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor

                    string query = "Insert into exceptiontable (exception, origin, source ) values (@exception,  @origin, @source );";
                    SqlCommand cmd = new SqlCommand(query, exdb.connection);
                    //@foupname,  @foupID , @cleanroom , CURRENT_TIMESTAMP, , @username , @userID );";

                    cmd.Parameters.AddWithValue("@exception", exception);
                    cmd.Parameters.AddWithValue("@origin", origin);
                    cmd.Parameters.AddWithValue("@source", source);

                    cmd.ExecuteNonQuery();
                    exdb.CloseConnection();
                }
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.ToString());
                //MessageBox.Show("POST ERROR FOR ::: " + source+ " "+exception);
                exdb.CloseConnection();
            }
            
            return;
        }

        public async void PostToMailingList(string fid, string fn, string message)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                   { "fid",fid },
                   { "fn", fn },
                   { "message", message }
                };

                var content = new FormUrlEncodedContent(values);
                try
                {
                    var response = await client.PostAsync("http://winappsweb/fouptrack/processFoupMailingList.aspx", content);

                    var responseString = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString());
                    PostException(e.ToString(), "", "");
                }
            }
        }

        public void sendRegistrationEmail()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://winappsweb/fouptrack/validateUsers.aspx");
            httpWebRequest.ContentType = "text";
            httpWebRequest.Method = "GET";


            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                PostException(e.ToString(), "", "");

            }
        }
    }
}
