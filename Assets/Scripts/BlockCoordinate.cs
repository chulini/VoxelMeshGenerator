using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct BlockCoordinate{
	public int x;
	public int y;
	public int z;
	public BlockCoordinate(int _x, int _y, int _z){
		x = _x;
		y = _y;
		z = _z;
	}
	public bool inBounds(){
		return (x >= 0 && y >= 0 && z >= 0 && x < Map.width && y < Map.height && z < Map.depth);
	}

	public static bool operator !=(BlockCoordinate a, BlockCoordinate b) {
		return !(a.x == b.x && a.y == b.y && a.z == b.z);
	}
	public static bool operator ==(BlockCoordinate a, BlockCoordinate b) {
		return (a.x == b.x && a.y == b.y && a.z == b.z);
	}
	public override bool Equals(object o){
		if (!(o is BlockCoordinate))
			return false;
		BlockCoordinate b = (BlockCoordinate)o;
		return x == b.x && y == b.y && z == b.z;
	}
	public override int GetHashCode(){
		return x+y+z;
	}

	public static BlockCoordinate operator +(BlockCoordinate a, BlockCoordinate b) {
		return new BlockCoordinate(a.x + b.x,a.y + b.y, a.z + b.z);;
	}

	public static BlockCoordinate operator -(BlockCoordinate a, BlockCoordinate b) {
		return new BlockCoordinate(a.x - b.x,a.y - b.y, a.z - b.z);;
	}
	public static BlockCoordinate operator *(BlockCoordinate a, int b) {
		return new BlockCoordinate(a.x * b,a.y * b, a.z * b);;
	}

	public override string ToString(){
		return "BlockCoordinate("+x+","+y+","+z+")";
	}
	public int HorizontalSquareDistance(BlockCoordinate b){
		int deltaX = Mathf.Abs (b.x - this.x);
		int deltaY = Mathf.Abs (b.y - this.y);
		return Mathf.Max (deltaX, deltaY);



	}
}

