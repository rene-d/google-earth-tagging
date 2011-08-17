// inspired from http://stackoverflow.com/questions/4504956/formatting-double-to-latitude-longitude-human-readable-format

using System;
using System.Text;

namespace GETagging
{
    public class Coordinate
    {
        public double Degrees { get; set;  }
        public double Minutes { get; set; }
        public double Seconds { get; set; }
        public CoordinatesPosition Position { get; set; }

        public Coordinate() 
        { 
        }

        public Coordinate(double value, CoordinatesPosition position)
        {
            //sanity
            if (value < 0 && position == CoordinatesPosition.N)
                position = CoordinatesPosition.S;
            //sanity
            if (value < 0 && position == CoordinatesPosition.E)
                position = CoordinatesPosition.W;
            //sanity
            if (value > 0 && position == CoordinatesPosition.S)
                position = CoordinatesPosition.N;
            //sanity
            if (value > 0 && position == CoordinatesPosition.W)
                position = CoordinatesPosition.E;

            value = Math.Abs(value);
            Degrees = Math.Truncate(value);

            value = (value - Degrees) * 60;       
            Minutes = Math.Truncate(value);

            Seconds = (value - Minutes) * 60; 

            Position = position;
        }

        public double ToDouble()
        {
            double result = (Degrees) + (Minutes) / 60 + (Seconds) / 3600;
            return Position == CoordinatesPosition.W || Position == CoordinatesPosition.S ? -result : result;
        }

        public override string ToString()
        {
            return string.Format("{0}° {1}' {2:.##}\" {3}", Degrees, Minutes, Seconds, Position);
        }
    }

    public enum CoordinatesPosition
    {
        N, E, S, W
    }
}
