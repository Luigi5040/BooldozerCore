using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using GameFormatReader.Common;
using Booldozer.Materials;
using System.Drawing;
//using Assimp;

/*

    TODO: 
     - Clen up. A lot.
     - Add wrapping to textures and obj exporter

 */

namespace Booldozer.Models.Bin
{
	public class BinModel
	{
		public List<GraphObject> Meshes;
		public List<Vector3> Vertices;
		public List<Vector3> Normals;
		public List<Vector2>[] UVs;
		public List<Color32>[] Colors;

		private uint[] m_Offsets = new uint[21];
		private string m_Name;

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
			UVs = new List<Vector2>[8];
			Colors = new List<Color32>[2];

			using (FileStream fs = new FileStream(path, FileMode.Open))
			{
				EndianBinaryReader stream = new EndianBinaryReader(fs, Encoding.GetEncoding("shift-jis"), Endian.Big);
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

				getGraphObjects(stream, 0);

				uint vertCount = 0;
				for (int i = 3; i < 21; i++)
				{
					if (m_Offsets[i] > 0)
					{
						vertCount = (m_Offsets[i] - m_Offsets[2]) / 6;
						break;
					}
				}

				stream.BaseStream.Seek(m_Offsets[2], 0);
				for (int i = 0; i < vertCount; i++)
				{
					Vertices.Add(new Vector3(stream.ReadInt16(), stream.ReadInt16(), stream.ReadInt16()));
				}

				WriteOBJ();
			}
		}

		private void getGraphObjects(EndianBinaryReader stream, int index)
		{
			stream.BaseStream.Seek(m_Offsets[12] + (0x8C * index), 0);
			var obj = new GraphObject(stream);
			stream.BaseStream.Seek(obj.partOffset + m_Offsets[12], 0);
			for (int i = 0; i < obj.partCount; i++)
			{
				//Console.WriteLine("Reading Part {0} at offset 0x{1:X}", i, stream.BaseStream.Position);
				obj.MeshParts.Add(new GraphObjectPart(stream, m_Offsets));
			}
			Meshes.Add(obj);
			if (obj.childIndex >= 0)
			{
				getGraphObjects(stream, obj.childIndex);
			}
			if (obj.nextIndex >= 0)
			{
				getGraphObjects(stream, obj.nextIndex);
			}
		}

		public void WriteOBJ(string filename = null)
		{
			if (filename == null)
			{
				filename = m_Name + ".obj";
			}

			StringWriter writer = new StringWriter();
			writer.WriteLine($"# Model \"{m_Name}\" dumped from bin by Booldozer v0.Ferns");
			writer.WriteLine();

			foreach (Vector3 v in Vertices)
			{
				writer.WriteLine($"v {v.X} {v.Y} {v.Z}");
			}

			writer.WriteLine();

			var curParts = 0;
			var texCount = 0;
			foreach (var mesh in Meshes)
			{
				writer.WriteLine($"g {m_Name}.{curParts}");
				curParts++;

				foreach (var part in mesh.MeshParts)
				{
					foreach (var mat in part.shader.materials)
					{
						if (mat != null)
						{
							var cTex = mat.texture;
							cTex.SaveImageToDisk($"{texCount}.png", cTex.GetData(), cTex.Width, cTex.Height);
							texCount++;
						}
					}

					foreach (var primitive in part.batch.primitives)
					{
						var verts = primitive.verts;
						switch ((GXPrimitiveType)primitive.type)
						{
							case GXPrimitiveType.Triangles:
								writer.WriteLine($"f {verts[0].posIndex + 1} {verts[1].posIndex + 1} {verts[2].posIndex + 1}");
								break;

							case GXPrimitiveType.TriangleStrip:
								for (int v = 2; v < verts.Count; v++)
								{
									bool even = v % 2 != 0;
									var tri = new int[3];
									tri[0] = verts[v - 2].posIndex;
									tri[1] = even ? verts[v].posIndex : verts[v - 1].posIndex;
									tri[2] = even ? verts[v - 1].posIndex : verts[v].posIndex;
									if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
									{
										writer.WriteLine($"f {tri[0] + 1} {tri[1] + 1} {tri[2] + 1}");
									}
								}
								break;

							case GXPrimitiveType.TriangleFan:
								for (int v = 1; v < verts.Count; v++)
								{
									var tri = new int[3];
									tri[0] = verts[v].posIndex + 1;
									tri[1] = verts[v + 1].posIndex + 1;
									tri[2] = verts[0].posIndex + 1;

									if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
									{
										writer.WriteLine($"f {tri[0]} {tri[1]} {tri[2]}");
									}
								}
								break;
						}
					}
				}
			}

			using (FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(writer.ToString().ToCharArray());
			}
		}
	}
}