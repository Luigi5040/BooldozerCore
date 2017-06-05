using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GameFormatReader.Common;
using Booldozer.Materials;
using System.Drawing;
using Booldozer.Models.GX;
//using Assimp;

/*

    TODO: 
     - Clean up. A lot.
     - Add wrapping to textures and obj exporter

 */

namespace Booldozer.Models.Bin
{
	public class BinModel : Mesh
	{
		public List<GraphObject> Meshes;
		public List<Vector3> Vertices;
		public List<Vector3> Normals;
		public List<Vector2>[] UVs;
		public List<Color32>[] Colors;

		private uint[] m_Offsets = new uint[21];
		private string m_Name;

		private uint m_PositionCount;
		private uint m_NormalCount;
		private uint m_UV0Count;
		private uint m_Color0Count;
		private uint m_Color1Count;

		public BinModel()
		{
			Meshes = new List<GraphObject>();
			Vertices = new List<Vector3>();
			Normals = new List<Vector3>();
			UVs = new List<Vector2>[8];
			Colors = new List<Color32>[2];
		}

		public BinModel(string path)
		{
			Meshes = new List<GraphObject>();
			Vertices = new List<Vector3>();
			Normals = new List<Vector3>();
			UVs = new List<Vector2>[8] { new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>() };
			Colors = new List<Color32>[2] { new List<Color32>(), new List<Color32>() };

			using (FileStream fs = new FileStream(path, FileMode.Open))
			{
				EndianBinaryReader stream = new EndianBinaryReader(fs, Endian.Big);
				stream.Skip(1);

				//hacky but for now its ok
				var p = new string(stream.ReadChars(11));
				foreach (var c in Path.GetInvalidFileNameChars())
				{
					p = p.Replace(c, ' ').Trim();
				}

				m_Name = p;

				for (int i = 0; i < 21; i++)
				{
					m_Offsets[i] = stream.ReadUInt32();
				}

				GetGraphObjects(stream, 0);

				CalculateAttributeCounts(stream);
				LoadAttributeData(stream);

				stream.BaseStream.Seek(m_Offsets[2], 0);
				for (int i = 0; i < m_PositionCount; i++)
				{
					Vertices.Add(new Vector3(stream.ReadInt16(), stream.ReadInt16(), stream.ReadInt16()));
				}

				if (m_Offsets[3] != 0)
				{

				}
			}

			WriteObj(@"D:\SZS Tools\Luigi's Mansion\BinTest2\test.obj");
		}

		private void CalculateAttributeCounts(EndianBinaryReader reader)
		{
			// Position
			for (int i = 3; i < 21; i++)
			{
				if (m_Offsets[i] > 0)
				{
					m_PositionCount = (m_Offsets[i] - m_Offsets[2]) / 6;
					break;
				}
			}

			// Normals/Binormals/Tangents
			if (m_Offsets[3] != 0)
			{
				for (int i = 4; i < 21; i++)
				{
					if (m_Offsets[i] > 0)
					{
						m_NormalCount = (m_Offsets[i] - m_Offsets[3]) / 12;
						break;
					}
				}
			}

			// Color0
			if (m_Offsets[4] != 0)
			{
				for (int i = 5; i < 21; i++)
				{
					if (m_Offsets[i] > 0)
					{
						m_Color0Count = (m_Offsets[i] - m_Offsets[4]) / 4;
						break;
					}
				}
			}

			// Color1
			if (m_Offsets[5] != 0)
			{
				for (int i = 6; i < 21; i++)
				{
					if (m_Offsets[i] > 0)
					{
						m_Color1Count = (m_Offsets[i] - m_Offsets[5]) / 4;
						break;
					}
				}
			}

			// UV0
			if (m_Offsets[6] != 0)
			{
				for (int i = 7; i < 21; i++)
				{
					if (m_Offsets[i] > 0)
					{
						m_UV0Count = (m_Offsets[i] - m_Offsets[6]) / 8;
						break;
					}
				}
			}
		}

		private void LoadAttributeData(EndianBinaryReader reader)
		{
			// Positions
			if (m_PositionCount != 0)
			{
				reader.BaseStream.Seek(m_Offsets[2], SeekOrigin.Begin);

				for (int i = 0; i < m_PositionCount; i++)
					Vertices.Add(new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()));
			}

			// Normals
			if (m_NormalCount != 0)
			{
				reader.BaseStream.Seek(m_Offsets[3], SeekOrigin.Begin);

				for (int i = 0; i < m_NormalCount; i++)
					Normals.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
			}
			// Color0
			if (m_Color0Count != 0)
			{
				reader.BaseStream.Seek(m_Offsets[4], SeekOrigin.Begin);

				for (int i = 0; i < m_Color0Count; i++)
					Colors[0].Add(new Color32() { R = reader.ReadByte(), G = reader.ReadByte(), B = reader.ReadByte(), A = reader.ReadByte() });
			}
			// Color1
			if (m_Color1Count != 0)
			{
				reader.BaseStream.Seek(m_Offsets[5], SeekOrigin.Begin);

				for (int i = 0; i < m_Color1Count; i++)
					Colors[1].Add(new Color32() { R = reader.ReadByte(), G = reader.ReadByte(), B = reader.ReadByte(), A = reader.ReadByte() });
			}
			// UV0
			if (m_UV0Count != 0)
			{
				reader.BaseStream.Seek(m_Offsets[6], SeekOrigin.Begin);

				for (int i = 0; i < m_UV0Count; i++)
					UVs[0].Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
			}
		}

