﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Receives an array of IntArrayFromTexture and generate a voxel greedy mesh
/// </summary>
public class GreedyMeshGenerator : ThreadedJob
{
    //To debug on console
    public bool verbose = false;
    
    //Inputs
	/// <summary>
	/// The blockIDs of every block in every layer
	/// </summary>
    public IntArrayFromTexture[] layers;
	
	/// <summary>
	/// The material that the mesh will use
	/// </summary>
    public Material material;
	
	/// <summary>
	/// the axis the thread will generate 
	/// </summary>
    public string renderAxis;
    
    //Outputs
	/// <summary>
	/// The mesh generated by the thread
	/// </summary>
    public Mesh mesh;
    
    
    //PRIVATE VARS TO MAKE THE MESH
	/// <summary>
	/// To count all the quads
	/// </summary>
    int _quadCounter = 0;
	public int QuadCounter
	{
		get { return _quadCounter; }
		private set { _quadCounter = value; }
	}
	
	/// <summary>
	/// Vertices the mesh will use
	/// </summary>
    List<Vector3> _vertices = new List<Vector3>();
	
	/// <summary>
	/// UVs the mesh will use
	/// </summary>
    List<Vector2> _uvs = new List<Vector2>();
	
	/// <summary>
	/// Normals the mesh will use
	/// </summary>
    List<Vector3> _normals = new List<Vector3>();
	
	/// <summary>
	/// Triangles the mesh will use
	/// </summary>
    List<int> _meshTris = new List<int>();
    
    //Map sliced by axises to make greedy faster
    List<BlockCoordinate>[] _xzSlices;
    List<BlockCoordinate>[] _xySlices;
    List<BlockCoordinate>[] _yzSlices;
    
	/// <summary>
	/// A dictionary to keep track of what side has been draw
	/// </summary>
    Dictionary<BlockCoordinate, BlockCoordDrawState> _mapBlocksDrawState;
    
    
	/// <summary>
	/// List of coordinates sorted by blockID to run the algorithm per every blockID that exists in the mesh
	/// </summary>
    Dictionary<int, List<BlockCoordinate>> _coordsPerBlock;
    
	/// <summary>
	/// With of a pixel from the palette normalized from 0 to 1
	/// </summary>
	readonly float _tileWidth = 1f/(float)Palette.cols;
	/// <summary>
	/// Height of a pixel from the palette normalized from 0 to 1
	/// </summary>
	readonly float _tileHeight = 1f/(float)Palette.rows;

	/// <summary>
	/// The function the thread will execute without affecting the framerate
	/// </summary>
	protected override void ThreadFunction()
    {
        try
        {
	        ThreadedInit ();
	        foreach (KeyValuePair<int,List<BlockCoordinate>> coordinates in _coordsPerBlock)
		        GreedyMesh(coordinates.Key,renderAxis);

	        IsDone = true;
        }
        catch (System.Threading.ThreadInterruptedException exception)
        {
            Debug.Log("ThreadInterruptedException: " + exception.Message);
            Clear();
        }
    }
	
