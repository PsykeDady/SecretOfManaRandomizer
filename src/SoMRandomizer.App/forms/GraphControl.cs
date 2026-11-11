#nullable disable
using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;
using System.Globalization;

namespace SoMRandomizer.forms
{
    /// <summary>
    /// Avalonia control that renders the difficulty graph for the older procgen modes.
    /// Replaces the WinForms PictureBox + System.Drawing rendering used by the original
    /// DifficultyVisualizer.
    /// </summary>
    public class GraphControl : Avalonia.Controls.Control
    {
        private Dictionary<Color, List<int>> allValues = new Dictionary<Color, List<int>>();

        public GraphControl()
        {
            ClipToBounds = true;
        }

        public void SetData(Dictionary<Color, List<int>> values)
        {
            allValues = values ?? new Dictionary<Color, List<int>>();
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            int width = (int)Bounds.Width;
            int height = (int)Bounds.Height;
            if (width <= 0 || height <= 0 || allValues.Count == 0)
            {
                return;
            }

            int max = 1;
            int numPoints = 0;
            foreach (List<int> values in allValues.Values)
            {
                foreach (int val in values)
                {
                    if (val > max)
                    {
                        max = val;
                    }
                }
                numPoints = values.Count;
            }

            if (numPoints < 2)
            {
                return;
            }

            // max .. 5, 10, 50, 100...
            int graphMax = 5;
            while (graphMax < max)
            {
                if (graphMax.ToString().Contains("5"))
                {
                    graphMax *= 2;
                }
                else
                {
                    graphMax *= 5;
                }
            }

            int bufferArea = 20;
            int yDrawArea = height - bufferArea * 2;
            int xDrawArea = width - bufferArea * 2;

            Typeface typeface = new Typeface(FontFamily.Default);
            IBrush textBrush = Brushes.Black;

            // horizontal gridlines (y axis)
            for (int axisY = 0; axisY <= graphMax; axisY += graphMax / 5)
            {
                int drawY = (int)(height - bufferArea - ((axisY / (double)graphMax) * yDrawArea));
                Pen pen = new Pen(Brushes.Black, 2, new DashStyle(new double[] { 5, 5 }, 0));
                context.DrawLine(pen, new Point(bufferArea, drawY), new Point(width - bufferArea, drawY));

                FormattedText text = new FormattedText(
                    "" + axisY,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    8,
                    textBrush);
                context.DrawText(text, new Point(0, drawY));
            }

            // vertical gridlines (x axis / floors)
            for (int floorNum = 0; floorNum < numPoints; floorNum++)
            {
                int drawX = (int)(bufferArea + ((floorNum / (double)(numPoints - 1)) * xDrawArea));
                Pen pen = new Pen(Brushes.Black, 1, new DashStyle(new double[] { 3, 3 }, 0));
                context.DrawLine(pen, new Point(drawX, bufferArea), new Point(drawX, height - bufferArea));

                FormattedText text = new FormattedText(
                    "" + (floorNum + 1),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    8,
                    textBrush);
                context.DrawText(text, new Point(drawX - 4, height - bufferArea));
            }

            // data curves
            foreach (KeyValuePair<Color, List<int>> entry in allValues)
            {
                Color col = entry.Key;
                List<int> values = entry.Value;
                Pen pen = new Pen(new SolidColorBrush(col), 1);
                for (int floorNum = 0; floorNum < numPoints - 1; floorNum++)
                {
                    int drawX1 = (int)(bufferArea + ((floorNum / (double)(values.Count - 1)) * xDrawArea));
                    int drawX2 = (int)(bufferArea + (((floorNum + 1) / (double)(values.Count - 1)) * xDrawArea));
                    int drawY1 = (int)(height - bufferArea - ((values[floorNum] / (double)graphMax) * yDrawArea));
                    int drawY2 = (int)(height - bufferArea - ((values[floorNum + 1] / (double)graphMax) * yDrawArea));
                    context.DrawLine(pen, new Point(drawX1, drawY1), new Point(drawX2, drawY2));
                }
            }
        }
    }
}
