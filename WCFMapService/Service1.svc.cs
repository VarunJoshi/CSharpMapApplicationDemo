//////////////////////////////////////////////////////////////////////////
//  Service1.svc.cs -  GoogleMaps WCF Service defining functions        //
//  ver 1.0                                                             //
//                                                                      //
//  Language:      Visual C#, 2010                                      //
//  Platform:      HP Pavilion dv4, Win7 Professional                   //
//  Application:   User interface for WCF application                   //
//  Author:        Varun Joshi                                          //
//////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WCFMapService
{
    public class Service1 : IService1
    {
        /// <summary>
        /// Get bytes for GoogleMaps image if location name is typed into search box
        /// </summary>
        /// <param name="location"></param>
        /// <param name="zoom"></param>
        /// <param name="mapType"></param>
        /// <returns>byte array</returns>

        public byte[] GetBytesForImage(string location, int zoom, string mapType)
        {
            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "size=600x500&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            HttpWebResponse response = SendUrlRequest(mapURL);
            byte[] bytes = GetBytesFromResponse(response);
            return bytes;
        }

        /// <summary>
        /// Get bytes for GoogleMaps image if latitude and longitude is typed into search box
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="location"></param>
        /// <param name="zoom"></param>
        /// <param name="mapType"></param>
        /// <returns>byte array</returns>
        
        public byte[] GetLatLongBytesForImage(double lat, double lng, string location, int zoom, string mapType)
        {
            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + lat + "," + lng + "&" + "size=600x500&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            HttpWebResponse response = SendUrlRequest(mapURL);
            byte[] bytes = GetBytesFromResponse(response);
            return bytes;
        }

        /// <summary>
        /// get xml data from google maps api
        /// </summary>
        /// <param name="location"></param>
        /// <returns>string</returns>

        public string GetNameURL(string location)
        {
            string geocodeURL = "http://maps.googleapis.com/maps/api/" + "geocode/xml?address=" + location + "&sensor=false";
            return geocodeURL;
        }
        
        /// <summary>
        /// Get http response from the GoogleMaps API
        /// </summary>
        /// <param name="url"></param>
        /// <returns>HTTP web response</returns>
        
        private HttpWebResponse SendUrlRequest(string url)
        {
            try
            {
                HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create(url);
                rq.Timeout = 1000000;
                rq.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)rq.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                    return response;
                else
                    return null;
            }
            catch (TimeoutException)
            {
                MessageBox.Show("Operation is timed out please try again..", "Error");
                return null;
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("{0} \r\n{1}", e.Message, e.StackTrace), "Error");
                return null;
            }
        }

        /// <summary>
        /// Store the response from GoogleMaps into array of bytes
        /// </summary>
        /// <param name="response"></param>
        /// <returns>byte array</returns>
        
        private byte[] GetBytesFromResponse(HttpWebResponse response)
        {
            int totalBytes = 0;
            byte[] buffer = new byte[4155];
            int BlockSize = 4096;
            byte[] block = new byte[BlockSize];
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                using (Stream stream = response.GetResponseStream())
                {
                    while (true)
                    {
                        int bytesRead = stream.Read(block, 0, BlockSize);
                        totalBytes += bytesRead;
                        if (bytesRead > 0)
                            memoryStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                    return memoryStream.ToArray();
                }
            }
            catch(NullReferenceException)
            {
                MessageBox.Show("Operation is timed out please try again..", "Error");
                return null;
            }
        }

    }
}
