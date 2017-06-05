using System;

namespace Booldozer.Jmp.Objects
{
	public class Enemy : RenderableMapObject
	{
		private string name;
		private string pathName;
		private string accessName;
		private string codeName;

		private float posX;
		private float posY;
		private float posZ;
		private float rotX;
		private float rotY;
		private float rotZ;
		private float scaleX;
		private float scaleY;
		private float scaleZ;

		private int roomNumber;

		private float floatingHeight;
		private int appearPercent;
		private int appearFlag;
		private int disappearFlag;
		private int eventSetNo;
		private int itemTable;
		private int condType;
		private int moveType;
		private int searchType;
		private int appearType;
		private int placeType;
		private int invisible;
		private int stay;

		public Enemy()
		{
		}

		public override void Save(string path)
		{
			throw new NotImplementedException();
		}
	}
}
