using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using GameFormatReader.Common;
//using Newtonsoft.Json;
using Assimp;

namespace BooldozerCore.Collision
{
	public partial class CollisionMesh
	{
		public enum FileType
		{
			none,
			compiled,
			json,
			obj
		}

		public void SaveFile(string fileName, FileType outputType)
		{
			switch (outputType)
			{
				case FileType.compiled: // We'll be making a .mp file at (fileName) and its properties at (fileName/jmp)
					SaveCompiled(fileName);
					break;
				case FileType.json: // We'll be making a JSON file for human readability of geometry + properties
					//SaveJson(fileName);
					break;
				case FileType.obj: // We'll be making an OBJ file containg only geometry
					SaveObj(fileName);
					break;
				default:
					break;
			}
		}
	}
}
