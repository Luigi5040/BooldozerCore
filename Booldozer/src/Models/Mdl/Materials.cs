using System;
using GameFormatReader.Common;
using System.Collections.Generic;


namespace Booldozer.Models.Mdl
{

    public class TexObj : ISectionItem
    {
        ushort textureIndex;
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
        ushort texobj_index;
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
        uint color;
        ushort unk1;
        byte unk2;
        byte num_tev_stages;
        byte unk4;
        //23 bytes of padding
        TevStage[] stages = new TevStage[8];

        public Material(){}

        public void Load(EndianBinaryReader stream)
        {
            color = stream.ReadUInt32();
            unk1 = stream.ReadUInt16();
            unk2 = stream.ReadByte();
            num_tev_stages = stream.ReadByte();
            unk4 = stream.ReadByte();
            for (int i = 0; i < 8; i++)
            {
                stages[i] = new TevStage(stream);
            }
        }
    }
}