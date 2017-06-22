using System;
using System.Collections.Generic;
using System.Text;
using GameFormatReader.Common;

namespace BooldozerCore.Models.Mdl
{
    public class TXPEntry
    {
        short Unknown1;
        short MaterialIndex;

        short[] TexObjIndices;

        public TXPEntry(EndianBinaryReader reader, int frameCount)
        {
            TexObjIndices = new short[frameCount];

            Unknown1 = reader.ReadInt16();
            if (Unknown1 != 1)
                throw new Exception("Unknown1 in TXPEntry wasn't 1!");

            MaterialIndex = reader.ReadInt16();

            if (reader.ReadInt32() != 0)
                throw new Exception("Field at 0x4 in TXPEntry was not 0!");

            int frameDataOffset = reader.ReadInt32();
            long curOffset = reader.BaseStream.Position;
            reader.BaseStream.Seek(frameDataOffset, System.IO.SeekOrigin.Begin);

            for (int i = 0; i < frameCount; i++)
            {
                TexObjIndices[i] = reader.ReadInt16();
            }

            reader.BaseStream.Seek(curOffset, System.IO.SeekOrigin.Begin);
        }

        public void ApplyTexObjToMaterial(List<Material> materialList)
        {
            Material mat = materialList[MaterialIndex];
            mat.stages[0].texobj_index = (ushort)TexObjIndices[0];
        }
    }

    public class TXPFile
    {
        short Unknown1;
        short Unknown2;

        short EntryCount;
        short FrameCount;

        int FrameDataOffset;

        public TXPEntry[] TXPEntries { get; private set; }

        public TXPFile(EndianBinaryReader reader)
        {
            Unknown1 = reader.ReadInt16();
            if (Unknown1 != 1)
                throw new Exception("Unknown1 in TXPFile wasn't 1!");

            Unknown2 = reader.ReadInt16();
            if (Unknown2 != 0)
                throw new Exception("Unknown2 in TXPFile wasn't 0!");

            EntryCount = reader.ReadInt16();
            FrameCount = reader.ReadInt16();
            FrameDataOffset = reader.ReadInt32();

            TXPEntries = new TXPEntry[EntryCount];

            for (int i = 0; i < EntryCount; i++)
            {
                TXPEntries[i] = new TXPEntry(reader, FrameCount);
            }
        }
    }
}