	/// <summary>
	/// Ensures that a greedy quadrilateral is made for all blockIDs the mesh has within the indicated axis
	/// </summary>
	/// <param name="blockID">BlockID of the quadrilaterals to be made</param>
	/// <param name="axises">Axis to be made</param>
	void GreedyMesh(int blockID, string axises)
	{
		if (axises == "xy")
		{
			for (int z = 0; z < layers.Length; z++)
			{
				for (int i = 0; _xySlices[z] != null && i < _xySlices[z].Count; i++)
				{
					BlockCoordinate current = _xySlices[z][i];
					string side = "top";
					if (GetBlockID(current) == blockID && !GetFaceDrawn(current, side) &&
					    !SideHidden(current, side))
					{
						MakeGreedyQuad(_xySlices[z][i], side);
					}

					side = "bottom";
					if (GetBlockID(current) == blockID && !GetFaceDrawn(current, side) &&
					    !SideHidden(current, side))
					{
						MakeGreedyQuad(_xySlices[z][i], side);
					}
				}
			}
		}

		if (axises == "xz")
		{
			for (int y = 0; y < layers[0].height; y++)
			{

				for (int i = 0; _xzSlices[y] != null && i < _xzSlices[y].Count; i++)
				{
					string side = "front";
					BlockCoordinate current = _xzSlices[y][i];
					if (GetBlockID(current) == blockID && !GetFaceDrawn(current, side) &&
					    !SideHidden(current, side))
					{
						MakeGreedyQuad(_xzSlices[y][i], side);
					}

					side = "back";
					if (GetBlockID(current) == blockID && !GetFaceDrawn(current, side) &&
					    !SideHidden(current, side))
					{
						MakeGreedyQuad(_xzSlices[y][i], side);
					}
				}

			}
		}


		if (axises == "yz")
		{
			for (int x = 0; x < layers[0].height; x++)
			{

				for (int i = 0; _yzSlices[x] != null && i < _yzSlices[x].Count; i++)
				{
					BlockCoordinate current = _yzSlices[x][i];
					string side = "left";
					if (GetBlockID(current) == blockID && !GetFaceDrawn(current, side) &&
					    !SideHidden(current, side))
					{
						MakeGreedyQuad(_yzSlices[x][i], side);
					}

					side = "right";
					if (GetBlockID(current) == blockID && !GetFaceDrawn(current, side) &&
					    !SideHidden(current, side))
					{
						MakeGreedyQuad(_yzSlices[x][i], side);
					}
				}
			}
		}
	}


	/// <summary>
	/// Initializations that needs to be done on the main thread
	/// </summary>
	public void Init()
    {    
        _mapBlocksDrawState = new Dictionary<BlockCoordinate, BlockCoordDrawState>();
        _coordsPerBlock = new Dictionary<int, List<BlockCoordinate>>();
        _xzSlices = new List<BlockCoordinate>[layers[0].height];
        _xySlices = new List<BlockCoordinate>[layers.Length];
        _yzSlices = new List<BlockCoordinate>[layers[0].width];
        Clear();
    }

	/// <summary>
	/// Initializations that can be made outside the main thread
	/// </summary>
    void ThreadedInit()
    {

        for (int x = 0; x < layers[0].width; x++)
        {
            for (int y = 0; y < layers[0].height; y++)
            {
                for (int z = 0; z < layers.Length; z++)
                {
                    BlockCoordinate currentCoord = new BlockCoordinate(x, y, z);
                    if (currentCoord.inBounds() && GetBlockID(currentCoord) != 0 && !BlockIsHidden(currentCoord))
                    {
                        MarkCoordToDraw(currentCoord);
                    }
                }
            }
        }

    }
	
    /// <summary>
    /// Marks the coordinate as a coordinate to be drawn
    /// Sets the coordinate in _coordsPerBlock, _xySlices, _yzSlices and _xzSlices
    /// </summary>
    /// <param name="coord">Coordinate to be drawn</param>
    void MarkCoordToDraw(BlockCoordinate coord)
    {
        int x = coord.x;
        int y = coord.y;
        int z = coord.z;
        int blockID = GetBlockID(coord);

        //Add this coord to _coordsPerBlock
        //If list already exist, add it
        List<BlockCoordinate> coordList;
        if (_coordsPerBlock.TryGetValue(blockID, out coordList))
        {
            coordList.Add(coord);
        }
        else //Else create a new entry in the dictionary and add the coord o a new list 
        {
            coordList = new List<BlockCoordinate>();
            coordList.Add(coord);
            _coordsPerBlock.Add(blockID, coordList);
        }

	    if (renderAxis == "xy")
	    {
		    if (_xySlices[z] == null)
			    _xySlices[z] = new List<BlockCoordinate>();
		    _xySlices[z].Add(coord);
	    }
	    else if (renderAxis == "yz")
	    {
		    if (_yzSlices[x] == null)
			    _yzSlices[x] = new List<BlockCoordinate>();
		    _yzSlices[x].Add(coord);
	    }
	    else if (renderAxis == "xz")
	    {
		    if (_xzSlices[y] == null)
			    _xzSlices[y] = new List<BlockCoordinate>();
		    _xzSlices[y].Add(coord);
	    }

    }
	
