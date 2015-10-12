
using SQLite.Net.Attributes;

namespace fITNat.LocalData.Models
{
    public class LocalEntityBase
    {
        [PrimaryKey, AutoIncrement]
        public int LocalId { get; set; }
        public bool WasOffline { get; set; }
    }
}