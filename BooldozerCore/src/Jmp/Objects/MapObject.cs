using System;
using System.Reflection;
using BooldozerCore.Models;

namespace BooldozerCore.Jmp.Objects
{
	public abstract class MapObject
	{
        public bool HasRoomNumber;

		public virtual void Load(jmp data, int entryIndex)
		{
			FieldInfo[] fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

			for (int i = 0; i < data.fields.Count; i++)
			{
				switch (data.fields[i].type)
				{
					case jmp.JMPType.FLOAT:
						if (fields[i].FieldType == typeof(float))
						{
							fields[i].SetValue(this, data.getFloatField(entryIndex, i));
						}
						break;
					case jmp.JMPType.INTEGER:
						if (fields[i].FieldType == typeof(int))
						{
							fields[i].SetValue(this, data.getIntField(entryIndex, i));
						}
						break;
					case jmp.JMPType.STRING:
						if (fields[i].FieldType == typeof(string))
						{
							fields[i].SetValue(this, data.getStringField(entryIndex, i));
						}
						break;
					default:
						break;
				}
			}
		}

		public abstract void Save(string path);
	}
}
