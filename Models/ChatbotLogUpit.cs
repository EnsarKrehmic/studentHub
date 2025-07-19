using System;

namespace StudentHub.Models
{
    public class ChatbotLogUpit
    {
        public int Id { get; set; }
        public string Pitanje { get; set; }
        public DateTime Datum { get; set; }
        public long? UserId { get; set; }
        public string Status { get; set; }
        public string? OdgovorSlužbe { get; set; }
        public DateTime? DatumOdgovora { get; set; }

        public int? PodrskaUpitId { get; set; }
        public PodrskaUpit PodrskaUpit { get; set; }
    }
}
