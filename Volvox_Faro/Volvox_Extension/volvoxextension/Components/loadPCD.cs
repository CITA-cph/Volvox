using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Volvox_Cloud;

namespace VolvoxExtension.Components
{
    public class loadPCD : tasTools.Components.tasThreaded_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public loadPCD()
          : base("Load PCD", "PCDload",
              "Imports PCL PCD format pointclouds.",
              "Volvox", "Glyph")
        {
        }

        // Global Variables
        string Log = "";
        PointCloud Cloud;
        object Lock = new object();
        string Path = "";
        double Scale = 1.0;
        bool Intensity = false;

        // Set ThreadedComponents Process
        protected override void Process()
        {
            UpdateLog("Loading");
            Log = "";
            tasTools.IO.PCD_Importer Importer = new tasTools.IO.PCD_Importer();
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

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            Grasshopper.Kernel.Parameters.Param_FilePath F_param = new Grasshopper.Kernel.Parameters.Param_FilePath() ;
            pManager.AddParameter(F_param, "File path", "F", "File path", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Intensity", "I", "Use intensity instead of color.", GH_ParamAccess.item, true);

            base.RegisterInputParams(pManager);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cloud", "C", "Imported point cloud.", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "d", "Debugging info.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
 
            // Access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            DA.GetData("Intensity", ref Intensity);
            if (!DA.GetData("File path", ref Path)) return;

            // Validate the data and warn the user if invalid data is supplied.
            if (!System.IO.File.Exists(Path) || !Path.EndsWith(".pcd"))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid file specified!");
                return;
            }

            // Run Threaded Component
            base.RunThread(DA);
            // Output
            if (Cloud != null && !Running)
            {
                Cloud.UserDictionary.Set("Path", Path);
                // get resolution from file name (ask Tom about integrating this in the cloud directly)
                string cloudName = System.IO.Path.GetFileNameWithoutExtension(Path);
                string resolution = cloudName.Substring(cloudName.Length - 3);
                Cloud.UserDictionary.Set("resolution", resolution);
                DA.SetData("Cloud", new GH_Cloud(Cloud));
            }
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{b16a81cd-0ef7-44f7-86be-9befe0570615}"); }
        }
    }
}