    /// <summary>
    /// Sets a face of the mesh as added to the mesh
    /// </summary>
    /// <param name="blockCoordinate">BlockCoordinate to set</param>
    /// <param name="side">Side of the BlockCoordinate</param>
    void SetFaceDrawn(BlockCoordinate blockCoordinate, string side){
        BlockCoordDrawState blockCoordDrawState;
        if (_mapBlocksDrawState.TryGetValue(blockCoordinate, out blockCoordDrawState))
        {
	        if (blockCoordinate == new BlockCoordinate(1, 0, 1) && side == "back")
	        {
		        Debug.Log("SetCoordinateDrawn ("+blockCoordinate+","+side+") true");
	        }
	        
            if (side == "top")
                blockCoordDrawState.top = true;
            else if (side == "bottom")
                blockCoordDrawState.bottom = true;
            else if (side == "front")
                blockCoordDrawState.front = true;
            else if (side == "back")
                blockCoordDrawState.back = true;
            else if (side == "left")
                blockCoordDrawState.left = true;
            else if (side == "right")
                blockCoordDrawState.right = true;
        }
        else
        {
	        if (blockCoordinate == new BlockCoordinate(1, 0, 1) && side == "back")
	        {
		        Debug.Log("SetCoordinateDrawn ("+blockCoordinate+","+side+") true");
	        }
	        
            blockCoordDrawState = new BlockCoordDrawState();
	        blockCoordDrawState.Reset();
            if (side == "top")
                blockCoordDrawState.top = true;
            else if (side == "bottom")
                blockCoordDrawState.bottom = true;
            else if (side == "front")
                blockCoordDrawState.front = true;
            else if (side == "back")
                blockCoordDrawState.back = true;
            else if (side == "left")
                blockCoordDrawState.left = true;
            else if (side == "right")
                blockCoordDrawState.right = true;
            _mapBlocksDrawState.Add(blockCoordinate,blockCoordDrawState);
        }
    }
    
    /// <param name="blockCoordinate">BlockCoord to be checked</param>
    /// <param name="side">Side of the blockCoord to be checked</param>
    /// <returns>Returns if a specific face of the mesh has been already added to the mesh or not</returns>
    bool GetFaceDrawn(BlockCoordinate blockCoordinate, string side){
        BlockCoordDrawState blockCoordDrawState;
        if (_mapBlocksDrawState.TryGetValue(blockCoordinate, out blockCoordDrawState))
        {
	        if (blockCoordinate == new BlockCoordinate(1, 0, 1) && side == "back")
	        {
		        Debug.Log("GetCoordinateDrawn ("+blockCoordinate+","+side+") = "+blockCoordDrawState.back);
	        }
		        
            if (side == "top")
                return blockCoordDrawState.top;
            if (side == "bottom")
                return blockCoordDrawState.bottom;
            if (side == "front")
                return blockCoordDrawState.front;
            if (side == "back")
                return blockCoordDrawState.back;
            if (side == "left")
                return blockCoordDrawState.left;
            if (side == "right")
                return blockCoordDrawState.right;

            return false;
        }
        else
        {
            return false;
        }
    }
    
    /// <param name="blockCoordinate">Coordinate to be tested</param>
    /// <param name="side">Side</param>
    /// <returns>Returns if a side is hidden because blocked by another block</returns>
    bool SideHidden(BlockCoordinate blockCoordinate, string side){

        BlockCoordinate offset = new BlockCoordinate (0, 1, 0);
        if (side == "top") {
            offset = new BlockCoordinate (0, 0, 1);
        } else if (side == "bottom") {
            offset = new BlockCoordinate (0, 0, -1);
        } else if (side == "front") {
            offset = new BlockCoordinate (0, 1, 0);
        } else if (side == "back") {
            offset = new BlockCoordinate (0, -1, 0);
        } else if (side == "left") {
            offset = new BlockCoordinate (-1, 0, 0);
        } else if (side == "right") {
            offset = new BlockCoordinate (1, 0, 0); 
        }

        BlockCoordinate nextBlockCoordinate = blockCoordinate + offset;
        if (!nextBlockCoordinate.inBounds())
            return false;
        
        if (GetBlockID(nextBlockCoordinate) != 0)
            return true;
        
        return false;

    }
    
