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

    public class MapillaryInfo
    {

        public String                                         FilePath          { get; private set; }
        public UInt32                                         NumberOfGPSPoints { get; set; }
        public UInt32                                         NumberOfImages    { get; set; }
        public SortedDictionary<DateTime, MapillaryImageInfo> Data              { get; private set; }
        public SortedDictionary<Double, UInt32>               DiffHistogram     { get; private set; }


        public MapillaryInfo(String FilePath)
        {
            this.FilePath       = FilePath;
            this.Data           = new SortedDictionary<DateTime, MapillaryImageInfo>();
            this.DiffHistogram  = new SortedDictionary<Double, UInt32>();
        }

    }

}
