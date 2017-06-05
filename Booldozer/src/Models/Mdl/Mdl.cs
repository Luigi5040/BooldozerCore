using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using GameFormatReader.Common;
using Booldozer.Materials;
using Booldozer.Models.GX;

namespace Booldozer.Models.Mdl
{
	public interface ISectionItem
	{
		void Load(EndianBinaryReader reader);
	}

	//Not the best way to handle this, but for not its fine
	public class vec3 : ISectionItem
	{
		public Vector3 v { get; private set; }
		public void Load(EndianBinaryReader reader)
		{
			v = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
	public class vec2 : ISectionItem
	{
		public Vector2 v { get; private set; }
		public void Load(EndianBinaryReader reader)
		{
			v = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		}
	}

	public class MdlModel : Mesh
	{
		ushort[] m_Counts; //20;
		long[] m_Offsets; //18
		List<Vector3> verticies;
		List<Vector3> normals;
		List<Vector2> uvs;
		List<DrawElement> drawelements;
		List<GXBatch> shapes;
		List<ShapePacket> shapepackets;
		List<Matrix4> globalMatrixTable;

		List<Material> materials;
		List<TexObj> texobjs;
		List<BinaryTextureImage> textures;

		public List<Vector3> fromVec3(List<vec3> l)
		{
			List<Vector3> nl = new List<Vector3>();
			foreach (var vec in l)
			{
				nl.Add(vec.v);
			}
			return nl;
		}

		public List<Vector2> fromVec2(List<vec2> l)
		{
			List<Vector2> nl = new List<Vector2>();
			foreach (var vec in l)
			{
				nl.Add(vec.v);
			}
			return nl;
		}

		public void WriteObj(string f)
		{
			//string dirPath = Path.GetDirectoryName(f);
			string fileName = Path.GetFileNameWithoutExtension(f);

			StringWriter objWriter = new StringWriter();
			StringWriter mtlWriter = new StringWriter();

			objWriter.WriteLine("# dumped with booldozer");
			objWriter.WriteLine($"mtllib { fileName }.mtl");

			foreach (var vert in verticies)
			{
				objWriter.WriteLine($"v {vert.X} {vert.Y} {vert.Z}");
			}

			if (normals.Count != 0)
			{
				foreach (var vert in normals)
					objWriter.WriteLine($"vn { vert.X } { vert.Y } { vert.Z }");
			}

			if (uvs.Count != 0)
			{
				foreach (var vert in uvs)
					objWriter.WriteLine($"vt { vert.X } { 1 - vert.Y }");
			}

			objWriter.WriteLine();

			int index = 0;
			foreach (DrawElement drw in drawelements)
			{
				Material mat = materials[drw.matIndex];
				GXBatch shp = shapes[drw.shapeIndex];

				mtlWriter.WriteLine($"newmtl { index }");
				mtlWriter.WriteLine($"Kd { ((mat.color & 0xFF000000) >> 24) / 255 } { ((mat.color & 0x00FF0000) >> 16) / 255 } { ((mat.color & 0x0000FF00) >> 8) / 255 }");
				mtlWriter.WriteLine($"d { (mat.color & 0x000000FF) / 255 }");

				if (mat.num_tev_stages > 0)
				{
					TexObj texObj = texobjs[mat.stages[0].texobj_index];
					mtlWriter.WriteLine($"map_Kd { index }.png");
					BinaryTextureImage tex = textures[texObj.textureIndex];
					tex.SaveImageToDisk($"{ index }.png", tex.GetData(), tex.Width, tex.Height);
				}

				objWriter.WriteLine($"o { index }");
				objWriter.WriteLine($"usemtl { index }");

				for (int i = 0; i < shp.RawVertices.Count; i += 3)
				{
					string[] verts = new string[3] { "", "", "" };

					for (int j = 0; j < 3; j++)
					{
						string pos = "";
						string uv = "";
						string norm = "";

						if (shp.ActiveAttributes.Contains(GXAttribute.Position))
							pos = $"{ Convert.ToString(shp.RawVertices[i + j].Indices[shp.ActiveAttributes.IndexOf(GXAttribute.Position)] + 1) }/";
						if (shp.ActiveAttributes.Contains(GXAttribute.Tex0))
							uv = $"{ Convert.ToString(shp.RawVertices[i + j].Indices[shp.ActiveAttributes.IndexOf(GXAttribute.Tex0)] + 1) }";
						if (shp.ActiveAttributes.Contains(GXAttribute.Normal))
							norm = $"/{ Convert.ToString(shp.RawVertices[i + j].Indices[shp.ActiveAttributes.IndexOf(GXAttribute.Normal)] + 1) }/";

						verts[j] = $"{ pos }{ uv }{ norm }";
					}

					objWriter.WriteLine($"f { verts[0] } { verts[1] } { verts[2] }");
				}

				index++;
			}

			using (FileStream s = new FileStream($"{ fileName }.obj", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(objWriter.ToString().ToCharArray());
			}

			using (FileStream s = new FileStream($"{ fileName }.mtl", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(mtlWriter.ToString().ToCharArray());
			}
		}

		public override void Render()
		{
			throw new NotImplementedException();
		}

		public List<T> LoadSection<T>(EndianBinaryReader stream, long offsetIndex, ushort m_CountIndex) where T : ISectionItem, new()
		{
			List<T> l = new List<T>();
			stream.BaseStream.Seek(m_Offsets[offsetIndex], 0);
			for (int i = 0; i < m_Counts[m_CountIndex]; i++)
			{
				var r = new T();
				r.Load(stream);
				l.Add(r);
			}
			return l;
		}

		public MdlModel()
		{
			m_Counts = new ushort[20];
			m_Offsets = new long[18];
		}
		public MdlModel(string path)
		{
			m_Counts = new ushort[20];
			m_Offsets = new long[18];
			shapes = new List<GXBatch>();
			globalMatrixTable = new List<Matrix4>();
			textures = new List<BinaryTextureImage>();

			using (FileStream fs = new FileStream(path, FileMode.Open))
			{
				EndianBinaryReader stream = new EndianBinaryReader(fs, Endian.Big);
				stream.ReadInt32(); //ignore the magic
				for (int i = 0; i < 20; i++)
				{
					m_Counts[i] = stream.ReadUInt16();
				}
				stream.BaseStream.Seek(0x30, 0);
				for (int i = 0; i < 18; i++)
				{
					m_Offsets[i] = stream.ReadUInt32();
				}

				verticies = fromVec3(LoadSection<vec3>(stream, 6, 6));
				normals = fromVec3(LoadSection<vec3>(stream, 7, 7));
				uvs = fromVec2(LoadSection<vec2>(stream, 9, 9));
				drawelements = LoadSection<DrawElement>(stream, 17, 17);
				shapepackets = LoadSection<ShapePacket>(stream, 1, 3);
				materials = LoadSection<Material>(stream, 14, 18);
				texobjs = LoadSection<TexObj>(stream, 15, 16);

				stream.BaseStream.Seek(m_Offsets[2], 0);
				for (int i = 0; i < m_Counts[5]; i++)
				{
					Matrix4 mat = new Matrix4(
						new Vector4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
						new Vector4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
						new Vector4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
						new Vector4(0, 0, 0, 1)
					);
					globalMatrixTable.Add(mat.Inverted());
				}
				for (int i = 0; i < m_Counts[4]; i++)
				{
					globalMatrixTable.Add(Matrix4.Identity);
				}

				stream.BaseStream.Seek(m_Offsets[16], 0);
				for (int i = 0; i < m_Counts[19]; i++)
				{
					GXBatch bat = new GXBatch();
					bat.LoadMdlBatch(stream, shapepackets, globalMatrixTable, verticies);
					shapes.Add(bat);
				}

				/*
				foreach (var packet in shapepackets)
				{
					//create local matrix table for shape packet
					Matrix4[] localMats = new Matrix4[packet.numMatIndicies];
					for (int i = 0; i < packet.numMatIndicies; i++)
					{
						if (packet.matIndicies[i] != 0xFFFF)
						{
							localMats[i] = globalMatrixTable[packet.matIndicies[i]];
						} else {break;}
					}
					//apply to shapes
					foreach (GXBatch shape in shapes)
					{
						if (shape.ActiveAttributes.Contains(GXAttribute.PositionMatrixIndex))
						{
							foreach (var vert in shape.RawVertices)
							{
								var matIndex = vert.Indices[shape.ActiveAttributes.IndexOf(GXAttribute.PositionMatrixIndex)];
								Console.WriteLine($"Mat Index: {matIndex}\nLocal Matrix List Size: {localMats.Length}");
								if(shape.ActiveAttributes.Contains(GXAttribute.PositionMatrixIndex)){
									Matrix4 mat = localMats[matIndex];
									Vector4 pos = new Vector4(verticies[vert.Indices[shape.ActiveAttributes.IndexOf(GXAttribute.Position)]]);
									Vector4.Transform(pos, mat);
									verticies[vert.Indices[shape.ActiveAttributes.IndexOf(GXAttribute.Position)]] = new Vector3(pos);
								}
							}
						}
					}
				}*/

				stream.BaseStream.Seek(m_Offsets[12], SeekOrigin.Begin);
				for (int i = 0; i < m_Counts[14]; i++)
				{
					int textureOffset = stream.ReadInt32();
					long nextOffsetPos = stream.BaseStream.Position;

					stream.BaseStream.Seek(textureOffset, SeekOrigin.Begin);
					BinaryTextureImage img = new BinaryTextureImage();
					img.Load(stream, textureOffset, 1);
					textures.Add(img);

					stream.BaseStream.Seek(nextOffsetPos, SeekOrigin.Begin);
				}

				WriteObj("mdlTest.obj");
			}
		}
	}
}