    /// <summary>
    /// Makes the biggest quadrilateral possible
    /// </summary>
    /// <param name="fromCoordinate">Starting coordinate of the quadrilateral</param>
    /// <param name="side">Side of the cube tah will be made</param>
    void MakeGreedyQuad(BlockCoordinate fromCoordinate, string side){
		if(verbose)
			Debug.Log("------ GreedyFrom "+fromCoordinate+" "+side);

		int width = 1;
		int height = 1;

		int blockID = GetBlockID(fromCoordinate);
		BlockCoordinate deltaW = new BlockCoordinate ();
		BlockCoordinate deltaH = new BlockCoordinate ();

		if (side == "top") {
			deltaW = new BlockCoordinate (1, 0, 0);
			deltaH = new BlockCoordinate (0, 1, 0);
		} else if (side == "bottom") {
			deltaW = new BlockCoordinate (1, 0, 0);
			deltaH = new BlockCoordinate (0, 1, 0);
		} else if (side == "front") {
			deltaW = new BlockCoordinate (1, 0, 0);
			deltaH = new BlockCoordinate (0, 0, 1);
		} else if (side == "back") {
			deltaW = new BlockCoordinate (1, 0, 0);
			deltaH = new BlockCoordinate (0, 0, 1);
		} else if (side == "left") {
			deltaW = new BlockCoordinate (0, 1, 0);
			deltaH = new BlockCoordinate (0, 0, 1);
		}else if (side == "right") {
			deltaW = new BlockCoordinate (0, 1, 0);
			deltaH = new BlockCoordinate (0, 0, 1);
		}
		SetFaceDrawn (fromCoordinate, side);
        
		if (verbose) {
			Debug.Log ("Greedy from coodenate " + fromCoordinate.ToString ());
			Debug.Log (">> Drawed " + fromCoordinate.ToString ());
		}
		bool growingWidth = true;
		bool growingHeight = true;
		do{
			if(verbose)
				Debug.Log("w="+width + " h="+height);

			if(growingWidth){
				if(verbose)
					Debug.Log("Grow Width");


				List<BlockCoordinate> checkingArea = BlockAreaIsID(blockID,1,height,fromCoordinate+deltaW*width,deltaW,deltaH, side);
				if(checkingArea != null){
					for(int i = 0; i < checkingArea.Count; i++){
						SetFaceDrawn (checkingArea [i], side);

						if(verbose)
							Debug.Log(">> Drawed "+checkingArea[i].ToString());
					}
					width++;
				} else {
					if(verbose)
						Debug.Log("growingWidth = false");
					
					growingWidth = false;
				}
			} else {
				if(verbose)
					Debug.Log("NOT Grow Width");
			}


			if(growingHeight){
				if(verbose)
					Debug.Log("Grow Height");
				
				List<BlockCoordinate> checkingArea = BlockAreaIsID(blockID,width,1,fromCoordinate+deltaH*height,deltaW,deltaH, side);
				if(checkingArea != null){
					for(int i = 0; i < checkingArea.Count; i++){
						SetFaceDrawn (checkingArea [i], side);
						if(verbose)
							Debug.Log(">> Drawed "+checkingArea[i].ToString());
					}
					height++;
				} else {
					if(verbose)
						Debug.Log("growingHeight = false");
					growingHeight = false;
				}
			} else {
				if(verbose)
					Debug.Log("NOT Grow Height");
			}

		}while(growingWidth || growingHeight);

		if(verbose)
			Debug.Log("Greedy ENDED");
		NewQuadrilateral (fromCoordinate, width, height, side, blockID);

	}
	
