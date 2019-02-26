using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BleCalibrate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    
    public partial class App : Application
    {
        private static DBConnect db;
        /*
        Use this code in conjuntion with the following entry on App.xaml
        DispatcherUnhandledException="Application_DispatcherUnhandledException"
        Example App.xaml at the end of this code          
        */
        // EXCEPTION HANDLING for uncaught exceptions in the build 
        public bool DoHandle { get; set; }
        private void Application_DispatcherUnhandledException(object sender,
                               System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (this.DoHandle)
            {
                //Handling the exception within the UnhandledException handler.
                //MessageBox.Show(e.Exception.Message, "Exception Caught",  MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            else
            {
                //If you do not set e.Handled to true, the application will close due to crash.
               // MessageBox.Show("Application is going to close! " + e.Exception.Message, "Uncaught Exception");
                //MessageBox.Show("Application is going to close! " + e.Exception.StackTrace, "Uncaught Exception");
                e.Handled = false;

                // Get Sensor ID 
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
                string sensorID = "NA";
                string line = ""; 
                try
                {

                    System.IO.StreamReader file = new System.IO.StreamReader(path + "sensorID");
                    Console.Write("sensorID:");
                    line = file.ReadLine();
                    Console.WriteLine(line);
                    // assign sensorID to the vars
                    sensorID = line.Trim();
                    file.Close();
                }
                catch (Exception ex) { }


                // Logging into CRASH LOGS
                path = "";
                try
                {
                    path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log\\" + DateTime.Today.ToString("yyyyMMdd") + "Crash.log";
                    if (!File.Exists(path))
                    {
                        using (var tw = new StreamWriter(path, true))
                        {
                            tw.WriteLine(DateTime.Now+ " Logging Started");
                            tw.WriteLine("-----------------------------------------------------------------");
                            tw.Close();
                        }
                    }
                    File.AppendAllText(path, DateTime.Now + Environment.NewLine + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace + Environment.NewLine + " Application Exiting " + Environment.NewLine
                        + "-----------------------------------------------------------------\n");

                    // Post Exception to Database
                    db = new DBConnect();
                    db.PostException("[" + DateTime.Now  + "] [" + sensorID + "] ERROR ::"+ e.Exception.Message + ":" + e.Exception.StackTrace, "FoupBLETracker" , "Unhandled");
                    // END Post Exception to Database
                }
                catch (Exception ex) {
                    // Post Exception to Database
                    db = new DBConnect();
                    db.PostException("[" + DateTime.Now + "][" + sensorID + "] ERROR: " + ex.ToString(), "FoupBLETracker", "E2@App.xaml");
                    // END Post Exception to Database
                }


                Environment.Exit(1);
            }
        }
        // END EXCEPTION HANDLING for uncaught exceptions in the build 

    }
}


/**** Example App.xaml  ****

 <Application x:Class="test_WpfApplication1.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:test_WpfApplication1"
             StartupUri="MainWindow.xaml"
             DispatcherUnhandledException="Application_DispatcherUnhandledException">
    <Application.Resources>
         
    </Application.Resources>
</Application>

 */
