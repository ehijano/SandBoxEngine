using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandBoxEngine
{
    public class Circle
    {
        public int Xc { get; set; }
        public int Yc { get; set; }
        public int R { get; set; }
        public double SpeedX { get; set; }
        public double SpeedY { get; set; }

        public double restitutionCoefficient = 0.6, mass = 1.0;
        public Circle(int x, int y, int r)
        {
            this.Xc = x;
            this.Yc = y;
            this.R = r;
            this.SpeedX = 0.0;
            this.SpeedY = 0.0;
        }

        public Boolean collidesCircle(Circle c)
        {
            int r = R + c.R;
            r *= r;
            return (r > (Xc - c.Xc) * (Xc - c.Xc) + (Yc - c.Yc) * (Yc - c.Yc));
        }

        public Boolean collidesFloor(int floorHeight)
        {
            return (Yc + R > floorHeight);
        }
    }
}
