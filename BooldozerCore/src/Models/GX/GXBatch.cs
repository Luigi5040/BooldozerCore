using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GameFormatReader.Common;
using BooldozerCore.Models.Mdl;
using BooldozerCore.Materials;

namespace BooldozerCore.Models.GX
{
	public class GXBatch
	{
		public List<GXAttribute> ActiveAttributes { get; private set; }
		public List<GXVertex> RawVertices { get; private set; }
		public BinaryTextureImage Texture { get; private set; }

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

		public void LoadMdlBatch(EndianBinaryReader reader, List<ShapePacket> packets, List<Matrix4> gMatTable, List<Vector3> verts)
		{
			uint attributeField = reader.ReadUInt32();
			ushort packetCount = reader.ReadUInt16();
			ushort firstPacketIndex = reader.ReadUInt16();

			long nextPos = reader.BaseStream.Position;

			int mask = 1;
			for (int i = 0; i < 24; i++)
			{
				int attrib = (int)(attributeField & mask) >> i;

				if (attrib == 1)
					ActiveAttributes.Add((GXAttribute)i);

				mask = mask << 1;
			}

			for (int i = 0; i < packetCount; i++)
			{
				reader.BaseStream.Seek(packets[i + firstPacketIndex].dataOffset, System.IO.SeekOrigin.Begin);
				List<GXVertex> CurrentPrims = ReadMdlPrimitives(reader, packets[i + firstPacketIndex]);

				var p = packets[i + firstPacketIndex];
				Matrix4[] localMats = new Matrix4[p.numMatIndicies];
				for (int j = 0; j < p.numMatIndicies; j++)
				{
					if (p.matIndicies[j] == 0xFFFF)
						continue;

					localMats[j] = gMatTable[p.matIndicies[j]];
				}

				List<int> done = new List<int>();
				foreach (var v in CurrentPrims)
				{
					if (done.Contains(v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]))
						continue;

					if (ActiveAttributes.Contains(GXAttribute.PositionMatrixIndex) && v.Indices[ActiveAttributes.IndexOf(GXAttribute.PositionMatrixIndex)] != 0xFF)
					{
						Matrix4 mat = localMats[v.Indices[ActiveAttributes.IndexOf(GXAttribute.PositionMatrixIndex)]];

						Matrix4 pos = new Matrix4(
							new Vector4(verts[v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]].X, 0, 0, 0),
							new Vector4(verts[v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]].Y, 0, 0, 0),
							new Vector4(verts[v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]].Z, 0, 0, 0),
							new Vector4(1, 0, 0, 0)
						);
						Matrix4 newV = Matrix4.Mult(mat, pos);
						verts[v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]] = new Vector3(newV.M11, newV.M21, newV.M31);


						//verts[v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]] = Vector3.TransformVector(verts[v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]], mat);

						done.Add(v.Indices[ActiveAttributes.IndexOf(GXAttribute.Position)]);
					}
				}

				RawVertices.AddRange(CurrentPrims);
			}

			reader.BaseStream.Seek(nextPos, System.IO.SeekOrigin.Begin);
			//UploadBufferData();
		}

		private List<GXVertex> ReadMdlPrimitives(EndianBinaryReader reader, ShapePacket curPak)
		{
			// OK. As far as we can tell, mdl suckerpunches our normal
			// understanding of the GC's GX, so we have to deal with a
			// hardcoded implementation.

			List<GXVertex> outList = new List<GXVertex>();

			GXPrimitiveType curPrim = (GXPrimitiveType)reader.ReadByte();
			while (curPrim != GXPrimitiveType.None)
			{
				List<GXVertex> tempVerts = new List<GXVertex>();
				ushort vertCount = reader.ReadUInt16();

				for (int i = 0; i < vertCount; i++)
				{
					List<int> tempList = new List<int>();

					byte mtxPos1 = (byte)(reader.ReadByte() / 3);
					byte mtxPos2 = (byte)(reader.ReadByte() / 3);
					byte mtxPos3 = (byte)(reader.ReadByte() / 3);



					if (mtxPos1 != mtxPos2)
					{
					}

					ushort posIndex = reader.ReadUInt16();
					ushort normalIndex = reader.ReadUInt16();
					ushort tex0Index = reader.ReadUInt16();

					if (ActiveAttributes.Contains(GXAttribute.PositionMatrixIndex))
						tempList.Add((int)mtxPos1);
					if (ActiveAttributes.Contains(GXAttribute.Position))
						tempList.Add(posIndex);
					if (ActiveAttributes.Contains(GXAttribute.Normal))
						tempList.Add(normalIndex);
					if (ActiveAttributes.Contains(GXAttribute.Tex0))
						tempList.Add(tex0Index);

					tempVerts.Add(new GXVertex(tempList.ToArray()));
				}

				outList.AddRange(ConvertTopologyToTriangles(curPrim, tempVerts));
				curPrim = (GXPrimitiveType)reader.ReadByte();
			}

			return outList;
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
