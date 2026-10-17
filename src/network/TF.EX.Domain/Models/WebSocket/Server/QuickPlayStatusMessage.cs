using System.Runtime.Serialization;

namespace TF.EX.Domain.Models.WebSocket.Server
{
    [DataContract]
    public class QuickPlayStatusMessage
    {
        [DataMember(Name = "QuickPlayStatus")]
        public QuickPlayStatus QuickPlayStatus { get; set; } = new QuickPlayStatus();
    }

    [DataContract]
    public class QuickPlayStatus
    {
        [DataMember(Name = "queued")]
        public bool Queued { get; set; }

        [DataMember(Name = "searching_standard")]
        public int SearchingStandard { get; set; }

        [DataMember(Name = "searching_wide")]
        public int SearchingWide { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>
        /// The players searching on this device, sent back by a server that seats several players per device
        /// </summary>
        [DataMember(Name = "local_players")]
        public int? LocalPlayers { get; set; }
    }
}
