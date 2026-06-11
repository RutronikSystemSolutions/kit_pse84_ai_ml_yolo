using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;

namespace ov7675.Views
{
    public partial class RawRadarSignalsView : UserControl
    {
        DateTimeAxis dateTimeAxis = new DateTimeAxis
        {
            MajorGridlineStyle = LineStyle.Dot,
            Position = AxisPosition.Bottom,
            AxislineStyle = LineStyle.Solid,
            AxislineColor = OxyColors.Gray,
            FontSize = 10,
            PositionAtZeroCrossing = true,
            IsPanEnabled = false,
            IsZoomEnabled = true,
            StringFormat = "HH:mm:ss",
            Title = "Time",
        };

        /// <summary>
        /// Y Axis for amplitude
        /// </summary>
        private LinearAxis yAxis = new LinearAxis
        {
            MajorGridlineStyle = LineStyle.Dot,
            AxislineStyle = LineStyle.Solid,
            AxislineColor = OxyColors.Gray,
            FontSize = 10,
            TextColor = OxyColors.Gray,
            Position = AxisPosition.Left,
            IsPanEnabled = false,
            IsZoomEnabled = true,
            Unit = "Volume",
            Key = "Amp",
        };

        private LineSeries timeSignalAntenna0LineSeries = new LineSeries();

        private AreaSeries areaSeries = new AreaSeries
        {
            Color = OxyColor.FromAColor(100, OxyColors.SkyBlue),
            Fill = OxyColor.FromAColor(50, OxyColors.SkyBlue),
            StrokeThickness = 2,
            LineStyle = OxyPlot.LineStyle.Solid
        };

        public RawRadarSignalsView()
        {
            InitializeComponent();
            InitPlot();
        }

        private void InitPlot()
        {
            // Raw signals plot
            var timeModel = new PlotModel
            {
                PlotType = PlotType.XY,
                PlotAreaBorderThickness = new OxyThickness(0),
            };

            // Set the axes
            // timeModel.Axes.Add(xAxis);
            timeModel.Axes.Add(dateTimeAxis);
            timeModel.Axes.Add(yAxis);

            // Add series
            timeSignalAntenna0LineSeries.Title = "Antenna 0";
            timeSignalAntenna0LineSeries.YAxisKey = yAxis.Key;

            //timeModel.Series.Add(timeSignalAntenna0LineSeries);
            timeModel.Series.Add(areaSeries);

            plotView.Model = timeModel;
            plotView.InvalidatePlot(true);
        }

        public void updateData(double predictedVolume)
        {
            //timeSignalAntenna0LineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), predictedVolume));
            areaSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), predictedVolume));
            /*if (timeSignalAntenna0LineSeries.Points.Count > 1000)
            {
                timeSignalAntenna0LineSeries.Points.RemoveAt(0);
            }*/
            if (areaSeries.Points.Count > 1000)
            {
                areaSeries.Points.RemoveAt(0);
            }
            plotView.InvalidatePlot(true);
        }
    }
}
