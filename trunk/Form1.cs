// René DEVICHI 2011

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using EARTHLib;


namespace GETagging
{
    public partial class Form1 : Form
    {
        static string GEFolderName = "My Tagging";

        ApplicationGE earth = null;
        Coordinate Latitude, Longitude;

        public Form1()
        {
            InitializeComponent();
        }


        private void trace(string format, params object[] args)
        {
            listBox1.Items.Add(string.Format(format, args));
        }


        private void button4_Click(object sender, EventArgs e)
        {
            // reset the COM object if connection has been broken since the first call
            // (i.e. GE has been closed)
            if (earth != null)
            {
                try
                {
                    earth.IsInitialized();
                }
                catch (Exception /*e*/)
                {
                    earth = null;
                }
            }

            if (earth == null)
            {
                // establish the COM connection with Google Earth
                try
                {
                    earth = new EARTHLib.ApplicationGE();
                }
                catch (Exception /*e*/)
                {
                    return;
                }
            }

            try
            {

                int n = 0;
                while (earth.IsInitialized() == 0)
                {
                    System.Threading.Thread.Sleep(100);

                    // wait 10s
                    if (++n >= 100)
                    {
                        throw new Exception("Google Earth is not available");
                    }
                }

                //trace("IsInitialized={0} VersionAppType={1} IsOnline={2}", earth.IsInitialized(), earth.VersionAppType, earth.IsOnline());
            }
            catch(Exception /*e*/)
            {
                earth = null;
                return;
            }


            // create a KML to display the crosshairs at the center of GE screen
            string s = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<kml xmlns=""http://earth.google.com/kml/2.0"">
  <Folder>
    <name>{0}</name>
    <ScreenOverlay>
      <name>Target</name>
      <Icon>
        <href>{1}</href>
      </Icon>
      <overlayXY x=""0.500000"" y=""0.500000"" xunits=""fraction"" yunits=""fraction"" />
      <screenXY x=""0.500000"" y=""0.500000"" xunits=""fraction"" yunits=""fraction"" />
      <size x=""0"" y=""0"" xunits=""pixels"" yunits=""pixels"" />
    </ScreenOverlay>
    <!--LookAt>
      <longitude>-1.440113</longitude>
      <latitude>43.653903</latitude>
      <range>250</range>
      <tilt>0.000000</tilt>
      <heading>0.000000</heading>
    </LookAt-->
  </Folder>
</kml>", GEFolderName, Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "xhairs.png"));

            string kml = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".kml";

            //trace("kml {0}", kml);

            // Creates a file for writing UTF-8 encoded text
            using (StreamWriter sw = File.CreateText(kml))
            {
                sw.Write(s);
            }

            earth.OpenKmlFile(kml, 1);

            // picasa does it, I'm not sure it's necessary
            try
            {
                foreach (FeatureGE f in earth.GetTemporaryPlaces().GetChildren())
                {
                    if (f.Name == GEFolderName)
                    {
                        f.Highlight();
                        break;
                    }
                }
            }
            catch (Exception /*e*/)
            {
            }

            //
            try
            {
                Form2 geotag = new Form2(earth.GetMainHwnd());

                DialogResult result = geotag.ShowDialog(this);
                
                if (result == DialogResult.OK)
                {
                    // retrieve the position

                    //CameraInfoGE ci = earth.GetCamera(1);
                    //trace("CameraInfo {0:.######} {1:.######} {2} {3}", ci.FocusPointLatitude, ci.FocusPointLongitude, ci.FocusPointAltitude, ci.FocusPointAltitudeMode);

                    // better: GetCamera doesn't return the Altitude
                    PointOnTerrainGE pot = earth.GetPointOnTerrainFromScreenCoords(0, 0);
                    trace("PointOnTerrain {0:.######} {1:.######} {2:.}", pot.Latitude, pot.Longitude, pot.Altitude);

                    Latitude = new Coordinate(pot.Latitude, CoordinatesPosition.N);
                    Longitude = new Coordinate(pot.Longitude, CoordinatesPosition.E);

                    textBox1.Text = Latitude.ToString();
                    textBox2.Text = Longitude.ToString();

                    textBox3.Text = string.Format("{0:.} m", pot.Altitude);
                }
                else
                {
                    trace("cancelled: {0}", result);
                }
            }
            catch (Exception /*e*/)
            {
            }

            // clear the temporary place 'GEFolderName'
            try
            {
                using (StreamWriter sw = File.CreateText(kml))
                {
                    sw.Write(@"<kml/>");
                }
                earth.OpenKmlFile(kml, 1);
                File.Delete(kml);
            }
            catch (Exception /*e*/)
            {
            }

            // bring us to the front
            this.Activate();            
        }
    }
}
