using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using GameFormatReader.Common;
using Booldozer.Materials;
using Booldozer.Models.GX;

namespace Booldozer.Models.Mdl
{
    public class MdlModel : Mesh
    {
        ushort[] counts; //20;
        long[] offsets; //18
        List<Vector3d> verticies;
        List<DrawElement> drawelements;
        List<Shape> shapes;
        List<ShapePacket> shapepackets;
        List<Primitive> primitives;

        public void WriteObj(string f)
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("#dumped with booldozer");
            foreach (var vert in verticies)
            {
                writer.WriteLine($"v {vert.X} {vert.Y} {vert.Z}");
            }
            foreach (var primitive in primitives)
            {
                var verts = primitive.verts;
				switch ((GXPrimitiveType)primitive.type)
				{
					case GXPrimitiveType.Triangles:
						writer.WriteLine($"f {verts[0].posIndex + 1} {verts[1].posIndex + 1} {verts[2].posIndex + 1}");
						break;

					case GXPrimitiveType.TriangleStrip:
						for (int v = 2; v < verts.Count; v++)
						{
							bool even = v % 2 != 0;
							var tri = new int[3];
							tri[0] = verts[v - 2].posIndex;
							tri[1] = even ? verts[v].posIndex : verts[v - 1].posIndex;
							tri[2] = even ? verts[v - 1].posIndex : verts[v].posIndex;
							if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
							{
								writer.WriteLine($"f {tri[0] + 1} {tri[1] + 1} {tri[2] + 1}");
							}
						}
						break;

				 	 case GXPrimitiveType.TriangleFan:
						for (int v = 1; v < verts.Count; v++)
						{
							var tri = new int[3];
							tri[0] = verts[v].posIndex + 1;
							tri[1] = verts[v + 1].posIndex + 1;
							tri[2] = verts[0].posIndex + 1;

							if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
							{
								writer.WriteLine($"f {tri[0]} {tri[1]} {tri[2]}");
							}
						}
						break;
				}
            }
            using (FileStream s = new FileStream(f, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
				w.Write(writer.ToString().ToCharArray());
			}
        }

		public override void Load(EndianBinaryReader reader)
		{
			throw new NotImplementedException();
		}

		public override void Render()
		{
			throw new NotImplementedException();
		}

		public MdlModel()
        {
            counts = new ushort[20];
            offsets = new long[18];
        }
        public MdlModel(string path)
        {
            counts = new ushort[20];
            offsets = new long[18];
            verticies = new List<Vector3d>();
            drawelements =  new List<DrawElement>();
            shapes =  new List<Shape>();
            shapepackets = new List<ShapePacket>();
            primitives = new List<Primitive>();
            using(FileStream fs = new FileStream(path, FileMode.Open))
            {
                EndianBinaryReader stream = new EndianBinaryReader(fs, Endian.Big);
                stream.ReadInt32(); //ignore the magic
                for (int i = 0; i < 20; i++)
                {
                    counts[i] = stream.ReadUInt16();
                }
                stream.BaseStream.Seek(0x30, 0);
                for (int i = 0; i < 18; i++)
                {
                    offsets[i] = stream.ReadUInt32();
                }

                stream.BaseStream.Seek(offsets[6], 0);
                for (int i = 0; i < counts[6]; i++)
                {
                    verticies.Add(new Vector3d(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle()));
                }
                Console.WriteLine("Reading Draw Elements");
                stream.BaseStream.Seek(offsets[17], 0);
                for (int i = 0; i < counts[17]; i++)
                {
                    drawelements.Add(new DrawElement(stream));
                }
                Console.WriteLine("Reading Shapes");
                stream.BaseStream.Seek(offsets[16], 0);
                for (int i = 0; i < counts[18]; i++)
                {
                    shapes.Add(new Shape(stream));
                }
                Console.WriteLine("Reading Shape Packets");
                stream.BaseStream.Seek(offsets[1], 0);
                for (int i = 0; i < counts[3]; i++)
                {
                    shapepackets.Add(new ShapePacket(stream));
                }

                foreach (var element in drawelements)
                {
                    var shape = shapes[element.shapeIndex];
                    for (int i = shape.first; i < shape.first+shape.count; i++)
                    {
                        var shapepacket = shapepackets[i];
                        stream.BaseStream.Seek(shapepacket.dataOffset, 0);
                        while (stream.BaseStream.Position <= shapepacket.dataOffset + shapepacket.dataSize)
                        {
                            primitives.Add(new Primitive(stream, counts));
                            /*
                            var op = stream.ReadByte();
                            var num = stream.ReadUInt16();
                            var faceIndicices = new int[num];
                            for (int j = 0; j < num; j++)
                            {
                                stream.ReadByte(); //Mat Index
                                stream.ReadByte(); //Tex0?
                                stream.ReadByte(); //Tex1?
                                faceIndicices[j] = stream.ReadUInt16(); //pos
                                if (counts[7] > 0)
                                {
                                    stream.ReadUInt16(); //normal   
                                }
                                if (counts[8] > 0)
                                {
                                    stream.ReadUInt16(); //color   
                                }
                                if (counts[9] > 0)
                                {
                                    stream.ReadUInt16(); //uv   
                                }
                            }
                            shapepackets[i].faces.AddRange(faceIndicices);
                            */
                        }
                    }
                }

                WriteObj("derp.obj");
            }
        }
    }
}