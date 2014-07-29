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

    public class GPSInfo
    {

        #region Properties

        public DateTime?  Timestamp     { get; private set; }
        public Double     Latitude      { get; private set; }
        public Double     Longitude     { get; private set; }
        public Double     Altitude      { get; private set; }

        #endregion

        #region Constructor(s)

        #region GPSInfo()

        public GPSInfo(DateTime? Timestamp,
                       Double    Latitude,
                       Double    Longitude,
                       Double    Altitude)
        {

            this.Timestamp  = Timestamp;
            this.Latitude   = Latitude;
            this.Longitude  = Longitude;
            this.Altitude   = Altitude;

        }

        #endregion

        #endregion

    }

}
