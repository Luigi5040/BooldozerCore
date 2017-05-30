using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Booldozer.Collision
{
	public partial class Collision
	{
		public int colDisplayList { get; set; }

		private void CreateDisplaylist()
		{
			colDisplayList = GL.GenLists(1);

			GL.NewList(colDisplayList, ListMode.Compile);

			GL.Begin(PrimitiveType.Triangles);
			for (int i = 0; i < Triangles.Count; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Vector3 triVert = Vertices[Triangles[i].VertexIndices[j]];
					GL.Vertex3(triVert);
				}
			}
			GL.End();

			GL.EndList();
		}

		public void Render()
		{
			GL.CallList(colDisplayList);
			GL.Flush();
		}
	}
}
