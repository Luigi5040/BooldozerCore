using System;
using System.Collections.Generic;
using System.Text;

namespace BooldozerCore.Jmp.Objects
{
    public class MapObjectContainer
    {
        List<MapObject>[] MapObjects;

        // This allows the use of [] on MapObjectContainer to get a specific MapObject list.
        // mapObjCont[0], for example, will return the MapObject list containing Character objects.
        public List<MapObject> this[int index]
        {
            get { return MapObjects[index]; }
            set { MapObjects[index] = value; }
        }

        public MapObjectContainer()
        {
            MapObjects = new List<MapObject>[Enum.GetValues(typeof(JmpType)).Length];
        }

        /// <summary>
        /// Adds the given object to the appropriate list of MapObjects.
        /// </summary>
        /// <param name="obj">MapObject to add</param>
        public void AddObject(MapObject obj)
        {
            JmpType type = TypeToEnum(obj.GetType());
            List<MapObject> objList = MapObjects[(int)type];

            // If the list is null, instantiate it
            if (objList == null)
                objList = new List<MapObject>();

            objList.Add(obj);
        }

        /// <summary>
        /// Removes the given object from the appropriate list of MapObjects, given that the list is not null and contains the object.
        /// </summary>
        /// <param name="obj">MapObject to remove</param>
        public void RemoveObject(MapObject obj)
        {
            JmpType type = TypeToEnum(obj.GetType());
            List<MapObject> objList = MapObjects[(int)type];

            // Error out if the list is null
            if (objList == null)
            {
                Console.WriteLine($"Failed to remove object. Object list for type { type.ToString() } is null!");
                return;
            }
            // Error out if the list doesn't contain the object we want to remove
            else if (!objList.Contains(obj))
            {
                Console.WriteLine($"Failed to remove object. Object list for type { type.ToString() } doesn't contain the object to remove!");
                return;
            }

            objList.Remove(obj);
        }

        /// <summary>
        /// Converts the given Type to its corresponding JmpType. Returns JmpType.None if no match is found.
        /// </summary>
        /// <param name="t">The Type to convert</param>
        /// <returns>The JmpType corresponding to the given Type</returns>
        public JmpType TypeToEnum(Type t)
        {
            if (t == typeof(Enemy))
                return JmpType.Enemy;
            else if (t == typeof(Room))
                return JmpType.Room;
            else
                return JmpType.None;
        }

        /// <summary>
        /// Converts the given JmpType to its corresponding Type. Returns null if no match is found.
        /// </summary>
        /// <param name="enumType">The JmpType to convert</param>
        /// <returns>The Type corresponding to the given JmpType</returns>
        public Type EnumToType(JmpType enumType)
        {
            if (enumType == JmpType.Enemy)
                return typeof(Enemy);
            else if (enumType == JmpType.Room)
                return typeof(Room);
            else
                return null;
        }
    }
}