    /// <summary>
    /// Adds a new quadrilateral to the mesh 
    /// </summary>
    /// <param name="fromCoordinate">The start coordinate of the quad</param>
    /// <param name="width">The width of the quad in blocks</param>
    /// <param name="height">The width of the quad in blocks</param>
    /// <param name="side">The side of the cube that will be made</param>
    /// <param name="blockID">The blockID of the quad</param>
    void NewQuadrilateral(BlockCoordinate fromCoordinate,int width, int height, string side, int blockID){

		BlockCoordinate currentCoord = new BlockCoordinate(fromCoordinate.x,fromCoordinate.z,-fromCoordinate.y);
		float l = 1f;
		Vector3 a = new Vector3(currentCoord.x*(l*2)-l,currentCoord.y*(l*2)-l,currentCoord.z*(l*2)-l);
		Vector3 b = new Vector3(currentCoord.x*(l*2)-l,currentCoord.y*(l*2)+l,currentCoord.z*(l*2)-l);
		Vector3 c = new Vector3(currentCoord.x*(l*2)+l,currentCoord.y*(l*2)+l,currentCoord.z*(l*2)-l);
		Vector3 d = new Vector3(currentCoord.x*(l*2)+l,currentCoord.y*(l*2)-l,currentCoord.z*(l*2)-l);
		Vector3 e = new Vector3(currentCoord.x*(l*2)-l,currentCoord.y*(l*2)+l,currentCoord.z*(l*2)+l);
		Vector3 f = new Vector3(currentCoord.x*(l*2)+l,currentCoord.y*(l*2)+l,currentCoord.z*(l*2)+l);
		Vector3 g = new Vector3(currentCoord.x*(l*2)+l,currentCoord.y*(l*2)-l,currentCoord.z*(l*2)+l);
		Vector3 h = new Vector3(currentCoord.x*(l*2)-l,currentCoord.y*(l*2)-l,currentCoord.z*(l*2)+l);

		if (side == "top") {
			//Right
			c += new Vector3 ((width - 1) * l * 2, 0, 0);
			f += new Vector3 ((width - 1) * l * 2, 0, 0);
			g += new Vector3 ((width - 1) * l * 2, 0, 0);
			d += new Vector3 ((width - 1) * l * 2, 0, 0);

			//Front
			b += new Vector3 (0, 0, -(height - 1) * l * 2);
			c += new Vector3 (0, 0, -(height - 1) * l * 2);
			d += new Vector3 (0, 0, -(height - 1) * l * 2);
			a += new Vector3 (0, 0, -(height - 1) * l * 2);

			//Top
			NewQuadrilateral (b, e, f, c, blockID);
		} else if (side == "bottom") {
			//Right
			c += new Vector3 ((width - 1) * l * 2, 0, 0);
			f += new Vector3 ((width - 1) * l * 2, 0, 0);
			g += new Vector3 ((width - 1) * l * 2, 0, 0);
			d += new Vector3 ((width - 1) * l * 2, 0, 0);

			//Front
			b += new Vector3 (0, 0, -(height - 1) * l * 2);
			c += new Vector3 (0, 0, -(height - 1) * l * 2);
			d += new Vector3 (0, 0, -(height - 1) * l * 2);
			a += new Vector3 (0, 0, -(height - 1) * l * 2);
			//Bottom
			NewQuadrilateral (d, g, h, a, blockID);


		} else if (side == "front") {
			//Right
			c += new Vector3 ((width - 1) * l * 2, 0, 0);
			f += new Vector3 ((width - 1) * l * 2, 0, 0);
			g += new Vector3 ((width - 1) * l * 2, 0, 0);
			d += new Vector3 ((width - 1) * l * 2, 0, 0);

			//Up
			b += new Vector3 (0, (height - 1) * l * 2, 0);
			c += new Vector3 (0, (height - 1) * l * 2, 0);
			f += new Vector3 (0, (height - 1) * l * 2, 0);
			e += new Vector3 (0, (height - 1) * l * 2, 0);

			//Front
			NewQuadrilateral(a,b,c,d, blockID);
		} else if (side == "back") {
			//Right
			c += new Vector3 ((width - 1) * l * 2, 0, 0);
			f += new Vector3 ((width - 1) * l * 2, 0, 0);
			g += new Vector3 ((width - 1) * l * 2, 0, 0);
			d += new Vector3 ((width - 1) * l * 2, 0, 0);

			//Up
			b += new Vector3 (0, (height - 1) * l * 2, 0);
			c += new Vector3 (0, (height - 1) * l * 2, 0);
			f += new Vector3 (0, (height - 1) * l * 2, 0);
			e += new Vector3 (0, (height - 1) * l * 2, 0);

			//back
			NewQuadrilateral(g,f,e,h, blockID);
		} else if(side == "left"){
			//Front
			b += new Vector3 (0, 0, -(width - 1) * l * 2);
			c += new Vector3 (0, 0, -(width - 1) * l * 2);
			d += new Vector3 (0, 0, -(width - 1) * l * 2);
			a += new Vector3 (0, 0, -(width - 1) * l * 2);

			//Up
			c += new Vector3 (0, (height - 1) * l * 2, 0);
			b += new Vector3 (0, (height - 1) * l * 2, 0);
			e += new Vector3 (0, (height - 1) * l * 2, 0);
			f += new Vector3 (0, (height - 1) * l * 2, 0);


			//Left
			NewQuadrilateral(b,a,h,e, blockID);
		} else if(side == "right"){

			//Front
			b += new Vector3 (0, 0, -(width - 1) * l * 2);
			c += new Vector3 (0, 0, -(width - 1) * l * 2);
			d += new Vector3 (0, 0, -(width - 1) * l * 2);
			a += new Vector3 (0, 0, -(width - 1) * l * 2);

			//Up
			c += new Vector3 (0, (height - 1) * l * 2, 0);
			b += new Vector3 (0, (height - 1) * l * 2, 0);
			e += new Vector3 (0, (height - 1) * l * 2, 0);
			f += new Vector3 (0, (height - 1) * l * 2, 0);


			//Right
			NewQuadrilateral(d,c,f,g, blockID); //OK

		}
	}


