using System;
using System.Collections.Generic;
using OpenTK;
using GameFormatReader.Common;
using Assimp;
//using Newtonsoft.Json;

namespace Booldozer.Collision
{
	public class Triangle
	{
		public class CollisionProperties
		{
			public enum CollisionMaterials
			{
				Normal,
				Ice,
				Unused1,
				Unused2
			}

			public CollisionMaterials ColMaterial;
			public bool IsLadder;
			public bool IgnorePointer;
		}

		public class SoundProperties
		{
			public enum SoundMaterials
			{
				dummy
			}

			public SoundMaterials SndMaterial;
			public int SndEchoSwitch;
		}

		public List<int> VertexIndices { get; private set; }
		public int NormalIndex { get; set; }
		public int Edge1TangentIndex { get; set; }
		public int Edge2TangentIndex { get; set; }
		public int Edge3TangentIndex { get; set; }
		public int PlanePointIndex { get; set; }
		public float PlaneDValue { get; private set; }
		public int Unknown1 { get; private set; }
		public int Unknown2 { get; private set; }

		public CollisionProperties ColProperties { get; private set; }
		public SoundProperties SndProperties { get; private set; }

		public Triangle()
		{
			ColProperties = new CollisionProperties();
			SndProperties = new SoundProperties();
			VertexIndices = new List<int>();
		}

		public Triangle(EndianBinaryReader reader)
		{
			ColProperties = new CollisionProperties();
			SndProperties = new SoundProperties();
			VertexIndices = new List<int>();
			VertexIndices.AddRange(new int[] { (int)reader.ReadInt16(), (int)reader.ReadInt16(), (int)reader.ReadInt16() });

			NormalIndex = (int)reader.ReadInt16();
			Edge1TangentIndex = (int)reader.ReadInt16();
			Edge2TangentIndex = (int)reader.ReadInt16();
			Edge3TangentIndex = (int)reader.ReadInt16();
			PlanePointIndex = (int)reader.ReadInt16();
			PlaneDValue = reader.ReadSingle();
			Unknown1 = (int)reader.ReadInt16();
			Unknown2 = (int)reader.ReadInt16();
		}

		public Triangle(Face face, List<Vector3> vertexes, List<Vector3> normals)
		{
			ColProperties = new CollisionProperties();
			SndProperties = new SoundProperties();
			VertexIndices = new List<int>();
			VertexIndices = face.Indices;

			Vector3[] vertexData = new Vector3[3];
			vertexData[0] = vertexes[VertexIndices[0]];
			vertexData[1] = vertexes[VertexIndices[1]];
			vertexData[2] = vertexes[VertexIndices[2]];
			GetNormalTangentData(vertexData, normals);

			Unknown1 = 0x8000;
			Unknown2 = 0;
		}

		private void GetNormalTangentData(Vector3[] vertexes, List<Vector3> normals)
		{
			Vector3 edge10 = vertexes[1] - vertexes[0];
			Vector3 edge20 = vertexes[2] - vertexes[0];

			Vector3 edge01 = vertexes[0] - vertexes[1];
			Vector3 edge21 = vertexes[2] - vertexes[1];

			Vector3 normal1 = Vector3.Cross(edge10, edge20).Normalized();
			//normal1 = new Vector3((float)Math.Round(normal1.X), (float)Math.Round(normal1.Y), (float)Math.Round(normal1.Z));
			Vector3 normal2 = Vector3.Cross(edge01, edge21).Normalized();
			Vector3 edge1Tan = Vector3.Cross(normal1, edge10).Normalized();
			Vector3 edge2Tan = Vector3.Cross(normal1, edge20).Normalized();
			Vector3 edge3Tan = Vector3.Cross(normal2, edge21).Normalized();

			NormalIndex = normals.Count;
			normals.Add(normal1);

			Edge1TangentIndex = normals.Count;
			normals.Add(edge1Tan);
			Edge2TangentIndex = normals.Count;
			normals.Add(edge2Tan);
			Edge3TangentIndex = normals.Count;
			normals.Add(edge3Tan);

			PlanePointIndex = normals.Count;

			Vector3 upAxis = Vector3.UnitY;

			float numerator = Vector3.Dot(normal1, upAxis);
			float denomenator = normal1.Length * upAxis.Length;
			float angle = (float)Math.Acos(numerator / denomenator);

			angle *= (float)(180 / Math.PI);
			if (Math.Abs(angle) >= 0.0f && Math.Abs(angle) <= 0.5f)
			{
				normals.Add(edge1Tan);
			}
			else
				normals.Add(Vector3.Zero);

			PlaneDValue = Vector3.Dot(edge3Tan, edge10);
		}

		//[JsonConstructor]
		public Triangle(List<int> VertexIndices, int NormalIndex, int TangentIndex, int BinormalIndex, int Unknown1Index, int PlanePointIndex, float PlaneDValue, int Unknown1, int Unknown2, CollisionProperties ColProperties, SoundProperties SndProperties)
		{
			this.VertexIndices = VertexIndices;
			this.NormalIndex = NormalIndex;
			this.Edge1TangentIndex = TangentIndex;
			this.Edge2TangentIndex = BinormalIndex;
			this.Edge3TangentIndex = Unknown1Index;
			this.PlanePointIndex = PlanePointIndex;
			this.PlaneDValue = PlaneDValue;
			this.Unknown1 = Unknown1;
			this.Unknown2 = Unknown2;
			this.ColProperties = ColProperties;
			this.SndProperties = SndProperties;
		}

		public void ReadCompiledColProperties(EndianBinaryReader reader)
		{
			int bitField = reader.ReadInt32();
			ColProperties.ColMaterial = (CollisionProperties.CollisionMaterials)(bitField & 3);
			ColProperties.IsLadder = Convert.ToBoolean(((bitField & 4) >> 2));
			ColProperties.IgnorePointer = Convert.ToBoolean(((bitField & 8) >> 3));
		}

		public void ReadCompiledSndProperties(EndianBinaryReader reader)
		{
			int bitField = reader.ReadInt32();
			SndProperties.SndMaterial = (SoundProperties.SoundMaterials)(bitField & 0xF);
			SndProperties.SndEchoSwitch = (bitField & 0x70) >> 4;
		}

		public void WriteCompiledTriangle(EndianBinaryWriter writer)
		{
			writer.Write((ushort)VertexIndices[0]);
			writer.Write((ushort)VertexIndices[1]);
			writer.Write((ushort)VertexIndices[2]);
			writer.Write((ushort)NormalIndex);
			writer.Write((ushort)Edge1TangentIndex);
			writer.Write((ushort)Edge2TangentIndex);
			writer.Write((ushort)Edge3TangentIndex);
			writer.Write((ushort)PlanePointIndex);
			writer.Write(PlaneDValue);
			writer.Write((ushort)Unknown1);
			writer.Write((ushort)Unknown2);
		}

		public void WriteCompiledColProperties(EndianBinaryWriter writer)
		{
			uint bitField = 0;

			uint ignorePointer = Convert.ToUInt32(ColProperties.IgnorePointer);
			bitField |= (uint)(ignorePointer << 3);

			uint isLadder = Convert.ToUInt32(ColProperties.IsLadder);
			bitField |= (uint)(isLadder << 2);

			bitField |= (uint)ColProperties.ColMaterial;
			writer.Write(bitField);
		}

		public void WriteCompiledSndProperties(EndianBinaryWriter writer)
		{
			uint bitField = 0;
			bitField |= (uint)(SndProperties.SndEchoSwitch << 4);
			bitField |= (uint)(SndProperties.SndMaterial);
			writer.Write(bitField);
		}
	}
}
