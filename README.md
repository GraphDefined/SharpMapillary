SharpMapillary
==============

SharpMapillary is a little toolbox for everyone who creates photos for [Mapillary](http://www.mapillary.com) using a sports camera like a [GoPro](http://www.gopro.com) and a software like [RunKeeper](http://runkeeper.com) for recording the GPS tracks. The toolbox currently provides the following features:

* Load GPS coordinates and their timestamps from GPX files
* Load EXIF infos from JPEG images
* Sync GPS coordinates and images by their timestamp (including a time offset)
* Interpolate missing GPS coordinates (NearestMatch, LinearInterpolation)
* Add or change EXIF information (e.g. Artist, UserComment, WindowsKeywords, Copyright, ...)
* Resize the images
* Upload to Mapillary (not yet!)

#### Usage

    SharpMapillary.Start(@"E:\_Projekte\Mapillary\Jena-West1").
                         LoadGPXs().
                         LoadJPGs().//TimeOffset: TimeSpan.FromSeconds(51)).
                         SyncGPS().
                         Do(v => Console.WriteLine("Number of GPS trackpoints: " + v.NumberOfGPSPoints)).
                         Do(v => Console.WriteLine("Number of images: " +          v.NumberOfImages)).
                         Store("fixed").
                         ToArray();
                         
