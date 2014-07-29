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

using ExifLibrary;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        //ToDo: Borken at the moment!
        public static void CreateGeoJSON(String FilePath, String Outfile)
        {

            using (var GeoJSONFile = new StreamWriter(FilePath + Path.DirectorySeparatorChar + Outfile, false))
            {

                GeoJSONFile.WriteLine(@"{");
                GeoJSONFile.WriteLine(@"""type"": ""FeatureCollection"",");
                GeoJSONFile.WriteLine(@"""features"": [");
                GeoJSONFile.WriteLine(@"{");
                GeoJSONFile.WriteLine(@"""type"": ""Feature"",");
                GeoJSONFile.WriteLine(@"""properties"": {},");
                GeoJSONFile.WriteLine(@"""geometry"": {");
                GeoJSONFile.WriteLine(@"""type"": ""LineString"",");
                GeoJSONFile.WriteLine(@"""coordinates"": [");

                ImageFile _ExifFile = null;

                GeoJSONFile.WriteLine(Directory.EnumerateFiles(FilePath, "*.JPG").
                    OrderBy(fn => fn).
                    Select(FileName => {

                        _ExifFile = ImageFile.FromFile(FileName);

                    if (_ExifFile.Properties.ContainsKey(ExifTag.GPSLatitude) &&
                        _ExifFile.Properties.ContainsKey(ExifTag.GPSLongitude))
                    {

                        //EXIFFile.Properties.Add(ExifTag.GPSImgDirection, new ExifURational(ExifTag.GPSImgDirection, new MathEx.UFraction32(0)));

                        var _lat  = (MathEx.UFraction32[]) _ExifFile.Properties[ExifTag.GPSLatitude].Value;
                        var _latO = (DMSLatitudeType)         _ExifFile.Properties[ExifTag.GPSLatitudeRef].Value;
                        var lat   = SharpMapillary.ToLatitude(_lat[0].Numerator / (Double) _lat[0].Denominator,
                                                              _lat[1].Numerator / (Double) _lat[1].Denominator,
                                                              _lat[2].Numerator / (Double) _lat[2].Denominator,
                                                              _latO);

                        var _lng  = ((MathEx.UFraction32[]) _ExifFile.Properties[ExifTag.GPSLongitude].Value);
                        var _lngO = (DMSLongitudeType)         _ExifFile.Properties[ExifTag.GPSLongitudeRef].Value;
                        var lng   = SharpMapillary.ToLongitude(_lng[0].Numerator / (Double) _lng[0].Denominator,
                                                               _lng[1].Numerator / (Double) _lng[1].Denominator,
                                                               _lng[2].Numerator / (Double) _lng[2].Denominator,
                                                               _lngO);

                        var alt = _ExifFile.Properties[ExifTag.GPSAltitude].Value;

                        var s = "[ " + lng.ToString() + ", " + lat.ToString() + " ]";

                        return s;

                    }

                    else
                    {
                        Console.WriteLine("Invalid file '" + FileName + "'!");
                        return null;
                    }

                }).
                Where(v => v != null).
                Aggregate((a, b) => a + ", " + Environment.NewLine + b));

                GeoJSONFile.WriteLine(@"]");
                GeoJSONFile.WriteLine(@"}");
                GeoJSONFile.WriteLine(@"}");
                GeoJSONFile.WriteLine(@"]");
                GeoJSONFile.WriteLine(@"}");

            }

        }

    }

}
