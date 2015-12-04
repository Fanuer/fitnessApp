using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Gms.Maps;

namespace fITNat.Activities
{
    [Activity(Label = "fIT", Icon = "@drawable/ic_actionBar_logo", MainLauncher = true, Theme = "@style/MyTheme")]
    public class HomeActivity : ActionBarActivity
    {
        private SupportToolbar mToolbar;
        //private MyActionBarDrawerToggle mDrawerToggle;
        private DrawerLayout mDrawerLayout;
        //private ListView mLeftDrawer;
        private ListView mRightDrawer;
        //private ArrayAdapter mLeftAdapter;
        private ArrayAdapter mRightAdapter;
        //private List<string> mLeftDataSet;
        private List<string> mRightDataSet;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Home);

            mToolbar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            //mLeftDrawer = FindViewById<ListView>(Resource.Id.left_drawer);
            mRightDrawer = FindViewById<ListView>(Resource.Id.right_drawer);

            //mLeftDrawer.Tag = 0;
            mRightDrawer.Tag = 1;

            SetSupportActionBar(mToolbar);

            /*mLeftDataSet = new List<string>();
            mLeftDataSet.Add("Left Item 1");
            mLeftDataSet.Add("Left Item 2");
            mLeftAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mLeftDataSet);
            mLeftDrawer.Adapter = mLeftAdapter;*/

            mRightDataSet = new List<string>();
            mRightDataSet.Add("Studio-Finder");
            //mRightDataSet.Add("Right Item 2");
            mRightAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mRightDataSet);
            mRightDrawer.Adapter = mRightAdapter;
            mRightDrawer.ItemClick += lv_ItemClick;

            /*mDrawerToggle = new MyActionBarDrawerToggle(
                this,                           //Host Activity
                mDrawerLayout,                  //DrawerLayout
                Resource.String.openDrawer,     //Opened Message
                Resource.String.closeDrawer     //Closed Message
            );*/

            //mDrawerLayout.SetDrawerListener(mDrawerToggle);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetTitle(Resource.String.home);
            SupportActionBar.SetLogo(Resource.Drawable.ic_actionBar_logo);
            //mDrawerToggle.SyncState();

            /*if (bundle != null)
            {
                if (bundle.GetString("DrawerState") == "Opened")
                {
                    SupportActionBar.SetTitle(Resource.String.openDrawer);
                }

                else
                {
                    SupportActionBar.SetTitle(Resource.String.closeDrawer);
                }
            }

            else
            {
                //This is the first the time the activity is ran
                SupportActionBar.SetTitle(Resource.String.closeDrawer);
            }*/

        }

        /// <summary>
        /// Clickevent auf ein Element des ListViews im rechten Menü
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lv_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string value = mRightDrawer.GetItemAtPosition(e.Position).ToString();
            if(value == "Studio-Finder")
            {
                var mapActivity = new Intent(this, typeof(MapActivity));
                StartActivity(mapActivity);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                /*
                case Android.Resource.Id.Home:
                    //The hamburger icon was clicked which means the drawer toggle will handle the event
                    //all we need to do is ensure the right drawer is closed so the don't overlap
                    mDrawerLayout.CloseDrawer(mRightDrawer);
                    mDrawerToggle.OnOptionsItemSelected(item);
                    return true;
                    */
                case Resource.Id.action_add:
                    //Refresh
                    return true;
                    
                case Resource.Id.action_menu:
                    if (mDrawerLayout.IsDrawerOpen(mRightDrawer))
                    {
                        //Right Drawer is already open, close it
                        mDrawerLayout.CloseDrawer(mRightDrawer);
                    }

                    else
                    {
                        //Right Drawer is closed, open it and just in case close left drawer
                        mDrawerLayout.OpenDrawer(mRightDrawer);
                        //mDrawerLayout.CloseDrawer(mLeftDrawer);
                    }

                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        /// <summary>
        /// Hier wird das action_menu in die Toolbar geladen
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        
        protected override void OnSaveInstanceState(Bundle outState)
        {
            /*if (mDrawerLayout.IsDrawerOpen((int)GravityFlags.Left))
            {
                outState.PutString("DrawerState", "Opened");
            }

            else
            {
                outState.PutString("DrawerState", "Closed");
            }*/

            base.OnSaveInstanceState(outState);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            //mDrawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            //mDrawerToggle.OnConfigurationChanged(newConfig);
        }
    }
}