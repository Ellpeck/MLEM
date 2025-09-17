# Tiled Extensions

If you're using [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)'s [Tiled](https://www.mapeditor.org/) map editor support, you can use the **MLEM.Extended** package alongside that to enhance your tilemap experience.

## Extensions
There are several opinionated extensions to tiled maps, tilesets and tiles, including, but not limited to:
- The ability to get a tileset tile from a tile easily
- The ability to get tile and tile map properties easily
- Getting multiple tiles and objects at a location or in an area

All of these extension methods can be found in the [TiledExtensions](xref:MLEM.Extended.Tiled.TiledExtensions) class.

## Tiled Map Collisions
MLEM.Extended includes a very easy way to set up collisions within your tiled maps through the use of [tile collisions](https://doc.mapeditor.org/en/stable/manual/editing-tilesets/#tile-collision-editor).

To get this set up, you simply have to add bounding rectangles to your tilesets within the Tiled editor. Then, you can query collisions like so:
```cs
// Creating the collision system for a tiled map
var collisions = new TiledMapCollisions(myMap);

// Getting a list of collisions for an area
var tiles = collisions.GetCollidingTiles(new RectangleF(2, 2, 3.5F, 3.5F));

// Checking if an area is colliding
var colliding = collisions.IsColliding(new RectangleF(4, 4, 1, 1));
```

### Collision Coordinate System
The coordinate system of these tiled collisions functions based on *percentages* rather than absolute pixel coordinates. The collision system sees each tile as being *one unit by one unit* big.

This means that, to check if the tile at tile coordinate `6, 10` contains any collisions, the following rectangle has to be used:
```cs
var tiles = collisions.GetCollidingTiles(new RectangleF(6, 10, 1, 1));
```
If the tile at that location is `16x16` pixels big, and it has a single collision box at pixels `4, 4` that is `8x8` pixels big, then the following code prints out its percentaged coordinates: `X: 0.25, Y: 0.25, Width: 0.5, Height: 0.5`.
```cs
foreach (var tile in tiles) 
    Console.WriteLine(tile.Collisions[0]);
```
