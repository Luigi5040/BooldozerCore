using System;
using GameFormatReader.Common;
using System.Collections.Generic;

namespace Booldozer.Models.Bin
{
	public class GXVertex
	{
		public short matrixIndex;
		public short posIndex;
		public short normalIndex;
		public short binormalIndex;
		public short tangentIndex;
		public short[] colorIndex; //2
		public short[] uvIndex; //8

		public GXVertex(EndianBinaryReader stream, byte uvCount, byte nbt, uint attribs)
		{
			if ((attribs & (1 << 9)) != 0)
			{
				posIndex = stream.ReadInt16();
			}
			if ((attribs & (1 << 10)) != 0)
			{
				normalIndex = stream.ReadInt16();
				if (nbt != 0)
				{
					binormalIndex = stream.ReadInt16();
					tangentIndex = stream.ReadInt16();
				}
			}

			colorIndex = new short[2]; //{ stream.ReadInt16(), stream.ReadInt16() };
			if ((attribs & (1 << 11)) != 0)
			{
				colorIndex[0] = stream.ReadInt16();
			}
			if ((attribs & (1 << 12)) != 0)
			{
				colorIndex[1] = stream.ReadInt16();
			}
			uvIndex = new short[8];
			//Console.WriteLine("{0}", uvCount);
			for (int i = 0; i < uvCount; i++)
			{
				if ((attribs & (1 << (13 + i))) != 0)
				{
					uvIndex[i] = stream.ReadInt16();
				}
			}
		}
	}

	public class Primitive
	{
		public byte type;
		public short count;
		public List<GXVertex> verts = new List<GXVertex>();

		public Primitive(EndianBinaryReader stream, uint[] offsets, byte nbt, byte uvCount, uint attribs)
		{
			type = stream.ReadByte();
			count = stream.ReadInt16();
			for (int i = 0; i < count; i++)
			{
				verts.Add(new GXVertex(stream, uvCount, nbt, attribs));
			}
		}

	}

	public class Batch
	{
		public ushort faceCount;
		public ushort listSize;
		uint attribs;
		byte useNormals;
		byte positions;
		byte uvCount;
		byte nbt;
		uint offset;

		public List<Primitive> primitives = new List<Primitive>();

		public Batch(EndianBinaryReader stream, uint[] offsets)
		{
			faceCount = stream.ReadUInt16();
			listSize = stream.ReadUInt16();

			attribs = stream.ReadUInt32();

			useNormals = stream.ReadByte();
			positions = stream.ReadByte();
			uvCount = stream.ReadByte();
			nbt = stream.ReadByte();
			offset = stream.ReadUInt32();
			stream.Skip(8);

			var f = 0;
			bool knownPrimitive = true; //Hax?
			stream.BaseStream.Seek(offset + offsets[11], 0);
			while (f < faceCount && knownPrimitive)
			{
				var p = new Primitive(stream, offsets, nbt, uvCount, attribs);
				knownPrimitive = (p.type == 0 ? false : true);
				f += p.count - 2;
				primitives.Add(p);
			}
		}

	}
}
