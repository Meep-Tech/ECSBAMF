# Tile.Type
**Archetype Sub-Folder:** `_tiles`
## Valid File Configurations
### Single File:
- **Image Only with Name:**
	- `[NAME].png`
- **Config Only:**
	- `*.json`
### Multi-File/Folder:
- **Name Implied:**
	- `[NAME].png`
	- `[NAME].json`
- **All Data In Config:**
	- `*.png`
	- `*.json`
## Config.Json
- `string: name`(optional): The name of the Archetype. Must be unique per-[[Mod Package]] and [[Base Archetype]].
- `string: packageName`(optional): The package this Archetype is from.
- `string: description`(optional): A decription of this Tile Type.
- `string[]: tags`(optional): Tags used to find and describe this Tile.Type
- `int: pixelsPerTileDiameter`(optional): The diameter of a single Tile in Pixels. This is for automatically splitting a larger Tile Sheet Image Asset into multiple Tile Types. See `sizeInTiles` for an alternative.
- `Vector2Int: sizeInTiles`(optional): Alternative to `pixelsPerTileDiameter` for specifying the dimensions of a Tile Sheet Image Asset using the dimensions of the tiles it needs to be cropped into instead of the size of each tile.
- \*`float: height`(optional): A default hieght value to give to all Tile.Types created.
- `bool: useDefaultBackgroundAsInWorldTileImage`(optional): Allows you to make invisible Tile.Types that still have an icon representation in the World Editor when set to false. This setting makes it so that when this Tile.Type is placed, the image representing the Tile.Type is not placed in the [[World]]. Defaults to `true`.
- `enum: mode`(optional): The import mode can be specified here. The valid options are:
	- `"Individual"`: This config represents one(or no) Image Asset containing just one Tile.Type 
	- `"Sheet"`: This config represents a Tile Sheet Image Asset that needs to be split into multiple Tile.Types depending on an `pixelsPerTileDiameter` or `sizeInTiles` value
	- `"Animation"`: (Not Yet Implemented) This config represents a Tile Sheet Image Asset or multiple individual Image Assets that need to be combined into an Animated Tile.Type.

\*: Providing these 'Special Value' properies will create 2 Tile.Type Archetypes from this config. One will be just the background image, and one will contain the 'Special Values' which will override any other 'Special Values' on a tile during set.