	/// <summary>
	/// Makes a new quadrilateral using v0..3 as vertices in this order:
	///  1 - - 2
	///  |   / |
	///  |  /  |
	///  0 - - 3
	/// </summary>
	/// <param name="v0">Vertex 0</param>
	/// <param name="v1">Vertex 1</param>
	/// <param name="v2">Vertex 2</param>
	/// <param name="v3">Vertex 3</param>
	/// <param name="blockID">The ID of the block that will set the color of the quad</param>
	void NewQuadrilateral(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int blockID)
	{
		
		int xOffset, yOffset;
		Palette.ColorToCoord(IntArrayFromTexture.IntToColor(blockID), out xOffset, out yOffset);
		
		//Offset is the center of the pixel color on the texture
		Vector2 offset = new Vector2((float) xOffset * _tileWidth + _tileWidth*.5f , (float) yOffset * _tileHeight+ _tileHeight*.5f);
		

		_vertices.Add (v0);
		_vertices.Add (v1);
		_vertices.Add (v2);
		_vertices.Add (v3);
		
		_meshTris.Add(_quadCounter*4+0);
		_meshTris.Add(_quadCounter*4+1);
		_meshTris.Add(_quadCounter*4+2);
		_meshTris.Add(_quadCounter*4+0);
		_meshTris.Add(_quadCounter*4+2);
		_meshTris.Add(_quadCounter*4+3);

		
		//uvs coords are really close to "stretch" the texture a lot so only one pixel in the texture
		//is enough to fill any quad with the same color
		_uvs.Add (offset + new Vector2 (0, 0));
		_uvs.Add (offset + new Vector2 (0, .0000001f));
		_uvs.Add (offset + new Vector2 (.0000001f,.0000001f));
		_uvs.Add (offset + new Vector2 (.0000001f, 0));


		Vector3 side1 = v1 - v0;
		Vector3 side2 = v3 - v0;
		Vector3 normal = Vector3.Cross(side1, side2).normalized;
		_normals.Add(normal);
		_normals.Add(normal);
		_normals.Add(normal);
		_normals.Add(normal);


		QuadCounter = _quadCounter + 1;
	}
	
