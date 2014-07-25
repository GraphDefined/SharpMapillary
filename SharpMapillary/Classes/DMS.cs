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
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public enum DMSLatitudeType
    {
        North,
        South
    }

    public enum DMSLongitudeType
    {
        West,
        East
    }


    /// <summary>
    /// http://www.gps-coordinates.net/gps-coordinates-converter
    /// </summary>
    public abstract class DMS
    {

        public Single   Degree { get; private set; }
        public Single   Minute { get; private set; }
        public Single   Second { get; private set; }

        public Single[] AsArray {
            get
            {
                return new Single[3] { Degree, Minute, Second };
            }
        }

        public DMS(Single Degree,
                   Single Minute,
                   Single Second)
        {

            this.Degree = Degree;
            this.Minute = Minute;
            this.Second = Second;

        }

        public DMS(Double Value)
        {

            // The whole units of degrees will remain the same (i.e. in 121.135° longitude, start with 121°).
            // Multiply the decimal by 60 (i.e. .135 * 60 = 8.1).
            // The whole number becomes the minutes (8').
            // Take the remaining decimal and multiply by 60. (i.e. .1 * 60 = 6).
            // The resulting number becomes the seconds (6"). Seconds can remain as a decimal.
            // Take your three sets of numbers and put them together, using the symbols for degrees (°), minutes (‘), and seconds (") (i.e. 121°8'6" longitude) 

                Value  = Math.Abs(Value);

              Degree = (Single) Math.Floor(Value);
            var Tmp1   = (Value - Degree) * 60;
              Minute = (Single) Math.Floor(Tmp1);
              Second = (Single) (Tmp1 - Minute) * 60;

        }


    }

    public class DMS_Lat : DMS
    {

        public DMSLatitudeType O { get; private set; }

        public DMS_Lat(Single           Degree,
                       Single           Minute,
                       Single           Second,
                       DMSLatitudeType  O)

            : base(Degree, Minute, Second)

        {
            this.O = O;
        }

        public DMS_Lat(Double Value)
            : base(Value)
        {
            this.O = (Value > 0) ? DMSLatitudeType.North : DMSLatitudeType.South;
        }

        public Double ToDegree()
        {
            return Degree + Minute / 60 + Second / 3600 * ((O == DMSLatitudeType.North) ? 1 : -1);
        }

    }

    public class DMS_Lng : DMS
    {

        public DMSLongitudeType O { get; private set; }

        public DMS_Lng(Single            Degree,
                       Single            Minute,
                       Single            Second,
                       DMSLongitudeType  O)

            : base(Degree, Minute, Second)

        {
            this.O = O;
        }

        public DMS_Lng(Double Value)
            : base(Value)
        {
            this.O = (Value > 0) ? DMSLongitudeType.East : DMSLongitudeType.West;
        }

        public Double ToDegree()
        {
            return Degree + Minute / 60 + Second / 3600 * ((O == DMSLongitudeType.East) ? 1 : -1);
        }

    }



    public static partial class SharpMapillary
    {

        public static DMS_Lat ToDMSLat(this Double Value)
        {
            return new DMS_Lat(Value);
        }

        public static DMS_Lng ToDMSLng(this Double Value)
        {
            return new DMS_Lng(Value);
        }


        public static Double ToLatitude(Double           Degree,
                                        Double           Minute,
                                        Double           Second,
                                        DMSLatitudeType  O)
        {
            return Degree + Minute / 60 + Second / 3600 * ((O == DMSLatitudeType.North) ? 1 : -1);
        }

        public static Double ToLongitude(Double Degree, Double Minute, Double Second, DMSLongitudeType O)
        {
            return Degree + Minute / 60 + Second / 3600 * ((O == DMSLongitudeType.East) ? 1 : -1);
        }

    }


}
