SharpMapillary
==============

SharpMapillary is a little toolbox for everyone who creates photos for [Mapillary](http://www.mapillary.com) using a sports camera like a [GoPro](http://www.gopro.com) and a software like [RunKeeper](http://runkeeper.com) for recording the GPS tracks. The toolbox currently provides the following features:

* Load GPS coordinates and their timestamps from GPX files
* Load EXIF metadata from JPEG images
* Sync GPS coordinates and images by their shared timestamps, including an optional time offset
* Interpolate missing GPS coordinates (NearestMatch, LinearInterpolation, ...)
* Add or change EXIF metadata (e.g. Artist, UserComment, WindowsKeywords, Copyright, ...)
* Resize the images
* Upload to Mapillary (not yet!)
* Create a proper GeoJSON/KML representation of the data set (not yet!)
* Use a little embedded HTTP server with some code and [leaflet](http://leafletjs.com) to verify and fine tune the data set (not yet!)

#### Usage

SharpMapillary is implemented as a set of pipes. Any enumeration of file system paths can be used to start the pipeline and you can intercept the pipeline everywhere to add your customizations.


    SharpMapillary.Start(@"E:\_Projekte\Mapillary\Jena-West1").
                         LoadGPXs().
                         Do(v => Console.WriteLine("Number of GPS trackpoints: " +           v.NumberOfGPSPoints)).
                         Do(v => Console.WriteLine("Number of duplicate GPS timestamps: " +  v.NumberOfDuplicateGPSTimestamps)).
                         LoadJPEGs(OnProcessed:         (Sum, Processed, Percentage)       => { if (Processed % 25 == 0) { Console.CursorLeft = 0; Console.Write(Percentage.ToString("0.00") + "% of " + Sum + " images loaded..."); } },                                 Do(v => Console.WriteLine(Environment.NewLine + "Number of duplicate EXIF timestamps: " + v.NumberOfDuplicateEXIFTimestamps)).
                         SyncGPS().
                         Do(v => Console.WriteLine("Number of images w/o GPS: " +            v.NumberOfImagesWithoutGPS)).
                         ResizeImages(2000, 1500).
                         Store("fixed", "noGPS", (Sum, Processed, Percentage) => { if (Processed % 25 == 0) { Console.CursorLeft = 0; Console.Write(Percentage.ToString("0.00") + "% of " + Sum + " images stored..."); } }).                                  ToArray();

#### Note

For best multi-threaded performance please put your data on a SSD!
