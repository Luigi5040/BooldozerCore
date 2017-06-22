using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using Assimp;
using GameFormatReader.Common;
//using Newtonsoft.Json;
using BooldozerCore.Util;

namespace BooldozerCore.Collision
{
	public partial class CollisionMesh
	{
		public string OriginalFilename { get; private set; }
		public static Vector3 StaticScale;
		public BoundingBox BBox { get; private set; }
		public List<Vector3> Vertices { get; private set; }
		public List<Vector3> NormalizedVectors { get; private set; }
		public List<Triangle> Triangles { get; private set; }

		public CollisionMesh()
		{
			StaticScale = new Vector3(256.0f, 512.0f, 256.0f);
			Vertices = new List<Vector3>();
			NormalizedVectors = new List<Vector3>();
			Triangles = new List<Triangle>();
		}

		public CollisionMesh(string fileName)
		{
			if (!File.Exists(fileName))
				throw new FormatException($"File \"{ fileName }\" does not exist.");

			OriginalFilename = fileName;
			StaticScale = new Vector3(256.0f, 512.0f, 256.0f);
			Vertices = new List<Vector3>();
			NormalizedVectors = new List<Vector3>();
			Triangles = new List<Triangle>();

			string fileExt = Path.GetExtension(fileName).ToLower();
			switch (fileExt)
			{
				case ".mp": // We were passed compiled collision
					OpenCompiled(fileName);
					break;
				case ".json": // We were passed a json file
					//OpenJson(fileName);
					break;
				default: // We may have been passed a model file or an incompatibile file, we'll check later
					OpenModelFile(fileName);
					break;
			}

			BBox = new BoundingBox(Vertices);
			CreateDisplaylist();
		}
	}
}
