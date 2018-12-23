using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon.Models
{
    class DeleteActivity : UserActivity
    {
        public DeleteActivity()
        {
            ActivityType = ActivityType.Delete;
        }

        public MongoEntity DeletedObj { get; set; }
    }
}
