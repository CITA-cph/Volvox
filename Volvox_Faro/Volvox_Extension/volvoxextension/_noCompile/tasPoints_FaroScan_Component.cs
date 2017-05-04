using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FaroNET;



namespace tasTools.Components
{
    public class tasPoints_FaroScan_Component : tasThreaded_Component
    {
        IPAddress IP;
        string LocalDir = "";
        string Log = "";

        volatile string ScanPath = "";
        bool Finished = false;

        string BaseName = "";
        int Resolution = 16;
        int NoiseFiltering = 1;
        FaroNET.Mode ScanMode = FaroNET.Mode.StationaryGrey;

        public tasPoints_FaroScan_Component()
          : base("Faro Scan", "FaroScan",
              "Run scan on a connectd Faro Focus scanner and copy the scan to a local folder.",
              "Volvox", "Faro")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("IPAddress", "IP", "IP address of scanner as string ('xxx.xxx.xxx.xxx').", GH_ParamAccess.item, "192.168.10.87");
            pManager.AddTextParameter("LocalPath", "LP", "Local folder to copy scan to.", GH_ParamAccess.item, "C:/tmp");
            pManager.AddTextParameter("BaseName", "BN", "Scan base name.", GH_ParamAccess.item, "FaroNET_Scan_");
            pManager.AddIntegerParameter("Resolution", "Res", "Scan resolution (see Faro SDK docs).", GH_ParamAccess.item, 16);
            pManager.AddIntegerParameter("NoiseFiltering", "NF", "Noise filtering setting (1, 2, or 4 only; see Faro SDK docs).", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Mode", "M", "Scan mode (0 = StationaryGrey, 1 = StationaryColor).", GH_ParamAccess.item, 0);
            base.RegisterInputParams(pManager);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Done", "D", "Flag set when scan is finished.", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "P", "Local path to new scan.", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "d", "Debugging info.", GH_ParamAccess.item);
        }

        protected override void Process()
        {
            Finished = false;
            UpdateLog("Initiating");
            FaroNET.FaroControlThreaded scanner = new FaroNET.FaroControlThreaded();
            scanner.ScannerIP = IP;
            scanner.Resolution = Resolution;
            scanner.BaseName = BaseName;
            scanner.NoiseCompression = NoiseFiltering;
            scanner.ScanType = ScanMode;

#region
            {
                scanner.RunScan();

                while (!scanner.Finished && Running)
                {
                    UpdateLog(scanner.CurrentLog);
                    Thread.Sleep(100);
                    if (scanner.Finished)
                        break;
                }
                scanner.WaitForScanner();
                System.Threading.Thread.Sleep(1000);
            }
#endregion

            UpdateLog("Copying");
            scanner.GetLastScan();
            scanner.WaitForScanner();



            //scanner.AbortScan();
            ScanPath = scanner.CopyPath;
            Finished = scanner.Finished;

            scanner = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            UpdateLog("Done");
            Thread.Sleep(1000);

            base.Process();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string IP_string = "";
            DA.GetData("IPAddress", ref IP_string);
            IP = IPAddress.Parse(IP_string);
            if (IP == null)
            {
                Finished = false;
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid IP address speicifed!");
            }

            if (!DA.GetData("LocalPath", ref LocalDir)) return;

            //if (!System.IO.Directory.Exists(LocalDir))
            //{
            //    Finished = false;
            //    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid local directory specified!");
            //    return;
            //}

            if (!DA.GetData("BaseName", ref BaseName)) BaseName = "FaroNET_Scan_";
            if (!DA.GetData("Resolution", ref Resolution)) Resolution = 16;
            if (!DA.GetData("NoiseFiltering", ref NoiseFiltering)) NoiseFiltering = 1;
            int mode_int = 0;
            DA.GetData("Mode", ref mode_int);
            switch(mode_int)
            {
                case (0):
                    ScanMode = FaroNET.Mode.StationaryGrey;
                    break;
                case (1):
                    ScanMode = FaroNET.Mode.StationaryColor;
                    break;
                default:
                    ScanMode = FaroNET.Mode.StationaryGrey;
                    break;
            }


            base.RunThread(DA);

            DA.SetData("debug", Log);
            DA.SetData("Done", Finished);
            DA.SetData("Path", ScanPath);
        }

        /*
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.icon_import_fls_component_24x24;
            }
        }
        */

        public override Guid ComponentGuid
        {
            get { return new Guid("{adae3284-65d9-4cd1-9372-bd6a51d39c23}"); }
        }
    }
}
