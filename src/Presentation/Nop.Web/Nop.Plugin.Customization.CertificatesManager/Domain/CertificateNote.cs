using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Customization.CertificatesManager.Domain
{
    public class CertificateNote:BaseEntity
    {
        public int CertificateId { get; set; }

        public CertificateNoteType NoteType { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAtUTC { get; set; }
    }

    public enum CertificateNoteType
    {
        None = 0,
        StatusChange = 10, //Writes each time when status changes
        Comment = 20, //for example if operator left a comment.
        NotificationSent = 30, //writes when notification sends.
    }
}
