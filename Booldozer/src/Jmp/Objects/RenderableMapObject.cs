using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Booldozer.Models;

namespace Booldozer.Jmp.Objects
{
	public abstract class RenderableMapObject : MapObject
	{
		public Mesh ObjectModel { get; private set; }

		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 Scale;

		public void Render(Matrix4 projViewModel, int shaderProgram)
		{
			// Create modelview matrix (transforms the mesh from model space to world space)
			Matrix4 modelView = Matrix4.CreateTranslation(Position) *
			                    Matrix4.CreateFromQuaternion(Rotation) * 
			                    Matrix4.CreateScale(Scale);
			// Create Model-View-Projection matrix
			Matrix4 mvpMat = projViewModel * modelView;

			// Upload matrix to the shader uniform
			int mvpShaderID = GL.GetUniformLocation(shaderProgram, "MVP");
			GL.UniformMatrix4(mvpShaderID, false, ref mvpMat);

			ObjectModel.Render();
		}
	}
}
