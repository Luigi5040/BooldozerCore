using System;
using Assimp;
using System.IO;
using OpenTK;
using GameFormatReader.Common;

namespace BooldozerCore.Collision
{
	public partial class CollisionMesh
	{
		#region Input

		private void OpenModelFile(string fileName)
		{
			AssimpContext cont = new AssimpContext();
			Scene scene;

			// We'll make sure the file loads. If it doesn't, we'll throw an exception
			try
			{
				scene = cont.ImportFile(fileName, PostProcessSteps.Triangulate);
			}
			catch
			{
				throw new FormatException($"Failed to open file \"{ fileName }\". The file is either not a supported model file or corrupted.");
			}

			LoadModelFile(scene);
		}

		private void LoadModelFile(Scene scene)
		{
			foreach (Mesh msh in scene.Meshes)
			{
				// Extract vertices
				foreach (Vector3D vert in msh.Vertices)
				{
					Vertices.Add(Util.Vec3DToVec3(vert));
				}

				// Extract triangles
				foreach (Face face in msh.Faces)
				{
					Triangles.Add(new Triangle(face, Vertices, NormalizedVectors));
				}

			}
		}

		#endregion

		#region Output

		private void SaveObj(string fileName)
		{
			StringWriter strWriter = new StringWriter();

			strWriter.WriteLine($"# This OBJ file was dumped from \"{ Path.GetFileName(OriginalFilename) }\" using Sage of Mirrors/Gamma's Luigi's Mansion collision tool.");
			strWriter.WriteLine($"# Created { DateTime.Now.ToString() }");
			strWriter.WriteLine($"# Vertex Count: { Vertices.Count }");
			strWriter.WriteLine($"# Triangle Count: { Triangles.Count }");

			strWriter.WriteLine();

			foreach (Vector3 vec in Vertices)
			{
				strWriter.WriteLine($"v { vec.X } { vec.Y } { vec.Z }");
			}

			strWriter.WriteLine();

			foreach (Triangle tri in Triangles)
			{
				strWriter.WriteLine($"vn { NormalizedVectors[tri.NormalIndex].X } { NormalizedVectors[tri.NormalIndex].Y } { NormalizedVectors[tri.NormalIndex].Z }");
			}

			strWriter.WriteLine();

			int normalIndex = 1;
			foreach (Triangle tri in Triangles)
			{
				strWriter.Write($"f { tri.VertexIndices[0] + 1 }//{ normalIndex }");
				strWriter.Write($" { tri.VertexIndices[1] + 1 }//{ normalIndex }");
				strWriter.Write($" { tri.VertexIndices[2] + 1 }//{ normalIndex }");
				strWriter.Write(Environment.NewLine);

				normalIndex++;
			}

			using (FileStream strm = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);
				writer.Write(strWriter.ToString().ToCharArray());

			}
		}

		#endregion
	}
}
