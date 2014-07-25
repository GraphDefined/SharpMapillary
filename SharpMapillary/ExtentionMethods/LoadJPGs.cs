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

using ExifLibrary;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        #region LoadJPGs(this Path, TimeOffset = null)

        public static MapillaryInfo LoadJPGs(this String  Path,
                                             TimeSpan?    TimeOffset = null)
        {
            return LoadJPGs(Path, null, TimeOffset);
        }

        #endregion

        #region LoadJPGs(this MapillaryInfo, TimeOffset = null)

        public static MapillaryInfo LoadJPGs(this MapillaryInfo  MapillaryInfo,
                                             TimeSpan?           TimeOffset = null)
        {
            return LoadJPGs(MapillaryInfo.FilePath, MapillaryInfo, TimeOffset);
        }

        #endregion

        #region LoadJPGs(Path, MapillaryInfo = null, TimeOffset = null)

        public static MapillaryInfo LoadJPGs(String         Path,
                                             MapillaryInfo  MapillaryInfo = null,
                                             TimeSpan?      TimeOffset    = null)
        {

            #region Initial checks...

            if (Path == null)
                throw new ArgumentNullException("Illegal path!");

            #endregion

            foreach (var JPGFile in Directory.EnumerateFiles(Path, "*.JPG"))
                LoadJPG(JPGFile, ref MapillaryInfo, TimeOffset);

            return MapillaryInfo;

        }

        #endregion

        #region LoadJPGs(this Paths, TimeOffset = null)

        public static IEnumerable<MapillaryInfo> LoadJPGs(this IEnumerable<String>  Paths,
                                                          TimeSpan?                 TimeOffset = null)
        {
            return Paths.Select(MapillaryInfo => LoadJPGs(MapillaryInfo, null, TimeOffset));
        }

        #endregion

        #region LoadJPGs(this MapillaryInfos, TimeOffset = null)

        public static IEnumerable<MapillaryInfo> LoadJPGs(this IEnumerable<MapillaryInfo>  MapillaryInfos,
                                                          TimeSpan?                        TimeOffset = null)
        {
            return MapillaryInfos.Select(MapillaryInfo => LoadJPGs(MapillaryInfo.FilePath, MapillaryInfo, TimeOffset));
        }

        #endregion


        #region LoadJPG(this JPGFile, TimeOffset = null)

        public static MapillaryInfo LoadJPG(this String  JPGFile,
                                            TimeSpan?    TimeOffset = null)
        {

            var Mapillary = new MapillaryInfo(JPGFile.Substring(0, JPGFile.LastIndexOf(Path.DirectorySeparatorChar)));

            return LoadJPG(JPGFile, ref Mapillary, TimeOffset);

        }

        #endregion

        #region LoadJPG(this MapillaryInfo, JPGFile, TimeOffset = null)

        public static MapillaryInfo LoadJPG(this MapillaryInfo  MapillaryInfo,
                                            String              JPGFile,
                                            TimeSpan?           TimeOffset = null)
        {
            return LoadJPG(JPGFile, ref MapillaryInfo, TimeOffset);
        }

        #endregion

        #region LoadJPG(JPGFile, ref MapillaryInfo, TimeOffset = null)

        public static MapillaryInfo LoadJPG(String             JPGFile,
                                            ref MapillaryInfo  MapillaryInfo,
                                            TimeSpan?          TimeOffset = null)
        {

            #region Initial checks...

            if (JPGFile == null)
                throw new ArgumentNullException("Illegal path!");

            if (MapillaryInfo == null)
                MapillaryInfo = new MapillaryInfo(JPGFile.Substring(0, JPGFile.LastIndexOf(Path.DirectorySeparatorChar)));

            #endregion

            #region Init...

            ImageFile               EXIFFile            = null;
            MapillaryImageInfo      MapillaryImage      = null;

            var                     Image_Timestamp     = DateTime.MinValue;
            var                     Image_Latitude      = 0.0;
            var                     Image_Longitude     = 0.0;
            var                     Image_Altitude      = 0.0;
            var                     Image_Direction     = 0.0;

            MathEx.UFraction32[]    EXIF_Latitude;
            MathEx.UFraction32[]    EXIF_Longitude;
            DMSLatitudeType            EXIF_LatitudeO;
            DMSLongitudeType           EXIF_LongitudeO;

            #endregion

            try
            {

                EXIFFile                = ImageFile.FromFile(JPGFile);

                if (EXIFFile.Properties.ContainsKey(ExifTag.GPSLatitude))
                {
                    EXIF_Latitude       = (MathEx.UFraction32[]) EXIFFile.Properties[ExifTag.GPSLatitude].   Value;
                    EXIF_LatitudeO      = (DMSLatitudeType)         EXIFFile.Properties[ExifTag.GPSLatitudeRef].Value; //ToDo: Wrong cast!
                    Image_Latitude      = SharpMapillary.ToLatitude(EXIF_Latitude[0].Numerator / (Double) EXIF_Latitude[0].Denominator,
                                                                    EXIF_Latitude[1].Numerator / (Double) EXIF_Latitude[1].Denominator,
                                                                    EXIF_Latitude[2].Numerator / (Double) EXIF_Latitude[2].Denominator,
                                                                    EXIF_LatitudeO);
                }

                if (EXIFFile.Properties.ContainsKey(ExifTag.GPSLongitude))
                {
                    EXIF_Longitude      = (MathEx.UFraction32[]) EXIFFile.Properties[ExifTag.GPSLongitude].Value;
                    EXIF_LongitudeO     = (DMSLongitudeType)        EXIFFile.Properties[ExifTag.GPSLongitudeRef].Value; //ToDo: Wrong cast!
                    Image_Longitude     = SharpMapillary.ToLongitude(EXIF_Longitude[0].Numerator / (Double) EXIF_Longitude[0].Denominator,
                                                                     EXIF_Longitude[1].Numerator / (Double) EXIF_Longitude[1].Denominator,
                                                                     EXIF_Longitude[2].Numerator / (Double) EXIF_Longitude[2].Denominator,
                                                                     EXIF_LongitudeO);
                }

                if (EXIFFile.Properties.ContainsKey(ExifTag.GPSAltitude))
                    Image_Altitude      = (Double) EXIFFile.Properties[ExifTag.GPSAltitude].Value;

                if (EXIFFile.Properties.ContainsKey(ExifTag.GPSImgDirection))
                    Image_Direction     = (Double) EXIFFile.Properties[ExifTag.GPSImgDirection].Value;

                if (EXIFFile.Properties.ContainsKey(ExifTag.DateTime))
                {

                    Image_Timestamp = DateTime.SpecifyKind((DateTime) EXIFFile.Properties[ExifTag.DateTime].Value, DateTimeKind.Utc);

                    if (TimeOffset.HasValue)
                        Image_Timestamp += TimeOffset.Value;

                }


                MapillaryInfo.NumberOfImages++;

                if (!MapillaryInfo.Data.TryGetValue(Image_Timestamp, out MapillaryImage))
                    MapillaryInfo.Data.Add(Image_Timestamp, new MapillaryImageInfo(JPGFile,
                                                                                   Image_Timestamp,
                                                                                   Image_Latitude,
                                                                                   Image_Longitude,
                                                                                   Image_Altitude,
                                                                                   Image_Direction));

                else
                {

                    if (!MapillaryImage.Image_Timestamp.HasValue)
                    {
                        MapillaryImage.Image_FileName   = JPGFile;
                        MapillaryImage.Image_Timestamp  = Image_Timestamp;
                        MapillaryImage.Image_Latitude   = Image_Latitude;
                        MapillaryImage.Image_Longitude  = Image_Longitude;
                        MapillaryImage.Image_Altitude   = Image_Altitude;
                    }

                    else
                        Console.WriteLine("Double EXIF timestamp: " + Image_Timestamp.ToString("s") + "Z" + " in image file: " + JPGFile);

                }

            }
            catch (Exception e)
            {

                Console.WriteLine("There is something wrong in file: " + JPGFile + Environment.NewLine + e.Message);
                Console.WriteLine("Moving it to the 'errors'-directory!");
                Directory.CreateDirectory(MapillaryInfo.FilePath + Path.DirectorySeparatorChar + "errors");

                File.Move(JPGFile, MapillaryInfo.FilePath + Path.DirectorySeparatorChar + "errors" + Path.DirectorySeparatorChar + JPGFile.Remove(0, JPGFile.LastIndexOf(Path.DirectorySeparatorChar) + 1));

            }

            return MapillaryInfo;

        }

        #endregion

     }

}
