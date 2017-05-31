using System;
using System.Collections.Generic;
using GameFormatReader.Common;
using OpenTK;

namespace Booldozer.Models.Bin
{
	public class GraphObject
	{
		public short parentIndex;
		public short childIndex;
		public short nextIndex;
		public short prevIndex;
		byte padding1;
		public byte renderFlag;
		public short padding2;
		public Vector3 scale;
		public Vector3 rot;
		public Vector3 pos;
		public Vector3 bbMin;
		public Vector3 bbMax;
		float unk1;

		public short partCount;
		short padding3;
		public int partOffset;

		public List<GraphObjectPart> MeshParts = new List<GraphObjectPart>();

		public GraphObject(EndianBinaryReader stream)
		{
			this.parentIndex = stream.ReadInt16();
			this.childIndex = stream.ReadInt16();
			this.nextIndex = stream.ReadInt16();
			this.prevIndex = stream.ReadInt16();
			this.padding1 = stream.ReadByte();
			this.renderFlag = stream.ReadByte();
			this.padding2 = stream.ReadInt16();
			this.scale = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
			this.rot = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
			this.pos = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
			this.bbMin = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
			this.bbMax = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
			this.unk1 = stream.ReadSingle();

			this.partCount = stream.ReadInt16();
			this.padding3 = stream.ReadInt16();
			this.partOffset = stream.ReadInt32();
		}
	}

	public struct GraphObjectPart
	{
		public short shaderIndex;
		public short batchIndex;
		public Batch batch;
		public Shader shader;

		public GraphObjectPart(EndianBinaryReader stream, uint[] offsets)
		{
			shaderIndex = stream.ReadInt16();
			batchIndex = stream.ReadInt16();
			var last = stream.BaseStream.Position;

			stream.BaseStream.Seek(offsets[11] + (0x18 * batchIndex), 0);
			batch = new Batch(stream, offsets);

			stream.BaseStream.Seek(offsets[10] + (0x28 * shaderIndex), 0);
			shader = new Shader(stream, offsets);
			stream.BaseStream.Seek(last, 0);
		}
	}
}
