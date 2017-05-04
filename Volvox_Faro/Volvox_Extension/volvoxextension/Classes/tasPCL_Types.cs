using System.IO;
using Rhino;
using Rhino.Geometry;

namespace tasTools.IO
{
    public enum Fields
    {
        x, y, z, r, g, b, intensity, normal_x, normal_y, normal_z
    };

    public class PCD_PointBuilder
    {
        public PCD_PointBuilder()
        {
            for (int i = 0; i < 10; ++i)
            {
                data[i] = 0.0;
            }
        }
        public object[] data = new object[10];
        public Point3d Point()
        {
            return new Point3d(
                System.Convert.ToDouble(data[0]),
                System.Convert.ToDouble(data[1]),
                System.Convert.ToDouble(data[2]));
        }
        public Vector3d Normal()
        {
            return new Vector3d(
                System.Convert.ToDouble(data[7]),
                System.Convert.ToDouble(data[8]),
                System.Convert.ToDouble(data[9]));
        }
        public int Color()
        {
            int col = System.Convert.ToInt32(data[3]);
            col |= System.Convert.ToInt32(data[4]) << 8;
            col |= System.Convert.ToInt32(data[5]) << 16;
            col |= System.Convert.ToInt32(data[6]) << 24;
            return col;
        }
        public int Intensity()
        {
            int col = System.Convert.ToInt32(data[6]);
            col |= col << 8;
            col |= col << 16;
            return col;
        }
    }

    public class PCD_Field
    {
        public virtual void read(byte[] buffer, int pos, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = null;
        }
        public virtual void readASCII(string str, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = null;
        }
        public int id = 0;
        public int size = 0;
    }

    public class PCD_Field_4F : PCD_Field
    {
        public PCD_Field_4F() { size = 4; }
        public override void read(byte[] buffer, int pos, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = (double)System.BitConverter.ToSingle(buffer, pos);
        }
        public override void readASCII(string str, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = System.Convert.ToSingle(str);
        }
    }

    public class PCD_Field_4U : PCD_Field
    {
        public PCD_Field_4U() { size = 4; }
        public override void read(byte[] buffer, int pos, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = (int)System.BitConverter.ToUInt32(buffer, pos);
        }
        public override void readASCII(string str, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = System.Convert.ToInt32(str);
        }
    }

    public class PCD_Field_1U : PCD_Field
    {
        public PCD_Field_1U() { size = 1; }
        public override void read(byte[] buffer, int pos, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = (int)buffer[pos];
        }
        public override void readASCII(string str, ref PCD_PointBuilder pbb)
        {
            if (id > -1)
                pbb.data[id] = System.Convert.ToInt32(str);
        }
    }

    public class PCD_Field_None : PCD_Field
    {
        public override void read(byte[] buffer, int pos, ref PCD_PointBuilder pbb) { }
    }

}