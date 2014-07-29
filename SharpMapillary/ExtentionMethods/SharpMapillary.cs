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
using System.Linq;
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    public static partial class SharpMapillary
    {

        //private static int BinarySearch<T, TT>(this SortedList<T, TT> list, T value)
        //{
        //    if (list == null)
        //        throw new ArgumentNullException("list");
        //    var comp = Comparer<T>.Default;
        //    int lo = 0, hi = list.Length - 1;
        //    while (lo < hi)
        //    {
        //        int m = (hi + lo) / 2;  // this might overflow; be careful.
        //        if (comp(list[m], value) < 0) lo = m + 1;
        //        else hi = m - 1;
        //    }
        //    if (comp(list[lo], value) < 0) lo++;
        //    return lo;
        //}


        #region Start(params Paths)

        public static IEnumerable<String> Start(params String[] Paths)
        {
            return Paths;
        }

        #endregion

        #region Do(this MapillaryInfo)

        public static SharpMapillaryInfo Do(this SharpMapillaryInfo     MapillaryInfo,
                                       Action<SharpMapillaryInfo>  Action)
        {

            if (Action != null)
                Action(MapillaryInfo);

            return MapillaryInfo;

        }

        #endregion

        #region Do(this MapillaryInfos)

        public static IEnumerable<SharpMapillaryInfo> Do(this IEnumerable<SharpMapillaryInfo>  MapillaryInfos,
                                                    Action<SharpMapillaryInfo>            Action)
        {

            if (Action != null)
                return MapillaryInfos. Select(MapillaryInfo => {
                    Action(MapillaryInfo);
                    return MapillaryInfo;
                });

            return MapillaryInfos;

        }

        #endregion

    }

}
