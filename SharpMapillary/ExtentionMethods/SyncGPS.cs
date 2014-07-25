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
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        #region SyncGPS(MapillaryInfos, GPSInterpolation = GPSInterpolation.LinearInterpolation, DirectionOffset = 0.0)

        public static IEnumerable<MapillaryInfo> SyncGPS(this IEnumerable<MapillaryInfo>  MapillaryInfos,
                                                         GPSInterpolation                 GPSInterpolation = GPSInterpolation.LinearInterpolation,
                                                         Double                           DirectionOffset  = 0.0)
        {
            return MapillaryInfos.Select(MapillaryInfo => SyncGPS(ref MapillaryInfo, GPSInterpolation, DirectionOffset));
        }

        #endregion

        #region SyncGPS(this MapillaryInfo, GPSInterpolation = GPSInterpolation.LinearInterpolation, DirectionOffset = 0.0)

        public static MapillaryInfo SyncGPS(this MapillaryInfo  MapillaryInfo,
                                            GPSInterpolation    GPSInterpolation  = GPSInterpolation.LinearInterpolation,
                                            Double              DirectionOffset   = 0.0)
        {
            return SyncGPS(ref MapillaryInfo, GPSInterpolation, DirectionOffset);
        }

        #endregion

        #region SyncGPS(ref MapillaryInfo, GPSInterpolation = GPSInterpolation.LinearInterpolation, DirectionOffset = 0.0)

        public static MapillaryInfo SyncGPS(ref MapillaryInfo  MapillaryInfo,
                                            GPSInterpolation   GPSInterpolation  = GPSInterpolation.LinearInterpolation,
                                            Double             DirectionOffset   = 0.0)
        {

            #region Data

            //var Ratio               = Math.Round((Double) GPXLookup.Data.Count() / (Double) NumberOfImages, 2);

            MapillaryImageInfo GPSEarly;
            MapillaryImageInfo GPSLate;
            MapillaryImageInfo NextGPSTrackpoint;

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

                if (ImageInfo.Image_Timestamp.HasValue)
                {

                    // ToDo: Currently inefficient!
                    GPSEarly                  = MapillaryInfo.Data.Values.Where(v => ImageInfo.Image_Timestamp >= v.GPS_Timestamp).LastOrDefault();
                    GPSLate                   = MapillaryInfo.Data.Values.Where(v => ImageInfo.Image_Timestamp <= v.GPS_Timestamp).FirstOrDefault();
                    //var GPXIndex               = GPXList.BinarySearch(timestamp);
                    //LateTimestamp              = (GPXIndex >= 0) ? GPXArray[GPXIndex] : GPXArray[-GPXIndex - 1]; // Returns the next timestamp if no exact match was found!

                    // Ignore GPS-less images without GPS coordinates
                    // before and after the image was taken!
                    if (GPSEarly != null && GPSLate != null)
                    {

                        GPSEarlyTimestamp                   = GPSEarly != null ? GPSEarly.GPS_Timestamp.Value : DateTime.MinValue;
                        GPSLateTimestamp                    = GPSLate  != null ? GPSLate. GPS_Timestamp.Value : DateTime.MaxValue;

                        if (GPSEarly == null || GPSLate == null)
                        {
                        }

                        EarlyDiff                           = (ImageInfo.Image_Timestamp.Value - GPSEarlyTimestamp)              .TotalSeconds;
                        LateDiff                            = (GPSLateTimestamp                - ImageInfo.Image_Timestamp.Value).TotalSeconds;

                        #region Update DiffHistogram

                        ImageInfo.Image2GPS_TimeDifference  = (EarlyDiff < LateDiff) ? -EarlyDiff : LateDiff;

                        if (!MapillaryInfo.DiffHistogram.ContainsKey(ImageInfo.Image2GPS_TimeDifference))
                            MapillaryInfo.DiffHistogram.Add(ImageInfo.Image2GPS_TimeDifference, 1);

                        else
                            MapillaryInfo.DiffHistogram[ImageInfo.Image2GPS_TimeDifference]++;

                        #endregion

                        #region A image sharing its timestamp with a GPS coordinate!

                        if (ImageInfo.Image2GPS_TimeDifference == 0)
                        {

                            #region Set Lat/Lng/Alt

                            ImageInfo.Image_Latitude    = ImageInfo.GPS_Latitude;
                            ImageInfo.Image_Longitude   = ImageInfo.GPS_Longitude;
                            ImageInfo.Image_Altitude    = ImageInfo.GPS_Altitude;

                            #endregion

                            #region Calculate image direction

                            // ToDo: Currently inefficient!
                            NextGPSTrackpoint       = MapillaryInfo.Data.Values.Where(v => ImageInfo.Image_Timestamp < v.GPS_Timestamp).FirstOrDefault();

                            if (NextGPSTrackpoint  != null)
                            {
                                dy                      = GPSEarly.GPS_Latitude - NextGPSTrackpoint.GPS_Latitude;
                                dx                      = Math.Cos(Math.PI / 180 * NextGPSTrackpoint.GPS_Latitude) * (NextGPSTrackpoint.GPS_Longitude - GPSEarly.GPS_Longitude);
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
                                    ImageInfo.Image_Latitude    = (EarlyDiff < LateDiff) ? GPSEarly.GPS_Latitude  : GPSLate.GPS_Latitude;
                                    ImageInfo.Image_Longitude   = (EarlyDiff < LateDiff) ? GPSEarly.GPS_Longitude : GPSLate.GPS_Longitude;
                                    ImageInfo.Image_Altitude    = (EarlyDiff < LateDiff) ? GPSEarly.GPS_Altitude  : GPSLate.GPS_Altitude;
                                    break;

                                case GPSInterpolation.LinearInterpolation:
                                    GPSTimeRange                = (GPSLateTimestamp - GPSEarlyTimestamp).TotalSeconds;
                                    GPSEarly2Image_TimeOffset   = (ImageInfo.Image_Timestamp.Value - GPSEarlyTimestamp).TotalSeconds;
                                    ImageInfo.Image_Latitude    = GPSEarly.GPS_Latitude  - (GPSEarly.GPS_Latitude  - GPSLate.GPS_Latitude)  / GPSTimeRange * GPSEarly2Image_TimeOffset;
                                    ImageInfo.Image_Longitude   = GPSEarly.GPS_Longitude - (GPSEarly.GPS_Longitude - GPSLate.GPS_Longitude) / GPSTimeRange * GPSEarly2Image_TimeOffset;
                                    ImageInfo.Image_Altitude    = GPSEarly.GPS_Altitude  - (GPSEarly.GPS_Altitude  - GPSLate.GPS_Altitude)  / GPSTimeRange * GPSEarly2Image_TimeOffset;
                                    break;

                            }

                            #endregion

                            #region Calculate image direction

                            dy                         = GPSEarly.GPS_Latitude - GPSLate.GPS_Latitude;
                            dx                         = Math.Cos(Math.PI / 180 * GPSLate.GPS_Latitude) * (GPSLate.GPS_Longitude - GPSEarly.GPS_Longitude);

                            #endregion

                        }

                        #region Calculate image direction

                        ImageInfo.Image_Direction = (90 + Math.Atan2(dy, dx) * 180 / Math.PI + DirectionOffset) % 360;

                        if (ImageInfo.Image_Direction < 0)
                            ImageInfo.Image_Direction += 360;

                        #endregion

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
