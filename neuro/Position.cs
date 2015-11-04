using System;
using System.Runtime.CompilerServices;
using MongoDB.Bson.Serialization.Attributes;

namespace neuro
{
    [Serializable]
    public class Position
    {
        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [BsonElement("x")]
        public int X { get; set; }
        [BsonElement("y")]
        public int Y { get; set; }
        [BsonElement("z")]
        public int Z { get; set; }

        public static double Dist(Position p1, Position p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z));
        }
    }
}