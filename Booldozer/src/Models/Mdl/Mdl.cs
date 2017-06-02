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
		List<Matrix4x3> globalMatrixTable;

		List<Material> materials;
		List<TexObj> texobjs;

		public List<Vector3> fromVec3(List<vec3> l)
		{
			List<Vector3> nl = new List<Vector3>();
			foreach (var vec in l)
			{
				nl.Add(vec.v);
			}
			return nl;
		}

		public void WriteObj(string f)
		{
			string dirPath = Path.GetDirectoryName(f);
			string fileName = Path.GetFileNameWithoutExtension(f);

			StringWriter objWriter = new StringWriter();
			StringWriter mtlWriter = new StringWriter();

			objWriter.WriteLine("# dumped with booldozer");

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
					objWriter.WriteLine($"vt { vert.X } { vert.Y }");
			}

			objWriter.WriteLine();

			int index = 0;
			foreach (GXBatch bat in shapes)
			{
				objWriter.WriteLine($"o { index++ }");
				//objWriter.WriteLine($"usemtl { index++ }");

				for (int i = 0; i < bat.RawVertices.Count; i += 3)
				{
					string[] verts = new string[3] { "", "", "" };

					for (int j = 0; j < 3; j++)
					{
						string pos = "";
						string uv = "";
						string norm = "";

						if (bat.ActiveAttributes.Contains(GXAttribute.Position))
							pos = $"{ Convert.ToString(bat.RawVertices[i + j].Indices[bat.ActiveAttributes.IndexOf(GXAttribute.Position)] + 1) }/";
						if (bat.ActiveAttributes.Contains(GXAttribute.Tex0))
							uv = $"{ Convert.ToString(bat.RawVertices[i + j].Indices[bat.ActiveAttributes.IndexOf(GXAttribute.Tex0)] + 1) }";
						if (bat.ActiveAttributes.Contains(GXAttribute.Normal))
							norm = $"/{ Convert.ToString(bat.RawVertices[i + j].Indices[bat.ActiveAttributes.IndexOf(GXAttribute.Normal)] + 1) }/";

						verts[j] = $"{ pos }{ uv }{ norm }";
					}

					objWriter.WriteLine($"f { verts[0] } { verts[1] } { verts[2] }");
				}
			}

			using (FileStream s = new FileStream($"{ dirPath }\\{ fileName }.obj", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(objWriter.ToString().ToCharArray());
			}

			using (FileStream s = new FileStream($"{ dirPath }\\{ fileName }.mtl", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(mtlWriter.ToString().ToCharArray());
			}
		}

		public override void Load(EndianBinaryReader reader)
		{
			throw new NotImplementedException();
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
			uvs = new List<Vector2>();
			shapes = new List<GXBatch>();
			globalMatrixTable = new List<Matrix4x3>();

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
				drawelements = LoadSection<DrawElement>(stream, 17, 17);
				shapepackets = LoadSection<ShapePacket>(stream, 1, 3);
				materials = LoadSection<Material>(stream, 14, 18);
				texobjs = LoadSection<TexObj>(stream, 15, 16);

				stream.BaseStream.Seek(m_Offsets[9], 0);
				for (int i = 0; i < m_Counts[9]; i++)
				{
					uvs.Add(new Vector2(stream.ReadSingle(), stream.ReadSingle()));
				}

				stream.BaseStream.Seek(m_Offsets[2], 0);
                for (int i = 0; i < m_Counts[5]; i++)
                {
                    Matrix4x3 mat = new Matrix4x3(
                        new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
                        new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
                        new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()),
                        new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle())
                    );
                    globalMatrixTable.Add(mat);                    
                }

				stream.BaseStream.Seek(m_Offsets[16], 0);
				for (int i = 0; i < m_Counts[18]; i++)
				{
					GXBatch bat = new GXBatch();
					bat.LoadMdlBatch(stream, shapepackets);
					shapes.Add(bat);
				}

				WriteObj(@"D:\SZS Tools\Luigi's Mansion\MdlTest\MdlTest.obj");
			}
		}
	}
}