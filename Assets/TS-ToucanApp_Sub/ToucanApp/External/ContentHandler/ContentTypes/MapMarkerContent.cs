using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace ToucanApp.Data
{
    public class MapMarkerContent :  AbstractContent<MapData>
    {
        public class CoordinatesData
        {
            public float lat;
            public float lon;
        }

        [SerializeField]
        private CoordinatesData coordinates;
        public CoordinatesData Coordinates
        {
            get { return coordinates; }
        }

#if USING_ONLINEMAPS
        private OnlineMaps map;
        public UnityEvent onMarkerClick;
		[HideInInspector]
        public OnlineMapsMarker marker;
#endif

		private void Start()
		{
#if USING_ONLINEMAPS
			map = OnlineMaps.instance;
#endif
		}

        public virtual void ShowMarker()
        {
#if USING_ONLINEMAPS
            map.AddMarker(marker);
            map.Redraw();

			marker.OnClick = (OnlineMapsMarkerBase m) => {

				onMarkerClick.Invoke();

			};
#endif
        }

        public void HideMarker()
        {
#if USING_ONLINEMAPS
            map.RemoveMarker(marker, false);
            map.Redraw();
			marker.OnClick = null;
#endif
        }

        public override void OnImportData()
        {
            base.OnImportData();

            coordinates = new CoordinatesData();
            coordinates.lat = (float)Data.lat;
            coordinates.lon = (float)Data.log;

#if USING_ONLINEMAPS
            marker = new OnlineMapsMarker();
            marker.position = new Vector2(coordinates.lon, coordinates.lat);

//			ContentHandler.Helpers.GetResource(Data, "markerTexture", (ResourceInfo info) => {
//
//				marker.texture = info.GetTexture();
//
//			});
#endif
        }

        public override void OnExportData ()
		{
			base.OnExportData ();
		}
    }
}
