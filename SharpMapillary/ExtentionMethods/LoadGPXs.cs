/*
 * Copyright (c) 2014 Achim 'ahzf' Friedland <achim@graphdefined.org>
 * This file is part of SharpMapillary <http://www.github.com/GraphDefined/SharpMapillary>
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

        #region LoadGPXs(this Path, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadGPXs(this String                                       Path,
                                                  Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp = null)
        {
            return LoadGPXs(Path, null, OnDupliateTimestamp);
        }

        #endregion

        #region LoadGPXs(this MapillaryInfo, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadGPXs(this SharpMapillaryInfo                           MapillaryInfo,
                                                  Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp = null)
        {
            return LoadGPXs(MapillaryInfo.FilePath, MapillaryInfo, OnDupliateTimestamp);
        }

        #endregion

        #region LoadGPXs(Path, MapillaryInfo = null, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadGPXs(String                                            Path,
                                                  SharpMapillaryInfo                                MapillaryInfo        = null,
                                                  Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp  = null)
        {

            #region Initial checks...

            if (Path == null)
                throw new ArgumentNullException("Illegal path!");

            #endregion

            foreach (var GPXFile in Directory.EnumerateFiles(Path, "*.gpx"))
                LoadGPX(GPXFile, ref MapillaryInfo, OnDupliateTimestamp);

            return MapillaryInfo;

        }

        #endregion

        #region LoadGPXs(this Paths, OnDupliateTimestamp = null)

        public static IEnumerable<SharpMapillaryInfo> LoadGPXs(this IEnumerable<String>                          Paths,
                                                               Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp = null)
        {
            return Paths.Select(v => LoadGPXs(v, null, OnDupliateTimestamp));
        }

        #endregion

        #region LoadGPXs(this MapillaryInfos, OnDupliateTimestamp = null)

        public static IEnumerable<SharpMapillaryInfo> LoadGPXs(this IEnumerable<SharpMapillaryInfo>              MapillaryInfos,
                                                               Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp = null)
        {
            return MapillaryInfos.Select(v => LoadGPXs(v.FilePath, v, OnDupliateTimestamp));
        }

        #endregion


        #region LoadGPX(this GPXFile, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadGPX(this String                                       GPXFile,
                                                 Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp = null)
        {

            var Mapillary = new SharpMapillaryInfo(GPXFile.Substring(0, GPXFile.LastIndexOf(Path.DirectorySeparatorChar)));

            return LoadGPX(GPXFile, ref Mapillary, OnDupliateTimestamp);

        }

        #endregion

        #region LoadGPX(this MapillaryInfo, GPXFile, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadGPX(this SharpMapillaryInfo                           MapillaryInfo,
                                                 String                                            GPXFile,
                                                 Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp = null)
        {
            return LoadGPX(GPXFile, ref MapillaryInfo, OnDupliateTimestamp);
        }

        #endregion

        #region LoadGPX(GPXFile, ref MapillaryInfo, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadGPX(String                                            GPXFile,
                                                 ref SharpMapillaryInfo                            MapillaryInfo,
                                                 Action<String, DateTime, Double, Double, Double>  OnDupliateTimestamp = null)
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
                MapillaryInfo = new SharpMapillaryInfo(GPXFile.Substring(0, GPXFile.LastIndexOf(Path.DirectorySeparatorChar)));

            #endregion

            #region Init...

            XNamespace      NS              = "http://www.topografix.com/GPX/1/1";
            GPSInfo         GPSInfoElement  = null;

            DateTime        Timestamp;
            Double          Latitude;
            Double          Longitude;
            Double          Altitude;

            #endregion

            try
            {

                foreach (var GPSTrackPoint in XDocument.Load(GPXFile).
                                                             Root.
                                                             Elements(NS + "trk").
                                                             Elements(NS + "trkseg").
                                                             Elements(NS + "trkpt"))

                {

                    Timestamp  = DateTime.Parse(GPSTrackPoint.Element(NS + "time").Value).ToUniversalTime();
                    Latitude   = Double.  Parse(GPSTrackPoint.Attribute("lat").Value);
                    Longitude  = Double.  Parse(GPSTrackPoint.Attribute("lon").Value);
                    Altitude   = Double.  Parse(GPSTrackPoint.Element(NS + "ele").Value);

                    MapillaryInfo.NumberOfGPSPoints++;

                    if (!MapillaryInfo.GPSData.TryGetValue(Timestamp, out GPSInfoElement))
                        MapillaryInfo.GPSData.Add(Timestamp, new GPSInfo(Timestamp,
                                                                         Latitude,
                                                                         Longitude,
                                                                         Altitude));

                    else
                    {

                        MapillaryInfo.NumberOfDuplicateGPSTimestamps++;

                        if (OnDupliateTimestamp != null)
                            OnDupliateTimestamp(GPXFile, Timestamp, Latitude, Longitude, Altitude);

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
