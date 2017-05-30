using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using GameFormatReader.Common;
//using Assimp;

/*

    TODO: 
     - Clean this up and plan/write out writing things
     - Fix a few issues with ?triangle conversion?
     - Fix issue with reading the right amount of primitives with files that base their primitive count on list size

 */

namespace Booldozer
{
    public class BinModel
    {
        public class GraphObject 
        {
            public short parentIndex;
            public short childIndex;
            public short nextIndex;
            public short prevIndex;
            byte padding1;
            public byte renderFlag;
            public short padding2;
            public Vector3 scale;
            public Vector3 rot;
            public Vector3 pos;
            public Vector3 bbMin;
            public Vector3 bbMax;
            float unk1;

            public short partCount;
            short padding3;
            public int partOffset;
            
            public List<GraphObjectPart> MeshParts = new List<GraphObjectPart>();

            public GraphObject(EndianBinaryReader stream)
            {
                this.parentIndex = stream.ReadInt16();
                this.childIndex = stream.ReadInt16();
                this.nextIndex = stream.ReadInt16();
                this.prevIndex = stream.ReadInt16();
                this.padding1 = stream.ReadByte();
                this.renderFlag = stream.ReadByte();
                this.padding2 = stream.ReadInt16();
                this.scale = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
                this.rot = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
                this.pos = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
                this.bbMin = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
                this.bbMax = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
                this.unk1 = stream.ReadSingle();

                this.partCount = stream.ReadInt16();
                this.padding3 = stream.ReadInt16();
                this.partOffset = stream.ReadInt32();
            }
        }

        public class GXVertex 
        {
            public short matrixIndex;
            public short posIndex;
            public short normalIndex;
            public short binormalIndex;
            public short tangentIndex;
            public short[] colorIndex; //2
            public short[] uvIndex; //8

            public GXVertex(EndianBinaryReader stream, byte uvCount, byte nbt, uint attribs){
                if ((attribs & (1 << 9)) != 0)
                {
                    posIndex = stream.ReadInt16();   
                }
                if ((attribs & (1 << 10)) != 0)
                {
                    normalIndex = stream.ReadInt16();
                    if (nbt != 0)
                    {
                        binormalIndex = stream.ReadInt16();
                        tangentIndex = stream.ReadInt16();
                    }
                }

                colorIndex = new short[2]; //{ stream.ReadInt16(), stream.ReadInt16() };
                if ((attribs & (1 << 11)) != 0)
                {
                    colorIndex[0] = stream.ReadInt16();
                }
                if ((attribs & (1 << 12)) != 0)
                {
                    colorIndex[1] = stream.ReadInt16();
                }
                uvIndex = new short[8];
                //Console.WriteLine("{0}", uvCount);
                for (int i = 0; i < uvCount; i++)
                {
                    if ((attribs & (1 << (13+i))) != 0)
                    {
                        uvIndex[i] = stream.ReadInt16();
                    }
                }
            }
        }

        public class Primitive
        {
            public byte type;
            public short count;
            public List<GXVertex> verts = new List<GXVertex>();

            public Primitive(EndianBinaryReader stream, uint[] offsets, byte nbt, byte uvCount, uint attribs)
            {
                type = stream.ReadByte();
                count = stream.ReadInt16();
                //Console.WriteLine("Reading primitive with {0} verts starting at 0x{1:X}", count, stream.BaseStream.Position);
                for (int i = 0; i < count; i++)
                {
                    verts.Add(new GXVertex(stream, uvCount, nbt, attribs));
                }
                //Console.WriteLine("Finished Reading Primitve");
            }

        }

        public class Batch
        {
            public ushort faceCount;
            public ushort listSize;
            uint attribs;
            byte useNormals;
            byte positions;
            byte uvCount;
            byte nbt;
            uint offset;

            public List<Primitive> primitives = new List<Primitive>();
            
            public Batch(EndianBinaryReader stream, uint[] offsets)
            {
                faceCount = stream.ReadUInt16();
                listSize = stream.ReadUInt16();

                attribs = stream.ReadUInt32();

                useNormals = stream.ReadByte();
                positions = stream.ReadByte();
                uvCount = stream.ReadByte();
                nbt = stream.ReadByte();
                offset = stream.ReadUInt32();
                stream.Skip(8);

                var f = 0;
                stream.BaseStream.Seek(offset + offsets[11], 0);
                while(f < faceCount && stream.BaseStream.Position < (offset + offsets[11]) + (listSize<<5))
                {
                    var p = new Primitive(stream, offsets, nbt, uvCount, attribs);
                    f += p.count-2;
                    primitives.Add(p);
                }
            }
            
        }

        public struct GraphObjectPart
        {
            public short shaderIndex;
            public short batchIndex;
            public Batch batch;

