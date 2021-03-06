﻿/*
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

using ExifLibrary;
using System.Threading.Tasks;
using System.Threading;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        //ToDo: TimeOffset -> Int16 to support values < 0!

        private static readonly Object SharpMapillaryLock = new Object();

        #region LoadJPEGs(this Path, SearchOption = SearchOption.TopDirectoryOnly, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnProcessed = null, OnDupliateTimestamp = null, ParallelOptions = null, OnResult = null)

        public static SharpMapillaryInfo LoadJPEGs(this String                               Path,
                                                   SearchOption                              SearchOption         = SearchOption.TopDirectoryOnly,
                                                   DateTimeKind                              DateTimeType         = DateTimeKind.Utc,
                                                   Int32?                                    TimeOffset           = null,
                                                   Action<UInt32, UInt32, Double>            OnProcessed          = null,
                                                   Func<String, DateTime, DateTime>          OnDupliateTimestamp  = null,
                                                   ParallelOptions                           ParallelOptions      = null,
                                                   Action<DateTime, DateTime, DateTimeKind>  OnResult             = null)

        {
            return LoadJPEGs(Path, SearchOption, null, DateTimeType, TimeOffset, OnProcessed, OnDupliateTimestamp, ParallelOptions, OnResult);
        }

        #endregion

        #region LoadJPEGs(this MapillaryInfo, SearchOption = SearchOption.TopDirectoryOnly, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnProcessed = null, OnDupliateTimestamp = null, ParallelOptions = null, OnResult = null)

        public static SharpMapillaryInfo LoadJPEGs(this SharpMapillaryInfo                   MapillaryInfo,
                                                   SearchOption                              SearchOption         = SearchOption.TopDirectoryOnly,
                                                   DateTimeKind                              DateTimeType         = DateTimeKind.Utc,
                                                   Int32?                                    TimeOffset           = null,
                                                   Action<UInt32, UInt32, Double>            OnProcessed          = null,
                                                   Func<String, DateTime, DateTime>          OnDupliateTimestamp  = null,
                                                   ParallelOptions                           ParallelOptions      = null,
                                                   Action<DateTime, DateTime, DateTimeKind>  OnResult             = null)

        {
            return LoadJPEGs(MapillaryInfo.FilePath, SearchOption, MapillaryInfo, DateTimeType, TimeOffset, OnProcessed, OnDupliateTimestamp, ParallelOptions, OnResult);
        }

        #endregion

        #region LoadJPEGs(Path, SearchOption = SearchOption.TopDirectoryOnly, MapillaryInfo = null, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnProcessed = null, OnDupliateTimestamp = null, ParallelOptions = null, OnResult = null)

        public static SharpMapillaryInfo LoadJPEGs(String                                    Path,
                                                   SearchOption                              SearchOption         = SearchOption.TopDirectoryOnly,
                                                   SharpMapillaryInfo                        MapillaryInfo        = null,
                                                   DateTimeKind                              DateTimeType         = DateTimeKind.Utc,
                                                   Int32?                                    TimeOffset           = null,
                                                   Action<UInt32, UInt32, Double>            OnProcessed          = null,
                                                   Func<String, DateTime, DateTime>          OnDupliateTimestamp  = null,
                                                   ParallelOptions                           ParallelOptions      = null,
                                                   Action<DateTime, DateTime, DateTimeKind>  OnResult             = null,
                                                   Boolean                                   ParallelProcessing   = false)

        {

            #region Initial checks...

            if (Path == null)
                throw new ArgumentNullException("Illegal path!");

            #endregion

            #region Init...

            var AllJPegs                = Directory.EnumerateFiles(Path, "*.JPG", SearchOption).OrderBy(a => a).ToArray();
            var NumberOfJPEGsFound      = (UInt32) AllJPegs.Length;
            var NumberOfJPEGsProcessed  = 0;
            var OnProcessedLocal        = OnProcessed;

            var MinDateTime             = DateTime.MaxValue;
            var MaxDateTime             = DateTime.MinValue;

            #endregion

            if (ParallelProcessing)
            {

                Parallel.ForEach(AllJPegs,
                                 ParallelOptions != null ? ParallelOptions : new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                                 JPegFile => {

                                     LoadJPEG(JPegFile, ref MapillaryInfo, DateTimeType, TimeOffset, OnDupliateTimestamp);
                                     Interlocked.Increment(ref NumberOfJPEGsProcessed);

                                     OnProcessedLocal = OnProcessed;
                                     if (OnProcessedLocal != null)
                                         OnProcessedLocal(NumberOfJPEGsFound, (UInt32) NumberOfJPEGsProcessed, (Double) NumberOfJPEGsProcessed / (Double) NumberOfJPEGsFound * 100);

                                 });

            }

            else
            {

                foreach (var JPegFile in AllJPegs)
                {

                    LoadJPEG(JPegFile, ref MapillaryInfo, DateTimeType, TimeOffset, OnDupliateTimestamp);
                    Interlocked.Increment(ref NumberOfJPEGsProcessed);

                    OnProcessedLocal = OnProcessed;
                    if (OnProcessedLocal != null)
                        OnProcessedLocal(NumberOfJPEGsFound, (UInt32) NumberOfJPEGsProcessed, (Double) NumberOfJPEGsProcessed / (Double) NumberOfJPEGsFound * 100);

                }

            }

            #region Process OnResult-delegate...

            if (OnResult != null)
            {

                foreach (var ImageInfo in MapillaryInfo.Images.Values)
                {

                    if (ImageInfo.Timestamp.Value < MinDateTime)
                        MinDateTime = ImageInfo.Timestamp.Value;

                    if (ImageInfo.Timestamp.Value > MaxDateTime)
                        MaxDateTime = ImageInfo.Timestamp.Value;

                }

                OnResult(MinDateTime, MaxDateTime, MinDateTime.Kind);

            }

            #endregion

            return MapillaryInfo;

        }

        #endregion

        #region LoadJPEGs(this Paths, SearchOption = SearchOption.TopDirectoryOnly, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnProcessed = null, OnDupliateTimestamp = null, ParallelOptions = null, OnResult = null)

        public static IEnumerable<SharpMapillaryInfo> LoadJPEGs(this IEnumerable<String>                  Paths,
                                                                SearchOption                              SearchOption         = SearchOption.TopDirectoryOnly,
                                                                DateTimeKind                              DateTimeType         = DateTimeKind.Utc,
                                                                Int32?                                    TimeOffset           = null,
                                                                Action<UInt32, UInt32, Double>            OnProcessed          = null,
                                                                Func<String, DateTime, DateTime>          OnDupliateTimestamp  = null,
                                                                ParallelOptions                           ParallelOptions      = null,
                                                                Action<DateTime, DateTime, DateTimeKind>  OnResult             = null)

        {
            return Paths.Select(MapillaryInfo => LoadJPEGs(MapillaryInfo, SearchOption, null, DateTimeType, TimeOffset, OnProcessed, OnDupliateTimestamp, ParallelOptions, OnResult));
        }

        #endregion

        #region LoadJPEGs(this MapillaryInfos, SearchOption = SearchOption.TopDirectoryOnly, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnProcessed = null, OnDupliateTimestamp = null, ParallelOptions = null, OnResult = null)

        public static IEnumerable<SharpMapillaryInfo> LoadJPEGs(this IEnumerable<SharpMapillaryInfo>      MapillaryInfos,
                                                                SearchOption                              SearchOption         = SearchOption.TopDirectoryOnly,
                                                                DateTimeKind                              DateTimeType         = DateTimeKind.Utc,
                                                                Int32?                                    TimeOffset           = null,
                                                                Action<UInt32, UInt32, Double>            OnProcessed          = null,
                                                                Func<String, DateTime, DateTime>          OnDupliateTimestamp  = null,
                                                                ParallelOptions                           ParallelOptions      = null,
                                                                Action<DateTime, DateTime, DateTimeKind>  OnResult             = null)

        {
            return MapillaryInfos.Select(MapillaryInfo => LoadJPEGs(MapillaryInfo.FilePath, SearchOption, MapillaryInfo, DateTimeType, TimeOffset, OnProcessed, OnDupliateTimestamp, ParallelOptions, OnResult));
        }

        #endregion


        #region LoadJPEG(this JPEGFile, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadJPEG(this String                       JPEGFile,
                                                  DateTimeKind                      DateTimeType         = DateTimeKind.Utc,
                                                  Int32?                            TimeOffset           = null,
                                                  Func<String, DateTime, DateTime>  OnDupliateTimestamp  = null)

        {

            var Mapillary = new SharpMapillaryInfo(JPEGFile.Substring(0, JPEGFile.LastIndexOf(Path.DirectorySeparatorChar)));

            return LoadJPEG(JPEGFile, ref Mapillary, DateTimeType, TimeOffset, OnDupliateTimestamp);

        }

        #endregion

        #region LoadJPEG(this MapillaryInfo, JPEGFile, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadJPEG(this SharpMapillaryInfo           MapillaryInfo,
                                                  String                            JPEGFile,
                                                  DateTimeKind                      DateTimeType         = DateTimeKind.Utc,
                                                  Int32?                            TimeOffset           = null,
                                                  Func<String, DateTime, DateTime>  OnDupliateTimestamp  = null)

        {
            return LoadJPEG(JPEGFile, ref MapillaryInfo, DateTimeType, TimeOffset, OnDupliateTimestamp);
        }

        #endregion

        #region LoadJPEG(JPEGFile, ref MapillaryInfo, DateTimeType = DateTimeKind.Utc, TimeOffset = null, OnDupliateTimestamp = null)

        public static SharpMapillaryInfo LoadJPEG(String                            JPEGFile,
                                                  ref SharpMapillaryInfo            MapillaryInfo,
                                                  DateTimeKind                      DateTimeType         = DateTimeKind.Utc,
                                                  Int32?                            TimeOffset           = null,
                                                  Func<String, DateTime, DateTime>  OnDupliateTimestamp  = null)

        {

            #region Initial checks...

            if (JPEGFile == null)
                throw new ArgumentNullException("Illegal path!");

            if (MapillaryInfo == null)
                lock (SharpMapillaryLock)
                {
                    MapillaryInfo = new SharpMapillaryInfo(JPEGFile.Substring(0, JPEGFile.LastIndexOf(Path.DirectorySeparatorChar)));
                }

            #endregion

            #region Init...

            ImageFile               EXIFFile        = null;
            ImageEXIFInfo           MapillaryImage  = null;

            var                     Timestamp       = DateTime.MinValue;
            var                     Latitude        = 0.0;
            var                     Longitude       = 0.0;
            var                     Altitude        = 0.0;
            var                     Direction       = 0.0;

            MathEx.UFraction32[]    EXIF_Latitude;
            MathEx.UFraction32[]    EXIF_Longitude;
            DMSLatitudeType         EXIF_LatitudeO;
            DMSLongitudeType        EXIF_LongitudeO;

            #endregion

            DateTimeType = DateTimeKind.Local;

            try
            {

                EXIFFile                = ImageFile.FromFile(JPEGFile);

                if (EXIFFile.Properties.ContainsKey(ExifTag.DateTime))
                {

                    Timestamp = DateTime.SpecifyKind((DateTime) EXIFFile.Properties[ExifTag.DateTime].Value, DateTimeType).ToUniversalTime();

                    if (TimeOffset.HasValue)
                    {
                        if (TimeOffset.Value > 0)
                            Timestamp += TimeSpan.FromSeconds(TimeOffset.Value);
                        else
                            Timestamp -= TimeSpan.FromSeconds(-TimeOffset.Value);
                    }

                }

                //if (EXIFFile.Properties.ContainsKey(ExifTag.GPSLatitude))
                //{
                //    EXIF_Latitude       = (MathEx.UFraction32[]) EXIFFile.Properties[ExifTag.GPSLatitude].   Value;
                //    EXIF_LatitudeO      = (DMSLatitudeType)         EXIFFile.Properties[ExifTag.GPSLatitudeRef].Value; //ToDo: Wrong cast!
                //    Latitude            = SharpMapillary.ToLatitude(EXIF_Latitude[0].Numerator / (Double) EXIF_Latitude[0].Denominator,
                //                                                    EXIF_Latitude[1].Numerator / (Double) EXIF_Latitude[1].Denominator,
                //                                                    EXIF_Latitude[2].Numerator / (Double) EXIF_Latitude[2].Denominator,
                //                                                    EXIF_LatitudeO);
                //}

                //if (EXIFFile.Properties.ContainsKey(ExifTag.GPSLongitude))
                //{
                //    EXIF_Longitude      = (MathEx.UFraction32[]) EXIFFile.Properties[ExifTag.GPSLongitude].Value;
                //    EXIF_LongitudeO     = (DMSLongitudeType)        EXIFFile.Properties[ExifTag.GPSLongitudeRef].Value; //ToDo: Wrong cast!
                //    Longitude           = SharpMapillary.ToLongitude(EXIF_Longitude[0].Numerator / (Double) EXIF_Longitude[0].Denominator,
                //                                                     EXIF_Longitude[1].Numerator / (Double) EXIF_Longitude[1].Denominator,
                //                                                     EXIF_Longitude[2].Numerator / (Double) EXIF_Longitude[2].Denominator,
                //                                                     EXIF_LongitudeO);
                //}

                //if (EXIFFile.Properties.ContainsKey(ExifTag.GPSAltitude))
                //    Altitude            = (Double) EXIFFile.Properties[ExifTag.GPSAltitude].Value;

                //if (EXIFFile.Properties.ContainsKey(ExifTag.GPSImgDirection))
                //    Direction           = (Double) EXIFFile.Properties[ExifTag.GPSImgDirection].Value;


                lock (SharpMapillaryLock)
                {

                    MapillaryInfo.NumberOfImages++;

                    if (!MapillaryInfo.Images.TryGetValue(Timestamp, out MapillaryImage))
                        MapillaryInfo.Images.Add(Timestamp, new ImageEXIFInfo(JPEGFile,
                                                                              Timestamp,
                                                                              Latitude,
                                                                              Longitude,
                                                                              Altitude,
                                                                              Direction));

                    else
                    {

                        if (!MapillaryImage.Timestamp.HasValue)
                        {
                            MapillaryImage.FileName   = JPEGFile;
                            MapillaryImage.Timestamp  = Timestamp;
                            MapillaryImage.Latitude   = Latitude;
                            MapillaryImage.Longitude  = Longitude;
                            MapillaryImage.Altitude   = Altitude;
                        }

                        else
                        {

                            MapillaryInfo.NumberOfDuplicateEXIFTimestamps++;

                            if (OnDupliateTimestamp != null)
                            {

                                var FixedTimestamp = OnDupliateTimestamp(JPEGFile, Timestamp);

                                MapillaryInfo.Images.Add(FixedTimestamp, new ImageEXIFInfo(JPEGFile,
                                                                                           FixedTimestamp,
                                                                                           Latitude,
                                                                                           Longitude,
                                                                                           Altitude,
                                                                                           Direction));

                            }

                        }

                    }

                }

            }
            catch (Exception e)
            {

                Console.WriteLine("There is something wrong in file: " + JPEGFile + Environment.NewLine + e.Message);
                Console.WriteLine("Moving it to the 'errors'-directory!");
                Directory.CreateDirectory(MapillaryInfo.FilePath + Path.DirectorySeparatorChar + "errors");

                File.Move(JPEGFile, MapillaryInfo.FilePath + Path.DirectorySeparatorChar + "errors" + Path.DirectorySeparatorChar + JPEGFile.Remove(0, JPEGFile.LastIndexOf(Path.DirectorySeparatorChar) + 1));

            }

            return MapillaryInfo;

        }

        #endregion

     }

}
