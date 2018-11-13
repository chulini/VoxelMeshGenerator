# Voxel Mesh Generator
Unity3D C# project that generates a voxel 3D mesh using the greedy mesh technique.

Uses a color palette from a small texture and load/save the layers as png image on the [persistance data path](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html).

![Example in gif...](https://media.giphy.com/media/w7SEXpaHlbgradmRmf/giphy.gif)

# About Greedy Mesh Algorithm
Greedy mesh algorithm is a technique used to generate voxel meshes in run time optimizing the amount of polygons quick as possible. This algorithm is essential for terrain generation in voxelized games like Minecraft.

Here is an example of voxel mesh techniques made by @[mikolalysenko](https://github.com/mikolalysenko) in Javascript/webGL:

http://mikolalysenko.github.io/MinecraftMeshes/index.html


A complete description of the algorithm can be found here:

https://0fps.net/2012/06/30/meshing-in-a-minecraft-game/
