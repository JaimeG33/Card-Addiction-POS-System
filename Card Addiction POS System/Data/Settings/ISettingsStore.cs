using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Data.Settings
{
    public interface ISettingsStore
    {
        AppSettings Load();
        void Save(AppSettings settings);
        void Reset();
    }
}
