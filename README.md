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
* Use a little embedded HTTP server to verify and fine tune the data set

#### Usage

SharpMapillary is implemented as a set of pipes. Any enumeration of file system paths can be used to start the pipeline and you can intercept the pipeline everywhere to add your customizations.


    SharpMapillary.Start(@"E:\_Projekte\Mapillary\Jena-West1").
                         LoadGPXs().
                         LoadJPGs().//TimeOffset: TimeSpan.FromSeconds(51)).
                         SyncGPS().
                         Do(v => Console.WriteLine("Number of GPS trackpoints: " + v.NumberOfGPSPoints)).
                         Do(v => Console.WriteLine("Number of images: " +          v.NumberOfImages)).
                         Store("fixed").
                         ToArray();
                         
