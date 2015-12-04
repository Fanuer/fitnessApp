
using Android.App;
using Android.OS;

namespace fITNat.Activities
{
    [Activity(Label = "MapActivity")]
    public class MapActivity : Activity
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Create your application here
            SetContentView(Resource.Layout.MapLayout);
        }
    }
}