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
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public class SharpMapillaryInfo
    {

        public String                                         FilePath                          { get; private set; }
        public UInt32                                         NumberOfGPSPoints                 { get; set; }
        public UInt32                                         NumberOfImages                    { get; set; }
        public UInt32                                         NumberOfImagesWithoutGPS          { get; set; }
        public UInt32                                         NumberOfDuplicateGPSTimestamps    { get; set; }
        public UInt32                                         NumberOfDuplicateEXIFTimestamps   { get; set; }
        public SortedDictionary<DateTime, GPSInfo>            GPSData                           { get; private set; }
        public SortedDictionary<DateTime, ImageEXIFInfo>      Images                            { get; private set; }
        public SortedDictionary<Double, UInt32>               DiffHistogram                     { get; private set; }

        public UInt32?                                        FinalImageWidth                   { get; set; }
        public UInt32?                                        FinalImageHeight                  { get; set; }


        public SharpMapillaryInfo(String FilePath)
        {

            this.FilePath       = FilePath;
            this.GPSData        = new SortedDictionary<DateTime,GPSInfo>();
            this.Images           = new SortedDictionary<DateTime, ImageEXIFInfo>();
            this.DiffHistogram  = new SortedDictionary<Double, UInt32>();

        }

    }

}
