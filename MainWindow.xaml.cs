
using System;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ESRI.ArcGIS.Client;
using System.Windows.Media;


namespace GeoCodeDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

       
        private void buttonSearchCoord_Click(object sender, RoutedEventArgs e)
        {
            if (this.map1.Layers.Contains(this.map1.Layers["SearchLyr"]))
            {
                this.map1.Layers.Remove(this.map1.Layers["SearchLyr"]);
            }
            ESRI.ArcGIS.Client.Geometry.Geometry mp= GetLocationCoord(this.textBoxAdd.Text);
            if (mp==null)
            {
                MessageBox.Show("Can not get the Coordinate");
                return; 
            }
            ESRI.ArcGIS.Client.Geometry.Polyline pl = CreateGeoForZoom(mp);
            ESRI.ArcGIS.Client.GraphicsLayer gl = CreateSearchLyr(mp);

            //this.map1.ZoomToResolution(this.map1.Resolution/510,mp as ESRI.ArcGIS.Client.Geometry.MapPoint);
            ESRI.ArcGIS.Client.Projection.WebMercator wm = new ESRI.ArcGIS.Client.Projection.WebMercator();
            ESRI.ArcGIS.Client.Geometry.MapPoint mpLonLat= (ESRI.ArcGIS.Client.Geometry.MapPoint)wm.ToGeographic(mp);
            this.textBoxResult.Text = this.textBoxAdd.Text + ":" + mpLonLat.X +","+ mpLonLat.Y;
            this.map1.ZoomTo(pl as ESRI.ArcGIS.Client.Geometry.Geometry);
            this.map1.Layers.Add(gl as ESRI.ArcGIS.Client.Layer);

        }

        public static  ESRI.ArcGIS.Client.GraphicsLayer CreateSearchLyr(ESRI.ArcGIS.Client.Geometry.Geometry mp)
        {
            ESRI.ArcGIS.Client.GraphicsLayer gl = new GraphicsLayer();
            ESRI.ArcGIS.Client.GraphicCollection listGc = new GraphicCollection();
            ESRI.ArcGIS.Client.Graphic gp = new Graphic()
            {
                Geometry = mp
            };
            listGc.Add(gp);
            gl.ID = "SearchLyr";
            gl.Graphics = listGc;
            ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol skb = new ESRI.ArcGIS.Client.Symbols.SimpleMarkerSymbol();
            skb.Color = Brushes.Red;
            skb.Size = 15;
            var render = new SimpleRenderer();
            render.Symbol = skb;
            gl.Renderer = render as IRenderer;
            return gl;
        }

        public static ESRI.ArcGIS.Client.Geometry.Polyline CreateGeoForZoom(ESRI.ArcGIS.Client.Geometry.Geometry mp)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint mp1 = new ESRI.ArcGIS.Client.Geometry.MapPoint()
            {
                X = (mp as ESRI.ArcGIS.Client.Geometry.MapPoint).X + 10000,
                Y = (mp as ESRI.ArcGIS.Client.Geometry.MapPoint).Y + 10000
            };
            ESRI.ArcGIS.Client.Geometry.MapPoint mp2 = new ESRI.ArcGIS.Client.Geometry.MapPoint()
            {
                X = (mp as ESRI.ArcGIS.Client.Geometry.MapPoint).X - 10000,
                Y = (mp as ESRI.ArcGIS.Client.Geometry.MapPoint).Y - 10000
            };
            System.Collections.ObjectModel.ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> path = new System.Collections.ObjectModel.ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection>();
            ESRI.ArcGIS.Client.Geometry.PointCollection plist = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            plist.Add(mp2 as ESRI.ArcGIS.Client.Geometry.MapPoint);
            plist.Add(mp1);
            path.Add(plist);
            ESRI.ArcGIS.Client.Geometry.Polyline pl = new ESRI.ArcGIS.Client.Geometry.Polyline
            {
                Paths = path
            };
            return pl;
        }

        public static ESRI.ArcGIS.Client.Geometry.Geometry GetLocationCoord(string address)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint _mp = null;
            string url=string.Format("http://maps.google.com/maps/api/geocode/json?address={0}ka&sensor=false",address);
            HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
            hwr.Method = "GET";
            HttpWebResponse httpResponse;
            StringBuilder stringBuffer = new StringBuilder();
            try
            {
                httpResponse = hwr.GetResponse() as HttpWebResponse;
                if (httpResponse.GetResponseStream().CanRead)
                {
                    StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8);
                    string s = sr.ReadToEnd();
                    JObject jObject = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(s);
                    string lon= ((((jObject["results"] as JArray)[0] as JObject)["geometry"] as JObject)["location"] as JObject)["lng"].ToString();
                    string lat = ((((jObject["results"] as JArray)[0] as JObject)["geometry"] as JObject)["location"] as JObject)["lat"].ToString();
    
                    ESRI.ArcGIS.Client.Geometry.MapPoint mp = new ESRI.ArcGIS.Client.Geometry.MapPoint() 
                    {   
                        X=double.Parse(lon),
                        Y=double.Parse(lat),
                        SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326)
                    };
                    ESRI.ArcGIS.Client.Projection.WebMercator w = new ESRI.ArcGIS.Client.Projection.WebMercator();
                    _mp = w.FromGeographic(mp) as ESRI.ArcGIS.Client.Geometry.MapPoint;
                }                                                                    
            }
            catch (Exception e)
            {
            }
            return _mp;
            
        }

        public static string GetLocationAddress(string coord)
        {
            string result = "";
            string[] coords= coord.Trim().Replace('，', ',').Split(',');
            double lon = double.Parse(coords[0]);
            double lat = double.Parse(coords[1]);
            string url = string.Format("http://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&sensor=false&language=zh-CN", lat, lon);

            HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
            hwr.Method = "GET";
            HttpWebResponse httpResponse;
            StringBuilder stringBuffer = new StringBuilder();
            try
            {
                httpResponse = hwr.GetResponse() as HttpWebResponse;
                if (httpResponse.GetResponseStream().CanRead)
                {
                    StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8);
                    string s = sr.ReadToEnd();
                    JObject jObject = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(s);
                    string addressInfo = ((jObject["results"] as JArray)[0] as JObject)["formatted_address"].ToString();
                    result = addressInfo;
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }

        private void textBoxAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            this.textBoxAdd.Text = "";
        }

        private void textBoxAdd_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.textBoxAdd.Text=="")
            {
                this.textBoxAdd.Text = "输入地址（如:广州市广州大道中）";
            }
        }

        private void buttonSearchAddress_Click(object sender, RoutedEventArgs e)
        {
            string[] coords = this.textBoxCoord.Text.Trim().Replace('，', ',').Split(',');
            if (coords==null||coords.Length<2)
            {
                MessageBox.Show("Wrong Coords Format,Check it out");
                return;
            }
            double lon = double.Parse(coords[0]);
            double lat = double.Parse(coords[1]);
            if (lat>90.0||lat<-90.0||lon>180.0||lon<-180.0)
            {
                MessageBox.Show("Wrong Coords Format,Check it out");
                return; 
            }
            string result= GetLocationAddress(this.textBoxCoord.Text);
            if (this.map1.Layers.Contains(this.map1.Layers["SearchLyr"]))
            {
                this.map1.Layers.Remove(this.map1.Layers["SearchLyr"]);
            }
            ESRI.ArcGIS.Client.Geometry.MapPoint mp = new ESRI.ArcGIS.Client.Geometry.MapPoint()
            {
                X=lon,
                Y=lat,
                SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(4326)
            };
            ESRI.ArcGIS.Client.Projection.WebMercator wm = new ESRI.ArcGIS.Client.Projection.WebMercator();
            mp= wm.FromGeographic(mp) as ESRI.ArcGIS.Client.Geometry.MapPoint;
            ESRI.ArcGIS.Client.Geometry.Polyline pl = CreateGeoForZoom(mp);
            ESRI.ArcGIS.Client.GraphicsLayer gl = CreateSearchLyr(mp);
            this.textBoxResult.Text = this.textBoxCoord.Text + ":" +result;
            this.map1.ZoomTo(pl as ESRI.ArcGIS.Client.Geometry.Geometry);
            this.map1.Layers.Add(gl as ESRI.ArcGIS.Client.Layer);
        }

        private void textBoxCoord_GotFocus(object sender, RoutedEventArgs e)
        {
            this.textBoxCoord.Text = "";
        }

        private void textBoxCoord_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.textBoxCoord.Text == "")
            {
                this.textBoxCoord.Text = "输入坐标（如:113.10，23.00）";
            }
        }
    }
}
