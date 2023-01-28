using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandBoxEngine
{
    public class Bbox
    {
        public int W { get; set; }
        public int H { get; set; }
        public int Xc { get; set; }
        public int Yc { get; set; }
        public double Phi { get; set; }

        public double SpeedX { get; set; }

        public double SpeedY { get; set; }

        public double SpeedPhi { get; set; }

        public Bbox(int x, int y, int w, int h, double phi)
        {
            this.Xc = x;
            this.Yc = y;
            this.W = w;
            this.H = h;
            this.Phi = phi;
        }

        public Polygon ToPolygon()
        {
            MyVector eW1 = new MyVector(W * Math.Cos(Phi * Math.PI / 180), W * Math.Sin(Phi * Math.PI / 180));
            MyVector eH1 = new MyVector(-H * Math.Sin(Phi * Math.PI / 180), H * Math.Cos(Phi * Math.PI / 180));

            MyVector center = new MyVector(Xc, Yc);
            List<MyVector> points = new List<MyVector>();

            points.Add(center.Add(eW1.DivideBy(2)).Add(eH1.DivideBy(2)));
            points.Add(center.Add(eW1.DivideBy(2)).Substract(eH1.DivideBy(2)));
            points.Add(center.Substract(eW1.DivideBy(2)).Substract(eH1.DivideBy(2)));
            points.Add(center.Substract(eW1.DivideBy(2)).Add(eH1.DivideBy(2)));

            return new Polygon(points);
        }
    }


}
