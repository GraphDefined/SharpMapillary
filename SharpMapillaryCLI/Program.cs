/*
 * Copyright (c) 2014 Achim 'ahzf' Friedland <achim@graphdefined.org>
 * This file is part of SharpMapillary <http://www.github.com/ahzf/SharpMapillary>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using ExifLibrary;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public class Program
    {

        public static void Main(String[] Arguments)
        {

            // Look North
            //var GPSEarly_Latitude  = 51.467696956223385;    // N|S
            //var GPSLate_Latitude   = 52.467696956223385;
            //var GPSEarly_Longitude = 11.87509765625;        // W|O
            //var GPSLate_Longitude  = 11.87509765625;

            // Look East
            var GPSEarly_Latitude  = 51.467696956223385;    // N|S
            var GPSLate_Latitude   = 51.467696956223385;
            var GPSEarly_Longitude = 11.87509765625;        // W|O
            var GPSLate_Longitude  = 12.97509765625;

            // Look South
            //var GPSEarly_Latitude  = 52.467696956223385;    // N|S
            //var GPSLate_Latitude   = 51.467696956223385;
            //var GPSEarly_Longitude = 11.87509765625;        // W|O
            //var GPSLate_Longitude  = 11.87509765625;

            // Look West
            //var GPSEarly_Latitude  = 51.467696956223385;    // N|S
            //var GPSLate_Latitude   = 51.467696956223385;
            //var GPSEarly_Longitude = 11.97509765625;        // W|O
            //var GPSLate_Longitude  = 11.47509765625;

            var dy              = GPSEarly_Latitude - GPSLate_Latitude;
            var dx              = Math.Cos(Math.PI / 180 * GPSLate_Latitude) * (GPSLate_Longitude - GPSEarly_Longitude);
            var Image_Direction = 90 + Math.Atan2(dy, dx) * 180 / Math.PI;

            if (Image_Direction < 0)
                Image_Direction += 360;

            //ToDo: Ensure, that there are always enough GPS trackpoints
            //      before the first and after the last image!

            SharpMapillary.Start(@"E:\_Projekte\Mapillary\Jena-West1").//Ziegenhain2").
                                 LoadGPXs().
                                 LoadJPGs().//TimeOffset: TimeSpan.FromSeconds(51)).
                                 SyncGPS().
                                 Do(v => Console.WriteLine("Number of GPS trackpoints: " + v.NumberOfGPSPoints)).
                                 Do(v => Console.WriteLine("Number of images: " +          v.NumberOfImages)).
                                 Store("fixed").
                                 ToArray();

        }

    }

}
