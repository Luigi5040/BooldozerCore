using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using BooldozerCore.Models.GX;
using GameFormatReader.Common;

namespace BooldozerCore.Models
{
	public abstract class Mesh
	{
		public List<Vector3> Positions { get; private set; }
		public List<Vector3> Normals { get; private set; }
		public List<Color4>[] Colors { get; private set; }
		public List<Vector2>[] TexCoords { get; private set; }
		public List<GXBatch> Geometry { get; private set; }

		public Mesh()
		{
			Positions = new List<Vector3>();
			Normals = new List<Vector3>();
			Colors = new List<Color4>[] { new List<Color4>(), new List<Color4>() };
			TexCoords = new List<Vector2>[8];

			for (int i = 0; i < 8; i++)
				TexCoords[i] = new List<Vector2>();

			Geometry = new List<GXBatch>();
		}

		public abstract void Render();
	}
}
