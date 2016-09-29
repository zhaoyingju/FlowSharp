﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FlowSharpLib
{
	public class ConnectionPropertyBag
	{
		public Guid ToElementId { get; set; }
		public ConnectionPoint ToConnectionPoint { get; set; }
		public ConnectionPoint ElementConnectionPoint { get; set; }
	}

	public class ElementPropertyBag
	{
		// For deserialization fixups.
		[XmlIgnore]
		public GraphicElement Element { get; set; }

		[XmlAttribute]
		public string ElementName { get; set; }
		[XmlAttribute]
		public Guid Id { get; set; }
		[XmlAttribute]
		public string Text { get; set; }

		public Rectangle DisplayRectangle { get; set; }
		public Point StartPoint { get; set; }
		public Point EndPoint { get; set; }

		public int BorderPenWidth { get; set; }

		[XmlAttribute]
		public bool HasCornerAnchors { get; set; }
		[XmlAttribute]
		public bool HasCenterAnchors { get; set; }
		[XmlAttribute]
		public bool HasLeftRightAnchors { get; set; }
		[XmlAttribute]
		public bool HasTopBottomAnchors { get; set; }

		[XmlAttribute]
		public bool HasCornerConnections { get; set; }
		[XmlAttribute]
		public bool HasCenterConnections { get; set; }
		[XmlAttribute]
		public bool HasLeftRightConnections { get; set; }
		[XmlAttribute]
		public bool HasTopBottomConnections { get; set; }

		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		public Guid StartConnectedShapeId { get; set; }
		public Guid EndConnectedShapeId { get; set; }

		public string TextFontFamily { get; set; }
		public float TextFontSize { get; set; }
		public bool TextFontUnderline { get; set; }
		public bool TextFontStrikeout { get; set; }
		public bool TextFontItalic { get; set; }

		public List<ConnectionPropertyBag> Connections { get; set; }

		[XmlIgnore]
		public Color TextColor { get; set; }
		[XmlElement("TextColor")]
		public int XTextColor
		{
			get { return TextColor.ToArgb(); }
			set { TextColor = Color.FromArgb(value); }
		}

		[XmlIgnore]
		public Color BorderPenColor { get; set; }
		[XmlElement("BorderPenColor")]
		public int XBorderPenColor
		{
			get { return BorderPenColor.ToArgb(); }
			set { BorderPenColor = Color.FromArgb(value); }
		}

		[XmlIgnore]
		public Color FillBrushColor { get; set; }
		[XmlElement("FillBrushColor")]
		public int XFillBrushColor
		{
			get { return FillBrushColor.ToArgb(); }
			set { FillBrushColor = Color.FromArgb(value); }
		}

		public ElementPropertyBag()
		{
			Connections = new List<ConnectionPropertyBag>();
		}
	}

	public static class Persist
	{
		public static string Serialize(List<GraphicElement> elements)
		{
			List<ElementPropertyBag> sps = new List<ElementPropertyBag>();
			elements.ForEach(el =>
			{
				ElementPropertyBag epb = new ElementPropertyBag();
				el.Serialize(epb);
				sps.Add(epb);
			});

			XmlSerializer xs = new XmlSerializer(sps.GetType());
			StringBuilder sb = new StringBuilder();
			TextWriter tw = new StringWriter(sb);
			xs.Serialize(tw, sps);

			return sb.ToString();
		}

		public static List<GraphicElement> Deserialize(Canvas canvas, string data)
		{
			List<GraphicElement> elements = new List<FlowSharpLib.GraphicElement>();
			XmlSerializer xs = new XmlSerializer(typeof(List<ElementPropertyBag>));
			TextReader tr = new StringReader(data);
			List<ElementPropertyBag> sps = (List<ElementPropertyBag>)xs.Deserialize(tr);

			foreach (ElementPropertyBag epb in sps)
			{
				Type t = Type.GetType(epb.ElementName);
				GraphicElement el = (GraphicElement)Activator.CreateInstance(t, new object[] { canvas });
				el.Deserialize(epb);
				elements.Add(el);
				epb.Element = el;
			}

			// Fixup Connection
			foreach (ElementPropertyBag epb in sps)
			{
				epb.Connections.Where(c =>c.ToElementId != Guid.Empty).ForEach(c =>
				{
					Connection conn = new Connection();
					conn.Deserialize(elements, c);
					epb.Element.Connections.Add(conn);
				});
			}

			foreach (ElementPropertyBag epb in sps)
			{
				epb.Element.FinalFixup(elements, epb);
			}

			return elements;
		}
	}
}