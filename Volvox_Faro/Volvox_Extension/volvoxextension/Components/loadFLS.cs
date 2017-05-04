using System;
using System.Threading;
using System.Collections.Generic;
using Volvox_Cloud;
using FaroNET;

using Grasshopper.Kernel;
using Rhino.Geometry;


namespace tasTools.Components
{

    public class tasPoints_ImportFLS_Component : tasThreaded_Component
    {
        Thread ProcessThread;
        PointCloud Cloud;
        string Log = "";
        string Path = "";
        int Step = 1;

        bool Intensity = false;
        double Scale = 1.0;
        bool PrevStart = false;

        public tasPoints_ImportFLS_Component()
          : base("Load FLS", "FLSload",
              "Imports Faro FLS format pointclouds.",
              "Volvox", "Faro")
        {
            //FaroScan.Initialize();
        }

        ~tasPoints_ImportFLS_Component()
        {
            //FaroScan.Uninitialize();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            Grasshopper.Kernel.Parameters.Param_FilePath F_param = new Grasshopper.Kernel.Parameters.Param_FilePath();
            pManager.AddParameter(F_param, "File path", "F", "File path", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Intensity", "Int", "Use intensity instead of color.", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Step", "S", "Import every nth point.", GH_ParamAccess.item, 4);
            pManager.AddBooleanParameter("Run", "R", "Do import.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Abort", "A", "Abort import.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cloud", "C", "Imported point cloud.", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "d", "Debugging info.", GH_ParamAccess.item);
        }

        protected override void Process()
        {
            Cloud = null;
            Scale = 1000.0; // tasTools.tasUtility.ScaleFromMeter();

            // START FARO
            Result Res = FaroScan.IsInitialized();
            //UpdateLog("FaroInit: " + Res.ToString());
            UpdateLog("Initializing");

            if (Res != Result.Success)
                Res = FaroScan.Initialize();

            if (!Running || Res != Result.Success)
            {
                Abort();
                return;
            }
            UpdateLog("Loading scan");

            Res = FaroScan.Load(Path);

            if (!Running || Res != Result.Success)
            {
                Abort();
                return;
            }

            UpdateLog("Parsing points");

            double[] PointsRaw;
            int[] ColorRaw;

            Res = FaroScan.GetXYZPoints(0, out PointsRaw, out ColorRaw, Step);
            List<Point3d> Points = new List<Point3d>();

            FaroScan.UnloadAll();
            FaroScan.Uninitialize();

            // END FARO

            if (!Running)
            {
                return;
            }

            UpdateLog("Creating PointCloud");

            for (int i = 0; i < PointsRaw.Length; i += 3)
            {
                Points.Add(new Point3d(PointsRaw[i], PointsRaw[i + 1], PointsRaw[i + 2]) * Scale);
            }

            if (!Running)
            {
                return;
            }

            Cloud = new PointCloud(Points);

            if(Intensity)
                for (int i = 0; i < Cloud.Count; ++i)
                {
                    int col = ColorRaw[i];
                    col |= col << 8;
                    col |= col << 16;
                    Cloud[i].Color = System.Drawing.Color.FromArgb(col);
                }
            else
                for (int i = 0; i < Cloud.Count; ++i)
                {
                    Cloud[i].Color = System.Drawing.Color.FromArgb(ColorRaw[i]);
                }

            UpdateLog("Success");
            Thread.Sleep(1000);
            UpdateLog("");
            Running = false;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            DA.GetData("Intensity", ref Intensity);
            DA.GetData("Path", ref Path);
            if (Path == "" || !(System.IO.File.Exists(Path) || System.IO.Directory.Exists(Path)))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid path: " + Path);
                return;
            }

            DA.GetData("Step", ref Step);

            base.RunThread(DA);

            if (Cloud != null)
                DA.SetData("Cloud", new GH_Cloud(Cloud));
            else
                DA.SetData("Cloud", null);
            DA.SetData("debug", Log);

        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //return Properties.Resources.icon_import_fls_component_24x24;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{226c699c-a33e-4f90-84fb-5390c69599aa}"); }
        }
    }
}
