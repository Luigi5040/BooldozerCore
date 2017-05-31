using System;
using GameFormatReader.Common;
using Booldozer.Materials;

namespace Booldozer.Models.Bin
{
	public class Shader
	{
		uint tint;
		short[] materialIndex = new short[8];
		short[] unkIndex = new short[8];

		public Material[] materials = new Material[8];
		public Shader(EndianBinaryReader stream, uint[] offsets)
		{
			stream.Skip(3);
			tint = stream.ReadUInt32();
			stream.SkipByte();
			for (int i = 0; i < 8; i++)
			{
				materialIndex[i] = stream.ReadInt16();
				//Console.WriteLine($"Material Index: {materialIndex[i]}");
			}
			for (int i = 0; i < 8; i++)
			{
				unkIndex[i] = stream.ReadInt16();
			}
			for (int i = 0; i < 8; i++)
			{
				if (materialIndex[i] >= 0)
				{
					stream.BaseStream.Seek(offsets[1] + (0x14 * materialIndex[i]), 0);
					materials[i] = new Material(stream, offsets[0]);
				}
			}
		}
	}

	public class Material
	{
		public short textureIndex;
		short wrapU;
		short wrapV;
		//12 bytes of padding, remember this
		public BinaryTextureImage texture;
		public Material(EndianBinaryReader stream, uint texOffset)
		{
			//Console.WriteLine("Reading Material at 0x{0:X}", stream.BaseStream.Position);
			textureIndex = stream.ReadInt16();
			stream.SkipInt16();
			wrapU = stream.ReadByte();
			wrapV = stream.ReadByte();
			stream.SkipInt16();
			stream.Skip(12);
			texture = new BinaryTextureImage(stream, texOffset + (0xC * textureIndex), texOffset);
		}
	}
}
