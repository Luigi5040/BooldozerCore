using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK;
using GameFormatReader.Common;

namespace Booldozer.Jmp
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

			public jmpField(EndianBinaryReader stream)
			{
				this.hash = stream.ReadInt32();
				this.bitmask = stream.ReadInt32();
				this.start = stream.ReadInt16();
				this.shift = stream.ReadByte();
				this.type = (JMPType)stream.ReadByte();
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

		public void addEntry(jmpValue[] entry)
		{
			//TODO: plan out way of adding entries properly
		}

		public jmp(string path)
		{
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(fs, Endian.Big);
				this.entryCount = reader.ReadInt32();
				this.fieldCount = reader.ReadInt32();
				this.entryOff = reader.ReadInt32();
				this.entrySize = reader.ReadInt32();
				
				for(int i = 0; i < this.fieldCount; i++)
				{
					this.fields.Add(new jmpField(reader));
				}
				
				for (int i = 0; i < this.entryCount; i++)
				{
					for (int j = 0; j < this.fieldCount; j++)
					{
						var curFeild = this.fields[j]; 
						reader.BaseStream.Seek((this.entryOff + (this.entrySize*i) + this.fields[j].start), SeekOrigin.Begin);

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
						this.entries.Add(value);
					}
				}

			}
		}
	}
}
