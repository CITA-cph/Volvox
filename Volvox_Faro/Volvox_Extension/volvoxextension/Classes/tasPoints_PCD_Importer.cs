using System;
using System.Collections.Generic;
using System.Collections;

using Rhino;
using Rhino.Geometry;

namespace tasTools.IO
{
    public class PCD_Importer
    {
        public PCD_Importer()
        {
            this.field_names = Enum.GetNames(typeof(Fields));
            this.field_count = field_names.Length;
            this.log = "";
            this.psize = 0;
            this.scale = 1000.0;
        }

        // num points
        public int num_points;
        string[] field_names;
        int field_count;
        int psize;
        public double scale;
        public bool binary = false;
        int width = 1, height = 1;

        // fields
        public List<PCD_Field> fields;

        // scanner position and orientation
        public Point3d scan_position;
        public Quaternion scan_orientation;

        private System.IO.BinaryReader br;

        public string log;

        private PCD_Field CreateField(int type, int size)
        {
            PCD_Field pf;
            int key = type | size << 8;
            switch (key)
            {
                case ((int)((int)'U' | 1 << 8)):
                    pf = new PCD_Field_1U();
                    break;
                case ((int)((int)'U' | 4 << 8)):
                    pf = new PCD_Field_4U();
                    break;
                case ((int)'F' | 4 << 8):
                    pf = new PCD_Field_4F();
                    break;
                default:
                    pf = new PCD_Field_None();
                    pf.size = size;
                    break;
            }
            return pf;
        }

        private bool OpenFile(string path, out string header)
        {
            log = "";
            this.br = new System.IO.BinaryReader(
                System.IO.File.Open(path, System.IO.FileMode.Open));
            byte b;
            int pos = 0;
            int length = (int)br.BaseStream.Length;
            int num_head_lines = 0;
            header = "";

            while (pos < length)
            {
                b = br.ReadByte();
                if (b == '\n')
                {
                    num_head_lines++;
                }
                if (num_head_lines < 11)
                {
                    pos++;
                    continue;
                }
                br.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                byte[] header_bytes = br.ReadBytes(pos + 1);
                header = System.Text.Encoding.Default.GetString(header_bytes);
                break;
            }

            if (header == "")
            {
                this.br.Close();
                return false;
            }
            return true;
        }

        private bool ParseHeader(string htext)
        {
            this.log += htext;

            // split header text into lines and items
            string[] header_lines = htext.Split('\n');
            List<List<string>> header_data = new List<List<string>>();
            for (int i = 0; i < header_lines.Length; ++i)
            {
                header_data.Add(new List<string>(header_lines[i].Split((char[])null, StringSplitOptions.RemoveEmptyEntries)));
            }

            // make parallel list of just the first line elements
            List<string> line_titles = new List<string>();
            for (int i = 0; i < header_data.Count; ++i)
            {
                if (header_data[i].Count < 1) continue;
                line_titles.Add(header_data[i][0]);
            }

            int index, lsize, prev;

            // get field names and create list of fields
            fields = new List<PCD_Field>();
            List<int> field_size = new List<int>();
            List<int> field_type = new List<int>();

            // get field type and sizes first, THEN specialize
            // get size of each field
            index = line_titles.IndexOf("SIZE");
            if (index < 0) return false;
            lsize = header_data[index].Count;

            for (int i = 0; i < lsize - 1; ++i)
            {
                field_size.Add(Convert.ToInt32(header_data[index][i + 1]));
            }

            // get type of each field
            index = line_titles.IndexOf("TYPE");
            if (index < 0) return false;

            for (int i = 0; i < lsize - 1; ++i)
            {
                field_type.Add((int)Convert.ToChar(header_data[index][i + 1]));
            }
            prev = lsize;

            index = line_titles.IndexOf("WIDTH");
            if (index >= 0)
            {
                this.width = Convert.ToInt32(header_data[index][1]);
            }

            index = line_titles.IndexOf("HEIGHT");
            if (index >= 0)
            {
                this.height = Convert.ToInt32(header_data[index][1]);
            }

            index = line_titles.IndexOf("FIELDS");
            if (index < 0) return false;
 
            lsize = header_data[index].Count;
            if (lsize != prev)
                return false;

            for (int i = 1; i < lsize; ++i)
            {
                this.fields.Add(CreateField(field_type[i - 1], field_size[i - 1]));
                int findex = Array.IndexOf(this.field_names, header_data[index][i]);
                if (header_data[index][i] == "i")
                    findex = Array.IndexOf(this.field_names, "intensity");
                this.fields[i-1].id = findex;
            }

            //if (this.fields.Count != lsize - 1)
            //    log += "WARNING: Not all fields were parsed.\n";

            // get total point struct size
            for (int i = 0; i < field_size.Count; ++i)
            {
                this.psize += field_size[i];
            }

            //PrintFields();

            // get scan position and orientation
            index = line_titles.IndexOf("VIEWPOINT");
            float[] f = new float[7];
            for (int i = 1; i < 8; ++i)
            {
                f[i - 1] = Convert.ToSingle(header_data[index][i]);
            }
            this.scan_position = new Point3d(f[0], f[1], f[2]);
            this.scan_orientation = new Quaternion(f[3], f[4], f[5], f[6]);

            // get number of points
            index = line_titles.IndexOf("POINTS");
            this.num_points = Convert.ToInt32(header_data[index][1]);

            // check PCD type
            index = line_titles.IndexOf("DATA");
            if (index < 0) return false;
            if (header_data[index][1] == "binary") this.binary = true;

            if (this.num_points < 1)
                return false;
            return true;
        }

