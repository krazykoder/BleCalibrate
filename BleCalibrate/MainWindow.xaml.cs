using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq; 
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

using Newtonsoft.Json;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;

using LiveCharts;
using LiveCharts.Defaults;
using System.Windows.Controls;

namespace BleCalibrate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            thisTimer();
            Initialize();

            this.Beacons = new Dictionary<string, beacon>();
            SignalScatterChart();

        }



        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("START Async", "$");
                // no task 

                Log("Hello This is Towshif!", "$");

            }

            catch (Exception ex)
            {
                if ((bool)verboseCheckBox.IsChecked) MessageBox.Show(ex.ToString());
                this.Dispatcher.Invoke(() => { Log(ex.ToString(), "error"); Log(DateTime.Now + ex.ToString(), "logfile"); });

                // Post Exception to Database
                db = new DBConnect();
                db.PostException("[" + DateTime.Now + "] ERROR: " + ex.ToString(), "FoupBLETracker" + sensorID + " v." + version, "MainWindow");
                // END Post Exception to Database
            }
        }
        private void Log(string text, string ID)
        {
            switch (ID)
            {
                default:
                case "log":
                    if ((bool)checkBox1.IsChecked)
                    {
                        richTextBox.AppendText(text + "\n");
                        richTextBox.ScrollToEnd();
                    }
                    break;
                case "$":
                    if ((bool)checkBox2.IsChecked)
                    {
                        jSONTextBox.AppendText("$>" + text + "\n");
                        jSONTextBox.ScrollToEnd();
                    }
                    break;
                case "JSON":
                    if ((bool)checkBox2.IsChecked)
                    {
                        jSONTextBox.AppendText("JSON:" + text + "\n");
                        jSONTextBox.ScrollToEnd();
                    }
                    break;
                case "error":
                    jSONTextBox.AppendText("Error:" + text + "\n");
                    jSONTextBox.ScrollToEnd();
                    break;
                case "logfile":
                    // write to log file with date / time stamp
                    string path = "";

                    try
                    {
                        path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log\\" + DateTime.Today.ToString("yyyyMMdd") + "Error.log";
                        if (!File.Exists(path))
                        {
                            using (var tw = new StreamWriter(path, true))
                            {
                                tw.WriteLine("Logging Started");
                                tw.WriteLine("-----------------------------------------------------------------");
                                tw.Close();
                            }
                        }
                        File.AppendAllText(path, text + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        jSONTextBox.AppendText("Erroriin path:" + path + "\n");
                        jSONTextBox.AppendText("Error:" + ex.ToString() + "\n");
                        jSONTextBox.ScrollToEnd();

                        // Post Exception to Database
                        db = new DBConnect();
                        db.PostException("[" + DateTime.Now + "] ERROR: " + ex.ToString(), "FoupBLETracker" + sensorID + " v." + version, "MainWindow");
                        // END Post Exception to Database
                    }

                    // write to <Datetime_file>.log file in the bin (same) folder ./
                    break;
            }


            //richTextBox.ScrollToEnd();
        }

        private async void button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("START Async", "$");
                // no task 

                Log("Hello This is Towshif!", "$");
                /*
                var locator = new Windows.Devices.Geolocation.Geolocator();
                var location = await locator.GetGeopositionAsync();
                var position = location.Coordinate.Point.Position;
                var latlong = string.Format("lat:{0}, long:{1}", position.Latitude, position.Longitude);
                var result = MessageBox.Show(latlong);
                */
            }
            catch (Exception ex)
            {
                if ((bool)verboseCheckBox.IsChecked) MessageBox.Show(ex.ToString());
                this.Dispatcher.Invoke(() => { Log(ex.ToString(), "error"); Log(DateTime.Now + ex.ToString(), "logfile"); });
                // Post Exception to Database
                db = new DBConnect();
                db.PostException("[" + DateTime.Now + "] ERROR: " + ex.ToString(), "FoupBLETracker" + sensorID + " v." + version, "MainWindow");
                // END Post Exception to Database
            }


        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)verboseCheckBox.IsChecked) MessageBox.Show("This is Towshif");

            try
            {
                StartBleDeviceWatcher();
            }
            catch (Exception ex)
            {
                if ((bool)verboseCheckBox.IsChecked) MessageBox.Show(ex.ToString());
                this.Dispatcher.Invoke(() => { Log(ex.ToString(), "error"); Log(DateTime.Now + ex.ToString(), "logfile"); });

                // Post Exception to Database
                db = new DBConnect();
                db.PostException("[" + DateTime.Now + "] ERROR: " + ex.ToString(), "FoupBLETracker" + sensorID + " v." + version, "MainWindow");
                // END Post Exception to Database
            }

        }

        #region  Bluetooth Code

        private DeviceWatcher deviceWatcher;

        protected void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopBleDeviceWatcher();

            // Save the selected device's ID for use in other scenarios.
            /*
            var bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;
            if (bleDeviceDisplay != null)
            {
                rootPage.SelectedBleDeviceId = bleDeviceDisplay.Id;
                rootPage.SelectedBleDeviceName = bleDeviceDisplay.Name;
            }
            */
        }

        #endregion


        #region Device discovery
        private void EnumerateButton_Click()
        {
            if (deviceWatcher == null)
            {
                StartBleDeviceWatcher();

                //rootPage.NotifyUser($"Device watcher started.", NotifyType.StatusMessage);
            }
            else
            {
                StopBleDeviceWatcher();

                //rootPage.NotifyUser($"Device watcher stopped.", NotifyType.StatusMessage);
            }
        }

        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {

                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }

            //EnumerateButton.Content = "Start enumerating";
        }

        /// <summary>
        ///     Starts a device watcher that looks for all nearby BT devices (paired or unpaired). Attaches event handlers and
        ///     populates the collection of devices.
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            //EnumerateButton.Content = "Stop enumerating";

            // Additional properties we would like about the device.
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")",
                        requestedProperties,
                        DeviceInformationKind.Unknown);


            // Register event handlers before starting the watcher.

            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            //ResultCollection.Clear();

            // Start the watcher.
            deviceWatcher.Start();
        }


        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            this.Dispatcher.Invoke(() =>
            {
                Log("New Device:\nID: " + deviceInfo.Id + "\tName: " + deviceInfo.Name, "log");
            });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.

            // We must update the collection on the UI thread because the collection is databound to a UI element.

        }



        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
        }

        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {

        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.

        }

        #endregion

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            StopBleDeviceWatcher();

            if ((bool)verboseCheckBox.IsChecked) MessageBox.Show("Scan, Stopped");
            //foreach (DeviceInformation device in deviceWatcher.)
            //{
            //  richTextBox.AppendText("ID:" + device.Id + "\tName: " + device.Name);
            //}
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

            LookForBeacons();
            button2.IsEnabled = false;
            button3.IsEnabled = true;

        }


        //private BluetoothLEAdvertisementWatcher watcher;
        private BluetoothLEAdvertisementWatcher watcher;

        public void LookForBeacons()
        {
            //watcher = new BluetoothLEAdvertisementWatcher();

            watcher = new BluetoothLEAdvertisementWatcher();

            // Begin of watcher configuration. Configure the advertisement filter to look for the data advertised by the publisher 
            // in Scenario 2 or 4. You need to run Scenario 2 on another Windows platform within proximity of this one for Scenario 1 to 
            // take effect. The APIs shown in this Scenario are designed to operate only if the App is in the foreground. For background
            // watcher operation, please refer to Scenario 3.

            // Please comment out this following section (watcher configuration) if you want to remove all filters. By not specifying
            // any filters, all advertisements received will be notified to the App through the event handler. You should comment out the following
            // section if you do not have another Windows platform to run Scenario 2 alongside Scenario 1 or if you want to scan for 
            // all LE advertisements around you.

            // For determining the filter restrictions programatically across APIs, use the following properties:
            //      MinSamplingInterval, MaxSamplingInterval, MinOutOfRangeTimeout, MaxOutOfRangeTimeout

            // Part 1A: Configuring the advertisement filter to watch for a particular advertisement payload

            // First, let create a manufacturer data section we wanted to match for. These are the same as the one 
            // created in Scenario 2 and 4.

            var manufacturerData = new BluetoothLEManufacturerData();

            // Then, set the company ID for the manufacturer data. Here we picked an unused value: 0xFFFE
            manufacturerData.CompanyId = 0xFFFE;

            // Finally set the data payload within the manufacturer-specific section
            // Here, use a 16-bit UUID: 0x1234 -> {0x34, 0x12} (little-endian)
            var writer = new DataWriter();
            writer.WriteUInt16(0x1234);

            // Make sure that the buffer length can fit within an advertisement payload. Otherwise you will get an exception.
            manufacturerData.Data = writer.DetachBuffer();

            // Add the manufacturer data to the advertisement filter on the watcher:
            // No need to Filter for our example 
            //watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
            //watcher.AdvertisementFilter.Advertisement.LocalName.Contains("Tile");
            //watcher.AdvertisementFilter.Advertisement.ManufacturerData


            watcher.ScanningMode = BluetoothLEScanningMode.Active;


            // Part 1B: Configuring the signal strength filter for proximity scenarios

            // Configure the signal strength filter to only propagate events when in-range
            // Please adjust these values if you cannot receive any advertisement 
            // Set the in-range threshold to -70dBm. This means advertisements with RSSI >= -70dBm 
            // will start to be considered "in-range".
            watcher.SignalStrengthFilter.InRangeThresholdInDBm = InRangeThresholdInDBm;


            // Set the out-of-range threshold to -75dBm (give some buffer). Used in conjunction with OutOfRangeTimeout
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = OutOfRangeThresholdInDBm;

            // Set the out-of-range timeout to be 2 seconds. Used in conjunction with OutOfRangeThresholdInDBm
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(timeout);

            // By default, the sampling interval is set to zero, which means there is no sampling and all
            // the advertisement received is returned in the Received event
            watcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(SamplingInterval);
            // End of watcher configuration. There is no need to comment out any code beyond this point.


            // Attach a handler to process the received advertisement. 
            // The watcher cannot be started without a Received handler attached
            watcher.Received += OnAdvertisementReceived;

            // Attach a handler to process watcher stopping due to various conditions,
            // such as the Bluetooth radio turning off or the Stop method was called
            watcher.Stopped += OnAdvertisementWatcherStopped;

            if ((bool)verboseCheckBox.IsChecked) MessageBox.Show("Advertisement reader started...");


            watcher.Start();


        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {

            try
            {
                // We can obtain various information about the advertisement we just received by accessing 
                // the properties of the EventArgs class

                // The timestamp of the event
                DateTimeOffset timestamp = eventArgs.Timestamp;

                // The type of advertisement
                BluetoothLEAdvertisementType advertisementType = eventArgs.AdvertisementType;

                // The received signal strength indicator (RSSI)
                Int16 rssi = eventArgs.RawSignalStrengthInDBm;



                // The local name of the advertising device contained within the payload, if any
                string localName = eventArgs.Advertisement.LocalName;

                // Check if there are any manufacturer-specific sections.
                // If there is, print the raw data of the first manufacturer section (if there are multiple).
                string manufacturerDataString = "";
                string UUID = "";
                var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
                var manufacturerID = eventArgs.Advertisement.ManufacturerData;
                if (manufacturerSections.Count > 0)
                {
                    // Only print the first one of the list
                    var manufacturerData = manufacturerSections[0];
                    var data = new byte[manufacturerData.Data.Length];
                    using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                    {
                        reader.ReadBytes(data);
                    }
                    // Print the company ID + the raw data in hex format
                    manufacturerDataString = string.Format("0x{0}: {1}",
                        manufacturerData.CompanyId.ToString("X"),
                        BitConverter.ToString(data));
                }

                foreach (var id in eventArgs.Advertisement.ServiceUuids) UUID += "{" + id.ToString() + "};";

                // another way to get Mac address for BLE device 
                //var dev = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress); 
                //UUID = dev.DeviceInformation.Id;


                /* GENERATE MAC from ulong adddress String */
                ulong input = eventArgs.BluetoothAddress;
                var tempMac = input.ToString("X");
                //tempMac is now 'E7A1F7842F17'
                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                var replace = "$1:$2:$3:$4:$5:$6";
                var macAddress = Regex.Replace(tempMac, regex, replace);

                double timedelta = (timestamp - reftime).TotalSeconds;
                curtime = DateTime.Now;

                // Serialize UI update to the main UI thread

                // Display these information on the list

                this.Dispatcher.Invoke(() =>
                {

                    Log(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, [{6}]", localName, location, macAddress, sensorID, rssi, advertisementType, manufacturerDataString), "log");

                    // create / update char data for all incoming signal
                    if (acquire) updateChart(macAddress, localName, manufacturerDataString, rssi, timedelta);

                    // create json data for post
                    createJSON(location, macAddress, sensorID, rssi);
                    /*
                        Log(string.Format("[{0}]: type={1}, rssi={2}, name={3}, manufacturerData=[{4}], UUID=[{5}], BLE-address={6}",
                    timestamp.ToString("hh\\:mm\\:ss\\.fff"),
                    advertisementTypex.ToString(),
                    rssi.ToString(),
                    localName,
                    manufacturerDataString,
                    eventArgs.BluetoothAddress.ToString(), macAddress
                    ));
                    */
                });
            }
            catch (Exception ex)
            {
                if ((bool)verboseCheckBox.IsChecked) MessageBox.Show(ex.ToString());
                this.Dispatcher.Invoke(() => { Log(ex.ToString(), "error"); Log(DateTime.Now + ex.ToString(), "logfile"); });

                // Post Exception to Database
                db = new DBConnect();
                db.PostException("[" + DateTime.Now + "] ERROR: " + ex.ToString(), "FoupBLETracker" + sensorID + " v." + version, "MainWindow");
                // END Post Exception to Database
            }
        }


        private DateTime reftime = DateTime.Now;
        private DateTime curtime;
        private double chartTimeScale = 20;

        #region The Beacon Class

        private Dictionary<string, beacon> Beacons { get; set; }

        private class beacon : LiveCharts.Wpf.LineSeries
        {
            public string name { get; set; }
            public string BID { get; set; }
            public string manufacturerDataString { get; set; }

            public Dictionary<int, double> data;

            public Dictionary<int, ObservablePoint> analysisData1;

            //public new string Title { get; set; }
            //public new ChartValues<ObservablePoint> Values = new ChartValues<ObservablePoint>();

            public beacon()
            {
                this.Values = new ChartValues<ObservablePoint>();
                this.data = new Dictionary<int, double>();
                this.analysisData1 = new Dictionary<int, ObservablePoint>();
            }


            public ChartValues<ObservablePoint> SimpleChartData = new ChartValues<ObservablePoint>();
            public ChartValues<ObservablePoint> SignatureChartData = new ChartValues<ObservablePoint>();
            public ChartValues<ObservablePoint> SignalChartData = new ChartValues<ObservablePoint>();



            public void addSignal(short signal, double time)
            {
                //if (Values.Count == 20) Values.RemoveAt(0); 

                this.Values.Add(new ObservablePoint(time, Convert.ToDouble(signal)));
                if (data.ContainsKey(signal))
                {
                    ++data[signal];
                    analysisData1[signal].Y = Convert.ToDouble(data[signal]);
                }
                else
                {
                    data.Add(signal, 1);
                    analysisData1.Add(signal, new ObservablePoint(signal, Convert.ToDouble(data[signal])));
                    SimpleChartData.Add(analysisData1[signal]);
                }

            }

            public void detectPeaks(int lag = 30, double threshold = 5.0, double influence = 0.0)
            {
                SignatureChartData = new ChartValues<ObservablePoint>();
                SignalChartData = new ChartValues<ObservablePoint>();

                List<double> input = data.Values.ToList();

                ZScore_Peak_Finder Z = new ZScore_Peak_Finder();

                try
                {
                    var output = Z.StartAlgo(input, lag, threshold, influence);

                    int i = 0;
                    foreach (var item in output.signals)
                    {
                        SignatureChartData.Add(new ObservablePoint(++i, Convert.ToDouble(item)));
                    }

                    i = 0;
                    foreach (var item in input)
                    {
                        SignalChartData.Add(new ObservablePoint(++i, Convert.ToDouble(item)));

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception" + ex.ToString()); 
                }
                
            }


            public ChartValues<ObservablePoint> getChartValues()
            {

                //chart_data = analysisData1.Values.ToList<ObservablePoint>();

                foreach (KeyValuePair<int, ObservablePoint> ip in analysisData1)
                {
                    SimpleChartData.Add(ip.Value);
                }
                return SimpleChartData;
            }
        }

        #endregion

        public class SignalArray : LiveCharts.Wpf.ColumnSeries
        {

            public SignalArray()
            {
                //this.Values = new ChartValues<ObservablePoint>();

                /*  Set Column Seies formatting  */
                // [begin] use for Column series only: if want to replaicate histogram 
                // will remove spacing b/w column bars; 
                // ref: https://github.com/Live-Charts/Live-Charts/issues/190
                // ref: https://stackoverflow.com/questions/38731985/how-customize-column-width-in-column-series
                MaxColumnWidth = double.PositiveInfinity;
                ColumnPadding = 0;
                // [end] 

            }

            public void assignArray(ChartValues<ObservablePoint> val)
            {
                this.Values = val;
            }

        }


        public class PeakArray : LiveCharts.Wpf.LineSeries
        {
            public PeakArray()
            {

            }

        }

        int lag = 30;
        double threshold = 5.0;
        double influence = 0.0;

        private void updateChart(string BID, string name, string manufacturerDataString, short rssi, double time)
        {

            BID = BID.Replace(":", "");

            // if checkbox enabled only collect this data
            if (checkBox_database.IsChecked == true)
            {
                if (BIDList.ContainsKey(BID))
                {
                    name = BIDList[BID];
                }
                else
                    return;

            }


            beacon b;
            if (name.Length == 0) name = BID;

            //ComboBoxItem sel = (ComboBoxItem)Dropdown_selectBID.SelectedItem;
            //if (sel.Content == BID)
            //    ;
            //else if (Dropdown_selectBID.SelectedIndex == 0 && checkBox_data_collect.IsChecked == true)
            //    ;
            //else
            //    return; 

            if (Beacons.ContainsKey(BID))
            {
                b = Beacons[BID];
                //b.addSignal(rssi, time);
                b.name = name;
                b.Title = BID;
                if (rssi != -127) b.addSignal(rssi, time);
            }

            else
            {
                b = new beacon();

                Beacons.Add(BID, b);

                b.DataLabels = true;
                b.Title = BID;
                b.name = name;
                b.BID = BID;
                b.manufacturerDataString = manufacturerDataString;
                if (rssi != -127)
                {
                    b.addSignal(rssi, time);
                    // add to dropdown UI 
                    ComboBoxItem cb = new ComboBoxItem();
                    cb.Tag = BID;
                    cb.Content = name;
                    // update UI 
                    Dropdown_selectBID.Items.Add(cb);
                }
            }


            // update series and UI
            if (Beacons.Count > myChart.Series.Count)
            {
                foreach (KeyValuePair<string, beacon> item in Beacons)
                {
                    if (!myChart.Series.Contains(item.Value))
                    {

                        myChart.Series.Add(item.Value);

                        // output final dict array
                        //dataconsole.AppendText("\n ID = " + item.Key);
                        //foreach (var p in myChart.Series)
                        //dataconsole.AppendText("[" + p.Values + "] ");



                        // check if visibility enabled in combobox
                        if (Dropdown_selectBID.SelectedIndex == 0)
                            item.Value.Visibility = Visibility.Visible;

                        else
                            item.Value.Visibility = Visibility.Hidden;
                    }
                }

                //DataContext = this;
            }

            double timescale = (curtime - reftime).TotalSeconds;

            int min = 0, max = -127;

            // cleanup data older than XX secs ( 20 sec) 
            foreach (KeyValuePair<string, beacon> item in Beacons)
            {
                foreach (ObservablePoint p in item.Value.Values)
                {
                    //cleanup list ''
                    if ((timescale - p.X) > chartTimeScale)
                    {
                        item.Value.Values.Remove(p);
                    }

                    else
                    {
                        if (min > p.Y) min = (int)p.Y;
                        if (p.Y > max) max = (int)p.Y;
                    }

                    // print entire list 
                    //dataconsole.AppendText("[" + p.X + "," + p.Y + "] ");
                }
            }



            foreach (ComboBoxItem it in Dropdown_selectBID.Items)
            {
                it.Background = Brushes.White;
                foreach (KeyValuePair<string, beacon> item in Beacons)
                {
                    if (it.Tag == item.Key && item.Value.Values.Count > 2)
                        it.Background = Brushes.Green;

                }
            }


            if (myChart.AxisX != null && myChart.AxisY != null && timescale > chartTimeScale)
            {

                // reset X Range
                LiveCharts.Wpf.Axis ax = myChart.AxisX[0]; ax.MinValue = timescale - chartTimeScale;
                // reset Y Range
                LiveCharts.Wpf.Axis ay = myChart.AxisY[0];
                ay.SetRange(min - 10, max + 10);
            }
        }


        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            // Notify the user that the watcher was stopped
            MessageBox.Show("Scan Stopped.");
        }



        private async void delete_OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {

            var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            if (manufacturerSections.Count > 0)
            {
                var manufacturerData = manufacturerSections[0];
                var data = new byte[manufacturerData.Data.Length];
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(data);

                    if ((bool)verboseCheckBox.IsChecked) MessageBox.Show(reader.ToString());

                    // If we arrive here we have detected a Bluetooth LE advertisement
                    // Add code here to decode the the bytes in data and read the beacon identifiers

                }
            }
        }


        /***      POST BLE DETECTION DATA TO SERVER       ***/
        public string version = "3.0.9";
        public int MAXNETWORKERRORCOUNT { get; private set; }
        public int NETWORKRESETCOUNTER { get; private set; }
        private string location = "";
        private string sensorID = "";
        private bool readSuccess = false;
        private string json = "[]";
        short InRangeThresholdInDBm = -120;
        short OutOfRangeThresholdInDBm = -120;
        int timeout = 2000;
        int SamplingInterval = 0;
        int NetworkErrorCount = 0;
        short startup = 0;

        private static DBConnect db;

        List<LocationData> loc = new List<LocationData>();

        /* // calling function to construct class object collection and convert to JSON 
            
            List<Device> devices = new List<Device>();
            List<LocationData> loc = new List<LocationData>();
            BluetoothClient bc = new BluetoothClient();
            // define search timeout
            bc.InquiryLength = TimeSpan.FromSeconds(5);

            BluetoothDeviceInfo[] array = bc.DiscoverDevices();
            int count = array.Length;
            Console.WriteLine("\n\n"+DateTime.Now);
            for (int i = 0; i < count; i++)
            {
                Device device = new Device(array[i]);
                Console.WriteLine(device.DeviceName + " " + device.ID + "  " + device.LastSeen+ " strength: "+device.rssi);
                LocationData l = new LocationData();
                l.BID = device.ID;
                // Both these need to be unique

                l.location = location;
                l.SID = sensorID;
                loc.Add(l);
                devices.Add(device);
                
            }
            json = JsonConvert.SerializeObject(loc);            
            Console.WriteLine("Count: "+array.Length);
            Console.WriteLine(json);
            postJSON();

         */

        System.Threading.Timer myTimer;
        private bool force;

        private async void thisTimer()
        {
            //if (true)
            if (Initialize())
            {
                myTimer = new System.Threading.Timer(postJSON, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

                if (startup == 1)
                { // auto initialize
                    LookForBeacons();
                    button2.IsEnabled = false;
                    button3.IsEnabled = true;
                } // else wait for user to input on UI (press read beacon button) 

                //MyTimerProc(null);
            }
            else
            {
                Console.WriteLine("Error Reading location or SensorID. Check File Config.");
                this.Dispatcher.Invoke(() => { Log("Error Reading location or SensorID. Check File Config.", "error"); });
            }
            Console.WriteLine("Press \'q\' to quit.");
            Console.ReadLine();
        }
        private bool Initialize()
        {
            BLEWindow.Title = "BLE Diagnostics v." + version;

            // Read the file and display it line by line.
            string line = "";
            int counter = 0;
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(path + "location");
                Console.Write("Location:");
                line = file.ReadLine();
                Console.WriteLine(line);
                // assign location to the vars
                location = line.Trim();
                file.Close();

                file = new System.IO.StreamReader(path + "sensorID");
                Console.Write("sensorID:");
                line = file.ReadLine();
                Console.WriteLine(line);
                // assign sensorID to the vars
                sensorID = line.Trim();
                file.Close();


                file = new System.IO.StreamReader(path + "InRangeThresholdInDBm");
                Console.Write("Location:");
                line = file.ReadLine();
                Console.WriteLine(line);
                // assign location to the vars
                InRangeThresholdInDBm = short.Parse(line.Trim());
                file.Close();

                file = new System.IO.StreamReader(path + "OutOfRangeThresholdInDBm");
                Console.Write("Location:");
                line = file.ReadLine();
                Console.WriteLine(line);
                // assign location to the vars
                OutOfRangeThresholdInDBm = short.Parse(line.Trim());
                file.Close();

                file = new System.IO.StreamReader(path + "timeout");
                Console.Write("Location:");
                line = file.ReadLine();
                Console.WriteLine(line);
                // assign location to the vars
                timeout = Int32.Parse(line.Trim());
                file.Close();

                file = new System.IO.StreamReader(path + "sampling");
                Console.Write("Location:");
                line = file.ReadLine();
                Console.WriteLine(line);
                // assign location to the vars
                SamplingInterval = Int32.Parse(line.Trim());
                file.Close();

                file = new System.IO.StreamReader(path + "startup");
                Console.Write("startup flag");
                line = file.ReadLine();
                Console.WriteLine(line);
                // assign location to the vars
                startup = short.Parse(line.Trim());
                file.Close();

                MAXNETWORKERRORCOUNT = 10;

                this.Dispatcher.Invoke(() =>
                {

                    Log(string.Format("{0}, {1}, {2}, {3}, {4}", InRangeThresholdInDBm, OutOfRangeThresholdInDBm, timeout, location, sensorID), "log");
                });

                // update labels SensorID, Cleanroom, IP Address.
                try
                {
                    ipAddressLabel.Content = GetIPAddress().ToString();
                    sensorIDLabel.Content = sensorID;
                    cleanroomLabel.Content = location;
                }
                catch (Exception ex)
                {
                    // Post Exception to Database
                    db = new DBConnect();
                    db.PostException("[" + DateTime.Now + "] ERROR: " + ex.ToString(), "FoupBLETracker" + sensorID + " v." + version, "MainWindow");
                    // END Post Exception to Database

                }


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Reading location or SensorID. Check File Config.");
                this.Dispatcher.Invoke(() => { Log(ex.ToString(), "error"); Log(DateTime.Now + ex.ToString(), "logfile"); });

                // Post Exception to Database
                db = new DBConnect();
                db.PostException("[" + DateTime.Now + "] ERROR: " + ex.ToString(), "FoupBLETracker" + sensorID + " v." + version, "MainWindow");
                // END Post Exception to Database

                Console.WriteLine(ex.ToString());
                return false;
            }
            return false;
        }

        private void createJSON(string location, string id, string sensorID, short rssi)
        {
            id = id.Replace(":", "");
            // change strip colon from MacID 
            //id = id.Replace(':', '\0');
            if (rssi == -127) return;

            if (loc.Exists(x => x.BID == id))
            {
                LocationData p = loc.Find(x => x.BID == id);
                if (p.rssi < rssi) p.rssi = rssi;
                return;
            }

            else
            {
                LocationData l = new LocationData();
                l.BID = id;
                l.location = location;
                l.SID = sensorID;
                l.rssi = rssi;
                l.version = version;
                loc.Add(l);
            }
        }

        private void postJSON(object source)
        {

            json = JsonConvert.SerializeObject(loc);
            //Console.WriteLine("Count: " + array.Length);
            Console.WriteLine(json);

            this.Dispatcher.Invoke(() =>
            {
                Log(json, "JSON");
            });

            // dev only [detete]
            return;


            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://winappsweb/fouptrack/postLocations.aspx");
                httpWebRequest.ContentType = "text/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //string json = "{\"user\":\"test\"," + "\"password\":\"bla\"}";               
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result.ToString());
                    // on success 
                    this.Dispatcher.Invoke(() =>
                    {
                        if ((bool)serverResponseCheckbox.IsChecked) Log("Server:" + result, "$");
                        Log("POST status: Success ; Watcher Status" + watcher.Status.ToString(), "JSON");
                        //Log("POST status: Success", "logfile");
                        ConnectionStatusLabel.Content = "Connected";
                        ConnectionStatusLabel.Foreground = Brushes.Green;
                    });
                    //
                }


            }

            catch (WebException ex)
            {
                // HttpException is expected
                //Log("Connection Active\n"+ex.ToString(), "error");
                var wRespStatusCode = 0;
                try { wRespStatusCode = (int)((HttpWebResponse)ex.Response).StatusCode; } catch (Exception e) {; }

                switch (wRespStatusCode)
                {

                    case 500:
                        this.Dispatcher.Invoke(() => { Log(wRespStatusCode + " Connection Active\n" + ex.ToString(), "JSON"); });
                        break;
                    case 404:
                        this.Dispatcher.Invoke(() => { Log(wRespStatusCode + " Connection Active\n" + ex.ToString(), "JSON"); });
                        break;
                    case 200:
                        this.Dispatcher.Invoke(() => { Log(wRespStatusCode + " Connection Active\n" + ex.ToString(), "JSON"); });
                        break;
                    default:
                        Console.WriteLine(ex.ToString());
                        // on failure 
                        this.Dispatcher.Invoke(() =>
                        {
                            Log("POST status: Connection Lost.", "JSON");
                            Log(ex.ToString(), "error");
                            ConnectionStatusLabel.Content = "Not Connected (" + NetworkErrorCount + "/" + MAXNETWORKERRORCOUNT + ")";
                            ConnectionStatusLabel.Foreground = Brushes.Red;

                            // reset network adapter if network not connected
                            if (NetworkErrorCount < MAXNETWORKERRORCOUNT) { NetworkErrorCount++; }
                            else
                            {
                                // Execute Nework Reset for this computer 
                                NetworkErrorCount = 0;
                                ExecuteCommand(""); // run enabledisableNetwork.cmd script.

                                // Post Exception to Database
                                db = new DBConnect();
                                db.PostException("[" + DateTime.Now + "]" + " NETWORK RESET TRIGGERED @" + System.Environment.MachineName + "<br>" + ex.ToString(), sensorID + " v." + version, "MainWindow");
                                // END Post Exception to Database

                                Log(ex.ToString(), "logfile");
                                Log(DateTime.Now + " : RESTARTING NETWORK", "log");
                                Log(DateTime.Now + " : RESTARTING NETWORK", "logfile");
                            }
                        });

                        break;
                }
            }

            // post data to appsengine befoe clearing JSON[]
            postJSON2Appsengine(source);

            // clean JSON 
            json = "[]";
            // clean loc
            loc.Clear();

            // add header for SENSOR ping detection
            LocationData l = new LocationData();
            l.BID = "Ping";
            l.location = location;
            l.SID = sensorID;
            l.rssi = 0;
            l.version = version;
            loc.Add(l);
            // end add header 

        }


        private void postJSON2Appsengine(object source)
        {

            //json = JsonConvert.SerializeObject(loc);
            //Console.WriteLine("Count: " + array.Length);
            //Console.WriteLine(json);

            this.Dispatcher.Invoke(() =>
            {
                Log(json, "JSON");
            });

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://appsengine:3000/trackerJSON");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //string json = "{\"user\":\"test\"," + "\"password\":\"bla\"}";               
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result.ToString());
                    // on success 
                    // on success 
                    this.Dispatcher.Invoke(() =>
                    {
                        if ((bool)serverResponseCheckbox.IsChecked) Log("Server:" + result, "$");
                        Log("AppsEngine POST status: Success", "JSON");
                        //Log("POST status: Success", "logfile");
                        ConnectionStatusLabel.Content = "Connected";
                        ConnectionStatusLabel.Foreground = Brushes.Green;
                    });
                    //
                    //
                }


            }

            catch (WebException ex)
            {
                // HttpException is expected
                //Log("Connection Active\n"+ex.ToString(), "error");
                var wRespStatusCode = 0;
                try { wRespStatusCode = (int)((HttpWebResponse)ex.Response).StatusCode; } catch (Exception e) {; }

                switch (wRespStatusCode)
                {
                    case 500:
                        this.Dispatcher.Invoke(() => { Log(wRespStatusCode + " Connection Active\n" + ex.ToString(), "JSON"); });
                        break;
                    case 404:
                        this.Dispatcher.Invoke(() => { Log(wRespStatusCode + " Connection Active\n" + ex.ToString(), "JSON"); });
                        break;
                    case 200:
                        this.Dispatcher.Invoke(() => { Log(wRespStatusCode + " Connection Active\n" + ex.ToString(), "JSON"); });
                        break;
                    default:
                        Console.WriteLine(ex.ToString());
                        // on failure 
                        this.Dispatcher.Invoke(() =>
                        {
                            Log("POST status: Connection Lost.", "JSON");
                            Log(ex.ToString(), "error");
                        });
                        break;
                        // do nothing and exit
                }
            }

        }



        // return the first IPv4, non-dynamic/link-local, non-loopback address
        public static IPAddress GetIPAddress()
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses("");

            foreach (IPAddress hostAddress in hostAddresses)
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(hostAddress) &&  // ignore loopback addresses
                    !hostAddress.ToString().StartsWith("169.254."))  // ignore link-local addresses
                    return hostAddress;
            }
            return null; // or IPAddress.None if you prefer
        }

        /* //Modification needed: Activate only when BT is checking Error in Foup checkins
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
                Console.WriteLine(e.ToString());

            }
        }
        */

        private static void ExecuteCommand(string command)
        {
            //var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            var processInfo = new ProcessStartInfo(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\EnableDisableNetwork.cmd", command);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        public class LocationData
        {
            public string location { get; set; }
            public string BID { get; set; }
            public string SID { get; set; }
            public short rssi { get; set; }
            public string version { get; set; }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
            button2.IsEnabled = true;
            button3.IsEnabled = false;
        }

        private void button_clear_log_Click(object sender, RoutedEventArgs e)
        {
            jSONTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Clear();
        }

        private void refreshIPButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ipAddressLabel.Content = GetIPAddress().ToString();
                sensorIDLabel.Content = sensorID;
                cleanroomLabel.Content = location;
            }
            catch (Exception ex) {; }
        }

        private void resetNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand("");
        }


        #region Charting Code 

        public void SignalScatterChart()
        {
            InitializeComponent();

            //myChart.AxisX = "isuds";

            //var r = new Random();
            //ValuesA = new ChartValues<ObservablePoint>();
            //ValuesB = new ChartValues<ObservablePoint>();
            //ValuesC = new ChartValues<ObservablePoint>();

            //for (var i = 0; i < 20; i++)
            //{
            //    ValuesA.Add(new ObservablePoint(r.NextDouble() * 10, r.NextDouble() * 10));
            //    ValuesB.Add(new ObservablePoint(r.NextDouble() * 10, r.NextDouble() * 10));
            //    ValuesC.Add(new ObservablePoint(r.NextDouble() * 10, r.NextDouble() * 10));
            //}

            //myChart.Series.Clear();
            //SeriesCollection sr = new SeriesCollection();

            // iterate over dict
            foreach (KeyValuePair<string, beacon> item in Beacons)
            {
                myChart.Series.Add(item.Value);
            }

            //myChart.Series.Add(sr); 


            DataContext = this;
        }

        //public void Analysis1ScatterChart()
        //{
        //    InitializeComponent();

        //    //SeriesCollection sr = new SeriesCollection();

        //    // iterate over dict
        //    foreach (KeyValuePair<string, beacon> item in Beacons)
        //    {
        //        analysisA.Series.Add(item.Value);
        //    }

        //    DataContext = this;
        //}


        //public ChartValues<ObservablePoint> ValuesA { get; set; }
        //public ChartValues<ObservablePoint> ValuesB { get; set; }
        //public ChartValues<ObservablePoint> ValuesC { get; set; }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            acquire = false;
            this.Button_Pause.Content = "Play";
            //Button_ResetChart_Click(sender, e);

            // iterate over dict
            foreach (KeyValuePair<string, beacon> item in Beacons)
            {
                // output final dict array
                dataconsole.AppendText("\n ID = " + item.Key);
                foreach (var p in myChart.Series)
                    dataconsole.AppendText("[" + p.Values + "] ");
            }

            // dispose all objects 
            Dropdown_selectBID.SelectedIndex = 0;

            for (int i = 1; i < Dropdown_selectBID.Items.Count; ++i)
                Dropdown_selectBID.Items.RemoveAt(i);

            slider_timeSpan.Value = 20;

            Beacons.Clear();
            myChart.Series.Clear();
            myChart.UpdateLayout();
        }


        #endregion

        private void showlabel_toggle(object sender, RoutedEventArgs e)
        {
            //showlabel_toggle

            bool p = false;
            if (checkBox_showlabel.IsChecked == true) p = true; else p = false;

            if (Beacons != null)
                foreach (KeyValuePair<string, beacon> item in Beacons)
                {
                    item.Value.DataLabels = p;
                }

        }

        private void Dropdown_selectBID_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxItem cb = (ComboBoxItem)Dropdown_selectBID.SelectedItem;
            if (Beacons != null)
            {
                //dataconsole.AppendText("Selections: " + Dropdown_selectBID.SelectedValue.ToString() + "\n");
                dataconsole.AppendText("Selections: " + cb.Tag + "\n");

                foreach (KeyValuePair<string, beacon> item in Beacons)
                {
                    dataconsole.AppendText("[" + item.Key + "=>" + item.Value.BID + "] ");
                    if (Dropdown_selectBID.SelectedIndex == 0)
                        item.Value.Visibility = Visibility.Visible;

                    else if (item.Key == cb.Tag)
                    {
                        item.Value.Visibility = Visibility.Visible;

                        // add function for analysis chart 
                        S.Values = item.Value.SimpleChartData;
                        S.Title = item.Key;

                    }
                    else
                        item.Value.Visibility = Visibility.Hidden;
                }

                if (analysisA.Series.Count > 0) analysisA.Series.Clear();
                analysisA.Series.Add(S);
                //analysisA.Series.Add(signal_peaks);
                analysisA.Update(); ///.UpdateLayout();

                //myChart.Update(restartView:true);
                myChart.UpdateLayout();
            }
        }

        SignalArray S = new SignalArray();


        private bool acquire = false;
        private void Button_ResetChart_Click(object sender, RoutedEventArgs e)
        {
            if (acquire)
                this.Button_Pause.Content = "Play";
            else
                this.Button_Pause.Content = "Pause";
            acquire = !acquire;


        }

        private void slider_timeSpan_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            chartTimeScale = (double)slider_timeSpan.Value;
            if (dataconsole != null) dataconsole.AppendText("Char time= " + chartTimeScale);
            if (label_timespan != null) label_timespan.Content = "TimeSpan = " + chartTimeScale + " secs.";
        }

        private Dictionary<string, string> BIDList = new Dictionary<string, string>();
        private void button_downloadDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://winappsweb/fouptrack/getTrackData.aspx?param=BID");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //string json = "{\"user\":\"test\"," + "\"password\":\"bla\"}";               
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string result = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result += streamReader.ReadToEnd();

                }
                //dataconsole.AppendText(result.ToString());

                // deserialize data 
                List<Foupdata> o = JsonConvert.DeserializeObject<List<Foupdata>>(result);


                // populate local database Object 
                foreach (var p in o)
                {
                    BIDList.Add(p.FoupBID, p.FoupName);
                    dataconsole.AppendText(p.FoupBID + ",");
                }

                // UI checkbx enable 
                checkBox_database.IsEnabled = true;
                checkBox_database.IsChecked = true;

            }
            catch (Exception ex)
            {
                Log(ex.ToString(), "error");
            }
        }

        private class Foupdata
        {
            // example JSON ref: http://winappsweb/fouptrack/getTrackData.aspx?param=BID
            public string FoupName { get; set; }
            public string FoupBID { get; set; }

        }


        LiveCharts.Wpf.LineSeries P = new LiveCharts.Wpf.LineSeries();
        LiveCharts.Wpf.ColumnSeries Sig = new LiveCharts.Wpf.ColumnSeries();
        private void button_ApplyAlgo_click(object sender, RoutedEventArgs e)
        {
            lag = Int32.Parse(textBox_lag.Text);
            threshold = Double.Parse(textBox_threshold.Text);
            influence = Double.Parse(textBox_influence.Text);

            ComboBoxItem cb = (ComboBoxItem)Dropdown_selectBID.SelectedItem;
            if (Beacons != null)
            {
                
                foreach (KeyValuePair<string, beacon> item in Beacons)
                {
                    if (Dropdown_selectBID.SelectedIndex == 0)
                        return;

                    else if (item.Key == cb.Tag)
                    {
                        item.Value.detectPeaks(lag, threshold, influence);
                        P.Values = item.Value.SignatureChartData;
                        Sig.Values = item.Value.SignalChartData; 
                    }
                }

                if (analysisB.Series.Count > 0) analysisB.Series.Clear();
                analysisB.Series.Add(P);
                analysisB.Series.Add(Sig);

                analysisB.Update(); ///.UpdateLayout();
            }
        }

    }
}