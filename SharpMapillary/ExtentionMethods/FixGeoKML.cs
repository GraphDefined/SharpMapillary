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

using ExifLibrary;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        public static void FixGeoKML(String FilePath)
        {

            Directory.CreateDirectory(FilePath + Path.DirectorySeparatorChar + "fixed");

            var doc = XDocument.Load(FilePath + "doc.kml");

            XNamespace NS = "http://earth.google.com/kml/2.1";

            foreach (var Placemark in doc.Root.Elements(NS + "Document").Elements(NS + "Folder").Where(xe => xe.Element(NS + "name").Value == "Photos").Elements(NS + "Placemark"))
            {

                // <Placemark xmlns="http://earth.google.com/kml/2.1">
                //   <name>G0018910.JPG</name> 
                //   <description>
                //     <![CDATA[ <img src='g0018910.jpg' width='600' height='450'/> ]]> 
                //   </description>
                //   <styleUrl>#defaultStyle1</styleUrl> 
                //   <Style>
                //     <IconStyle>
                //       <Icon>
                //         <href>thumbs/thumb_G0018910.JPG</href> 
                //       </Icon>
                //     </IconStyle>
                //   </Style>
                //   <Point>
                //     <coordinates>11.625668000,50.932349000,183.0</coordinates> 
                //   </Point>
                // </Placemark>

                var FileName        = Placemark.Element(NS + "name").Value;
                var Coordinates     = Placemark.Element(NS + "Point").Element(NS + "coordinates").Value.Split(new Char[] { ',' }).Select(v => Double.Parse(v)).ToArray();

                var _ExifFile       = ImageFile.FromFile(FilePath + FileName);

                var _lat            = ((MathEx.UFraction32[]) _ExifFile.Properties[ExifTag.GPSLatitude].Value);
                var _real_lat       = Coordinates[1].ToDMSLat();


                var a = _ExifFile.Properties[ExifTag.GPSLatitude] as GPSLatitudeLongitude;
                a.Degrees = new MathEx.UFraction32(_real_lat.Degree);
                a.Minutes = new MathEx.UFraction32(_real_lat.Minute);
                a.Seconds = new MathEx.UFraction32(_real_lat.Second);

                _ExifFile.Properties.Set(ExifTag.GPSLatitude, _real_lat.Degree, _real_lat.Minute, _real_lat.Second);


                var _lng            = ((MathEx.UFraction32[]) _ExifFile.Properties[ExifTag.GPSLongitude].Value);
                var _real_lng       = Coordinates[0].ToDMSLng();

                _ExifFile.Properties.Set(ExifTag.GPSLongitude, _real_lng.Degree, _real_lng.Minute, _real_lng.Second);


                if (!_ExifFile.Properties.ContainsKey(ExifTag.GPSImgDirection))
                    _ExifFile.Properties.Add(new ExifURational(ExifTag.GPSImgDirection, new MathEx.UFraction32(0)));

                // Save exif data with the image
                _ExifFile.Save(FilePath + Path.DirectorySeparatorChar + "fixed" + Path.DirectorySeparatorChar + FileName);

                var _ExifFile2 = ImageFile.FromFile(FilePath + FileName);
                var __lat = ((MathEx.UFraction32[]) _ExifFile2.Properties[ExifTag.GPSLatitude].Value);
                var __lng = ((MathEx.UFraction32[]) _ExifFile2.Properties[ExifTag.GPSLongitude].Value);

            }

        }

    }

}
