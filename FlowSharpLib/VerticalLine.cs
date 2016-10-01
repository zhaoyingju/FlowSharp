﻿/* The MIT License (MIT)
* 
* Copyright (c) 2016 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	public class VerticalLine : Line
	{
		// Fixes background erase issues with dynamic connector.
		public override Rectangle UpdateRectangle { get { return DisplayRectangle.Grow(anchorWidthHeight + 1 + BorderPen.Width); } }

		public VerticalLine(Canvas canvas) : base(canvas)
		{
			HasCornerAnchors = false;
			HasCenterAnchors = false;
			HasTopBottomAnchors = true;
			HasCornerConnections = false;
			HasCenterConnections = false;
			HasTopBottomConnections = true;
		}

		public override GraphicElement Clone(Canvas canvas)
		{
			VerticalLine line = (VerticalLine)base.Clone(canvas);
			line.StartCap = StartCap;
			line.EndCap = EndCap;

			return line;
		}

		public override Rectangle DefaultRectangle()
		{
			return new Rectangle(20, 20, 20, 40);
		}

		public override void MoveAnchor(ConnectionPoint cpShape, ConnectionPoint cp)
		{
			if (cp.Type == GripType.Start)
			{
				DisplayRectangle = new Rectangle(cpShape.Point.X-BaseController.MIN_WIDTH/2, cpShape.Point.Y, DisplayRectangle.Size.Width, DisplayRectangle.Size.Height);
			}
			else
			{
				DisplayRectangle = new Rectangle(cpShape.Point.X-BaseController.MIN_WIDTH/2, cpShape.Point.Y - DisplayRectangle.Size.Height, DisplayRectangle.Size.Width, DisplayRectangle.Size.Height);
			}

			// TODO: Redraw is updating too much in this case -- causes jerky motion of attached shape.
			// canvas.Controller.Redraw(this, (cpShape.Point.X - cp.Point.X).Abs() + BaseController.MIN_WIDTH, (cpShape.Point.Y - cp.Point.Y).Abs() + BaseController.MIN_HEIGHT);
		}

		public override void Draw(Graphics gr)
		{
			// See CustomLineCap for creating other possible endcaps besides arrows.
			// https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.customlinecap(v=vs.110).aspx

			AdjustableArrowCap adjCap = new AdjustableArrowCap(5, 5, true);
			Pen pen = (Pen)BorderPen.Clone();

			if (ShowLineAsSelected)
			{
				pen.Color = pen.Color.ToArgb() == Color.Red.ToArgb() ? Color.Blue : Color.Red;
			}

			if (StartCap == AvailableLineCap.Arrow)
			{
				pen.CustomStartCap = adjCap;
			}

			if (EndCap == AvailableLineCap.Arrow)
			{
				pen.CustomEndCap = adjCap;
			}

			gr.DrawLine(pen, DisplayRectangle.TopMiddle(), DisplayRectangle.BottomMiddle());
			pen.Dispose();

			base.Draw(gr);
		}
	}
}
