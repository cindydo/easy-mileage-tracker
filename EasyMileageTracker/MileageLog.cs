using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMileageTracker
{
    public class MileageLogContext : DbContext
    {
        public DbSet<MileageLog> MileageLogs { get; set; }
    }

    public class MileageLog
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string StartingPoint { get; set; }
        public string Destination { get; set; }
        public string Purpose { get; set; }
        public string PurposeDetails { get; set; }
        public double MileageStart { get; set; }
        public double MileageEnd { get; set; }
        public double TotalDistance { get; set; }
    }
}
