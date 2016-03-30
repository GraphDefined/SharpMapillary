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

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public class ImageEXIFInfo
    {

        #region Properties

        public String     FileName                  { get; set; }
        public DateTime?  Timestamp                 { get; set; }

        public Double     Image2GPS_TimeDifference  { get; set; }
        public Double?    Latitude                  { get; set; }
        public Double?    Longitude                 { get; set; }
        public Double?    Altitude                  { get; set; }
        public Double     ViewingDirection          { get; set; }

        public Boolean?   NoValidGPSFound           { get; set; }

        public String     Sequence                  { get; set; }

        #endregion

        #region Constructor(s)

        #region ImageEXIFInfo()

        public ImageEXIFInfo()
        {
        }

        #endregion

        #region ImageEXIFInfo(...)

        public ImageEXIFInfo(String     FileName,
                             DateTime?  Timestamp,
                             Double     Latitude,
                             Double     Longitude,
                             Double     Altitude,
                             Double     Direction)
        {

            this.FileName          = FileName;
            this.Timestamp         = Timestamp;
            this.Latitude          = Latitude;
            this.Longitude         = Longitude;
            this.Altitude          = Altitude;
            this.ViewingDirection  = Direction;

        }

        #endregion

        #endregion

    }

}
