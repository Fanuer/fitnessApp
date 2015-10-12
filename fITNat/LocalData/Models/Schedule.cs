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
using SQLite;

namespace fITNat.LocalData.Models
{
    [Table("Schedule")]
    public class Schedule:LocalEntityBase
    {   
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public override string ToString()
        {
            return string.Format("[Schedule: LocalId={0}, WasOffline={1}, Id={2}, UserId={3}, Name={4}, Url={5}]", LocalId, WasOffline, Id, UserId, Name, Url);
        }
    }
}