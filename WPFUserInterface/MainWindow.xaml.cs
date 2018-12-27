/////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs -  GoogleMaps WPF interface                 //
//  ver 1.0                                                        //
//                                                                 //
//  Language:      Visual C#, 2010                                 //
//  Platform:      HP Pavilion dv4, Win7 Professional              //
//  Application:   User interface for GoogleMaps application       //
//  Author:        Varun Joshi                                     //
/////////////////////////////////////////////////////////////////////

/*
  Module Operations: 
  ==================
   This file has all the definitions for elements declared in the corresponding XAML
 
  Public Interface:
  ================= 
   public MainWindow() - Initializes the application
 
  Build Process:
  ==============
  Build commands (either one)
    - devenv IndependentStudy.sln
    - csc /target:exe /define:TEST_MAINWINDOW MainWindow.xaml.cs

  Maintenance History:
  ====================
  ver 1.1 : 10/21/2011  
  - second release  [Moved the GetGeoCode functionality to WCF application]   
  ver 1.0 : 10/12/2011
  - first release
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Linq;
using System.ServiceModel;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;


namespace IndependentStudy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
    #region "Fields"
    private XDocument geoDoc;
    private string location;
    private int zoom;
    private string mapType;
    private double lat;
    private double lng;
    #endregion
    

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Hide the progress bar after map loads
        /// </summary>
        
        private void HideProgressBar()
        {
            MapProgressBar.Visibility = Visibility.Hidden;
            ShowMapButton.IsEnabled = true;
        }
        
        ///<summary>
        /// Get geocode data and store in a xml document
        ///</summary>
        
        private void GetGeocodeData(string location)
        {
            using (ChannelFactory<GoogleMapsServiceReference.IService1> customersFactory = new ChannelFactory<GoogleMapsServiceReference.IService1>("BasicHttpBinding_IService1"))
            {
                GoogleMapsServiceReference.IService1 customersProxy = customersFactory.CreateChannel();
                string geoURL = customersProxy.GetNameURL(location);
                try
                {
                    geoDoc = XDocument.Load(geoURL);
                }
                catch
                {
                    this.Dispatcher.BeginInvoke(new ThreadStart(HideProgressBar), DispatcherPriority.Normal, null);
                    MessageBox.Show("Ensure that internet connection is available.", "Map App", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.Dispatcher.BeginInvoke(new ThreadStart(ShowGeocodeData), DispatcherPriority.Normal, null);
            }
        }

        /// <summary>
        /// Get the latitude and longitude of the location entered into the textbox
        /// </summary>
         
        private void ShowGeocodeData()
        {
            string responseStatus = geoDoc.Element("GeocodeResponse").Element("status").Value;
            if (responseStatus == "OK")
            {
                string formattedAddress = geoDoc.Element("GeocodeResponse").Element("result").Element("formatted_address").Value;
                string longitude = geoDoc.Element("GeocodeResponse").Element("result").Element("geometry").Element("location").Element("lng").Value;
                string latitude = geoDoc.Element("GeocodeResponse").Element("result").Element("geometry").Element("location").Element("lat").Value;
                string locationType = geoDoc.Element("GeocodeResponse").Element("result").Element("geometry").Element("location_type").Value;

                AddressTxtBlck.Text = formattedAddress;
                LatitudeTxtBlck.Text = latitude;
                LongitudeTxtBlck.Text = longitude;

                switch (locationType)
                {
                    case "APPROXIMATE":
                        AccuracyTxtBlck.Text = "Approximate";
                        break;
                    case "ROOFTOP":
                        AccuracyTxtBlck.Text = "Precise";
                        break;
                    default:
                        AccuracyTxtBlck.Text = "Approximate";
                        break;
                }

                lat = Double.Parse(latitude);
                lng = Double.Parse(longitude);
            }
            else if (responseStatus == "ZERO_RESULTS")
            {
                MessageBox.Show("Unable to show results for: \n" + location + "Unknown Location" + MessageBoxButton.OK + MessageBoxImage.Information);
                DisplayXXXXXXs();
                AddressTxtBox.SelectAll();
            }

            ShowMapButton.IsEnabled = true;
            ZoomInButton.IsEnabled = true;
            ZoomOutButton.IsEnabled = true;
            TerrainToggleButton.IsEnabled = true;
            RoadmapToggleButton.IsEnabled = true;
            MapProgressBar.Visibility = System.Windows.Visibility.Hidden;
        }
        
        ///<summary>
        /// Zoom-in on the map
        ///</summary>
         
        private void ZoomIn()
        {
            if(zoom < 21)
            {
                zoom += 1;
                ShowMapUsingLatLng(mapType);

                if (ZoomOutButton.IsEnabled == false) 
                    ZoomOutButton.IsEnabled = true;
            }
            else
                ZoomInButton.IsEnabled = false;
        }

        ///<summary>
        /// Zoom-out on map
        ///</summary>
         
        private void ZoomOut()
        {
            if (zoom > 0)
            {
                zoom -= 1;
                ShowMapUsingLatLng(mapType);

                if (ZoomInButton.IsEnabled == false)
                    ZoomInButton.IsEnabled = true;
            }
            else
                ZoomOutButton.IsEnabled = false;
        }

        /// <summary>
        /// Function to move the map upwards
        /// </summary>
 
        private void MoveUp()
        {
            //Default zoom is 15 and at this level changing the center point is done by 0.003 degrees. 
            //Shifting the center point is done by higher values at zoom levels less than 15.
            double diff;
            double shift;

            if (lat < 88) // to avoid values beyond 90 degrees of lat
            {
                if (zoom == 15)
                    lat += 0.003;
                else if (zoom > 15) 
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lat += shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lat += shift;
                }
                ShowMapUsingLatLng(mapType);
            }
            else
                lat = 90;
        }

        /// <summary>
        /// Function to move the map downwards
        /// </summary>
      
        private void MoveDown()
        {
            double diff;
            double shift;
            if (lat > -88)
            {
                if (zoom == 15)
                    lat -= 0.003;
                else if (zoom > 15)
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lat -= shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lat -= shift;
                }
                ShowMapUsingLatLng(mapType);
            }
            else
                lat = -90;
        }

        /// <summary>
        /// Function to move the map to the left
        /// </summary>
      
        private void MoveLeft()
        {
            double diff = 0;
            double shift = 0;
            // Use -178 to avoid negative values below -180.
            if ((lng > -178))
            {
                if ((zoom == 15))
                {
                    lng -= 0.003;
                }
                else if ((zoom > 15))
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lng -= shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lng -= shift;
                }
                ShowMapUsingLatLng(mapType);
            }
            else
            {
                lng = 180;
            }
        }

        /// <summary>
        /// Function to move the map to the right
        /// </summary>
      
        private void MoveRight()
        {
            double diff = 0;
            double shift = 0;
            if ((lng < 178))
            {
                if ((zoom == 15))
                {
                    lng += 0.003;
                }
                else if ((zoom > 15))
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lng += shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lng += shift;
                }
                ShowMapUsingLatLng(mapType);
            }
            else
            {
                lng = -180;
            }
        }

        /// <summary>
        /// Utlizes the WCF service to contact Google Static Map API and show the map
        /// </summary>

        private void ShowMapImage(String location, int zoom, String mapType)
        {
            using (ChannelFactory<GoogleMapsServiceReference.IService1> customersFactory = new ChannelFactory<GoogleMapsServiceReference.IService1>("BasicHttpBinding_IService1"))
            {
                GoogleMapsServiceReference.IService1 customersProxy = customersFactory.CreateChannel();
                byte[] mapByte = customersProxy.GetBytesForImage(location, zoom, mapType);
                MemoryStream stream = new MemoryStream(mapByte);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                MapImage.Source = image;
            }
        }

        /// <summary>
        /// Utlizes the WCF service to contact Google Static Map API and show the map
        /// </summary>
        /// <param name="maptype"></param>

        private void ShowMapUsingLatLng(string maptype)
        {
            using (ChannelFactory<GoogleMapsServiceReference.IService1> customersFactory = new ChannelFactory<GoogleMapsServiceReference.IService1>("BasicHttpBinding_IService1"))
            {
                GoogleMapsServiceReference.IService1 customersProxy = customersFactory.CreateChannel();
                byte[] mapByte = customersProxy.GetLatLongBytesForImage(lat, lng, location, zoom, mapType);
                MemoryStream stream = new MemoryStream(mapByte);
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.StreamSource = stream;
                bmpImage.EndInit();
                MapImage.Source = bmpImage;
            }
        }

        /// <summary>
        /// Function to fill the textblocks defined in the XAML
        /// </summary>
     
        private void DisplayXXXXXXs()
        {
            AddressTxtBlck.Text = "City, State, Country";
            LatitudeTxtBlck.Text = "Latitude";
            LongitudeTxtBlck.Text = "Longitude";
            AccuracyTxtBlck.Text = "Accuracy";
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddressTxtBox.Focus();
        }
      
        private void CloseButton_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event Handler for Show Map Button
        /// </summary>
        /// <param name="sender"></param>
        ///<param name="e"></param>
  
        private void ShowMapButton_Click_1(object sender, RoutedEventArgs e)
        {
            if ((AddressTxtBox.Text != string.Empty))
            {
                location = AddressTxtBox.Text.Replace(" ", "+");
                zoom = 15;
                mapType = "roadmap";
                Thread geoThread = new Thread(() => GetGeocodeData(location));
                geoThread.Start();
                ShowMapImage(location, zoom, mapType); // call the ShowMapImage method 
                AddressTxtBox.SelectAll();
                ShowMapButton.IsEnabled = false;
                MapProgressBar.Visibility = System.Windows.Visibility.Visible;

                if ((RoadmapToggleButton.IsChecked == false))
                {
                    RoadmapToggleButton.IsChecked = true;
                    TerrainToggleButton.IsChecked = false;
                }
            }
            else
            {
                MessageBox.Show("Enter location address.", "Map App", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                AddressTxtBox.Focus();
            }

        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn(); // calls the method to perform the zoom in operation
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut(); // calls the method to perform the zoom out operation
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDown(); // calls the method to perform the move down operation
        }

        private void MoveLeftButton_Click(object sender, RoutedEventArgs e)
        {
            MoveLeft(); // calls the method to perform the move left operation
        }

        private void MoveRightButton_Click(object sender, RoutedEventArgs e)
        {
            MoveRight(); // calls the method to perform the move right operation
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveUp(); // calls the method to perform the move up operation
        }

        /// <summary>
        /// Method to handle the event when the terrain toggle button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void TerrainToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mapType != "terrain")
            {
                mapType = "terrain";
                ShowMapUsingLatLng(mapType);
                RoadmapToggleButton.IsChecked = false;
            }
        }

        /// <summary>
        /// Method to handle the event when the roadmap toggle button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void RoadmapToggleButton_Checked(object sender, RoutedEventArgs e) 
        {
            if (mapType != "roadmap")
            {
                mapType = "roadmap";
                ShowMapUsingLatLng(mapType);
                TerrainToggleButton.IsChecked = false;
            }

        }

        /// <summary>
        /// Method to minimize the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

    }
}
