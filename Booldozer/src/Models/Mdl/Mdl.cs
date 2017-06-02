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
		public Vector3 v {get; private set;}
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

		public List<Vector3> fromVec3(List<vec3> l){
			List<Vector3> nl = new List<Vector3>();
			foreach (var vec in l)
			{
				nl.Add(vec.v);
			}
			return nl;
		}

		public void WriteObj(string f)
		{
			StringWriter writer = new StringWriter();
			writer.WriteLine("#dumped with booldozer");
			foreach (var vert in verticies)
			{
				writer.WriteLine($"v {vert.X} {vert.Y} {vert.Z}");
			}

			if (normals.Count != 0)
			{
				foreach (var vert in normals)
					writer.WriteLine($"vn { vert.X } { vert.Y } { vert.Z }");
			}

			if (uvs.Count != 0)
			{
				foreach (var vert in uvs)
					writer.WriteLine($"vt { vert.X } { vert.Y }");	
			}

			writer.WriteLine();

			int index = 0;
			foreach (GXBatch bat in shapes)
			{
				writer.WriteLine($"o { index++ }");
				int posIndex = bat.ActiveAttributes.IndexOf(GXAttribute.Position);
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

					writer.WriteLine($"f { verts[0] } { verts[1] } { verts[2] }");
				}
			}

			using (FileStream s = new FileStream(f, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(writer.ToString().ToCharArray());
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

		public List<T> LoadSection<T>(EndianBinaryReader stream, long offsetIndex, ushort m_CountIndex) where T : ISectionItem, new(){
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

				stream.BaseStream.Seek(m_Offsets[9], 0);
				for (int i = 0; i < m_Counts[9]; i++)
				{
					uvs.Add(new Vector2(stream.ReadSingle(), stream.ReadSingle()));
				}

				stream.BaseStream.Seek(m_Offsets[16], 0);
				for (int i = 0; i < m_Counts[18]; i++)
				{
					GXBatch bat = new GXBatch();
					bat.LoadMdlBatch(stream, shapepackets);
					shapes.Add(bat);
				}

				WriteObj("derp.obj");
			}
		}
	}
}