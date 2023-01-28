using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandBoxEngine
{
    public class Polygon
    {
        public List<MyVector> edges { get; set; }
        public List<MyVector> points { get; set; }
        public Polygon(List<MyVector> points)
        {
            this.points = points;


            edges = new List<MyVector>();
            for (int i = 0; i < points.Count() - 1; i++)
            {
                edges.Add(points.ElementAt(i + 1).Substract(points.ElementAt(i)));
            }
            edges.Add(points.ElementAt(0).Substract(points.ElementAt(points.Count() - 1)));
        }

        public MyVector CenterOfMass()
        {
            MyVector com = new MyVector(0, 0);
            int pointNumber = points.Count();
            for (int i = 0; i < pointNumber; i++)
            {
                com = com.Add(points.ElementAt(i));
            }
            return com.DivideBy(pointNumber);
        }

        public double PolygonDistance(Polygon poly)
        {
            return this.CenterOfMass().Substract(poly.CenterOfMass()).Norm();
        }

    }
}
