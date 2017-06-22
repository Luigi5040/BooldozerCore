using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK;
using GameFormatReader.Common;

namespace BooldozerCore.Jmp
{
	public class jmp
	{

		public struct jmpValue
		{
			public float valueFloat;
			public int valueInt;
			public string valueString;
		}

		public enum JMPType
		{
			INTEGER,
			STRING,
			FLOAT
		}

		public struct jmpField
		{
			public int hash;
			public int bitmask;
			public short start;
			public byte shift;
			public JMPType type;

			public void WriteField(EndianBinaryWriter stream)
			{
				stream.Write(hash);
				stream.Write(bitmask);
				stream.Write(start);
				stream.Write(shift);
				stream.Write((byte)type);
			}

			public jmpField(EndianBinaryReader stream)
			{
				hash = stream.ReadInt32();
				bitmask = stream.ReadInt32();
				start = stream.ReadInt16();
				shift = stream.ReadByte();
				type = (JMPType)stream.ReadByte();
			}
		}

		private int entryCount;
		private int fieldCount;
		private int entryOff;
		private int entrySize;

		public List<jmpField> fields = new List<jmpField>();
		public List<jmpValue> entries = new List<jmpValue>();

		public JMPType getFieldType(int fieldNum){
			return this.fields[fieldNum].type;
		}

		public int getIntField(int entryNum, int fieldNum)
		{
			return entries[entryNum+fieldNum].valueInt;
		}

		public float getFloatField(int entryNum, int fieldNum)
		{
			return entries[entryNum+fieldNum].valueFloat;
		}

		public string getStringField(int entryNum, int fieldNum)
		{
			return entries[entryNum+fieldNum].valueString;
		}

		public void WriteJmp(string path)
		{
			using(FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
			{
				EndianBinaryWriter stream = new EndianBinaryWriter(fs, Endian.Big);
				stream.Write(entryCount);
				stream.Write(fieldCount);
				stream.Write(entryOff);
				stream.Write(entrySize);
				foreach (var field in fields) field.WriteField(stream);
				for (int i = 0; i < entryCount; i++)
				{
					for (int j = 0; j < fieldCount; j++)
					{
						switch (fields[j].type)
						{
							case JMPType.INTEGER:
								stream.Write((entries[i*j].valueInt & fields[j].bitmask) << fields[j].shift);
								break;
							
							case JMPType.STRING:
								stream.WriteFixedString(entries[i*j].valueString, 32);
								break;
							
							case JMPType.FLOAT:
								stream.Write(entries[i*j].valueFloat);
								break;
						}
					}
				}
				
			}
		}

		public jmp(string path)
		{
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(fs, Endian.Big);
				entryCount = reader.ReadInt32();
				fieldCount = reader.ReadInt32();
				entryOff = reader.ReadInt32();
				entrySize = reader.ReadInt32();
				
				for(int i = 0; i < fieldCount; i++)
				{
					fields.Add(new jmpField(reader));
				}
				
				for (int i = 0; i < entryCount; i++)
				{
					for (int j = 0; j < fieldCount; j++)
					{
						var curFeild = fields[j]; 
						reader.BaseStream.Seek((entryOff + (entrySize*i) + fields[j].start), SeekOrigin.Begin);

						jmpValue value = new jmpValue();
						switch (curFeild.type)
						{
							case JMPType.INTEGER:
								value.valueInt = ((reader.ReadInt32() & curFeild.bitmask) >> curFeild.shift);
								break;

							case JMPType.FLOAT:
								value.valueFloat = (float)reader.ReadSingle();
								break;
							
							case JMPType.STRING:
								value.valueString = new string(reader.ReadChars(32));
								break;

						}
						entries.Add(value);
					}
				}

			}
		}
	}
}
