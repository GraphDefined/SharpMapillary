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
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        #region LoadGPXs(this Path)

        public static MapillaryInfo LoadGPXs(this String Path)
        {
            return LoadGPXs(Path, null);
        }

        #endregion

        #region LoadGPXs(this MapillaryInfo)

        public static MapillaryInfo LoadGPXs(this MapillaryInfo MapillaryInfo)
        {
            return LoadGPXs(MapillaryInfo.FilePath, MapillaryInfo);
        }

        #endregion

        #region LoadGPXs(Path, MapillaryInfo = null)

        public static MapillaryInfo LoadGPXs(String Path, MapillaryInfo MapillaryInfo = null)
        {

            #region Initial checks...

            if (Path == null)
                throw new ArgumentNullException("Illegal path!");

            #endregion

            foreach (var GPXFile in Directory.EnumerateFiles(Path, "*.gpx"))
                LoadGPX(GPXFile, ref MapillaryInfo);

            return MapillaryInfo;

        }

        #endregion

        #region LoadGPXs(this Paths)

        public static IEnumerable<MapillaryInfo> LoadGPXs(this IEnumerable<String> Paths)
        {
            return Paths.Select(v => LoadGPXs(v, null));
        }

        #endregion

        #region LoadGPXs(this MapillaryInfos)

        public static IEnumerable<MapillaryInfo> LoadGPXs(this IEnumerable<MapillaryInfo> MapillaryInfos)
        {
            return MapillaryInfos.Select(v => LoadGPXs(v.FilePath, v));
        }

        #endregion


        #region LoadGPX(this GPXFile)

        public static MapillaryInfo LoadGPX(this String GPXFile)
        {

            var Mapillary = new MapillaryInfo(GPXFile.Substring(0, GPXFile.LastIndexOf(Path.DirectorySeparatorChar)));

            return LoadGPX(GPXFile, ref Mapillary);

        }

        #endregion

        #region LoadGPX(this MapillaryInfo, GPXFile)

        public static MapillaryInfo LoadGPX(this MapillaryInfo MapillaryInfo, String GPXFile)
        {
            return LoadGPX(GPXFile, ref MapillaryInfo);
        }

        #endregion

        #region LoadGPX(GPXFile, ref MapillaryInfo)

        public static MapillaryInfo LoadGPX(String GPXFile, ref MapillaryInfo MapillaryInfo)
        {

            #region GPX XML docu...

            // <?xml version="1.0" encoding="UTF-8"?>
            // <gpx version="1.1"
            //      creator="RunKeeper - http://www.runkeeper.com"
            //      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            //      xmlns="http://www.topografix.com/GPX/1/1"
            //      xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd"
            //      xmlns:gpxtpx="http://www.garmin.com/xmlschemas/TrackPointExtension/v1">
            //   <trk>
            //     <name><![CDATA[Cycling 7/22/14 1:29 pm]]></name>
            //     <time>2014-07-22T13:29:07Z</time>
            //     <trkseg>
            //       <trkpt lat="50.927267000" lon="11.612756000"><ele>236.0</ele><time>2014-07-22T13:29:07Z</time></trkpt>
            //       <trkpt lat="50.927267000" lon="11.612754000"><ele>236.0</ele><time>2014-07-22T13:29:09Z</time></trkpt>
            //       <trkpt lat="50.927367000" lon="11.612731000"><ele>236.0</ele><time>2014-07-22T13:29:33Z</time></trkpt>
            //       <trkpt lat="50.927454000" lon="11.612754000"><ele>235.0</ele><time>2014-07-22T13:29:35Z</time></trkpt>
            //       <trkpt lat="50.927563000" lon="11.612748000"><ele>234.2</ele><time>2014-07-22T13:29:37Z</time></trkpt>
            //     </trkseg>
            //   </trk>
            // </gpx>

            #endregion

            #region Initial checks...

            if (GPXFile == null)
                throw new ArgumentNullException("Illegal path!");

            if (MapillaryInfo == null)
                MapillaryInfo = new MapillaryInfo(GPXFile.Substring(0, GPXFile.LastIndexOf(Path.DirectorySeparatorChar)));

            #endregion

            #region Init...

            XNamespace          NS              = "http://www.topografix.com/GPX/1/1";
            MapillaryImageInfo  MapillaryImage  = null;

            DateTime            GPS_Timestamp;
            Double              GPS_Latitude;
            Double              GPS_Longitude;
            Double              GPS_Altitude;

            #endregion

            try
            {

                foreach (var GPSTrackPoint in XDocument.Load(GPXFile).
                                                            Root.
                                                            Elements(NS + "trk").
                                                            Elements(NS + "trkseg").
                                                            Elements(NS + "trkpt"))

                {

                    GPS_Timestamp  = DateTime.Parse(GPSTrackPoint.Element(NS + "time").Value).ToUniversalTime();
                    GPS_Latitude   = Double.  Parse(GPSTrackPoint.Attribute("lat").Value);
                    GPS_Longitude  = Double.  Parse(GPSTrackPoint.Attribute("lon").Value);
                    GPS_Altitude   = Double.  Parse(GPSTrackPoint.Element(NS + "ele").Value);

                    MapillaryInfo.NumberOfGPSPoints++;

                    if (!MapillaryInfo.Data.TryGetValue(GPS_Timestamp, out MapillaryImage))
                        MapillaryInfo.Data.Add(GPS_Timestamp, new MapillaryImageInfo() {
                                                                     GPS_Timestamp  = GPS_Timestamp,
                                                                     GPS_Latitude   = GPS_Latitude,
                                                                     GPS_Longitude  = GPS_Longitude,
                                                                     GPS_Altitude   = GPS_Altitude
                                                                 });

                    else
                    {

                        if (!MapillaryImage.GPS_Timestamp.HasValue)
                        {
                            MapillaryImage.GPS_Timestamp  = GPS_Timestamp;
                            MapillaryImage.GPS_Latitude   = GPS_Latitude;
                            MapillaryImage.GPS_Longitude  = GPS_Longitude;
                            MapillaryImage.GPS_Altitude   = GPS_Altitude;
                        }

                        else
                            Console.WriteLine("Double GPS timestamp: " + GPS_Timestamp.ToString("s") + "Z" + " in GPX file: " + GPXFile);

                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("There is something wrong in file: " + GPXFile + Environment.NewLine + e.Message);
            }

            return MapillaryInfo;

        }

        #endregion

    }

}
