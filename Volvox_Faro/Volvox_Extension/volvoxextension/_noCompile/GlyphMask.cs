using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI.Canvas;
using System.Drawing;
using Grasshopper.GUI;

namespace VolvoxExtension
{
    public class tasBitmask3_Component : GH_Param<GH_Integer>
    {
        public tasBitmask3_Component()
          : base(new GH_InstanceDescription("Bitmask 3", "Bits3",
              "Bitmask composed of 3 bits.",
              "tasTools", "Test"))
        {
        }


        public override void CreateAttributes()
        {
            m_attributes = new BitMask3ObjectAttributes(this);
        }


        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.primary;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{a0aa1443-f885-45dc-98f1-0a6255d01f1c}"); }
        }

        private int m_value = 0;
        public int Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        protected override void CollectVolatileData_Custom()
        {
            VolatileData.Clear();
            AddVolatileData(new Grasshopper.Kernel.Data.GH_Path(0), 0, new GH_Integer(Value));
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("Bitmask3", m_value);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            m_value = 0;
            reader.TryGetInt32("Bitmask3", ref m_value);
            BitMask3ObjectAttributes attr = m_attributes as BitMask3ObjectAttributes;
            attr.SetBitmask(m_value);

            return base.Read(reader);
        }
    }

    public class BitMask3ObjectAttributes : GH_Attributes<tasBitmask3_Component>
    {

        GH_Palette palWhite = GH_Palette.White;
        GH_Palette palBlack = GH_Palette.Black;

        private bool[] m_bits;

        public BitMask3ObjectAttributes(tasBitmask3_Component owner)
            : base(owner)
        {
            m_bits = new bool[9];
            SetBitmask(owner.Value);
        }

        public override bool HasInputGrip { get { return false; } }
        public override bool HasOutputGrip { get { return true; } }

        private const int ButtonSize = 30;

        //Our object is always the same size, but it needs to be anchored to the pivot.
        protected override void Layout()
        {
            Pivot = GH_Convert.ToPoint(Pivot);
            Bounds = new RectangleF(Pivot, new SizeF(3 * ButtonSize, 3 * ButtonSize));
        }

        private Rectangle Button(int column, int row)
        {
            int x = Convert.ToInt32(Pivot.X);
            int y = Convert.ToInt32(Pivot.Y);
            return new Rectangle(x + column * ButtonSize, y + row * ButtonSize, ButtonSize, ButtonSize);
        }

        private bool Value(int column, int row)
        {
            return m_bits[column+row];
        }

        public int GetBitmask()
        {
            int r = (m_bits[8] ? 1 << 0 : 0) | (m_bits[7] ? 1 << 0 : 0) | (m_bits[6] ? 1 << 0 : 0) | (m_bits[5] ? 1 << 0 : 0) | (m_bits[4] ? 1 << 0 : 0) | (m_bits[3] ? 1 << 0 : 0) | (m_bits[2] ? 1 << 0 : 0) | (m_bits[1] ? 1 << 1 : 0) | (m_bits[0] ? 1 << 2 : 0);
            return r;
        }

        public void SetBitmask(int value)
        {
            m_bits[8] = ((value & 1) > 0 ? true : false);
            m_bits[7] = ((value & 1) > 0 ? true : false);
            m_bits[6] = ((value & 1) > 0 ? true : false);
            m_bits[5] = ((value & 1) > 0 ? true : false);
            m_bits[4] = ((value & 1) > 0 ? true : false);
            m_bits[3] = ((value & 1) > 0 ? true : false);
            m_bits[2] = ((value & 1) > 0 ? true : false);
            m_bits[1] = ((value & 2) > 0 ? true : false);
            m_bits[0] = ((value & 4) > 0 ? true : false);
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            //On a double click we'll set the owner value.
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                for (int col = 0; col < 3; col++)
                {
                    for (int row = 0; row < 3; row++)
                    { 
                        RectangleF button = Button(col,row);
                        if (button.Contains(e.CanvasLocation))
                        {
                            m_bits[col+row] = !m_bits[col+row];
                            int value = GetBitmask();
                            Owner.RecordUndoEvent("Bit Change");
                            Owner.Value = value;
                            Owner.ExpireSolution(true);
                            return GH_ObjectResponse.Handled;
                            
                        }
                    }
                }
            }

            return base.RespondToMouseDoubleClick(sender, e);
        }
        public override void SetupTooltip(PointF point, GH_TooltipDisplayEventArgs e)
        {
            base.SetupTooltip(point, e);
            e.Description = "Double click to set a new integer";
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Objects)
            {
                //Render output grip.
                GH_CapsuleRenderEngine.RenderOutputGrip(graphics, canvas.Viewport.Zoom, OutputGrip, true);

                //Render capsules.
                for (int col = 0; col < 3; col++)
                {
                    for (int row = 0; row < 3; row++)
                    {
                        Rectangle button = Button(col,row);
                        GH_Capsule capsule = Value(col, row) ?
                            GH_Capsule.CreateTextCapsule(button, button, palWhite, "1", 0, 0)
                            :
                            GH_Capsule.CreateTextCapsule(button, button, palBlack, "0", 0, 0);

                        capsule.Render(graphics, Selected, Owner.Locked, false);
                        capsule.Dispose();
                    }

                }
            }
        }
    }
}


