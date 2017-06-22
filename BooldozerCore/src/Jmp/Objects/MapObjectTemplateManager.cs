using System;
using System.Collections.Generic;
using System.Text;

namespace BooldozerCore.Jmp.Objects
{
    public static class MapObjectTemplateManager
    {
        public static MapObject GetTemplate(JmpType type)
        {
            switch (type)
            {
                case JmpType.Character:
                    //return new Character();
                    break;
                case JmpType.Enemy:
                    return new Enemy();
                case JmpType.Event:
                    //return new Event();
                    break;
                case JmpType.Furniture:
                    //return new Furniture();
                    break;
                case JmpType.Generator:
                    //return new Generator();
                    break;
                case JmpType.None:
                    throw new Exception("MapObjectTemplateManager received JmpTyp.None!");
                default:
                    return null;
            }

            return null;
        }
    }
}
