using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Hoho.ExternalAPIs.Domain
{
    public class ApiRequestLog : BaseEntity
    {
        public int ApiKeyId { get; set; } // ID использованного API ключа
        public bool IsSuccessUsage { get; set; } //true or false
        public int? UserId { get; set; } // Пользователь, который сделал запрос, если применимо
        public string RequestUrl { get; set; } // URL запроса
        public string RequestMethod { get; set; } // Метод запроса (GET, POST и т.д.)
        public string IpAddress { get; set; } // IP-адрес клиента, сделавшего запрос
        public string UserAgent { get; set; } // User-Agent клиента
        public DateTime RequestTimeUtc { get; set; } // Время запроса
    }
}
