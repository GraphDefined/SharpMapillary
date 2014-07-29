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
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        #region SyncGPS(MapillaryInfos, GPSInterpolation = GPSInterpolation.LinearInterpolation, DirectionOffset = 0.0)

        public static IEnumerable<SharpMapillaryInfo> SyncGPS(this IEnumerable<SharpMapillaryInfo>  MapillaryInfos,
                                                              GPSInterpolation                      GPSInterpolation = GPSInterpolation.LinearInterpolation,
                                                              Double                                DirectionOffset  = 0.0)
        {
            return MapillaryInfos.Select(MapillaryInfo => SyncGPS(ref MapillaryInfo, GPSInterpolation, DirectionOffset));
        }

        #endregion

        #region SyncGPS(this MapillaryInfo, GPSInterpolation = GPSInterpolation.LinearInterpolation, DirectionOffset = 0.0)

        public static SharpMapillaryInfo SyncGPS(this SharpMapillaryInfo  MapillaryInfo,
                                                 GPSInterpolation         GPSInterpolation  = GPSInterpolation.LinearInterpolation,
                                                 Double                   DirectionOffset   = 0.0)
        {
            return SyncGPS(ref MapillaryInfo, GPSInterpolation, DirectionOffset);
        }

        #endregion

        #region SyncGPS(ref MapillaryInfo, GPSInterpolation = GPSInterpolation.LinearInterpolation, DirectionOffset = 0.0)

        public static SharpMapillaryInfo SyncGPS(ref SharpMapillaryInfo  MapillaryInfo,
                                                 GPSInterpolation        GPSInterpolation  = GPSInterpolation.LinearInterpolation,
                                                 Double                  DirectionOffset   = 0.0)
        {

            #region Data

            //var Ratio               = Math.Round((Double) GPXLookup.Data.Count() / (Double) NumberOfImages, 2);

            GPSInfo GPSEarly;
            GPSInfo GPSLate;
            GPSInfo NextGPSTrackpoint;

            var GPSEarlyTimestamp           = DateTime.MinValue;
            var GPSLateTimestamp            = DateTime.MinValue;
            var EarlyDiff                   = 0.0;
            var LateDiff                    = 0.0;
            var GPSTimeRange                = 0.0;
            var GPSEarly2Image_TimeOffset   = 0.0;
            var dx                          = 0.0;
            var dy                          = 0.0;

            //var GPXArray = GPXLookup.Keys.ToArray();
            //var GPXList  = GPXLookup.Keys.ToList();

            #endregion

            foreach (var ImageInfo in MapillaryInfo.Data.Values)
            {

                // == is an image!
                if (ImageInfo.FileName != null &&
                    ImageInfo.Timestamp.HasValue)
                {

                    // ToDo: Currently a bit inefficient!
                    GPSEarly                  = MapillaryInfo.GPSData.Values.Where(v => ImageInfo.Timestamp >= v.Timestamp).LastOrDefault();
                    GPSLate                   = MapillaryInfo.GPSData.Values.Where(v => ImageInfo.Timestamp <= v.Timestamp).FirstOrDefault();
                    //var GPXIndex               = GPXList.BinarySearch(timestamp);
                    //LateTimestamp              = (GPXIndex >= 0) ? GPXArray[GPXIndex] : GPXArray[-GPXIndex - 1]; // Returns the next timestamp if no exact match was found!

                    // Ignore GPS-less images without GPS coordinates
                    // before and after the image was taken!
                    if (GPSEarly != null && GPSLate != null)
                    {

                        GPSEarlyTimestamp                   = GPSEarly != null ? GPSEarly.Timestamp.Value : DateTime.MinValue;
                        GPSLateTimestamp                    = GPSLate  != null ? GPSLate. Timestamp.Value : DateTime.MaxValue;

                        EarlyDiff                           = (ImageInfo.Timestamp.Value - GPSEarlyTimestamp)              .TotalSeconds;
                        LateDiff                            = (GPSLateTimestamp          - ImageInfo.Timestamp.Value).TotalSeconds;

                        ImageInfo.Image2GPS_TimeDifference  = (EarlyDiff < LateDiff) ? -EarlyDiff : LateDiff;

                        #region Update DiffHistogram

                        if (!MapillaryInfo.DiffHistogram.ContainsKey(ImageInfo.Image2GPS_TimeDifference))
                            MapillaryInfo.DiffHistogram.Add(ImageInfo.Image2GPS_TimeDifference, 1);

                        else
                            MapillaryInfo.DiffHistogram[ImageInfo.Image2GPS_TimeDifference]++;

                        #endregion

                        #region A image sharing its timestamp with a GPS coordinate!

                        if (ImageInfo.Image2GPS_TimeDifference == 0)
                        {

                            #region Set Lat/Lng/Alt

                            ImageInfo.Latitude    = GPSEarly.Latitude;
                            ImageInfo.Longitude   = GPSEarly.Longitude;
                            ImageInfo.Altitude    = GPSEarly.Altitude;

                            #endregion

                            #region Calculate image direction

                            // ToDo: Currently a bit inefficient!
                            NextGPSTrackpoint       = MapillaryInfo.GPSData.Values.Where(v => ImageInfo.Timestamp < v.Timestamp).FirstOrDefault();

                            if (NextGPSTrackpoint  != null)
                            {
                                dy                      = GPSEarly.Latitude - NextGPSTrackpoint.Latitude;
                                dx                      = Math.Cos(Math.PI / 180 * NextGPSTrackpoint.Latitude) * (NextGPSTrackpoint.Longitude - GPSEarly.Longitude);
                            }

                            #endregion

                        }

                        #endregion

                        else
                        {

                            #region GPSInterpolation

                            switch (GPSInterpolation)
                            {

                                case GPSInterpolation.NearestMatch:
                                    ImageInfo.Latitude         = (EarlyDiff < LateDiff) ? GPSEarly.Latitude  : GPSLate.Latitude;
                                    ImageInfo.Longitude        = (EarlyDiff < LateDiff) ? GPSEarly.Longitude : GPSLate.Longitude;
                                    ImageInfo.Altitude         = (EarlyDiff < LateDiff) ? GPSEarly.Altitude  : GPSLate.Altitude;
                                    break;

                                case GPSInterpolation.LinearInterpolation:
                                    GPSTimeRange               = (GPSLateTimestamp - GPSEarlyTimestamp).TotalSeconds;
                                    GPSEarly2Image_TimeOffset  = (ImageInfo.Timestamp.Value - GPSEarlyTimestamp).TotalSeconds;
                                    ImageInfo.Latitude         = GPSEarly.Latitude  - (GPSEarly.Latitude  - GPSLate.Latitude)  / GPSTimeRange * GPSEarly2Image_TimeOffset;
                                    ImageInfo.Longitude        = GPSEarly.Longitude - (GPSEarly.Longitude - GPSLate.Longitude) / GPSTimeRange * GPSEarly2Image_TimeOffset;
                                    ImageInfo.Altitude         = GPSEarly.Altitude  - (GPSEarly.Altitude  - GPSLate.Altitude)  / GPSTimeRange * GPSEarly2Image_TimeOffset;
                                    break;

                            }

                            #endregion

                            #region Calculate image direction

                            dy                         = GPSEarly.Latitude - GPSLate.Latitude;
                            dx                         = Math.Cos(Math.PI / 180 * GPSLate.Latitude) * (GPSLate.Longitude - GPSEarly.Longitude);

                            #endregion

                        }

                        #region Calculate image direction

                        ImageInfo.ViewingDirection = (90 + Math.Atan2(dy, dx) * 180 / Math.PI + DirectionOffset) % 360;

                        if (ImageInfo.ViewingDirection < 0)
                            ImageInfo.ViewingDirection += 360;

                        #endregion

                        ImageInfo.NoValidGPSFound = false;

                    }

                    else
                    {
                        ImageInfo.NoValidGPSFound = true;
                        MapillaryInfo.NumberOfImagesWithoutGPS++;
                    }

                }

            }

            #region Store DiffHistogram

            using (var DiffHistogramFile = new StreamWriter(MapillaryInfo.FilePath + Path.DirectorySeparatorChar + @"DiffHistogram.txt"))
            {
                foreach (var KVPair in MapillaryInfo.DiffHistogram)
                    DiffHistogramFile.WriteLine(String.Concat(KVPair.Key, ";", KVPair.Value));
            }

            #endregion

            return MapillaryInfo;

        }

        #endregion

    }

}
