using System;
using System.IO;
using GameFormatReader.Common;

namespace BooldozerCore.Collision
{
	public partial class CollisionMesh
	{
		private void GenerateAllCells()
		{
			int xCellCount = (int)(Math.Floor(BBox.AxisLengths.X / 256) + 1);
			int yCellCount = (int)(Math.Floor(BBox.AxisLengths.Y / 512) + 1);
			int zCellCount = (int)(Math.Floor(BBox.AxisLengths.Z / 256) + 1);

			float xCellSize = BBox.AxisLengths.X / xCellCount;
			float yCellSize = BBox.AxisLengths.Y / yCellCount;
			float zCellSize = BBox.AxisLengths.Z / zCellCount;

			float curX = BBox.Minimum.X;
			float curY = BBox.Minimum.Y;
			float curZ = BBox.Minimum.Z;

			StringWriter wrtr = new StringWriter();

			for (int k = 0; k <= zCellCount; k++)
			{
				for (int j = 0; j <= yCellCount; j++)
				{
					for (int i = 0; i <= xCellCount; i++)
					{
						wrtr.WriteLine($"v { curX } { curY } { curZ }");

						curX += xCellSize;
					}

					curX = BBox.Minimum.X;
					curY += yCellSize;
				}

				/*
				wrtr.WriteLine($"v { curX } { curY } { curZ }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY } { curZ }");
				wrtr.WriteLine($"v { curX } { curY + yCellSize } { curZ }");
				wrtr.WriteLine($"v { curX } { curY } { curZ + zCellSize }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY + yCellSize } { curZ }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY } { curZ + zCellSize }");
				wrtr.WriteLine($"v { curX } { curY + yCellSize } { curZ + zCellSize }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY + yCellSize } { curZ + zCellSize }");
				*/

				curY = BBox.Minimum.Y;
				curZ += zCellSize;
			}

			/*
			for (int i = 0; i < xCellCount + yCellCount + zCellCount; i++)
			{
				wrtr.WriteLine($"o { i }");

				wrtr.WriteLine($"f { (i * 8) + 1 } { (i * 8) + 2 } { (i * 8) + 3 }");
				wrtr.WriteLine($"f { (i * 8) + 2 } { (i * 8) + 5 } { (i * 8) + 3 }");

				wrtr.WriteLine($"f { (i * 8) + 4 } { (i * 8) + 1 } { (i * 8) + 3 }");
				wrtr.WriteLine($"f { (i * 8) + 4 } { (i * 8) + 3 } { (i * 8) + 7 }");

				wrtr.WriteLine($"f { (i * 8) + 2 } { (i * 8) + 5 } { (i * 8) + 6 }");
				wrtr.WriteLine($"f { (i * 8) + 6 } { (i * 8) + 5 } { (i * 8) + 8 }");

				wrtr.WriteLine($"f { (i * 8) + 6 } { (i * 8) + 4 } { (i * 8) + 7 }");
				wrtr.WriteLine($"f { (i * 8) + 6 } { (i * 8) + 7 } { (i * 8) + 8 }");

				wrtr.WriteLine($"f { (i * 8) + 5 } { (i * 8) + 3 } { (i * 8) + 7 }");
				wrtr.WriteLine($"f { (i * 8) + 5 } { (i * 8) + 7 } { (i * 8) + 8 }");

				wrtr.WriteLine($"f { (i * 8) + 1 } { (i * 8) + 2 } { (i * 8) + 6 }");
				wrtr.WriteLine($"f { (i * 8) + 1 } { (i * 8) + 6 } { (i * 8) + 4 }");
			}*/

			using (FileStream strm = new FileStream(@"D:\SZS Tools\Luigi's Mansion\ManGrid.obj", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);
				writer.Write(wrtr.ToString().ToCharArray());

			}
		}

		private void GenerateFilledCells(EndianBinaryReader reader)
		{
			StringWriter strWriter = new StringWriter();

			int xCellCount = (int)(Math.Floor(BBox.AxisLengths.X / 256.0f) + 1);
			int yCellCount = (int)(Math.Floor(BBox.AxisLengths.Y / 512.0f) + 1);
			int zCellCount = (int)(Math.Floor(BBox.AxisLengths.Z / 256.0f) + 1);

			float xCellSize = BBox.AxisLengths.X / xCellCount;
			float yCellSize = BBox.AxisLengths.Y / yCellCount;
			float zCellSize = BBox.AxisLengths.Z / zCellCount;

			float curX = BBox.Minimum.X;
			float curY = BBox.Minimum.Y;
			float curZ = BBox.Minimum.Z;

			for (int zCoord = 0; zCoord < zCellCount; zCoord++)
			{
				for (int yCoord = 0; yCoord < yCellCount; yCoord++)
				{
					for (int xCoord = 0; xCoord < xCellCount; xCoord++)
					{
						int index1 = reader.ReadInt32();
						int index2 = reader.ReadInt32();

						if (index1 != 0 || index2 != 0)
							strWriter.WriteLine($"v { curX + (xCellSize / 2) } { curY + (yCellSize / 2) } { curZ + (zCellSize / 2)}");

						curX += xCellSize;
					}

					curX = BBox.Minimum.X;
					curY += yCellSize;
				}

				curY = BBox.Minimum.Y;
				curZ += zCellSize;
			}

			using (FileStream objOut = new FileStream(@"D:\SZS Tools\Luigi's Mansion\gridOut.obj", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter objWriter = new EndianBinaryWriter(objOut, Endian.Big);
				objWriter.Write(strWriter.ToString().ToCharArray());
			}
		}
	}
}
