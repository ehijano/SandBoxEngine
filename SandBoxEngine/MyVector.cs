using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandBoxEngine
{
    public class MyVector
    {
        public double vx, vy;
        public MyVector(double vx, double vy)
        {
            this.vx = vx;
            this.vy = vy;
        }

        public double DotProduct(MyVector v2)
        {
            return vx * v2.vx + vy * v2.vy;
        }

        public double Norm()
        {
            return Math.Sqrt(vx * vx + vy * vy);
        }

        public MyVector Add(MyVector v2)
        {
            return new MyVector(vx + v2.vx, vy + v2.vy);
        }

        public MyVector Substract(MyVector v2)
        {
            return new MyVector(vx - v2.vx, vy - v2.vy);
        }

        public MyVector DivideBy(double div)
        {
            return new MyVector(vx / div, vy / div);
        }

        public MyVector MultiplyBy(double mult)
        {
            return new MyVector(vx * mult, vy * mult);
        }

        public MyVector Normalize()
        {
            double norm = this.Norm();
            vx = vx / norm;
            vy = vy / norm;
            return this;
        }

        public MyVector Perpendicular()
        {
            double norm = this.Norm();
            return new MyVector(-vy / norm, vx / norm);
        }

    }

}
