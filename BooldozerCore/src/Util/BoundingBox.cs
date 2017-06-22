using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GameFormatReader.Common;

namespace BooldozerCore.Util
{
	public class BoundingBox
	{
		public Vector3 Minimum;
		public Vector3 Maximum;
		public Vector3 AxisLengths;
		Vector3 Center;
		public float SphereRadius;

		public BoundingBox()
		{
			Minimum = new Vector3();
			Maximum = new Vector3();
			SphereRadius = 0.0f;
		}

		/*
		public BoundingBox(Vector3 min, Vector3 axisLength)
		{
			Minimum = new Vector3(min.X - 100, min.Y - 100, min.Z - 100);
			Maximum = new Vector3(min.X + axisLength.X, min.Y + axisLength.Y, min.Z + axisLength.Z);
			AxisLengths = axisLength;
		}*/

		public BoundingBox(Vector3 min, Vector3 max)
		{
			Maximum = max;
			Minimum = min;
			AxisLengths = new Vector3((Maximum.X) - Minimum.X, (Maximum.Y) - Minimum.Y, (Maximum.Z) - Minimum.Z);
		}

		public BoundingBox(List<Vector3> positions)
		{
			#region Max and min
			float maxX = float.MinValue;

			float maxY = float.MinValue;

			float maxZ = float.MinValue;

			float minX = float.MaxValue;

			float minY = float.MaxValue;

			float minZ = float.MaxValue;

			foreach (Vector3 vec in positions)
			{
				if (vec.X > maxX)
					maxX = vec.X;

				if (vec.Y > maxY)
					maxY = vec.Y;

				if (vec.Z > maxZ)
					maxZ = vec.Z;

				if (vec.X < minX)
					minX = vec.X;

				if (vec.Y < minY)
					minY = vec.Y;

				if (vec.Z < minZ)
					minZ = vec.Z;
			}

			Maximum = new Vector3(maxX + 100, maxY + 100, maxZ + 100);

			Minimum = new Vector3(minX - 100, minY - 100, minZ - 100);
			AxisLengths = new Vector3((Maximum.X) - Minimum.X, (Maximum.Y) - Minimum.Y, (Maximum.Z) - Minimum.Z);
			#endregion

			#region Center
			Center.X = (Maximum.X + Minimum.X) / 2;

			Center.Y = (Maximum.Y + Minimum.Y) / 2;

			Center.Z = (Maximum.Z + Minimum.Z) / 2;
			#endregion

			//Maximum = Maximum - Center;
			//Minimum = Minimum - Center;

			#region Bounding Sphere Radius

			float radius = float.MinValue;

			foreach (Vector3 vec in positions)
			{
				Vector3 transformedVec = vec - Center;

				if (transformedVec.Length > radius)
					radius = transformedVec.Length;
			}

			SphereRadius = ((Maximum - Minimum) / 2).Length;

			SphereRadius = radius;

			#endregion
		}

		public void WriteBoundingBox(EndianBinaryWriter writer)
		{
			writer.Write(Minimum.X);
			writer.Write(Minimum.Y);
			writer.Write(Minimum.Z);

			writer.Write(AxisLengths.X);
			writer.Write(AxisLengths.Y);
			writer.Write(AxisLengths.Z);

			/*
            writer.Write(Minimum.X);
            writer.Write(Minimum.Y);
            writer.Write(Minimum.Z);

            writer.Write(Maximum.X);
            writer.Write(Maximum.Y);
            writer.Write(Maximum.Z);*/
		}
	}
}
