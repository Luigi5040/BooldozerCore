/*
using System;
using System.IO;
using Newtonsoft.Json;
using Assimp;
using OpenTK;
using System.Collections.Generic;
using GameFormatReader.Common;

namespace LMCollisionDev
{
	public partial class Collision
	{
		#region Input

		private void OpenJson(string fileName)
		{
			using (StreamReader strmReader = File.OpenText(fileName))
			{
				using (JsonTextReader jsonReader = new JsonTextReader(strmReader))
				{
					jsonReader.SupportMultipleContent = true;

					JsonSerializer srl = new JsonSerializer();

					jsonReader.Read();
					List<Vector3D> simpleVerts = srl.Deserialize<List<Vector3D>>(jsonReader);
					foreach (Vector3D vec in simpleVerts)
						Vertices.Add(Util.Vec3DToVec3(vec));

					jsonReader.Read();
					List<Vector3D> simpleNrms = srl.Deserialize<List<Vector3D>>(jsonReader);
					foreach (Vector3D vec in simpleNrms)
						NormalizedVectors.Add(Util.Vec3DToVec3(vec));

					jsonReader.Read();
					Triangles = srl.Deserialize<List<Triangle>>(jsonReader);
				}

			}
		}

		#endregion

		#region Output

		private void SaveJson(string fileName)
		{
			StringWriter strWriter = new StringWriter();

			JsonSerializer ser = new JsonSerializer();
			//ser.Converters.Add(J

			List<Vector3D> simpleVerts = new List<Vector3D>();
			foreach (Vector3 vec in Vertices)
				simpleVerts.Add(Util.Vec3ToVec3D(vec));

			string vertexes = JsonConvert.SerializeObject(simpleVerts, Formatting.Indented);
			strWriter.Write(vertexes);

			List<Vector3D> simpleNrms = new List<Vector3D>();
			foreach (Vector3 vec in NormalizedVectors)
				simpleNrms.Add(Util.Vec3ToVec3D(vec));

			string normals = JsonConvert.SerializeObject(simpleNrms, Formatting.Indented);
			strWriter.Write(normals);

			string triangles = JsonConvert.SerializeObject(Triangles, Formatting.Indented);
			strWriter.Write(triangles);

			using (FileStream strm = new FileStream(fileName, FileMode.Create))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);
				writer.Write(strWriter.ToString().ToCharArray());

			}

		}

		#endregion
	}
}
*/