        public bool Peek(string path)
        {
            string header;
            if (!OpenFile(path, out header))
                return false;
            if (!ParseHeader(header))
            {
                this.br.Close();
                return false;
            }
            return true;
        }

        public bool Import(string path, out PointCloud pc, bool intensity = false)
        {
            pc = new PointCloud();

            string header;
            if (!OpenFile(path, out header))
                return false;
            if (!ParseHeader(header))
            {
                if (this.br != null)
                    this.br.Close();
                return false;
            }
            
            pc.UserDictionary.Set("width", this.width);
            pc.UserDictionary.Set("height", this.height);

            if (this.binary)
            {
                byte[] buffer;

                int log_step = this.num_points / 5;

                for (int i = 0; i < this.num_points; ++i)
                {
                    //if (i % log_step == 0)
                    //    if (UpdateLog != null)
                    //        UpdateLog(((double)i/this.num_points * 100.0).ToString("0.0") + "%");
                    int pos = 0;
                    PCD_PointBuilder pbb = new PCD_PointBuilder();
                    buffer = br.ReadBytes(this.psize);
                    for (int j = 0; j < this.fields.Count; ++j)
                    {
                        this.fields[j].read(buffer, pos, ref pbb);
                        pos += this.fields[j].size;
                    }
                    if (intensity)
                        pc.Add(pbb.Point() * scale, pbb.Normal(), System.Drawing.Color.FromArgb(pbb.Intensity()));
                    else
                        pc.Add(pbb.Point() * scale, pbb.Normal(), System.Drawing.Color.FromArgb(pbb.Color()));
                }
                this.br.Close();
                return true;
            }
            else
            {
                // read all data into buffer
                byte[] buffer = br.ReadBytes((int)br.BaseStream.Length - (int)br.BaseStream.Position);

                // parse buffer as ASCII text
                string bufferStr = System.Text.Encoding.Default.GetString(buffer);

                // split string buffer into list of strings
                List<string> lines = new List<string>(bufferStr.Split('\n'));

                if (lines.Count != this.num_points)
                    log += "Header data doesn't match up with number of data lines!\n";

                // parse points from line data
                for (int i = 0; i < lines.Count; ++i)
                {
                    PCD_PointBuilder pbb = new PCD_PointBuilder();
                    string[] tokens = lines[i].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    //log += lines[i];
                    //log += " " + tokens.Length.ToString() + "\n";
                    if (tokens.Length != this.fields.Count)
                        continue;

                    for (int j = 0; j < this.fields.Count; ++j)
                    {
                        this.fields[j].readASCII(tokens[j], ref pbb);
                    }
                    if (intensity)
                        pc.Add(pbb.Point() * this.scale, pbb.Normal(), System.Drawing.Color.FromArgb(pbb.Intensity()));
                    else
                        pc.Add(pbb.Point() * this.scale, pbb.Normal(), System.Drawing.Color.FromArgb(pbb.Color()));
                }
                this.br.Close();
                return true;
            }

        }

        void PrintFields()
        {
            log += "FIELDS\n";
            for (int i = 0; i < this.fields.Count; ++i)
            {
                log += "FIELD " + i.ToString() + ": " + Enum.GetName(typeof(Fields), this.fields[i].id) + "\n";
            }
            log += "END FIELDS\n";
        }
    }
}