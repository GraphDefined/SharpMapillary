using ExifLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace org.GraphDefined.SharpMapillary.TimeShifter
{

    public class Program
    {

        public static void Main(String[] Arguments)
        {

            XNamespace NS               = "http://www.topografix.com/GPX/1/1";
            DateTime   Timestamp;
            Int32      TimeOffset       = 74;
            Double     Latitude;
            Double     Longitude;
            Double     Altitude;
            var        GPSData          = new SortedDictionary<DateTime, GPSInfo>();
            var        StartDirectory   = Environment.CurrentDirectory;

            foreach (var GPXFile in Directory.EnumerateFiles(StartDirectory, "*.gpx"))
                foreach (var GPSTrackPoint in XDocument.Load(GPXFile).
                                                             Root.
                                                             Elements(NS + "trk").
                                                             Elements(NS + "trkseg").
                                                             Elements(NS + "trkpt"))
                {

                    Timestamp = DateTime.Parse(GPSTrackPoint.Element(NS + "time").Value).ToUniversalTime();

                    if (TimeOffset > 0)
                        Timestamp += TimeSpan.FromSeconds(TimeOffset);
                    else
                        Timestamp -= TimeSpan.FromSeconds(-TimeOffset);

                    Latitude  = Double.Parse(GPSTrackPoint.Attribute ("lat").Value, CultureInfo.InvariantCulture);
                    Longitude = Double.Parse(GPSTrackPoint.Attribute ("lon").Value, CultureInfo.InvariantCulture);
                    Altitude  = Double.Parse(GPSTrackPoint.Element(NS+"ele").Value, CultureInfo.InvariantCulture);

                    if (!GPSData.ContainsKey(Timestamp))
                        GPSData.Add(Timestamp, new GPSInfo(Timestamp,
                                                           Latitude,
                                                           Longitude,
                                                           Altitude));

                }


            var NewDirectory = StartDirectory + Path.DirectorySeparatorChar + "fixed";
            Directory.CreateDirectory(NewDirectory);

            foreach (var JPEGFile in Directory.EnumerateFiles(StartDirectory, "*.jpg"))
            {

                try
                {

                    var JPEGImage = ImageFile.FromFile(JPEGFile);


                    #region Set/update EXIF data...

                    LatitudeDMS. Value = ImageInfo.Latitude. Value.ToDMSLat();
                    LongitudeDMS.Value = ImageInfo.Longitude.Value.ToDMSLng();

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

            }


        }

    }

}
