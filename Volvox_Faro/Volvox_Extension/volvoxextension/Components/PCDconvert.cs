using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;

namespace VolvoxExtension.Components
{



    public class PCD : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PCD() : base("PCD Converter2", "toPCD", "Converts .fls file to .pcd file.", "Volvox", "Faro")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Grasshopper.Kernel.Parameters.Param_FilePath(), "File path", "F", "File path", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Resolution X", "Rx", "Row steps to load", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Resolution Y", "Ry", "Column steps to load", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Run", "R", "Run component", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Abort", "A", "Abort loading", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "L", "Log from Convertion Thread.", GH_ParamAccess.list);
            pManager.AddTextParameter("PCD FilePath", "P", "Path to PCD file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string inPath = null;
            int Rx = 0;
            int Ry = 0;
            bool Run = false;
            bool Abort = false;

            if (!(DA.GetData(0, ref inPath)))
                return;
            if (!(DA.GetData(1, ref Rx)))
                return;
            if (!(DA.GetData(2, ref Ry)))
                return;
            if (!(DA.GetData(3, ref Run)))
                return;
            if (!(DA.GetData(4, ref Abort)))
                return;

            // Check if filepath is .fls
            if (!(System.IO.Path.GetExtension(inPath) == ".fls"))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "FilePath is not correct.");
                return;
            }
            // Get Right FilePath (.fls is a folder with .fls)
            string filePath = null;
            if (inPath.Split('.').Length == 3)
            {
                filePath = inPath;
            }
            else if (inPath.Split('.').Length == 2)
            {
                string fileName = System.IO.Path.GetFileName(inPath);
                filePath = inPath + "\\" + fileName;
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "FilePath is not correct.");
            }

            // run convert thread
            if (Run)
            {
                if (!threadMade)
                {
                    this.Message = "";
                    Convert(filePath, Rx, Rx, 0, 0);
                    threadMade = true;
                    if (Rx != 1 & Ry != 1)
                    {
                        Convert(filePath, 1, 1, 0, 0);
                        extraThread = true;
                    }
                }
            }

            // running
            if (!finished)
            {
                if (threadMade)
                {
                    this.Message = "Converting...";
                }
            }

            // finished
            if (finished)
            {
                threadMade = false;
                this.Message = "";
            }

            // check if output excists
            string pcdPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension(filePath) + "_" + Rx + "x" + Ry + ".pcd");
            if (System.IO.File.Exists(pcdPath))
            {
                outPcdPath = pcdPath;
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "PCD output file does not exist.");
                return;
            }
            // check if right image path exist
            string imgPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension(filePath) + "_1x1_grey.png");
            if (!System.IO.File.Exists(imgPath))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Right Sperical Panorama Image does not exist.");
                this.Message = "Processing Image";
            }
            else
            {
                if (finished)
                {
                    FileInfo fi = new FileInfo(imgPath);
                    if (IsFileLocked(fi))
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Right Sperical Panorama Image does not exist.");
                        this.Message = "Processing Image";
                    }
                    else
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Blank, "");
                        this.Message = "";
                    }
                }

            }


            //output the data
            DA.SetDataList(0, cmdOutput);
            DA.SetData(1, outPcdPath);
        }

        List<string> cmdOutput = new List<string>();
        Action expire = ExpireComponent;
        bool extraThread = false;
        bool finished = false;

        bool threadMade = false;

        string outPcdPath = string.Empty;
        public void Convert(string filePath, int resX, int resY, int makeMesh, int useDisplay)
        {
            string arg = string.Empty;
            arg = filePath;
            arg = arg + " " + resX.ToString();
            arg = arg + " " + resY.ToString();
            arg = arg + " " + makeMesh.ToString();
            arg = arg + " " + useDisplay.ToString();

            finished = false;
            outPcdPath = string.Empty;

            System.Threading.Thread thread = new System.Threading.Thread(ThreadConvert);
            thread.IsBackground = true;
            thread.Start(arg);
        }

        public void ThreadConvert(string arg)
        {
            cmdOutput.Clear();
            System.Diagnostics.Process process = default(System.Diagnostics.Process);
            string GHlib = Folders.DefaultAssemblyFolder;
            System.Diagnostics.ProcessStartInfo p = new System.Diagnostics.ProcessStartInfo(GHlib + "\\Volvox\\ScanConverter\\ScanConverter.exe", arg);
            p.RedirectStandardError = true;
            p.RedirectStandardOutput = true;
            p.CreateNoWindow = true;
            p.UseShellExecute = false;

            process = System.Diagnostics.Process.Start(p);

            //Read cmd output
            System.IO.StreamReader oStreamReader = process.StandardOutput;
            string @out = null;
            do
            {
                @out = oStreamReader.ReadLine();

                if ((@out != null))
                {
                    cmdOutput.Insert(0, @out);
                    Rhino.RhinoApp.MainApplicationWindow.Invoke(expire);
                }
            } while (!(@out == null));


            //wait for process to finish
            process.WaitForExit();
            //If extraThread Then
            //extraThread = False
            //Else
            finished = true;
            //End If

            Rhino.RhinoApp.MainApplicationWindow.Invoke(expire);
        }

        //EXPIRE COMPONENT
        private void ExpireComponent()
        {
            this.ExpireSolution(true);
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException generatedExceptionName)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            //file is not locked
            return false;
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            //You can add image files to your project resources and access them like this:
            // return Resources.IconForThisComponent;
            get { return null; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{8aea87ec-a434-4d39-82dd-28788f406c3f}"); }
        }
    }
}
