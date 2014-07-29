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

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        #region ResizeImages(this MapillaryInfos, FinalWidth, FinalHeight)

        public static IEnumerable<SharpMapillaryInfo> ResizeImages(this IEnumerable<SharpMapillaryInfo>  MapillaryInfos,
                                                                   UInt32                                FinalWidth,
                                                                   UInt32                                FinalHeight)
        {
            return MapillaryInfos.Select(MapillaryInfo => MapillaryInfo.ResizeImages(FinalWidth, FinalHeight));
        }

        #endregion

        #region ResizeImages(this MapillaryInfo, FinalWidth, FinalHeight)

        public static SharpMapillaryInfo ResizeImages(this SharpMapillaryInfo  MapillaryInfo,
                                                      UInt32                   FinalWidth,
                                                      UInt32                   FinalHeight)
        {

            MapillaryInfo.FinalImageWidth  = FinalWidth;
            MapillaryInfo.FinalImageHeight = FinalHeight;

            return MapillaryInfo;

        }

        #endregion

    }

}