    /// <summary>
    /// Returns the coordinates of the area if is all the area from the side is blockID, otherwise returns null
    /// </summary>
    /// <param name="blockID">BlockID to be tested</param>
    /// <param name="width">Width of the area to be tested in the deltaW direction</param>
    /// <param name="height">Height of the area to be tested in the deltaH direction</param>
    /// <param name="fromCoordinate">The starting BlockCoordinate where the function will start checking</param>
    /// <param name="deltaW">Means the unitary direction the function will check depending on the side,
    /// for example deltaW = BlockCoordinate(1,0,0) when side is "front"</param>
    /// <param name="deltaH">Means the unitary direction the function will check depending on the side,
    /// for example deltaW = BlockCoordinate(0,0,1) when side is "front"</param>
    /// <param name="side">side of the cube</param>
    /// <returns></returns>
    List<BlockCoordinate> BlockAreaIsID(int blockID, int width,int height, BlockCoordinate fromCoordinate, BlockCoordinate deltaW, BlockCoordinate deltaH, string side){
//		Debug.Log("from "+fromCoordenate.ToString()+" ("+width+"."+height+")");
        List<BlockCoordinate> area = new List<BlockCoordinate>();
        BlockCoordinate currentCoordinate = fromCoordinate;
        for(int i = 0; i < height; i++){
            for(int j = 0; j < width; j++){
                currentCoordinate = fromCoordinate + deltaH*i + deltaW*j;
//				Debug.Log("[AREA]["+i+","+j+"] currentCoordenate = "+currentCoordenate);
                if (currentCoordinate.inBounds () &&
                    GetBlockID(currentCoordinate) == blockID  && 
                    GetFaceDrawn(currentCoordinate,side) == false
                    && !SideHidden(currentCoordinate,side) && currentCoordinate.z < layers.Length) {
                    area.Add (currentCoordinate);
                } else {
                    return null;
                }
            }
        }
        return area;
    }
    
    int GetBlockID(BlockCoordinate coord)
    {
        return GetBlockID(coord.x, coord.y,coord.z);
    }

    int GetBlockID(int x, int y, int z)
    {
        return layers[z].GetInt(x, y);
    }
    
    /// <summary>
    /// A block is hidden when is surrounded by other blocks so it can't be seen 
    /// </summary>
    bool BlockIsHidden(BlockCoordinate blockCoord){
        int x = blockCoord.x;
        int y = blockCoord.y;
        int z = blockCoord.z;

        if( z > 0 && z < layers.Length - 1
                  && x > 0 && x < layers[0].width - 1
                  && y > 0 && y < layers[0].height - 1
                  && GetBlockID(x,y,z+1) != 0
                  && GetBlockID(x,y,z-1) != 0
                  && GetBlockID(x+1,y,z) != 0
                  && GetBlockID(x-1,y,z) != 0
                  && GetBlockID(x,y+1,z) != 0
                  && GetBlockID(x,y-1,z) != 0
        )
        {
            return true;
        }
        return false;
    }
    void Clear()
    {
        mesh = new Mesh();
        QuadCounter = 0;
        _vertices.Clear();
        _uvs.Clear();
        _normals.Clear();
        _meshTris.Clear();
        _mapBlocksDrawState.Clear();
        _coordsPerBlock.Clear();
    }
	
	/// <summary>
	/// Generates the mesh using vertices, tris and uvs calculated on the thread function
	/// </summary>
	public void UpdateMesh(){
		if(_vertices.Count < 65000){
			mesh.Clear();
			mesh.vertices = _vertices.ToArray();
			mesh.subMeshCount = 1;
			int[] tris = _meshTris.ToArray();
			mesh.SetTriangles(tris,0);			
			mesh.uv = _uvs.ToArray();
			mesh.normals = _normals.ToArray();
		} else {
			Debug.LogError(_vertices.Count+" vertices are too many to be drawn on one mesh! (max 65000)");
		}
	}
	
}
