using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiChecker.PresentationLayer
{
    public class DataPlotter
    {
        public double[] xBase { get; set; }
        public double[] yBase { get; set; }
        public static DataPlotter Instance(double[] xAxis , double[] yValues) => new DataPlotter(xAxis, yValues);

        public Plot Plt= new Plot(8000,3500);
        public DataPlotter(double[] xAxis, double[] yValues) 
        {
            xBase=xAxis;
            yBase=yValues;
            Plt.AddScatter(xAxis, yValues);
            Plt.XAxis.DateTimeFormat(true);
            Plt.AddHorizontalLine(70);
            Plt.AddHorizontalLine(30);
        }
        public void Plot(string chartName="chart")
        {
            Plt.Legend();
            Plt.SaveFig($"{chartName}.png");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Environment.CurrentDirectory + @$"\{chartName}.png",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        public DataPlotter AddScatter(double[] yValues,string label)
        {
            var scatter1 = Plt.AddScatter(xBase, yValues, label:label);
            scatter1.OnNaN = ScottPlot.Plottable.ScatterPlot.NanBehavior.Ignore;

            return this;
        }
    }
}
