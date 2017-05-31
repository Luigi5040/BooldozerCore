using System;
using GameFormatReader.Common;
using System.Collections.Generic;

namespace Booldozer.Models.Mdl
{
    public struct DrawElement
    {
        public short matIndex;
        public short shapeIndex;
        public DrawElement(EndianBinaryReader stream){
            matIndex = stream.ReadInt16();
            shapeIndex = stream.ReadInt16();
        }
    }

    public class Shape
    {
        byte unk0;
        byte unk1;
        byte unk2;
        byte unk3;
        public ushort count;
        public ushort first;

        public Shape(EndianBinaryReader stream)
        {
            unk0 = stream.ReadByte();
            unk1 = stream.ReadByte();
            unk2 = stream.ReadByte();
            unk3 = stream.ReadByte();
            count = stream.ReadUInt16();
            first = stream.ReadUInt16();
        }
    }

    public class ShapePacket
    {
        public uint dataOffset;
        public uint dataSize;
        public short unk0;
        public ushort numMatIndicies;
        public ushort[] matIndicies;
        public List<int> faces;

        public ShapePacket(EndianBinaryReader stream)
        {
            dataOffset = stream.ReadUInt32();
            dataSize = stream.ReadUInt32();
            unk0 = stream.ReadInt16();
            numMatIndicies = stream.ReadUInt16();
            matIndicies = new ushort[10];
            for (int i = 0; i < 10; i++)
            {
                matIndicies[i] = stream.ReadUInt16();
            }
        }
    }
    public class GXVertex
    {
        byte matIndex;
        byte Tex0;
        byte Tex1;
        public ushort posIndex;
        ushort normIndex;
        ushort colorIndex;
        ushort uvIndex;
        public GXVertex(EndianBinaryReader stream, ushort[] counts){
            stream.ReadByte(); //Mat Index
            stream.ReadByte(); //Tex0?
            stream.ReadByte(); //Tex1?
            posIndex = stream.ReadUInt16(); //pos
            if (counts[7] > 0)
            {
                stream.ReadUInt16(); //normal   
            }
            if (counts[8] > 0)
            {
                stream.ReadUInt16(); //color   
            }
            if (counts[9] > 0)
            {
                stream.ReadUInt16(); //uv   
            }
        }
    }
    public class Primitive
	{
		public byte type;
		public ushort count;
		public List<GXVertex> verts = new List<GXVertex>();

		public Primitive(EndianBinaryReader stream, ushort[] counts)
		{
			type = stream.ReadByte();
			count = stream.ReadUInt16();
			for (int i = 0; i < count; i++)
			{
				verts.Add(new GXVertex(stream, counts));
			}
		}

	}
}