		private void GetGraphObjects(EndianBinaryReader stream, int index)
		{
			stream.BaseStream.Seek(m_Offsets[12] + (0x8C * index), 0);
			var obj = new GraphObject(stream);
			stream.BaseStream.Seek(obj.partOffset + m_Offsets[12], 0);

			for (int i = 0; i < obj.partCount; i++)
			{
				obj.MeshParts.Add(new GraphObjectPart(stream, m_Offsets));
			}

			Meshes.Add(obj);

			if (obj.childIndex >= 0)
			{
				GetGraphObjects(stream, obj.childIndex);
			}

			if (obj.nextIndex >= 0)
			{
				GetGraphObjects(stream, obj.nextIndex);
			}
		}

		public void WriteObj(string outFile)
		{
			string outDir = Path.GetDirectoryName(outFile);

			StringWriter objWriter = new StringWriter(); // Writes the OBJ geometry
			StringWriter mtlWriter = new StringWriter(); // Writes the texture maps to mtl

			objWriter.WriteLine($"mtllib { m_Name }.mtl"); // Material library name reference

			// Write vertices
			for (int i = 0; i < Vertices.Count; i++)
				objWriter.WriteLine($"v { Vertices[i].X } { Vertices[i].Y } { Vertices[i].Z }");

			// Write UV0s, if present
			if (UVs[0].Count != 0)
			{
				for (int i = 0; i < UVs[0].Count; i++)
					objWriter.WriteLine($"vt { UVs[0][i].X } { 1 - UVs[0][i].Y }");
			}

			// Write normals, if present
			if (Normals.Count != 0)
			{
				for (int i = 0; i < Normals.Count; i++)
					objWriter.WriteLine($"vn { Normals[i].X } { Normals[i].Y } { Normals[i].Z }");
			}

			objWriter.WriteLine();

			int texCount = 0;
			int partCount = 0;
			foreach (GraphObject obj in Meshes)
			{
				foreach (GraphObjectPart part in obj.MeshParts)
				{
					foreach (var mat in part.shader.materials)
					{
						// Output textures
						if (mat != null)
						{
							var cTex = mat.texture;
							cTex.SaveImageToDisk($"{ outDir }\\{ texCount }.png", cTex.GetData(), cTex.Width, cTex.Height);

							mtlWriter.WriteLine($"newmtl { partCount }"); // New material for part
							mtlWriter.WriteLine($"map_kd { texCount }.png"); // Set diffuse texture to the texture we just dumped

							texCount++;
						}
					}

					objWriter.WriteLine($"o { partCount }"); // New object for part for clarity
					objWriter.WriteLine($"usemtl { partCount++ }"); // Material reference for part

					for (int i = 0; i < part.batch.RawVertices.Count; i += 3)
					{
						string[] verts = new string[] { "", "", "" };

						for (int j = 0; j < 3; j++)
						{
							string pos = "";
							string uv = "";
							string norm = "";

							// Position index. Has the divider / at the end.
							if (part.batch.ActiveAttributes.Contains(GXAttribute.Position))
								pos = $"{ Convert.ToString(part.batch.RawVertices[i + j].Indices[part.batch.ActiveAttributes.IndexOf(GXAttribute.Position)] + 1) }/";

							// UV index. Might be absent. Has no / dividers.
							if (part.batch.ActiveAttributes.Contains(GXAttribute.Tex0))
								uv = $"{ Convert.ToString(part.batch.RawVertices[i + j].Indices[part.batch.ActiveAttributes.IndexOf(GXAttribute.Tex0)] + 1) }";

							// Normal index. Might be absent. Has / divider at the beginning.
							if (part.batch.ActiveAttributes.Contains(GXAttribute.Normal))
								norm = $"/{ Convert.ToString(part.batch.RawVertices[i + j].Indices[part.batch.ActiveAttributes.IndexOf(GXAttribute.Normal)] + 1) }";

							// With this / divider setup, the possible combinations are:
							// position
							// position/uv
							// position//normal
							// position/uv/normal
							// which is what we need

							verts[j] = $"{pos}{uv}{norm}";
						}

						objWriter.WriteLine($"f { verts[0] } { verts[1] } { verts[2] }");
					}
				}
			}

			// Output OBJ
			using (FileStream s = new FileStream($"{ outDir }\\{ m_Name }.obj", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(objWriter.ToString().ToCharArray());
			}

			// Output MTL
			using (FileStream s = new FileStream($"{ outDir }\\{ m_Name }.mtl", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(mtlWriter.ToString().ToCharArray());
			}
		}

		public override void Render()
		{
			throw new NotImplementedException();
		}
	}
}