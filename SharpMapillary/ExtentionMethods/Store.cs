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
using System.Threading.Tasks;
using System.Threading;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        #region Store(MapillaryInfos, SubDirectory = "fixed", SubDirectoryNoGPS = "noGPS", OnProcessed = null, ParallelOptions = null)

        /// <summary>
        /// Store the given enumeration of SharpMapillaryInfos within the given subdirectory.
        /// </summary>
        /// <param name="MapillaryInfos">An enumeration of SharpMapillaryInfos.</param>
        /// <param name="SubDirectory">A subdirectory for storing the processed Mapillary images.</param>
        /// <param name="SubDirectoryNoGPS">A subdirectory for storing Mapillary images without valid GPS data.</param> </param>
        /// <param name="OnProcessed">An optional delegate to be invoked for every processed SharpMapillaryInfo.</param>
        /// <param name="ParallelOptions">Optional for controlling the multi-threading behaviour.</param>
        public static IEnumerable<SharpMapillaryInfo> Store(this IEnumerable<SharpMapillaryInfo>  MapillaryInfos,
                                                            String                                SubDirectory       = "fixed",
                                                            String                                SubDirectoryNoGPS  = "noGPS",
                                                            Action<UInt32, UInt32, Double>        OnProcessed        = null,
                                                            ParallelOptions                       ParallelOptions    = null)
        {

            if (MapillaryInfos == null)
                throw new ArgumentNullException("MapillaryInfos", "The given enumeration of MapillaryInfos must not be null!");

            return MapillaryInfos.Select(MapillaryInfo => Store(ref MapillaryInfo, SubDirectory, SubDirectoryNoGPS, OnProcessed, ParallelOptions));

        }

        #endregion

        #region Store(this MapillaryInfo, SubDirectory = "fixed", SubDirectoryNoGPS = "noGPS", OnProcessed = null, ParallelOptions = null)

        /// <summary>
        /// Store the given SharpMapillaryInfo within the given subdirectory.
        /// </summary>
        /// <param name="MapillaryInfo">A SharpMapillaryInfo data structure.</param>
        /// <param name="SubDirectory">A subdirectory for storing the processed Mapillary images.</param>
        /// <param name="SubDirectoryNoGPS">A subdirectory for storing Mapillary images without valid GPS data.</param> </param>
        /// <param name="OnProcessed">An optional delegate to be invoked for every processed SharpMapillaryInfo.</param>
        /// <param name="ParallelOptions">Optional for controlling the multi-threading behaviour.</param>
        public static SharpMapillaryInfo Store(this SharpMapillaryInfo         MapillaryInfo,
                                               String                          SubDirectory       = "fixed",
                                               String                          SubDirectoryNoGPS  = "noGPS",
                                               Action<UInt32, UInt32, Double>  OnProcessed        = null,
                                               ParallelOptions                 ParallelOptions    = null)
        {

            if (MapillaryInfo == null)
                throw new ArgumentNullException("MapillaryInfo", "The given parameter must not be null!");

            return Store(ref MapillaryInfo, SubDirectory, SubDirectoryNoGPS, OnProcessed, ParallelOptions);

        }

        #endregion

        #region Store(ref MapillaryInfo, SubDirectory = "fixed", SubDirectoryNoGPS = "noGPS", OnProcessed = null, ParallelOptions = null)

        /// <summary>
        /// Store the given SharpMapillaryInfo within the given subdirectory.
        /// </summary>
        /// <param name="MapillaryInfo">A SharpMapillaryInfo data structure.</param>
        /// <param name="SubDirectory">A subdirectory for storing the processed Mapillary images.</param>
        /// <param name="SubDirectoryNoGPS">A subdirectory for storing Mapillary images without valid GPS data.</param> </param>
        /// <param name="OnProcessed">An optional delegate to be invoked for every processed SharpMapillaryInfo.</param>
        /// <param name="ParallelOptions">Optional for controlling the multi-threading behaviour.</param>
        public static SharpMapillaryInfo Store(ref SharpMapillaryInfo          MapillaryInfo,
                                               String                          SubDirectory       = "fixed",
                                               String                          SubDirectoryNoGPS  = "noGPS",
                                               Action<UInt32, UInt32, Double>  OnProcessed        = null,
                                               ParallelOptions                 ParallelOptions    = null)
        {

            #region Data

            if (MapillaryInfo == null)
                throw new ArgumentNullException("MapillaryInfo", "The given parameter must not be null!");

            // ref/out parameters are not allowed inside Parallel.ForEach!
            var _MapillaryInfo              = MapillaryInfo;

            var EXIFFile                    = new ThreadLocal<ImageFile>();
            var LatitudeDMS                 = new ThreadLocal<DMS>();
            var LongitudeDMS                = new ThreadLocal<DMS>();
            var newImage                    = new ThreadLocal<Bitmap>();
            var oldImage                    = new ThreadLocal<Bitmap>();
            var g                           = new ThreadLocal<Graphics>();
            var NewFilePath                 = new ThreadLocal<String>();

            var NumberOfJPEGs               = (UInt32) MapillaryInfo.Images.Count;
            var NumberOfJPEGsProcessed      = 0;
            var NumberOfImagesWithoutGPS    = 0;
            var OnProcessedLocal            = OnProcessed;

            #endregion

            Directory.CreateDirectory(MapillaryInfo.FilePath + Path.DirectorySeparatorChar + SubDirectory);

            Parallel.ForEach(MapillaryInfo.Images.Values,
                             ParallelOptions != null ? ParallelOptions : new ParallelOptions() { MaxDegreeOfParallelism = 8 },
                             ImageInfo => {

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

                        #region Load image and update EXIF metadata...

                        var MS = new MemoryStream();

                        try
                        {

                            EXIFFile.Value = ImageFile.FromFile(ImageInfo.FileName);

                            #region Set/update EXIF data...

                            LatitudeDMS. Value  = ImageInfo.Latitude. Value.ToDMSLat();
                            LongitudeDMS.Value  = ImageInfo.Longitude.Value.ToDMSLng();

                            //if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.GPSLatitude))
                                EXIFFile.Value.Properties.Set(ExifTag.GPSLatitude, LatitudeDMS.Value.Degree, LatitudeDMS.Value.Minute, LatitudeDMS.Value.Second);

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.GPSLatitudeRef))
                                EXIFFile.Value.Properties.Set(ExifTag.GPSLatitudeRef, ImageInfo.Latitude > 0 ? GPSLatitudeRef.North : GPSLatitudeRef.South);

                            //if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.GPSLongitude))
                                EXIFFile.Value.Properties.Set(ExifTag.GPSLongitude, LongitudeDMS.Value.Degree, LongitudeDMS.Value.Minute, LongitudeDMS.Value.Second);

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.GPSLongitudeRef))
                                EXIFFile.Value.Properties.Set(ExifTag.GPSLongitudeRef, ImageInfo.Longitude > 0 ? GPSLongitudeRef.East : GPSLongitudeRef.West);

                            //if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.GPSAltitude))
                                EXIFFile.Value.Properties.Set(ExifTag.GPSAltitude, ImageInfo.Altitude.Value);

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.GPSAltitudeRef))
                                EXIFFile.Value.Properties.Set(ExifTag.GPSAltitudeRef, GPSAltitudeRef.AboveSeaLevel);

                            //if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.GPSImgDirection))
                                EXIFFile.Value.Properties.Add(new ExifURational(ExifTag.GPSImgDirection, new MathEx.UFraction32(ImageInfo.ViewingDirection)));



                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.Artist))
                                EXIFFile.Value.Properties.Set(ExifTag.Artist, "Achim 'ahzf' Friedland <achim@graphdefined.org>");

                            //// Used by GoPro
                            //if (!EXIFFile.Properties.ContainsKey(ExifTag.ImageDescription))
                            //    EXIFFile.Properties.Set(ExifTag.ImageDescription, "ImageDescription");

                            //// Used by GoPro
                            //if (!EXIFFile.Properties.ContainsKey(ExifTag.Software))
                            //    EXIFFile.Properties.Set(ExifTag.Software, "Software");

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.UserComment))
                                EXIFFile.Value.Properties.Set(ExifTag.UserComment, "Mapillary");

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.WindowsAuthor))
                                EXIFFile.Value.Properties.Set(ExifTag.WindowsAuthor, "Achim 'ahzf' Friedland <achim@graphdefined.org>");

                            //if (!EXIFFile.Properties.ContainsKey(ExifTag.WindowsComment))
                            //    EXIFFile.Properties.Set(ExifTag.WindowsComment, "WindowsComment");

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.WindowsKeywords))
                                EXIFFile.Value.Properties.Set(ExifTag.WindowsKeywords, "Mapillary; Deutschland; Thüringen; Jena; GPSLinearInterpolation");

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.WindowsSubject))
                                EXIFFile.Value.Properties.Set(ExifTag.WindowsSubject, "Mapillary");

                            //if (!EXIFFile.Properties.ContainsKey(ExifTag.WindowsTitle))
                            //    EXIFFile.Properties.Set(ExifTag.WindowsTitle, "WindowsTitle");

                            if (!EXIFFile.Value.Properties.ContainsKey(ExifTag.Copyright))
                                EXIFFile.Value.Properties.Set(ExifTag.Copyright, "Creative Commons Attribution-NonCommercial 4.0 International License (CC BY-NC)");

                            //if (!EXIFFile.Properties.ContainsKey(ExifTag.ImageUniqueID))
                            //    EXIFFile.Properties.Set(ExifTag.ImageUniqueID, "4711");

                            #endregion

                            EXIFFile.Value.Save(MS);

                        }

                        catch (Exception e)
                        {
                            Console.WriteLine("Exception during 'Load image and update EXIF metadata': " + e.Message);
                        }

                        #endregion

                        #region Resize and store the image...

                        try
                        {

                            #region Load Image from memory stream and resize it...

                            oldImage.Value = new Bitmap(MS);
                            newImage.Value = new Bitmap((Int32)_MapillaryInfo.FinalImageWidth,
                                                        (Int32)_MapillaryInfo.FinalImageHeight);

                            g.Value = Graphics.FromImage((Image)newImage.Value);
                            g.Value.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.Value.CompositingQuality = CompositingQuality.HighQuality;
                            g.Value.CompositingMode = CompositingMode.SourceCopy;
                            g.Value.DrawImage(oldImage.Value,
                                              0, 0,
                                              (Int32)_MapillaryInfo.FinalImageWidth,
                                              (Int32)_MapillaryInfo.FinalImageHeight);

                            g.Value.Dispose();

                            #endregion

                            #region Copy EXIF metadata

                            // Copy all metadata...
                            foreach (var PropertyItem in oldImage.Value.PropertyItems)
                                newImage.Value.SetPropertyItem(PropertyItem);

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

                            // Generate new file name including a possible subdirectory...
                            NewFilePath.Value = String.Concat(_MapillaryInfo.FilePath,
                                                              Path.DirectorySeparatorChar,
                                                              SubDirectory,
                                                              ImageInfo.FileName.Replace(_MapillaryInfo.FilePath, ""));

                            // Check if the possible subdirectory already exists... or create it!
                            Directory.CreateDirectory(NewFilePath.Value.Substring(0, NewFilePath.Value.LastIndexOf(Path.DirectorySeparatorChar)));

                            newImage.Value.Save(NewFilePath.Value,
                                                ImageFormat.Jpeg);

                            newImage.Value.Dispose();
                            MS.Dispose();

                            #endregion

                        }

                        catch (Exception e)
                        {
                            Console.WriteLine("Exception during 'Resize and store the image': " + e.Message);
                        }

                        #endregion

                        Interlocked.Increment(ref NumberOfJPEGsProcessed);

                        OnProcessedLocal = OnProcessed;
                        if (OnProcessedLocal != null)
                            OnProcessedLocal(NumberOfJPEGs, (UInt32) NumberOfJPEGsProcessed, (Double) NumberOfJPEGsProcessed / (Double) NumberOfJPEGs * 100);

                    }

                    #endregion

                    #region ...no valid GPS data!

                    else
                    {

                        if (ImageInfo.FileName != null)
                        {

                            Directory.CreateDirectory(_MapillaryInfo.FilePath + Path.DirectorySeparatorChar + SubDirectoryNoGPS);

                            File.Copy(ImageInfo.FileName,
                                      _MapillaryInfo.FilePath +
                                      Path.DirectorySeparatorChar + SubDirectoryNoGPS +
                                      Path.DirectorySeparatorChar + ImageInfo.FileName.Remove(0, ImageInfo.FileName.LastIndexOf(Path.DirectorySeparatorChar) + 1));

                            Interlocked.Increment(ref NumberOfImagesWithoutGPS);

                        }

                    }

                    #endregion

                }

            });

            _MapillaryInfo.NumberOfImagesWithoutGPS = (UInt32) NumberOfImagesWithoutGPS;

            return _MapillaryInfo;

        }

        #endregion

    }

}
