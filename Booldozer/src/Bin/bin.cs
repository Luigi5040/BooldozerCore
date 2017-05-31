using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;
using GameFormatReader.Common;
using Booldozer.Materials;
using System.Drawing;
//using Assimp;

/*

    TODO: 
     - Clen up. A lot.
     - Add wrapping to textures and obj exporter

 */

namespace Booldozer.Bin
{
    public class BinModel
    {

        public enum GXPrimitiveType
        {
            Points = 0xB8,
            Lines = 0xA8,
            LineStrip = 0xB0,
            Triangles = 0x90,
            TriangleStrip = 0x98,
            TriangleFan = 0xA0,
            Quads = 0x80,
        }

        public class Shader
        {
            uint tint;
            short[] materialIndex = new short[8];
            short[] unkIndex = new short[8];
            
            public Material[] materials = new Material[8];
            public Shader(EndianBinaryReader stream, uint[] offsets)
            {
                stream.Skip(3);
                tint = stream.ReadUInt32();
                stream.SkipByte();
                for (int i = 0; i < 8; i++)
                {
                    materialIndex[i] = stream.ReadInt16();
                    //Console.WriteLine($"Material Index: {materialIndex[i]}");
                }
                for (int i = 0; i < 8; i++)
                {
                    unkIndex[i] = stream.ReadInt16();
                }
                for (int i = 0; i < 8; i++)
                {
                    if (materialIndex[i] >= 0)
                    {
                        stream.BaseStream.Seek(offsets[1] + (0x14*materialIndex[i]), 0);
                        materials[i] = new Material(stream, offsets[0]);   
                    }
                }
            }
        }

        public class Material 
        {
            public short textureIndex;
            short wrapU;
            short wrapV;
            //12 bytes of padding, remember this
            public BinaryTextureImage texture;
            public Material(EndianBinaryReader stream, uint texOffset){
                //Console.WriteLine("Reading Material at 0x{0:X}", stream.BaseStream.Position);
                textureIndex = stream.ReadInt16();
                stream.SkipInt16();
                wrapU = stream.ReadByte();
                wrapV = stream.ReadByte();
                stream.SkipInt16();
                stream.Skip(12);
                texture = new BinaryTextureImage(stream, texOffset+(0xC*textureIndex), texOffset);
            }
        }

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
                bool knownPrimitive = true; //Hax?
                stream.BaseStream.Seek(offset + offsets[11], 0);
                while(f < faceCount && knownPrimitive)
                {
                    var p = new Primitive(stream, offsets, nbt, uvCount, attribs);
                    knownPrimitive = (p.type == 0 ? false : true); 
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
            public Shader shader;

            public GraphObjectPart(EndianBinaryReader stream, uint[] offsets)
            {
                shaderIndex = stream.ReadInt16();
                batchIndex = stream.ReadInt16();
                var last = stream.BaseStream.Position;
                
                stream.BaseStream.Seek(offsets[11] + (0x18*batchIndex), 0);
                batch = new Batch(stream, offsets);
                
                stream.BaseStream.Seek(offsets[10] + (0x28*shaderIndex), 0);
                shader = new Shader(stream, offsets);
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

        string mName;

        public void writeOBJ(string filename = null){
            if (filename == null)
            {
                filename = mName + ".obj";
            }
            StringWriter writer = new StringWriter();
            writer.WriteLine($"# Model \"{mName}\" dumped from bin by Booldozer v0.Ferns");
            writer.WriteLine();
            foreach(Vector3 v in Verticies){
                writer.WriteLine($"v {v.X} {v.Y} {v.Z}");
            }
            writer.WriteLine();
            var curParts = 0;
            var texCount = 0;
            foreach (var mesh in Meshes)
            {
                writer.WriteLine($"g {mName}.{curParts}");
                curParts++;
                foreach (var part in mesh.MeshParts)
                {
                    foreach (var mat in part.shader.materials)
                    {
                        if (mat != null)
                        {
                            var cTex = mat.texture;
                            cTex.SaveImageToDisk($"{texCount}.png", cTex.GetData(), cTex.Width, cTex.Height);
                            texCount++;
                        }
                    }
                    foreach (var primitive in part.batch.primitives)
                    {
                        var verts = primitive.verts;
                        switch ((GXPrimitiveType)primitive.type)
                        {
                            case GXPrimitiveType.Triangles:
                            writer.WriteLine($"f {verts[0].posIndex+1} {verts[1].posIndex+1} {verts[2].posIndex+1}");
                            break;

                            case GXPrimitiveType.TriangleStrip:
                            for (int v = 2; v < verts.Count; v++)
                            {
                                bool even = v % 2 != 0;
                                var tri = new int[3];
                                tri[0] = verts[v-2].posIndex;
                                tri[1] = even ? verts[v].posIndex : verts[v-1].posIndex;
                                tri[2] = even ? verts[v-1].posIndex : verts[v].posIndex;
                                if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
                                {
                                    writer.WriteLine($"f {tri[0]+1} {tri[1]+1} {tri[2]+1}");
                                }
                            }
                            break;

                            case GXPrimitiveType.TriangleFan:
                            for (int v = 1; v < verts.Count; v++)
                            {
                                var tri = new int[3];
                                tri[0] = verts[v].posIndex+1;
                                tri[1] = verts[v+1].posIndex+1;
                                tri[2] = verts[0].posIndex+1;

                                if (tri[0] != tri[1] && tri[1] != tri[2] && tri[2] != tri[0])
                                {
                                    writer.WriteLine($"f {tri[0]} {tri[1]} {tri[2]}");
                                }
                            }
                            break;
                        }
                    }
                }
            }

            using(FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                EndianBinaryWriter w = new EndianBinaryWriter(s, Endian.Big);
                w.Write(writer.ToString().ToCharArray());
            }
        }

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
                mName = p;
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
                        break;
                    }                    
                }
                
                stream.BaseStream.Seek(offsets[2], 0);
                for (int i = 0; i < vertCount; i++)
                {
                    Verticies.Add(new Vector3(stream.ReadInt16(), stream.ReadInt16(), stream.ReadInt16()));
                }
                writeOBJ();
            }
        }
    }
}