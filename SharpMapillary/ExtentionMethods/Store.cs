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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

using ExifLibrary;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        #region Store(MapillaryInfos, SubDirectory = "fixed", SubDirectoryNoGPS = "noGPS")

        public static IEnumerable<SharpMapillaryInfo> Store(this IEnumerable<SharpMapillaryInfo>  MapillaryInfos,
                                                       String                           SubDirectory       = "fixed",
                                                       String                           SubDirectoryNoGPS  = "noGPS")
        {
            return MapillaryInfos.Select(MapillaryInfo => Store(ref MapillaryInfo, SubDirectory));
        }

        #endregion

        #region Store(this MapillaryInfo, SubDirectory = "fixed", SubDirectoryNoGPS = "noGPS")

        public static SharpMapillaryInfo Store(this SharpMapillaryInfo  MapillaryInfo,
                                          String              SubDirectory       = "fixed",
                                          String              SubDirectoryNoGPS  = "noGPS")
        {
            return Store(ref MapillaryInfo);
        }

        #endregion

        #region Store(ref MapillaryInfo, SubDirectory = "fixed", SubDirectoryNoGPS = "noGPS")

        public static SharpMapillaryInfo Store(ref SharpMapillaryInfo  MapillaryInfo,
                                          String             SubDirectory       = "fixed",
                                          String             SubDirectoryNoGPS  = "noGPS")
        {

            ImageFile EXIFFile      = null;
            DMS       LatitudeDMS   = null;
            DMS       LongitudeDMS  = null;
            Bitmap    newImage, oldImage;
            Graphics  g;

            Directory.CreateDirectory(MapillaryInfo.FilePath + Path.DirectorySeparatorChar + SubDirectory);

            foreach (var ImageInfo in MapillaryInfo.Data.Values)
            {

                // == is an image!
                if (ImageInfo.FileName != null &&
                    ImageInfo.Timestamp.HasValue)
                {

                    #region Valid GPS data...

                    if (ImageInfo.NoValidGPSFound.HasValue &&
                        ImageInfo.NoValidGPSFound.Value == false &&
                        ImageInfo.Latitude. HasValue &&
                        ImageInfo.Longitude.HasValue &&
                        ImageInfo.Altitude. HasValue)
                    {

                        EXIFFile = ImageFile.FromFile(ImageInfo.FileName);

                        #region Set/update EXIF data...

                        LatitudeDMS  = ImageInfo.Latitude. Value.ToDMSLat();
                        LongitudeDMS = ImageInfo.Longitude.Value.ToDMSLng();

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.GPSLatitude))
                            EXIFFile.Properties.Set(ExifTag.GPSLatitude, LatitudeDMS.Degree, LatitudeDMS.Minute, LatitudeDMS.Second);

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.GPSLatitudeRef))
                            EXIFFile.Properties.Set(ExifTag.GPSLatitudeRef, ImageInfo.Latitude > 0 ? GPSLatitudeRef.North : GPSLatitudeRef.South);

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.GPSLongitude))
                            EXIFFile.Properties.Set(ExifTag.GPSLongitude, LongitudeDMS.Degree, LongitudeDMS.Minute, LongitudeDMS.Second);

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.GPSLongitudeRef))
                            EXIFFile.Properties.Set(ExifTag.GPSLongitudeRef, ImageInfo.Longitude > 0 ? GPSLongitudeRef.East : GPSLongitudeRef.West);

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.GPSAltitude))
                            EXIFFile.Properties.Set(ExifTag.GPSAltitude, ImageInfo.Altitude.Value);

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.GPSAltitudeRef))
                            EXIFFile.Properties.Set(ExifTag.GPSAltitudeRef, GPSAltitudeRef.AboveSeaLevel);

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.GPSImgDirection))
                            EXIFFile.Properties.Add(new ExifURational(ExifTag.GPSImgDirection, new MathEx.UFraction32(ImageInfo.ViewingDirection)));



                        if (!EXIFFile.Properties.ContainsKey(ExifTag.Artist))
                            EXIFFile.Properties.Set(ExifTag.Artist, "Achim 'ahzf' Friedland <achim@graphdefined.org>");

                        //// Used by GoPro
                        //if (!EXIFFile.Properties.ContainsKey(ExifTag.ImageDescription))
                        //    EXIFFile.Properties.Set(ExifTag.ImageDescription, "ImageDescription");

                        //// Used by GoPro
                        //if (!EXIFFile.Properties.ContainsKey(ExifTag.Software))
                        //    EXIFFile.Properties.Set(ExifTag.Software, "Software");

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.UserComment))
                            EXIFFile.Properties.Set(ExifTag.UserComment, "Mapillary");

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.WindowsAuthor))
                            EXIFFile.Properties.Set(ExifTag.WindowsAuthor, "Achim 'ahzf' Friedland <achim@graphdefined.org>");

                        //if (!EXIFFile.Properties.ContainsKey(ExifTag.WindowsComment))
                        //    EXIFFile.Properties.Set(ExifTag.WindowsComment, "WindowsComment");

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.WindowsKeywords))
                            EXIFFile.Properties.Set(ExifTag.WindowsKeywords, "Mapillary; Deutschland; Thüringen; Jena; GPSLinearInterpolation");

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.WindowsSubject))
                            EXIFFile.Properties.Set(ExifTag.WindowsSubject, "Mapillary");

                        //if (!EXIFFile.Properties.ContainsKey(ExifTag.WindowsTitle))
                        //    EXIFFile.Properties.Set(ExifTag.WindowsTitle, "WindowsTitle");

                        if (!EXIFFile.Properties.ContainsKey(ExifTag.Copyright))
                            EXIFFile.Properties.Set(ExifTag.Copyright, "Creative Commons Attribution-NonCommercial 4.0 International License (CC BY-NC)");

                        //if (!EXIFFile.Properties.ContainsKey(ExifTag.ImageUniqueID))
                        //    EXIFFile.Properties.Set(ExifTag.ImageUniqueID, "4711");

                        #endregion

                        #region Store EXIF in memorystream...

                        var MS = new MemoryStream();

                        EXIFFile.Save(MS);

                        #endregion

                        #region Resize the image...

                        oldImage = new Bitmap(MS);
                        newImage = new Bitmap((Int32) MapillaryInfo.FinalImageWidth,
                                              (Int32) MapillaryInfo.FinalImageHeight);

                        g                     = Graphics.FromImage((Image) newImage);
                        g.InterpolationMode   = InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality  = CompositingQuality.HighQuality;
                        g.CompositingMode     = CompositingMode.SourceCopy;
                        g.DrawImage(oldImage,
                                    0, 0,
                                    (Int32) MapillaryInfo.FinalImageWidth,
                                    (Int32) MapillaryInfo.FinalImageHeight);

                        g.Dispose();

                        // Copy all metadata...
                        foreach (var PropertyItem in oldImage.PropertyItems)
                            newImage.SetPropertyItem(PropertyItem);

                        //// Get an ImageCodecInfo object that represents the JPEG codec.
                        //var imageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg);

                        //// Create an Encoder object for the Quality parameter.
                        //var encoder        = Encoder.Quality;

                        //// Create an EncoderParameters object. 
                        //EncoderParameters encoderParameters = new EncoderParameters(1);

                        //// Save the image as a JPEG file with quality level.
                        //EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                        //encoderParameters.Param[0] = encoderParameter;
                        //newImage.Save(filePath, imageCodecInfo, encoderParameters);

                        #endregion

                        #region Store resized image on disc...

                        newImage.Save(String.Concat(MapillaryInfo.FilePath,
                                                    Path.DirectorySeparatorChar,
                                                    SubDirectory,
                                                    Path.DirectorySeparatorChar,
                                                    ImageInfo.FileName.Remove(0, ImageInfo.FileName.LastIndexOf(Path.DirectorySeparatorChar) + 1)),
                                      ImageFormat.Jpeg);

                        newImage.Dispose();

                        #endregion

                    }

                    #endregion

                    #region ...no valid GPS data!

                    else
                    {

                        if (ImageInfo.FileName != null)
                        {

                            Directory.CreateDirectory(MapillaryInfo.FilePath + Path.DirectorySeparatorChar + SubDirectoryNoGPS);

                            File.Copy(ImageInfo.FileName,
                                      MapillaryInfo.FilePath +
                                      Path.DirectorySeparatorChar + SubDirectoryNoGPS +
                                      Path.DirectorySeparatorChar + ImageInfo.FileName.Remove(0, ImageInfo.FileName.LastIndexOf(Path.DirectorySeparatorChar) + 1));

                            MapillaryInfo.NumberOfImagesWithoutGPS++;

                        }

                    }

                    #endregion

                }

            }

            return MapillaryInfo;

        }

        #endregion

    }

}
