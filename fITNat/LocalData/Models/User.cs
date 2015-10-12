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
using SQLite.Net;
using SQLite.Net.Attributes;

namespace fITNat.LocalData.Models
{
    [Table("User")]
    public class User:LocalEntityBase
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public override string ToString()
        {
            return string.Format("[User: LocalId={0}, UserId={1}, Username={2}, Password={3}]", LocalId, UserId, Username, Password);
        }
    }
}