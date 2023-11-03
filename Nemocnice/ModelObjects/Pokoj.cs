﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nemocnice.ModelObjects
{
    public class Pokoj
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; }
        public int HospitalWardId { get; set; }
        public Pokoj(int id, int roomNumber, int hospitalWardId)
        {
            Id = id;
            RoomNumber = roomNumber;
            HospitalWardId = hospitalWardId;
        }
    }
}