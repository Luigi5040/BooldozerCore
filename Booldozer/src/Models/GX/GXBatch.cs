using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GameFormatReader.Common;

namespace Booldozer.Models.GX
{
	public class GXBatch
	{
		public List<GXAttribute> ActiveAttributes { get; private set; }
		public List<GXVertex> RawVertices { get; private set; }

		private int[] m_glIndices;
		private int m_glEbo;

		public GXBatch()
		{
			ActiveAttributes = new List<GXAttribute>();
			RawVertices = new List<GXVertex>();

			//m_glEbo = GL.GenBuffer();
		}

		public void LoadBinBatch(EndianBinaryReader reader, int batchDataOffset)
		{
			ushort faceCount = reader.ReadUInt16();
			ushort listSize = reader.ReadUInt16();
			uint attributeField = reader.ReadUInt32();

			int mask = 1;
			for (int i = 0; i < 26; i++)
			{
				int attrib = (int)(attributeField & mask) >> i;

				if (attrib == 1)
					ActiveAttributes.Add((GXAttribute)i);

				mask = mask << 1;
			}

			bool useNormals = reader.ReadBoolean();
			byte positions = reader.ReadByte();
			byte uvCount = reader.ReadByte();
			byte nbt = reader.ReadByte();

			int primDataOffset = reader.ReadInt32();
			reader.BaseStream.Seek(primDataOffset + batchDataOffset, System.IO.SeekOrigin.Begin);

			RawVertices = ReadPrimitives(reader);

			//UploadBufferData();
		}

		public void LoadMdlBatch(EndianBinaryReader reader)
		{
			UploadBufferData();
		}

		private List<GXVertex> ReadPrimitives(EndianBinaryReader reader)
		{
			List<GXVertex> verts = new List<GXVertex>();

			GXPrimitiveType curPrim = (GXPrimitiveType)reader.ReadByte();
			while (curPrim != GXPrimitiveType.None)
			{
				List<GXVertex> temp = new List<GXVertex>();

				ushort vertCount = reader.ReadUInt16();

				for (int i = 0; i < vertCount; i++)
					temp.Add(new GXVertex(reader, ActiveAttributes.Count));

				verts.AddRange(ConvertTopologyToTriangles(curPrim, temp));

				curPrim = (GXPrimitiveType)reader.ReadByte();
			}

			return verts;
		}

		private void UploadBufferData()
		{
			// Upload the data
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_glEbo);
			GL.BufferData(BufferTarget.ElementArrayBuffer, m_glIndices.Length * 4, m_glIndices, BufferUsageHint.StaticDraw);
		}

		private List<GXVertex> ConvertTopologyToTriangles(GXPrimitiveType originalType, List<GXVertex> vertices)
		{
			List<GXVertex> tris = new List<GXVertex>();

			switch (originalType)
			{
				case GXPrimitiveType.Triangles:
					return tris;

				case GXPrimitiveType.TriangleStrip:
					for (int v = 2; v < vertices.Count; v++)
					{
						bool even = v % 2 != 0;
						var tri = new GXVertex[3];
						tri[0] = vertices[v - 2];
						tri[1] = even ? vertices[v] : vertices[v - 1];
						tri[2] = even ? vertices[v - 1] : vertices[v];
						if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
						{
							tris.AddRange(tri);
						}
					}
					break;

				case GXPrimitiveType.TriangleFan:
					for (int v = 1; v < vertices.Count; v++)
					{
						var tri = new GXVertex[3];
						tri[0] = vertices[v];
						tri[1] = vertices[v + 1];
						tri[2] = vertices[0];

						if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
						{
							tris.AddRange(tri);
						}
					}
					break;
			}

			return tris;
		}
	}
}