            public GraphObjectPart(EndianBinaryReader stream, uint[] offsets)
            {
                shaderIndex = stream.ReadInt16();
                batchIndex = stream.ReadInt16();
                var last = stream.BaseStream.Position;
                stream.BaseStream.Seek(offsets[11] + (0x18*batchIndex), 0);
                batch = new Batch(stream, offsets);
                stream.BaseStream.Seek(last, 0);
            }
        }

        private void getGraphObjects(EndianBinaryReader stream, int index)
        {
            stream.BaseStream.Seek(offsets[12] + (0x8C * index), 0);
            var obj = new GraphObject(stream);
            stream.BaseStream.Seek(obj.partOffset + offsets[12], 0);
            for (int i = 0; i < obj.partCount; i++)
            {
                //Console.WriteLine("Reading Part {0} at offset 0x{1:X}", i, stream.BaseStream.Position);
                obj.MeshParts.Add(new GraphObjectPart(stream, offsets));
            }
            Meshes.Add(obj);
            if (obj.childIndex >= 0){
                getGraphObjects(stream, obj.childIndex);
            }
            if (obj.nextIndex >= 0)
            {
                getGraphObjects(stream, obj.nextIndex);
            }
        }

        public List<GraphObject> Meshes = new List<GraphObject>();
        public List<Vector3> Verticies = new List<Vector3>();
        private uint[] offsets = new uint[21];

        public BinModel(string path)
        {
            using(FileStream fs = new FileStream(path, FileMode.Open))
            {
                EndianBinaryReader stream = new EndianBinaryReader(fs, Encoding.GetEncoding("shift-jis"), Endian.Big);
                stream.Skip(1);
                //hacky but for now its ok
                var p = new string(stream.ReadChars(11));
                foreach(var c in Path.GetInvalidFileNameChars()){
                    p = p.Replace(c, ' ').Trim();
                }
                TextWriter o = new StreamWriter(p + ".obj");//stream.ReadChars(11));
                for (int i = 0; i < 21; i++)
                {
                    offsets[i] = stream.ReadUInt32();
                }
                getGraphObjects(stream, 0);

                uint vertCount = 0;
                for (int i = 3; i < 21; i++)
                {
                    if (offsets[i] > 0)
                    {
                        vertCount = (offsets[i] - offsets[2]) / 6;
                    }                    
                }
                
                stream.BaseStream.Seek(offsets[2], 0);
                for (int i = 0; i < vertCount; i++)
                {
                    Verticies.Add(new Vector3(stream.ReadInt16(), stream.ReadInt16(), stream.ReadInt16()));
                }
                for (int i = 0; i < Verticies.Count; i++)
                {
                    o.WriteLine("v {0} {1} {2}\n", Verticies[i].X, Verticies[i].Y, Verticies[i].Z);
                }
                var cGourp = 0;
                foreach (var obj in Meshes)
                {
                    o.WriteLine("g GraphObject{0}", cGourp);
                    cGourp += 1;
                    foreach (var part in obj.MeshParts)
                    {
                        foreach (var primitive in part.batch.primitives)
                        {
                            var face = "f {0} {1} {2}";
                            var tri = new int[3];
                            switch(primitive.type){
                                case 0x90:
                                o.WriteLine(face, primitive.verts[0].posIndex+1, primitive.verts[1].posIndex+1, primitive.verts[2].posIndex+1);
                                break;

                                case 0x98:
                                for (int v = 2; v < primitive.verts.Count; v++)
                                {
                                    bool even = v % 2 != 0;
                                    tri[0] = primitive.verts[v-2].posIndex;
                                    tri[1] = even ? primitive.verts[v].posIndex : primitive.verts[v-1].posIndex;
                                    tri[2] = even ? primitive.verts[v-1].posIndex : primitive.verts[v].posIndex;
                                    if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
                                    {
                                        o.WriteLine(face, tri[0]+1, tri[1]+1, tri[2]+1);
                                    }
                                }
                                break;

                                case 0xA0:
                                for (int v = 1; v < primitive.verts.Count; v++)
                                {
                                    tri[0] = primitive.verts[v].posIndex;
                                    tri[1] = primitive.verts[v].posIndex;
                                    tri[2] = primitive.verts[v].posIndex;
                                    if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
                                    {
                                        o.WriteLine(face, tri[0]+1, tri[1]+1, tri[2]+1);
                                    }
                                }
                                break;

                                case 0x80:
                                o.WriteLine(face, primitive.verts[0].posIndex+1, primitive.verts[1].posIndex+1, primitive.verts[2].posIndex+1);
                                o.WriteLine(face, primitive.verts[1].posIndex+1, primitive.verts[2].posIndex+1, primitive.verts[3].posIndex+1);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}