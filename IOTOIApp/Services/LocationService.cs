using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace IOTOIApp.Services
{
    public class LocationService
    {
        public async static Task<Geoposition> GetPosition()
        {
            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync();

                Debug.WriteLine("GetPosition accessStatus :: " + accessStatus);
                //if (accessStatus != GeolocationAccessStatus.Allowed)
                //{
                //    //throw new Exception();
                //}
                var geolocator = new Geolocator { DesiredAccuracyInMeters = 0 };
                var position = await geolocator.GetGeopositionAsync();
                return position;

            }catch(Exception e)
            {
                return null;
            }
        }
    }
}
