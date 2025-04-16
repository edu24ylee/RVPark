using System;
using System.Collections.Generic;
using ApplicationCore.Models;

namespace RVPark.ViewModels
{
    public class ReservationDetailsVM
    {
        public ReservationHeader ReservationHeader { get; set; } = new ReservationHeader();
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    public class ReservationHeader
    {
        public string ReservationName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Comments { get; set; }
    }
}
