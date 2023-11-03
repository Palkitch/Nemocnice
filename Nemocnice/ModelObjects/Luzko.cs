using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Luzko
    {
        public int Id { get; set; }
        public int BedNumber { get; set; }
        public int? NurseId { get; set; }
        public int RoomId { get; set; }

        public Luzko(int id, int bedNumber, int? nurseId, int roomId)
        {
            Id = id;
            BedNumber = bedNumber;
            NurseId = nurseId;
            RoomId = roomId;
        }
    }
}
