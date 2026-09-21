# Working with Locations

## Map Locations

MapLocation objects are used to show areas on the map for active conditions. 
They support two formats:


1. Coordinate object: 
{
    "Location": {
        "X": -157607.328,
        "Y": -687586.562,
        "Z": 667.976
    },
    "SizeFactor": 1.0
}
    

2. String format (from game): 
{
    "Location": "{X=-157607.328 Y=-687586.562 Z=667.976|P=353.113800 Y=101.191971 R=0.000000}",
    "SizeFactor": 1.0
}
    

- **SizeFactor**: Controls circle size (1.0 = ~300m diameter)

## Interaction Locations

For interaction conditions, you need Location objects that define where players interact:

- **AnchorMesh**: Exact instance of object on map (from #GetMeshInfo)
- **Instance**: Technical identifier (from #GetMeshInfo)
- **FallbackTransform**: Position and rotation (used in singleplayer)
- **VisibleMesh**: 3D model of the object

## Using #GetMeshInfo

1. In game, look at an object (not landscape or foliage)
2. Execute command #GetMeshInfo
3. A Location JSON object is copied to clipboard
4. Use this data in your interaction conditions

Distance is limited to interaction range to ensure object is reachable.