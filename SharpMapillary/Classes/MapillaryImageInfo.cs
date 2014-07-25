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

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public class MapillaryImageInfo
    {

        #region Properties

        public DateTime?  GPS_Timestamp      { get; set; }
        public Double     GPS_Latitude       { get; set; }
        public Double     GPS_Longitude      { get; set; }
        public Double     GPS_Altitude       { get; set; }

        public String     Image_FileName     { get; set; }
        public DateTime?  Image_Timestamp    { get; set; }
        public Double     Image_Latitude     { get; set; }
        public Double     Image_Longitude    { get; set; }
        public Double     Image_Altitude     { get; set; }
        public Double     Image_Direction    { get; set; }

        public Double     Image2GPS_TimeDifference     { get; set; }
        public String     Sequence           { get; set; }

        #endregion

        #region Constructor(s)

        #region MapillaryImageInfo()

        public MapillaryImageInfo()
        {
        }

        #endregion

        #region MapillaryImageInfo(...)

        public MapillaryImageInfo(String     Image_FileName,
                                  DateTime?  Image_Timestamp,
                                  Double     Image_Latitude,
                                  Double     Image_Longitude,
                                  Double     Image_Altitude,
                                  Double     Image_Direction)
        {

            this.Image_FileName     = Image_FileName;
            this.Image_Timestamp    = Image_Timestamp;
            this.Image_Latitude     = Image_Latitude;
            this.Image_Longitude    = Image_Longitude;
            this.Image_Altitude     = Image_Altitude;
            this.Image_Direction    = Image_Direction;

        }

        #endregion

        #endregion

    }

}
