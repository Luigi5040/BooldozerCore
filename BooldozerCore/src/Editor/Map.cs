using System;
using System.Collections.Generic;
using System.Text;
using BooldozerCore.Jmp;
using BooldozerCore.Jmp.Objects;
using BooldozerCore.Collision;

namespace BooldozerCore.Editor
{
    /// <summary>
    /// Container for all of the data that makes up a map - rooms, collision, and entities.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// A list of Room objects that stores all of the currently loaded rooms.
        /// </summary>
        //List<Room> Rooms;

        /// <summary>
        /// An array of MapObject lists that stores all of the currently loaded MapObjects.
        /// </summary>
        MapObjectContainer Objects;

        /// <summary>
        /// The collision mesh for the currently loaded map.
        /// </summary>
        CollisionMesh MapCollision;

        public Map()
        {
            //Rooms = new List<Room>();
            Objects = new MapObjectContainer();
            MapCollision = new CollisionMesh();
        }

        public void AddMapObject(MapObject obj)
        {
            Objects.AddObject(obj);

            //if (obj.HasRoomNumber)
                //Rooms[roomNum].AddRoomObject(obj);
        }

        public void RemoveMapObject(MapObject obj)
        {
            Objects.RemoveObject(obj);

            //if (obj.HasRoomNumber)
                //Rooms[roomNum].RemoveRoomObject(obj);
        }
    }
}
