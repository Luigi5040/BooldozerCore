using System;
using GameFormatReader.Common;

namespace Booldozer.Models
{
	public abstract class Mesh
	{
		public abstract void Load(EndianBinaryReader reader);
		public abstract void Render();
	}
}
