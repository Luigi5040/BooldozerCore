using System;
using GameFormatReader.Common;

namespace BooldozerCore.Models.GX
{
	public class GXVertex
	{
		public int[] Indices { get; private set; }

		public GXVertex()
		{
			
		}

		public GXVertex(EndianBinaryReader reader, int compCount)
		{
			Indices = new int[compCount];
			for (int i = 0; i < compCount; i++)
				Indices[i] = reader.ReadInt16();
		}

		public GXVertex(int[] indices)
		{
			Indices = indices;
		}
	}
}
