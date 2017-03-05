Delaunay
=======

Polygon map terrain generator, written in C#. See http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/

![Example in 3D](https://github.com//svejdo1/Delaunay/blob/master/sample-3d.png?raw=true)

## Sample

```
Install-Package Barbar.Delaunay.WindowsDrawing
```


```C#
Bootstrapper.Initialize();
// will create .png file with dump of polygon map
SampleGenerator.CreateVoronoiGraphAndSave();
```

