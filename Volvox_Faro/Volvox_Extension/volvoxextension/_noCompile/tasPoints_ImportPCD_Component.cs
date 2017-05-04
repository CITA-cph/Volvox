using System;
using System.Collections.Generic;
using Volvox_Cloud;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace tasTools.Components
{
    public class tasPoints_ImportPCD_Component : tasThreaded_Component
    {

        public tasPoints_ImportPCD_Component()
          : base("Load PCD", "PCDload",
              "Imports PCL PCD format pointclouds.",
              "Volvox", "I/O")
        {
        }

        string Log = "";
        PointCloud Cloud;
        object Lock = new object();
        string Path = "";
        double Scale = 1.0;
        bool Intensity = false;

        protected override void Process()
        {
            UpdateLog("Loading");
            Log = "";
            
            IO.PCD_Importer Importer = new IO.PCD_Importer();
            try
            {
                RWL.AcquireWriterLock(100);
                try
                {
                    Importer.Import(Path, out Cloud, Intensity);
                }
                finally
                { 
                    RWL.ReleaseWriterLock();
                }
            }
            catch (Exception)
            {
                UpdateLog("Error.");
            }
           
            Log += Importer.log;

            base.Process();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to PCD file.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Intensity", "I", "Use intensity instead of color.", GH_ParamAccess.item, true);

            base.RegisterInputParams(pManager);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cloud", "C", "Imported point cloud.", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "d", "Debugging info.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Scale = tasUtility.ScaleFromMeter(1.0);

            DA.GetData("Intensity", ref Intensity);
            if (!DA.GetData("Path", ref Path)) return;

            if (!System.IO.File.Exists(Path) || !Path.EndsWith(".pcd"))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid file specified!");
                return;
            }

            base.RunThread(DA);

            //try
            //{
                if (Cloud != null && !Running)
                    DA.SetData("Cloud", new GH_Cloud(Cloud));
            //}
            //catch
            //{
                //DA.SetData("Cloud", null);
            //}

            DA.SetData("debug", Log);
        }
        /*
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.icon_import_pcd_component_24x24;
            }
        }
        */
        public override Guid ComponentGuid
        {
            get { return new Guid("{d8f5ed1b-a3d1-45ad-ab4a-c23841445608}"); }
        }
    }
}
//#endif