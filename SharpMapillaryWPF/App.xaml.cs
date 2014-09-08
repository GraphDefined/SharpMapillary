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
using System.Windows;
using System.Threading.Tasks;

#endregion

namespace org.GraphDefined.SharpMapillary.WPF
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Data

        /// <summary>
        /// The command line arguments.
        /// </summary>
        public static String[] CommandLineArguments;

        #endregion

        #region Properties

        public String               StartDirectory          { get; set; }
        public ParallelOptions      ParallelReadOptions     { get; set; }
        public ParallelOptions      ParallelWriteOptions    { get; set; }

        #endregion

        #region Constructor(s)

        public App()
        {

            #region Defaults...

            StartDirectory          = Environment.CurrentDirectory;
            ParallelReadOptions     = new ParallelOptions();
            ParallelWriteOptions    = new ParallelOptions() { MaxDegreeOfParallelism = 8 };

            #endregion

        }

        #endregion


        #region Application_Startup(Sender, StartupEventArgs)

        private void Application_Startup(Object Sender, StartupEventArgs StartupEventArgs)
        {
            CommandLineArguments = StartupEventArgs.Args != null ? StartupEventArgs.Args : new String[0];
        }

        #endregion

        #region Application_Exit(Sender, ExitEventArgs)

        private void Application_Exit(Object Sender, ExitEventArgs ExitEventArgs)
        {

        }

        #endregion


    }

}
