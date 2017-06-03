using System;
using GameFormatReader.Common;
using System.Collections.Generic;

namespace Booldozer.Models.Mdl
{
    public class DrawElement : ISectionItem
    {
        public short matIndex;
        public short shapeIndex;
        public DrawElement(){}
        public void Load(EndianBinaryReader stream)
        {
            matIndex = stream.ReadInt16();
            shapeIndex = stream.ReadInt16();
        }
    }

    public class ShapePacket : ISectionItem
    {
        public uint dataOffset;
        public uint dataSize;
        public short unk0;
        public ushort numMatIndicies;
        public ushort[] matIndicies;
        public ShapePacket(){}
        public void Load(EndianBinaryReader stream)
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
}