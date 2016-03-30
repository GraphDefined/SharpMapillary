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
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using org.GraphDefined.Vanaheimr.Aegir;
using org.GraphDefined.Vanaheimr.Aegir.Tiles;

#endregion

namespace org.GraphDefined.SharpMapillary.WPF
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Data

        private readonly TilesLayer                 TilesLayer;
        private readonly ShapeLayer                 ShapeLayer1;
        private readonly FeatureLayer               FeatureLayer1;

        #endregion

        public MainWindow()
        {

            InitializeComponent();

            #region Add map layers to MapControl

            TilesLayer      = MapControl.AddLayer<TilesLayer>  ("TilesLayer",    0);
            TilesLayer.TilesClient.Register(new OSMProvider());

            ShapeLayer1     = MapControl.AddLayer<ShapeLayer>  ("ShapeLayer",   10);
            FeatureLayer1   = MapControl.AddLayer<FeatureLayer>("FeatureLayer", 20);

            #endregion

            this.SizeChanged += MainWindow_SizeChanged;

        }


        #region (private) MainWindow_SizeChanged(...)

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            #region Zoom to geo position //FIX ME!

            //ToDo: FIX ME! Does not zoom to the middle of the screen!
            //            MapControl.ZoomTo(new Latitude(49.105), new Longitude(10.105), 13);
            MapControl.ZoomTo(new Latitude(49.730936), new Longitude(10.141107), 16);

            #endregion

        }

        #endregion

        #region (private) Window_Closing(...)

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        { }

        #endregion

        #region (private) Window_SizeChanged(...)

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        { }

        #endregion


        private void SelectedFolderTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
  //          SelectedFolderTextBox.preview

      //      AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new EventHandler<MouseButtonEventArgs>(HandleClickOutsideOfControl), true);
        }

        private void HandleClickOutsideOfControl(object sender, MouseButtonEventArgs e)
        {
            //do stuff (eg close drop down)
            ReleaseMouseCapture();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new FolderBrowserDialog();

            DialogResult result = dialog.ShowDialog();

            if (Directory.Exists(dialog.SelectedPath))
            {

                SelectedFolderTextBox.Text = dialog.SelectedPath;
                SelectedFolderTextBox_LostFocus(sender, null);

            }

            else
            {
                SelectedFolderTextBox.Text = "Choose a folder and feel lucky!";
            }


        }

        private void SelectedFolderTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                Keyboard.ClearFocus();
                SelectedFolderTextBox_LostFocus(sender, null);
                //SelectedFolderButton.Focus();
            }

        }

        private void SelectedFolderTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

            if (Directory.Exists(SelectedFolderTextBox.Text))
            {

                SelectedFolderTextBox.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0xCC, 0x00));

                SharpMapillary.Start(SelectedFolderTextBox.Text).

                    LoadGPXs(OnDupliateTimestamp: (GPXFile, Timestamp, lat, lng, alt) => { Console.WriteLine("Duplicate GPS timestamp: " + Timestamp.ToUniversalTime().ToString("s") + "Z" + " in GPX file: " + GPXFile); return Timestamp.Add(TimeSpan.FromMilliseconds(500)); },
                             OnResult:            (Min, Max, Kind)                    => Console.WriteLine("Min/Max GPS timestamps: "  + Min.ToLocalTime().ToString("s") + " / " + Max.ToLocalTime().ToString("s") + " - " + Kind.ToString())).

                    Do(v => Console.WriteLine("Number of GPS trackpoints: " + v.NumberOfGPSPoints)).
                    Do(v => Console.WriteLine("Number of duplicate GPS timestamps: " + v.NumberOfDuplicateGPSTimestamps)).
                    Do(v => {

                        Logfile.Text += "Number of GPS trackpoints: " + v.NumberOfGPSPoints + Environment.NewLine;
                        Logfile.Text += "Number of duplicate GPS timestamps: " + v.NumberOfDuplicateGPSTimestamps + Environment.NewLine;

                        // C:\Mapillary\Jena-Halle
                        foreach (var vv in v.GPSData)
                        {

                            var GPSInfo  = vv.Value;

                            FeatureLayer1.AddFeature("GPX",
                                             new Latitude (GPSInfo.Latitude),
                                             new Longitude(GPSInfo.Longitude),
                                             5, 5,
                                             Brushes.Orange,
                                             Brushes.Black,
                                             1.0);

                        }

                        var v1 = v.GPSData.First();
                        var v2 = v.GPSData.Last();

                        MapControl.ZoomTo(new Latitude(v1.Value.Latitude), new Longitude(v1.Value.Longitude), 10);

                    }).

                    ToArray();

            }

            else
            {
                SelectedFolderTextBox.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0xCC, 0x00, 0x00));
                //SelectedFolderTextBox.Text = "Choose a folder and feel lucky!";
            }

        }

        private void SelectedFolderTextBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

    }

}
