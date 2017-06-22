using System;

namespace BooldozerCore.Jmp.Objects
{
	public class Enemy : RenderableMapObject
	{
		private string name;
		private string createName;
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

		public override void Load(jmp data, int entryIndex)
		{
			base.Load(data, entryIndex);

			Position = new OpenTK.Vector3(posX, posY, posZ);
			Rotation = new OpenTK.Quaternion(new OpenTK.Vector3(rotX, rotY, rotZ));
			Scale = new OpenTK.Vector3(scaleX, scaleY, scaleZ);

            HasRoomNumber = true;
		}

		public override void Save(string path)
		{
			throw new NotImplementedException();
		}
	}
}
