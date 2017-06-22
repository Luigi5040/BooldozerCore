using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace BooldozerCore.Jmp.Objects
{
    public class Room : RenderableMapObject
    {
        [Description("Room Number")]
        public int RoomNumber { get { return m_RoomNumber; } }
        [Description("Thunder")]
        public int Thunder { get { return m_Thunder; } set { m_Thunder = value; } }
        [Description("Show Skybox")]
        public bool VRBox { get { return Convert.ToBoolean(m_VRBox); } set { m_VRBox = Convert.ToInt32(value); } }
        [Description("Dustiness Level")]
        public int DustLevel { get { return m_DustLevel; } set { m_DustLevel = value; } }
        [Description("Light Color")]
        public Vector4 Lightcolor { get { return new Vector4(m_LightColorR, m_LightColorG, m_LightColorB, (int)255); } set { m_LightColorR = (int)(value.X / 255f); m_LightColorG = (int)(value.Y / 255f); m_LightColorB = (int)(value.Z / 255f); } }
        [Description("Distance")]
        public int Distance { get { return m_Distance; } set { m_Distance = value; } }
        [Description("Level")]
        public int Level { get { return m_Level; } set { m_Level = value; } }
        [Description("Sound Echo")]
        public int SoundEchoParameter { get { return m_SoundEchoParameter; } set { m_SoundEchoParameter = value; } }
        [Description("Sound Code")]
        public int SoundRoomCode { get { return m_SoundRoomCode; } set { m_SoundRoomCode = value; } }
        [Description("Sound Room Size")]
        public int SoundRoomSize { get { return m_SoundroomSize; } set { m_SoundroomSize = value; } }

        public MapObjectContainer RoomObjects;
        
        private string m_Name;
        private int m_RoomNumber;
        private int m_Thunder;
        private int m_VRBox;
        private int m_DustLevel;
        private int m_LightColorR;
        private int m_LightColorG;
        private int m_LightColorB;
        private int m_Distance;
        private int m_Level;
        private int m_SoundEchoParameter;
        private int m_SoundRoomCode;
        private int m_SoundroomSize;

        public void AddRoomObject(MapObject obj)
        {
            RoomObjects.AddObject(obj);
        }

        public void RemoveRoomObject(MapObject obj)
        {
            RoomObjects.RemoveObject(obj);
        }

        public override void Save(string path)
        {
            throw new NotImplementedException();
        }
    }
}
