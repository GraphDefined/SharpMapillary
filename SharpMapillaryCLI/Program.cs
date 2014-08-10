﻿/*
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
using System.Threading.Tasks;

#endregion

namespace org.GraphDefined.SharpMapillary
{

    /// <summary>
    /// SharpMapillary Command Line Edition.
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Main...
        /// </summary>
        /// <param name="Arguments">The CLI Arguments.</param>
        public static void Main(String[] Arguments)
        {

            #region Defaults...

            var StartDirectory       = Environment.CurrentDirectory;
            var ParallelReadOptions  = new ParallelOptions();
            var ParallelWriteOptions = new ParallelOptions() { MaxDegreeOfParallelism = 8 };

            #endregion

            #region Parse CLI Arguments...

            for (var i = 0; i < Arguments.Length; i++)
            {

                #region Parse --threads

                if (Arguments[i] == "--threads")
                {

                    ushort MaxDegreeOfParallelism = 8;

                    if (i + 1 > Arguments.Length ||
                        !(UInt16.TryParse(Arguments[i + 1], out MaxDegreeOfParallelism)))
                    {
                        Console.WriteLine("Could not parse '--threads' parameter!");
                        Environment.Exit(1);
                    }

                    i++;

                    Console.WriteLine("Using " + MaxDegreeOfParallelism + " threads...");

                    ParallelReadOptions. MaxDegreeOfParallelism = (Int32) MaxDegreeOfParallelism;
                    ParallelWriteOptions.MaxDegreeOfParallelism = (Int32) MaxDegreeOfParallelism;

                }

                #endregion

                #region Check if the last argument is a valid directory...

                else
                {

                    if (Directory.Exists(Arguments[i]))
                    {
                        StartDirectory = Arguments[i];
                    }
                    else
                    {
                        Console.WriteLine("Directory '" + Arguments[0] + " not found!");
                        Environment.Exit(1);
                    }

                    Console.WriteLine("Processing directory: " + StartDirectory);

                }

                #endregion

            }

            #endregion

            SharpMapillary.Start(StartDirectory).

                                 LoadGPXs(OnDupliateTimestamp: (GPXFile, Timestamp, lat, lng, alt) => Console.WriteLine("Duplicate GPS timestamp: " + Timestamp.ToString("s") + "Z" + " in GPX file: " + GPXFile)).
                                 Do(v => Console.WriteLine("Number of GPS trackpoints: " +           v.NumberOfGPSPoints)).
                                 Do(v => Console.WriteLine("Number of duplicate GPS timestamps: " +  v.NumberOfDuplicateGPSTimestamps)).

                                 LoadJPEGs(OnProcessed:         (Sum, Processed, Percentage)       => { if (Processed % 25 == 0) { Console.CursorLeft = 0; Console.Write(Percentage.ToString("0.00") + "% of " + Sum + " images loaded..."); } },
                                           OnDupliateTimestamp: (JPEGFile, Timestamp)              => Console.WriteLine("Duplicate EXIF timestamp: " + Timestamp.ToString("s") + "Z" + " in image file: " + JPEGFile)).
                                 // Ziegenhain2: 52
                                 // Jena-Ost3:   44
                                 Do(v => Console.WriteLine(Environment.NewLine +
                                                           "Number of duplicate EXIF timestamps: " + v.NumberOfDuplicateEXIFTimestamps)).

                                 SyncGPS().
                                 Do(v => Console.WriteLine("Number of images w/o GPS: " +            v.NumberOfImagesWithoutGPS)).

                                 ResizeImages(2000, 1500).
                                 Store("fixed", "noGPS", (Sum, Processed, Percentage) => { if (Processed % 25 == 0) { Console.CursorLeft = 0; Console.Write(Percentage.ToString("0.00") + "% of " + Sum + " images stored..."); } }, ParallelWriteOptions).
                                 ToArray();

        }

    }

}
