using System;
using System.Threading;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace tasTools.Components
{
    public abstract class tasThreaded_Component : GH_Component
    {
        Thread ProcessThread;
        protected static ReaderWriterLock RWL = new ReaderWriterLock();
        protected volatile bool Running = false;
        volatile bool ThreadFinished = false;
        bool PrevStart = false;

        public tasThreaded_Component(string Name, string Nickname, string Description, string Category, string SubCategory)
          : base(Name, Nickname,
              Description,
              Category, SubCategory)
        {
        }

        protected virtual void Process()
        {
            Running = false;
            ThreadFinished = true;
            this.ExpireSolution(false);
            UpdateLog("");

            return;
        }

        protected virtual void Abort()
        {
            Running = false;
            UpdateLog("");
        }

        protected void UpdateLog(string Message)
        {
            this.Message = Message;
            this.OnPingDocument().ScheduleSolution(30);
            //Thread.Sleep(50);
        }

        protected void RunThread(IGH_DataAccess DA)
        {
            if (ThreadFinished)
            {
                ProcessThread = null;
                Running = false;
                ThreadFinished = false;
            }

            bool Start = false;
            bool Abort = false;

            DA.GetData("Run", ref Start);
            DA.GetData("Abort", ref Abort);

            if (Abort)
                Start = Running = false;

            if (Start && !Running && !PrevStart)
            {
                Running = true;
                ProcessThread = new Thread(new ThreadStart(Process));
                ProcessThread.IsBackground = true;
                ProcessThread.Start();
            }

            PrevStart = Start;
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Run process.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Abort", "A", "Abort process.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{7b331a9a-61eb-4e7c-a57e-e0e0312cce89}"); }
        }
    }
}