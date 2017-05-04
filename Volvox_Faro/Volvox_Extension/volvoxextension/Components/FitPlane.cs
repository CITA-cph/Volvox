using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using Grasshopper.Kernel;
using Volvox_Cloud;
using Rhino.Geometry;

namespace VolvoxExtension.Components
{




    public class FitPlane : GH_Component
    {

        public FitPlane() : base("Best Fit Plane", "FitPln", "Get Best Fit Plane in the Point Cloud", "Volvox", "Analysis")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6051a5f5-e3cc-4d55-89e9-97a688ce6bb0"); }
        }

        //Protected Overrides ReadOnly Property Icon As Bitmap
        //Get
        //Return My.Resources.Icon_Count
        //End Get
        //End Property

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Cloud(), "Cloud", "C", "Cloud to fit plane to.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Best Fit Plane", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Cloud> pcL = new List<GH_Cloud>();
            if (!DA.GetDataList(0, pcL))
                return;

            PointCloud Mcl = new PointCloud();
            foreach (GH_Cloud pc in pcL)
            {
                Mcl.Merge(pc.Value);
            }

            IEnumerable<Point3d> points = Mcl.GetPoints();


            Plane pln = new Plane();
            Plane.FitPlaneToPoints(points, out pln);

            double X = 0;
            double Y = 0;
            double Z = 0;

            for (int i = 0; i <= Mcl.Count - 1; i += 1)
            {
                X += Mcl[i].X;
                Y += Mcl[i].Y;
                Z += Mcl[i].Z;
            }

            X = X / Mcl.Count;
            Y = Y / Mcl.Count;
            Z = Z / Mcl.Count;

            Point3d pt = new Point3d(X, Y, Z);

            pln.Origin = pt;

            Mcl.Dispose();
            //output the data
            DA.SetData(0, pln);
        }
    }
}
