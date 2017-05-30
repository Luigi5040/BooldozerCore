using System;
using System.IO;
using GameFormatReader.Common;
using OpenTK;
using System.Text;
using System.Collections.Generic;
using Booldozer.Util;

namespace Booldozer.Collision
{
	public partial class Collision
	{
		#region Input

		private void OpenCompiled(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(strm, Endian.Big);

				// If these turn out to change, this will tell us. Otherwise they should always be these values
				if (reader.ReadSingle() != 256.0f)
					throw new FormatException("Static X scale was not 256!");
				if (reader.ReadSingle() != 512.0f)
					throw new FormatException("Static Y scale was not 512!");
				if (reader.ReadSingle() != 256.0f)
					throw new FormatException("Static Z scale was not 256!");

				BBox = new BoundingBox(
					new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
					new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));

				int vertexDataOffset = reader.ReadInt32();
				int normalizedDataOffset = reader.ReadInt32();
				int faceDataOffset = reader.ReadInt32();
				int trianglGroupsOffset = reader.ReadInt32();
				int gridIndexData1Offset = reader.ReadInt32();
				int gridIndexData2Offset = reader.ReadInt32(); // This is the offset the game uses. Don't know about the first one.
				int unk4DataOffset = reader.ReadInt32();

				int numVertices = (normalizedDataOffset - vertexDataOffset) / 0x0C; // Vector3s are 12/0xC bytes long
				int numNormalized = (faceDataOffset - normalizedDataOffset) / 0x0C; // See above
				int numFaces = (trianglGroupsOffset - faceDataOffset) / 0x18; // Face entries are 24/0x18 bytes long

				reader.BaseStream.Seek(vertexDataOffset, SeekOrigin.Begin);

				for (int i = 0; i < numVertices; i++)
				{
					Vertices.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
				}

				BBox = new BoundingBox(Vertices);

				reader.BaseStream.Seek(normalizedDataOffset, SeekOrigin.Begin);

				for (int i = 0; i < numNormalized; i++)
				{
					NormalizedVectors.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
				}

				reader.BaseStream.Seek(faceDataOffset, SeekOrigin.Begin);

				//StringWriter statWriter = new StringWriter();

				for (int i = 0; i < numFaces; i++)
				{
					Triangles.Add(new Triangle(reader));


					/*statWriter.WriteLine(NormalizedVectors[Triangles[i].NormalIndex]);
					statWriter.WriteLine(NormalizedVectors[Triangles[i].Edge1TangentIndex]);
					statWriter.WriteLine(NormalizedVectors[Triangles[i].Edge2TangentIndex]);
					statWriter.WriteLine(NormalizedVectors[Triangles[i].Edge3TangentIndex]);
					statWriter.WriteLine(NormalizedVectors[Triangles[i].PlanePointIndex]);
					statWriter.WriteLine(Triangles[i].PlaneDValue);
					statWriter.WriteLine();*/
				}

				/*using (FileStream objOut = new FileStream(@"D:\SZS Tools\Luigi's Mansion\stats_custom.txt", FileMode.Create, FileAccess.Write))
				{
					EndianBinaryWriter objWriter = new EndianBinaryWriter(objOut, Endian.Big);
					objWriter.Write(statWriter.ToString().ToCharArray());
				}*/

				reader.BaseStream.Seek(gridIndexData1Offset, SeekOrigin.Begin);
			}

			string jmpFolderName = $"{ Path.GetDirectoryName(fileName) }\\jmp";
			if (!Directory.Exists(jmpFolderName))
			{
				Console.WriteLine("No jmp folder was found. Skipping collision properties...");
				return;
			}

			string colPropsFileName = $" { jmpFolderName }\\polygoninfo";
			if (File.Exists(colPropsFileName))
			{
				LoadCompiledColProperties(colPropsFileName);
			}
			else
			{
				Console.WriteLine("No polygoninfo file was found. Skipping collision properties...");
			}

