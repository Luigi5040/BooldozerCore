using System;
using System.IO;
using Assimp;
using OpenTK;
using GameFormatReader.Common;

namespace BooldozerCore.Collision
{
	public static class Util
	{
		public static Vector3 Vec3DToVec3(Vector3D assimpVec)
		{
			return new Vector3(assimpVec.X, assimpVec.Y, assimpVec.Z);
		}

		public static Vector3D Vec3ToVec3D(Vector3 openTKVec)
		{
			return new Vector3D(openTKVec.X, openTKVec.Y, openTKVec.Z);
		}

		public static void PadStream(EndianBinaryWriter writer, int padVal)
		{
			// Pad up to a 32 byte alignment
			// Formula: (x + (n-1)) & ~(n-1)
			long nextAligned = (writer.BaseStream.Length + (padVal - 1)) & ~(padVal - 1);

			long delta = nextAligned - writer.BaseStream.Length;
			writer.BaseStream.Position = writer.BaseStream.Length;
			for (int i = 0; i < delta; i++)
			{
				writer.Write((byte)0x40);
			}
		}
	}
}
