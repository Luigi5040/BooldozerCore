using System;
using GameFormatReader.Common;
using System.Collections.Generic;


namespace BooldozerCore.Models.Mdl
{

    public class TexObj : ISectionItem
    {
        public ushort textureIndex;
        byte unk1;
        byte unk2;
        byte unk3;

        public TexObj(){}

        public void Load(EndianBinaryReader stream)
        {
            textureIndex = stream.ReadUInt16();
            stream.SkipInt16();
            stream.Skip(3);  //these three are spread out for organizational purposes.
            stream.Skip(1);
        }

    }

    public class TevStage
    {
        ushort unk0;
        public ushort texobj_index;
        //float[] unk1 = new float[7];
        public TevStage(EndianBinaryReader stream)
        {
            unk0 = stream.ReadUInt16();
            texobj_index = stream.ReadUInt16();
            stream.Skip(28);
        }
    }

    public class Material : ISectionItem
    {
        public uint color;
        public ushort unk1;
        public byte unk2;
        public byte num_tev_stages;
        public byte unk4;
        //23 bytes of padding
        public TevStage[] stages = new TevStage[8];

        public Material(){}

        public void Load(EndianBinaryReader stream)
        {
            color = stream.ReadUInt32();
            unk1 = stream.ReadUInt16();
            unk2 = stream.ReadByte();
            num_tev_stages = stream.ReadByte();
            unk4 = stream.ReadByte();

			stream.Skip(23);

            for (int i = 0; i < 8; i++)
            {
                stages[i] = new TevStage(stream);
            }
        }
    }
}