			string sndPropsFileName = $" { jmpFolderName }\\soundpolygoninfo";
			if (File.Exists(sndPropsFileName))
			{
				LoadCompiledSndProperties(sndPropsFileName);
			}
			else
			{
				Console.WriteLine("No soundpolygoninfo file was found. Skipping sound properties...");

			}
		}

		private void LoadCompiledColProperties(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(strm, Endian.Big);

				if (reader.ReadInt32() != Triangles.Count)
				{
					Console.WriteLine("Triangle-collision property count mismatch!");
					return;
				}

				reader.BaseStream.Seek(0x34, SeekOrigin.Begin);

				for (int i = 0; i < Triangles.Count; i++)
				{
					Triangles[i].ReadCompiledColProperties(reader);
				}
			}
		}

		private void LoadCompiledSndProperties(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(strm, Endian.Big);

				if (reader.ReadInt32() != Triangles.Count)
				{
					Console.WriteLine("Triangle-sound property count mismatch!");
					return;
				}

				reader.BaseStream.Seek(0x28, SeekOrigin.Begin);

				for (int i = 0; i < Triangles.Count; i++)
				{
					Triangles[i].ReadCompiledSndProperties(reader);
				}

			}
		}

		#endregion

		#region Output

		private void SaveCompiled(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);

				// Static scale, which is always (256, 512, 256)
				writer.Write(StaticScale.X);
				writer.Write(StaticScale.Y);
				writer.Write(StaticScale.Z);

				BBox.WriteBoundingBox(writer); // Bounding box, min then max

				writer.Write((int)0x40); // Vertex offset, always 0x40
				writer.Write((int)0); // Placeholder for normalized data offset
				writer.Write((int)0); // Placeholder for triangle data offset
				writer.Write((int)0); // Placeholder for triangle group data offset
				writer.Write((int)0); // Placeholder for grid index data 1 offset
				writer.Write((int)0); // Placeholder for grid index data 2 offset
				writer.Write((int)0); // Placeholder for ??? data offset

				List<Vector3> vertexExport = new List<Vector3>();
				foreach (Triangle tri in Triangles)
				{
					for (int i = 0; i < 3; i++)
					{
						Vector3 vec = Vertices[tri.VertexIndices[i]];

						if (!vertexExport.Contains(vec))
						{
							vertexExport.Add(vec);
						}

						tri.VertexIndices[i] = vertexExport.IndexOf(vec);
					}
				}

				// Write vertexes
				foreach (Vector3 vec in vertexExport)
				{
					writer.Write(vec.X);
					writer.Write(vec.Y);
					writer.Write(vec.Z);
				}

				// Write offset to normal data
				writer.BaseStream.Seek(0x28, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				List<Vector3> normalizedExport = new List<Vector3>();
				foreach (Triangle tri in Triangles)
				{
					Vector3 normal = NormalizedVectors[tri.NormalIndex];
					if (!normalizedExport.Contains(normal))
						normalizedExport.Add(normal);
					tri.NormalIndex = normalizedExport.IndexOf(normal);

					Vector3 edge1 = NormalizedVectors[tri.Edge1TangentIndex];
					Vector3 neg = -edge1;
					if (!normalizedExport.Contains(-edge1))
						normalizedExport.Add(-edge1);
					tri.Edge1TangentIndex = normalizedExport.IndexOf(-edge1);

					Vector3 edge2 = NormalizedVectors[tri.Edge2TangentIndex];
					if (!normalizedExport.Contains(edge2))
						normalizedExport.Add(edge2);
					tri.Edge2TangentIndex = normalizedExport.IndexOf(edge2);

					Vector3 edge3 = NormalizedVectors[tri.Edge3TangentIndex];
					if (!normalizedExport.Contains(edge3))
						normalizedExport.Add(edge3);
					tri.Edge3TangentIndex = normalizedExport.IndexOf(edge3);

					Vector3 unk1 = NormalizedVectors[tri.PlanePointIndex];
					if (!normalizedExport.Contains(-unk1))
						normalizedExport.Add(-unk1);
					tri.PlanePointIndex = normalizedExport.IndexOf(-unk1);
				}

				// Write normals
				foreach (Vector3 vec in normalizedExport)
				{
					writer.Write(vec.X);
					writer.Write(vec.Y);
					writer.Write(vec.Z);
				}

				// Write offset to face data
				writer.BaseStream.Seek(0x2C, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				foreach (Triangle tri in Triangles)
				{
					tri.WriteCompiledTriangle(writer);
				}

				int xCellCount, yCellCount, zCellCount;
				List<GridCell> grid = GenerateGrid(out xCellCount, out yCellCount, out zCellCount, vertexExport, normalizedExport);

				List<short> allTriangleIndexesForGrid = new List<short>();
				List<int> gridTriangleIndexes = new List<int>();
				allTriangleIndexesForGrid.Add(-1);

				for (int z = 0; z < zCellCount; z++)
				{
					for (int y = 0; y < yCellCount; y++)
					{
						for (int x = 0; x < xCellCount; x++)
						{
							int index = x + (y * xCellCount) + (z * xCellCount * yCellCount);

							if (grid[index].AllIntersectingTris.Count != 0)
							{
								gridTriangleIndexes.Add(allTriangleIndexesForGrid.Count);

								foreach (Triangle tri in grid[index].AllIntersectingTris)
									allTriangleIndexesForGrid.Add((short)Triangles.IndexOf(tri));
								allTriangleIndexesForGrid.Add(-1);
							}
							else if (y > 0)
							{
								int prevIndex = x + ((y - 1) * xCellCount) + (z * xCellCount * yCellCount);

								if (grid[prevIndex].AllIntersectingTris.Count != 0)
								{
									gridTriangleIndexes.Add(allTriangleIndexesForGrid.Count);
									foreach (Triangle tri in grid[prevIndex].AllIntersectingTris)
										allTriangleIndexesForGrid.Add((short)Triangles.IndexOf(tri));
									allTriangleIndexesForGrid.Add(-1);
								}
								else
									gridTriangleIndexes.Add(0);
							}
							else
								gridTriangleIndexes.Add(0);

							if (grid[index].FloorIntersectingTris.Count != 0)
							{
								gridTriangleIndexes.Add(allTriangleIndexesForGrid.Count);

								foreach (Triangle tri in grid[index].FloorIntersectingTris)
									allTriangleIndexesForGrid.Add((short)Triangles.IndexOf(tri));
								allTriangleIndexesForGrid.Add(-1);
							}
							else
								gridTriangleIndexes.Add(0);
						}
					}
				}

				/*
				foreach (GridCell cell in grid)
				{
					if (cell.AllIntersectingTris.Count != 0)
					{
						gridTriangleIndexes.Add(allTriangleIndexesForGrid.Count);
						foreach (Triangle tri in cell.AllIntersectingTris)
							allTriangleIndexesForGrid.Add((short)Triangles.IndexOf(tri));
						allTriangleIndexesForGrid.Add(-1);
					}
					else
						gridTriangleIndexes.Add(0);

					if (cell.FloorIntersectingTris.Count != 0)
					{
						gridTriangleIndexes.Add(allTriangleIndexesForGrid.Count);
						foreach (Triangle tri in cell.FloorIntersectingTris)
							allTriangleIndexesForGrid.Add((short)Triangles.IndexOf(tri));
						allTriangleIndexesForGrid.Add(-1);
					}
					else
						gridTriangleIndexes.Add(0);
				}*/

				allTriangleIndexesForGrid.Add(-1);

				// Write offset to triangle group data
				writer.BaseStream.Seek(0x30, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				foreach (short shr in allTriangleIndexesForGrid)
					writer.Write(shr);

				// Write offset to grid indices (twice)
				writer.BaseStream.Seek(0x34, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				foreach (int inte in gridTriangleIndexes)
					writer.Write(inte);

				Util.PadStream(writer, 32);
			}

			string jmpFolderName = $"{ Path.GetDirectoryName(fileName) }\\jmp";

			if (!Directory.Exists(jmpFolderName))
				Directory.CreateDirectory(jmpFolderName);

			string colPropertiesFileName = $"{ jmpFolderName }\\polygoninfo";
			using (FileStream strm = new FileStream(colPropertiesFileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);

				WriteCompiledColPropertiesHeader(writer);

				foreach (Triangle tri in Triangles)
					tri.WriteCompiledColProperties(writer);
				Util.PadStream(writer, 32);
			}


			string sndPropertiesFileName = $"{ jmpFolderName }\\soundpolygoninfo";
			using (FileStream strm = new FileStream(sndPropertiesFileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);

				WriteCompiledSndPropertiesHeader(writer);

				foreach (Triangle tri in Triangles)
					tri.WriteCompiledSndProperties(writer);
				Util.PadStream(writer, 32);

			}
		}

		private List<GridCell> GenerateGrid(out int xCellCount, out int yCellCount, out int zCellCount, List<Vector3> vertexExport, List<Vector3> normalExport)
		{
			List<GridCell> cells = new List<GridCell>();

			xCellCount = (int)(Math.Floor(BBox.AxisLengths.X / 256.0f) + 1);
			yCellCount = (int)(Math.Floor(BBox.AxisLengths.Y / 512.0f) + 1);
			zCellCount = (int)(Math.Floor(BBox.AxisLengths.Z / 256.0f) + 1);

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
						GridCell cell = new GridCell(new Vector3(curX, curY, curZ), new Vector3(curX + xCellSize, curY + yCellSize, curZ + zCellSize));

						for (int i = Triangles.Count - 1; i >= 0; i--)
						{
							cell.CheckTriangleBoundingBox(Triangles[i], vertexExport, normalExport);
						}
						cells.Add(cell);

						curX += xCellSize;
					}

					curX = BBox.Minimum.X;
					curY += yCellSize;
				}

				curY = BBox.Minimum.Y;
				curZ += zCellSize;
			}

			return cells;
		}

		private void WriteCompiledColPropertiesHeader(EndianBinaryWriter writer)
		{
			writer.Write(Triangles.Count);
			writer.Write((int)3);
			writer.Write((int)0x34);
			writer.Write((int)4);

			writer.Write((int)0x002AAF7F);
			writer.Write((int)3);
			writer.Write((int)0);

			writer.Write((int)0x01C2B94A);
			writer.Write((int)4);
			writer.Write((int)0x200);

			writer.Write((int)0x00AF2BA5);
			writer.Write((int)8);
			writer.Write((int)0x300);
		}

		private void WriteCompiledSndPropertiesHeader(EndianBinaryWriter writer)
		{
			writer.Write(Triangles.Count);
			writer.Write((int)2);
			writer.Write((int)0x28);
			writer.Write((int)4);

			writer.Write((int)0x006064D7);
			writer.Write((int)0xF);
			writer.Write((int)0);

			writer.Write((int)0x005169FA);
			writer.Write((int)0x70);
			writer.Write((int)0x400);
		}

		#endregion
	}
}
