//////////////////////////////////////////////////////////////////////////
//  IService1.cs -  GoogleMaps WCF Service interface defining contracts //
//  ver 1.0                                                             //
//                                                                      //
//  Language:      Visual C#, 2010                                      //
//  Platform:      HP Pavilion dv4, Win7 Professional                   //
//  Application:   User interface for WCF application                   //
//  Author:        Varun Joshi, Syracuse University                     //
//                 (315) 706-7277, vjoshi@syr.edu                       //
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Net;

namespace WCFMapService
{
    [ServiceContract]
    public interface IService1
    {
        /// <summary>
        /// Operation Contract used to get bytes for the image that we get from Google Maps
        /// </summary>
        /// <param name="location"></param>
        /// <param name="zoom"></param>
        /// <param name="mapType"></param>
        /// <returns></returns>

        [OperationContract]
        byte[] GetBytesForImage(string location, int zoom, string mapType);

        /// <summary>
        /// Operation Contract used to get bytes for the image if latitude longitude are provided
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="location"></param>
        /// <param name="zoom"></param>
        /// <param name="mapType"></param>
        /// <returns></returns>
        
        [OperationContract]
        byte[] GetLatLongBytesForImage(double lat, double lng, string location, int zoom, string mapType);

        /// <summary>
        /// Operation Contract used to get the XML data from the Google Maps API
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        
        [OperationContract]
        string GetNameURL(string location);

    }

}
